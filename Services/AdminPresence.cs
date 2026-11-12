namespace Pisco.Services;

public sealed class AdminPresence
{
    private readonly object gate = new();
    private int sessions;
    private DateTimeOffset? lastSeen;

    public event Action? Changed;

    public bool IsOnline
    {
        get
        {
            lock (gate)
                return sessions > 0;
        }
    }

    public DateTimeOffset? LastSeen
    {
        get
        {
            lock (gate)
                return lastSeen;
        }
    }

    public void Join() => update(+1);

    public void Leave() => update(-1);

    private void update(int delta)
    {
        bool before, after;
        lock (gate)
        {
            before = sessions > 0;
            sessions = Math.Max(0, sessions + delta);
            after = sessions > 0;
            if (before && !after)
                lastSeen = DateTimeOffset.UtcNow;
        }
        if (before != after)
            Changed?.Invoke();
    }
}
