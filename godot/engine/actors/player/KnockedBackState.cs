using Engine.Actors.Components;
using Engine.Shared;

namespace Engine.Actors;

/// <summary>Knocked back by a hit: thrown up and away for a moment, out of the player's control.</summary>
public sealed class KnockedBackState : PlayerState
{
    /// <summary>The way the player is thrown.</summary>
    private readonly Facing _toward;

    /// <summary>The updates left before the player has control again.</summary>
    private Countdown _left;

    /// <summary>A knockback throwing the player <paramref name="toward"/>.</summary>
    public KnockedBackState(Facing toward) => _toward = toward;

    /// <inheritdoc/>
    public override bool CanAttack => false;

    /// <inheritdoc/>
    public override bool InControl => false;

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        owner.Velocity = new PixelVelocity(
            _toward.Toward(Pixels.FromNumber(owner.KnockbackAcrossSpeed)), -Pixels.FromNumber(owner.KnockbackUpSpeed));
        _left = Countdown.Of(owner.KnockbackUpdates);
    }

    /// <inheritdoc/>
    public override State<Player> Step(Player owner)
    {
        owner.MoveAcross();
        owner.MoveDown();
        owner.Fall();
        _left = _left.Stepped();
        if (_left.IsRunning)
            return this;
        return owner.IsOnGround ? new StandingState() : new FallingState();
    }
}
