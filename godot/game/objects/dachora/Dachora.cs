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

    /// <summary>
    /// Its glow cycle, as the cartridge counts it: the frame of its glow it shows next times 256, plus the updates left
    /// before it, all one number within 65536, so an update counted off no updates takes one from the frame and a step
    /// that passes 255 updates carries into the frame.
    /// </summary>
    private int _glowCycle;

    /// <summary>The updates one frame of the glow cycle is worth.</summary>
    private const int GlowUpdatesPerFrame = 256;

    /// <summary>Where a run starts the glow cycle: frame nought, one update away. Only a run that finds it there asks for the speed sound.</summary>
    private const int RunStartGlow = 1;

    /// <summary>One step of the speed glow, the next frame sixteen updates later; at the end it starts again from its brightest frame.</summary>
    private const int SpeedGlowStep = GlowUpdatesPerFrame + 16, SpeedGlowEnd = 4 * GlowUpdatesPerFrame + 16, SpeedGlowRestart = 3 * GlowUpdatesPerFrame + 16;

    /// <summary>One step of the shine glow, the next frame; at the end it starts again from frame nought.</summary>
    private const int ShineGlowStep = GlowUpdatesPerFrame, ShineGlowEnd = 4 * GlowUpdatesPerFrame;

    /// <summary>The frames each glow has.</summary>
    private const int GlowFrames = 4;

    private Doing _doing;
    private int _timer;
    private Pixels _speed;
    private Pixels _lift;
    private int _echoWait;

    private string Side => Facing == Facing.Right ? "right" : "left";

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        Facing = StartsFacing;
        (_doing, _speed, _glowCycle) = (Doing.Waiting, Pixels.Zero, 0);
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
                    BeginRun(RunStartGlow);
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
                (_doing, _speed, _glowCycle) = (Doing.Falling, Pixels.Zero, 0);
                Tint(Colors.White);
                Sound(CeilingSound);
                break;

            case Doing.Falling:
                _speed += Pixels.FromNumber(Gravity);
                if (_speed >= FastestFall)
                    _speed = FastestFall;
                // Landing, it runs on with the glow cycle the ceiling cleared, so that run asks for no speed sound.
                if (MoveDownBy(_speed))
                    BeginRun(_glowCycle);
                break;

            default:
                throw new InvalidOperationException($"The Dachora has no step {_doing}.");
        }
    }

    /// <summary>Starts a run from rest, its glow cycle at <paramref name="glowCycle"/>.</summary>
    private void BeginRun(int glowCycle)
    {
        (_doing, _speed, _glowCycle) = (Doing.Running, Pixels.Zero, glowCycle);
        Play($"running_{Side}", restart: true);
        Tint(Colors.White);
    }

    /// <summary>One update of its run: it speeds up, follows the ground, and turns or shoots up where its run ends.</summary>
    private void Run()
    {
        if (_speed >= TopSpeed)
        {
            // At full speed it glows, as Samus does. The speed sound is asked for on the update that finds the glow
            // cycle where a run starts it; a run that began with the cycle cleared never finds it there.
            if (_glowCycle == RunStartGlow)
                Sound(SpeedSound);
            _glowCycle = (_glowCycle - 1) & 0xFFFF;
            if (_glowCycle % GlowUpdatesPerFrame == 0)
            {
                ShowGlow(shining: false, _glowCycle / GlowUpdatesPerFrame);
                _glowCycle = (_glowCycle + SpeedGlowStep) & 0xFFFF;
                if (unchecked((short)(_glowCycle - SpeedGlowEnd)) >= 0)
                    _glowCycle = SpeedGlowRestart;
            }
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
                // Turned back by a wall, it runs with the glow cycle cleared.
                Sound(SpeedStopSound);
                Facing = Facing.Left;
                BeginRun(0);
                return;
            }

            if (Position.X.Whole < ShootsUpAt)
                return;
            Play($"charge_{Side}", restart: true);
            (_doing, _timer, _speed, _glowCycle) = (Doing.Charging, 60, Pixels.Zero, 0);
            Position += PixelOffset.FromWhole(0, 8);
            Sound(ChargeSound);
            return;
        }

        if (!hitWall && Position.X.Whole >= TurnsBackAt)
            return;
        Facing = Facing.Right;
        BeginRun(RunStartGlow);
    }

    /// <summary>Shows the shine glow's next frame and steps its cycle on, each update it gathers itself and flies.</summary>
    private void StepShineGlow()
    {
        ShowGlow(shining: true, _glowCycle / GlowUpdatesPerFrame);
        _glowCycle = (_glowCycle + ShineGlowStep) & 0xFFFF;
        if (unchecked((short)(_glowCycle - ShineGlowEnd)) >= 0)
            _glowCycle = 0;
    }

    /// <summary>It glows: the blue of speed while it runs, the gold of the shinespark while it gathers and flies.</summary>
    private void ShowGlow(bool shining, int frame)
    {
        if (frame >= GlowFrames)
            throw new InvalidOperationException($"The Dachora's glow has no frame {frame}.");
        float pulse = 1f + 0.15f * frame;
        Tint(shining ? new Color(pulse * 1.2f, pulse * 1.2f, pulse * 0.8f) : new Color(pulse * 0.9f, pulse, pulse * 1.3f));
    }

    private void Tint(Color colour)
    {
        foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
            visual.Tint(colour);
    }
}
