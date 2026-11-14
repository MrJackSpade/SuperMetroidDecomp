namespace SuperMetroid.Core.Samus.Movement;

/// <summary>The water-entry splash Samus's movement type selects. Its values are arbitrary.</summary>
internal enum WaterSplashKind
{
    /// <summary>Single diving water-entry splash used by airborne movement.</summary>
    Diving,
    /// <summary>Paired surface splashes used by grounded postures.</summary>
    GroundedPair,
}
