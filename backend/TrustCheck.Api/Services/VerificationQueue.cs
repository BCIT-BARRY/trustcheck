using System.Threading.Channels;

namespace TrustCheck.Api.Services;

public class VerificationQueue
{
    private readonly Channel<Guid> _queue = Channel.CreateUnbounded<Guid>();

    public ValueTask EnqueueAsync(Guid id)
    {
        return _queue.Writer.WriteAsync(id);
    }

    public ValueTask<Guid> DequeueAsync(CancellationToken cancellationToken)
    {
        return _queue.Reader.ReadAsync(cancellationToken);
    }
}