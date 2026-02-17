using System.Diagnostics.CodeAnalysis;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.CandidateService>("candidateservice");
builder.AddProject<Projects.GatewayAPI>("gateway");
builder.AddProject<Projects.InterviewService>("interviewservice");
builder.AddProject<Projects.ReportService>("reportservice");
builder.AddProject<Projects.RequirementService>("requirementservice");

builder.Build().Run();

[ExcludeFromCodeCoverage]
public partial class Program { }
