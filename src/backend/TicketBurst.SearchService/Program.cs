// See https://aka.ms/new-console-template for more information

using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;

Console.WriteLine("Hello, World!");

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:3001");

// Add services to the container.

builder.Services.Configure<JsonOptions>(options => {
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Always;
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
