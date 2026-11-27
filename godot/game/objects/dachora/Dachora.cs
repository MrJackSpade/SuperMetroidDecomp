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
/// "falling", each ending in "_left" or "_right" for the way it faces. As it shoots up it leaves a trail of echoes of
/// itself, the visuals under its node "Echoes", each one place in the trail; they play its "shinespark" timeline.
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

    /// <summary>How many updates each echo it leaves as it shoots up lasts.</summary>
    [Export, DataValue("echo_updates")]
    public int EchoUpdates { get; set; } = 8;

    /// <summary>How many updates apart it leaves its echoes as it shoots up.</summary>
    [Export, DataValue("echo_spacing")]
    public int EchoSpacing { get; set; } = 2;

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

    /// <summary>The visuals of its trail of echoes, one for each place in the trail.</summary>
    private ActorVisual[] _echoes = [];

    /// <summary>Where each echo of the trail was left.</summary>
    private PixelPosition[] _echoPlaces = [];

    /// <summary>The updates each echo of the trail has left; none for one that is gone.</summary>
    private int[] _echoLives = [];

    /// <summary>The updates before it leaves its next echo.</summary>
    private int _echoWait;

    /// <summary>The timeline its echoes play, from the update it shot up; null before it ever has.</summary>
    private TimelinePlayer? _echoAnimation;

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

        // Its echoes are its own pictures and lie as they do.
        _echoes = [.. GetNode("Echoes").GetChildren().OfType<ActorVisual>()];
        if (data.Has("picture_offset"))
        {
            var offset = new Vector2I(data.Get("picture_offset").Int("x"), data.Get("picture_offset").Int("y"));
            foreach (ActorVisual echo in _echoes)
                echo.PictureOffset = offset;
        }
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
        (_echoPlaces, _echoLives, _echoWait, _echoAnimation) = (new PixelPosition[_echoes.Length], new int[_echoes.Length], 0, null);
        foreach (ActorVisual echo in _echoes)
        {
            echo.Bind(Room.Content);
            echo.SetShown(false);
        }
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
                (_doing, _speed, _lift) = (Doing.Shinesparking, Pixels.Zero, Pixels.Zero);
                Position -= PixelOffset.FromWhole(0, 8);
                Sound(LaunchSound);
                // Its trail begins again: every echo gone, and all of them playing the timeline afresh.
                (_echoWait, _echoAnimation) = (0, new TimelinePlayer(Timelines.Get($"shinespark_{Side}")));
                Array.Clear(_echoLives);
                break;

            case Doing.Shinesparking:
                StepShineGlow();
                LeaveEcho();

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

        StepEchoes();
    }

    /// <summary>
    /// Leaves an echo where it stands, before it moves, once every so many updates as it shoots up: in the first
    /// place of its trail whose echo is gone. With none gone, it leaves none.
    /// </summary>
    private void LeaveEcho()
    {
        if (_echoWait > 0)
        {
            _echoWait--;
            return;
        }

        _echoWait = EchoSpacing - 1;
        int place = Array.IndexOf(_echoLives, 0);
        if (place < 0)
            return;
        (_echoPlaces[place], _echoLives[place]) = (Position, EchoUpdates);
    }

    /// <summary>
    /// One update of its echoes: each counts down its life, and while it lasts shows on the updates of one parity of
    /// the short count, the first place of the trail on odd ones, the next on even ones, and so on by turns; and all
    /// of them step on through the timeline they play.
    /// </summary>
    private void StepEchoes()
    {
        bool odd = Room.Session.Clock.ShortUpdates % 2 == 1;
        _echoAnimation?.Step();
        for (int place = 0; place < _echoes.Length; place++)
        {
            ActorVisual echo = _echoes[place];
            if (_echoLives[place] == 0)
            {
                echo.SetShown(false);
                continue;
            }

            _echoLives[place]--;
            echo.SetShown(odd == (place % 2 == 0));
            echo.Position = new Vector2(_echoPlaces[place].X.Whole - Position.X.Whole, _echoPlaces[place].Y.Whole - Position.Y.Whole);
            if (_echoAnimation is { } animation)
                echo.Show(animation.Shown);
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
        // At full speed a run from its blink or its turn is heard and glows the colours of speed, as Samus does,
        // brighter and brighter; a run after a wall or the ceiling stopped it is silent and shows no glow.
        if (_speed >= TopSpeed && !_runAfterStop)
        {
            int update = ++_glowing;
            if (update == 1)
                Sound(SpeedSound);
            ShowColours(_speedGlow[Math.Min((update - 1) / SpeedGlowFrameUpdates, _speedGlow.Length - 1)]);
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

    /// <summary>Shows its pictures with the colours given changed; none shows them in their own colours. Its echoes keep their own.</summary>
    private void ShowColours(IReadOnlyList<ColourSwap> swaps)
    {
        foreach (ActorVisual visual in GetChildren().OfType<ActorVisual>())
            visual.Recolour(nameof(Dachora), swaps);
    }
}
