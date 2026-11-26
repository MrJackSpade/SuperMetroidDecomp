using Engine.Actors;
using Engine.Actors.Components;
using Engine.Content;
using Engine.Core;
using Engine.Input;
using Engine.Shared;
using Engine.World;
using Game.Data;
using Godot;

namespace Game.Objects;

/// <summary>
/// Samus's gunship. It hovers where it landed, bobbing. When she stands on its hatch and presses down it takes her
/// in, refills everything she carries, asks whether to save, and lets her out again. Told to "land", it comes down
/// out of the sky with her aboard. Once the escape has begun, taking her in is the end: it fires its engines and
/// leaves the planet.
/// </summary>
/// <remarks>
/// Its visuals are "Top" and "Bottom", its two halves, which show the pictures its data names "top" and "bottom",
/// and "Pad", its hatch, which plays the timelines its data names "pad_closed", "pad_opening" and "pad_closing".
/// Its data also says how far its bottom half lies below its top one and its hatch above it, and lists the
/// "dust_effects" its engines raise, one for each place they rise from. Its lights glow for as long as its room is
/// there: its data names the steps of that change of colours ("glow"), which it adds to the room's effects as it is
/// set up and shows on its three visuals. The hull she stands on is the room's own
/// terrain; where the gunship stands is the place of its top half at rest.
/// </remarks>
public partial class Gunship : RoomObject, ITriggerTarget
{
    /// <summary>What is told "away" once it has left the planet.</summary>
    [Export]
    public Godot.Collections.Array<NodePath> ToldWhenAway { get; set; } = [];

    /// <summary>The question it asks once it has refilled her, by name, answered yes or no.</summary>
    [Export]
    public string SaveQuestion { get; set; } = "";

    /// <summary>The message shown once the game is saved, by name.</summary>
    [Export]
    public string SavedMessage { get; set; } = "";

    /// <summary>The timelines of its hatch, by their data files.</summary>
    [Export, DataValue("pad_closed")]
    public string PadClosed { get; set; } = "";

    /// <summary>The timeline of its hatch opening.</summary>
    [Export, DataValue("pad_opening")]
    public string PadOpening { get; set; } = "";

    /// <summary>The timeline of its hatch closing.</summary>
    [Export, DataValue("pad_closing")]
    public string PadClosing { get; set; } = "";

    /// <summary>The picture of its top half.</summary>
    [Export, DataValue("top")]
    public string TopPicture { get; set; } = "";

    /// <summary>The steps of its lights' glow, by their data file.</summary>
    [Export(PropertyHint.File), DataValue("glow")]
    public string GlowFile { get; set; } = "";

    /// <summary>The picture of its bottom half.</summary>
    [Export, DataValue("bottom")]
    public string BottomPicture { get; set; } = "";

    /// <summary>How far its bottom half lies below its top one, in pixels.</summary>
    [Export, DataValue("bottom_below_top")]
    public int BottomBelowTop { get; set; }

    /// <summary>How far its hatch lies above its top half, in pixels.</summary>
    [Export, DataValue("pad_above_top")]
    public int PadAboveTop { get; set; }

    /// <summary>The sound of it hovering, by name.</summary>
    [Export, DataValue("hover_sound")]
    public string HoverSound { get; set; } = "";

    /// <summary>The sound of its hatch opening.</summary>
    [Export, DataValue("pad_opening_sound")]
    public string PadOpeningSound { get; set; } = "";

    /// <summary>The sound of its hatch closing.</summary>
    [Export, DataValue("pad_closing_sound")]
    public string PadClosingSound { get; set; } = "";

    /// <summary>The sound of the game being saved, heard as its question goes away answered yes.</summary>
    [Export, DataValue("saving_sound")]
    public string SavingSound { get; set; } = "";

    /// <summary>How many updates pass between its question going away answered yes and its message of having saved.</summary>
    [Export, DataValue("saving_updates")]
    public int SavingUpdates { get; set; }

    /// <summary>How many updates its message of having saved stays up before it can be dismissed.</summary>
    [Export, DataValue("saved_message_hold_updates")]
    public int SavedMessageHoldUpdates { get; set; }

    /// <summary>How many updates nothing shows before its message of having saved begins to open.</summary>
    [Export, DataValue("saved_message_before_opening_updates")]
    public int SavedMessageBeforeOpeningUpdates { get; set; }

