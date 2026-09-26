using System.Net;
using System.Net.Http.Json;
using TrustCheck.Api.Dtos;

namespace TrustCheck.Api.Tests;

public class VerificationEndpointsTests : IClassFixture<TrustCheckApiFactory>
{
    private readonly HttpClient _client;

    public VerificationEndpointsTests(TrustCheckApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static CreateVerificationRequest ValidRequest(string documentNumber = "AB123457")
    {
        return new CreateVerificationRequest
        {
            FirstName = "Jane",
            LastName = "Doe",
            DateOfBirth = new DateOnly(1990, 5, 20),
            Address = "123 Main Street, Vancouver",
            Country = "Canada",
            DocumentType = "Passport",
            DocumentNumber = documentNumber
        };
    }

    [Fact]
    public async Task Post_ValidRequest_Returns201()
    {
        var request = ValidRequest();

        var response = await _client.PostAsJsonAsync("/api/verifications", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var body = await response.Content.ReadFromJsonAsync<CreateVerificationResponse>();

        Assert.NotNull(body);
        Assert.Equal("Submitted", body!.Status);
    }

    [Fact]
    public async Task Post_EmptyFirstName_Returns400()
    {
        var request = ValidRequest();
        request.FirstName = "   ";

        var response = await _client.PostAsJsonAsync("/api/verifications", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();

        Assert.NotNull(body);
        Assert.Equal("validation_error", body!.Code);
    }

    [Fact]
    public async Task Post_BadDocumentType_Returns400()
    {
        var request = ValidRequest();
        request.DocumentType = "LibraryCard";

        var response = await _client.PostAsJsonAsync("/api/verifications", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_All_ReturnsCreatedItem()
    {
        var request = ValidRequest();
        var created = await _client.PostAsJsonAsync("/api/verifications", request);
        var createdBody = await created.Content.ReadFromJsonAsync<CreateVerificationResponse>();

        var response = await _client.GetAsync("/api/verifications");
        var items = await response.Content.ReadFromJsonAsync<List<VerificationSummaryResponse>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(items);
        Assert.Contains(items!, item => item.Id == createdBody!.Id);
    }

    [Fact]
    public async Task Get_ByStatus_ReturnsOnlyThatStatus()
    {
        var created = await _client.PostAsJsonAsync("/api/verifications", ValidRequest());
        var createdBody = await created.Content.ReadFromJsonAsync<CreateVerificationResponse>();
        await _client.PostAsync($"/api/verifications/{createdBody!.Id}/run", null);

        var response = await _client.GetAsync("/api/verifications?status=Submitted");
        var items = await response.Content.ReadFromJsonAsync<List<VerificationSummaryResponse>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(items);
        Assert.All(items!, item => Assert.Equal("Submitted", item.Status));
        Assert.DoesNotContain(items!, item => item.Id == createdBody.Id);
    }

    [Fact]
    public async Task Get_ByUnknownStatus_Returns400()
    {
        var response = await _client.GetAsync("/api/verifications?status=Pending");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();

        Assert.NotNull(body);
        Assert.Equal("validation_error", body!.Code);
    }

    [Fact]
    public async Task Get_ById_ExistingId_ReturnsMatchingNames()
    {
        var request = ValidRequest();
        var created = await _client.PostAsJsonAsync("/api/verifications", request);
        var createdBody = await created.Content.ReadFromJsonAsync<CreateVerificationResponse>();

        var response = await _client.GetAsync($"/api/verifications/{createdBody!.Id}");
        var body = await response.Content.ReadFromJsonAsync<VerificationDetailsResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal(request.FirstName, body!.FirstName);
        Assert.Equal(request.LastName, body.LastName);
    }

    [Fact]
    public async Task Get_ById_UnknownId_Returns404()
    {
        var response = await _client.GetAsync($"/api/verifications/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();

        Assert.NotNull(body);
        Assert.Equal("verification_not_found", body!.Code);
    }

    [Fact]
    public async Task Run_ValidId_Returns202AndVerifying()
    {
        var request = ValidRequest();
        var created = await _client.PostAsJsonAsync("/api/verifications", request);
        var createdBody = await created.Content.ReadFromJsonAsync<CreateVerificationResponse>();

        var response = await _client.PostAsync($"/api/verifications/{createdBody!.Id}/run", null);
        var body = await response.Content.ReadFromJsonAsync<VerificationDetailsResponse>();

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal("Verifying", body!.Status);
    }

    [Fact]
    public async Task Run_UnknownId_Returns404()
    {
        var response = await _client.PostAsync($"/api/verifications/{Guid.NewGuid()}/run", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();

        Assert.NotNull(body);
        Assert.Equal("verification_not_found", body!.Code);
    }

    [Fact]
    public async Task Run_AlreadyRunning_Returns409()
    {
        var request = ValidRequest();
        var created = await _client.PostAsJsonAsync("/api/verifications", request);
        var createdBody = await created.Content.ReadFromJsonAsync<CreateVerificationResponse>();

        await _client.PostAsync($"/api/verifications/{createdBody!.Id}/run", null);
        var response = await _client.PostAsync($"/api/verifications/{createdBody.Id}/run", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();

        Assert.NotNull(body);
        Assert.Equal("verification_in_progress", body!.Code);
    }

    private async Task<VerificationDetailsResponse> PollUntilCompletedAsync(Guid id)
    {
        var deadline = DateTime.UtcNow.AddSeconds(5);

        while (DateTime.UtcNow < deadline)
        {
            var response = await _client.GetAsync($"/api/verifications/{id}");
            var body = await response.Content.ReadFromJsonAsync<VerificationDetailsResponse>();

            if (body!.Status == "Completed")
            {
                return body;
            }

            await Task.Delay(250);
        }

        throw new TimeoutException("Verification did not complete within 5 seconds.");
    }

    [Fact]
    public async Task EndToEnd_DocumentNumberNotEndingInZero_IsVerified()
    {
        var request = ValidRequest();
        var created = await _client.PostAsJsonAsync("/api/verifications", request);
        var createdBody = await created.Content.ReadFromJsonAsync<CreateVerificationResponse>();

        await _client.PostAsync($"/api/verifications/{createdBody!.Id}/run", null);
        var completed = await PollUntilCompletedAsync(createdBody.Id);

        Assert.Equal("Verified", completed.Result);
    }

    [Fact]
    public async Task EndToEnd_DocumentNumberEndingInZero_IsRejected()
    {
        var request = ValidRequest("AB123450");
        var created = await _client.PostAsJsonAsync("/api/verifications", request);
        var createdBody = await created.Content.ReadFromJsonAsync<CreateVerificationResponse>();

        await _client.PostAsync($"/api/verifications/{createdBody!.Id}/run", null);
        var completed = await PollUntilCompletedAsync(createdBody.Id);

        Assert.Equal("Rejected", completed.Result);
    }
}
