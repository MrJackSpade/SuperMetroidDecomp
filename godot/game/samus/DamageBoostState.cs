using Engine.Actors;
using Engine.Collision;
using Engine.Shared;

namespace Game;

/// <summary>
/// A damage boost: jumping out of a knockback carries her far and fast the way she steers, with the height the
/// knockback gave her, until she lands.
/// </summary>
public sealed class DamageBoostState : PlayerState, ISamusLook
{
    /// <summary>The name of the move values a damage boost goes by.</summary>
    public const string Moves = "damage_boost";

    private readonly Facing _toward;

    /// <summary>A damage boost carrying her <paramref name="toward"/>.</summary>
    public DamageBoostState(Facing toward) => _toward = toward;

    /// <inheritdoc/>
    public string Look => "damage_boost";

    /// <inheritdoc/>
    public override bool CanAttack => false;

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        owner.Facing = _toward;
        owner.TrySetForm(owner.FormOr(Player.Jumping), aroundMiddle: true);
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        JumpingState.CutShort(owner);
        owner.StepDash(building: false);
        owner.Steer(owner.Moves(Moves));
        owner.MoveAcross();
        bool falling = !owner.Velocity.Y.IsNegative;
        MoveResult down = owner.MoveDown();
        owner.Fall();
        if ((down.Blocked && falling) || owner.IsOnGround)
            return owner.Intent != 0 ? new RunningState() : new StandingState();
        return null;
    }
}
