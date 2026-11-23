using Engine.Actors;
using Engine.Actors.Movement;
using Engine.Collision;
using Engine.Content;
using Engine.Input;
using Engine.Shared;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Yard: a snail that crawls round whatever it clings to. When Samus looks its way it shuts itself in its shell.
/// She can stand on it; running into it kicks it flying, and a shot knocks it into the air too. Kicked, it bounces to
/// rest and then comes after her in a temper, crawling fast, until it has touched her.
/// </summary>
/// <remarks>
/// Its timelines are "floor", "ceiling", "left_wall" and "right_wall" for crawling, the same with "hiding_" before
/// them for its shut shell, and "airborne_left" and "airborne_right". Its solid collider is what Samus stands on.
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

    /// <summary>How many updates it keeps its shell shut at a time once nobody is looking; by chance it shuts it again.</summary>
    [Export, DataValue("shell_closed_updates")]
    public int ShellClosedUpdates { get; set; } = 16;

    /// <summary>The fastest it can be falling, in whole pixels an update, and still come to rest rather than bounce.</summary>
    [Export, DataValue("landing_speed_limit")]
    public int LandingSpeedLimit { get; set; } = 3;

    /// <summary>The downward speed it gains each update in the air, as a number of pixels.</summary>
    [Export, DataValue("gravity")]
    public double Gravity { get; set; } = 0.125;

    /// <summary>The speed across it loses each update in the air.</summary>
    [Export, DataValue("air_drag")]
    public double AirDrag { get; set; } = 0.03125;

    /// <summary>The speed, across and upward, a shot knocks it into the air with.</summary>
    [Export, DataValue("shot_launch_speed")]
    public double ShotLaunchSpeed { get; set; } = 1;

    /// <summary>How hard the room must be shaking to shake it off what it clings to.</summary>
    [Export, DataValue("falls_at_shake")]
    public int FallsAtShake { get; set; } = 2;

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

    private readonly SurfaceCrawl _crawl = new();
    private readonly List<Pixels> _kicks = [];
    private Mood _mood;
    private bool _airborne;
    private bool _still;
    private Countdown _shellShut;

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        // The upward speed a kick gives it, by how fast Samus was running, in whole pixels an update.
        _kicks.Clear();
        _kicks.AddRange(data.List("kick_speeds").Select(speed => speed.AsPixels()));
        if (_kicks.Count == 0)
            throw data.Problem("must list at least one kick speed");
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _crawl.Speed = Pixels.FromNumber(CrawlSpeed);
        _crawl.Start(StartsOn, Clockwise);
        Play(SurfaceName);
    }

    /// <inheritdoc/>
    protected override void OnHurt(Hit hit, Collider part) => KnockedByShot();

    /// <inheritdoc/>
    protected override void OnAbsorbed(Hit hit, Collider part) => KnockedByShot();

    /// <inheritdoc/>
    protected override void Touch(Collider part)
    {
        Player samus = Room.Player;
        bool towardIt = samus.Input.IsHeld(GameAction.Right) && !samus.Input.IsHeld(GameAction.Left) ? Facing == Facing.Left : Facing == Facing.Right;
        bool shrugsOff = _mood == Mood.Angry && !_still && towardIt;
        if (!shrugsOff && (_airborne || samus.HasMomentum))
        {
            // Kicked: it flies off the way Samus faces, higher the faster she was going.
            _mood = Mood.Kicked;
            TakeOff();
            Pixels speed = samus.Velocity.X.Abs();
            Pixels across = speed + Pixels.FromWhole(1);
            Velocity = new PixelVelocity(samus.Facing == Facing.Left ? -across : across, -_kicks[Math.Min(speed.Whole, _kicks.Count - 1)]);
            Sound(BounceSound);
            return;
        }

        if (_still || _mood is Mood.Dropped or Mood.Kicked)
            return;
        base.Touch(part);
        _crawl.Speed = Pixels.FromNumber(CrawlSpeed);
        if (_mood != Mood.Calm)
            TurnRound();
        _mood = Mood.Calm;
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        Player samus = Room.Player;
        if (!_airborne && Room.ShakeStrength >= FallsAtShake)
            Drop();
        WatchSamus(samus);
        if (Solid is { } shell)
        {
            shell.Enabled = !_airborne && (_mood == Mood.Angry
                ? !samus.HasMomentum && _still
                : !samus.HasMomentum && _mood is Mood.Calm or Mood.Hiding);
        }

        if (_airborne)
        {
            Fly();
            return;
        }

        if (_mood == Mood.Hiding || _still)
        {
            if (_shellShut.IsRunning)
                _shellShut = _shellShut.Stepped();
            else if (_mood != Mood.Hiding && Random.Draw().Part(0, 1) == 0)
                ComeOut();
            else
                _shellShut = Countdown.Of(ShellClosedUpdates);
            return;
        }

        MoveReport report = _crawl.Step(this);
        if (_crawl.IsFalling)
        {
            Drop();
            return;
        }

        if (report.ReachedEdge || report.Finished)
            Play(SurfaceName);
    }

    /// <summary>The name of the surface it is on, which is also the name of its crawling timeline there.</summary>
    private string SurfaceName => _crawl.Side switch
    {
        SurfaceCrawl.Clings.Below => "floor",
        SurfaceCrawl.Clings.Above => "ceiling",
        SurfaceCrawl.Clings.Left => "left_wall",
        SurfaceCrawl.Clings.Right => "right_wall",
        _ => throw new InvalidOperationException($"Undefined side {_crawl.Side}."),
    };

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
            Play($"hiding_{SurfaceName}", restart: true);
            _mood = Mood.Hiding;
            _shellShut = Countdown.Of(ShellClosedUpdates);
            return;
        }

        _mood = Mood.Calm;
    }

    /// <summary>Brings it out of its shell and sets it crawling.</summary>
    private void ComeOut()
    {
        _still = false;
        Play(SurfaceName, restart: true);
    }

    /// <summary>Turns it to crawl the other way round.</summary>
    private void TurnRound()
    {
        if (_mood == Mood.Hiding || _still)
            return;
        Clockwise = !Clockwise;
        _crawl.Start(_crawl.Side, Clockwise);
        Facing = Facing.Opposite();
        Play(SurfaceName, restart: true);
    }

    /// <summary>Lets it fall off what it clings to.</summary>
    private void Drop()
    {
        if (_mood == Mood.Dropped)
            return;
        _mood = Mood.Dropped;
        TakeOff();
        Velocity = PixelVelocity.Zero;
    }

    /// <summary>Puts it in the air, shown the way it faces.</summary>
    private void TakeOff()
    {
        _airborne = true;
        _still = false;
        Play(Facing == Facing.Left ? "airborne_left" : "airborne_right", restart: true);
    }

    /// <summary>Answers a shot: one that could hurt it knocks it into the air, the way Samus faces.</summary>
    private void KnockedByShot()
    {
        if (_mood is Mood.Calm or Mood.Angry or Mood.Hiding or Mood.Launched)
        {
            _mood = Mood.Launched;
            TakeOff();
            Pixels launch = Pixels.FromNumber(ShotLaunchSpeed);
            Velocity = new PixelVelocity(
                Velocity.X.WithWhole(0) + (Room.Player.Facing == Facing.Left ? -launch : launch),
                Velocity.Y.WithWhole(0) - launch);
        }

        Sound(BounceSound);
    }

    /// <summary>Moves it one update through the air: bouncing off walls, and off the floor until it lands gently enough.</summary>
    private void Fly()
    {
        if (_mood != Mood.Dropped)
        {
            if (MoveAcrossBy(Velocity.X))
            {
                Velocity = Velocity.WithX(-Velocity.X);
                Sound(BounceSound);
            }
            else
            {
                Velocity = Velocity.WithX(Velocity.X.Toward(Pixels.Zero, Pixels.FromNumber(AirDrag)));
            }
        }

        if (!MoveDownBy(Velocity.Y))
        {
            Velocity = Velocity.WithY(Velocity.Y + Pixels.FromNumber(Gravity));
            return;
        }

        int falling = Velocity.Y.Whole;
        if (falling < 0 || falling >= LandingSpeedLimit)
        {
            Velocity = Velocity.WithY(-Velocity.Y);
            return;
        }

        // Landed: after a plain fall it is calm; after a kick or a shot it is angry, and faces Samus.
        Velocity = PixelVelocity.Zero;
        _airborne = false;
        if (_mood == Mood.Dropped)
        {
            _mood = Mood.Calm;
        }
        else
        {
            _mood = Mood.Angry;
            _crawl.Speed = Pixels.FromNumber(AggressiveCrawlSpeed);
            Facing = TowardPlayer;
        }

        Clockwise = Facing == Facing.Left;
        _crawl.Start(SurfaceCrawl.Clings.Below, Clockwise);
        _still = true;
        _shellShut = Countdown.Of(ShellClosedUpdates);
        Play("hiding_floor", restart: true);
    }
}
