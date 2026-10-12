namespace BuildPolicy;

/// <summary>Represents ide0330.</summary>
internal sealed class Ide0330
{
    /// <summary>Stores gate.</summary>
    private readonly object _gate = new();
    /// <summary>Stores count.</summary>
    private int _count;

    /// <summary>increment.</summary>
    internal int Increment()
    {
        lock (_gate)
            return ++_count;
    }
}
