namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// One vertical terrain scan: what <see cref="SamusBlockCollision"/>'s column probe reads from a
/// vertical move. Every member is explicit; the scan has no default side-effect policy (#633).
/// </summary>
internal readonly partial record struct VerticalTerrainProbe
{
    /// <summary>Signed vertical displacement to probe.</summary>
    public required SignedSixteenSixteen Displacement { get; init; }

    /// <summary>Whether the probed columns are scanned left to right.</summary>
    public required bool ScanLeftToRight { get; init; }

    /// <summary>Whether bomb blocks along the probe break.</summary>
    public required bool CanBreakBombBlocks { get; init; }

    /// <summary>What the door blocks the probe meets do to Samus beyond stopping her or letting her pass (#2545).</summary>
    public required DoorBlockMeeting DoorBlocks { get; init; }

    /// <summary>
    /// Whether the move only observes the blocks, as the pose and wall probes do: it activates no station and records no
    /// inherited movement, its non-square slopes react as a floor, and its quicksand and contact crumbles react to no
    /// direction of travel.
    /// </summary>
    public required bool Observes { get; init; }
}
