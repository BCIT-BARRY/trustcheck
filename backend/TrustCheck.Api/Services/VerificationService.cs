using TrustCheck.Api.Dtos;
using TrustCheck.Api.Models;
using TrustCheck.Api.Repositories;

namespace TrustCheck.Api.Services;

public class VerificationService
{
    private readonly IVerificationRepository _repository;

    public VerificationService(IVerificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Verification> CreateAsync(CreateVerificationRequest request)
    {
        ValidateCreateRequest(request);

        var verification = new Verification
        {
            Id = Guid.CreateVersion7(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            DateOfBirth = request.DateOfBirth,
            Address = request.Address.Trim(),
            Country = request.Country.Trim(),
            DocumentType = request.DocumentType.Trim(),
            DocumentNumber = request.DocumentNumber.Trim(),
            Status = "Submitted",
            Result = null,
            Reason = null,
            CreatedAt = DateTimeOffset.UtcNow,
            CompletedAt = null
        };

        await _repository.AddAsync(verification);

        return verification;
    }

    public Task<Verification?> GetByIdAsync(Guid id)
    {
        return _repository.GetByIdAsync(id);
    }

    public Task<IReadOnlyList<Verification>> GetAllAsync()
    {
        return _repository.GetAllAsync();
    }

    public async Task<Verification?> RunAsync(Guid id)
    {
        var verification = await _repository.GetByIdAsync(id);

        if (verification is null)
        {
            return null;
        }

        if (verification.Status == "Verifying")
        {
            throw new InvalidOperationException("verification_in_progress");
        }

        if (verification.Status == "Completed")
        {
            throw new InvalidOperationException("verification_already_completed");
        }

        verification.Status = "Verifying";

        await _repository.UpdateAsync(verification);

        return verification;
    }

    public async Task CompleteAsync(Guid id)
    {
        var verification = await _repository.GetByIdAsync(id);

        if (verification is null || verification.Status != "Verifying")
        {
            return;
        }

        // Deterministic fake verification rule for learning purposes.
        // Test document numbers ending in 0 are rejected; everything else verifies.
        var verified = !verification.DocumentNumber.EndsWith("0");

        verification.Status = "Completed";
        verification.Result = verified ? "Verified" : "Rejected";
        verification.Reason = verified
            ? "Identity passed the TrustCheck demo verification rules."
            : "Document number failed the TrustCheck demo verification rule.";
        verification.CompletedAt = DateTimeOffset.UtcNow;

        await _repository.UpdateAsync(verification);
    }

    private static void ValidateCreateRequest(CreateVerificationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName) ||
            string.IsNullOrWhiteSpace(request.Address) ||
            string.IsNullOrWhiteSpace(request.Country) ||
            string.IsNullOrWhiteSpace(request.DocumentNumber))
        {
            throw new ArgumentException("Required fields cannot be empty.");
        }

        if (request.DateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new ArgumentException("Date of birth cannot be in the future.");
        }

        if (request.DocumentType is not "Passport" and not "DriverLicence")
        {
            throw new ArgumentException(
                "Document type must be Passport or DriverLicence.");
        }
    }
}