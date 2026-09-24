namespace TrustCheck.Api.Services;

public class VerificationWorker : BackgroundService
{
    private readonly VerificationQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;

    public VerificationWorker(
        VerificationQueue queue,
        IServiceScopeFactory scopeFactory)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var id = await _queue.DequeueAsync(stoppingToken);

            // Simulates an external identity provider taking time to respond.
            await Task.Delay(1000, stoppingToken);

            using var scope = _scopeFactory.CreateScope();

            var service =
                scope.ServiceProvider.GetRequiredService<VerificationService>();

            await service.CompleteAsync(id);
        }
    }
}