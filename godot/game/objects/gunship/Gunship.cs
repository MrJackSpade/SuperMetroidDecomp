using Engine.Actors;
using Engine.Actors.Components;
using Engine.Content;
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
/// Its visuals are "Top" and "Bottom", its two halves, and "Pad", its hatch, which plays the timelines its data
/// names "pad_closed", "pad_opening" and "pad_closing". The hull she stands on is the room's own terrain; where
/// the gunship stands is the place of its top half at rest.
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

    /// <summary>How far above the room's top it starts when it comes down to land, in pixels.</summary>
    [Export]
    public int ArrivesFromAbove { get; set; } = 256;

    /// <summary>The timelines of its hatch, by their data files.</summary>
    [Export, DataValue("pad_closed")]
    public string PadClosed { get; set; } = "";

    /// <summary>The timeline of its hatch opening.</summary>
    [Export, DataValue("pad_opening")]
    public string PadOpening { get; set; } = "";

    /// <summary>The timeline of its hatch closing.</summary>
    [Export, DataValue("pad_closing")]
    public string PadClosing { get; set; } = "";

    /// <summary>The effect of the dust its engines raise, by the data file of its timeline.</summary>
    [Export, DataValue("dust_effect")]
    public string DustEffect { get; set; } = "";

    /// <summary>The sound of it hovering, by name.</summary>
    [Export, DataValue("hover_sound")]
    public string HoverSound { get; set; } = "";

    /// <summary>The sound of its hatch opening.</summary>
    [Export, DataValue("pad_opening_sound")]
    public string PadOpeningSound { get; set; } = "";

    /// <summary>The sound of its hatch closing.</summary>
    [Export, DataValue("pad_closing_sound")]
    public string PadClosingSound { get; set; } = "";

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

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _restY = Position.Y.Whole;
        (_phase, _bob, _bobWait, _hoverWait, _answer) = (Phase.Idle, 0, 1, 71, null);
        ShowPad(PadClosed);
    }

    /// <inheritdoc/>
    public void Triggered(string what)
    {
        if (what != "land")
            throw new ArgumentException($"The gunship can be told to \"land\", not \"{what}\".", nameof(what));
        // It starts high above with Samus aboard.
        Player samus = Room.Player;
        samus.ChangeState(new InGunshipState());
        Position = new PixelPosition(Position.X, Pixels.FromWhole(-ArrivesFromAbove));
        samus.Position = new PixelPosition(Position.X, Position.Y + Pixels.FromWhole(TopAboveSamus));
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
            GetNodeOrNull<ActorVisual>("Pad")?.Show(_pad.Current.Picture);

        switch (_phase)
        {
            case Phase.DescendToLand:
                MoveWithSamus(samus, Pixels.FromNumber(Position.Y.Whole < _restY - 351 ? 4.5 : 2.5));
                if (Position.Y.Whole < _restY)
                    break;
                MoveWithSamus(samus, Pixels.FromWhole(_restY) - Position.Y);
                (_phase, _brake) = (Phase.LandingBrakes, 0);
                break;

            case Phase.LandingBrakes:
                MoveWithSamus(samus, Pixels.FromWhole(LandingBrakes[_brake]));
                if (++_brake < LandingBrakes.Length)
                    break;
                (_bob, _bobWait) = (0, 1);
                samus.Position = new PixelPosition(Position.X + Pixels.FromWhole(1), samus.Position.Y);
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
                samus.Release();
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
                    (_phase, _timer) = (Phase.WaitForEngines, 5);
                    break;
                }

                if (!Refill(samus))
                    _phase = Phase.AskToSave;
                break;

            case Phase.AskToSave:
                _answer = null;
                _phase = Phase.AwaitAnswer;
                if (SaveQuestion.Length > 0)
                    Room.Session.RequestQuestion(SaveQuestion, answer => _answer = answer);
                else
                    _answer = false;
                break;

            case Phase.AwaitAnswer:
                if (_answer is not { } yes)
                    break;
                if (yes)
                {
                    Room.Session.RequestSave(Room.SceneFilePath, Name);
                    if (SavedMessage.Length > 0)
                        Room.Session.RequestMessage(SavedMessage);
                }

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
                samus.Release();
                break;

            case Phase.WaitForEngines:
                if (--_timer <= 0)
                    (_phase, _rumble) = (Phase.FireUpEngines, 0);
                break;

            case Phase.FireUpEngines:
                // It shakes on the ground, harder as its engines catch, and raises dust.
                int shake = (_rumble >= 64 ? 2 : 1) * (_rumble % 2 == 1 ? 1 : -1);
                MoveWithSamus(samus, Pixels.FromWhole(shake));
                if (++_rumble == 64 && DustEffect.Length > 0)
                {
                    foreach (int across in DustAcross)
                        Room.Spawn(Effect.Of(FrameTimeline.Read(Room.Content.ReadData(DustEffect)), samus.Position + PixelOffset.FromWhole(across, 80)));
                }

                if (_rumble >= 128)
                    _phase = Phase.SteadyLiftoff;
                break;

            case Phase.SteadyLiftoff:
                MoveWithSamus(samus, Pixels.FromWhole(-2));
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
        if (!samus.Input.WasPressed(GameAction.Down) || samus.State is not StandingState)
            return;
        PixelOffset from = samus.Position - Position;
        if (from.X.Whole <= -8 || from.X.Whole > 8 || from.Y.Whole <= -64 || from.Y.Whole > 0)
            return;
        samus.ChangeState(new InGunshipState());
        samus.Position = new PixelPosition(Position.X, samus.Position.Y);
        OpenHatch();
        _phase = Phase.WaitForHatchToOpen;
    }

    /// <summary>Gives her a little of everything she is short of. Returns whether she is still short of anything.</summary>
    private static bool Refill(Player samus)
    {
        Inventory inventory = samus.Inventory;
        bool shortOfAny = false;
        foreach (string amount in new[] { Inventory.Energy, Amounts.ReserveEnergy, Amounts.Missiles, Amounts.SuperMissiles, Amounts.PowerBombs })
        {
            // Her reserves fill once her energy has.
            if (amount == Amounts.ReserveEnergy && !inventory.IsFull(Inventory.Energy))
            {
                shortOfAny |= !inventory.IsFull(amount);
                continue;
            }

            inventory.Add(amount, 2);
            shortOfAny |= !inventory.IsFull(amount);
        }

        return shortOfAny;
    }

    private void Accelerate(Player samus)
    {
        _escapeSpeed += Pixels.FromNumber(0.25);
        if (_escapeSpeed >= Pixels.FromWhole(10))
            _escapeSpeed = Pixels.FromWhole(9);
        MoveWithSamus(samus, -_escapeSpeed);
    }

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
