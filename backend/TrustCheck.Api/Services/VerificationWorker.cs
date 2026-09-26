namespace TrustCheck.Api.Services;

public class VerificationWorker : BackgroundService
{
    private readonly VerificationQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<VerificationWorker> _logger;

    public VerificationWorker(
        VerificationQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<VerificationWorker> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var id = await _queue.DequeueAsync(stoppingToken);

            // Simulates an external identity provider taking time to respond.
            await Task.Delay(1000, stoppingToken);

            // One failed verification must not stop the worker for every other one.
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var service =
                    scope.ServiceProvider.GetRequiredService<VerificationService>();

                await service.CompleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Verification {VerificationId} failed to complete.", id);
            }
        }
    }
}
