using GatewayAPI.Services;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

/* ----------------------------------------
   CONTROLLERS
   Exclude RequirementService + InterviewService
   assemblies so their controllers don't get
   picked up and cause routing conflicts or
   DI errors in the Gateway
---------------------------------------- */
builder.Services.AddControllers()
    .ConfigureApplicationPartManager(manager =>
    {
        var toRemove = manager.ApplicationParts
            .Where(p => p.Name is "RequirementService" or "InterviewService" or "CandidateService")
            .ToList();
        foreach (var part in toRemove)
            manager.ApplicationParts.Remove(part);
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "GatewayAPI", Version = "v1" });
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
    c.CustomSchemaIds(t => t.FullName);
});

/* ----------------------------------------
   HTTP CLIENTS
---------------------------------------- */
builder.Services.AddHttpClient<CandidateClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Services:CandidateService"] ?? "https://localhost:7199");
});
builder.Services.AddHttpClient<InterviewClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Services:InterviewService"] ?? "https://localhost:7200");
});
builder.Services.AddHttpClient<RequirementClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Services:RequirementService"] ?? "https://localhost:7175");
});
builder.Services.AddHttpClient<ReportClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Services:ReportService"] ?? "https://localhost:7266");
});

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
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthorization();
app.MapControllers();
app.MapDefaultEndpoints();
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public partial class Program { }