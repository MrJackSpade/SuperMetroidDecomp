namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>The animations of the Ceres cinematics' actors: the flight to Ceres, its destruction and the Zebes reveal (#2197).</summary>
internal enum CeresAnimation
{
    /// <summary>The flight's star field.</summary>
    Stars,
    /// <summary>The large asteroids drifting past on the flight to Ceres.</summary>
    LargeAsteroid,
    /// <summary>Ceres station under attack.</summary>
    StationUnderAttack,
    /// <summary>The small asteroids.</summary>
    SmallAsteroid,
    /// <summary>The purple space vortex, alternating its two drawings every update.</summary>
    Vortex,
    /// <summary>The large asteroids drifting past Ceres as the station is destroyed.</summary>
    DestructionLargeAsteroid,
    /// <summary>Ceres's first delayed small explosion.</summary>
    SmallExplosion,
    /// <summary>Ceres's repeating small explosion: a small explosion and a pause, played six times.</summary>
    RepeatingSmallExplosion,
    /// <summary>Ceres's final large explosion: a large blast and a pause, played seven times.</summary>
    LargeExplosion,
    /// <summary>The Ceres station blast.</summary>
    StationBlast,
    /// <summary>Planet Zebes.</summary>
    ZebesPlanet,
    /// <summary>The upper-left quarter of the stars around Zebes.</summary>
    ZebesStarsUpperLeft,
    /// <summary>The upper-right quarter of the stars around Zebes.</summary>
    ZebesStarsUpperRight,
    /// <summary>The lower-left quarter of the stars around Zebes.</summary>
    ZebesStarsLowerLeft,
    /// <summary>The lower-right quarter of the stars around Zebes.</summary>
    ZebesStarsLowerRight,
    /// <summary>The PLANET ZEBES title, which fades in and out and starts the flight to Zebes.</summary>
    ZebesTitle,
    /// <summary>The Japanese PLANET ZEBES title.</summary>
    ZebesJapaneseTitle,
    /// <summary>The Ceres explosion spawner's two waves and the repeating explosions between them.</summary>
    ExplosionSpawner,
}
