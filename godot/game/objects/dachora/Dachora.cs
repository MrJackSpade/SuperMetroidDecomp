using Engine.Actors;
using Engine.Actors.Components;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.Ui;
using Engine.World.Objects;
using Godot;

namespace Game.Objects;

/// <summary>
/// The Dachora, the bird that shows Samus the shinespark. It stands until she comes near, blinks at her, and then
/// runs: back and forth along its corridor, faster and faster until it glows; at the foot of the shaft it crouches,
/// gathers itself and shoots straight up to the ceiling, then falls and runs again.
/// </summary>
/// <remarks>
/// Its timelines are "idle", "blink", "running", "running_fast", "running_very_fast", "charge", "shinespark" and
/// "falling", each ending in "_left" or "_right" for the way it faces.
/// </remarks>
public partial class Dachora : Creature
{
    /// <summary>The way it faces to begin with.</summary>
    [Export]
    public Facing StartsFacing { get; set; } = Facing.Left;

    /// <summary>How near Samus must come for it to start, in pixels across and down.</summary>
    [Export]
    public Vector2I StartsWithin { get; set; } = new(96, 64);

    /// <summary>How far across the room it turns back when running left, in pixels.</summary>
    [Export]
    public int TurnsBackAt { get; set; } = 96;

    /// <summary>How far across the room the foot of its shaft is, in pixels: running right, it shoots up from here.</summary>
    [Export]
    public int ShootsUpAt { get; set; } = 1152;

    /// <summary>How fast things fall here, in pixels an update each update.</summary>
    [Export]
    public double Gravity { get; set; } = 0.109375;

    /// <summary>The effect it leaves behind it as it shoots up, by the data file of its timeline.</summary>
    [Export, DataValue("echo_effect")]
    public string EchoEffect { get; set; } = "";

    /// <summary>The sound of it noticing Samus, by name.</summary>
    [Export, DataValue("activation_sound")]
    public string ActivationSound { get; set; } = "";

    /// <summary>The sound of it running at full speed.</summary>
    [Export, DataValue("speed_sound")]
    public string SpeedSound { get; set; } = "";

    /// <summary>The sound that takes the place of that one when it stops.</summary>
    [Export, DataValue("speed_stop_sound")]
    public string SpeedStopSound { get; set; } = "";

    /// <summary>The sound of it gathering itself.</summary>
    [Export, DataValue("charge_sound")]
    public string ChargeSound { get; set; } = "";

    /// <summary>The sound of it shooting up.</summary>
    [Export, DataValue("launch_sound")]
    public string LaunchSound { get; set; } = "";

    /// <summary>The sound of it striking the ceiling.</summary>
    [Export, DataValue("ceiling_sound")]
    public string CeilingSound { get; set; } = "";

    private enum Doing
    {
        Waiting,
        Blinking,
        Running,
        Charging,
        Shinesparking,
        Falling,
    }

    private static readonly Pixels TopSpeed = Pixels.FromWhole(8);
    private static readonly Pixels FastSpeed = Pixels.FromWhole(4);
    private static readonly Pixels Acceleration = Pixels.FromNumber(1 / 16.0);
    private static readonly Pixels FastestFall = Pixels.FromWhole(10);

    /// <summary>The updates each frame of the speed glow shows before the next; it stays on its last, brightest frame.</summary>
    private const int SpeedGlowFrameUpdates = 16;

    /// <summary>The frames of its speed glow, each the colours of its pictures it shows changed.</summary>
    private ColourSwap[][] _speedGlow = [];

    /// <summary>The frames of its shine glow, each the colours of its pictures it shows changed.</summary>
    private ColourSwap[][] _shineGlow = [];

    private Doing _doing;
    private int _timer;
    private Pixels _speed;
    private Pixels _lift;
    private int _echoWait;

    /// <summary>
    /// The update at full speed on which the speed glow of a run begun after a wall or the ceiling stopped it first
    /// shows; such a run asks for no speed sound. The cartridge finds that glow's count run out and counts it down
    /// from the top, so its first frame comes sixteen updates after the 256th.
    /// </summary>
    private const int GlowAfterStopFirstUpdate = 272;

    /// <summary>
    /// The update at full speed on which a run begun after a stop would show a frame its speed glow does not have, as
    /// the cartridge reads one past the glow's frames there. It runs out of its corridor long before.
    /// </summary>
    private const int GlowAfterStopStrayUpdate = 256;

    /// <summary>The updates it has glowed for, since its run reached full speed or since it began to gather itself.</summary>
    private int _glowing;

    /// <summary>Whether its run began after a wall or the ceiling stopped it, and not from its blink or its turn.</summary>
    private bool _runAfterStop;

