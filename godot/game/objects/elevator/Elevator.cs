using Engine.Actors;
using Engine.Actors.Components;
using Engine.Content;
using Engine.Core;
using Engine.Input;
using Engine.Shared;
using Engine.World;
using Game.Data;
using Godot;

namespace Game;

/// <summary>
/// An elevator, update for update as the cartridge's. Samus arms it by standing on the blocks it rests on; a press
/// of its way on its next turn starts the ride. It then moves each turn with Samus held above it until her own step
/// down meets the door of its shaft. In the room the ride reaches, the elevator there starts from where rides arrive,
/// with Samus on it, and comes back to rest before she is let go, still facing forward.
/// </summary>
/// <remarks>
/// It takes its turn among the enemies, and holds still through a door's passage. The ride itself is the session's
/// (<see cref="ElevatorJourney"/>). Its data names the timeline that shows it: "rest".
/// </remarks>
public partial class Elevator : RoomObject
{
    /// <summary>Whether it takes Samus down; otherwise up.</summary>
    [Export]
    public bool GoesDown { get; set; }

    /// <summary>The height in the room at which it stands as a ride arrives, in pixels.</summary>
    [Export]
    public int ArrivesAtY { get; set; }

    /// <summary>The blocks it rests on, which Samus stands on to arm it: their rectangle in the room, in pixels.</summary>
    [Export]
    public Rect2I RestsOn { get; set; }

    /// <summary>How far it moves each update of a ride, as a number of pixels.</summary>
    [Export]
    public double Speed { get; set; } = 1.5;

    /// <summary>How far above it Samus's middle is held, in pixels.</summary>
    [Export]
    public int HoldsSamusAbove { get; set; } = 26;

    /// <summary>The sound of it setting off, by name.</summary>
    [Export]
    public string DepartureSound { get; set; } = "";

    /// <summary>The sound asked for as it comes to rest, by name.</summary>
    [Export]
    public string RestSound { get; set; } = "";

    private int _restY;
    private TimelineSet _timelines = TimelineSet.None;
    private TimelinePlayer? _showing;

    /// <inheritdoc/>
    public override bool ActsWithEnemies => true;

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _timelines = TimelineSet.ReadFrom(data);
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _restY = Position.Y.Whole;
        _showing = null;
        ElevatorJourney journey = ElevatorJourney.Of(Room.Session);
        if (journey.Status == ElevatorJourney.Stage.Arriving)
        {
            // A ride is arriving: it starts from where rides arrive, with Samus on it.
            Position = new PixelPosition(Position.X, Position.Y.WithWhole(ArrivesAtY));
            Hold(Room.Player);
            journey.SetDownAt(Room.Player.Position);
        }
        else
        {
            // Any other way into its room leaves no ride and no armed door.
            journey.End();
        }

