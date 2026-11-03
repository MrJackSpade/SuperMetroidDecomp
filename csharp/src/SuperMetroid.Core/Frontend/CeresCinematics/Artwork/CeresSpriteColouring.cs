namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>
/// The colours a Ceres cinematic actor's palette-inheriting parts draw in (#1512), named for what the Ceres flight and the
/// Ceres destruction show in them. Which of the flight's colours each one is, the import alone knows.
/// </summary>
public enum CeresSpriteColouring
{
    /// <summary>The approach's and the reveal's asteroids, stars and vortex.</summary>
    SpaceObjects,

    /// <summary>The Ceres station as the approach shows it under attack.</summary>
    Station,

    /// <summary>Planet Zebes as the escape reaches it.</summary>
    Zebes,

    /// <summary>The planet's name shown as the escape reaches Zebes.</summary>
    ZebesTitle,

    /// <summary>The Ceres destruction's explosions.</summary>
    Explosions,
}
