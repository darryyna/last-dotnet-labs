using BookCatalog.Application;
using BookCatalog.Infrastructure;
using BookCatalog.Infrastructure.Database;
using OpenTelemetry.Trace;
using Shared.ErrorHandling;
using Shared.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddMongoDBClient("bookCatalogDb");
//builder.Host.ConfigureSerilog();

builder.Services.AddSwaggerGen();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services
    .ConfigureApplication()
    .ConfigureInfrastructure(builder.Configuration);
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.AddSource("MongoDB.Driver.Core.Extensions.DiagnosticSources");
        tracing.AddGrpcClientInstrumentation();
    });

var app = builder.Build();

app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    await app.SetupDatabase();
}

app.MapDefaultEndpoints();
app.UseExceptionHandler();

app.MapControllers();

await app.RunAsync();