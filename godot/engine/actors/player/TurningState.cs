using Engine.Shared;

namespace Engine.Actors;

/// <summary>
/// Turning around on the ground: the player slows for a moment, loses any momentum, and then faces the other way.
/// </summary>
public sealed class TurningState : PlayerState
{
    /// <summary>The updates left before the turn is done.</summary>
    private Countdown _left;

    /// <inheritdoc/>
    public override bool CanAttack => false;

    /// <inheritdoc/>
    public override void Enter(Player owner) => _left = Countdown.Of(owner.TurnUpdates);

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        // The update's movement still carries the dash; the turn ends it afterwards. A turn that leaves the ground is
        // finished all the same, and what follows it finds the ground gone.
        owner.Slow(owner.Moves(TurningMoves));
        owner.MoveAcross();
        owner.LoseMomentum();
        _left = _left.Stepped();
        return null;
    }

    /// <inheritdoc/>
    public override PlayerState? Ended(Player owner)
    {
        if (_left.IsRunning)
            return null;
        owner.Facing = owner.Facing.Opposite();
        return owner.Intent != 0 ? new RunningState() : new StandingState();
    }
}
