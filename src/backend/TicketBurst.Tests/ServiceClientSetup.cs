﻿using System;
using System.Collections.Generic;
using TicketBurst.ServiceInfra;

namespace TicketBurst.Tests;

public static class ServiceClientSetup
{
    public static void UseForApiTest()
    {
        var awsApiGatewayUrl = "https://3cnuf521pd.execute-api.eu-south-1.amazonaws.com"; 
        ServiceClient.UseHosts(new Dictionary<ServiceName, string> {
            {
                ServiceName.Search, 
                OperatingSystem.IsWindows() 
                    ? "http://localhost:3001" 
                    : awsApiGatewayUrl
            },
            { 
                ServiceName.Reservation, 
                OperatingSystem.IsWindows() 
                    ? "http://localhost:3002" 
                    : awsApiGatewayUrl 
            },
            {
                ServiceName.Checkout, 
                OperatingSystem.IsWindows() 
                    ? "http://localhost:3003" 
                    : awsApiGatewayUrl
            },
        });
    }
}