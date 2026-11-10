namespace SuperMetroid.Core.Fx;

/// <summary>
/// One earthquake's look: how strongly and which way it throws the room, and what it shakes. A producer starts the
/// room's earthquake with one (<see cref="RoomEarthquake"/>); its displacement comes from
/// <see cref="EarthquakeShakeDefinitions"/>.
/// </summary>
/// <param name="Strength">How far the shake throws what it shakes.</param>
/// <param name="Direction">Which way the shake throws what it shakes.</param>
/// <param name="Reach">What the shake shakes.</param>
public readonly record struct EarthquakeShake(EarthquakeStrength Strength, EarthquakeDirection Direction, EarthquakeReach Reach)
{
    /// <summary>Whether the shake also shakes the enemies and draws their projectiles displaced.</summary>
    public bool ShakesEnemies => EarthquakeShakeDefinitions.EnemyReaches.Contains(Reach);

    /// <summary>
    /// The foreground, background and enemy-projectile displacement the shake shows on the side of its rest position it
    /// starts on; the room's earthquake shows its negation on the other side.
    /// </summary>
    internal RoomShakeDefinition Displacement
    {
        get
        {
            int pixels = EarthquakeShakeDefinitions.Pixels[Strength];
            Offset16<RoomPixels> thrown = new(
                EarthquakeShakeDefinitions.HorizontalDirections.Contains(Direction) ? pixels : 0,
                EarthquakeShakeDefinitions.VerticalDirections.Contains(Direction) ? pixels : 0);
            return new(
                EarthquakeShakeDefinitions.ForegroundReaches.Contains(Reach) ? thrown : Offset16<RoomPixels>.Zero,
                EarthquakeShakeDefinitions.BackgroundReaches.Contains(Reach)
                    ? new(thrown.X, thrown.Y)
                    : Offset16<BackgroundPixels>.Zero,
                ShakesEnemies ? thrown : Offset16<RoomPixels>.Zero);
        }
    }
}
