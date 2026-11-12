namespace Fixture;

/// <summary>A gauge with private members nothing uses.</summary>
public sealed class Gauge
{
    private readonly int _seeded = Seed();
    private readonly int _level;

    /// <summary>Creates a gauge at <paramref name="level"/>.</summary>
    public Gauge(int level)
    {
        _level = level;
    }

    /// <summary>The gauge's level.</summary>
    public int Level => _level;

    private static int Seed() => 3;
}
