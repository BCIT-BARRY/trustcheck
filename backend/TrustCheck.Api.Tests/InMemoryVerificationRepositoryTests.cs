using TrustCheck.Api.Models;
using TrustCheck.Api.Repositories;

namespace TrustCheck.Api.Tests;

public class InMemoryVerificationRepositoryTests
{
    private readonly InMemoryVerificationRepository _repository = new();

    [Fact]
    public async Task GetByIdAsync_MissingId_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEveryVerification()
    {
        await _repository.AddAsync(new Verification { Id = Guid.NewGuid(), Status = "Submitted" });
        await _repository.AddAsync(new Verification { Id = Guid.NewGuid(), Status = "Completed" });

        var all = await _repository.GetAllAsync();

        Assert.Equal(2, all.Count);
    }

    [Fact]
    public async Task GetByStatusAsync_OnlyReturnsMatchingStatus()
    {
        await _repository.AddAsync(new Verification { Id = Guid.NewGuid(), Status = "Submitted" });
        await _repository.AddAsync(new Verification { Id = Guid.NewGuid(), Status = "Completed" });
        await _repository.AddAsync(new Verification { Id = Guid.NewGuid(), Status = "Submitted" });

        var submitted = await _repository.GetByStatusAsync("Submitted");

        Assert.Equal(2, submitted.Count);
        Assert.All(submitted, v => Assert.Equal("Submitted", v.Status));
    }
}
