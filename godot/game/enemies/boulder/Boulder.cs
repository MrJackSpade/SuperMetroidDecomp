using Engine.Actors;
using Engine.Actors.Movement;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Boulder: it hangs out of sight until Samus passes below and ahead of it, then drops, bounces a few times, each
/// lower than the last, and rolls on, gathering speed, until it breaks against a wall. Some break on their first
/// landing, and some roll from the start.
/// </summary>
/// <remarks>Its timelines are "left" and "right". Its speeds, down and across, come from the speed curve its data names.</remarks>
public partial class Boulder : Enemy
{
    /// <summary>How far below it Samus may be, in pixels, and still set it off.</summary>
    [Export]
    public int TriggerHeight { get; set; } = 256;

    /// <summary>How far ahead of it across Samus may be, in pixels, and still set it off.</summary>
    [Export]
    public int TriggerReach { get; set; } = 64;

    /// <summary>How far above where it is placed it hangs, in pixels, before it drops. None, and it rolls from the start.</summary>
    [Export]
    public int DropHeight { get; set; }

    /// <summary>The way it rolls.</summary>
    [Export]
    public Facing Rolls { get; set; } = Facing.Left;

    /// <summary>Whether it breaks the first time it lands rather than bouncing on.</summary>
    [Export]
    public bool BreaksOnLanding { get; set; }

    /// <summary>The mark of the curve its speeds while bouncing rise to and stay at.</summary>
    [Export, DataValue("fastest_bouncing_mark")]
    public int FastestBouncingMark { get; set; } = 80;

    /// <summary>The mark of the curve its rebound from its first drop starts at.</summary>
    [Export, DataValue("first_rebound_mark")]
    public int FirstReboundMark { get; set; } = 32;

    /// <summary>How far along the curve its speed across moves each update while it bounces, in 256ths of a mark.</summary>
    [Export, DataValue("bouncing_travel_step")]
    public int BouncingTravelStep { get; set; } = 32;

    /// <summary>How far along the curve its speed across moves each update while it rolls, in 256ths of a mark.</summary>
    [Export, DataValue("rolling_travel_step")]
    public int RollingTravelStep { get; set; } = 64;

    /// <summary>The mark of the curve its speed across while rolling rises to and stays at.</summary>
    [Export, DataValue("fastest_rolling_mark")]
    public int FastestRollingMark { get; set; } = 64;

    /// <summary>How far it reaches down for the ground while it rolls, in pixels, until it has found level ground.</summary>
    [Export, DataValue("ground_reach")]
    public int GroundReach { get; set; } = 2;

    /// <summary>The effect it leaves when it breaks, by the data file of its timeline.</summary>
    [Export, DataValue("break_effect")]
    public string BreakEffect { get; set; } = "";

    /// <summary>The sound of it landing, by name.</summary>
    [Export, DataValue("impact_sound")]
    public string ImpactSound { get; set; } = "";

    /// <summary>The sound of it breaking, by name.</summary>
    [Export, DataValue("break_sound")]
    public string BreakSound { get; set; } = "";

    /// <summary>The data file of the speed curve its speeds come from.</summary>
    [Export, DataValue("speed_curve")]
    public string SpeedCurveFile { get; set; } = "";

    private enum Doing
    {
        Waiting,
        FirstDrop,
        Rebounding,
        Falling,
        Rolling,
    }

