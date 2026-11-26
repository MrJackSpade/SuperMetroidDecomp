using Engine.Actors;
using Engine.Actors.Components;
using Engine.Actors.Movement;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Game.Compatibility;
using Game.Data;
using Godot;

namespace Game.Enemies;

/// <summary>
/// Botwoon: a serpent that lives in a wall full of holes. It weaves out of one hole and into another along set
/// paths, its long body following exactly where its head went, and now and then stops with its head out to spit a
/// fan of acid at Samus. Only its head can be hurt. As it weakens it moves and spits faster. Killed, it falls to
/// the floor piece by piece, and the wall it lived in crumbles.
/// </summary>
/// <remarks>
/// Its head plays "hidden", and "moving_" and "spitting_" followed by one of the eight ways it can point; the cue
/// "spit_frame" of a spitting timeline lets the volley go, and "radius:" with a half-width and a half-height sizes
/// its head for the way it points. The visuals "Body1" through "Body13", tail first, play "body_" or "tail_" and a
/// way, and the hurtboxes "BodyHit1" through "BodyHit13" are those segments, which stop shots and are not hurt, and touched hurt Samus by "segment_damage". Its
/// data gives its four holes under "holes", in the room's own pixels, and its paths under "paths": where each runs
/// from and to, whether through the open or inside the wall, and its steps as one digit each, 0 for up and on round
/// clockwise; "health_shades", in the order it shows them, the set of its pictures it shows once its health is
/// "below" each, a picture's path holding "{shade}" where the set's name goes; and "wall_crumble", the change to the
/// terrain that brings its wall down (<see cref="Engine.World.PatchSequence"/>). Its shot is "spit". Its falls follow
/// the speed curve its data names.
/// </remarks>
public partial class Botwoon : Enemy
{
    /// <summary>The things told as its wall begins to fall, so that the view may go on past it.</summary>
    [Export]
    public Godot.Collections.Array<NodePath> ToldWhenWallFalls { get; set; } = [];

    /// <summary>What they are told.</summary>
    [Export]
    public string WallFallsMessage { get; set; } = "open";

    /// <summary>The music that plays once it is dead, by name.</summary>
    [Export]
    public string VictoryMusic { get; set; } = "";

    /// <summary>How many updates pass before it first moves.</summary>
    [Export, DataValue("opening_delay")]
    public int OpeningDelay { get; set; } = 256;

    /// <summary>The damage a segment of its body does to Samus when it touches her.</summary>
    [Export, DataValue("segment_damage")]
    public int SegmentDamage { get; set; } = 128;

    /// <summary>How far down the room its floor is, in pixels: where its pieces land.</summary>
    [Export, DataValue("floor_y")]
    public int FloorY { get; set; } = 200;

    /// <summary>How many updates pass between its death and its head falling.</summary>
    [Export, DataValue("pre_fall_wait")]
    public int PreFallWait { get; set; } = 16;

    /// <summary>How many updates the wall takes to come down.</summary>
    [Export, DataValue("wall_updates")]
    public int WallUpdates { get; set; } = 192;

    /// <summary>How many things it drops as the wall comes down.</summary>
    [Export, DataValue("wall_drops")]
    public int WallDrops { get; set; } = 16;

    /// <summary>How far across the room the wall is, in pixels: where its explosions are.</summary>
    [Export, DataValue("wall_x")]
    public int WallX { get; set; } = 232;

    /// <summary>The effect of an explosion, by the data file of its timeline.</summary>
    [Export, DataValue("explosion_effect")]
    public string ExplosionEffect { get; set; } = "";

    /// <summary>The effect of an explosion on its wall as it falls.</summary>
    [Export, DataValue("wall_explosion_effect")]
    public string WallExplosionEffect { get; set; } = "";

    /// <summary>The effect of a puff of smoke.</summary>
    [Export, DataValue("smoke_effect")]
    public string SmokeEffect { get; set; } = "";

    /// <summary>The sound of an explosion, by name.</summary>
    [Export, DataValue("explosion_sound")]
    public string ExplosionSound { get; set; } = "";

    /// <summary>The data file of the speed curve its falls follow.</summary>
    [Export, DataValue("speed_curve")]
    public string SpeedCurveFile { get; set; } = "";

    /// <summary>The sound of a shot a segment of its body stops, by name.</summary>
    [Export, DataValue("dud_sound")]
    public string DudSound { get; set; } = "";

