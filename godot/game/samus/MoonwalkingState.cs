using Engine.Actors;
using Engine.Input;
using Engine.Shared;
using Game.Data;

namespace Game;

/// <summary>
/// Moonwalking: walking slowly backwards while the attack is held, still facing and firing the way she was. A jump
/// turns her round, and the turn becomes a spin jump while the jump and the way she walked stay held.
/// </summary>
public sealed class MoonwalkingState : PlayerState, ISamusLook
{
    /// <summary>The name of the move values she moonwalks by.</summary>
    public const string Moves = "moonwalking";

    /// <inheritdoc/>
    public string Look => "moonwalking";

    /// <inheritdoc/>
    public override void Enter(Player owner) => owner.TrySetForm(Player.Standing);

    /// <summary>The way she walks: against the way she faces.</summary>
    private static int Backward(Player owner) => owner.Facing == Facing.Right ? -1 : 1;

    /// <inheritdoc/>
    public override PlayerState? Asked(Player owner)
    {
        owner.Aim = owner.Input.IsHeld(Actions.AngleUp) ? Aim.DiagonalUp
            : owner.Input.IsHeld(Actions.AngleDown) ? Aim.DiagonalDown : Aim.Forward;
        if (owner.MayCrouch && owner.Input.WasPressed(GameAction.Down))
            return new CrouchingState();
        if (owner.JumpPressed)
            return new TurningState();
        int backward = Backward(owner);
        if (owner.Input.IsHeld(GameAction.Attack) && owner.Intent == backward)
            return this;

        // With the attack let go she turns to go the way she walked; steering the way she faces, she runs.
        if (owner.Intent == backward)
            return new TurningState();
        return owner.Intent == 0 ? null : new RunningState();
    }

    /// <summary>With everything let go, she stands.</summary>
    public override PlayerState? Unasked(Player owner) => new StandingState();

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        // She gains speed backwards up to the moonwalk's top speed, without turning to face the way she goes.
        owner.StepDash(building: false);
        owner.Steer(owner.Moves(Moves), heading: owner.Facing.Opposite());
        owner.MoveAcross();
        if (owner.IsOnGround)
            return null;

        // Walking off a floor, she falls facing the way she was going.
        owner.Facing = owner.Facing.Opposite();
        return new FallingState();
    }
}
