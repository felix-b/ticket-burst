using System.Net;
using System.Text.Json;
using Amazon.Runtime;
using Amazon.StepFunctions;
using Amazon.StepFunctions.Model;
using TicketBurst.Contracts;

namespace TicketBurst.ServiceInfra.Aws;

public class AwsStepFunctionsSagaEnginePlugin : ISagaEnginePlugin
{
    private readonly EmailServiceSecret _secret;
    private readonly AWSCredentials _credentials;
    private readonly Amazon.RegionEndpoint _regionEndpoint;

    public AwsStepFunctionsSagaEnginePlugin(EmailServiceSecret secret)
    {
        _secret = secret;
        Console.WriteLine($"AwsStepFunctionsSagaEnginePlugin> ctor, role=[{secret.Role}], region=[{secret.Region}], workflowArn=[{secret.CheckoutStateMachineArn}]");

        try
        {
            var instanceProfileCreds = new InstanceProfileAWSCredentials(secret.Role);
            _credentials = instanceProfileCreds;
            var tryGetCredentials = instanceProfileCreds.GetCredentials();
            Console.WriteLine($"AwsStepFunctionsSagaEnginePlugin> ctor, got credentials: token=[{tryGetCredentials.Token}], expiry=[{instanceProfileCreds.PreemptExpiryTime}]");

            _regionEndpoint = Amazon.RegionEndpoint.GetBySystemName(secret.Region);
            Console.WriteLine($"AwsStepFunctionsSagaEnginePlugin> ctor, got region endpoint: sysname=[{_regionEndpoint.SystemName}] partition=[{_regionEndpoint.PartitionName}|{_regionEndpoint.PartitionDnsSuffix}]");
        }
        catch (Exception e)
        {
            Console.WriteLine($"AwsStepFunctionsSagaEnginePlugin.CTOR FAILED! {e.ToString()}");
        }
    }

    public async Task CreateOrderCompletionWorkflow(OrderContract order)
    {
        try
        {
            using var amazonStepFunctionsClient = new AmazonStepFunctionsClient(_credentials, _regionEndpoint);

            var workflowState = new CheckoutWorkflowStateContract {
                Order = new CheckoutWorkflowStateContract.OrderPart(order),
                PaymentResult = new CheckoutWorkflowStateContract.PaymentResultPart {
                    PaymentStatus = OrderStatus.CompletionInProgress.ToString()
                }
            };
            var workflowStateJson = JsonSerializer.Serialize(workflowState); 
            var startExecutionRequest = new StartExecutionRequest {
                Name = $"ORDER{order.OrderNumber}",
                Input = workflowStateJson,
                StateMachineArn = _secret.CheckoutStateMachineArn
            };

            Console.WriteLine($"AwsStepFunctionsSagaEnginePlugin.CreateOrderCompletionWorkflow> orderNumber={order.OrderNumber}");
            var response = await amazonStepFunctionsClient.StartExecutionAsync(startExecutionRequest);

            Console.WriteLine($"AwsStepFunctionsSagaEnginePlugin.CreateOrderCompletionWorkflow[{order.OrderNumber}]> SUCCESS, http[{response.HttpStatusCode}], execArn[{response.ExecutionArn}]");
        }
        catch (Exception e)
        {
            Console.WriteLine($"AwsStepFunctionsSagaEnginePlugin.CreateOrderCompletionWorkflow[{order.OrderNumber}]> FAILED! {e.ToString()}");
        }
    }

    public async Task DispatchPaymentCompletionEvent(string awaitStateToken, string paymentToken, uint orderNumber, OrderStatus orderStatus)
    {
        Console.WriteLine($"AwsStepFunctionsSagaEnginePlugin.DispatchPaymentCompletionEvent> orderNumber[{orderNumber}] awaitStateToken[{awaitStateToken}] orderStatus[{orderStatus}]");

        try
        {
            var resultObject = new CheckoutWorkflowStateContract.PaymentResultPart {
                PaymentStatus = orderStatus.ToString()
            };
            var resultObjectJson = JsonSerializer.Serialize(resultObject);
            var request = new SendTaskSuccessRequest() {
                TaskToken = awaitStateToken,
                Output = resultObjectJson
            };

            using var amazonStepFunctionsClient = new AmazonStepFunctionsClient(_credentials, _regionEndpoint);
            await amazonStepFunctionsClient.SendTaskSuccessAsync(request);
        }
        catch (Exception e)
        {
            Console.WriteLine($"AwsStepFunctionsSagaEnginePlugin.DispatchPaymentCompletionEvent[{orderNumber}]> FAILED! {e.ToString()}");
        }
    }    
}
