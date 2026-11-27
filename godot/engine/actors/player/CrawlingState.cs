using Engine.Collision;
using Engine.Input;
using Engine.Shared;

namespace Engine.Actors;

/// <summary>
/// The low form: a small body that fits low gaps, moving on the ground and through the air. Up changes back to a
/// crouch where there is room, and so does the jump for a player who cannot jump in this form. On the ground it rolls
/// while a direction is held and stops dead when it is let go. Coming down fast it bounces, twice, before it is on the
/// ground again. Where the game allows it, the low form can jump, and one that lands with the jump held jumps again.
/// </summary>
public sealed class CrawlingState : PlayerState
{
    /// <summary>Whether the low form is off the ground: fallen off a floor, jumped, or bouncing.</summary>
    private bool _airborne;

    /// <summary>
    /// Whether the low form is off the ground in this state, by what it is doing: fallen off a floor, jumped, or
    /// bouncing. It is the state's own account, kept from one update to the next, and not a look at what lies under
    /// the player now: a game whose enemies choose by what the player is doing asks it.
    /// </summary>
    public bool IsAirborne => _airborne && !_asOnGround;

    /// <summary>
    /// Whether, in the air, it is taken to be on the ground (<see cref="TakeAsOnGround"/>): it rolls and may jump as
    /// on the ground and goes on rising or falling as it was, until it lands or jumps.
    /// </summary>
    private bool _asOnGround;

    /// <summary>Whether it does what the low form on the ground does: it is on it, or is taken to be.</summary>
    private bool StandsAsOnGround => !_airborne || _asOnGround;

    /// <summary>
    /// Takes a low form that is in the air to be on the ground from here on, without moving it or changing its
    /// speed: for a game that puts the player in the low form's pose of the ground wherever they are, as this game
    /// does as a pause ends in which what lets the low form jump was switched. Until it lands or jumps it rolls
    /// across by the ground's rule, may jump as from the ground, and goes on rising or falling under gravity; a jump
    /// of its own or a bounce it was in is over. On the ground it changes nothing.
    /// </summary>
    public void TakeAsOnGround()
    {
        if (_airborne)
            (_asOnGround, _jumping, _bounces) = (true, false, 0);
    }

    /// <summary>Whether it is in a jump of its own.</summary>
    private bool _jumping;

    /// <summary>How many times it has bounced since it last fell.</summary>
    private int _bounces;

    /// <summary>Whether it is rolling along the ground, a direction having been held, and not sitting still.</summary>
    private bool _rolling;

    /// <summary>What this update's input asked for within the state.</summary>
    private bool _jumpAsked, _turnAsked, _rollAsked;

    /// <summary>Whether it had any speed of its own across as this update's input was asked.</summary>
    private bool _hadSpeed;

    /// <inheritdoc/>
    public override bool CanAttack => true;

