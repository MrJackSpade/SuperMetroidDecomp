using Engine.Input;

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

    /// <summary>Whether this update's input is read with up and down before the way behind (<see cref="PassesOverBehind"/>).</summary>
    private bool _aimsBeforeTurning;

    /// <summary>How the arm is held in the stance the fall is in: at rest before it has one.</summary>
    public AirArm Arm => _stance?.Arm ?? AirArm.AtRest;

    private readonly bool _offFloor;

    /// <summary>A fall.</summary>
    /// <param name="offFloor">Whether the player has walked off a floor, and so falls from rest unless they are rising.</param>
    public FallingState(bool offFloor = false) => _offFloor = offFloor;

    /// <inheritdoc/>
    public override FormAsked? EntersIn(Player owner) => new FormAsked(AirStance.FormFor(owner, Player.Falling), AroundMiddle: true);

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        if (_offFloor && !owner.IsRising)
            owner.FallFromRest();
        owner.SteerAfresh();
    }

    /// <inheritdoc/>
    public override PlayerState? Asked(Player owner)
    {
        // The stance held as the update begins decides how its input is read.
        bool armOutOrAimingDown = owner.Aim == Shared.Aim.Down || (owner.Aim == Shared.Aim.Forward && Arm == AirArm.Out);
        _aimsBeforeTurning = armOutOrAimingDown && (owner.Input.IsHeld(GameAction.Up) || owner.Input.IsHeld(GameAction.Down));
        owner.AimFromInput();
        if (AirTurningState.IsAskedFor(owner))
            return new AirTurningState(fromJump: false);
        AirStance asked = AirStance.Of(owner, forwardCounts: false);
        _stanceAsked = _stance is { } held ? held.Kept(asked) : asked;
        return AirStance.AsksNothing(owner, jumpCounts: false) ? null : this;
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        owner.StepDash(building: false);
        owner.SteerAndMoveInAir(owner.Moves(FallingMoves));

        // The fall moves by the speed it had, and gravity adds to it afterwards: a fall from rest moves nothing in its
        // first update.
        owner.TurnAtTop();
        return AirStance.FallAndLand(owner, this);
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

    /// <summary>
    /// Left and right held together are read as the way behind by a player falling upright, who turns in the air, and
    /// as the way ahead by one falling in the low form, who falls on as they are. With up or down held as well the
    /// upright one reads them as the way ahead too, and aims on a slant the way they face; an aiming action of the
    /// game's own does not do that in a fall.
    /// </summary>
    public override int ReadsBothAcross(Player owner) =>
        owner.Form == Player.Crawling || owner.Input.IsHeld(GameAction.Up) || owner.Input.IsHeld(GameAction.Down) ? Forward(owner) : -Forward(owner);

    /// <summary>
    /// A fall with the arm held out level, or aiming straight down, reads up and down before the way behind: with
    /// either held, the way behind held with it steers nobody, so the player aims straight up or down the way they
    /// face and does not turn. In any other stance of a fall the way behind turns them whatever is held with it.
    /// </summary>
    public override bool PassesOverBehind(Player owner) => _aimsBeforeTurning;
}
