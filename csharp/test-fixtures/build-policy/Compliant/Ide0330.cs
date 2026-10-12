namespace BuildPolicy;

/// <summary>Represents ide0330.</summary>
internal sealed class Ide0330
{
    /// <summary>Stores gate.</summary>
    private readonly Lock _gate = new();
    /// <summary>Stores count.</summary>
    private int _count;

    /// <summary>Increment.</summary>
    internal int Increment()
    {
        lock (_gate)
            return ++_count;
    }
}