        ShowLook();
    }

    /// <summary>
    /// Whether a step down stopped over <paramref name="under"/>, the row of pixels beneath Samus's feet, met the
    /// blocks it rests on first: her blocks are tried from the left or from the right, and the first one met is the
    /// one she stands on.
    /// </summary>
    public bool IsStoodOn(PixelRect under, bool fromLeft)
    {
        if (under.Top < RestsOn.Position.Y || under.Top >= RestsOn.End.Y)
            return false;
        int first = fromLeft ? under.Left : under.Right - 1;
        int block = first - (((first % Block) + Block) % Block);
        return block >= RestsOn.Position.X && block < RestsOn.End.X;
    }

    private const int Block = 16;

    /// <inheritdoc/>
    protected override void OnStep()
    {
        ShowLook();
        GameSession session = Room.Session;
        ElevatorJourney journey = ElevatorJourney.Of(session);
        Player samus = Room.Player;
        var door = session.Mode as DoorTransitionMode;

        // A stand she made at its door as she last moved reaches it on its next turn of play.
        if (door is null)
            journey.TakeStand();

        // It holds still through a door's passage, and while there is no ride and its door is not armed.
        if (door is { PassageBegun: true } || (journey.Status == ElevatorJourney.Stage.None && !journey.DoorArmed))
            return;
        switch (journey.Status)
        {
            case ElevatorJourney.Stage.None:
                WaitForPress(journey, samus);
                break;

            case ElevatorJourney.Stage.Leaving:
                MoveBy(journey.GoesDown ? Speed : -Speed);
                Hold(samus);
                break;

            case ElevatorJourney.Stage.Arriving:
                journey.BeginReturn();
                ReturnToRest(journey, samus);
                break;

            case ElevatorJourney.Stage.Returning:
                ReturnToRest(journey, samus);
                break;

            default:
                throw new InvalidOperationException($"Undefined stage {journey.Status} of an elevator's ride.");
        }
    }

    /// <summary>Its door armed, it starts the ride on a fresh press of its way; without one the door is disarmed.</summary>
    private void WaitForPress(ElevatorJourney journey, Player samus)
    {
        if (!samus.Input.WasPressed(GoesDown ? GameAction.Down : GameAction.Up))
        {
            journey.Disarm();
            return;
        }

        Sound(DepartureSound, "objects");
        Sound(Sounds.SpinJumpStop, "samus");
        samus.ChangeState(new RidingElevatorState());

        // Every shot and bomb of hers is gone, and her charge with them, and the wait her shots and bombs share is over.
        // Each of her shot places keeps the part of a pixel of what held it, as when a room is left.
        (samus as Samus)?.KeepWhatShotPlacesHold();
        foreach (Projectile shot in Room.Projectiles.Where(projectile => projectile.Side == HitSide.Player).ToList())
            Room.Remove(shot);
        (samus as Samus)?.ForgetCharge();
        // A shot's glow of her suit ends where it stands, and a full charge's pulse starts over.
        (samus as Samus)?.EndShotGlow();
        samus.Weapons.SetWait(0);
        Hold(samus);
        journey.Begin(GoesDown);
    }

    /// <summary>
    /// Moves back toward where it rests. One that takes her up receives her from above and stops once it is no higher
    /// than its rest; one that takes her down receives her from below and stops only once it is above its rest. It
    /// then stands at its rest, and she is let go.
    /// </summary>
    private void ReturnToRest(ElevatorJourney journey, Player samus)
    {
        bool stillReturning;
        if (GoesDown)
        {
            MoveBy(-Speed);
            stillReturning = Position.Y.Whole >= _restY;
        }
        else
        {
            MoveBy(Speed);
            stillReturning = Position.Y.Whole < _restY;
        }

        if (stillReturning)
        {
            Hold(samus);
            return;
        }

        journey.End();
        Sound(RestSound, "objects");
        Position = new PixelPosition(Position.X, Position.Y.WithWhole(_restY));
        Hold(samus);

        // She is let go still facing forward, and stays so until she is turned.
        samus.ChangeState(new StandingForwardState());
    }

    private void MoveBy(double pixels) => Position = new PixelPosition(Position.X, Position.Y + Pixels.FromNumber(pixels));

    /// <summary>Puts Samus above it: over its middle, keeping what part of a pixel she had across, and at a whole pixel's height.</summary>
    private void Hold(Player samus)
    {
        samus.Position = new PixelPosition(
            samus.Position.X.WithWhole(Position.X.Whole), Pixels.FromWhole(Position.Y.Whole - HoldsSamusAbove));
        samus.Velocity = new PixelVelocity(samus.Velocity.X, Pixels.Zero);
    }

    /// <summary>Shows its timeline, where its data has one, and moves it on an update.</summary>
    private void ShowLook()
    {
        if (!_timelines.Has("rest"))
            return;
        // Its first update is the first of its first picture's.
        _showing ??= new TimelinePlayer(_timelines.Get("rest"));
        _showing.Step();
        foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
            visual.Show(_showing.Shown);
    }

    private void Sound(string sound, string channel)
    {
        if (sound.Length > 0)
            Room.Session.Sounds.Request(sound, channel: channel);
    }
}
