namespace BuildPolicy;

/// <summary>Exercises the auto-property style rule with a manually backed value.</summary>
internal sealed class Ide0032
{
    /// <summary>Backing value read and written only by the fixture property.</summary>
    private int _value;

    /// <summary>Uses explicit accessors for an otherwise trivial stored value.</summary>
    internal int Value
    {
        get => _value;
        set => _value = value;
    }
}
