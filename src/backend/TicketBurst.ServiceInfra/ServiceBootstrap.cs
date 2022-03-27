using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace TicketBurst.ServiceInfra;

public static class ServiceBootstrap
{
    public static WebApplication CreateHttpEndpoint(
        string serviceName, 
        string serviceDescription,
        int listenPortNumber,
        string[] commandLineArgs, 
        Action<WebApplicationBuilder>? configure = null)
    {
        var builder = WebApplication.CreateBuilder(commandLineArgs);

        builder.WebHost.UseUrls($"http://*:{listenPortNumber}");

        // Add services to the container.
        builder.Services.Configure<JsonOptions>(options => {
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Always;
        });

        builder.Services.AddDataProtection()
            .SetApplicationName("ticketburst")
            .DisableAutomaticKeyGeneration();
        
        configure?.Invoke(builder);
        
        builder.Services.AddControllers();
        
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options => {
            options.SwaggerDoc("v1", new OpenApiInfo() {
                Title = serviceName,
                Description = serviceDescription,
                Version = "v1"
            });
            options.DescribeAllParametersInCamelCase();
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        app.UseSwagger();
        app.UseSwaggerUI();

        app.MapControllers();
        return app;
    }
}
