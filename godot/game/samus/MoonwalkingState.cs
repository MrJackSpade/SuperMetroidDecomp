using Engine.Actors;
using Engine.Input;
using Engine.Shared;
using Game.Data;

namespace Game;

/// <summary>
/// Moonwalking: walking slowly backwards while the attack is held, still facing and firing the way she was. A jump
/// turns her round, and the turn becomes a spin jump while the jump and the way she walked stay held; a turn made
/// with the jump that runs out leaves her adrift in a spin with no jump begun.
/// </summary>
public sealed class MoonwalkingState : PlayerState, ISamusLook
{
    /// <summary>The name of the move values she moonwalks by.</summary>
    public const string Moves = "moonwalking";

    /// <inheritdoc/>
    public string Look => "moonwalking";

    /// <inheritdoc/>
    public override FormAsked? EntersIn(Player owner) => new FormAsked(Player.Standing);

    /// <summary>The way she walks: against the way she faces.</summary>
    private static int Backward(Player owner) => owner.Facing == Facing.Right ? -1 : 1;

    /// <inheritdoc/>
    public override PlayerState? Asked(Player owner)
    {
        owner.Aim = AimOf(owner, angleDownFirst: owner.Facing == Facing.Left);
        if (owner.MayCrouch && owner.Input.WasPressed(GameAction.Down))
            return owner.Crouch();
        if (owner.JumpPressed)
            return TurnWithJump();
        int backward = Backward(owner);
        if (owner.Input.IsHeld(GameAction.Attack) && owner.Intent == backward)
            return this;

        // With the attack let go she turns to go the way she walked, with the jump if it is held; steering the way
        // she faces, she runs.
        if (owner.Intent == backward)
            return owner.Input.IsHeld(GameAction.Jump) ? TurnWithJump() : new TurningState();
        return owner.Intent == 0 ? null : new RunningState();
    }

    /// <summary>
    /// Her aim in a moonwalk: diagonally as the angle actions ask, level with neither. With both held the one tested
    /// first wins: as the cartridge has it, angle-down as a moonwalk is taken up and while she faces left, angle-up
    /// while she faces right.
    /// </summary>
    public static Aim AimOf(Player owner, bool angleDownFirst)
    {
        bool angleUp = owner.Input.IsHeld(Actions.AngleUp), angleDown = owner.Input.IsHeld(Actions.AngleDown);
        if (angleUp && angleDown)
            return angleDownFirst ? Aim.DiagonalDown : Aim.DiagonalUp;
        return angleUp ? Aim.DiagonalUp : angleDown ? Aim.DiagonalDown : Aim.Forward;
    }

    /// <summary>
    /// The turn a moonwalk makes with the jump: a turn on the ground that the jump, pressed afresh or held with the
    /// way she then faces, makes a real jump of while it lasts, and that otherwise runs out into a spin with no jump
    /// begun, as the cartridge's does. She is then adrift in the spin's smaller body, drawn down with no fastest fall.
    /// </summary>
    private static TurningState TurnWithJump() => new(new SpinJumpingState(SpinJumpingState.Start.Adrift));

    /// <summary>With everything let go, she stands.</summary>
    public override PlayerState? Unasked(Player owner) => new StandingState();

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        // She gains speed backwards up to the moonwalk's top speed, without turning to face the way she goes.
        owner.StepDash(building: false);
        owner.Steer(owner.Moves(Moves), heading: owner.Facing.Opposite());
        owner.MoveAcross();
        if (owner.PressToGround())
            return null;

        // Walking off a floor, she falls facing the way she was going.
        owner.Facing = owner.Facing.Opposite();
        return new FallingState(offFloor: true);
    }

    /// <summary>
    /// Left and right held together are read as the way behind while the attack is held, so she walks on backwards,
    /// and as the way ahead without it, which sets her running.
    /// </summary>
    public override int ReadsBothAcross(Player owner) => owner.Input.IsHeld(GameAction.Attack) ? -Forward(owner) : Forward(owner);
}