    private string Side => Facing == Facing.Right ? "right" : "left";

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        DataNode glows = data.Get("glow_colours");
        _speedGlow = ReadGlow(glows, "speed");
        _shineGlow = ReadGlow(glows, "shine");
    }

    /// <summary>Reads the frames of one glow, each a group of the colours it changes.</summary>
    private static ColourSwap[][] ReadGlow(DataNode glows, string name)
    {
        ColourSwap[][] frames = [.. glows.List(name).Select(ColourChange.ReadSwaps)];
        return frames.Length > 0 ? frames : throw glows.Problem($"must give at least one frame of its {name} glow");
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        Facing = StartsFacing;
        (_doing, _speed) = (Doing.Waiting, Pixels.Zero);
        Play($"idle_{Side}");
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        Player samus = Room.Player;
        switch (_doing)
        {
            case Doing.Waiting:
                MoveDownBy(Pixels.FromWhole(1), SlopeTreatment.Climb);
                PixelOffset to = samus.Position - Position;
                if (Math.Abs(to.Y.Whole) >= StartsWithin.Y || Math.Abs(to.X.Whole) >= StartsWithin.X)
                    break;
                Play($"blink_{Side}", restart: true);
                (_doing, _timer) = (Doing.Blinking, 120);
                Sound(ActivationSound);
                break;

            case Doing.Blinking:
                if (--_timer <= 0)
                    BeginRun(afterStop: false);
                break;

            case Doing.Running:
                Run();
                break;

            case Doing.Charging:
                StepShineGlow();
                if (--_timer > 0)
                    break;
                Play($"shinespark_{Side}", restart: true);
                (_doing, _speed, _lift, _echoWait) = (Doing.Shinesparking, Pixels.Zero, Pixels.Zero, 0);
                Position -= PixelOffset.FromWhole(0, 8);
                Sound(LaunchSound);
                break;

            case Doing.Shinesparking:
                StepShineGlow();
                if (EchoEffect.Length > 0 && --_echoWait <= 0)
                {
                    _echoWait = 2;
                    Room.Spawn(Effect.Of(FrameTimeline.Read(Room.Content.ReadData(EchoEffect)), Position));
                }

                // It gathers speed faster and faster on the way up.
                _lift += Pixels.FromNumber(Gravity);
                _speed += _lift;
                Pixels rise = _speed.Whole >= 15 ? _speed.WithWhole(15) : _speed;
                if (!MoveDownBy(-rise))
                    break;
                Facing = Facing.Opposite();
                Play($"falling_{Side}", restart: true);
                (_doing, _speed) = (Doing.Falling, Pixels.Zero);
                ShowColours([]);
                Sound(CeilingSound);
                break;

            case Doing.Falling:
                _speed += Pixels.FromNumber(Gravity);
                if (_speed >= FastestFall)
                    _speed = FastestFall;
                if (MoveDownBy(_speed))
                    BeginRun(afterStop: true);
                break;

            default:
                throw new InvalidOperationException($"The Dachora has no step {_doing}.");
        }
    }

    /// <summary>Starts a run from rest: after a wall or the ceiling stopped it, or not.</summary>
    private void BeginRun(bool afterStop)
    {
        (_doing, _speed, _glowing, _runAfterStop) = (Doing.Running, Pixels.Zero, 0, afterStop);
        Play($"running_{Side}", restart: true);
        ShowColours([]);
    }

    /// <summary>One update of its run: it speeds up, follows the ground, and turns or shoots up where its run ends.</summary>
    private void Run()
    {
        if (_speed >= TopSpeed)
        {
            // At full speed it glows the colours of speed, as Samus does, brighter and brighter; a run from its blink
            // or its turn is heard as it reaches full speed and glows at once, one after a stop only much later.
            int update = ++_glowing;
            if (update == 1 && !_runAfterStop)
                Sound(SpeedSound);
            if (_runAfterStop && update == GlowAfterStopStrayUpdate)
                throw new InvalidOperationException($"The Dachora has run {update} updates at full speed after a stop, where the cartridge shows a frame its speed glow does not have.");
            int firstUpdate = _runAfterStop ? GlowAfterStopFirstUpdate : 1;
            if (update >= firstUpdate)
                ShowColours(_speedGlow[Math.Min((update - firstUpdate) / SpeedGlowFrameUpdates, _speedGlow.Length - 1)]);
        }

        MoveDownBy(Pixels.FromWhole(1), SlopeTreatment.Climb);
        if (_speed < TopSpeed)
        {
            _speed += Acceleration;
            if (_speed == FastSpeed)
                Play($"running_fast_{Side}", restart: true);
            else if (_speed == TopSpeed)
                Play($"running_very_fast_{Side}", restart: true);
        }

        bool right = Facing == Facing.Right;
        bool hitWall = MoveAcrossBy(right ? _speed : -_speed, SlopeTreatment.Climb);
        if (right)
        {
            if (hitWall)
            {
                Sound(SpeedStopSound);
                Facing = Facing.Left;
                BeginRun(afterStop: true);
                return;
            }

            if (Position.X.Whole < ShootsUpAt)
                return;
            Play($"charge_{Side}", restart: true);
            (_doing, _timer, _speed, _glowing) = (Doing.Charging, 60, Pixels.Zero, 0);
            Position += PixelOffset.FromWhole(0, 8);
            Sound(ChargeSound);
            return;
        }

        if (!hitWall && Position.X.Whole >= TurnsBackAt)
            return;
        Facing = Facing.Right;
        BeginRun(afterStop: false);
    }

    /// <summary>Shows the shine glow's next frame, each update it gathers itself and flies: its frames over and over.</summary>
    private void StepShineGlow() => ShowColours(_shineGlow[_glowing++ % _shineGlow.Length]);

    /// <summary>Shows its pictures with the colours given changed; none shows them in their own colours.</summary>
    private void ShowColours(IReadOnlyList<ColourSwap> swaps)
    {
        foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
            visual.Recolour(nameof(Dachora), swaps);
    }
}