    private enum Phase
    {
        DescendToLand,
        LandingBrakes,
        WaitForLandingHatch,
        RaiseArrival,
        FinishLanding,
        Idle,
        WaitForHatchToOpen,
        LowerSamus,
        WaitForHatchToClose,
        RefillOrLeave,
        AskToSave,
        AwaitAnswer,
        WaitForExitHatch,
        RaiseSamus,
        FinishExit,
        WaitForEngines,
        FireUpEngines,
        SteadyLiftoff,
        AcceleratingLiftoff,
        Away,
    }

    /// <summary>How far each update of its settling moves it, and how far each beat of its bobbing does.</summary>
    private static readonly int[] LandingBrakes = [1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, -1, -1, -1, -1, -1, -1];
    private static readonly int[] Bobs = [1, -1, -1, 1];
    private static readonly int[] DustAcross = [64, 72, 80, -64, -72, -80];

    /// <summary>How many updates its hatch takes to open or close, and each beat of its bobbing lasts.</summary>
    private const int HatchUpdates = 144, BobUpdates = 16;

    /// <summary>How far above her middle its top half sits while she is aboard.</summary>
    private const int TopAboveSamus = 17;

    /// <summary>How much of each thing she carries the ship gives her each update she is aboard.</summary>
    private const int RefillEach = 2;

    private Phase _phase;
    private int _timer;
    private int _restY;
    private int _bob;
    private int _bobWait;
    private int _hoverWait;
    private int _brake;
    private int _rumble;
    private Pixels _escapeSpeed;
    private bool? _answer;
    private TimelinePlayer? _pad;
    private readonly List<string> _dustEffects = [];

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _dustEffects.Clear();
        _dustEffects.AddRange(data.List("dust_effects").Select(effect => effect.AsText()));
        if (_dustEffects.Count != DustAcross.Length)
            throw data.Problem($"must name {DustAcross.Length} dust effects, one for each place its engines raise dust");
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _restY = Position.Y.Whole;
        (_phase, _bob, _bobWait, _hoverWait, _answer) = (Phase.Idle, 0, 1, 71, null);
        GetNodeOrNull<ActorVisual>("Top")?.Show(TopPicture);
        if (GetNodeOrNull<ActorVisual>("Bottom") is { } bottom)
        {
            bottom.Position = new Vector2(0, BottomBelowTop);
            bottom.Show(BottomPicture);
        }

        if (GetNodeOrNull<ActorVisual>("Pad") is { } pad)
            pad.Position = new Vector2(0, -PadAboveTop);
        ShowPad(PadClosed);

