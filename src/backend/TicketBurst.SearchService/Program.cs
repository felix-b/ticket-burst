// See https://aka.ms/new-console-template for more information

using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.OpenApi.Models;

Console.WriteLine("TicketBurst Search Service starting.");

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://*:3001");

// Add services to the container.

builder.Services.Configure<JsonOptions>(options => {
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Always;
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.SwaggerDoc("v1", new OpenApiInfo() {
    Title = "ticketburst-services-search",
    Description = "Searches for events and available seats. Responsible for Venues, Events, and Seating Maps.",
    Version = "v1"
}));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();

