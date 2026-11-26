using Engine.Actors;
using Engine.Actors.Movement;
using Engine.Collision;
using Engine.Content;
using Engine.Input;
using Engine.Shared;
using Engine.World;
using Game.Data;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Yard: a snail that crawls round whatever it clings to, stopping at each corner to turn. When Samus looks its way
/// it shuts itself in its shell. She can stand on it; running into it kicks it flying, and a shot knocks it into the
/// air too. Kicked, it bounces to rest and then comes after her in a temper, crawling fast, until it has touched her.
/// </summary>
/// <remarks>
/// A way of crawling is a surface and a way along it: "floor_left", "floor_right", "ceiling_left", "ceiling_right",
/// "left_wall_up", "left_wall_down", "right_wall_up" and "right_wall_down". Its timelines, for each:
/// "crawl_" and the way; "outside_turn_" and "inside_turn_" and the way, which lead into the crawl beyond the corner;
/// "hide_" and the way, its legs tucking, its shell shut and then coming out again; and "shell_" and the way, which is
/// the same from the shut shell on. "airborne_left" and "airborne_right" show it in the air, and "landed_left" and
/// "landed_right" its shell on the floor before it comes out. Its cues: "crawl" sets it crawling; "turn" holds it
/// still, unable to hide; "hide" stops it to hide; "shell_closed" is where it may shut its shell again;
/// "move:&lt;x&gt;:&lt;y&gt;" shifts it round a corner. Its solid collider is what Samus stands on.
/// </remarks>
public partial class Yard : Enemy
{
    /// <summary>The surface it starts on.</summary>
    [Export]
    public SurfaceCrawl.Clings StartsOn { get; set; } = SurfaceCrawl.Clings.Below;

    /// <summary>Whether it crawls clockwise round what it clings to.</summary>
    [Export]
    public bool Clockwise { get; set; } = true;

    /// <summary>How far it crawls each update, as a number of pixels.</summary>
    [Export]
    public double CrawlSpeed { get; set; } = 0.5;

    /// <summary>How far it crawls each update once it has been kicked, until it has touched Samus.</summary>
    [Export, DataValue("aggressive_crawl_speed")]
    public double AggressiveCrawlSpeed { get; set; } = 2.25;

    /// <summary>How far above it Samus may be, in pixels, and still make it hide by looking at it.</summary>
    [Export, DataValue("hiding_reach_above")]
    public int HidingReachAbove { get; set; } = 96;

    /// <summary>How far beyond a crawling step it feels for what it clings to while it hides, in pixels; finding nothing, it falls.</summary>
    [Export, DataValue("hiding_probe_reach")]
    public int HidingProbeReach { get; set; } = 7;

    /// <summary>How far ahead of itself it feels for a corner, in pixels.</summary>
    [Export, DataValue("turn_lookahead")]
    public int TurnLookahead { get; set; } = 7;

    /// <summary>
    /// How many updates of plain crawling follow a turn before it feels ahead along a floor or ceiling again; until
    /// then it goes round a corner there without stopping.
    /// </summary>
    [Export, DataValue("turn_rest_updates")]
    public int TurnRestUpdates { get; set; } = 16;

    /// <summary>The fastest it can be falling, in whole pixels an update, and still come to rest rather than bounce.</summary>
    [Export, DataValue("landing_speed_limit")]
    public int LandingSpeedLimit { get; set; } = 3;

    /// <summary>The downward speed it gains each update in the air, as a number of pixels.</summary>
    [Export, DataValue("gravity")]
    public double Gravity { get; set; } = 0.125;

    /// <summary>The whole pixels an update its fall never reaches.</summary>
    [Export, DataValue("fall_speed_limit")]
    public int FallSpeedLimit { get; set; } = 4;

    /// <summary>The speed across it loses each update in the air.</summary>
    [Export, DataValue("air_drag")]
    public double AirDrag { get; set; } = 0.0625;

    /// <summary>The speed, across and upward, a shot knocks it into the air with, in whole pixels an update.</summary>
    [Export, DataValue("shot_launch_speed")]
    public int ShotLaunchSpeed { get; set; } = 1;

    /// <summary>The sound of it being kicked or bouncing, by name.</summary>
    [Export, DataValue("bounce_sound")]
    public string BounceSound { get; set; } = "";

    private enum Mood
    {
        Calm,
        Angry,
        Hiding,
        Dropped,
        Kicked,
        Launched,
    }

    private enum Doing
    {
        Still,
        Hiding,
        Crawling,
        Airborne,
    }

    private readonly SurfaceCrawl _crawl = new();
    private readonly List<Pixels> _kicks = [];
    private Mood _mood;
    private Doing _doing;