    private const int Segments = 13;
    private const int Trail = 256;
    private const int FallStep = 192;

    private static readonly string[] Ways = ["up", "up_right", "right", "down_right", "down", "down_left", "left", "up_left"];
    private static readonly (int X, int Y)[] Steps = [(0, -1), (1, -1), (1, 0), (1, 1), (0, 1), (-1, 1), (-1, 0), (-1, -1)];

    // How fast it moves and spits, and how closely its body follows, by how hurt it is: whole, below half, below a quarter.
    private static readonly (int Speed, int Spacing, int SpitSpeed)[] Stages = [(2, 6, 2), (3, 4, 3), (4, 3, 4)];

    private sealed record Path(int From, int To, bool InsideWall, int[] Steps);

    private enum Phase
    {
        InitialDelay,
        Choosing,
        OnPath,
        Spitting,
        DeathDelay,
        HeadFalling,
        WaitingForBody,
        WallFalling,
    }

    private enum HeadDoing
    {
        FollowingMovement,
        Aiming,
        WaitingForSpitFrame,
        SpitAimed,
        Cooldown,
    }

    private enum Death
    {
        Alive,
        AwaitingTailExit,
        Dying,
    }

    private readonly List<(int X, int Y)> _holes = [];
    private readonly List<Path> _paths = [];
    private readonly (int X, int Y)[] _trail = new (int X, int Y)[Trail];
    private readonly (int X, int Y)[] _headTrail = new (int X, int Y)[4];
    private readonly (int X, int Y)[] _segmentAt = new (int X, int Y)[Segments];
    private readonly bool[] _segmentHidden = new bool[Segments];
    private readonly int[] _segmentFallWait = new int[Segments];
    private readonly int[] _segmentFallPlace = new int[Segments];
    private readonly bool[] _segmentLanded = new bool[Segments];

    /// <summary>
    /// What holds each segment's place among the room's places for what enemies launch: on the cartridge each segment
    /// is a shot of its own. Null for a segment the room had no place for, which is not there at all, and for one that
    /// has landed and handed its place to its explosion. They are taken from behind the head back, so those that got
    /// one are always the nearest the head.
    /// </summary>
    private readonly Effect?[] _segmentPlaces = new Effect?[Segments];
    private readonly List<(int Below, string Shade)> _healthShades = [];
    private Engine.World.PatchSequence? _wallCrumble;
    private int _shown = -1;
    private string _shade = "";
    private SpeedCurve? _curve;
    private TimelinePlayer? _ripple;
    private Phase _phase;
    private HeadDoing _head;
    private Death _death;
    private Countdown _wait;
    private Countdown _spitCooldown;
    private int _ring;
    private int _stage;
    private int _targetHole;
    private Path? _path;
    private bool _pathBackward;
    private int _pathStep;
    private bool _towardHole = true;
    private bool _finished;
    private bool _insideHole = true;
    private bool _wasInsideHole = true;
    private bool _holeLatch;
    private bool _firstAction = true;
    private bool _tailExited;
    private int? _savedRing;
    private int _startingHealth;

    // The way it aims its spit, as the cartridge counts an angle: none right, a quarter turn up.
    private int _spitAngle;
    private int _headFallPlace;
    private int _wallAge;
    private int _largeIn = 1;
    private int _smallIn = 1;
    private int? _dudSoundAt;
    private bool _headTangible;

    /// <summary>
    /// Whether its head meets shots and Samus. It starts hidden and out of reach; its head is put in reach or out of it
    /// as it moves outside the wall or inside it, put in reach as it stops to spit, and put out of reach by any hit that
    /// finds it at no health and as it lands dead. Dying, its head can so be hit again whenever it moves outside the wall,
    /// until the next hit, as on the cartridge. The segments of its body out of the wall meet shots all the same, the old
    /// game's segments being things of their own.
    /// </summary>
    private bool HeadIsTangible => _headTangible;

