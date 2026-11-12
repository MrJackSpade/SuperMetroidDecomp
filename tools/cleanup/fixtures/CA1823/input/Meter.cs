namespace Fixture;

/// <summary>A meter with private fields nothing reads.</summary>
public sealed class Meter
{
    private readonly int _first = Next();
    private readonly int _second = Next();
    private readonly string _name = "meter";
    private readonly int _used, _spare;

    /// <summary>Creates a meter at <paramref name="value"/>.</summary>
    public Meter(int value)
    {
        _used = value;
    }

    /// <summary>Creates a meter at zero.</summary>
    public Meter()
        : this(0)
    {
    }

    /// <summary>The meter's value.</summary>
    public int Value => _used;

    private static int Next() => 1;
}
