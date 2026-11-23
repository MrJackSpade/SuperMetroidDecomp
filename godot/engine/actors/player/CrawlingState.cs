using Engine.Actors.Components;
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

    /// <inheritdoc/>
    public override bool CanAttack => true;

    /// <inheritdoc/>
    public override void Enter(Player owner) => owner.TrySetForm(Player.Crawling);

    /// <inheritdoc/>
    public override State<Player> Step(Player owner)
    {
        owner.Aim = Aim.Forward;
        bool grounded = owner.IsOnGround;
        if (owner.Input.WasPressed(GameAction.Up))
        {
            if (grounded && owner.TrySetForm(Player.Crouching))
                return new CrouchingState();
            if (!grounded && owner.TrySetForm(Player.Standing))
                return new FallingState();
        }

        if (grounded && owner.MayJumpWhileCrawling && owner.Input.WasPressed(GameAction.Jump))
        {
            owner.Velocity = owner.Velocity.WithY(-owner.LaunchSpeed(offWall: false));
            grounded = false;
        }
        else if (grounded && !owner.Velocity.Y.IsNegative)
        {
            owner.Velocity = owner.Velocity.WithY(Pixels.Zero);
        }

        if (!grounded && owner.MayJumpWhileCrawling)
            JumpingState.CutShort(owner);

        owner.StepDash(building: false);
        owner.Steer(owner.Moves(grounded ? CrawlingMoves : CrawlingInAirMoves));
        owner.MoveAcross();
        if (grounded)
        {
            _bounced = false;
            return this;
        }

        Pixels fallingAt = owner.Velocity.Y;
        MoveResult down = owner.MoveDown();
        owner.Fall();
        if (down.Blocked && !fallingAt.IsNegative)
            Land(owner, fallingAt);
        return this;
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
