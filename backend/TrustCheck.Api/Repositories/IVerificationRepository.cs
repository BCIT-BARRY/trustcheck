using TrustCheck.Api.Models;

namespace TrustCheck.Api.Repositories;

public interface IVerificationRepository
{
    Task AddAsync(Verification verification);

    Task<Verification?> GetByIdAsync(Guid id);

    Task<IReadOnlyList<Verification>> GetAllAsync();

    Task<IReadOnlyList<Verification>> GetByStatusAsync(string status);

    Task UpdateAsync(Verification verification);
}