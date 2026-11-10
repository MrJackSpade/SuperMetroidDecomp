namespace SuperMetroid.Core.Earthquakes;

/// <summary>One earthquake shake's background and enemy-projectile displacement (<see cref="EarthquakeShake.Displacement"/>).</summary>
/// <param name="Foreground">The foreground scroll displacement.</param>
/// <param name="Background">The background scroll displacement.</param>
/// <param name="Projectile">Enemy-projectile draw displacement.</param>
internal readonly record struct RoomShakeDefinition(
    Offset16<RoomPixels> Foreground, Offset16<BackgroundPixels> Background, Offset16<RoomPixels> Projectile);
