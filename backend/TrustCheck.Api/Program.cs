using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Runtime;using TrustCheck.Api.Dtos;
using TrustCheck.Api.Models;
using TrustCheck.Api.Repositories;
using TrustCheck.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// OpenAPI describes the HTTP API during development.
builder.Services.AddOpenApi();

// DynamoDB client.
// Development points to the Docker-hosted DynamoDB Local instance.
builder.Services.AddSingleton<IAmazonDynamoDB>(_ => !string.IsNullOrWhiteSpace(builder.Configuration["DynamoDb:ServiceUrl"])
    ? new AmazonDynamoDBClient("local", "local", new AmazonDynamoDBConfig { ServiceURL = builder.Configuration["DynamoDb:ServiceUrl"] })
    : new AmazonDynamoDBClient(RegionEndpoint.GetBySystemName(builder.Configuration["DynamoDb:Region"] ?? "us-west-2")));

// The API now persists verifications in DynamoDB instead of RAM.
builder.Services.AddSingleton<
    IVerificationRepository,
    DynamoDbVerificationRepository>();

// Business logic lives in the service layer.
builder.Services.AddScoped<VerificationService>();

// Background verification processing.
builder.Services.AddSingleton<VerificationQueue>();
builder.Services.AddHostedService<VerificationWorker>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


// Group all verification endpoints under the same resource route.
var verifications = app.MapGroup("/api/verifications");


// CREATE
// POST /api/verifications
verifications.MapPost("/", async (
    CreateVerificationRequest request,
    VerificationService service) =>
{
    try
    {
        var verification = await service.CreateAsync(request);

        var response = new CreateVerificationResponse
        {
            Id = verification.Id,
            Status = verification.Status,
            Result = verification.Result,
            Reason = verification.Reason,
            CreatedAt = verification.CreatedAt,
            CompletedAt = verification.CompletedAt
        };

        return Results.Created(
            $"/api/verifications/{verification.Id}",
            response);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new ApiErrorResponse
        {
            Code = "validation_error",
            Message = ex.Message
        });
    }
});


// READ ALL
// GET /api/verifications
// GET /api/verifications?status=Submitted
verifications.MapGet("/", async (
    string? status,
    VerificationService service) =>
{
    IReadOnlyList<Verification> items;

    try
    {
        items = string.IsNullOrWhiteSpace(status)
            ? await service.GetAllAsync()
            : await service.GetByStatusAsync(status);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new ApiErrorResponse
        {
            Code = "validation_error",
            Message = ex.Message
        });
    }

    var response = items.Select(verification =>
        new VerificationSummaryResponse
        {
            Id = verification.Id,
            FirstName = verification.FirstName,
            LastName = verification.LastName,
            Status = verification.Status,
            Result = verification.Result,
            CreatedAt = verification.CreatedAt
        });

    return Results.Ok(response);
});


// READ ONE
// GET /api/verifications/{id}
verifications.MapGet("/{id:guid}", async (
    Guid id,
    VerificationService service) =>
{
    var verification = await service.GetByIdAsync(id);

    if (verification is null)
    {
        return Results.NotFound(new ApiErrorResponse
        {
            Code = "verification_not_found",
            Message = "Verification was not found."
        });
    }

    var response = new VerificationDetailsResponse
    {
        Id = verification.Id,
        FirstName = verification.FirstName,
        LastName = verification.LastName,
        DateOfBirth = verification.DateOfBirth,
        Address = verification.Address,
        Country = verification.Country,
        DocumentType = verification.DocumentType,
        DocumentNumber = verification.DocumentNumber,
        Status = verification.Status,
        Result = verification.Result,
        Reason = verification.Reason,
        CreatedAt = verification.CreatedAt,
        CompletedAt = verification.CompletedAt
    };

    return Results.Ok(response);
});


// DOMAIN ACTION
// POST /api/verifications/{id}/run
verifications.MapPost("/{id:guid}/run", async (
    Guid id,
    VerificationService service,
    VerificationQueue queue) =>
{
    try
    {
        var verification = await service.RunAsync(id);

        if (verification is null)
        {
            return Results.NotFound(new ApiErrorResponse
            {
                Code = "verification_not_found",
                Message = "Verification was not found."
            });
        }

        await queue.EnqueueAsync(id);

        return Results.Accepted(
            $"/api/verifications/{id}",
            new
            {
                verification.Id,
                verification.Status,
                verification.Result,
                verification.Reason,
                verification.CompletedAt
            });
    }
    catch (InvalidOperationException ex)
    {
        var message = ex.Message switch
        {
            "verification_in_progress" =>
                "Verification is already being processed.",

            "verification_already_completed" =>
                "Verification has already completed.",

            _ => "Verification cannot be processed."
        };

        return Results.Conflict(new ApiErrorResponse
        {
            Code = ex.Message,
            Message = message
        });
    }
});


// Starts the ASP.NET Core application.
app.Run();

public partial class Program { }