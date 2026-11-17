namespace SuperMetroid.Core.Samus.Movement;

/// <summary>One vertical block move: the displacement, column scan order, and which collision side effects it publishes (#633).</summary>
public sealed record VerticalBlockMove
{
    /// <summary>Signed vertical displacement; negative moves up and nonnegative moves down.</summary>
    public required SignedSixteenSixteen Displacement { get; init; }

    /// <summary>Whether the move scans the columns Samus covers from left to right; false scans them right to left.</summary>
    public required bool ScanLeftToRight { get; init; }

    /// <summary>Whether the move breaks the bomb blocks it touches, as a shinespark does.</summary>
    public bool CanBreakBombBlocks { get; init; } = false;

    /// <summary>Whether solid enemies stop the move as blocks do.</summary>
    public bool IncludeSolidEnemies { get; init; } = true;

    /// <summary>Whether a door block the move touches publishes its door transition and other side effects.</summary>
    public bool PublishDoorSideEffects { get; init; } = true;

    /// <summary>Whether landing in quicksand publishes her grounding in it.</summary>
    public bool PublishQuicksandGrounding { get; init; } = true;

    /// <summary>
    /// The direction the touched blocks react to, or null to take the move's own; the nondirectional probe suppresses
    /// directional station activation.
    /// </summary>
    public SamusCollisionDirection? BlockReactionDirection { get; init; } = null;
}
