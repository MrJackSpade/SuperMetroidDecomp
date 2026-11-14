namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How a Brinstar Pipe Bug (a Zeb or Zebbo) rises out of its pipe (#1845): one pixel per update, and two on the update its
/// height's fraction is zero, which then counts down from the top of its range. A bug's height starts with no fraction
/// when it is placed, so its first emergence opens with a two-pixel rise; a bug that respawns takes its spawn height's
/// whole pixels as its fraction, so its later emergences rise one pixel per update unless it rises for that many updates.
/// </summary>
/// <remarks>
/// Consulted every update a Brinstar Pipe Bug emerges, before the test that ends its rise, and as it respawns. Evidence:
/// the original decrements the height's fraction as a value of its own and subtracts two whole pixels when it was zero,
/// one otherwise, and restores the spawn height into both the whole pixels and the fraction; the Pipe Bug emergence check
/// steps a placed and a respawned bug against that arithmetic.
/// </remarks>
[CompatibilityService("a Brinstar Pipe Bug's height fraction counts its emergence: two pixels on the update it is zero, one otherwise, and a respawn sets it to the spawn height")]
public static class PipeBugEmergenceRise
{
    /// <summary>The fractions a height holds.</summary>
    private const int FractionRange = 65536;

    /// <summary>The height after one emerging update from <paramref name="height"/>.</summary>
    public static FixedCoordinate Risen(FixedCoordinate height) => new(
        CoordinateAxis.Wrap(height.Whole - (height.Fraction == 0 ? 2 : 1)),
        (height.Fraction + FractionRange - 1) % FractionRange);

    /// <summary>The height a bug respawning at <paramref name="spawnY"/> takes: its whole pixels in both parts.</summary>
    public static FixedCoordinate Respawned(int spawnY) => new(spawnY, spawnY);
}
