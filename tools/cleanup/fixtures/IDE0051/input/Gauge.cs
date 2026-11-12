namespace Fixture;

/// <summary>A gauge with private members nothing uses.</summary>
public sealed class Gauge
{
    private int _unused;
    private readonly int _seeded = Seed();
    private readonly int _level;

    /// <summary>Creates a gauge at <paramref name="level"/>.</summary>
    public Gauge(int level)
    {
        _level = level;
    }

    /// <summary>The gauge's level.</summary>
    public int Level => _level;

    private event EventHandler? Changed;

    private int Doubled => _level * 2;

    private int Halved() => _level / 2;

    private static int Seed() => 3;
}
