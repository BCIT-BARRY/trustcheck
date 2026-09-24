using TrustCheck.Api.Models;

namespace TrustCheck.Api.Repositories;

public class InMemoryVerificationRepository : IVerificationRepository
{
    private readonly List<Verification> _verifications = new();

    public Task AddAsync(Verification verification)
    {
        _verifications.Add(verification);
        return Task.CompletedTask;
    }

    public Task<Verification?> GetByIdAsync(Guid id)
    {
        var verification = _verifications.FirstOrDefault(v => v.Id == id);
        return Task.FromResult(verification);
    }

    public Task<IReadOnlyList<Verification>> GetAllAsync()
    {
        IReadOnlyList<Verification> verifications = _verifications.ToList();
        return Task.FromResult(verifications);
    }

    public Task<IReadOnlyList<Verification>> GetByStatusAsync(string status)
    {
        IReadOnlyList<Verification> verifications = _verifications
            .Where(v => v.Status == status)
            .ToList();

        return Task.FromResult(verifications);
    }

    public Task UpdateAsync(Verification verification)
    {
        return Task.CompletedTask;
    }
}