namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>Where an Evir's arms and idle spit sit on its body, and when and how fast it spits (#2149, #2279).</summary>
internal static class EvirMotionDefinitions
{
    /// <summary>The horizontal distance, in pixels, Samus must come strictly within for an Evir to spit at her.</summary>
    internal const int SpitActivationDistance = 128;

    /// <summary>The spit's speed toward Samus, in whole pixels per update along its aim.</summary>
    internal const int SpitSpeed = 4;

    /// <summary>How far beyond each edge of the camera, in pixels, the spit flies before it regenerates at the mouth.</summary>
    internal const int SpitRegenerationMargin = 256;

    /// <summary>The horizontal distance, in pixels, from the mouth at which the regenerating spit starts.</summary>
    internal const int RegenerationStartDistance = 8;

    /// <summary>Where an Evir's arms sit from its body's middle while it faces right; facing left they sit as far to its left.</summary>
    internal static Offset16<RoomPixels> ArmsFacingRight { get; } = new(4, 10);

    /// <summary>Where an Evir's idle spit hangs from its body's middle while it faces right; facing left it hangs as far to its left.</summary>
    internal static Offset16<RoomPixels> IdleSpitFacingRight { get; } = new(4, 18);
}
