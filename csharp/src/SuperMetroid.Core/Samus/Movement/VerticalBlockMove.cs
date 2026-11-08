namespace SuperMetroid.Core.Samus.Movement;

/// <summary>One vertical block move: the displacement, column scan order, and which collision side effects it publishes (#633).</summary>
public sealed record VerticalBlockMove
{
    /// <summary>Signed vertical displacement; negative moves up and nonnegative moves down.</summary>
    public required SignedSixteenSixteen Displacement { get; init; }

    /// <summary>Scan left to right.</summary>
    public required bool ScanLeftToRight { get; init; }

    /// <summary>Can break bomb blocks.</summary>
    public bool CanBreakBombBlocks { get; init; } = false;

    /// <summary>Include solid enemies.</summary>
    public bool IncludeSolidEnemies { get; init; } = true;

    /// <summary>Publish door side effects.</summary>
    public bool PublishDoorSideEffects { get; init; } = true;

    /// <summary>Publish quicksand grounding.</summary>
    public bool PublishQuicksandGrounding { get; init; } = true;

    /// <summary>Block reaction direction.</summary>
    public SamusCollisionDirection? BlockReactionDirection { get; init; } = null;
}