    /// <inheritdoc/>
    public override FormAsked? EntersIn(Player owner) => new FormAsked(Player.Crawling);

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        // One adrift takes the low form as on the ground: its first move finds no floor and it falls from rest.
        _airborne = owner.IsRising || (!owner.IsAdrift && !owner.IsOnGround);
        _asOnGround = false;
    }

    /// <summary>Thrown, it has bounced no times: it comes down from the throw as from any fall.</summary>
    public override void OnThrown(Player owner) => _bounces = 0;

    /// <inheritdoc/>
    public override PlayerState? Asked(Player owner)
    {
        owner.Aim = Aim.Forward;
        GameInput input = owner.Input;
        _hadSpeed = !owner.Velocity.X.IsZero;
        (_jumpAsked, _turnAsked, _rollAsked) = (false, false, false);
        bool jumps = owner.MayJumpWhileCrawling;
        if ((input.WasPressed(GameAction.Up) || (!jumps && owner.JumpPressed)) && owner.CanTakeForm(Player.Crouching))
            return owner.LeaveLowForm();
        if (jumps && StandsAsOnGround && owner.JumpPressed)
        {
            _jumpAsked = true;
            return this;
        }

        if (owner.Intent == 0)
            return null;
        _turnAsked = (owner.Intent > 0) != (owner.Facing == Facing.Right);
        _rollAsked = true;
        return this;
    }

    /// <summary>What the input asked for within the state takes effect once the low form has moved.</summary>
    public override void CarriedOn(Player owner)
    {
        if (_turnAsked)
        {
            // Turning, it goes on the old way, reversing, with whatever dash it had added into its speed.
            owner.TurnAround();
            owner.EndMomentum();
        }

        if (_rollAsked && StandsAsOnGround)
            _rolling = true;
        if (!_jumpAsked || !StandsAsOnGround)
            return;
        owner.Rise(owner.LaunchSpeed(offWall: false, owner.Form));
        (_airborne, _jumping, _bounces, _asOnGround) = (true, true, 0, false);
    }

    /// <summary>
    /// With no direction held the low form loses everything across: on the ground, and in the air where it can jump.
    /// One that cannot jump keeps falling unsteered while it has speed, its dash added into that speed.
    /// </summary>
    public override PlayerState? Unasked(Player owner)
    {
        if (!_airborne || owner.MayJumpWhileCrawling)
        {
            owner.StopAcross();
            _rolling = false;
            return null;
        }

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

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        if (!_airborne)
        {
            MoveOnGround(owner);
        }
        else if (_asOnGround)
        {
            // Taken to be on the ground in the air: across by the ground's rule, and on up or down as it was.
            RollAcross(owner);
            RiseOrFall(owner);
        }
        else
        {
            MoveInAir(owner);
        }

        return null;
    }

    /// <summary>Moves across as on the ground: rolling, or sitting still but for a dash. Returns whether it sits still.</summary>
    private bool RollAcross(Player owner)
    {
        bool still = owner.Steering == Steering.Steered && !_rolling;
        if (still)
        {
            owner.MoveAcross(byDashAlone: true);
        }
        else
        {
            owner.StepDash(building: false);
            owner.Steer(owner.Moves(CrawlingMoves));
            owner.MoveAcross();
        }

        return still;
    }

    /// <summary>Rolls along the ground, or sits still.</summary>
    private void MoveOnGround(Player owner)
    {
        bool still = RollAcross(owner);
        if (!owner.PressToGround())
        {
            // Rolled off a floor: it falls from rest.
            owner.FallFromRest();
            (_airborne, _jumping, _bounces) = (true, false, 0);
        }
        else if (still)
        {
            owner.StopAcross();
        }
    }

    /// <summary>Moves through the air: in a jump of its own, falling, or bouncing.</summary>
    private void MoveInAir(Player owner)
    {
        bool directionHeld = owner.Input.IsHeld(GameAction.Left) || owner.Input.IsHeld(GameAction.Right);
        if (_jumping && _bounces == 0)
        {
            // A jump of its own is cut short like any jump, and steered with no direction held it does not move across.
            JumpingState.CutShort(owner);
            owner.SteerAndMoveInAir(owner.Moves(CrawlingJumpingMoves));
        }
        else
        {
            // Falling or bouncing, steered with no direction held it loses everything across.
            if (!directionHeld && owner.Steering == Steering.Steered)
                owner.StopAcross();
            owner.SteerInAir(owner.Moves(_jumping ? CrawlingJumpingMoves : CrawlingInAirMoves));
            if (!directionHeld && owner.Steering == Steering.Steered)
                owner.ComeToRest();
            owner.MoveAcross();
        }

        RiseOrFall(owner);
    }

    /// <summary>Goes on up or down under gravity, landing where it comes down on something and falling from rest where it rises into something.</summary>
    private void RiseOrFall(Player owner)
    {
        owner.TurnAtTop();
        bool falling = !owner.Velocity.Y.IsNegative;
        MoveResult down = owner.MoveDownUnderGravity(keepsSpeedWhenStopped: true);
        if (!down.Blocked)
            return;
        if (falling)
            Land(owner);
        else
            owner.FallFromRest();
    }

    /// <summary>
    /// Comes down: a low form that can jump, with the jump held, jumps again at once; one coming down fast enough
    /// bounces, and bounces once more, lower; otherwise it is on the ground, at rest across.
    /// </summary>
    private void Land(Player owner)
    {
        _asOnGround = false;
        owner.CameDownAt(owner.Velocity.Y);
        if (owner.MayJumpWhileCrawling && owner.Input.IsHeld(GameAction.Jump))
        {
            _bounces = 0;
            owner.Rise(owner.LaunchSpeed(offWall: false, owner.Form));
            return;
        }

        bool bounces = owner.CrawlBounceSpeed > 0;
        if (bounces && _bounces == 0 && owner.LandedAtLeast(owner.CrawlBounceFallSpeed))
        {
            _bounces = 1;
            owner.Rise(Pixels.FromNumber(owner.CrawlBounceSpeed));
            return;
        }

        if (bounces && _bounces == 1)
        {
            _bounces = 2;
            owner.Rise(Pixels.FromNumber(owner.CrawlSecondBounceSpeed));
            return;
        }

        (_airborne, _jumping, _bounces, _rolling) = (false, false, 0, false);
        owner.FallFromRest();
        owner.ComeToRest();
    }

    /// <summary>
    /// Left and right held together: on the ground they are read as rightward, whichever way the player faces. In
    /// the air a low form that cannot jump reads them as the way ahead; one that can reads them as neither through a
    /// jump of its own, and as the way behind while it falls or bounces.
    /// </summary>
    public override int ReadsBothAcross(Player owner) =>
        StandsAsOnGround ? 1
        : !owner.MayJumpWhileCrawling ? Forward(owner)
        : _jumping && _bounces == 0 ? 0
        : -Forward(owner);
}
