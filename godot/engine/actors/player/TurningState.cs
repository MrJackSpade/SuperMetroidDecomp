using Engine.Actors.Components;
using Engine.Shared;

namespace Engine.Actors;

/// <summary>Turning around on the ground: a short pause before facing the other way.</summary>
public sealed class TurningState : PlayerState
{
    /// <summary>The updates left before the turn is done.</summary>
    private Countdown _left;

    /// <inheritdoc/>
    public override void Enter(Player owner) => _left = Countdown.Of(owner.TurnUpdates);

    /// <inheritdoc/>
    public override State<Player> Step(Player owner)
    {
        if (!owner.IsOnGround)
            return new FallingState();
        owner.SlowToRest();
        owner.MoveAcross();
        _left = _left.Stepped();
        if (_left.IsRunning)
            return this;
        owner.Facing = owner.Facing.Opposite();
        return owner.Intent != 0 ? new RunningState() : new StandingState();
    }
}
