namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>Where an Evir's arms and idle spit sit on its body (#2149).</summary>
internal static class EvirMotionDefinitions
{
    /// <summary>Where an Evir's arms sit from its body's middle while it faces right; facing left they sit as far to its left.</summary>
    internal static Offset16<RoomPixels> ArmsFacingRight { get; } = new(4, 10);

    /// <summary>Where an Evir's idle spit hangs from its body's middle while it faces right; facing left it hangs as far to its left.</summary>
    internal static Offset16<RoomPixels> IdleSpitFacingRight { get; } = new(4, 18);
}
