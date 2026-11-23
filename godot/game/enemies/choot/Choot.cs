using Engine.Actors;
using Engine.Actors.Movement;
using Engine.Content;
using Engine.Shared;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Choot: it rests until Samus comes near across, waits a moment, springs straight up, and then floats back down
/// like a leaf, swinging from side to side, to where it started. Nothing in the room stops it.
/// </summary>
/// <remarks>
/// Its data lists its ways of floating down under "falling_paths": each has "loop_fall", how far one swing of the
/// path brings it down, and "samples", the places of one swing from where the swing began, each with the updates it
/// is held for. Its timelines are "idle", "jumping" and "falling".
/// </remarks>
public partial class Choot : Enemy
{
    /// <summary>The way it floats down, by the name of one of its falling paths.</summary>
    [Export]
    public string FallingPath { get; set; } = "";

    /// <summary>How many swings of its path it floats down through; its spring carries it that high.</summary>
    [Export]
    public int PatternLoops { get; set; } = 1;

    /// <summary>How many updates it waits, once Samus is near, before it springs.</summary>
    [Export]
    public int JumpDelay { get; set; } = 16;

    /// <summary>How near across Samus must come, in pixels, to set it off.</summary>
    [Export, DataValue("reach")]
    public int Reach { get; set; } = 80;

    /// <summary>The data file of the speed curve its spring follows.</summary>
    [Export, DataValue("speed_curve")]
    public string SpeedCurveFile { get; set; } = "";

    /// <summary>How far along the curve its spring moves each update, in 256ths of a mark.</summary>
    [Export, DataValue("ascent_step")]
    public int AscentStep { get; set; } = 256;

    private sealed record Path(int LoopFall, IReadOnlyList<(PixelOffset Offset, int Updates)> Samples);

    private enum Doing
    {
        Resting,
        Poised,
        Springing,
        Floating,
    }

    private readonly Dictionary<string, Path> _paths = new(StringComparer.Ordinal);
    private SpeedCurve? _curve;
    private Doing _doing;
    private Countdown _wait;
    private PixelPosition _home;
    private PixelPosition _swingFrom;
    private int _launchPlace;
    private int _place;
    private int _sample;
    private int _heldFor;
    private int _swingsLeft;

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _paths.Clear();
        foreach ((string name, DataNode path) in data.Get("falling_paths").Entries())
        {
            var samples = path.List("samples")
                .Select(sample => (PixelOffset.FromWhole(sample.Int("x"), sample.Int("y")), sample.Int("updates"))).ToList();
            _paths[name] = new Path(path.Int("loop_fall"), samples);
        }
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        SpeedCurve curve = SpeedCurve.Read(Room.Content.ReadData(SpeedCurveFile));
        _curve = curve;
        _home = PixelPosition.FromWhole(Position.X.Whole, Position.Y.Whole);
        // Where on the curve its spring starts: the first place from which the speeds passed through carry it as
        // high as it will float down from.
        int height = PatternLoops * ThePath.LoopFall;
        Pixels risen = Pixels.Zero;
        for (_launchPlace = AscentStep; ; _launchPlace += AscentStep)
        {
            risen += curve.SpeedAt(_launchPlace);
            if (risen.Whole >= height)
                break;
        }

        _place = _launchPlace;
        Play("idle");
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        switch (_doing)
        {
            case Doing.Resting:
                if (Math.Abs(Target.X.Whole - Position.X.Whole) >= Reach)
                    return;
                _wait = Countdown.Of(JumpDelay + 1);
                _doing = Doing.Poised;
                return;

            case Doing.Poised:
                _wait = _wait.Stepped();
                if (_wait.IsRunning)
                    return;
                Play("jumping", restart: true);
                _doing = Doing.Springing;
                return;

            case Doing.Springing:
                Position -= new PixelOffset(Pixels.Zero, (_curve ?? throw NotInRoom()).SpeedAt(_place));
                _place -= AscentStep;
                if (_place >= 0)
                    return;
                _swingFrom = _home - PixelOffset.FromWhole(0, PatternLoops * ThePath.LoopFall);
                Position = _swingFrom;
                _sample = 0;
                _heldFor = 0;
                _swingsLeft = Math.Max(1, PatternLoops);
                Play("falling", restart: true);
                _doing = Doing.Floating;
                return;

            case Doing.Floating:
                Path path = ThePath;
                if (_sample < path.Samples.Count)
                {
                    Position = _swingFrom + path.Samples[_sample].Offset;
                    if (++_heldFor >= path.Samples[_sample].Updates)
                    {
                        _sample++;
                        _heldFor = 0;
                    }

                    return;
                }

                // A swing is done: the next begins that much lower, and after the last it is home again.
                _swingFrom += PixelOffset.FromWhole(0, path.LoopFall);
                _sample = 0;
                _heldFor = 0;
                if (--_swingsLeft > 0)
                    return;
                _place = _launchPlace;
                Position = _home;
                Play("idle", restart: true);
                _doing = Doing.Resting;
                return;

            default:
                throw new InvalidOperationException($"Undefined Choot stage {_doing}.");
        }
    }

    /// <summary>The path it floats down by.</summary>
    private Path ThePath =>
        _paths.TryGetValue(FallingPath, out Path? path) ? path : throw new InvalidOperationException($"The Choot \"{Name}\" has no falling path named \"{FallingPath}\".");

    /// <summary>The error for using the Choot before it is in a room.</summary>
    private InvalidOperationException NotInRoom() => new($"The Choot \"{Name}\" is not in a room.");
}
