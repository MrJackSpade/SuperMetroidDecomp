namespace SuperMetroid.Core.Samus.Movement;

/// <summary>Physical water-entry particle layouts selected by Samus's movement type.</summary>
internal enum WaterSplashKind : byte
{
    /// <summary>Single diving water-entry splash used by airborne movement.</summary>
    Diving = 0,
    /// <summary>Paired surface splashes used by grounded postures.</summary>
    GroundedPair = 1,
}
