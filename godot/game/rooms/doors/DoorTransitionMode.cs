using Engine.Core;
using Engine.Input;
using Engine.Shared;
using Engine.Ui;
using Engine.World;
using Game.Compatibility;
using Game.Data;
using Godot;

namespace Game;

/// <summary>
/// The passage through a door, update for update as the cartridge makes it: the room being left fades out while its
/// enemies go on, the view is brought square to a screen, the next room is set up and scrolls in while Samus is
/// carried through the doorway at the door's own speed, and it fades in while its enemies and their shots go on.
/// Play is kept beneath it; Samus is not stepped until it is over.
/// </summary>
/// <remarks>
/// Each step below is a count of updates the old transition takes, and says whether the update opens as an update of
/// play does (the liquid, the statues and the update's random number), with the random number alone, or with
/// neither. A power bomb's blast does not go on through a door. The source room is not kept on screen while the
/// next scrolls in.
/// <para>
/// A door entered while an elevator's door is armed is an elevator's. Going down, the ride goes on for a wait of
/// updates before the passage begins. As the next room is set up a ride that was leaving arrives there: its elevator
/// puts Samus above it, and the scroll carries her on from that place.
/// </para>
/// </remarks>
public partial class DoorTransitionMode : GameMode
{
    private enum Stage
    {
        Entering,
        WaitingForSounds,
        FadingOut,
        BeginningDisplay,
        AligningView,
        WaitingForSetup,
        MovingToScrollStart,
        PlacingSamus,
        Loading,
        Scrolling,
        AligningSamus,
        SteppingBlocks,
        WaitingForMusic,
        Finishing,
        FadingIn,
    }

    /// <summary>The last of a fade's counted steps that moves its colours; the step after ends it.</summary>
    private const int FadeLastStep = 13;

    /// <summary>How many updates an elevator going down rides on before its door's passage begins.</summary>
    private const int ElevatorDownWaitUpdates = 48;

    /// <summary>How many updates pass while the next room is taken to be setting up.</summary>
    private const int SetupWaitUpdates = 2;

    /// <summary>How far the view moves on each update of the scroll, in pixels.</summary>
    private const int ViewStep = 4;

    /// <summary>How far below the next room's screen an upward door's scroll leaves the view.</summary>
    private const int UpwardViewSnap = 32;

    /// <summary>Half a block: what a doorway across aligns Samus to.</summary>
    private const int HalfBlock = 8;

    private const int Block = 16;

    private readonly PlayingMode _playing;
    private readonly string _toRoom;
    private readonly string _toExit;
    private readonly Door.Way _way;
    private readonly Vector2I _toScreen;
    private readonly Pixels _samusStep;
    private ScreenFade? _fade;
    private Stage _stage = Stage.Entering;
    private int _fadeSteps;
    private int _wait;
    private int _scrollLeft;
    private PixelPosition _sourceSamus;
    private Vector2I _view;
    private int _stoodAtX;
    private int _stoodAtY;
    private ClosingSoundWork _closing = ClosingSoundWork.Usual;
    private bool _elevatorAsked;
    private int _elevatorWait;

    /// <summary>
    /// Whether the passage itself has begun: from then until play has control again, what holds still through a
    /// door, as an elevator does, holds still. Before it an elevator going down still rides on.
    /// </summary>
    public bool PassageBegun { get; private set; }

    /// <summary>The passage that follows Samus entering <paramref name="door"/> during <paramref name="playing"/>.</summary>
    public DoorTransitionMode(PlayingMode playing, Door door)
    {
        ArgumentNullException.ThrowIfNull(playing);
        ArgumentNullException.ThrowIfNull(door);
        if (door.LeadsToRoom.Length == 0 || door.LeadsToExit.Length == 0)
            throw new InvalidOperationException($"The door \"{door.Name}\" does not say which room and exit it leads to.");
        _playing = playing;

        // The door is gone with its room before the passage is over: what it says is taken now.
        (_toRoom, _toExit, _way, _toScreen) = (door.LeadsToRoom, door.LeadsToExit, door.Travel, door.ToScreen);
        _samusStep = Pixels.FromNumber(door.SamusStep);
    }

