using Engine.Actors;
using Engine.Input;
using Engine.Shared;

namespace Game;

/// <summary>
/// A damage boost: holding the jump and the direction behind her in a knockback turns her that way and carries her
/// off, steered like a jump by the boost's own move values, with whatever speed the knockback had given her. It lasts
/// while the jump and that direction stay held; let go, she faces as she did and is in a plain jump. When the
/// knockback's time runs out her rise ends, and she flies on level and falls. If she has left the boost by then, for
/// a jump or the ground, the knockback still runs out, and ends whatever rise she is then in.
/// </summary>
/// <remarks>
/// What the input asks of a boost is the first of these that holds, as the cartridge has it: the jump with left, then
/// the jump with right, each keeping the boost when it is the way the boost carries her and otherwise leaving it for a
/// jump in the stance of steering the way she faced; anything else leaves it for a jump with her arm at rest.
/// </remarks>
public sealed class DamageBoostState : PlayerState, ISamusLook
{
    /// <summary>The name of the move values a damage boost goes by.</summary>
    public const string Moves = "damage_boost";

    private readonly Facing _toward;

    /// <summary>The updates of the knockback still to come.</summary>
    private Countdown _knockbackLeft;

    /// <summary>Whether the knockback she left for the boost still has time to run.</summary>
    public bool KnockbackRuns => _knockbackLeft.IsRunning;

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

    /// <summary>
    /// The jump's body, taken where she is with no feeling for room and hers at once, as the cartridge sets the boost's
    /// pose and its size outright out of the knockback's.
    /// </summary>
    public override FormAsked? EntersIn(Player owner) => new FormAsked(owner.FormOr(Player.Jumping), Unfitted: true, AtOnce: true);

    /// <inheritdoc/>
    public override void Enter(Player owner) => owner.Facing = _toward;

    /// <summary>The boost lasts while the jump and the way it carries her are the first of its chords held.</summary>
    public override PlayerState? Asked(Player owner) => FirstChord(owner) == _toward ? this : null;

    /// <summary>
    /// Let go, she faces the way she did before the boost and jumps on, aiming level: steering that way where the
    /// jump was held with it, and otherwise with her arm at rest.
    /// </summary>
    public override PlayerState? Unasked(Player owner)
    {
        Facing before = _toward.Opposite();
        owner.Facing = before;
        ((Samus)owner).CarryKnockbackOn(_knockbackLeft);
        return new JumpingState(new AirStance(Aim.Forward, FirstChord(owner) == before ? AirArm.Forward : AirArm.AtRest));
    }

    /// <summary>The direction of the first chord held, the jump with left before the jump with right; null when neither is.</summary>
    private static Facing? FirstChord(Player owner)
    {
        if (!owner.Input.IsHeld(GameAction.Jump))
            return null;
        return owner.Input.IsHeld(GameAction.Left) ? Facing.Left : owner.Input.IsHeld(GameAction.Right) ? Facing.Right : null;
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        owner.StepDash(building: false);
        JumpingState.CutShort(owner);
        owner.SteerAndMoveInAir(owner.Moves(Moves));
        // Met with a ceiling she goes on in the boost; landed, she is out of it.
        PlayerState? met = AirStance.FallAndLand(owner, this);
        bool landed = met is not null && !ReferenceEquals(met, this);

        // The knockback she left still runs out, and as it does her rise is over. Landed, she takes what is left of
        // it with her: it ends the rise of a jump she takes before it has run out.
        if (landed)
        {
            ((Samus)owner).CarryKnockbackOn(_knockbackLeft);
        }
        else if (_knockbackLeft.IsRunning)
        {
            _knockbackLeft = _knockbackLeft.Stepped();
            if (_knockbackLeft.IsOver)
                owner.FallFromRest();
        }

        return met;
    }
}
