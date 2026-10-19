using System.Collections.Concurrent;

namespace AiBusinessAssistant.Features.AI;

public sealed class ConversationLockProvider
{
    private readonly ConcurrentDictionary<int, SemaphoreSlim> _locks = new();

    public async ValueTask<IAsyncDisposable> AcquireAsync(int conversationId, CancellationToken cancellationToken)
    {
        var semaphore = _locks.GetOrAdd(conversationId, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(cancellationToken);
        return new LockHandle(semaphore);
    }

    private sealed class LockHandle(SemaphoreSlim semaphore) : IAsyncDisposable
    {
        public ValueTask DisposeAsync()
        {
            semaphore.Release();
            return ValueTask.CompletedTask;
        }
    }
}
