using System.Diagnostics.CodeAnalysis;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.

builder.AddServiceDefaults();

// Add services to the container.

builder.Services.AddProblemDetails();

// Swagger / OpenAPI (net8-compatible)

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())

{

    app.UseSwagger();

    app.UseSwaggerUI();

}

// Aspire default endpoints (health, metrics, etc.)

app.MapDefaultEndpoints();

app.Run();

[ExcludeFromCodeCoverage]
public partial class Program { }