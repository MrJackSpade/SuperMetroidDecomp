using Engine.Actors;
using Engine.Input;
using Engine.Shared;
using Game.Data;

namespace Game;

/// <summary>
/// Stopped against a wall she ran into, as the cartridge's wall-stop pose: she moves as she does standing, and her
/// input is asked in an order of its own. A jump from it is always a plain one, whatever is held. Forward with up
/// or down runs again aiming that way; down pressed crouches; back with an angle action walks her backwards aiming
/// that way, or turns her where walking backwards is switched off; up or an angle action alone stands her aiming;
/// back turns her; forward runs. Anything else leaves her as she is.
/// </summary>
public sealed class WallStoppedState : PlayerState
{
    /// <inheritdoc/>
    public override FormAsked? EntersIn(Player owner) => new FormAsked(Player.Standing);

    /// <inheritdoc/>
    public override PlayerState? Asked(Player owner)
    {
        GameInput input = owner.Input;
        bool right = owner.Facing == Facing.Right;
        bool forward = input.IsHeld(right ? GameAction.Right : GameAction.Left);
        bool back = input.IsHeld(right ? GameAction.Left : GameAction.Right);
        bool angleUp = input.IsHeld(Actions.AngleUp), angleDown = input.IsHeld(Actions.AngleDown);

        if (owner.JumpPressed)
            return new JumpingState();
        if (forward && input.IsHeld(GameAction.Up))
            return Aiming(owner, Aim.DiagonalUp, new RunningState());
        if (forward && input.IsHeld(GameAction.Down))
            return Aiming(owner, Aim.DiagonalDown, new RunningState());
        if (input.WasPressed(GameAction.Down) && owner.MayCrouch)
            return owner.Crouch();
        if (back && (angleDown || angleUp))
        {
            // Walking backwards switched off, the chord turns her as back alone does.
            if (!owner.Room.Session.Options.IsOn(Toggles.Moonwalk))
                return new TurningState();
            return Aiming(owner, angleDown ? Aim.DiagonalDown : Aim.DiagonalUp, new MoonwalkingState());
        }

        if (input.IsHeld(GameAction.Up))
            return Aiming(owner, Aim.Up, new StandingState());
        if (angleUp)
            return Aiming(owner, Aim.DiagonalUp, new StandingState());
        if (angleDown)
            return Aiming(owner, Aim.DiagonalDown, new StandingState());
        if (back)
            return new TurningState();
        return forward ? new RunningState() : this;
    }

    private static PlayerState Aiming(Player owner, Aim aim, PlayerState state)
    {
        owner.Aim = aim;
        return state;
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        owner.ComeToRest();
        owner.MoveAcross();
        bool grounded = owner.PressToGround();
        owner.StopAcross();
        return grounded ? null : new FallingState(offFloor: true);
    }

    /// <summary>Left and right held together are read as the way behind: she turns from the wall.</summary>
    public override int ReadsBothAcross(Player owner) => -Forward(owner);
}
