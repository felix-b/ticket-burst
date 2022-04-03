using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.EKS;
using Amazon.CDK.AWS.IAM;
using Constructs;

namespace Cdk;

public class TicketBurstBackendStack : Stack
{
    internal TicketBurstBackendStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
    {
        var clusterAdminRole = new Role(this, "ticketburst-cluster-admin-role", new RoleProps {
            //AssumedBy = new ArnPrincipal("arn:aws:iam::989574085263:user/admin-user")
            AssumedBy = new AccountRootPrincipal()
        });
        
        var k8sCluster = new Amazon.CDK.AWS.EKS.Cluster(this, "ticketburst-cluster", new ClusterProps {
            ClusterName = "ticketburst-cluster",
            MastersRole = clusterAdminRole,
            Version = KubernetesVersion.V1_21,
            DefaultCapacity = 2,
            DefaultCapacityInstance = new InstanceType("t3.medium")
        });
        
        k8sCluster.AddNodegroupCapacity("ticketburst-nodegroup-1", new NodegroupOptions {
            CapacityType = CapacityType.ON_DEMAND,
            MinSize = 2,
            DesiredSize = 2,
            MaxSize = 20,
            DiskSize = 20,
            Tags = CommonTags.App()
        });
    }
}

