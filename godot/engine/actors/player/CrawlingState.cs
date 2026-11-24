using Engine.Collision;
using Engine.Input;
using Engine.Shared;

namespace Engine.Actors;

/// <summary>
/// The low form: a small body that fits low gaps, moving on the ground and through the air. Up changes back to a
/// crouch on the ground, or to a fall in the air, where there is room. Where the game allows it, the low form can
/// jump, and it bounces once on landing from a fast fall when the game gives it a bounce.
/// </summary>
public sealed class CrawlingState : PlayerState
{
    /// <summary>Whether the bounce of this fall has been spent.</summary>
    private bool _bounced;

    /// <summary>Whether this update's input asked for a jump from the ground.</summary>
    private bool _jumpAsked;

    /// <inheritdoc/>
    public override bool CanAttack => true;

    /// <inheritdoc/>
    public override void Enter(Player owner) => owner.TrySetForm(Player.Crawling);

    /// <summary>Whether the low form is on the ground and not leaving it.</summary>
    private static bool IsGrounded(Player owner) => owner.IsOnGround && !owner.Velocity.Y.IsNegative;

    /// <inheritdoc/>
    public override PlayerState? Asked(Player owner)
    {
        owner.Aim = Aim.Forward;
        bool grounded = IsGrounded(owner);
        _jumpAsked = false;
        if (owner.Input.WasPressed(GameAction.Up))
        {
            if (grounded && owner.CanTakeForm(Player.Crouching))
                return new CrouchingState();
            if (!grounded && owner.CanTakeForm(owner.FormOr(Player.Falling), aroundMiddle: true))
                return new FallingState();
        }

        _jumpAsked = grounded && owner.MayJumpWhileCrawling && owner.JumpPressed;
        return this;
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        bool grounded = IsGrounded(owner);
        if (grounded)
            owner.Velocity = owner.Velocity.WithY(Pixels.Zero);
        else if (owner.MayJumpWhileCrawling)
            JumpingState.CutShort(owner);
        else
            owner.TurnAtTop();

        owner.StepDash(building: false);
        owner.SteerPlainly(owner.Moves(grounded ? CrawlingMoves : CrawlingInAirMoves));
        owner.MoveAcross();
        if (grounded)
        {
            _bounced = false;

            // A jump asked for leaves the ground on the next update, if the ground is still there.
            if (_jumpAsked && owner.IsOnGround)
                owner.Velocity = owner.Velocity.WithY(-owner.LaunchSpeed(offWall: false));
            return null;
        }

        Pixels fallingAt = owner.Velocity.Y;
        MoveResult down = owner.MoveDown();
        owner.Fall();
        if (down.Blocked && !fallingAt.IsNegative)
            Land(owner, fallingAt);
        return null;
    }

    /// <summary>Lands the low form: it bounces once if it fell fast enough and the game gives it a bounce.</summary>
    private void Land(Player owner, Pixels fallingAt)
    {
        if (_bounced || owner.CrawlBounceSpeed <= 0 || fallingAt.Whole < owner.CrawlBounceFallSpeed)
        {
            _bounced = false;
            return;
        }

        _bounced = true;
        owner.Velocity = owner.Velocity.WithY(-Pixels.FromNumber(owner.CrawlBounceSpeed));
    }
}
