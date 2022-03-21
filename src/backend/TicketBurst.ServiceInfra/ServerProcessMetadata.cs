using System.Collections.Immutable;
using k8s;
using TicketBurst.Contracts;

namespace TicketBurst.ServiceInfra;

public static class ServerProcessMetadata
{
    private static ImmutableDictionary<string, string>? __metadata = null;
    
    public static ImmutableDictionary<string, string> Get()
    {
        if (__metadata == null)
        {
            PopulateMetadata();
        }

        return __metadata!;
    }

    public static ServerInfoContract GetInfo()
    {
        return new ServerInfoContract(
            HostName: Environment.GetEnvironmentVariable("HOSTNAME") ?? Environment.MachineName,
            PodName: Environment.GetEnvironmentVariable("MY_POD_NAME"),
            PodIPAddress: Environment.GetEnvironmentVariable("MY_POD_IP"),
            PodServiceAccount: Environment.GetEnvironmentVariable("MY_POD_SERVICE_ACCOUNT"),
            ShardId: string.Empty
        );
    }

    public static ImmutableList<ServerInfoContract> GetCombinedInfo(ImmutableList<ServerInfoContract>? other = null)
    {
        var baseList = other ?? ImmutableList<ServerInfoContract>.Empty;
        var thisInfo = GetInfo();
        return baseList.Add(thisInfo);
    }

    private static void PopulateMetadata()
    {
        var builder = ImmutableDictionary.CreateBuilder<string, string>();

        var variables = Environment.GetEnvironmentVariables();
        foreach (var key in variables.Keys)
        {
            var value = variables[key];
            builder.Add(key?.ToString() ?? string.Empty, value?.ToString() ?? string.Empty);
        }
        

        // try
        // {
        //     Console.WriteLine("Fetching K8s metadata");
        //
        //     var config = KubernetesClientConfiguration.BuildDefaultConfig();
        //     var client = new Kubernetes(config);
        //     var list = client.ListNamespacedPod("default");
        //     
        //     foreach (var item in list.Items)
        //     {
        //         builder[$"{item.Metadata.Name}_"] = string.Empty;
        //     }
        //
        //     if (list.Items.Count == 0)
        //     {
        //         Console.WriteLine("Empty!");
        //     }
        // }
        // catch (Exception e)
        // {
        //     Console.WriteLine("Failed to fetch K8s metadata: " + e.Message);
        //     builder.Add("n/a", e.Message);
        // }

        __metadata = builder.ToImmutable();
    }
}
