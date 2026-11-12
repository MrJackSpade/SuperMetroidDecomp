namespace Fixture;

/// <summary>A counter whose updates hold a dedicated monitor.</summary>
public sealed class Counter
{
    private readonly Lock _gate = new();
    private int _count;

    /// <summary>Increments the counter while holding its monitor and returns the new value.</summary>
    public int Increment()
    {
        lock (_gate)
            return ++_count;
    }
}