        // Its lights glow from the room's first update on, among the room's changes of colours.
        var glow = new ColourEffect { Name = "GunshipGlow", StepsFile = GlowFile, OnTerrain = false, OnBackdrops = false };
        Room.AddEffect(glow);
        foreach (ActorVisual piece in GetChildren().OfType<ActorVisual>())
            glow.ShowOn(piece);
    }

    /// <inheritdoc/>
    public void Triggered(string what)
    {
        if (what != "land")
            throw new ArgumentException($"The gunship can be told to \"land\", not \"{what}\".", nameof(what));
        // It starts about Samus, where she arrives high over the landing site, with her aboard.
        Player samus = Room.Player;
        samus.ChangeState(new FacingForwardState());
        PlaceAbout(samus);
        (_phase, _hoverWait) = (Phase.DescendToLand, 1);
    }

    /// <inheritdoc/>
    protected override void OnStep()
    {
        Player samus = Room.Player;
        if (--_hoverWait <= 0)
        {
            _hoverWait = 70;
            Sound(HoverSound);
        }

        if (_phase is >= Phase.WaitForLandingHatch and <= Phase.WaitForEngines)
            Bob();
        _pad?.Step();
        if (_pad is not null)
            GetNodeOrNull<ActorVisual>("Pad")?.Show(_pad.Shown);

        switch (_phase)
        {
            case Phase.DescendToLand:
                MoveWithSamus(samus, Pixels.FromNumber(Position.Y.Whole < _restY - 351 ? 4.5 : 2.5));
                if (Position.Y.Whole < _restY)
                    break;

                // The ship alone is set at its landing height, keeping its part of a pixel; Samus stays as far down
                // as the last of the descent took her.
                Position = new PixelPosition(Position.X, Position.Y.WithWhole(_restY));
                (_phase, _brake) = (Phase.LandingBrakes, 0);
                break;

            case Phase.LandingBrakes:
                MoveWithSamus(samus, Pixels.FromWhole(LandingBrakes[_brake]));
                if (++_brake < LandingBrakes.Length)
                    break;
                (_bob, _bobWait) = (0, 1);

                // She is put a pixel right of the ship's middle, which the view does not follow as a move of hers.
                samus.Position = new PixelPosition(samus.Position.X.WithWhole(Position.X.Whole + 1), samus.Position.Y);
                Room.Session.Camera.TakePlayerToHaveStoodAtX(samus.Position.X.Whole);
                OpenHatch();
                _phase = Phase.WaitForLandingHatch;
                break;

            case Phase.WaitForLandingHatch:
                if (--_timer <= 0)
                    _phase = Phase.RaiseArrival;
                break;

            case Phase.RaiseArrival:
                samus.Position -= PixelOffset.FromWhole(0, 1);
                if (samus.Position.Y.Whole >= _restY - 30)
                    break;
                CloseHatch(sounding: false);
                _phase = Phase.FinishLanding;
                break;

            case Phase.FinishLanding:
                if (--_timer > 0)
                    break;
                _phase = Phase.Idle;

                // She is let go still in the front view.
                samus.ChangeState(new StandingForwardState());
                // Having landed, the game is saved.
                Room.Session.RequestSave(Room.SceneFilePath, Name);
                break;

            case Phase.Idle:
                TakeSamusIn(samus);
                break;

            case Phase.WaitForHatchToOpen:
                if (--_timer <= 0)
                    _phase = Phase.LowerSamus;
                break;

            case Phase.LowerSamus:
                samus.Position += PixelOffset.FromWhole(0, 2);
                if (samus.Position.Y.Whole < _restY + 18)
                    break;
                CloseHatch(sounding: false);
                _phase = Phase.WaitForHatchToClose;
                break;

            case Phase.WaitForHatchToClose:
                if (--_timer <= 0)
                    _phase = Phase.RefillOrLeave;
                break;

            case Phase.RefillOrLeave:
                if (Room.Session.Progress.Has(StoryEvents.EscapeStarted))
                {
                    // She has made it: the countdown stops.
                    Room.Session.Service<Escape>().Stop();
                    (_phase, _timer) = (Phase.WaitForEngines, 5);
                    break;
                }

                if (!Refill(samus))
                    _phase = Phase.AskToSave;
                break;

            case Phase.AskToSave:
                _answer = null;
                _phase = Phase.AwaitAnswer;
                // The saving is heard and told of before the answer comes back: play stays stopped through both.
                if (SaveQuestion.Length > 0)
                    Room.Session.RequestQuestion(SaveQuestion, answer => _answer = answer, SavedMessage.Length == 0 ? null
                        : new MessageAfterYes(SavingSound, SavingUpdates, SavedMessage, SavedMessageHoldUpdates, SavedMessageBeforeOpeningUpdates));
                else
                    _answer = false;
                break;

            case Phase.AwaitAnswer:
                if (_answer is not { } yes)
                    break;
                if (yes)
                    Room.Session.RequestSave(Room.SceneFilePath, Name);

                // The pad opens to let her out without its sound.
                OpenHatch(sounding: false);
                _phase = Phase.WaitForExitHatch;
                break;

            case Phase.WaitForExitHatch:
                if (--_timer <= 0)
                    _phase = Phase.RaiseSamus;
                break;

            case Phase.RaiseSamus:
                samus.Position -= PixelOffset.FromWhole(0, 2);
                if (samus.Position.Y.Whole >= _restY - 30)
                    break;
                CloseHatch(sounding: true);
                _phase = Phase.FinishExit;
                break;

            case Phase.FinishExit:
                if (--_timer > 0)
                    break;
                _phase = Phase.Idle;
                samus.ChangeState(new StandingForwardState());
                break;

            case Phase.WaitForEngines:
                if (--_timer <= 0)
                    (_phase, _rumble) = (Phase.FireUpEngines, 0);
                break;

            case Phase.FireUpEngines:
                // It shakes on the ground, harder as its engines catch, and raises dust.
                int shake = (_rumble >= 64 ? 2 : 1) * (_rumble % 2 == 1 ? 1 : -1);
                samus.Position += PixelOffset.FromWhole(0, shake);
                PlaceAbout(samus);
                if (++_rumble == 64)
                {
                    // Each cloud is one of the enemies' shots on the cartridge: it holds one of the room's places while
                    // it plays, and is not made with none free.
                    for (int cloud = 0; cloud < DustAcross.Length; cloud++)
                        Room.Spawn(Effect.Of(FrameTimeline.Read(Room.Content.ReadData(_dustEffects[cloud])), samus.Position + PixelOffset.FromWhole(DustAcross[cloud], 80), holdsPlace: true));
                }

                if (_rumble >= 128)
                    _phase = Phase.SteadyLiftoff;
                break;

            case Phase.SteadyLiftoff:
                samus.Position -= PixelOffset.FromWhole(0, 2);
                PlaceAbout(samus);
                if (Position.Y.Whole < _restY - 223)
                    (_phase, _escapeSpeed) = (Phase.AcceleratingLiftoff, Pixels.FromWhole(2));
                break;

            case Phase.AcceleratingLiftoff:
                Accelerate(samus);
                if (Position.Y.Whole >= _restY - 863)
                    break;
                _phase = Phase.Away;
                foreach (NodePath path in ToldWhenAway)
                    (GetNode(path) as ITriggerTarget ?? throw new InvalidOperationException($"The gunship is to tell \"{path}\" that it is away, but that is nothing that can be told.")).Triggered("away");
                break;

            case Phase.Away:
                Accelerate(samus);
                break;

            default:
                throw new InvalidOperationException($"The gunship has no step {_phase}.");
        }
    }

    /// <summary>With Samus standing on its hatch, a press of down takes her in.</summary>
    private void TakeSamusIn(Player samus)
    {
        if (!samus.Input.WasPressed(GameAction.Down) || samus.State is not (StandingState or StandingForwardState))
            return;
        PixelOffset from = samus.Position - Position;
        if (from.X.Whole <= -8 || from.X.Whole > 8 || from.Y.Whole <= -64 || from.Y.Whole > 0)
            return;
        samus.ChangeState(new FacingForwardState());

        // Going aboard ends an elevator's ride and disarms its door.
        ElevatorJourney.Of(Room.Session).End();

        // She is put over the ship's middle, keeping her part of a pixel, and the view does not follow that.
        if (samus.Position.X.Whole != Position.X.Whole)
        {
            samus.Position = new PixelPosition(samus.Position.X.WithWhole(Position.X.Whole), samus.Position.Y);
            Room.Session.Camera.TakePlayerToHaveStoodAtX(samus.Position.X.Whole);
        }

        OpenHatch();
        _phase = Phase.WaitForHatchToOpen;
    }

    /// <summary>Gives her a little of everything she is short of. Returns whether she is still short of anything.</summary>
    private static bool Refill(Player samus)
    {
        Inventory inventory = samus.Inventory;

        // Her energy takes its two; what of them it has no room for goes to her reserves, on the same update.
        EnergyRestore.Give(inventory, RefillEach);
        bool shortOfAny = !inventory.IsFull(Inventory.Energy) || !inventory.IsFull(Amounts.ReserveEnergy);
        foreach (string amount in new[] { Amounts.Missiles, Amounts.SuperMissiles, Amounts.PowerBombs })
        {
            inventory.Add(amount, RefillEach);
            shortOfAny |= !inventory.IsFull(amount);
        }

        return shortOfAny;
    }

    private void Accelerate(Player samus)
    {
        _escapeSpeed += Pixels.FromNumber(0.25);
        if (_escapeSpeed >= Pixels.FromWhole(10))
            _escapeSpeed = Pixels.FromWhole(9);
        samus.Position -= new PixelOffset(Pixels.Zero, _escapeSpeed);
        PlaceAbout(samus);
    }

    /// <summary>Sets the ship about Samus: its top a set height above her whole place, keeping its own part of a pixel.</summary>
    private void PlaceAbout(Player samus) =>
        Position = new PixelPosition(Position.X, Position.Y.WithWhole(samus.Position.Y.Whole - TopAboveSamus));

    /// <summary>Moves the ship, and Samus with it.</summary>
    private void MoveWithSamus(Player samus, Pixels down)
    {
        Position += new PixelOffset(Pixels.Zero, down);
        samus.Position += new PixelOffset(Pixels.Zero, down);
    }

    /// <summary>It bobs where it hovers.</summary>
    private void Bob()
    {
        if (--_bobWait > 0)
            return;
        _bobWait = BobUpdates;
        Position += PixelOffset.FromWhole(0, Bobs[_bob]);
        _bob = (_bob + 1) % Bobs.Length;
    }

    private void OpenHatch(bool sounding = true)
    {
        ShowPad(PadOpening);
        _timer = HatchUpdates;
        if (sounding)
            Sound(PadOpeningSound);
    }

    private void CloseHatch(bool sounding)
    {
        ShowPad(PadClosing);
        _timer = HatchUpdates;
        if (sounding)
            Sound(PadClosingSound);
    }

    private void ShowPad(string timeline)
    {
        if (timeline.Length > 0)
            _pad = new TimelinePlayer(FrameTimeline.Read(Room.Content.ReadData(timeline)));
    }

    private void Sound(string sound)
    {
        if (sound.Length > 0)
            Room.Session.Sounds.Request(sound, channel: "objects");
    }
}
