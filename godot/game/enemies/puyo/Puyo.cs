using Engine.Actors;
using Engine.Actors.Movement;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Puyo: a blob that hops toward Samus, resting between hops. Near her it hops higher, and chance makes its next
/// hop small, big or long. A hop that meets a wall or a ceiling drops straight down, bounces once, and is followed by
/// one giant hop before it goes back to its ordinary ones.
/// </summary>
/// <remarks>
/// Its hops are listed in its data under "hops", each with its height, its speed across, the step it searches the
/// curve by for its launch, the step it moves along the curve by, and the speed it drops at. Its timelines are
/// "grounded_slow", "grounded_medium", "grounded_fast", "upright", "tilted_left", "tilted_right", "tilted_far_left"
/// and "tilted_far_right".
/// </remarks>
public partial class Puyo : Enemy
{
    /// <summary>How many updates it rests between hops.</summary>
    [Export]
    public int HopCooldown { get; set; } = 32;

    /// <summary>How near across Samus must be, in pixels, for its ordinary hop to be the big one.</summary>
    [Export]
    public int BigHopDistance { get; set; } = 96;

    /// <summary>The data file of the speed curve its hops follow.</summary>
    [Export, DataValue("speed_curve")]
    public string SpeedCurveFile { get; set; } = "";

    private const string Small = "small", Big = "big", Long = "long", Giant = "giant",
        DroppedSmall = "dropped_small", DroppedBig = "dropped_big";

    private sealed record HopValues(int Height, int Across, int SearchStep, int CurveStep, Pixels DropSpeed);

    private enum Doing
    {
        Grounded,
        Rising,
        Falling,
    }

    /// <summary>
    /// How its moves are tried, as the cartridge moves an enemy: only where each would end, and sloped ground that holds
    /// things up by their middle met under its middle alone.
    /// </summary>
    private static readonly MoveOptions Moves =
        MoveOptions.AmongBlocks with { Tries = WayTried.WhereItEnds, Slopes = SlopeTreatment.ClimbUnderMiddle, RestsAgainstSlopes = true };

    /// <summary>How a move across is tried: as its other moves, and such sloped ground is no part of it.</summary>
    private static readonly MoveOptions MovesAcross = Moves with { Slopes = SlopeTreatment.IgnoreMiddleHeld };

    private readonly Dictionary<string, HopValues> _hops = new(StringComparer.Ordinal);
    private SpeedCurve? _curve;
    private Doing _doing;
    private Countdown _rest;
    private string _next = Small;
    private string _hop = Small;
    private int _place;
    private int _launchPlace;
    private Facing? _turnBack;

