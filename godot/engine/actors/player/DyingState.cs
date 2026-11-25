using Engine.Shared;

namespace Engine.Actors;

/// <summary>Dying: the player neither moves nor acts, and after a set time is dead.</summary>
public sealed class DyingState : PlayerState
{
    /// <summary>The updates left before the player is dead.</summary>
    private Countdown _left;

    /// <inheritdoc/>
    public override bool CanAttack => false;

    /// <inheritdoc/>
    public override bool InControl => false;

    /// <inheritdoc/>
    public override bool InAnothersHands => true;

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        owner.Velocity = PixelVelocity.Zero;
        _left = Countdown.Of(owner.DyingUpdates);
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        _left = _left.Stepped();
        if (_left.IsOver)
            owner.FinishDying();
        return null;
    }
}
