using Engine.Actors;
using Engine.Input;
using Engine.Shared;
using Game.Data;

namespace Game;

/// <summary>
/// Holding onto a wall with the grapple beam, as the cartridge has it: a swing stopped by her hand at one of the
/// angles its data names leaves her against the wall, in the wall's own pose. She stays while the attack is held
/// and the line holds. Let go, the pose and the line stay for a window of updates more, in each of which a fresh
/// press of the jump with a wall within the probe's reach on the side she faces is a wall jump, away from it. The
/// update that finds the window run out drops her, and feels for no wall.
/// </summary>
public sealed class GrappleWallGrabState : PlayerState, ISamusLook
{
    private readonly PixelPosition _at;

    /// <summary>The updates left of the window after she let go; below nothing while she still holds on.</summary>
    private int _window = -1;

    /// <summary>Against the wall at <paramref name="at"/>.</summary>
    public GrappleWallGrabState(PixelPosition at) => _at = at;

    /// <inheritdoc/>
    public string Look => "grapple_wall_contact";

    /// <inheritdoc/>
    public override bool CanAttack => false;

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        owner.LoseMomentum();
        owner.Velocity = PixelVelocity.Zero;
        owner.Aim = Aim.Forward;
        owner.Position = _at;
        ((Samus)owner).CapViewAfterPlacing();
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        var samus = (Samus)owner;
        SamusGrapple? grapple = samus.Grapple;
        if (_window < 0)
        {
            if (grapple is { IsLatched: true } && owner.Input.IsHeld(GameAction.Attack) && samus.Selected == Equipment.GrappleBeam)
                return null;

            // Let go, or the line lost: the window opens, and nothing else changes on this update.
            _window = grapple?.Swing.WallJumpWindow ?? 0;
            return null;
        }

        if (_window == 0)
        {
            // The window has run out: she is dropped, the line ending on the update after, her speeds cleared.
            grapple?.EndOnNextUpdate();
            owner.Velocity = PixelVelocity.Zero;
            return owner.StateAtRest();
        }

        _window--;
        int probe = grapple?.Swing.WallJumpProbe ?? 0;
        if (owner.IsWithinReachOfWall(owner.Facing, probe) && owner.JumpPressed)
        {
            // She jumps away from the wall she faced: facing right, to the left, and any other way to the right.
            grapple?.EndOnNextUpdate();
            owner.Facing = owner.Facing == Facing.Right ? Facing.Left : Facing.Right;
            owner.ComeToRest();
            return new WallJumpingState();
        }

        return null;
    }
}
