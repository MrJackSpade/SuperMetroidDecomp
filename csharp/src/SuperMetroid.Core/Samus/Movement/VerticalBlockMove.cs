namespace SuperMetroid.Core.Samus.Movement;

/// <summary>One vertical block move: the displacement, column scan order, and which collision side effects it publishes (#633).</summary>
public sealed partial record VerticalBlockMove
{
    /// <summary>Signed vertical displacement; negative moves up and nonnegative moves down.</summary>
    public required SignedSixteenSixteen Displacement { get; init; }

    /// <summary>Whether the move scans the columns Samus covers from left to right; false scans them right to left.</summary>
    public required bool ScanLeftToRight { get; init; }

    /// <summary>Whether the move breaks the bomb blocks it touches, as a shinespark does.</summary>
    public bool CanBreakBombBlocks { get; init; } = false;

    /// <summary>Whether solid enemies stop the move as blocks do.</summary>
    public bool IncludeSolidEnemies { get; init; } = true;

    /// <summary>What the door blocks the move meets do to Samus beyond stopping her or letting her pass (#2545).</summary>
    public DoorBlockMeeting DoorBlocks { get; init; } = DoorBlockMeeting.Contacts;

    /// <summary>
    /// Whether the move only observes the blocks, as the pose and wall probes do: it activates no station and records no
    /// inherited movement, its non-square slopes react as a floor, and its quicksand and contact crumbles react to no
    /// direction of travel.
    /// </summary>
    public bool Observes { get; init; } = false;
}
