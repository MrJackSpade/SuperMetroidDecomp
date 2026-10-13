namespace BuildPolicy;

/// <summary>Exercises the lock-body style rule through a synchronized increment.</summary>
internal sealed class Ide0330
{
    /// <summary>The monitor shared by increments of this fixture's counter.</summary>
    private readonly object _gate = new();
    /// <summary>The value protected by the monitor.</summary>
    private int _count;

    /// <summary>Increments the counter while holding its monitor.</summary>
    internal int Increment()
    {
        lock (_gate)
            return ++_count;
    }
}
