using Engine.Actors;
using Engine.Input;
using Engine.Shared;

namespace Game;

/// <summary>
/// A damage boost: holding the jump and the direction behind her in a knockback turns her that way and carries her
/// off, steered like a jump by the boost's own move values, with whatever speed the knockback had given her. It lasts
/// while the jump and that direction stay held; let go, she faces as she did and is in a plain jump. When the
/// knockback's time runs out her rise ends, and she flies on level and falls.
/// </summary>
public sealed class DamageBoostState : PlayerState, ISamusLook
{
    /// <summary>The name of the move values a damage boost goes by.</summary>
    public const string Moves = "damage_boost";

    private readonly Facing _toward;

    /// <summary>The updates of the knockback still to come.</summary>
    private Countdown _knockbackLeft;

    /// <summary>A damage boost carrying her <paramref name="toward"/>, out of a knockback with <paramref name="knockbackLeft"/> updates still to come.</summary>
    public DamageBoostState(Facing toward, int knockbackLeft)
    {
        _toward = toward;
        _knockbackLeft = Countdown.Of(knockbackLeft);
    }

    /// <inheritdoc/>
    public string Look => "damage_boost";

    /// <inheritdoc/>
    public override bool CanAttack => false;

    /// <inheritdoc/>
    public override bool HoldsOffInterruptions => true;

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        owner.Facing = _toward;
        owner.TrySetForm(owner.FormOr(Player.Jumping), aroundMiddle: true);
    }

    /// <summary>The boost lasts while the jump and the way it carries her are held.</summary>
    public override PlayerState? Asked(Player owner)
    {
        bool toward = owner.Input.IsHeld(_toward == Facing.Right ? GameAction.Right : GameAction.Left);
        return toward && owner.Input.IsHeld(GameAction.Jump) ? this : null;
    }

    /// <summary>Let go, she faces the way she did before the boost and jumps on plainly.</summary>
    public override PlayerState? Unasked(Player owner)
    {
        owner.Facing = _toward.Opposite();
        return new JumpingState(keepsSpeed: true);
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        owner.StepDash(building: false);
        JumpingState.CutShort(owner);
        owner.SteerAndMoveInAir(owner.Moves(Moves));
        PlayerState? landed = AirStance.FallAndLand(owner);

        // The knockback she left still runs out, and as it does her rise is over.
        if (landed is null && _knockbackLeft.IsRunning)
        {
            _knockbackLeft = _knockbackLeft.Stepped();
            if (_knockbackLeft.IsOver)
                owner.FallFromRest();
        }

        return landed;
    }
}