    /// <summary>Switches its head's hurtbox on or off as its head meets shots or not.</summary>
    private void ShowHeadTangible() => GetNode<Collider>("Hurtbox").Enabled = HeadIsTangible;

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _holes.Clear();
        _holes.AddRange(data.List("holes").Select(hole => (hole.Int("x"), hole.Int("y"))));
        if (_holes.Count != 4)
            throw data.Problem("must list four holes: left, bottom, top, right");
        _paths.Clear();
        foreach (DataNode path in data.List("paths"))
        {
            int[] steps = [.. path.Text("steps").Select(step => step - '0')];
            if (steps.Any(step => step is < 0 or > 7))
                throw path.Problem("has a step that is not one of the eight ways, 0 through 7");
            _paths.Add(new Path(path.Int("from"), path.Int("to"), path.Flag("inside_wall"), steps));
        }

        _healthShades.Clear();
        _healthShades.AddRange(data.List("health_shades").Select(band => (band.Int("below"), band.Text("shade"))));
        _wallCrumble = Engine.World.PatchSequence.Read(data.Get("wall_crumble"));
    }

    /// <inheritdoc/>
    protected override string PictureOf(TimelineStep step) => Shaded(step.Picture);

    /// <summary>A picture's path with the set of its pictures it shows put where the path leaves room for one.</summary>
    private string Shaded(string picture) => picture.Replace("{shade}", _shade, StringComparison.Ordinal);

    /// <inheritdoc/>
    public override HitResult TakeHit(Hit hit, Collider part)
    {
        if (part.Name == "Hurtbox")
        {
            HitResult result = base.TakeHit(hit, part);
            PutHeadOutOfReachIfDead();
            return result;
        }

        // A segment of its body is met only by her beams and missiles: never by a bomb, nor by what she strikes with
        // her body.
        if (!hit.CarriesAny([new HitKind(HitKinds.Beam), new HitKind(HitKinds.Missile), new HitKind(HitKinds.SuperMissile)]))
            return HitResult.Ignored;

        // It stops the shot with the dud's sound and takes nothing from it; a plasma shot is not stopped, and sounds
        // the dud each update it is there.
        if (_dudSoundAt != Updates)
        {
            _dudSoundAt = Updates;
            Sound(DudSound);
        }

        return hit.Carries(new HitKind(HitKinds.Plasma)) ? HitResult.Ignored : HitResult.Absorbed;
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _curve = SpeedCurve.Read(Room.Content.ReadData(SpeedCurveFile));
        _ripple = new TimelinePlayer(Timelines.Get("body_right"));
        _wait = Countdown.Of(OpeningDelay);
        _startingHealth = MaxHealth;
        AlwaysActive = true;
        Play("hidden");
        (int X, int Y) here = (Position.X.Whole, Position.Y.Whole);
        Array.Fill(_trail, here);
        Array.Fill(_headTrail, here);
        Array.Fill(_segmentAt, here);
        Array.Fill(_segmentHidden, true);

        // Its segments take their places as it is set up, the one behind its head first and the tail last.
        for (int segment = Segments - 1; segment >= 0; segment--)
        {
            Effect place = Effect.HoldingPlace(Position);
            _segmentPlaces[segment] = Room.Spawn(place) ? place : null;
        }

        ShowBody();
    }

    /// <summary>Whether a segment of its body is there: whether the room had a place for it, landed or not.</summary>
    private bool IsThere(int segment) => _segmentPlaces[segment] is not null || _segmentLanded[segment];

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        if (cue == "spit_frame")
        {
            _head = HeadDoing.SpitAimed;
            return;
        }

        string[] parts = cue.Split(':');
        if (parts[0] != "radius")
            throw new InvalidOperationException($"Botwoon's timeline \"{Playing}\" gives the cue \"{cue}\", which it does not know.");
        int across = int.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture), down = int.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);
        GetNode<Collider>("Hurtbox").Rect = new PixelRect(-across, -down, across * 2, down * 2);
    }

    /// <inheritdoc/>
    protected override bool DiesAtOnce(Hit hit)
    {
        // Dead, it carries on until the last of it is out of the wall, and only then falls.
        if (_death == Death.Alive)
            _death = Death.AwaitingTailExit;
        return false;
    }

    /// <summary>Puts its head out of reach after a hit or a touch that finds it at no health.</summary>
    private void PutHeadOutOfReachIfDead()
    {
        if (Health.Current != 0)
            return;
        _headTangible = false;
        ShowHeadTangible();
    }

    /// <inheritdoc/>
    protected override void Touch(Collider part)
    {
        if (part.Name != "Hurtbox")
        {
            TouchWithSegment(part);
            return;
        }

        if (!HeadIsTangible)
            return;
        base.Touch(part);
        PutHeadOutOfReachIfDead();
    }

    /// <summary>
    /// A segment of its body out of the wall hurts Samus by its own damage, not the head's, until it has landed: the
    /// old game's segments were things it launched, harmful only while out of the wall.
    /// </summary>
    private void TouchWithSegment(Collider segment)
    {
        PixelRect at = segment.RoomRect;
        int middle = at.Left + at.Width / 2;
        var away = PixelOffset.FromWhole(Room.Player.Position.X.Whole >= middle ? 1 : -1, 0);
        Room.Player.TakeHit(new Hit(SegmentDamage, HitSide.Enemies, HitKind.Contact) { Direction = away, Source = this }, Room.Player.Hurtbox);
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        // Its colours move on one set an update for as long as its health is below what the next one waits for.
        if (_shown + 1 < _healthShades.Count && Health.Current < _healthShades[_shown + 1].Below)
            _shade = _healthShades[++_shown].Shade;

        if (_death == Death.AwaitingTailExit && _tailExited)
        {
            _death = Death.Dying;
            _phase = Phase.DeathDelay;
            _wait = Countdown.Of(PreFallWait);
            _tailExited = false;
            // How long each segment waits before it falls goes by the place it holds, not by where it is in the body.
            for (int segment = 0; segment < Segments; segment++)
            {
                if (_segmentPlaces[segment] is { } held)
                    _segmentFallWait[segment] = 160 - 8 * (Room.PlaceOf(held) ?? throw new InvalidOperationException("A segment of Botwoon holds no place."));
            }
        }

        switch (_phase)
        {
            case Phase.InitialDelay:
                if (Ends(ref _wait))
                    _phase = Phase.Choosing;
                break;

            case Phase.Choosing:
                Choose();
                break;

            case Phase.OnPath:
                if (_finished)
                {
                    _phase = Phase.Choosing;
                    _finished = false;
                    _towardHole = true;
                    if (_insideHole)
                        _holeLatch = false;
                    break;
                }

                MoveOneUpdate();
                break;

            case Phase.Spitting:
                if (_head != HeadDoing.FollowingMovement)
                {
                    StepHead();
                    break;
                }

                _phase = Phase.OnPath;
                _insideHole = _death != Death.AwaitingTailExit && Random.Draw().Part(0, 1) == 1;
                _wasInsideHole = _insideHole;
                if (_insideHole)
                {
                    _holeLatch = false;
                    _savedRing = null;
                }

                ChoosePath();
                break;

            case Phase.DeathDelay:
                if (Ends(ref _wait))
                    _phase = Phase.HeadFalling;
                break;

            case Phase.HeadFalling:
                Position += new PixelOffset(Pixels.Zero, Curve.SpeedAt(_headFallPlace));
                if (Position.Y.Whole < FloorY)
                {
                    _headFallPlace += FallStep;
                    break;
                }

                Position = new PixelPosition(Position.X, Position.Y.WithWhole(FloorY));
                _phase = Phase.WaitingForBody;
                Explode(Position);
                _headTangible = false;
                Visual?.SetShown(false);
                break;

            case Phase.WaitingForBody:
                if (_segmentLanded[0])
                    BeginWallFall();
                break;

            case Phase.WallFalling:
                WallFall();
                break;
        }

        if (_death == Death.Dying)
            FallBody();
        ShowBody();
    }

    /// <summary>Counts one update of a running wait, and returns whether that ended it.</summary>
    private static bool Ends(ref Countdown wait)
    {
        if (wait.IsOver)
            return false;
        wait = wait.Stepped();
        return wait.IsOver;
    }

    /// <summary>
    /// The name of the way an offset points, of eight: the nearest of them where that is asked for, as its aim takes
    /// it, and otherwise the one it has last turned past going clockwise, as its head and body take theirs as they move.
    /// </summary>
    private static string WayOf(int across, int down, bool nearest = false) =>
        Ways[Turns.Wrap(Turns.PerTurn / 4 - CartridgeAngles.AimAngle(across, down) + (nearest ? Turns.PerTurn / 16 : 0)) * Ways.Length / Turns.PerTurn];

    /// <summary>The speed curve its falls follow.</summary>
    private SpeedCurve Curve => _curve ?? throw new InvalidOperationException("Botwoon is not in a room.");

    /// <summary>Decides what comes next once a movement is done: another path, or a stop to spit.</summary>
    private void Choose()
    {
        if (!_finished)
        {
            MoveOneUpdate();
            DetectHole();
            return;
        }

        bool takesPath = _insideHole || _firstAction || _stage != 0 || Random.Draw().Part(1, 3) < 3;
        _firstAction = false;
        _finished = false;
        if (takesPath)
        {
            _phase = Phase.OnPath;
            _head = HeadDoing.FollowingMovement;
            ChoosePath();
        }
        else
        {
            _phase = Phase.Spitting;
            _head = HeadDoing.Aiming;
            _headTangible = true;
        }
    }

    /// <summary>Picks its next path by chance, from where it is to another hole, and sets how fast it now goes.</summary>
    private void ChoosePath()
    {
        if (!_insideHole)
        {
            int health = Health.Current;
            _stage = health == 0 || health >= _startingHealth / 2 ? 0 : health >= _startingHealth / 4 ? 1 : 2;
        }

        int[] others = [.. Enumerable.Range(0, 4).Where(hole => hole != _targetHole)];
        int draw = Random.Draw().Part(3, 2);
        int from = _targetHole;
        // In the open any hole may be next, even the one it is at; inside the wall the last of the others is likeliest.
        int to = _insideHole ? others[Math.Min(draw, 2)] : draw < 3 ? others[draw] : from;
        Path path = _insideHole
            ? _paths.Single(candidate => candidate.InsideWall && ((candidate.From == from && candidate.To == to) || (candidate.From == to && candidate.To == from)))
            : _paths.Single(candidate => !candidate.InsideWall && candidate.From == from && candidate.To == to);
        _path = path;
        _pathBackward = _insideHole && path.From != from;
        _pathStep = _pathBackward ? path.Steps.Length - 2 : 0;
        _targetHole = to;
        _towardHole = false;
    }

    /// <summary>Moves its head one update, records where it went, and brings its body along behind.</summary>
    private void MoveOneUpdate()
    {
        (int speed, int spacing, _) = Stages[_stage];
        if (_towardHole)
        {
            // Homing on its hole, it stops the moment it has passed through the hole's mouth.
            (int x, int y) = _holes[_targetHole];
            int bearing = CartridgeAngles.AimAngle(HoleReach(x - Position.X.Whole), HoleReach(y - Position.Y.Whole));
            if (_insideHole == _wasInsideHole)
            {
                Position += CartridgeAngles.VelocityAlong(bearing, speed).OverOneUpdate;
            }
            else
            {
                _wasInsideHole = _insideHole;
                _finished = true;
            }
        }
        else if (_path is { } path)
        {
            (int X, int Y) moved = (0, 0);
            for (int taken = 0; taken < speed; taken++)
            {
                if (_pathStep < 0 || _pathStep >= path.Steps.Length)
                {
                    _finished = true;
                    return;
                }

                (int x, int y) = Steps[path.Steps[_pathStep]];
                moved = (moved.X + x, moved.Y + y);
                _pathStep += _pathBackward ? -1 : 1;
            }

            Position += _pathBackward ? PixelOffset.FromWhole(-moved.X, -moved.Y) : PixelOffset.FromWhole(moved.X, moved.Y);
        }

        _trail[_ring] = (Position.X.Whole, Position.Y.Whole);
        // The segments the room had no place for are not there, and the cartridge never learns that a missing tail has
        // left the wall: then, once dead, it goes on and on without falling.
        int place = Wrap(_ring - spacing);
        for (int segment = Segments - 1; segment >= 0 && IsThere(segment); segment--)
        {
            if (_savedRing == place)
            {
                // This segment is at the mouth of the hole the head passed: it goes in, or comes out.
                _segmentHidden[segment] = !_segmentHidden[segment];
                if (segment == 0)
                {
                    _holeLatch = false;
                    _tailExited = !_insideHole;
                    _savedRing = null;
                }
            }

            _segmentAt[segment] = _trail[place];
            place = Wrap(place - spacing);
        }

        StepHead();
        _ring = Wrap(_ring + 1);
    }

    /// <summary>A place in its trail brought within the ring.</summary>
    private static int Wrap(int place) => ((place % Trail) + Trail) % Trail;

    /// <summary>A distance to its hole as its aim reads it: held within 255 either way, except that 256 left or up is kept.</summary>
    private static int HoleReach(int distance) => distance < -256 ? -255 : distance > 255 ? 255 : distance;

    /// <summary>Notes when its head passes through the mouth of a hole, going in or coming out.</summary>
    private void DetectHole()
    {
        if (_holeLatch)
            return;
        for (int hole = _holes.Count - 1; hole >= 0; hole--)
        {
            int across = Position.X.Whole - (_holes[hole].X - 4), down = Position.Y.Whole - (_holes[hole].Y - 4);
            if (across is < 0 or >= 8 || down is < 0 or >= 8)
                continue;
            _holeLatch = true;
            _insideHole = !_insideHole;
            _savedRing = _ring;
            return;
        }
    }

    /// <summary>Moves what its head is doing on by one update.</summary>
    private void StepHead()
    {
        switch (_head)
        {
            case HeadDoing.FollowingMovement:
                (int X, int Y) oldest = _headTrail[^1];
                int across = Position.X.Whole - oldest.X, down = Position.Y.Whole - oldest.Y;
                if (across != 0 || down != 0)
                {
                    // Only its head goes behind everything inside the wall and in front of Samus out of it; its body
                    // stays behind her.
                    SetHeadDepth(_insideHole ? RoomDepth.BehindRoom : RoomDepth.InFrontOfThings);
                    _headTangible = !_insideHole;
                    Play(_insideHole ? "hidden" : $"moving_{WayOf(across, down)}");
                }

                for (int index = _headTrail.Length - 1; index > 0; index--)
                    _headTrail[index] = _headTrail[index - 1];
                _headTrail[0] = (Position.X.Whole, Position.Y.Whole);
                break;

            case HeadDoing.Aiming:
                SetHeadDepth(RoomDepth.InFrontOfThings);
                int toSamusAcross = Target.X.Whole - Position.X.Whole, toSamusDown = Target.Y.Whole - Position.Y.Whole;
                _spitAngle = CartridgeAngles.AimAngle(toSamusAcross, toSamusDown);
                Play($"spitting_{WayOf(toSamusAcross, toSamusDown, nearest: true)}");
                _head = HeadDoing.WaitingForSpitFrame;
                break;

            case HeadDoing.SpitAimed:
                // A fan of five, an eighth of a turn to either side of her, fired from her clockwise side round.
                int speed = Stages[_stage].SpitSpeed;
                for (int shot = 0; shot < 5; shot++)
                {
                    int angle = _spitAngle + (shot - 2) * (Turns.PerTurn / 16);
                    Fire("spit", PixelPosition.FromWhole(Position.X.Whole, Position.Y.Whole), CartridgeAngles.VelocityAlong(angle, speed));
                }

                _spitCooldown = Countdown.Of(48);
                _head = HeadDoing.Cooldown;
                break;

            case HeadDoing.Cooldown:
                if (Ends(ref _spitCooldown))
                    _head = HeadDoing.FollowingMovement;
                break;
        }
    }

    /// <summary>Puts its head in front of or behind the rest of what is drawn; its body keeps its own place.</summary>
    private void SetHeadDepth(int depth)
    {
        if (Visual is { } head)
        {
            head.ZAsRelative = false;
            head.ZIndex = depth;
        }
    }

    /// <summary>Lets its body fall, each segment after its own wait, the one nearest the head first.</summary>
    private void FallBody()
    {
        for (int segment = Segments - 1; segment >= 0; segment--)
        {
            if (_segmentPlaces[segment] is not { } held)
                continue;
            if (_segmentFallWait[segment] > 0)
            {
                _segmentFallWait[segment]--;
                continue;
            }

            (int x, int y) = _segmentAt[segment];
            y += Math.Max(1, Curve.SpeedAt(_segmentFallPlace[segment]).Whole);
            _segmentFallPlace[segment] += FallStep;
            if (y >= FloorY)
            {
                y = FloorY;
                _segmentLanded[segment] = true;
                _segmentHidden[segment] = true;
                _segmentPlaces[segment] = null;

                // The segment becomes its explosion, which holds its place until it has played.
                if (ExplosionEffect.Length > 0)
                    Room.SpawnInPlaceOf(held, Effect.Of(FrameTimeline.Read(Room.Content.ReadData(ExplosionEffect)), PixelPosition.FromWhole(x, y), holdsPlace: true));
                else
                    Room.Remove(held);
                Sound(ExplosionSound);
            }

            _segmentAt[segment] = (x, y);
        }
    }

    /// <summary>Leaves an explosion, with its sound.</summary>
    private void Explode(PixelPosition at)
    {
        // The explosion is one of the enemies' shots on the cartridge: it holds one of the room's places while it plays,
        // and is not made with none free.
        if (ExplosionEffect.Length > 0)
            Room.Spawn(Effect.Of(FrameTimeline.Read(Room.Content.ReadData(ExplosionEffect)), PixelPosition.FromWhole(at.X.Whole, at.Y.Whole), holdsPlace: true));
        Sound(ExplosionSound);
    }

    /// <summary>Starts the wall coming down: it crumbles, and what it held scatters out.</summary>
    private void BeginWallFall()
    {
        _phase = Phase.WallFalling;
        if (_wallCrumble is { } crumble)
            Room.Play(crumble.FromStart());
        foreach (NodePath path in ToldWhenWallFalls)
            (GetNode(path) as Engine.World.ITriggerTarget ?? throw new InvalidOperationException($"Botwoon is to tell \"{path}\" that its wall falls, but that is nothing that can be told.")).Triggered(WallFallsMessage);
        for (int drop = 0; drop < WallDrops; drop++)
        {
            RandomDraw draw = Random.Draw();
            Drop(Drops, PixelPosition.FromWhole(draw.Part(0, 7) + 64, draw.Part(8, 6) + 128));
        }
    }

    /// <summary>Moves the wall's fall on by one update: explosions work down it, and at the end the fight is over.</summary>
    private void WallFall()
    {
        if (_wallAge >= WallUpdates)
        {
            if (VictoryMusic.Length > 0)
                Room.Session.Sounds.RequestMusic(VictoryMusic);
            Die();
            return;
        }

        if (_wallAge >= 64)
        {
            int top = _wallAge - 8;
            if (--_largeIn <= 0)
            {
                _largeIn = 13;
                PixelPosition burst = PixelPosition.FromWhole(WallX + Random.Draw().Part(0, 5), top + Random.Draw().Part(0, 5));
                if (WallExplosionEffect.Length > 0)
                    LeaveEffect(WallExplosionEffect, burst);
                Sound(ExplosionSound);
            }

            if (--_smallIn <= 0)
            {
                _smallIn = 5;
                for (int puff = 0; puff < 2 && SmokeEffect.Length > 0; puff++)
                    LeaveEffect(SmokeEffect, PixelPosition.FromWhole(WallX - 8 + Random.Draw().Part(0, 6), top + Random.Draw().Part(0, 5)));
            }
        }

        _wallAge++;
    }

    /// <summary>Puts each segment of its body where it is, pointing at the one ahead of it.</summary>
    private void ShowBody()
    {
        ShowHeadTangible();
        // The ripple shows the step this update falls in, which is the one it is on before it is stepped.
        int ripple = _ripple?.StepIndex ?? 0;
        _ripple?.Step();
        for (int segment = 0; segment < Segments; segment++)
        {
            (int x, int y) = _segmentAt[segment];
            (int X, int Y) ahead = segment == Segments - 1 ? (Position.X.Whole, Position.Y.Whole) : _segmentAt[segment + 1];
            var offset = new Vector2(x - Position.X.Whole, y - Position.Y.Whole);
            bool hidden = _segmentHidden[segment];
            if (GetNodeOrNull<ActorVisual>($"Body{segment + 1}") is { } visual)
            {
                visual.SetShown(!hidden);
                visual.Position = offset;
                if (!hidden)
                {
                    FrameTimeline look = Timelines.Get($"{(segment == 0 ? "tail" : "body")}_{WayOf(ahead.X - x, ahead.Y - y)}");
                    TimelineStep step = look.Steps[ripple % look.Steps.Count];
                    // Its body flashes with its head, in its flash's colours, as every visual of an enemy does.
                    visual.Show(step with { Picture = Shaded(step.Picture) });
                }
            }

            if (GetNodeOrNull<Collider>($"BodyHit{segment + 1}") is { } hitbox)
            {
                // A segment out of the wall goes on touching Samus as it falls, until it lands.
                hitbox.Enabled = !hidden;
                hitbox.Rect = new PixelRect((int)offset.X - hitbox.Size.X / 2, (int)offset.Y - hitbox.Size.Y / 2, hitbox.Size.X, hitbox.Size.Y);
            }
        }
    }
}
