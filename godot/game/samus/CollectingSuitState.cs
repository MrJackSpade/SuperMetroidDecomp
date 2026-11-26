using Engine.Actors;
using Engine.Shared;

namespace Game;

/// <summary>
/// Collecting a suit: she is held where she is, bathed in light, while the suit becomes hers, and then let go.
/// Nothing can hurt her meanwhile, and everything else in the room waits.
/// </summary>
public sealed class CollectingSuitState : PlayerState, IUntouchable, ISamusLook
{
    private Countdown _left;

    /// <inheritdoc/>
    public string Look => "collecting_suit";

    /// <inheritdoc/>
    public override bool CanAttack => false;

    /// <inheritdoc/>
    public override bool InControl => false;

    /// <inheritdoc/>
    public override bool InAnothersHands => true;

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        owner.StopAcross();
        owner.Velocity = PixelVelocity.Zero;
        owner.TrySetForm(Player.Standing);
        _left = Countdown.Of(((Samus)owner).SuitPickupUpdates);
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        _left = _left.Stepped();
        return _left.IsRunning ? null : owner.StateAtRest();
    }
}
