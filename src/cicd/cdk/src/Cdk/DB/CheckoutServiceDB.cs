using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.RDS;
using Constructs;

namespace Cdk.DB;

public static class CheckoutServiceDB
{
    public static void Add(Construct scope, IVpc vpc, IConnectable checkoutService)
    {
        var auroraCluster = new DatabaseCluster(scope, "aurora-cluster", new DatabaseClusterProps {
            Engine = DatabaseClusterEngine.AuroraMysql(new AuroraMysqlClusterEngineProps {
                Version = AuroraMysqlEngineVersion.VER_2_08_1
            }),
            Credentials = Credentials.FromGeneratedSecret(username: "checkout_dbuser"),
            Instances = 2,
            InstanceProps = new Amazon.CDK.AWS.RDS.InstanceProps {
                Vpc = Vpc.FromVpcAttributes(scope, "vpc-lookup-1", new VpcAttributes {
                    VpcId = "vpc-066ee5b96b9b45336"
                }),
                VpcSubnets = new SubnetSelection {
                    SubnetType = SubnetType.PRIVATE_WITH_NAT  
                },
                InstanceType = InstanceType.Of(InstanceClass.MEMORY5, InstanceSize.LARGE)
            }
        });

        auroraCluster.Connections.AllowFrom(
            checkoutService,
            Port.AllTraffic(),
            "Allow connections from Checkout service");
    }
}