    /// <inheritdoc/>
    public override GameMode? Keeps => _playing;

    /// <inheritdoc/>
    protected override void Enter()
    {
        _fade = new ScreenFade { Name = "Fade" };
        AddChild(_fade);
        _fade.Cover(0, FadeLastStep);
    }

    /// <inheritdoc/>
    public override GameMode Step(GameInput input)
    {
        Room room = _playing.Room;
        Samus samus = Session.Player as Samus ?? throw new InvalidOperationException("The player passing through a door is not Samus.");
        ScreenFade fade = _fade ?? throw new InvalidOperationException("The door's passage has not taken control.");
        _closing = _stage is Stage.Loading or Stage.Scrolling ? ClosingSoundWork.None : ClosingSoundWork.Usual;

        // From the update that places Samus for the next room's load to the one that finishes the scroll, the
        // clock's short count is started over on every update, so what is timed by it begins together afterwards.
        if (_stage is Stage.PlacingSamus or Stage.Loading or Stage.Scrolling or Stage.AligningSamus or Stage.SteppingBlocks
            or Stage.WaitingForMusic or Stage.Finishing)
            Session.Clock.RestartShort();
        switch (_stage)
        {
            case Stage.Entering:
                // An elevator's door, going down: the ride goes on behind the door for its wait, each update played
                // as one of the door's waits for sounds, and the passage begins on the update after.
                if (!_elevatorAsked)
                {
                    _elevatorAsked = true;
                    ElevatorJourney journey = ElevatorJourney.Of(Session);
                    if (journey.DoorArmed && journey.GoesDown)
                        _elevatorWait = ElevatorDownWaitUpdates;
                }

                if (_elevatorWait > 0)
                {
                    _elevatorWait--;
                    room.OpenUpdate(movesLiquid: true, opensProjectiles: false);
                    room.StepForTransition(enemies: true);
                    break;
                }

                PassageBegun = true;

                // The door's first update: the room's enemies run once more with sounds still let in, and then
                // Samus's sounds and the enemies' are told to stop. From here until the next room has faded in
                // every sound asked for is turned away.
                room.OpenUpdate(movesLiquid: true, opensProjectiles: false);
                room.StepForTransition(enemies: true);
                StopSounds(samus, input);
                CartridgeSounds.Of(Session).DoorTurnsSoundsAway = true;
                _stage = Stage.WaitingForSounds;
                break;

            case Stage.WaitingForSounds:
                room.OpenUpdate(movesLiquid: true, opensProjectiles: false);
                room.StepForTransition(enemies: true);
                if (!Session.Sounds.HasQueuedSounds)
                    _stage = Stage.FadingOut;
                break;

            case Stage.FadingOut:
                room.OpenUpdate(movesLiquid: true, opensProjectiles: false);
                if (TakeFadeStep(out int moved))
                {
                    fade.Cover(FadeLastStep, FadeLastStep);
                    _stage = Stage.BeginningDisplay;
                }
                else
                {
                    fade.Cover(moved, FadeLastStep);
                    room.StepForTransition(enemies: true);
                }

                break;

            case Stage.BeginningDisplay:
                // The last update on which the liquid and the statues move, until the next room is loaded.
                room.OpenUpdate(movesLiquid: true, opensProjectiles: false);
                _stage = Stage.AligningView;
                break;

            case Stage.AligningView:
                Session.OpenUpdate();
                if (ViewIsSquare())
                {
                    _wait = SetupWaitUpdates;
                    _stage = Stage.WaitingForSetup;
                }

                break;

            case Stage.WaitingForSetup:
                Session.OpenUpdate();
                if (--_wait == 0)
                    _stage = Stage.MovingToScrollStart;
                break;

            case Stage.MovingToScrollStart:
                // The scroll starts from where she stands now; a door going down alone does not move her yet.
                Session.OpenUpdate();
                _sourceSamus = samus.Position;
                _stage = Stage.PlacingSamus;
                break;

            case Stage.PlacingSamus:
                Session.OpenUpdate();
                _stage = Stage.Loading;
                break;

            case Stage.Loading:
                Session.OpenUpdate();
                Load(samus);
                _stage = Stage.Scrolling;
                break;

            case Stage.Scrolling:
                // The scroll's own updates open with nothing: no random number is drawn on them.
                Scroll(samus);
                if (--_scrollLeft == 0)
                {
                    // The view comes to rest on the door's screen, a little lower for a door going up, and the
                    // room's objects run once before the fade begins.
                    _view = new Vector2I(_toScreen.X * Camera.Screen, _toScreen.Y * Camera.Screen + (_way == Door.Way.Up ? UpwardViewSnap : 0));
                    Session.Camera.MoveTo(_view.X, _view.Y);
                    room.StepForTransition(objects: true);
                    _stage = Stage.AligningSamus;
                }
                else
                {
                    room.ShowNow();
                }

                break;

            case Stage.AligningSamus:
                // A doorway across brings her to the edge of its half block; one up or down, nowhere.
                int x = samus.Position.X.Whole;
                if (_way == Door.Way.Right)
                    samus.Position = new PixelPosition(samus.Position.X.WithWhole(x - x % HalfBlock + HalfBlock - 1), samus.Position.Y);
                else if (_way == Door.Way.Left)
                    samus.Position = new PixelPosition(samus.Position.X.WithWhole(x - x % HalfBlock), samus.Position.Y);
                room.ShowNow();
                _stage = Stage.SteppingBlocks;
                break;

            case Stage.SteppingBlocks:
                room.OpenUpdate(movesLiquid: true, opensProjectiles: false);
                room.StepForTransition(terrain: true);
                _stage = Stage.WaitingForMusic;
                break;

            case Stage.WaitingForMusic:
                // Once no music waits, the room's own is asked for: its set was asked for as it was set up.
                room.OpenUpdate(movesLiquid: true, opensProjectiles: false);
                if (!Session.Sounds.HasQueuedMusic)
                {
                    CartridgeSounds sounds = CartridgeSounds.Of(Session);
                    if (!sounds.DemoPlaying)
                        sounds.AskRoomMusic(room);
                    _stage = Stage.Finishing;
                }

                break;

            case Stage.Finishing:
                // Only now is she moved clear of the doorway's blocks at the screen's edges.
                room.OpenUpdate(movesLiquid: true, opensProjectiles: false);
                samus.Position = new PixelPosition(
                    samus.Position.X.WithWhole(OffFarEdge(OffNearEdge(samus.Position.X.Whole))),
                    samus.Position.Y.WithWhole(OffNearEdge(samus.Position.Y.Whole)));

                // An elevator's door leaves her in the elevator's hands.
                if (ElevatorJourney.Of(Session).DoorArmed && samus.State is not RidingElevatorState)
                    samus.ChangeState(new RidingElevatorState());
                room.ShowNow();

                // A door forces the screen fully lit before the room beyond fades in, however dark a pause's fade left it.
                Session.Darkness.Show(0);
                _fadeSteps = 0;
                _stage = Stage.FadingIn;
                break;

            case Stage.FadingIn:
                room.OpenUpdate(movesLiquid: true, opensProjectiles: false);
                room.StepForTransition(terrain: true, enemies: true, enemyProjectiles: true);
                if (TakeFadeStep(out int shown))
                {
                    fade.Cover(0, FadeLastStep);

                    // The view takes her to have stood where the scroll last put her, so it follows the alignment
                    // and the nudge as moves of hers on the first update of play.
                    Session.Camera.TakePlayerToHaveStoodAtX(_stoodAtX);
                    Session.Camera.TakePlayerToHaveStoodAtY(_stoodAtY);

                    // Sounds are let in again, and the spin's, stopped as the door was entered, is asked for anew.
                    CartridgeSounds.Of(Session).DoorTurnsSoundsAway = false;
                    if (samus.SpinSound is { } spinSound)
                        Session.Sounds.Request(spinSound, channel: "samus", priority: SoundPriority.High);
                    return _playing;
                }

                fade.Cover(FadeLastStep - shown, FadeLastStep);
                break;

            default:
                throw new InvalidOperationException($"Undefined stage {_stage} of a door's passage.");
        }

        return this;
    }

