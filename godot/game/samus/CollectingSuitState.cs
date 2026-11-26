using Engine.Actors;
using Engine.Shared;

namespace Game;

/// <summary>
/// Collecting a suit: she is put at a fixed place of the screen and held there, bathed in light, while the suit
/// becomes hers, and then let go from there. Nothing can hurt her meanwhile, and everything else in the room waits.
/// </summary>
public sealed class CollectingSuitState : PlayerState, IUntouchable, ISamusLook
{
    /// <summary>Where on the screen she is put, in pixels from the view's corner.</summary>
    private static readonly Godot.Vector2I ScreenPlace = new(120, 136);

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

        // She is put at a fixed place of the screen, by her whole pixels alone, and the view is told she stood there:
        // it follows none of the move.
        Engine.World.Camera view = owner.Room.Session.Camera;
        int across = view.View.Left + ScreenPlace.X, down = view.View.Top + ScreenPlace.Y;
        owner.Position = new PixelPosition(owner.Position.X.WithWhole(across), owner.Position.Y.WithWhole(down));
        view.TakePlayerToHaveStoodAtX(across);
        view.TakePlayerToHaveStoodAtY(down);

        // The suit's own sound is asked for as she is put there.
        owner.Room.Session.Sounds.Request(Game.Data.Sounds.SuitAcquired, channel: "samus");
        _left = Countdown.Of(((Samus)owner).SuitPickupUpdates);
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        _left = _left.Stepped();
        return _left.IsRunning ? null : owner.StateAtRest();
    }
}
