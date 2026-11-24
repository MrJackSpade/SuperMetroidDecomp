using Engine.Shared;

namespace Engine.Actors;

/// <summary>
/// Turning around on the ground, standing or crouched. The player faces the new way from the start and goes on moving
/// the old way, reversing, by the turn's own move values; each update of the turn ends their momentum. When its time
/// is up they stand, or crouch as they were.
/// </summary>
public sealed class TurningState : PlayerState
{
    /// <summary>The updates left before the turn is done.</summary>
    private Countdown _left;

    /// <summary>A turn on the ground.</summary>
    /// <param name="crouched">Whether the player turns crouched, and stays so.</param>
    public TurningState(bool crouched = false) => Crouched = crouched;

    /// <summary>Whether the player turns crouched.</summary>
    public bool Crouched { get; }

    /// <inheritdoc/>
    public override bool CanAttack => false;

    /// <inheritdoc/>
    public override bool HoldsOffInterruptions => true;

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        owner.TurnAround();
        _left = Countdown.Of(owner.InMedium("turn_updates", owner.TurnUpdates));
    }

    /// <summary>
    /// Standing, a jump can be asked for, and the new way held keeps the turn. Crouched, anything held keeps it. With
    /// neither, the player stops reversing.
    /// </summary>
    public override PlayerState? Asked(Player owner)
    {
        if (Crouched)
            return owner.Input.AnyHeld ? this : null;
        if (owner.JumpPressed)
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
    public override PlayerState? Ended(Player owner)
    {
        if (_left.IsRunning)
            return null;
        owner.ArmJumpBuffer();
        return Crouched ? new CrouchingState() : new StandingState();
    }

    /// <summary>A jump asked for on the last update of the turn is not lost to its end.</summary>
    public override bool EndGivesWayTo(PlayerState asked) => true;

    /// <summary>With the new way let go, the turn goes on, but the player no longer reverses: they are steered.</summary>
    public override PlayerState? Unasked(Player owner)
    {
        owner.Steering = Steering.Steered;
        owner.EndMomentum();
        return null;
    }
}
