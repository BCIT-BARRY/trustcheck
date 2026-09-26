using TrustCheck.Api.Dtos;
using TrustCheck.Api.Repositories;
using TrustCheck.Api.Services;

namespace TrustCheck.Api.Tests;

public class VerificationServiceTests
{
    private readonly InMemoryVerificationRepository _repository = new();
    private readonly VerificationService _service;

    public VerificationServiceTests()
    {
        _service = new VerificationService(_repository);
    }

    private static CreateVerificationRequest ValidRequest()
    {
        return new CreateVerificationRequest
        {
            FirstName = "Jane",
            LastName = "Doe",
            DateOfBirth = new DateOnly(1990, 5, 20),
            Address = "123 Main Street, Vancouver",
            Country = "Canada",
            DocumentType = "Passport",
            DocumentNumber = "AB123457"
        };
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_StartsAsSubmitted()
    {
        var verification = await _service.CreateAsync(ValidRequest());

        Assert.Equal("Submitted", verification.Status);
        Assert.Null(verification.Result);
        Assert.Null(verification.CompletedAt);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_IsSavedInRepository()
    {
        var verification = await _service.CreateAsync(ValidRequest());

        var saved = await _repository.GetByIdAsync(verification.Id);

        Assert.NotNull(saved);
    }

    [Fact]
    public async Task CreateAsync_TrimsSpacesFromFields()
    {
        var request = ValidRequest();
        request.FirstName = "  Jane  ";
        request.DocumentNumber = " AB123457 ";

        var verification = await _service.CreateAsync(request);

        Assert.Equal("Jane", verification.FirstName);
        Assert.Equal("AB123457", verification.DocumentNumber);
    }

    [Fact]
    public async Task CreateAsync_EmptyFirstName_Throws()
    {
        var request = ValidRequest();
        request.FirstName = "   ";

        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_FutureDateOfBirth_Throws()
    {
        var request = ValidRequest();
        request.DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(request));
    }

    [Theory]
    [InlineData("Passport")]
    [InlineData("DriverLicence")]
    public async Task CreateAsync_AllowedDocumentType_IsAccepted(string documentType)
    {
        var request = ValidRequest();
        request.DocumentType = documentType;

        var verification = await _service.CreateAsync(request);

        Assert.Equal(documentType, verification.DocumentType);
    }

    [Fact]
    public async Task CreateAsync_UnknownDocumentType_Throws()
    {
        var request = ValidRequest();
        request.DocumentType = "LibraryCard";

        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(request));
    }

    [Fact]
    public async Task GetByStatusAsync_ReturnsOnlyThatStatus()
    {
        var first = await _service.CreateAsync(ValidRequest());
        await _service.CreateAsync(ValidRequest());
        await _service.RunAsync(first.Id);

        var verifying = await _service.GetByStatusAsync("Verifying");

        Assert.Single(verifying);
        Assert.Equal(first.Id, verifying[0].Id);
    }

    [Fact]
    public async Task GetByStatusAsync_UnknownStatus_Throws()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetByStatusAsync("Pending"));
    }

    [Fact]
    public async Task RunAsync_UnknownId_ReturnsNull()
    {
        var result = await _service.RunAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task RunAsync_SubmittedVerification_MovesToVerifying()
    {
        var verification = await _service.CreateAsync(ValidRequest());

        var result = await _service.RunAsync(verification.Id);

        Assert.NotNull(result);
        Assert.Equal("Verifying", result.Status);
    }

    [Fact]
    public async Task RunAsync_AlreadyVerifying_Throws()
    {
        var verification = await _service.CreateAsync(ValidRequest());
        await _service.RunAsync(verification.Id);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RunAsync(verification.Id));

        Assert.Equal("verification_in_progress", error.Message);
    }

    [Fact]
    public async Task RunAsync_AlreadyCompleted_Throws()
    {
        var verification = await _service.CreateAsync(ValidRequest());
        await _service.RunAsync(verification.Id);
        await _service.CompleteAsync(verification.Id);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RunAsync(verification.Id));

        Assert.Equal("verification_already_completed", error.Message);
    }

    [Fact]
    public async Task CompleteAsync_DocumentNumberEndingInZero_IsRejected()
    {
        var request = ValidRequest();
        request.DocumentNumber = "AB123450";
        var verification = await _service.CreateAsync(request);
        await _service.RunAsync(verification.Id);

        await _service.CompleteAsync(verification.Id);

        Assert.Equal("Completed", verification.Status);
        Assert.Equal("Rejected", verification.Result);
        Assert.NotNull(verification.CompletedAt);
    }

    [Fact]
    public async Task CompleteAsync_OtherDocumentNumber_IsVerified()
    {
        var verification = await _service.CreateAsync(ValidRequest());
        await _service.RunAsync(verification.Id);

        await _service.CompleteAsync(verification.Id);

        Assert.Equal("Completed", verification.Status);
        Assert.Equal("Verified", verification.Result);
    }

    [Fact]
    public async Task CompleteAsync_NotRunYet_DoesNothing()
    {
        var verification = await _service.CreateAsync(ValidRequest());

        await _service.CompleteAsync(verification.Id);

        Assert.Equal("Submitted", verification.Status);
        Assert.Null(verification.Result);
    }
}
