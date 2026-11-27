using Engine.Actors;
using Engine.Collision;
using Engine.Shared;
using Game.Data;

namespace Game;

/// <summary>
/// Riding an elevator: facing forward, out of the player's hands, held above the elevator as it takes its turn. Her
/// own movement is a step down of one pixel on each update the ride moves her, which the terrain under her feet
/// stops. While the ride is leaving, that step is what meets the door of the shaft and the blocks the elevator rests
/// on; otherwise she is carried past both. She meets no exit by overlapping it.
/// </summary>
public sealed class RidingElevatorState : PlayerState, ISamusLook
{
    /// <summary>
    /// She rides in the body she boarded with: she is shown facing forward, but the cartridge takes up a pose's size
    /// only where it reads her input, which a ride does not, so her step down feels for the blocks under the feet
    /// she had.
    /// </summary>
    public override FormAsked? EntersIn(Player owner) => null;

    /// <inheritdoc/>
    public string Look => "facing_forward";

    /// <inheritdoc/>
    public override bool CanAttack => false;

    /// <inheritdoc/>
    public override bool InControl => false;

    /// <inheritdoc/>
    public override bool InAnothersHands => true;

    /// <inheritdoc/>
    public override bool MeetsExits => false;

    /// <summary>The row under her feet her last step down was tried against; nothing on an update the ride did not move her.</summary>
    private PixelRect? _stepped;

    /// <summary>
    /// The shaft's triggers are met by her step down alone, the row under her feet, and by nothing on an update the
    /// ride does not move her, as all through a ride arriving upward.
    /// </summary>
    public override PixelRect? TriggerReach(Player owner) => _stepped;

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        owner.Velocity = PixelVelocity.Zero;
        // Her run's speed ends; her boost does not (Samus.OnMomentumLost), as the cartridge's setup leaves its count.
        owner.LoseMomentum();
        ((Samus)owner).TurnToFaceForward();
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        owner.Velocity = PixelVelocity.Zero;
        ElevatorJourney journey = ElevatorJourney.Of(owner.Room.Session);
        _stepped = null;
        if (!journey.MovesSamus)
            return null;

        // The step is tried against the row of pixels under her feet alone, and only the terrain stops it.
        PixelRect body = owner.BodyRect;
        var under = new PixelRect(body.Left, body.Bottom, body.Width, 1);
        _stepped = under;
        bool leaving = journey.Status == ElevatorJourney.Stage.Leaving;
        if (owner.Room.Mover.IsFree(under, owner, owner.BodyMoves with { StoppedBySolids = false }))
        {
            owner.Position = new PixelPosition(owner.Position.X, Pixels.FromWhole(owner.Position.Y.Whole + 1));
            if (leaving && owner.Room.Exits.OfType<Door>().FirstOrDefault(exit => exit.Area.Overlaps(under)) is { } door)
                owner.Room.EnterExit(door);
        }
        else
        {
            // Stopped by the terrain, she rests at the far end of her pixel, as any move down the terrain stops does.
            owner.Position = new PixelPosition(owner.Position.X, owner.Position.Y.WithFraction(Pixels.FractionsPerPixel - 1));

            // Stopped by the blocks the elevator rests on, she stands at its door again; in a later suit she does not.
            if (leaving && owner is Samus samus && !samus.Inventory.IsOn(Equipment.VariaSuit) && !samus.Inventory.IsOn(Equipment.GravitySuit))
                samus.NoteElevatorStand(under);
        }

        return null;
    }
}
