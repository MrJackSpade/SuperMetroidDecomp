using Engine.Input;
using Engine.Shared;

namespace Engine.Actors;

/// <summary>
/// Standing upright on the ground. Standing keeps none of the player's speed across: on each update a dash's speed
/// alone still carries them, and then everything across is at rest.
/// </summary>
public sealed class StandingState : PlayerState
{
    /// <summary>The updates left in which the player is still settling from a landing.</summary>
    private Countdown _settling;

    /// <summary>Standing.</summary>
    /// <param name="settlingUpdates">
    /// How many updates the player takes to settle, having just landed: when they have stood that long, a jump
    /// already held may once more count as pressed afresh.
    /// </param>
    public StandingState(int settlingUpdates = 0) => _settling = Countdown.Of(settlingUpdates);

    /// <summary>Whether the player is still settling from a landing.</summary>
    public bool IsSettling => _settling.IsRunning;

    /// <summary>Whether the player was still settling from a landing through this state's latest move.</summary>
    public bool MovedSettling { get; private set; }

    /// <inheritdoc/>
    public override FormAsked? EntersIn(Player owner) => new FormAsked(Player.Standing);

    /// <inheritdoc/>
    public override PlayerState? Asked(Player owner)
    {
        owner.AimFromInput();

        // An aim taken up while settling ends the landing at once: the player stands aiming, and the landing's own
        // end, with what it gives, never comes.
        if (_settling.IsRunning && owner.Aim != Shared.Aim.Forward)
            _settling = Countdown.Over;
        if (owner.JumpPressed)
            return new JumpingState();
        if (owner.MayCrouch && owner.Input.WasPressed(GameAction.Down))
            return owner.Crouch();
        if (owner.Intent == 0)
            return owner.Aim == Shared.Aim.Forward ? null : this;
        if ((owner.Intent > 0) == (owner.Facing == Shared.Facing.Right))
            return new RunningState();
        if (owner.TurnUpdates > 0)
            return new TurningState();

        // A player who turns at once is already running the other way.
        owner.TurnAround();
        return new RunningState();
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        owner.ComeToRest();
        owner.MoveAcross();
        bool grounded = owner.PressToGround();
        owner.StopAcross();
        MovedSettling = _settling.IsRunning;
        if (_settling.IsRunning)
        {
            _settling = _settling.Stepped();
            if (_settling.IsOver)
                owner.ArmJumpBuffer();
        }

        return grounded ? null : new FallingState(offFloor: true);
    }

    /// <summary>Left and right held together are read as the way behind: the player turns, and turns again once turned.</summary>
    public override int ReadsBothAcross(Player owner) => -Forward(owner);
}
