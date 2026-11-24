namespace Engine.Actors;

/// <summary>
/// Falling through the air, having walked off a floor or been let go in the air, until landing. Steering against the
/// way the player faces turns them in the air.
/// </summary>
public sealed class FallingState : PlayerState
{
    /// <summary>The stance the player last held in this fall, once they have held one, and the one this update's input asks for.</summary>
    private AirStance? _stance;
    private AirStance _stanceAsked;

    private readonly bool _offFloor;

    /// <summary>A fall.</summary>
    /// <param name="offFloor">Whether the player has walked off a floor, and so falls from rest unless they are rising.</param>
    public FallingState(bool offFloor = false) => _offFloor = offFloor;

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        owner.TrySetForm(AirStance.FormFor(owner, Player.Falling), aroundMiddle: true);
        if (_offFloor && !owner.IsRising)
            owner.FallFromRest();
        owner.SteerAfresh();
    }

    /// <inheritdoc/>
    public override PlayerState? Asked(Player owner)
    {
        owner.AimFromInput();
        if (AirTurningState.IsAskedFor(owner))
            return new AirTurningState(fromJump: false);
        _stanceAsked = AirStance.Of(owner, forwardCounts: false);
        return AirStance.AsksNothing(owner, jumpCounts: false) ? null : this;
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        if (owner.WallJumpFromAnyJump && WallJumpingState.CanStart(owner))
            return new WallJumpingState();

        owner.StepDash(building: false);
        owner.SteerAndMoveInAir(owner.Moves(FallingMoves));

        // The fall moves by the speed it had, and gravity adds to it afterwards: a fall from rest moves nothing in its
        // first update.
        owner.TurnAtTop();
        return AirStance.FallAndLand(owner);
    }

    /// <summary>A change of stance sets the steering afresh, and the body takes the form of the stance where it fits.</summary>
    public override void CarriedOn(Player owner)
    {
        if (_stance is { } held && _stanceAsked != held)
            owner.SteerAfresh();
        _stance = _stanceAsked;
        owner.TrySetForm(AirStance.FormFor(owner, Player.Falling), aroundMiddle: true);
    }

    /// <summary>With nothing asked, the aim comes level, and a dash and its momentum are lost.</summary>
    public override PlayerState? Unasked(Player owner)
    {
        CarriedOn(owner);
        owner.LoseMomentum();
        return null;
    }
}