    /// <summary>
    /// Takes one step of a fade: its first step shows what it starts from, each later one moves one more part, and
    /// the step after its last ends it. When it ends is by the count every fade of the cartridge shares
    /// (<see cref="SharedFadeCount"/>); how far it shows is by its own steps.
    /// </summary>
    /// <returns>Whether the fade ended on this update, moving nothing.</returns>
    private bool TakeFadeStep(out int moved)
    {
        moved = Math.Min(_fadeSteps, FadeLastStep);
        if (Session.Service<SharedFadeCount>().TakeStep(FadeLastStep))
        {
            _fadeSteps = 0;
            return true;
        }

        _fadeSteps++;
        return false;
    }

    /// <summary>
    /// Brings the view a pixel nearer a screen's boundary across the door's way of travel: a view past the middle of
    /// its screen moves on to the next, one before it back to its own. True when it already stood on one.
    /// </summary>
    private bool ViewIsSquare()
    {
        bool alignsAcross = _way is Door.Way.Down or Door.Way.Up;
        Camera camera = Session.Camera;
        int coordinate = alignsAcross ? camera.Left : camera.Top;
        int within = ((coordinate % Camera.Screen) + Camera.Screen) % Camera.Screen;
        if (within == 0)
            return true;
        int moved = coordinate + (within >= Camera.Screen / 2 ? 1 : -1);
        camera.MoveTo(alignsAcross ? moved : camera.Left, alignsAcross ? camera.Top : moved);
        _playing.Room.ShowNow();
        return false;
    }

