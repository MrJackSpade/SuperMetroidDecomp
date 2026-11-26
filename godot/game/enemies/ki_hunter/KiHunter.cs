using Engine.Actors;
using Engine.Actors.Components;
using Engine.Actors.Movement;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Game.Compatibility;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Ki-Hunter: a wasp. With its wings it patrols back and forth, bobbing, and swoops down in an arc through where
/// Samus stands. Hurt badly enough it loses its wings, which flutter to the ground, and from then on it hops after
/// her and spits acid when she is near. Some are placed without wings.
/// </summary>
/// <remarks>
/// Its timelines are "flying_left" and "flying_right"; "swoop_left" and "swoop_right", whose cue "return_to_flying"
/// puts it back to flying; "jump_left" and "jump_right", whose cue "hop" launches it; "land_left" and "land_right",
/// whose cue "think" sets it choosing what to do next; "spit_left" and "spit_right", whose cue "spit" lets the acid
/// go and whose "think" follows it; and "wings_left", "wings_right" and "detached_wings" for the visual named
/// "Wings". Its shot is "acid". Its loose wings turn by the speed curve its data names.
/// </remarks>
public partial class KiHunter : Enemy
{
    /// <summary>Whether it is placed already without its wings.</summary>
    [Export]
    public bool Wingless { get; set; }

    /// <summary>The health at or below which it loses its wings.</summary>
    [Export]
    public int WingsLostAtHealth { get; set; }

    /// <summary>How near across Samus must be, in pixels, for it to swoop.</summary>
    [Export, DataValue("swoop_distance")]
    public int SwoopDistance { get; set; } = 96;

    /// <summary>How far below it Samus must be, in pixels, for it to swoop.</summary>
    [Export, DataValue("swoop_least_depth")]
    public int SwoopLeastDepth { get; set; } = 32;

    /// <summary>How far it travels across each update on patrol, in pixels.</summary>
    [Export, DataValue("patrol_speed")]
    public int PatrolSpeed { get; set; } = 1;

    /// <summary>How far above where it is placed its patrol bobs, in pixels.</summary>
    [Export, DataValue("patrol_rise")]
    public int PatrolRise { get; set; } = 16;

    /// <summary>How tall its patrol's bobbing is, in pixels.</summary>
    [Export, DataValue("patrol_depth")]
    public int PatrolDepth { get; set; } = 32;

    /// <summary>How much faster its swoop turns each update, in 256ths of an angle step.</summary>
    [Export, DataValue("swoop_quickening")]
    public int SwoopQuickening { get; set; } = 32;

    /// <summary>The most angle steps its swoop turns in an update.</summary>
    [Export, DataValue("swoop_fastest_turn")]
    public int SwoopFastestTurn { get; set; } = 2;

    /// <summary>How many angle steps into its swoop it strikes.</summary>
    [Export, DataValue("strike_after_steps")]
    public int StrikeAfterSteps { get; set; } = 16;

    /// <summary>The downward speed it gains each update without wings, as a number of pixels.</summary>
    [Export, DataValue("gravity")]
    public double Gravity { get; set; } = 0.875;

    /// <summary>How far it travels across each update of a hop, in pixels.</summary>
    [Export, DataValue("hop_across_speed")]
    public int HopAcrossSpeed { get; set; } = 2;

    /// <summary>The upward speed its lowest hop starts with, in pixels; by chance one may start a pixel slower.</summary>
    [Export, DataValue("hop_up_speed")]
    public int HopUpSpeed { get; set; } = 8;

    /// <summary>How many updates it waits after landing before choosing what to do.</summary>
    [Export, DataValue("landing_wait")]
    public int LandingWait { get; set; } = 12;

    /// <summary>How many updates it waits after spitting before choosing what to do.</summary>
    [Export, DataValue("spit_wait")]
    public int SpitWait { get; set; } = 24;

    /// <summary>How near across Samus must be, in pixels, for it to spit rather than hop.</summary>
    [Export, DataValue("spit_range")]
    public int SpitRange { get; set; } = 96;

    /// <summary>How far in front of its middle its acid leaves from, in pixels.</summary>
    [Export, DataValue("spit_reach")]
    public int SpitReach { get; set; } = 22;

