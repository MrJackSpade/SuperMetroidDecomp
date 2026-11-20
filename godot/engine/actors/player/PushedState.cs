using Engine.Actors.Components;
using Engine.Shared;

namespace Engine.Actors;

/// <summary>Pushed along at a set speed for a set time, out of the player's control, and still stopped by walls.</summary>
public sealed class PushedState : PlayerState
{
    /// <summary>The speed the player is pushed at.</summary>
    private readonly PixelVelocity _velocity;

    /// <summary>The updates left in the push.</summary>
    private Countdown _left;

    /// <summary>A push at <paramref name="velocity"/> lasting <paramref name="updates"/> updates.</summary>
    public PushedState(PixelVelocity velocity, int updates)
    {
        _velocity = velocity;
        _left = Countdown.Of(updates);
    }

    /// <inheritdoc/>
    public override bool CanAttack => false;

    /// <inheritdoc/>
    public override bool InControl => false;

    /// <inheritdoc/>
    public override State<Player> Step(Player owner)
    {
        owner.Velocity = _velocity;
        owner.MoveAcross();
        owner.MoveDown();
        _left = _left.Stepped();
        if (_left.IsRunning)
            return this;
        return owner.IsOnGround ? new StandingState() : new FallingState();
    }
}
