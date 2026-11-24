using Engine.Input;
using Engine.Shared;

namespace Engine.Actors;

/// <summary>
/// Turning around on the ground. The player faces the new way from the start and goes on moving the old way,
/// reversing, by the turn's own move values; each update of the turn ends their momentum. When its time is up they
/// stand.
/// </summary>
public sealed class TurningState : PlayerState
{
    /// <summary>The updates left before the turn is done.</summary>
    private Countdown _left;

    /// <inheritdoc/>
    public override bool CanAttack => false;

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        owner.TurnAround();
        _left = Countdown.Of(owner.TurnUpdates);
    }

    /// <inheritdoc/>
    public override PlayerState? Asked(Player owner)
    {
        if (owner.Input.WasPressed(GameAction.Jump))
            return new JumpingState();
        return owner.Intent != 0 && (owner.Intent > 0) == (owner.Facing == Facing.Right) ? this : null;
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        // The update's movement still carries what the turn began with; the turn ends the momentum afterwards. A turn
        // that leaves the ground is finished all the same, and what follows it finds the ground gone.
        owner.StepDash(building: false);
        owner.Steer(owner.Moves(TurningMoves));
        owner.MoveAcross();
        owner.LoseMomentum();
        _left = _left.Stepped();
        return null;
    }

    /// <inheritdoc/>
    public override PlayerState? Ended(Player owner) => _left.IsRunning ? null : new StandingState();

    /// <summary>A jump asked for on the turn's last update is not lost to the turn's end.</summary>
    public override bool EndGivesWayTo(PlayerState asked) => true;

    /// <summary>With the new way let go, the turn goes on, but the player no longer reverses: they are steered.</summary>
    public override PlayerState? Unasked(Player owner)
    {
        owner.Steering = Steering.Steered;
        owner.EndMomentum();
        return null;
    }
}
