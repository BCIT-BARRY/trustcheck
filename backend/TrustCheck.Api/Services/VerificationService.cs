using TrustCheck.Api.Dtos;
using TrustCheck.Api.Models;
using TrustCheck.Api.Repositories;

namespace TrustCheck.Api.Services;

public class VerificationService
{
    public const int MaxNameLength = 100;
    public const int MaxAddressLength = 200;
    public const int MaxCountryLength = 60;
    public const int MaxDocumentNumberLength = 30;

    private static readonly DateOnly EarliestDateOfBirth = new DateOnly(1900, 1, 1);

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
            Status = VerificationStatus.Submitted,
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

    public async Task<VerificationCountsResponse> GetCountsAsync()
    {
        var verifications = await _repository.GetAllAsync();

        var counts = new VerificationCountsResponse
        {
            Total = verifications.Count
        };

        foreach (var verification in verifications)
        {
            if (verification.Status == VerificationStatus.Submitted)
            {
                counts.Submitted++;
            }
            else if (verification.Status == VerificationStatus.Verifying)
            {
                counts.Verifying++;
            }
            else if (verification.Status == VerificationStatus.Completed)
            {
                counts.Completed++;
            }

            if (verification.Result == VerificationResult.Verified)
            {
                counts.Verified++;
            }
            else if (verification.Result == VerificationResult.Rejected)
            {
                counts.Rejected++;
            }
        }

        return counts;
    }

    public Task<IReadOnlyList<Verification>> GetByStatusAsync(string status)
    {
        if (!VerificationStatus.All.Contains(status))
        {
            throw new ArgumentException(
                "Status must be Submitted, Verifying or Completed.");
        }

        return _repository.GetByStatusAsync(status);
    }

    public async Task<Verification?> RunAsync(Guid id)
    {
        var verification = await _repository.GetByIdAsync(id);

        if (verification is null)
        {
            return null;
        }

        if (verification.Status == VerificationStatus.Verifying)
        {
            throw new InvalidOperationException("verification_in_progress");
        }

        if (verification.Status == VerificationStatus.Completed)
        {
            throw new InvalidOperationException("verification_already_completed");
        }

        verification.Status = VerificationStatus.Verifying;

        await _repository.UpdateAsync(verification);

        return verification;
    }

    public async Task CompleteAsync(Guid id)
    {
        var verification = await _repository.GetByIdAsync(id);

        if (verification is null || verification.Status != VerificationStatus.Verifying)
        {
            return;
        }

        // Deterministic fake verification rule for learning purposes.
        // Test document numbers ending in 0 are rejected; everything else verifies.
        var verified = !verification.DocumentNumber.EndsWith("0");

        verification.Status = VerificationStatus.Completed;
        verification.Result = verified ? VerificationResult.Verified : VerificationResult.Rejected;
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

        if (request.DateOfBirth < EarliestDateOfBirth)
        {
            throw new ArgumentException("Date of birth cannot be before 1900.");
        }

        if (request.FirstName.Trim().Length > MaxNameLength ||
            request.LastName.Trim().Length > MaxNameLength)
        {
            throw new ArgumentException(
                $"Names cannot be longer than {MaxNameLength} characters.");
        }

        if (request.Address.Trim().Length > MaxAddressLength)
        {
            throw new ArgumentException(
                $"Address cannot be longer than {MaxAddressLength} characters.");
        }

        if (request.Country.Trim().Length > MaxCountryLength)
        {
            throw new ArgumentException(
                $"Country cannot be longer than {MaxCountryLength} characters.");
        }

        if (request.DocumentNumber.Trim().Length > MaxDocumentNumberLength)
        {
            throw new ArgumentException(
                $"Document number cannot be longer than {MaxDocumentNumberLength} characters.");
        }

        if (request.DocumentType is not "Passport" and not "DriverLicence")
        {
            throw new ArgumentException(
                "Document type must be Passport or DriverLicence.");
        }
    }
}