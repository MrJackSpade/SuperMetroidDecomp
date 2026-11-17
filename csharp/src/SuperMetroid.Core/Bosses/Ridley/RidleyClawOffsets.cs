namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Where Ridley's claw lies from his body (#2335), which his grab's reach and a carried Samus's place follow.</summary>
internal static class RidleyClawOffsets
{
    /// <summary>The claw's horizontal offset by the way he faces: twelve pixels less with each facing, left to right.</summary>
    internal static RidleyFacedX Across { get; } = new(FacingLeft: 12, Turning: 0, FacingRight: -12);

    /// <summary>
    /// The claw's drop below his body by his legs' reach: lower by 10.5 pixels, rounded upward, for each step his legs
    /// extend.
    /// </summary>
    internal static RidleyClawDrop Below { get; } = new(Standing: 35, LegsHalfExtended: 46, LegsExtended: 56);
}
