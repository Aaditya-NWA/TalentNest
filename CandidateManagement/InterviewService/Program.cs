using CandidateService.Data; // Add this
using CandidateService.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using InterviewService.Contracts.Repositories;
using InterviewService.Contracts.Services;
using InterviewService.Data;
using InterviewService.DTOs.Requests.Feedbacks;
using InterviewService.DTOs.Requests.Interviews;
using InterviewService.Repositories;
using InterviewService.Services;
using InterviewService.Validators;
using Microsoft.AspNetCore.DataProtection.XmlEncryption;
using Microsoft.EntityFrameworkCore;
using RequirementService.Data;
using System.Diagnostics.CodeAnalysis;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// ============ DATABASES - BOTH USE SAME CANDIDATEDB ============
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Register CandidateDbContext from CandidateService
builder.Services.AddDbContext<CandidateDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register RequirementDbContext from CandidateService
builder.Services.AddDbContext<RequirementDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register InterviewDbContext
builder.Services.AddDbContext<InterviewDbContext>(options =>
    options.UseSqlServer(connectionString));

// ============ REPOSITORIES ============
builder.Services.AddScoped<IInterviewRepository, InterviewRepository>();
builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();

// ============ SERVICES ============
builder.Services.AddScoped<IInterviewValidationService, InterviewValidationService>();
builder.Services.AddScoped<IInterviewService, InterviewService.Services.InterviewService>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();
builder.Services.AddScoped<ICandidateBulkInsertService, CandidateBulkInsertService>();

// ============ VALIDATORS ============
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<CreateInterviewValidator>();
builder.Services.AddScoped<IValidator<CreateInterviewRequest>, CreateInterviewValidator>();
builder.Services.AddScoped<IValidator<UpdateInterviewRequest>, UpdateInterviewValidator>();
builder.Services.AddScoped<IValidator<CreateFeedbackRequest>, CreateFeedbackValidator>();
builder.Services.AddScoped<IValidator<UpdateFeedbackRequest>, UpdateFeedbackValidator>();
builder.Services.AddScoped<IValidator<SetOutcomeRequest>, SetOutcomeRequestValidator>();

// ============ API ============
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Interview Service API", Version = "v1" });
    c.UseInlineDefinitionsForEnums();
});

var app = builder.Build();

// ============ MIGRATIONS ============
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<InterviewDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapDefaultEndpoints();
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();

[ExcludeFromCodeCoverage]
public partial class Program { }