    /// <summary>Where the view stands as the next room's scroll is set up, from the screen the door leads to.</summary>
    private Vector2I SetupView => new Vector2I(_toScreen.X * Camera.Screen, _toScreen.Y * Camera.Screen) + _way switch
    {
        Door.Way.Right => new Vector2I(-252, 0),
        Door.Way.Left => new Vector2I(252, 0),
        Door.Way.Down => new Vector2I(0, -224),
        Door.Way.Up => new Vector2I(0, 251),
        _ => throw new InvalidOperationException($"Undefined way {_way} through a door."),
    };

    /// <summary>How many steps the scroll takes, the two of the update that loads the room among them.</summary>
    private int MovingUpdates => _way switch
    {
        Door.Way.Right or Door.Way.Left => 63,
        Door.Way.Down => 56,
        Door.Way.Up => 55,
        _ => throw new InvalidOperationException($"Undefined way {_way} through a door."),
    };

    /// <summary>One update's move of the view along the door's way.</summary>
    private Vector2I ViewStepAlong => _way switch
    {
        Door.Way.Right => new Vector2I(ViewStep, 0),
        Door.Way.Left => new Vector2I(-ViewStep, 0),
        Door.Way.Down => new Vector2I(0, ViewStep),
        Door.Way.Up => new Vector2I(0, -ViewStep),
        _ => throw new InvalidOperationException($"Undefined way {_way} through a door."),
    };

    /// <summary>Samus moved by one update of the scroll, along the door's way at the door's speed.</summary>
    private PixelPosition Stepped(PixelPosition from) => _way switch
    {
        Door.Way.Right => new PixelPosition(from.X + _samusStep, from.Y),
        Door.Way.Left => new PixelPosition(from.X - _samusStep, from.Y),
        Door.Way.Down => new PixelPosition(from.X, from.Y + _samusStep),
        Door.Way.Up => new PixelPosition(from.X, from.Y - _samusStep),
        _ => throw new InvalidOperationException($"Undefined way {_way} through a door."),
    };

