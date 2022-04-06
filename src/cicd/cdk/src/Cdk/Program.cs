using Amazon.CDK;
using System.Collections.Generic;
using Cdk;

namespace Cdk
{
    sealed class Program
    {

        public static void Main(string[] args)
        {
            var app = new App();

            var stack = new TicketburstDatabaseStack(app, "ticketburst-database-stack", new StackProps {
                StackName = "ticketburst-database-stack",
                Tags = CommonTags.App(),
                Env = GetTargetEnvironment()
            });

            // var stack = new TicketburstKubernetesClusterStack(app, "ticketburst-backend-stack", new StackProps {
            //     StackName = "ticketburst-backend-stack",
            //     Tags = CommonTags.App(),
            //     Env = GetTargetEnvironment()
            // });

            app.Synth();
        }

        static Environment GetTargetEnvironment()
        {
            var result = new Environment {
                Account = "381777116710",//System.Environment.GetEnvironmentVariable("CDK_DEPLOYTO_ACCOUNT"),
                Region = "eu-south-1"//System.Environment.GetEnvironmentVariable("CDK_DEPLOYTO_REGION"),
            };

            System.Console.WriteLine($"Using environment ACCOUNT=[{result.Account}] REGION=[{result.Region}]");
            return result;
        }
    }
}
