using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.RDS;
using Constructs;

namespace Cdk;

public class TicketburstDatabaseStack : Stack
{
    
    
    internal TicketburstDatabaseStack(Construct scope, string id, IStackProps props = null)
    {
        var databaseVpc = new Vpc(this, "db-vpc", )        
        
        var auroraCluster = new DatabaseCluster(this, "aurora-cluster", new DatabaseClusterProps {
            Engine = DatabaseClusterEngine.AuroraMysql(new AuroraMysqlClusterEngineProps {
                Version = AuroraMysqlEngineVersion.VER_2_08_1
            }),
            Credentials = Credentials.FromGeneratedSecret("clusteradmin"),
            Instances = 2,
            InstanceProps = new Amazon.CDK.AWS.RDS.InstanceProps {
                
                VpcSubnets = new SubnetSelection {
                    SubnetType = SubnetType.PRIVATE_ISOLATED  
                }
            }
        });
    }

    public override string[] AvailabilityZones => new[] { "eu-south-1a", "eu-south-1b" };
}
