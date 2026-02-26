using ReportService.Services;
using System.Diagnostics.CodeAnalysis;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

/* ----------------------------------------
   SERVICE URL CONFIGURATION
---------------------------------------- */

var interviewServiceUrl =
    builder.Configuration["Services:InterviewService"]
    ?? "https://localhost:7200";

var candidateServiceUrl =
    builder.Configuration["Services:CandidateService"]
    ?? "https://localhost:7199";

var requirementServiceUrl =
    builder.Configuration["Services:RequirementService"]
    ?? "https://localhost:7175";

/* ----------------------------------------
   HTTP CLIENT REGISTRATIONS
---------------------------------------- */

builder.Services.AddHttpClient<IInterviewClient, InterviewClient>(client =>
{
    client.BaseAddress = new Uri(interviewServiceUrl);
});

builder.Services.AddHttpClient<CandidateClient>(client =>
{
    client.BaseAddress = new Uri(candidateServiceUrl);
});

builder.Services.AddHttpClient<RequirementClient>(client =>
{
    client.BaseAddress = new Uri(requirementServiceUrl);
});
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
    options.InstanceName = "ReportService:";
});

/* ----------------------------------------
   REPORT MANAGER REGISTRATION  ✅ FIX
---------------------------------------- */

builder.Services.AddScoped<IReportManager, ReportManager>();

/* ----------------------------------------
   BUILD APP
---------------------------------------- */

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();
app.MapDefaultEndpoints();

app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();

[ExcludeFromCodeCoverage]
public partial class Program { }