    /// <summary>How far above its middle its acid leaves from, in pixels.</summary>
    [Export, DataValue("spit_height")]
    public int SpitHeight { get; set; } = 16;

    /// <summary>The radius of the arcs its loose wings flutter down in, in pixels.</summary>
    [Export, DataValue("wing_arc_radius")]
    public int WingArcRadius { get; set; } = 48;

    /// <summary>How far down the first arc of its loose wings is aimed, in pixels, which sets how fast they start.</summary>
    [Export, DataValue("wing_arc_climb")]
    public int WingArcClimb { get; set; } = 32;

    /// <summary>How far along the curve its loose wings slow each update, in 256ths of a mark.</summary>
    [Export, DataValue("wing_arc_step")]
    public int WingArcStep { get; set; } = 384;

    /// <summary>Half the width of its loose wings, in pixels, for meeting the ground.</summary>
    [Export, DataValue("wing_half_width")]
    public int WingHalfWidth { get; set; } = 8;

    /// <summary>Half the height of its loose wings, in pixels, for meeting the ground.</summary>
    [Export, DataValue("wing_half_height")]
    public int WingHalfHeight { get; set; } = 8;

    /// <summary>The data file of the speed curve its loose wings turn by.</summary>
    [Export, DataValue("speed_curve")]
    public string SpeedCurveFile { get; set; } = "";

    private const int HalfTurn = Turns.PerTurn / 2;

    private enum Doing
    {
        Patrolling,
        Swooping,
        Striking,
        Recovering,
        Falling,
        StartHop,
        Posed,
        Hopping,
        Thinking,
        ChoosingSpit,
    }

    private enum WingState
    {
        Attached,
        Circling,
        Sinking,
        Gone,
    }

    private Doing _doing;
    private Facing _heading;
    private int _acrossSpeed;
    private Pixels _downSpeed;
    private int _upperY;
    private int _lowerY;
    private int _placedY;
    private int _swoopAngle;
    private int _swoopTurn;
    private int _swoopAcross;
    private int _swoopDown;
    private int _swoopTargetX;
    private int _swoopOriginY;
    private Countdown _wait;
    private Facing _spits;

