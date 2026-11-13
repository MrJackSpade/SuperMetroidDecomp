namespace SuperMetroid.Core.Samus.Movement;

/// <summary>One horizontal block move: the displacement and which collision side effects it publishes (#633).</summary>
public sealed record HorizontalBlockMove
{
    /// <summary>Signed horizontal displacement; negative moves left and nonnegative selects the right entry.</summary>
    public required SignedSixteenSixteen Displacement { get; init; }

    /// <summary>Explicit permission to break supported collision-bomb blocks in addition to pose and momentum checks.</summary>
    public bool CanBreakBombBlocks { get; init; } = false;

    /// <summary>Whether horizontal door dispatch publishes the door transition's side effects rather than only resolving the destination.</summary>
    public bool PublishDoorSideEffects { get; init; } = true;

    /// <summary>Enables post-move non-square slope alignment when the state's slope-enable bit permits it, and live movement inheritance publication.</summary>
    public bool AlignToSlopeAfterMovement { get; init; } = true;

    /// <summary>Optional block-reaction direction override; the nondirectional probe value suppresses directional station activation.</summary>
    public SamusCollisionDirection? BlockReactionDirection { get; init; } = null;

    /// <summary>The direction stored by the displacement calculator. When supplied, this is a directed move right or left: the solid-enemy probe always runs in this direction,      even for a zero displacement, while the sign selects the entry (zero enters right).      Null keeps a sign-derived probe that is skipped for a zero displacement.</summary>
    public SamusCollisionDirection? CollisionMovementDirection { get; init; } = null;
}
