using Engine.Actors;
using Engine.Actors.Movement;
using Engine.Content;
using Engine.Shared;
using Game.Compatibility;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Geega: a pipe bug that flies out sideways. When Samus is in front of its pipe it waits a moment and flies out
/// at her; as it nears her it dips and climbs back in one swoop, and flies on. Out of view, it is back in its pipe.
/// </summary>
/// <remarks>
/// Its timelines are "flying_left", "flying_right", "arcing_left" and "arcing_right". Its swoop follows the speed
/// curve its data names.
/// </remarks>
public partial class Geega : Enemy
{
    /// <summary>How far it flies each update, as a number of pixels.</summary>
    [Export]
    public double Speed { get; set; } = 2;

    /// <summary>How far in front of it Samus may be, in pixels, and still bring it out.</summary>
    [Export, DataValue("trigger_distance")]
    public int TriggerDistance { get; set; } = 192;

    /// <summary>How near Samus's height must be to its own, in pixels, to bring it out.</summary>
    [Export, DataValue("trigger_height")]
    public int TriggerHeight { get; set; } = 48;

    /// <summary>How many updates it waits before flying out.</summary>
    [Export, DataValue("emergence_delay")]
    public int EmergenceDelay { get; set; } = 24;

    /// <summary>How near across it must come to Samus, in pixels, to start its swoop.</summary>
    [Export, DataValue("swoop_distance")]
    public int SwoopDistance { get; set; } = 48;

    /// <summary>The mark of the speed curve its swoop starts from.</summary>
    [Export, DataValue("swoop_start_mark")]
    public int SwoopStartMark { get; set; } = 40;

    /// <summary>The data file of the speed curve its swoop follows.</summary>
    [Export, DataValue("speed_curve")]
    public string SpeedCurveFile { get; set; } = "";

    private enum Doing
    {
        WaitingForSamus,
        AboutToFly,
        Flying,
        FlyingOn,
        Dipping,
        Climbing,
    }

    private SpeedCurve? _curve;
    private Doing _doing;
    private PixelPosition _pipe;
    private Countdown _delay;
    private int _place;

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _curve = SpeedCurve.Read(Room.Content.ReadData(SpeedCurveFile));
        _pipe = PixelPosition.FromWhole(Position.X.Whole, Position.Y.Whole);
        Position = _pipe;
        Play(Flying);
        Visual?.SetShown(false);
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        SpeedCurve curve = _curve ?? throw new InvalidOperationException($"The Geega \"{Name}\" is not in a room.");
        var across = new PixelOffset(Facing.Toward(Pixels.FromNumber(Speed)), Pixels.Zero);
        switch (_doing)
        {
            case Doing.WaitingForSamus:
                int ahead = Target.X.Whole - Position.X.Whole;
                bool inFront = Facing == Facing.Left ? ahead < 0 && ahead >= -TriggerDistance : ahead >= 0 && ahead < TriggerDistance;
                if (!inFront || Math.Abs(Position.Y.Whole - Target.Y.Whole) >= TriggerHeight)
                    return;
                Visual?.SetShown(true);
                _delay = Countdown.Of(EmergenceDelay);
                _doing = Doing.AboutToFly;
                return;

            case Doing.AboutToFly:
                if (_delay.IsOver)
                    return;
                _delay = _delay.Stepped();
                if (_delay.IsRunning)
                    return;
                Play(Flying, restart: true);
                _doing = Doing.Flying;
                return;

            case Doing.Flying or Doing.FlyingOn:
                Position += across;
                if (GoneBackToPipe() || _doing == Doing.FlyingOn)
                    return;
                int toSamus = Facing == Facing.Left ? Position.X.Whole - Target.X.Whole : Target.X.Whole - Position.X.Whole;
                if (toSamus >= SwoopDistance)
                    return;
                _doing = Doing.Dipping;
                _place = SwoopStartMark * SpeedCurve.PartsPerMark;
                Play(Facing == Facing.Left ? "arcing_left" : "arcing_right", restart: true);
                return;

            case Doing.Dipping:
                if (GoneBackToPipe())
                    return;
                Position += across;
                if (_place == 0)
                {
                    _doing = Doing.Climbing;
                    return;
                }

                _place -= SpeedCurve.PartsPerMark;
                Position += new PixelOffset(Pixels.Zero, curve.SpeedAt(_place));
                return;

            case Doing.Climbing:
                if (GoneBackToPipe())
                    return;
                Position += across;
                _place += SpeedCurve.PartsPerMark;
                Position -= new PixelOffset(Pixels.Zero, curve.SpeedAt(_place));
                if (Facing == Facing.Left ? Position.Y.Whole >= _pipe.Y.Whole : _pipe.Y.Whole < Position.Y.Whole)
                    return;
                _doing = Doing.FlyingOn;
                Play(Flying, restart: true);
                return;

            default:
                throw new InvalidOperationException($"Undefined Geega stage {_doing}.");
        }
    }

    /// <summary>The name of its flying timeline for the way it faces.</summary>
    private string Flying => Facing == Facing.Left ? "flying_left" : "flying_right";

    /// <summary>Puts it back in its pipe if its middle has left the enemies' view, and returns whether it did.</summary>
    private bool GoneBackToPipe()
    {
        if (EnemyView.Contains(Room, Position.X.Whole, Position.Y.Whole))
            return false;
        Position = _pipe;
        _doing = Doing.WaitingForSamus;
        Play(Flying, restart: true);
        Visual?.SetShown(false);
        return true;
    }
}
