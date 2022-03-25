using System.Threading.Channels;

namespace TicketBurst.ServiceInfra;

public class InProcessMessagePublisher<T> : InProcessJob<T>, IMessagePublisher<T>
    where T : class
{
    private static readonly string __jobName = $"InProcessMessagePublisher<{typeof(T).Name}>"; 
    
    public InProcessMessagePublisher(ServiceName receiverServiceName, string[] urlPath)
        : base(__jobName, boundCapacity: 1000, CreateDoWork(receiverServiceName, urlPath))
    {
    }

    public void Publish(T message)
    {
        EnqueueWorkItem(message);
    }

    private static Func<T, Task> CreateDoWork(ServiceName receiverServiceName, string[] urlPath)
    {
        return async message => {
            var reply = await ServiceClient.HttpPostJson<string>(
                receiverServiceName,
                urlPath,
                body: message);
            Console.WriteLine($"{__jobName}: SEND SUCCESS message [{message}], reply [{reply}]");
        };
    }
}
