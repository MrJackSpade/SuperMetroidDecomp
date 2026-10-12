namespace BuildPolicy;

/// <summary>IDE0330 compliant fixture protecting a counter with the dedicated Lock type.</summary>
internal sealed class Ide0330
{
    /// <summary>Dedicated monitor used to serialize updates to the fixture counter.</summary>
    private readonly Lock _gate = new();
    /// <summary>Value incremented only while the dedicated lock is held.</summary>
    private int _count;

    /// <summary>Acquires the dedicated lock before incrementing and returning the counter.</summary>
    internal int Increment()
    {
        lock (_gate)
            return ++_count;
    }
}
