using Engine.Actors;
using Engine.Actors.Components;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Game.Compatibility;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Nuclear Waffle: a chain of fire that leaps out of the lava, sweeps over in a great arc and plunges back in on
/// the far side, then waits and does it again. Nothing harms it. Shots pass its head by and stop dead on the four
/// links that burn; the links between those are only seen.
/// </summary>
/// <remarks>
/// Its timeline is "head"; "link" is for the links that burn and "trail" for the ones between them. Its scene has
/// a visual named "Link1" through "Link7" for the links, nearest the head first; the hitbox "Head", which hurts as
/// the enemy does; and for the four links that burn a hitbox named "Burn1" through "Burn4", which hurts as a link
/// does, and a hurtbox named "Guard1" through "Guard4", which is what a shot stops on.
/// </remarks>
public partial class NuclearWaffle : Enemy
{
    /// <summary>How far its sweep turns each update, in 256ths of an angle step.</summary>
    [Export]
    public int TurnSpeed { get; set; } = 256;

    /// <summary>The radius of its arc, in pixels.</summary>
    [Export]
    public int OrbitRadius { get; set; } = 64;

    /// <summary>Whether its sweep turns the other way round.</summary>
    [Export]
    public bool Backward { get; set; }

    /// <summary>How many updates it waits in the lava between sweeps.</summary>
    [Export]
    public int WaitUpdates { get; set; } = 64;

    /// <summary>The middle of its sweep, in angle steps: straight up, a turn on so that the sweep never passes nothing.</summary>
    [Export, DataValue("sweep_middle")]
    public int SweepMiddle { get; set; } = 320;

    /// <summary>How far to either side of its middle the sweep reaches, in angle steps.</summary>
    [Export, DataValue("sweep_reach")]
    public int SweepReach { get; set; } = 80;

    /// <summary>How far to either side of its middle the lava's surface is, in angle steps, where it splashes.</summary>
    [Export, DataValue("surface_reach")]
    public int SurfaceReach { get; set; } = 64;

    /// <summary>How far behind one another its burning links follow, in angle steps; the others lie halfway between.</summary>
    [Export, DataValue("link_spacing")]
    public int LinkSpacing { get; set; } = 24;

    /// <summary>Half the size of a burning link, in pixels.</summary>
    [Export, DataValue("link_half_size")]
    public int LinkHalfSize { get; set; } = 8;

    /// <summary>The damage a burning link does on contact.</summary>
    [Export, DataValue("link_damage")]
    public int LinkDamage { get; set; } = 64;

    /// <summary>The effect of the lava splashing where a part of it breaks the surface, by the data file of its timeline.</summary>
    [Export, DataValue("splash_effect")]
    public string SplashEffect { get; set; } = "";

    /// <summary>The effect thrown up to the left as it breaks the surface.</summary>
    [Export, DataValue("left_burst_effect")]
    public string LeftBurstEffect { get; set; } = "";

    /// <summary>The effect thrown up to the right as it breaks the surface.</summary>
    [Export, DataValue("right_burst_effect")]
    public string RightBurstEffect { get; set; } = "";

    /// <summary>The sound of it breaking the surface on its way out, by name.</summary>
    [Export, DataValue("surface_sound")]
    public string SurfaceSound { get; set; } = "";

    private const int BurningLinks = 4;
    private const int TrailLinks = 3;

    // How far through the surface a part of the chain is: under on the way out, in the air, or under again.
    private enum Passage
    {
        NotYetOut,
        Out,
        BackUnder,
    }

    private readonly Passage[] _burning = new Passage[BurningLinks];
    private readonly Passage[] _trail = new Passage[TrailLinks];
    private readonly PixelOffset[] _burningAt = new PixelOffset[BurningLinks];
    private readonly PixelOffset[] _trailAt = new PixelOffset[TrailLinks];

    /// <summary>
    /// What holds each burning link's place among the room's places for what enemies launch: on the cartridge each is
    /// a shot of its own. Null for a link the room had no place for, which is not there at all. They are taken in
    /// order, so those that got one are always the nearest the head.
    /// </summary>
    private readonly Effect?[] _linkPlaces = new Effect?[BurningLinks];
    private Passage _head;
    private TimelinePlayer? _linkPlayer;
    private TimelinePlayer? _trailPlayer;
    private PixelPosition _origin;
    private Countdown _wait;
    private bool _sweeping;
    private int _angle;

    /// <summary>The angle its sweep starts from, in 256ths of a step.</summary>
    private int StartAngle => (SweepMiddle + (Backward ? SweepReach : -SweepReach)) * 256;

    /// <summary>The angle its sweep ends at.</summary>
    private int EndAngle => (SweepMiddle + (Backward ? -SweepReach : SweepReach)) * 256;

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _origin = PixelPosition.FromWhole(Position.X.Whole, Position.Y.Whole);
        _angle = StartAngle;
        Position = PointAt(_angle);
        _wait = Countdown.Of(WaitUpdates + 1);
        Play("head");
        _linkPlayer = new TimelinePlayer(Timelines.Get("link"));
        _trailPlayer = new TimelinePlayer(Timelines.Get("trail"));
        for (int link = 0; link < BurningLinks; link++)
        {
            _burningAt[link] = PixelOffset.Zero;

            // Its burning links take their places as it is set up, the one nearest the head first, and keep them.
            Effect place = Effect.HoldingPlace(Position);
            _linkPlaces[link] = Room.Spawn(place) ? place : null;
        }

