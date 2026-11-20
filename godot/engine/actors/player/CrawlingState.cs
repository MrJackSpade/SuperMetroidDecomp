using Engine.Actors.Components;
using Engine.Input;
using Engine.Shared;

namespace Engine.Actors;

/// <summary>
/// The low form: a small body that fits low gaps, moving on the ground and through the air. Up changes back to a
/// crouch where there is room. Where the game allows it, the low form can jump.
/// </summary>
public sealed class CrawlingState : PlayerState
{
    /// <inheritdoc/>
    public override bool CanAttack => true;

    /// <inheritdoc/>
    public override void Enter(Player owner) => owner.TrySetForm(BodyForm.Crawling);

    /// <inheritdoc/>
    public override State<Player> Step(Player owner)
    {
        owner.Aim = Aim.Forward;
        bool grounded = owner.IsOnGround;
        if (grounded && owner.Input.WasPressed(GameAction.Up) && owner.TrySetForm(BodyForm.Crouching))
            return new CrouchingState();
        if (grounded && owner.CanJumpWhileCrawling && owner.Input.WasPressed(GameAction.Jump))
            owner.Velocity = owner.Velocity.WithY(-Pixels.FromNumber(owner.CrawlJumpSpeed));
        else if (!grounded)
            owner.Fall();
        else if (!owner.Velocity.Y.IsNegative)
            owner.Velocity = owner.Velocity.WithY(Pixels.Zero);

        owner.Steer(grounded ? owner.RunAcceleration : owner.AirAcceleration, owner.CrawlSpeed);
        owner.MoveAcross();
        owner.MoveDown();
        return this;
    }
}
