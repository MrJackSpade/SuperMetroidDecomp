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
/// update that finds the window run out drops her, and feels for no wall. A wall jump or a drop is made on the update
/// after it is decided, as the line ends; she does not move meanwhile. Dropped, she is crouched aiming diagonally
/// down, as the wall's pose is.
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

    /// <summary>
    /// A knockback waits while she is in this state, the window after she lets go included, as the cartridge refuses
    /// it in the wall's pose and tries again each update of its time.
    /// </summary>
    public override bool HoldsOffInterruptions => true;

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

        // What the update before decided is done on this one, as the cartridge's teardown is, and she does not move
        // meanwhile.
        switch (_ending)
        {
            case Ending.Drop:
                // Dropped from the wall's pose, which is compact and aims diagonally down, she is crouched so aiming,
                // her speeds cleared.
                owner.Velocity = PixelVelocity.Zero;
                owner.Aim = Aim.DiagonalDown;
                return new CrouchingState();
            case Ending.WallJump:
                // She jumps away from the wall she faced: facing right, to the left, and any other way to the right.
                // The jump lets go of a charge she held.
                owner.Facing = owner.Facing == Facing.Right ? Facing.Left : Facing.Right;
                owner.ComeToRest();
                samus.ForgetCharge();
                return new WallJumpingState();
        }

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
            // The window has run out: she is dropped on the update after, when the line ends.
            grapple?.EndOnNextUpdate();
            _ending = Ending.Drop;
            return null;
        }

        _window--;
        int probe = grapple?.Swing.WallJumpProbe ?? 0;
        if (owner.IsWithinReachOfWall(owner.Facing, probe) && owner.JumpPressed)
        {
            // A wall jump, made on the update after, when the line ends.
            grapple?.EndOnNextUpdate();
            _ending = Ending.WallJump;
        }

        return null;
    }

    /// <summary>What the hold on the wall ends in on the update after it is decided.</summary>
    private enum Ending
    {
        /// <summary>Nothing is decided yet.</summary>
        None,

        /// <summary>The window ran out: she is dropped.</summary>
        Drop,

        /// <summary>She jumps off the wall.</summary>
        WallJump,
    }

    private Ending _ending;
}