        ShowLinks();
    }

    /// <inheritdoc/>
    protected override void Touch(Collider part)
    {
        if (part.Name == "Head")
        {
            base.Touch(part);
            return;
        }

        // A link burns with a damage of its own.
        var direction = new PixelOffset(Room.Player.Position.X >= Position.X ? Pixels.FromWhole(1) : Pixels.FromWhole(-1), Pixels.Zero);
        Room.Player.TakeHit(new Hit(LinkDamage, HitSide.Enemies, HitKind.Contact) { Direction = direction, Source = this }, Room.Player.Hurtbox);
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        if (!_sweeping)
        {
            _wait = _wait.Stepped();
            if (_wait.IsRunning)
            {
                ShowLinks();
                return;
            }

            _wait = Countdown.Of(WaitUpdates + 1);
            _angle = StartAngle;
            _sweeping = true;
            _head = Passage.NotYetOut;
            Array.Fill(_burning, Passage.NotYetOut);
            Array.Fill(_trail, Passage.NotYetOut);
            AlwaysActive = true;
            ShowLinks();
            return;
        }

        int direction = Backward ? -1 : 1;
        int current = _angle - (_angle % 256);
        PixelPosition head = PixelPosition.FromWhole(Position.X.Whole, Position.Y.Whole);
        Cross(ref _head, current, head);
        Position = PointAt(Clamp(current).Angle);

        // The burning links follow the head, the first half a spacing behind; the others lie between them. The sweep
        // ends as the last burning link there reaches its end: never, when the room had a place for none.
        bool finished = false;
        int angle = current + direction * (LinkSpacing / 2) * 256;
        for (int link = 0; link < BurningLinks && _linkPlaces[link] is not null; link++)
        {
            angle -= direction * LinkSpacing * 256;
            Cross(ref _burning[link], angle, head + _burningAt[link]);
            (int clamped, finished) = Clamp(angle);
            _burningAt[link] = PointAt(clamped) - Position;
        }

        angle = current;
        for (int link = 0; link < TrailLinks; link++)
        {
            angle -= direction * LinkSpacing * 256;
            Cross(ref _trail[link], angle, head + _trailAt[link]);
            _trailAt[link] = PointAt(Clamp(angle).Angle) - Position;
        }

        _angle += direction * TurnSpeed;
        if (finished)
        {
            _sweeping = false;
            AlwaysActive = false;
        }

        ShowLinks();
    }

    /// <summary>Whether an angle of the sweep has come as far as <paramref name="mark"/>, the way the sweep turns.</summary>
    private bool HasReached(int angle, int mark) => Backward ? angle < mark : angle >= mark;

    /// <summary>An angle held within the sweep, and whether it has reached the sweep's end.</summary>
    private (int Angle, bool Finished) Clamp(int angle)
    {
        if (HasReached(angle, EndAngle))
            return (EndAngle, true);
        return HasReached(angle, StartAngle) ? (angle, false) : (StartAngle, false);
    }

    /// <summary>The place on its arc at an angle of the sweep, on whole pixels by the cartridge's short product.</summary>
    private PixelPosition PointAt(int angle) => _origin + CartridgeAngles.CircleOffset(Turns.Wrap(angle / 256), OrbitRadius);

    /// <summary>
    /// Notes how far through the lava's surface a part of the chain at <paramref name="angle"/> has come, and throws
    /// up a splash at <paramref name="at"/> when it has just broken the surface either way.
    /// </summary>
    private void Cross(ref Passage passage, int angle, PixelPosition at)
    {
        int outAt = (SweepMiddle + (Backward ? SurfaceReach : -SurfaceReach)) * 256;
        int underAt = (SweepMiddle + (Backward ? -SurfaceReach : SurfaceReach)) * 256;
        Passage now = HasReached(angle, underAt) ? Passage.BackUnder : HasReached(angle, outAt) ? Passage.Out : Passage.NotYetOut;
        if (now == passage)
            return;
        passage = now;
        // The burst is thrown the way that part of the chain is travelling across.
        bool leftward = now == Passage.Out ? Backward : now == Passage.BackUnder && !Backward;
        if (SplashEffect.Length > 0)
            LeaveEffect(SplashEffect, at);
        string burst = leftward ? LeftBurstEffect : RightBurstEffect;
        if (burst.Length > 0)
            LeaveEffect(burst, at);
        if (now != Passage.BackUnder)
            Sound(SurfaceSound);
    }

    /// <summary>Puts every link's picture and burning part where the link is.</summary>
    private void ShowLinks()
    {
        _linkPlayer?.Step();
        _trailPlayer?.Step();
        for (int link = 0; link < BurningLinks; link++)
        {
            // A link the room had no place for neither burns, stops shots nor shows.
            bool there = _linkPlaces[link] is not null;
            PixelOffset at = _burningAt[link];
            var rect = new PixelRect(at.X.Whole - LinkHalfSize, at.Y.Whole - LinkHalfSize, LinkHalfSize * 2, LinkHalfSize * 2);
            if (GetNodeOrNull<Collider>($"Burn{link + 1}") is { } burn)
            {
                burn.Rect = rect;
                burn.Enabled = there;
            }

            if (GetNodeOrNull<Collider>($"Guard{link + 1}") is { } guard)
            {
                guard.Rect = rect;
                guard.Enabled = there;
            }

            if (_linkPlayer is not null && GetNodeOrNull<ActorVisual>($"Link{link * 2 + 1}") is { } visual)
            {
                visual.Position = new Vector2(at.X.Whole, at.Y.Whole);
                visual.Show(_linkPlayer.Shown);
                visual.SetShown(there);
            }
        }

        for (int link = 0; link < TrailLinks; link++)
        {
            PixelOffset at = _trailAt[link];
            if (_trailPlayer is not null && GetNodeOrNull<ActorVisual>($"Link{link * 2 + 2}") is { } visual)
            {
                visual.Position = new Vector2(at.X.Whole, at.Y.Whole);
                visual.Show(_trailPlayer.Shown);
            }
        }
    }
}