    /// <summary>
    /// Sets the next room up with Samus in it, and takes the scroll's first two steps: the one taken while the room
    /// loaded, and this update's own. She arrives where the scroll's setup puts her: the first step of the scroll
    /// taken from where she stood (not for a door going down), her place within her screen kept at the view's setup
    /// place, fractions and all. An elevator that receives a ride puts her above it as the room is set up, and
    /// after the scroll's first step she is put back there, at a whole pixel's height, to be carried on from it.
    /// </summary>
    private void Load(Samus samus)
    {
        PixelPosition moved = _way == Door.Way.Down ? _sourceSamus : Stepped(_sourceSamus);
        Vector2I setup = SetupView;
        var rebased = new PixelPosition(
            moved.X.WithWhole(setup.X + WithinScreen(moved.X.Whole)), moved.Y.WithWhole(setup.Y + WithinScreen(moved.Y.Whole)));
        (_stoodAtX, _stoodAtY) = (rebased.X.Whole, rebased.Y.Whole);

        // A ride that was leaving the room arrives in the next.
        ElevatorJourney journey = ElevatorJourney.Of(Session);
        journey.PassDoor();
        var entry = new RoomEntry(_toExit, samus.Velocity, samus.Facing) { At = rebased, KeepsView = true };
        _playing.ChangeRoom(_toRoom, entry);
        PixelPosition? onElevator = journey.TakeSetDownPlace();
        samus.Position = rebased;
        _view = setup;
        _scrollLeft = MovingUpdates;
        Scroll(samus);
        if (onElevator is { } place)
            samus.Position = new PixelPosition(samus.Position.X.WithWhole(place.X.Whole), Pixels.FromWhole(place.Y.Whole));
        Scroll(samus);
        _scrollLeft -= 2;
        _fadeSteps = 0;
        _playing.Room.ShowNow();
    }

    /// <summary>One update of the scroll: the view and Samus each move along the door's way.</summary>
    private void Scroll(Samus samus)
    {
        _view += ViewStepAlong;
        Session.Camera.MoveTo(_view.X, _view.Y);
        samus.Position = Stepped(samus.Position);
        if (_way is Door.Way.Right or Door.Way.Left)
            _stoodAtX = samus.Position.X.Whole;
        else
            _stoodAtY = samus.Position.Y.Whole;
    }

    private static int WithinScreen(int pixel) => ((pixel % Camera.Screen) + Camera.Screen) % Camera.Screen;

    /// <summary>A place in the second block of its screen is moved on past that block's end.</summary>
    private static int OffNearEdge(int pixel) =>
        WithinScreen(pixel) / Block == 1 ? pixel - pixel % Block + Block - 1 + HalfBlock : pixel;

    /// <summary>A place in the fifteenth block of its screen is moved back before that block's start.</summary>
    private static int OffFarEdge(int pixel) =>
        WithinScreen(pixel) / Block == 14 ? pixel - pixel % Block - HalfBlock : pixel;

    /// <summary>
    /// The music waiting gets no turn on the updates that carry the next room's load on: the scroll's, and the one
    /// that aligns Samus.
    /// </summary>
    public override bool GivesMusicItsTurn => _stage is not (Stage.Scrolling or Stage.AligningSamus);

    /// <summary>The sounds waiting get no turn on an update that leaves the next room's load running: the load's own, and the scroll's.</summary>
    public override ClosingSoundWork SoundWorkAtClose => _closing;

    /// <summary>
    /// Tells Samus's sounds and the enemies' to stop as the door is entered: her spin's alone while she spins, and
    /// otherwise all of hers unless she holds the attack or a charge has begun to sound. No number of sounds waiting
    /// turns these away.
    /// </summary>
    private void StopSounds(Samus samus, GameInput input)
    {
        if (samus.State is SpinJumpingState or Engine.Actors.WallJumpingState)
            Session.Sounds.Request(Sounds.SpinJumpStop, channel: "samus", priority: SoundPriority.Essential);
        else if (!input.IsHeld(GameAction.Attack) && !samus.ChargeIsSounding)
            Session.Sounds.Request(Sounds.SamusStop, channel: "samus", priority: SoundPriority.Essential);
        Session.Sounds.Request(Sounds.EnemyStop, channel: "enemies", priority: SoundPriority.Essential);
    }
}
