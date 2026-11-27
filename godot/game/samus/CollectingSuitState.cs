using Engine.Actors;
using Engine.Shared;

namespace Game;

/// <summary>
/// Collecting a suit: she is put at a fixed place of the screen and held there, bathed in light, while the suit
/// is put on, and then let go from there. The suit is first seen on her part way through, on the update the light
/// shows it. Nothing can hurt her meanwhile, and everything else in the room waits.
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

    /// <summary>
    /// The front view's body, taken with no feeling for room and hers at once, as the cartridge sets the front pose and
    /// its size outright as the pickup begins: she is then put at her place of the screen.
    /// </summary>
    public override FormAsked? EntersIn(Player owner) => new FormAsked(RidingElevatorState.Form, Unfitted: true, AtOnce: true);

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        owner.StopAcross();
        owner.Velocity = PixelVelocity.Zero;

        // She is put at a fixed place of the screen, by her whole pixels alone, and the view is told she stood there:
        // it follows none of the move.
        Engine.World.Camera view = owner.Room.Session.Camera;
        int across = view.View.Left + ScreenPlace.X, down = view.View.Top + ScreenPlace.Y;
        owner.Position = new PixelPosition(owner.Position.X.WithWhole(across), owner.Position.Y.WithWhole(down));
        view.TakePlayerToHaveStoodAtX(across);
        view.TakePlayerToHaveStoodAtY(down);

        // The suit's own sound is asked for as she is put there.
        owner.Room.Session.Sounds.Request(Game.Data.Sounds.SuitAcquired, channel: "samus");
        var samus = (Samus)owner;
        _left = Countdown.Of(samus.SuitPickupUpdates);
        samus.SuitLight?.Begin(samus.SuitBeingPutOn);
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        var samus = (Samus)owner;
        _left = _left.Stepped();
        samus.SuitLight?.Step();
        if (samus.SuitPickupUpdates - _left.Remaining == samus.SuitShownOnUpdate)
            samus.ShowSuitPutOn();
        return _left.IsRunning ? null : owner.StateAtRest();
    }

    /// <summary>However she leaves it, the suit is seen on her from then.</summary>
    public override void Exit(Player owner) => ((Samus)owner).ShowSuitPutOn();
}
