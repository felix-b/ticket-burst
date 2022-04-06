using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Cdk.DB;
using Constructs;

namespace Cdk;

public class TicketburstDatabaseStack : Stack
{
    internal TicketburstDatabaseStack(Construct scope, string id, IStackProps props = null)
        : base(scope, id, props)
    {
        var vpc = Vpc.FromVpcAttributes(scope, "vpc-lookup-1", new VpcAttributes {
            VpcId = "vpc-066ee5b96b9b45336"
        });

        var searchSecurityGroup = SecurityGroup.FromSecurityGroupId(
            scope, 
            "sg-lookup-1", 
            "sg-09a951477ca49a953"); 
        var reservationSecurityGroup = SecurityGroup.FromSecurityGroupId(
            scope, 
            "sg-lookup-1", 
            "sg-0def73b1962437d69");
        var checkoutSecurityGroup = SecurityGroup.FromSecurityGroupId(
            scope, 
            "sg-lookup-1", 
            "sg-0e053eb913529fbc8"); 
        
        SearchServiceDB.Add(this, vpc, searchSecurityGroup);
        ReservationServiceDB.Add(this, vpc, reservationSecurityGroup);
        CheckoutServiceDB.Add(this, vpc, checkoutSecurityGroup);
    }

    public override string[] AvailabilityZones => new[] {
        "eu-south-1a", 
        "eu-south-1b"
    };
}