    /// <summary>The speed it next takes up crawling at.</summary>
    private Pixels _speed;

    /// <summary>Its speed through the air.</summary>
    private PixelVelocity _flight;

    /// <summary>The way its shell faces: the way it is shown in the air, and the way it sets off on landing.</summary>
    private Facing _faces;

    /// <summary>The timeline it hides with; empty while it cannot hide.</summary>
    private string _hide = "";

    /// <summary>The timeline it lands with.</summary>
    private string _landing = "";

    /// <summary>
    /// Whether, hiding, it feels across for what it clings to rather than down. Until its first crawl has begun it
    /// feels across whatever it is on, as the original does, so one hidden as its room is set up may fall.
    /// </summary>
    private bool _feelsAcross = true;

    private Countdown _turnRest;

    /// <summary>
    /// The shake that shakes it off what it clings to, as its data gives it ("falls_with_shake": a super missile
    /// striking a wall). Only that shake does, and only as it begins.
    /// </summary>
    private RoomShake? _fallsWith;

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _fallsWith = RoomShake.Read(data.Get("falls_with_shake"));
        // The speed down a kick gives it, which is upward and so below nought, by the whole pixels of the camera's last measure of Samus's movement across.
        _kicks.Clear();
        _kicks.AddRange(data.List("kick_speeds").Select(speed => speed.AsPixels()));
        if (_kicks.Count == 0)
            throw data.Problem("must list at least one kick speed");
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _speed = Pixels.FromNumber(CrawlSpeed);
        _crawl.Speed = _speed;
        _crawl.Start(StartsOn, Clockwise);
        _faces = Clockwise ? Facing.Right : Facing.Left;
        _hide = $"hide_{Way}";
        Play($"crawl_{Way}");
    }

    /// <summary>The way it crawls: the surface it is on and the way along it.</summary>
    private string Way => _crawl.Side switch
    {
        SurfaceCrawl.Clings.Below => Clockwise ? "floor_right" : "floor_left",
        SurfaceCrawl.Clings.Above => Clockwise ? "ceiling_left" : "ceiling_right",
        SurfaceCrawl.Clings.Left => Clockwise ? "left_wall_down" : "left_wall_up",
        SurfaceCrawl.Clings.Right => Clockwise ? "right_wall_up" : "right_wall_down",
        _ => throw new InvalidOperationException($"Undefined side {_crawl.Side}."),
    };

    /// <summary>Whether a hit is a shot, which only knocks its shell into the air: bombs alone do it harm.</summary>
    private static bool KnocksShell(Hit hit) =>
        hit.CarriesAny([new HitKind(HitKinds.Beam), new HitKind(HitKinds.Missile), new HitKind(HitKinds.SuperMissile)]);

    /// <inheritdoc/>
    protected override Reaction ReactionTo(Hit hit, Collider part) =>
        KnocksShell(hit) ? Reaction.FullDamage with { DamageTimes = 0 } : base.ReactionTo(hit, part);

    /// <inheritdoc/>
    protected override void OnAbsorbed(Hit hit, Collider part)
    {
        if (KnocksShell(hit))
            KnockedByShot();
    }

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        string[] parts = cue.Split(':');
        switch (parts[0])
        {
            case "crawl":
                _doing = Doing.Crawling;
                _hide = $"hide_{Way}";
                _feelsAcross = _crawl.Side is SurfaceCrawl.Clings.Left or SurfaceCrawl.Clings.Right;
                _faces = Clockwise ? Facing.Right : Facing.Left;
                break;
            case "turn":
                _doing = Doing.Still;
                _hide = "";
                break;
            case "hide":
                _doing = Doing.Hiding;
                break;
            case "shell_closed":
                // While Samus watches it stays shut; after that, by chance, an update at a time.
                if (_mood == Mood.Hiding || Random.Draw().Part(0, 1) != 0)
                    Play($"shell_{Way}", restart: true);
                break;
            case "move":
                Position += new PixelOffset(Pixels.FromWhole(int.Parse(parts[1])), Pixels.FromWhole(int.Parse(parts[2])));
                break;
            default:
                throw new InvalidOperationException($"The Yard \"{Name}\" does not know the cue \"{cue}\".");
        }
    }

    /// <inheritdoc/>
    protected override void Touch(Collider part)
    {
        Player samus = Room.Player;
        bool towardIt = samus.Input.IsHeld(GameAction.Right) && !samus.Input.IsHeld(GameAction.Left) ? _faces == Facing.Left : _faces == Facing.Right;
        bool shrugsOff = _mood == Mood.Angry && _doing != Doing.Still && towardIt;
        if (!shrugsOff && (_doing == Doing.Airborne || samus.HasMomentum))
        {
            // Kicked: it flies off the way Samus faces. Its speed across is not hers but the camera's last measure of
            // how far she moved across, a pixel more, fraction and all, taken before this update's view follows her;
            // the whole pixels of that measure choose how high it flies.
            _mood = Mood.Kicked;
            TakeOff();
            Pixels across = Room.Session.Camera.Speed.X;
            _flight = new PixelVelocity(samus.Facing == Facing.Left ? Reversed(across) : across, _kicks[Math.Min(across.Whole, _kicks.Count - 1)]);
            Sound(BounceSound);
            return;
        }

        if (_doing == Doing.Still || _mood is Mood.Dropped or Mood.Kicked)
            return;
        base.Touch(part);
        _speed = Pixels.FromNumber(CrawlSpeed);
        if (_mood != Mood.Calm)
            TurnRound();
        _mood = Mood.Calm;
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        Player samus = Room.Player;
        if (_doing != Doing.Airborne && Room.HasJustBegunShaking(_fallsWith!))
            Drop();
        WatchSamus(samus);
        if (Solid is { } shell)
        {
            shell.Enabled = _doing != Doing.Airborne && (_mood == Mood.Angry
                ? !samus.HasMomentum && _doing == Doing.Still
                : !samus.HasMomentum && _mood is Mood.Calm or Mood.Hiding);
        }

        switch (_doing)
        {
            case Doing.Still:
                break;
            case Doing.Hiding:
                if (!StillClings())
                    Drop();
                break;
            case Doing.Airborne:
                Fly();
                break;
            case Doing.Crawling:
                Crawl();
                break;
            default:
                throw new InvalidOperationException($"Undefined doing {_doing}.");
        }
    }

    /// <summary>
    /// Crawls one update. At a corner it stops and plays its turn, which leads into the crawl beyond; along a floor or
    /// ceiling, soon after a turn, it does not feel ahead and goes round without stopping.
    /// </summary>
    private void Crawl()
    {
        bool alongWall = _crawl.Side is SurfaceCrawl.Clings.Left or SurfaceCrawl.Clings.Right;
        bool stopsToTurn = alongWall || _turnRest.IsOver;
        _crawl.Lookahead = stopsToTurn ? TurnLookahead : 0;
        string before = Way;
        MoveReport report = _crawl.Step(this);
        if (_crawl.IsFalling)
        {
            Drop();
            return;
        }

        if (!report.ReachedEdge)
        {
            _turnRest = _turnRest.Stepped();
            return;
        }

        // Something in its way is an inside corner; its surface ending is an outside one.
        bool inside = report.HitWall || report.HitFloor || report.HitCeiling;
        Play(stopsToTurn ? $"{(inside ? "inside" : "outside")}_turn_{before}" : $"crawl_{Way}", restart: true);
        _turnRest = Countdown.Of(TurnRestUpdates);
    }

    /// <summary>Whether what it clings to is still there, a little beyond a crawling step away.</summary>
    private bool StillClings()
    {
        PixelPosition from = Position;
        Pixels reach = _crawl.Speed + Pixels.FromWhole(HidingProbeReach);
        bool clings = _feelsAcross ? MoveAcrossBy(reach * _crawl.PressesAcross) : MoveDownBy(reach * _crawl.PressesDown);
        Position = from;
        return clings;
    }

    /// <summary>Shuts it in its shell while Samus is level with it, or below, and looking its way.</summary>
    private void WatchSamus(Player samus)
    {
        if (_mood is not (Mood.Calm or Mood.Hiding))
            return;
        bool level = Position.Y.Whole - samus.Position.Y.Whole >= -HidingReachAbove;
        bool watched = Position.X.Whole < samus.Position.X.Whole ? samus.Facing == Facing.Left : samus.Facing == Facing.Right;
        if (level && watched)
        {
            if (_mood == Mood.Hiding)
                return;
            if (_hide.Length > 0)
            {
                Play(_hide, restart: true);
                _mood = Mood.Hiding;
                return;
            }
        }

        _mood = Mood.Calm;
    }

    /// <summary>Turns it to crawl the other way round.</summary>
    private void TurnRound()
    {
        if (_mood == Mood.Hiding || _doing == Doing.Still)
            return;
        Clockwise = !Clockwise;
        _crawl.Speed = _speed;
        _crawl.Start(_crawl.Side, Clockwise);
        _faces = Clockwise ? Facing.Right : Facing.Left;
        _hide = $"hide_{Way}";
        _doing = Doing.Crawling;
        _turnRest = Countdown.Of(TurnRestUpdates);
        Play($"crawl_{Way}", restart: true);
    }

    /// <summary>Lets it fall off what it clings to.</summary>
    private void Drop()
    {
        if (_mood == Mood.Dropped)
            return;
        _mood = Mood.Dropped;
        TakeOff();
        _flight = PixelVelocity.Zero;
    }

    /// <summary>Puts it in the air, shown the way its shell faces.</summary>
    private void TakeOff()
    {
        _doing = Doing.Airborne;
        string side = _faces == Facing.Left ? "left" : "right";
        _landing = $"landed_{side}";
        _hide = _landing;
        Play($"airborne_{side}", restart: true);
    }

    /// <summary>Answers a shot: it is knocked into the air, the way Samus faces, unless it is falling or kicked already.</summary>
    private void KnockedByShot()
    {
        if (_mood is Mood.Calm or Mood.Angry or Mood.Hiding or Mood.Launched)
        {
            _mood = Mood.Launched;
            TakeOff();
            // Only the whole pixels of its speed are set; what fraction it had it keeps.
            _flight = new PixelVelocity(
                _flight.X.WithWhole(Room.Player.Facing == Facing.Left ? -ShotLaunchSpeed : ShotLaunchSpeed),
                _flight.Y.WithWhole(-ShotLaunchSpeed));
        }

        Sound(BounceSound);
    }

    /// <summary>A speed turned back, as the original turns it: its whole pixels and its fraction each negated on their own.</summary>
    private static Pixels Reversed(Pixels speed) =>
        Pixels.FromParts(-speed.Whole, (Pixels.FractionsPerPixel - speed.Fraction) % Pixels.FractionsPerPixel);

    /// <summary>
    /// Its speed across after an update of drag. The original never lets the whole pixels of it reach nought: there
    /// the drag changes only the fraction.
    /// </summary>
    private Pixels Slowed(Pixels speed)
    {
        Pixels drag = Pixels.FromNumber(AirDrag);
        Pixels slowed = speed.IsNegative ? speed + drag : speed - drag;
        return slowed.Whole != 0 ? slowed : speed.WithFraction(slowed.Fraction);
    }

    /// <summary>
    /// Its falling speed after an update of gravity. The original never lets the whole pixels of it reach the limit:
    /// there gravity changes only the fraction.
    /// </summary>
    private Pixels Fallen(Pixels speed)
    {
        Pixels faster = speed + Pixels.FromNumber(Gravity);
        return faster.Whole < FallSpeedLimit ? faster : speed.WithFraction(faster.Fraction);
    }

    /// <summary>Moves it one update through the air: bouncing off walls, and off the floor until it lands gently enough.</summary>
    private void Fly()
    {
        if (_mood != Mood.Dropped)
        {
            if (MoveAcrossBy(_flight.X))
            {
                _flight = _flight.WithX(Reversed(_flight.X));
                Sound(BounceSound);
            }
            else
            {
                _flight = _flight.WithX(Slowed(_flight.X));
            }
        }

        if (!MoveDownBy(_flight.Y))
        {
            _flight = _flight.WithY(Fallen(_flight.Y));
            return;
        }

        int falling = _flight.Y.Whole;
        if (falling < 0 || falling >= LandingSpeedLimit)
        {
            _flight = _flight.WithY(Reversed(_flight.Y));
            return;
        }

        Land();
    }

    /// <summary>
    /// Brings it to rest on the floor in its shut shell: after a plain fall it is calm; after a kick or a shot it is
    /// angry, and turns its shell to face Samus. It comes out crawling the way its shell faces.
    /// </summary>
    private void Land()
    {
        _flight = PixelVelocity.Zero;
        _turnRest = Countdown.Of(TurnRestUpdates);
        if (_mood == Mood.Dropped)
        {
            _mood = Mood.Calm;
        }
        else
        {
            _mood = Mood.Angry;
            _speed = Pixels.FromNumber(AggressiveCrawlSpeed);
            int x = Position.X.Whole, samusX = Room.Player.Position.X.Whole;
            if (_faces == Facing.Right ? x >= samusX : x < samusX)
                _faces = _faces.Opposite();
            string side = _faces == Facing.Left ? "left" : "right";
            _landing = $"landed_{side}";
            _hide = _landing;
        }

        Clockwise = _faces == Facing.Right;
        _crawl.Speed = _speed;
        _crawl.Start(SurfaceCrawl.Clings.Below, Clockwise);
        _doing = Doing.Still;
        Play(_landing, restart: true);
    }
}