    private readonly List<int> _reboundMarks = [];
    private SpeedCurve? _curve;
    private Doing _doing;
    private int _restsAt;
    private Pixels _lastY;
    private int _bouncePlace;
    private int _travelPlace;
    private int _landings;
    private int _reach;

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        // The mark each later rebound starts at, one for each landing; after the last it rolls.
        _reboundMarks.Clear();
        _reboundMarks.AddRange(data.List("rebound_marks").Select(mark => mark.AsInt()));
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _curve = SpeedCurve.Read(Room.Content.ReadData(SpeedCurveFile));
        _restsAt = Position.Y.Whole;
        _lastY = Position.Y;
        // One that rolls from the start still begins a pixel above where it is placed.
        Position -= PixelOffset.FromWhole(0, DropHeight > 0 ? DropHeight : 1);
        _reach = GroundReach;
        Play(Rolls == Facing.Right ? "right" : "left");
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        SpeedCurve curve = _curve ?? throw new InvalidOperationException($"The Boulder \"{Name}\" is not in a room.");
        int fastestBouncing = FastestBouncingMark * SpeedCurve.PartsPerMark;
        switch (_doing)
        {
            case Doing.Waiting:
                int below = Target.Y.Whole - Position.Y.Whole;
                if (below < 0 || below >= TriggerHeight)
                    return;
                int across = Target.X.Whole - Position.X.Whole;
                bool ahead = Rolls == Facing.Left ? across < 0 && -across <= TriggerReach : across >= 0 && across < TriggerReach;
                if (ahead)
                    _doing = DropHeight > 0 ? Doing.FirstDrop : Doing.Rolling;
                return;

            case Doing.FirstDrop:
                // Its first drop passes through everything, down to where it was placed.
                Position += new PixelOffset(Pixels.Zero, curve.SpeedAt(_bouncePlace));
                if (Position.Y.Whole < _restsAt)
                {
                    _bouncePlace = Math.Min(_bouncePlace + SpeedCurve.PartsPerMark, fastestBouncing);
                    return;
                }

                Position = new PixelPosition(Position.X, Position.Y.WithWhole(_restsAt));
                _doing = Doing.Rebounding;
                _bouncePlace = FirstReboundMark * SpeedCurve.PartsPerMark;
                return;

            case Doing.Rebounding:
                Position -= new PixelOffset(Pixels.Zero, curve.SpeedAt(_bouncePlace));
                if (_bouncePlace < SpeedCurve.PartsPerMark)
                {
                    _bouncePlace = 0;
                    _doing = Doing.Falling;
                    return;
                }

                _bouncePlace -= SpeedCurve.PartsPerMark;
                Position += new PixelOffset(Rolls.Toward(curve.SpeedAt(_travelPlace)), Pixels.Zero);
                _travelPlace = Math.Min(_travelPlace + BouncingTravelStep, fastestBouncing);
                return;

            case Doing.Falling:
                if (Room.Mover.MoveDown(MovingBody(), curve.SpeedAt(_bouncePlace), Moves).Blocked)
                {
                    Sound(ImpactSound);
                    if (BreaksOnLanding)
                    {
                        Break();
                        return;
                    }

                    if (_landings >= _reboundMarks.Count)
                        throw new InvalidOperationException($"The Boulder \"{Name}\" has landed more often than its data lists rebounds for.");
                    _doing = Doing.Rebounding;
                    _bouncePlace = _reboundMarks[_landings] * SpeedCurve.PartsPerMark;
                    if (++_landings == _reboundMarks.Count)
                    {
                        _lastY = Position.Y;
                        _doing = Doing.Rolling;
                    }

                    return;
                }

                _bouncePlace += SpeedCurve.PartsPerMark;
                Position += new PixelOffset(Rolls.Toward(curve.SpeedAt(_travelPlace)), Pixels.Zero);
                _travelPlace = Math.Min(_travelPlace + BouncingTravelStep, fastestBouncing);
                return;

            case Doing.Rolling:
                // It presses down to stay on the ground, reaching a little further until the ground runs level.
                Room.Mover.MoveDown(MovingBody(), curve.SpeedAt(_travelPlace) + Pixels.FromWhole(_reach), Moves);
                Position -= PixelOffset.FromWhole(0, _reach);
                if (Room.Mover.MoveAcross(MovingBody(), Rolls.Toward(curve.SpeedAt(_travelPlace)), MovesAcross).Blocked)
                {
                    Sound(ImpactSound);
                    Break();
                    return;
                }

                _travelPlace = Math.Min(_travelPlace + RollingTravelStep, FastestRollingMark * SpeedCurve.PartsPerMark);
                if (Position.Y == _lastY)
                    _reach = 0;
                _lastY = Position.Y;
                return;

            default:
                throw new InvalidOperationException($"Undefined Boulder stage {_doing}.");
        }
    }

    /// <summary>
    /// How its moves are tried, as the cartridge moves an enemy: only where each would end, and sloped ground that holds
    /// things up by their middle met under its middle alone.
    /// </summary>
    private static readonly MoveOptions Moves =
        MoveOptions.AmongBlocks with { Tries = WayTried.WhereItEnds, Slopes = SlopeTreatment.ClimbUnderMiddle, RestsAgainstSlopes = true };

    /// <summary>How a move across is tried: as its other moves, and such sloped ground is no part of it.</summary>
    private static readonly MoveOptions MovesAcross = Moves with { Slopes = SlopeTreatment.IgnoreMiddleHeld };

    /// <summary>The collider it moves by.</summary>
    private Collider MovingBody() => Body ?? throw new InvalidOperationException($"The Boulder \"{Name}\" has no body collider.");

    /// <summary>
    /// Breaks the boulder up: it is gone, leaving its dust, one of the enemies' shots on the cartridge, which holds one
    /// of the room's places while it plays and is not made with none free.
    /// </summary>
    private void Break()
    {
        if (BreakEffect.Length > 0)
            Room.Spawn(Effect.Of(FrameTimeline.Read(Room.Content.ReadData(BreakEffect)), Position, holdsPlace: true));
        Sound(BreakSound);
        Vanish();
    }
}
