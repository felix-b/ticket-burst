﻿using Amazon.CDK.AWS.DocDB;
using Amazon.CDK.AWS.EC2;
using Constructs;

namespace Cdk.DB;

public static class ReservationServiceDB
{
    public static void Add(Construct scope, IVpc vpc, IConnectable reservationService)
    {
        DatabaseCluster cluster = new DatabaseCluster(scope, "Database", new DatabaseClusterProps {
            MasterUser = new Login {
                Username = "reservation_dbuser"
            },
            InstanceType = InstanceType.Of(InstanceClass.MEMORY5, InstanceSize.LARGE),
            Instances = 2,
            VpcSubnets = new SubnetSelection {
                SubnetType = SubnetType.PRIVATE_WITH_NAT
            },
            Vpc = vpc,
        });        
    }
}
