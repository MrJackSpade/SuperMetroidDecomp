namespace Engine.Actors;

/// <summary>
/// Falling through the air, having walked off a floor or been let go in the air, until landing. Steering against the
/// way the player faces turns them in the air.
/// </summary>
public sealed class FallingState : PlayerState
{
    /// <summary>The stance the player last held in this fall, and the one this update's input asks for.</summary>
    private int _stance = -1, _stanceAsked;

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        owner.TrySetForm(owner.FormOr(Player.Falling), aroundMiddle: true);
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

    /// <summary>A change of stance sets the steering afresh.</summary>
    public override void CarriedOn(Player owner)
    {
        if (_stanceAsked != _stance && _stance >= 0)
            owner.SteerAfresh();
        _stance = _stanceAsked;
    }

    /// <summary>With nothing asked, the aim comes level, and a dash and its momentum are lost.</summary>
    public override PlayerState? Unasked(Player owner)
    {
        CarriedOn(owner);
        owner.LoseMomentum();
        return null;
    }
}