    private SpeedCurve? _curve;
    private TimelinePlayer? _wingsPlayer;
    private WingState _wings;
    private int _wingAngle;
    private int _wingAngleParts;
    private int _wingPlace;
    private (int X, int Y) _wingAt;
    private (int X, int Y) _wingCentre;

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _curve = SpeedCurve.Read(Room.Content.ReadData(SpeedCurveFile));
        Play("flying_left");
        _heading = Facing.Left;
        _acrossSpeed = PatrolSpeed;
        _downSpeed = Pixels.FromWhole(PatrolSpeed);
        _upperY = Position.Y.Whole - PatrolRise;
        _lowerY = _upperY + PatrolDepth;
        _placedY = Position.Y.Whole;
        if (Wingless)
        {
            _wings = WingState.Gone;
            _doing = Doing.Falling;
            _downSpeed = Pixels.FromWhole(1);
            WingsVisual?.SetShown(false);
        }
        else
        {
            _wingsPlayer = new TimelinePlayer(Timelines.Get("wings_left"));
        }
    }

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        switch (cue)
        {
            case "return_to_flying":
                Play(_heading == Facing.Right ? "flying_right" : "flying_left", restart: true);
                if (_wings == WingState.Attached)
                    _wingsPlayer = new TimelinePlayer(Timelines.Get(_heading == Facing.Right ? "wings_right" : "wings_left"));
                break;
            case "hop":
                _doing = Doing.Hopping;
                break;
            case "think":
                _doing = Doing.Thinking;
                break;
            case "spit":
                PixelPosition mouth = PixelPosition.FromWhole(Position.X.Whole + (_spits == Facing.Left ? -SpitReach : SpitReach), Position.Y.Whole - SpitHeight);
                Fire("acid", mouth, new PixelVelocity(_spits.Toward(Shot("acid").Speed), Pixels.Zero));
                _wait = Countdown.Of(SpitWait);
                break;
        }
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        switch (_doing)
        {
            case Doing.Patrolling:
                Patrol();
                break;

            case Doing.Swooping:
                // A sixteenth of a turn into its arc, it strikes.
                if (_heading == Facing.Left ? _swoopAngle < Turns.PerTurn - StrikeAfterSteps : _swoopAngle >= HalfTurn + StrikeAfterSteps)
                {
                    _doing = Doing.Striking;
                    Play(_heading == Facing.Left ? "swoop_left" : "swoop_right", restart: true);
                }

                Swoop();
                break;

            case Doing.Striking:
                Swoop();
                break;

            case Doing.Recovering:
                if (MoveAcrossBy(_heading.Toward(Pixels.FromWhole(_acrossSpeed))))
                    _doing = Doing.Patrolling;
                else if (MoveDownBy(_downSpeed) || Position.Y.Whole < _placedY)
                    _doing = Doing.Patrolling;
                break;

            case Doing.Falling:
                if (MoveDownBy(_downSpeed))
                    _doing = Doing.StartHop;
                else
                    _downSpeed += Pixels.FromNumber(Gravity);
                break;

            case Doing.StartHop:
                _doing = Doing.Posed;
                _heading = Position.X.Whole >= Target.X.Whole ? Facing.Left : Facing.Right;
                _acrossSpeed = HopAcrossSpeed;
                // The number last drawn decides whether this hop starts a pixel slower.
                _downSpeed = Pixels.FromWhole(Random.Last.Part(0, 1) - HopUpSpeed);
                Play(_heading == Facing.Left ? "jump_left" : "jump_right", restart: true);
                break;

            case Doing.Posed:
                break;

            case Doing.Hopping:
                Hop();
                break;

            case Doing.Thinking:
                if (_wait.IsOver)
                    break;
                _wait = _wait.Stepped();
                if (_wait.IsRunning)
                    break;
                _doing = Math.Abs(Position.X.Whole - Target.X.Whole) < SpitRange ? Doing.ChoosingSpit : Doing.StartHop;
                break;

            case Doing.ChoosingSpit:
                _spits = Position.X.Whole < Target.X.Whole ? Facing.Right : Facing.Left;
                Play(_spits == Facing.Right ? "spit_right" : "spit_left", restart: true);
                _doing = Doing.Posed;
                break;

            default:
                throw new InvalidOperationException($"Undefined Ki-Hunter stage {_doing}.");
        }

        StepWings();
    }

    /// <inheritdoc/>
    protected override void OnHurt(Hit hit, Collider part)
    {
        if (_wings == WingState.Gone || Health.Current > WingsLostAtHealth || !Flies)
            return;
        _doing = Doing.Falling;
        _downSpeed = Pixels.FromWhole(1);
        if (_wings == WingState.Attached)
            LoseWings();
    }

    /// <summary>Whether it is doing one of the things it does on the wing.</summary>
    private bool Flies => _doing is Doing.Patrolling or Doing.Swooping or Doing.Striking or Doing.Recovering;

    /// <summary>Flies one update of its patrol, and begins a swoop when Samus is near enough below.</summary>
    private void Patrol()
    {
        if (MoveDownBy(_downSpeed))
            _downSpeed = -_downSpeed;
        else if (Position.Y.Whole < _upperY)
            _downSpeed = Pixels.FromWhole(PatrolSpeed);
        else if (Position.Y.Whole >= _lowerY)
            _downSpeed = Pixels.FromWhole(-PatrolSpeed);
        if (MoveAcrossBy(_heading.Toward(Pixels.FromWhole(_acrossSpeed))))
        {
            _heading = _heading.Opposite();
            FaceAlong();
        }

        int across = Target.X.Whole - Position.X.Whole;
        int depth = Target.Y.Whole - Position.Y.Whole;
        if (Math.Abs(across) >= SwoopDistance || depth < SwoopLeastDepth)
            return;
        // The swoop is half an oval: from where it is, down through where Samus stands, and up the far side.
        _swoopTurn = 0;
        _swoopAcross = Math.Abs(across);
        _swoopDown = depth;
        _swoopTargetX = Target.X.Whole;
        _swoopOriginY = Position.Y.Whole;
        _doing = Doing.Swooping;
        _heading = across < 0 ? Facing.Left : Facing.Right;
        _acrossSpeed = PatrolSpeed;
        _swoopAngle = _heading == Facing.Left ? Turns.PerTurn - 1 : HalfTurn;
        FaceAlong();
    }

    /// <summary>Flies one update of its swoop: the turn quickens, and it moves to the next place on the oval.</summary>
    private void Swoop()
    {
        // The turn is kept in 256ths of a step; only its whole steps are taken, rounded down, and only they are capped.
        _swoopTurn += _heading == Facing.Left ? -SwoopQuickening : SwoopQuickening;
        int whole = FloorSteps(_swoopTurn);
        int cap = _heading == Facing.Left ? -SwoopFastestTurn : SwoopFastestTurn;
        if (_heading == Facing.Left ? whole < cap : whole > cap)
        {
            _swoopTurn = cap * 256 + (_swoopTurn - whole * 256);
            whole = cap;
        }

        _swoopAngle = Turns.Wrap(_swoopAngle + whole);
        if (_swoopAngle < HalfTurn)
        {
            _doing = Doing.Patrolling;
            return;
        }

        int x = _swoopTargetX + CircleAcross(_swoopAngle, _swoopAcross);
        if (MoveAcrossBy(Pixels.FromWhole(x - Position.X.Whole)))
        {
            _heading = _heading.Opposite();
            _acrossSpeed = PatrolSpeed;
            Recover();
            return;
        }

        int y = _swoopOriginY + CircleDown(_swoopAngle, _swoopDown);
        if (MoveDownBy(Pixels.FromWhole(y - Position.Y.Whole)))
            Recover();
    }

    /// <summary>Breaks off a swoop that met something: it climbs back toward its patrol.</summary>
    private void Recover()
    {
        _doing = Doing.Recovering;
        _downSpeed = Pixels.FromWhole(-1);
    }

    /// <summary>Moves one update of a hop.</summary>
    private void Hop()
    {
        if (MoveDownBy(_downSpeed))
        {
            if (_downSpeed.IsNegative)
            {
                // Its head met something: it starts back down.
                _downSpeed = _downSpeed.WithWhole(1);
                return;
            }

            _downSpeed = Pixels.FromWhole(-4);
            _doing = Doing.Posed;
            _wait = Countdown.Of(LandingWait);
            Play(_heading == Facing.Left ? "land_left" : "land_right", restart: true);
            return;
        }

        if (MoveAcrossBy(_heading.Toward(Pixels.FromWhole(_acrossSpeed))))
        {
            _heading = _heading.Opposite();
            return;
        }

        _downSpeed += Pixels.FromNumber(Gravity);
    }

    /// <summary>Shows it and its wings flying the way it is heading.</summary>
    private void FaceAlong()
    {
        Play(_heading == Facing.Right ? "flying_right" : "flying_left", restart: true);
        if (_wings == WingState.Attached)
            _wingsPlayer = new TimelinePlayer(Timelines.Get(_heading == Facing.Right ? "wings_right" : "wings_left"));
    }

    /// <summary>Lets its wings go: they begin to flutter down from where it is.</summary>
    private void LoseWings()
    {
        _wingAt = (Position.X.Whole, Position.Y.Whole);
        _wingsPlayer = new TimelinePlayer(Timelines.Get("detached_wings"));
        BeginWingCircle();
    }

    /// <summary>Starts its loose wings on an arc that swings them out and down.</summary>
    private void BeginWingCircle()
    {
        _wings = WingState.Circling;
        _wingPlace = Curve.PlaceForClimb(Pixels.FromWhole(WingArcClimb), WingArcStep);
        _wingAngle = Turns.PerTurn - Turns.PerTurn / 8;
        _wingAngleParts = 0;
        _wingCentre = _wingAt;
    }

    /// <summary>Moves its wings on by one update: with it while they are on, and fluttering down once they are loose.</summary>
    private void StepWings()
    {
        if (_wings == WingState.Gone || WingsVisual is not { } visual || _wingsPlayer is null)
            return;
        _wingsPlayer.Step();
        visual.Show(_wingsPlayer.Shown);
        if (_wings == WingState.Attached)
        {
            visual.Position = Vector2.Zero;
            return;
        }

        // The speed along the curve is how far the arc turns, in 256ths of a step.
        Pixels speed = Curve.SpeedAt(_wingPlace);
        int turn = (int)((long)speed.Whole * 256 + (speed.Fraction >> 8));
        if (_wings == WingState.Circling)
        {
            int start = Turns.PerTurn - Turns.PerTurn / 8;
            _wingAngleParts -= turn;
            _wingAngle = Turns.Wrap(start + FloorSteps(_wingAngleParts));
            _wingAt = (
                _wingCentre.X + CircleAcross(_wingAngle, WingArcRadius) - CircleAcross(start, WingArcRadius),
                _wingCentre.Y + CircleDown(_wingAngle, WingArcRadius) - CircleDown(start, WingArcRadius));
            if (IsInMiddleHalfTurn(_wingAngle))
            {
                _wings = WingState.Sinking;
                _wingPlace = Curve.PlaceForClimb(Pixels.FromWhole(WingArcClimb), WingArcStep);
                _wingAngleParts = 0;
                _wingCentre = _wingAt;
            }
            else
            {
                SlowWings();
            }
        }
        else
        {
            int start = Turns.PerTurn - 3 * Turns.PerTurn / 8;
            _wingAngleParts += turn;
            _wingAngle = Turns.Wrap(start + FloorSteps(_wingAngleParts));
            int y = _wingCentre.Y + CircleDown(_wingAngle, WingArcRadius) - CircleDown(start, WingArcRadius);
            var body = new PixelRect(_wingAt.X - WingHalfWidth, y - WingHalfHeight, WingHalfWidth * 2, WingHalfHeight * 2);
            if (y > _wingAt.Y && Room.Terrain.IsSolidIn(body, SlopeTreatment.Wall))
            {
                // They have reached the ground, and are gone.
                _wings = WingState.Gone;
                visual.SetShown(false);
                return;
            }

            _wingAt = (_wingCentre.X + CircleAcross(_wingAngle, WingArcRadius) - CircleAcross(start, WingArcRadius), y);
            if (!IsInMiddleHalfTurn(_wingAngle))
                BeginWingCircle();
            else
                SlowWings();
        }

        visual.Position = new Vector2(_wingAt.X - Position.X.Whole, _wingAt.Y - Position.Y.Whole);
    }

    /// <summary>Slows its loose wings along the curve, never quite to a stop.</summary>
    private void SlowWings()
    {
        int next = _wingPlace - WingArcStep;
        _wingPlace = next < 0 ? SpeedCurve.PartsPerMark : next;
    }

    /// <summary>Whether an angle lies in the half turn from a quarter turn to three quarters.</summary>
    private static bool IsInMiddleHalfTurn(int angle) => Turns.Wrap(angle + Turns.PerTurn / 4) >= HalfTurn;

    /// <summary>The whole angle steps in a turn counted in 256ths of a step, rounded down.</summary>
    private static int FloorSteps(int parts) => (int)Math.Floor(parts / 256.0);

    /// <summary>How far across a point at <paramref name="angle"/> on a circle of <paramref name="radius"/> lies, in whole pixels, by the cartridge's short product.</summary>
    private static int CircleAcross(int angle, int radius) => CartridgeAngles.CircleCosine(angle, radius).Whole;

    /// <summary>How far down a point at <paramref name="angle"/> on a circle of <paramref name="radius"/> lies, in whole pixels, by the cartridge's short product; angles turn upward.</summary>
    private static int CircleDown(int angle, int radius) => CartridgeAngles.CircleSine(angle + HalfTurn, radius).Whole;

    /// <summary>The visual of its wings.</summary>
    private ActorVisual? WingsVisual => GetNodeOrNull<ActorVisual>("Wings");

    /// <summary>The speed curve its loose wings turn by.</summary>
    private SpeedCurve Curve => _curve ?? throw new InvalidOperationException($"The Ki-Hunter \"{Name}\" is not in a room.");
}
