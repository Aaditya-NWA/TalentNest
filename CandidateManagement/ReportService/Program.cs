using ReportService.Services;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

/* ?? Declare config value FIRST (top-level scope) */
var interviewServiceUrl =
    builder.Configuration["Services:InterviewService"]
    ?? "https://localhost:7200";

/* ?? Register Interview client ONCE */
builder.Services.AddHttpClient<IInterviewClient, InterviewClient>(client =>
{
    client.BaseAddress = new Uri(interviewServiceUrl);
});

/* ?? Other clients */
builder.Services.AddHttpClient<CandidateClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7199");
});

builder.Services.AddHttpClient<RequirementClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7175");
});

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
