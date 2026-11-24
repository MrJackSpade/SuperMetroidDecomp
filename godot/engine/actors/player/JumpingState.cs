using Engine.Input;
using Engine.Shared;

namespace Engine.Actors;

/// <summary>
/// A jump, from its start until the player lands: it keeps the jump's own move values through its rise and its fall.
/// A jump from the ground may first gather itself, moving nothing. Letting go of the jump while rising cuts the rise
/// short; steering against the way the player faces turns them in the air.
/// </summary>
public sealed class JumpingState : PlayerState
{
    /// <summary>Whether the jump keeps the upward speed the player already has, as one carried on from a wall jump does.</summary>
    private readonly bool _keepsSpeed;

    /// <summary>The updates left in which a jump from the ground gathers itself.</summary>
    private Countdown _gathering;

    /// <summary>Whether the player had any speed of their own across as this update's input was asked.</summary>
    private bool _hadSpeed;

    /// <summary>The stance the player last held in this jump, and the one this update's input asks for.</summary>
    private int _stance = -1, _stanceAsked;

    /// <summary>A jump from the ground.</summary>
    public JumpingState()
    {
    }

    /// <summary>A jump that carries on with the upward speed the player already has.</summary>
    public JumpingState(bool keepsSpeed) => _keepsSpeed = keepsSpeed;

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        owner.TrySetForm(owner.FormOr(Player.Jumping), aroundMiddle: true);
        if (_keepsSpeed)
        {
            owner.SteerAfresh();
            return;
        }

        owner.Rise(owner.LaunchSpeed(offWall: false));
        _gathering = Countdown.Of(owner.InMedium("jump_start_updates", owner.JumpStartUpdates));
    }

    /// <inheritdoc/>
    public override PlayerState? Asked(Player owner)
    {
        owner.AimFromInput();
        _hadSpeed = !owner.Velocity.X.IsZero;
        _stanceAsked = AirStance.Of(owner, forwardCounts: true);

        // A jump that is gathering itself listens to nothing but the aim.
        if (_gathering.IsRunning)
            return this;
        if (AirTurningState.IsAskedFor(owner))
            return new AirTurningState(fromJump: true);
        return AirStance.AsksNothing(owner, jumpCounts: true) ? null : this;
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        if (owner.WallJumpFromAnyJump && WallJumpingState.CanStart(owner))
            return new WallJumpingState();

        owner.StepDash(building: false);
        if (_gathering.IsRunning)
        {
            // Gathering, the player moves nothing of their own: a dash still carries them, and nothing else.
            owner.Steering = Steering.Steered;
            owner.MoveAcross(byDashAlone: true);
            _gathering = _gathering.Stepped();
            if (_gathering.IsOver)
                owner.SteerAfresh();
            return null;
        }

        CutShort(owner);
        owner.SteerAndMoveInAir(owner.Moves(JumpingMoves));
        return AirStance.FallAndLand(owner);
    }

    /// <summary>A change of stance sets the steering afresh.</summary>
    public override void CarriedOn(Player owner)
    {
        if (_stanceAsked != _stance && _stance >= 0)
            owner.SteerAfresh();
        _stance = _stanceAsked;
    }

    /// <summary>
    /// With nothing at all held, a jumper who has speed across becomes unsteered: the dash is added into that speed
    /// and lost, and the momentum ends. One with no speed is steered.
    /// </summary>
    public override PlayerState? Unasked(Player owner)
    {
        if (_hadSpeed && !owner.Velocity.X.IsZero)
        {
            owner.FoldDash();
            owner.Steering = Steering.Unsteered;
        }
        else
        {
            owner.Steering = Steering.Steered;
        }

        owner.EndMomentum();
        return null;
    }

    /// <summary>
    /// Ends a rise at its top, the update after gravity has slowed it past rest, and cuts a rising jump short once the
    /// jump action is no longer held: to the cut speed, or, where that is none, to a fall from rest.
    /// </summary>
    public static void CutShort(Player owner)
    {
        if (!owner.IsRising)
            return;
        if (owner.TurnAtTop() || owner.Input.IsHeld(GameAction.Jump))
            return;
        Pixels cut = -Pixels.FromNumber(owner.JumpCutSpeed);
        if (cut.IsZero)
            owner.FallFromRest();
        else if (owner.Velocity.Y < cut)
            owner.Velocity = owner.Velocity.WithY(cut);
    }
}
