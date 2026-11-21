namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Lower Norfair Ridley's arena targets by the way he faces (#2335).</summary>
internal static partial class RidleyMovementTargets
{
    /// <summary>The wall sweep's point ahead of Ridley: successive 64-pixel quarter-arena points by facing.</summary>
    internal static RidleyFacedX WallAhead { get; } = new(FacingLeft: 64, Turning: 128, FacingRight: 192);

    /// <summary>The wall sweep's point behind Ridley: the quarter-arena point opposite <see cref="WallAhead"/>.</summary>
    internal static RidleyFacedX WallBehind { get; } = new(FacingLeft: 192, Turning: 128, FacingRight: 64);

    /// <summary>Where Ridley hovers between attacks: left of centre unless he faces left.</summary>
    internal static RidleyFacedX Hover { get; } = new(FacingLeft: 192, Turning: 96, FacingRight: 96);

    /// <summary>Where Ridley's swoop starts from: near the left wall unless he faces left.</summary>
    internal static RidleyFacedX SwoopStart { get; } = new(FacingLeft: 192, Turning: 64, FacingRight: 64);

    /// <summary>The side of the arena the pogo attack starts from.</summary>
    internal static RidleyFacedX PogoSide { get; } = new(FacingLeft: 176, Turning: 128, FacingRight: 96);

    /// <summary>Where Ridley carries a grabbed Samus to: either side of the arena, or its left edge turning toward the screen.</summary>
    internal static RidleyFacedX CarryAnchor { get; } = new(FacingLeft: 64, Turning: 0, FacingRight: 208);

    /// <summary>
    /// Where Ridley flies once he lets a carried Samus go: the carry's anchor reflected about the arena's centre, or the
    /// arena's left edge turning toward the screen.
    /// </summary>
    internal static RidleyFacedX CarryRelease { get; } = new(FacingLeft: 176, Turning: 0, FacingRight: 80);
}
