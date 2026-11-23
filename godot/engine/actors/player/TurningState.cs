using Engine.Actors.Components;
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
    public override State<Player> Step(Player owner)
    {
        if (!owner.IsOnGround)
        {
            owner.Facing = owner.Facing.Opposite();
            return new FallingState();
        }

        // The update's movement still carries the dash; the turn ends it afterwards.
        owner.Slow(owner.Moves(TurningMoves));
        owner.MoveAcross();
        owner.LoseMomentum();
        _left = _left.Stepped();
        if (_left.IsRunning)
            return this;
        owner.Facing = owner.Facing.Opposite();
        return owner.Intent != 0 ? new RunningState() : new StandingState();
    }
}
