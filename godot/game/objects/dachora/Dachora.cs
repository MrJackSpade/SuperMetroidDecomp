using Engine.Actors;
using Engine.Actors.Components;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
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

    /// <summary>The frames each glow has.</summary>
    private const int GlowFrames = 4;

    /// <summary>The updates each frame of the speed glow shows before the next; it stays on its brightest.</summary>
    private const int SpeedGlowFrameUpdates = 16;

    private Doing _doing;
    private int _timer;
    private Pixels _speed;
    private Pixels _lift;
    private int _echoWait;

    /// <summary>The updates it has glowed for, since its run reached full speed or since it began to gather itself.</summary>
    private int _glowing;

    private string Side => Facing == Facing.Right ? "right" : "left";

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
                    BeginRun();
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
                Tint(Colors.White);
                Sound(CeilingSound);
                break;

            case Doing.Falling:
                _speed += Pixels.FromNumber(Gravity);
                if (_speed >= FastestFall)
                    _speed = FastestFall;
                if (MoveDownBy(_speed))
                    BeginRun();
                break;

            default:
                throw new InvalidOperationException($"The Dachora has no step {_doing}.");
        }
    }

    /// <summary>Starts a run from rest.</summary>
    private void BeginRun()
    {
        (_doing, _speed, _glowing) = (Doing.Running, Pixels.Zero, 0);
        Play($"running_{Side}", restart: true);
        Tint(Colors.White);
    }

    /// <summary>One update of its run: it speeds up, follows the ground, and turns or shoots up where its run ends.</summary>
    private void Run()
    {
        if (_speed >= TopSpeed)
        {
            // At full speed it glows, as Samus does, brighter and brighter, and is heard.
            if (_glowing == 0)
                Sound(SpeedSound);
            ShowGlow(shining: false, Math.Min(_glowing / SpeedGlowFrameUpdates, GlowFrames - 1));
            _glowing++;
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
                BeginRun();
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
        BeginRun();
    }

    /// <summary>Shows the shine glow's next frame, each update it gathers itself and flies: its frames over and over.</summary>
    private void StepShineGlow() => ShowGlow(shining: true, _glowing++ % GlowFrames);

    /// <summary>It glows: the blue of speed while it runs, the gold of the shinespark while it gathers and flies.</summary>
    private void ShowGlow(bool shining, int frame)
    {
        float pulse = 1f + 0.15f * frame;
        Tint(shining ? new Color(pulse * 1.2f, pulse * 1.2f, pulse * 0.8f) : new Color(pulse * 0.9f, pulse, pulse * 1.3f));
    }

    private void Tint(Color colour)
    {
        foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
            visual.Tint(colour);
    }
}
