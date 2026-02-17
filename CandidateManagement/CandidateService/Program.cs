using CandidateService.Data;
using CandidateService.Services;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
// Add services to the container
builder.Services.AddDbContext<CandidateDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<ICandidateBulkInsertService, CandidateBulkInsertService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapDefaultEndpoints();
app.MapGet("/", () => Results.Redirect("/Swagger"));
app.Run();
[ExcludeFromCodeCoverage]
public partial class Program { }