    /// <summary>Whether a wall or a ceiling has cut its hop short, so that it drops at that hop's drop speed.</summary>
    private bool _dropping;

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _hops.Clear();
        foreach ((string name, DataNode hop) in data.Get("hops").Entries())
        {
            _hops[name] = new HopValues(
                hop.Int("height"), hop.Int("across"), hop.Int("search_step"), hop.Int("curve_step"), hop.Distance("drop_speed"));
        }
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _curve = SpeedCurve.Read(Room.Content.ReadData(SpeedCurveFile));
        _rest = Countdown.Of(HopCooldown + 1);
        Facing = Facing.Right;
        Play("grounded_fast");
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        switch (_doing)
        {
            case Doing.Grounded:
                _rest = _rest.Stepped();
                if (_rest.IsRunning)
                    return;
                _doing = Doing.Rising;
                _rest = Countdown.Of(HopCooldown + 1);
                BeginHop();
                return;
            case Doing.Rising:
                Fly(rising: true);
                return;
            case Doing.Falling:
                Fly(rising: false);
                return;
            default:
                throw new InvalidOperationException($"Undefined Puyo stage {_doing}.");
        }
    }

    /// <summary>Settles which hop comes now and which follows, turns it toward Samus, and finds where on the curve the hop starts.</summary>
    private void BeginHop()
    {
        bool ordinary = _next is Small or Big or Long;
        if (ordinary)
            _next = Math.Abs(Target.X.Whole - Position.X.Whole) < BigHopDistance ? Big : Small;
        Facing = _turnBack ?? TowardPlayer;
        _turnBack = null;

        // One number is drawn for every hop, and settles an ordinary one together with how long the Puyo has lived.
        RandomDraw drawn = Random.Draw();
        if (ordinary)
        {
            if (_next == Small)
                _next = ((drawn.Part(0, 1) + Updates) & 1) == 0 ? Small : Big;
            else
                _next = ((drawn.Part(0, 3) + Updates) & 7) switch { 0 => Small, 1 => Big, _ => Long };
        }

        _hop = _next;
        _dropping = false;
        HopValues hop = _hops[_hop];
        SpeedCurve curve = Curve;
        Pixels height = Pixels.FromWhole(hop.Height), risen = Pixels.Zero;
        int place = 0;
        do
        {
            place += hop.SearchStep;
            Pixels speed = curve.SpeedAt(place);
            risen += speed.WithFraction(speed.Fraction & ~0xFF);
        }
        while (risen <= height);
        _place = place;
        _launchPlace = place;
    }

    /// <summary>Moves one update of a hop: up or down along the curve, and across.</summary>
    private void Fly(bool rising)
    {
        if (_dropping)
        {
            if (!Drop(_hops[_hop].DropSpeed))
                return;
            // Landed from its drop, chance picks the size of the bounce that follows.
            _next = ((Random.Draw().Part(0, 1) + Updates) & 1) == 1 ? DroppedBig : DroppedSmall;
            _doing = Doing.Grounded;
            return;
        }

        // Its picture on the hop's end is the hop's own, whether it landed or a wall or a ceiling has just started its
        // drop; only the drop then goes on.
        if (!Move(rising))
            return;
        switch (_hop)
        {
            case Small:
                Play("grounded_slow", restart: true);
                break;
            case Big:
                Play("grounded_medium", restart: true);
                break;
            case Long:
                Play("grounded_fast", restart: true);
                break;
            case Giant:
                if (_turnBack is null)
                {
                    _next = Small;
                    _doing = Doing.Grounded;
                }

                Play("grounded_slow", restart: true);
                break;
            default:
                // A bounce after a drop is followed by the giant hop.
                _next = Giant;
                _doing = Doing.Grounded;
                Play("grounded_slow", restart: true);
                break;
        }
    }

    /// <summary>Moves the hop one update. Returns whether the hop is over: landed, or turned into a drop.</summary>
    private bool Move(bool rising)
    {
        HopValues hop = _hops[_hop];
        SpeedCurve curve = Curve;
        Pixels speed = curve.SpeedAt(Math.Min(_place, 64 * SpeedCurve.PartsPerMark));
        if (Drop(rising ? -speed : speed))
        {
            if (rising)
            {
                // Meeting a ceiling, it turns back for its next hop: on the cartridge that hangs on what was last
                // drawn, which in every update of play says to turn back.
                _turnBack = Facing.Opposite();
                BeginDrop();
            }
            else
            {
                _doing = Doing.Grounded;
            }

            return true;
        }

        ShowTilt(rising);
        if (rising)
        {
            _place -= hop.CurveStep;
            if (_place < 0)
            {
                _doing = Doing.Falling;
                _place = 0;
            }
        }
        else
        {
            _place += hop.CurveStep;
            if (_place >= 128 * SpeedCurve.PartsPerMark)
                _place = 0;
        }

        if (!Room.Mover.MoveAcross(MovingBody(), Facing.Toward(Pixels.FromWhole(hop.Across)), MovesAcross).Blocked)
            return false;
        _turnBack = Facing.Opposite();
        BeginDrop();
        return true;
    }

    /// <summary>Turns the hop into a straight drop.</summary>
    private void BeginDrop() => _dropping = true;

    /// <summary>Shows it leaning into the hop: far over while it is fastest, less so as it slows, upright at the top.</summary>
    private void ShowTilt(bool rising)
    {
        bool leansRight = rising == (Facing == Facing.Right);
        int half = _launchPlace / 2, threeQuarters = _launchPlace / 4 + half;
        Play(_place >= threeQuarters ? (leansRight ? "tilted_far_right" : "tilted_far_left")
            : _place >= half ? (leansRight ? "tilted_right" : "tilted_left")
            : "upright");
    }

    /// <summary>Moves it down by <paramref name="distance"/>, upward when less than nothing; returns whether something stopped it.</summary>
    private bool Drop(Pixels distance) => Room.Mover.MoveDown(MovingBody(), distance, Moves).Blocked;

    /// <summary>The collider it moves by.</summary>
    private Collider MovingBody() => Body ?? throw new InvalidOperationException($"The Puyo \"{Name}\" has no body collider.");

    /// <summary>The speed curve, once the Puyo is in its room.</summary>
    private SpeedCurve Curve => _curve ?? throw new InvalidOperationException($"The Puyo \"{Name}\" is not in a room.");
}
