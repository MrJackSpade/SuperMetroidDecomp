using Engine.Actors;
using Engine.Input;
using Engine.Shared;
using Game.Data;

namespace Game;

/// <summary>
/// Moonwalking: walking slowly backwards while the attack is held, still facing and firing the way she was. Jumping
/// turns her round into a spin jump the way she was walking.
/// </summary>
public sealed class MoonwalkingState : PlayerState, ISamusLook
{
    /// <summary>The name of the move values she moonwalks by.</summary>
    public const string Moves = "moonwalking";

    /// <inheritdoc/>
    public string Look => "moonwalking";

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        owner.TrySetForm(Player.Standing);
        owner.LoseMomentum();
    }

    /// <inheritdoc/>
    public override PlayerState? Asked(Player owner)
    {
        if (owner.Input.WasPressed(GameAction.Jump))
            return new SpinJumpingState(owner.Facing.Opposite());
        return owner.Input.IsHeld(GameAction.Attack) && owner.Intent == Backward(owner) ? this : null;
    }

    /// <summary>The way she walks: against the way she faces.</summary>
    private static int Backward(Player owner) => owner.Facing == Facing.Right ? -1 : 1;

    /// <summary>With the attack or the direction let go, she stops and stands.</summary>
    public override PlayerState? Unasked(Player owner)
    {
        owner.Velocity = owner.Velocity.WithX(Pixels.Zero);
        return new StandingState();
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        // She gains speed backwards up to the moonwalk's top speed, without turning to face the way she goes.
        int backward = Backward(owner);
        MoveValues moves = owner.Moves(Moves);
        Pixels faster = owner.Velocity.X.Abs() + moves.Gain;
        owner.Velocity = owner.Velocity.WithX((faster > moves.Top ? moves.Top : faster) * backward);
        owner.Aim = owner.Input.IsHeld(Actions.AngleUp) ? Aim.DiagonalUp
            : owner.Input.IsHeld(Actions.AngleDown) ? Aim.DiagonalDown : Aim.Forward;
        owner.MoveAcross();
        return owner.IsOnGround ? null : new FallingState();
    }
}
