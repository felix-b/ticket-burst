﻿#pragma warning disable CS1998

using System.Reflection;
using MimeKit;

namespace TicketBurst.CheckoutService.Integrations;

public class MockEmailGatewayPlugin : IEmailGatewayPlugin
{
    private static readonly string __outboxFolderPath = Path.Combine(
        Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!,
        "mock-email-outbox");
    
    public MockEmailGatewayPlugin()
    {
        Directory.CreateDirectory(__outboxFolderPath);
    }

    public async Task SendEmailMessage(MimeMessage message)
    {
        var fileName = $"{DateTime.UtcNow:yyyyMMdd-HHmmss-fffff}.txt";
        var filePath = Path.Combine(__outboxFolderPath, fileName);

        await using var file = File.Create(filePath);
        await message.WriteToAsync(file);
    }
}
