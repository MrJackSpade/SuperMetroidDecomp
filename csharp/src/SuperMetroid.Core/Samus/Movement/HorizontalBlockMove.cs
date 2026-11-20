namespace SuperMetroid.Core.Samus.Movement;

/// <summary>One horizontal block move: the displacement and which collision side effects it publishes (#633).</summary>
public sealed record HorizontalBlockMove
{
    /// <summary>Signed horizontal displacement; negative moves Samus left, and zero or more moves or probes her
    /// rightward.</summary>
    public required SignedSixteenSixteen Displacement { get; init; }

    /// <summary>Explicit permission to break supported collision-bomb blocks in addition to pose and momentum checks.</summary>
    public bool CanBreakBombBlocks { get; init; } = false;

    /// <summary>Enables post-move non-square slope alignment when the state allows slope alignment, and live movement inheritance publication.</summary>
    public bool AlignToSlopeAfterMovement { get; init; } = true;

    /// <summary>
    /// Whether the move only observes the blocks, as the pose and wall probes do: it activates no station and records no
    /// inherited movement, its non-square slopes react as a floor, and its quicksand and contact crumbles react to no
    /// direction of travel.
    /// </summary>
    public bool Observes { get; init; } = false;

    /// <summary>
    /// The direction of a move deliberately made left or right. When supplied, the solid-enemy probe always runs in this
    /// direction, even for no displacement, while the displacement's sign still selects the side entered (none enters on
    /// the right). Null probes in the displacement's own direction and skips the probe for no displacement.
    /// </summary>
    public HorizontalDirection? CollisionMovementDirection { get; init; } = null;
}
