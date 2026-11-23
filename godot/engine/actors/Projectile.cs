using Engine.Actors.Components;
using Engine.Collision;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Engine.Actors;

/// <summary>
/// Something launched to deliver a hit, living on its own once launched. It moves, hits what it meets, and removes
/// itself. It never decides what its target does about the hit, and never knows what its target is. How it behaves is
/// its <see cref="ProjectileSettings"/>; a projectile with behaviour no setting gives is a class on this one.
/// </summary>
public partial class Projectile : Node2D, IRoomEntity, IHittable
{
    private Room? _room;
    private Hit? _hit;
    private ProjectileSettings _settings = new();
    private ICollisionOwner? _launcher;
    private Collider? _hurtbox;
    private ActorVisual? _visual;
    private TimelinePlayer? _timeline;
    private Countdown _left;
    private int _wavePhase;
    private Pixels _waveOffset;
    private string _direction = "right";
    private readonly HashSet<IHittable> _alreadyHurt = [];

    /// <summary>A projectile delivering <paramref name="hit"/>, launched from <paramref name="at"/> at <paramref name="velocity"/>.</summary>
    /// <param name="hit">The hit it delivers.</param>
    /// <param name="at">Its centre as it is launched.</param>
    /// <param name="velocity">Its launch speed.</param>
    /// <param name="settings">How it behaves.</param>
    /// <param name="launcher">Whatever launched it, which it never hits.</param>
    public static Projectile Of(Hit hit, PixelPosition at, PixelVelocity velocity, ProjectileSettings settings, ICollisionOwner? launcher)
    {
        var projectile = new Projectile { Name = "Projectile" };
        projectile.Launch(hit, at, velocity, settings, launcher);
        return projectile;
    }

    /// <summary>
    /// Gives a projectile made from a scene, or by a class of its own, what <see cref="Of"/> gives a plain one. Called
    /// once, before it is spawned into its room.
    /// </summary>
    public void Launch(Hit hit, PixelPosition at, PixelVelocity velocity, ProjectileSettings settings, ICollisionOwner? launcher)
    {
        ArgumentNullException.ThrowIfNull(hit);
        ArgumentNullException.ThrowIfNull(settings);
        _hit = hit;
        Position = at;
        Velocity = velocity;
        _settings = settings;
        _launcher = launcher;
        _direction = DirectionName(velocity);
        _wavePhase = settings.WaveStart;
    }

    /// <inheritdoc/>
    public new PixelPosition Position { get; set; }

    /// <summary>Its speed.</summary>
    public PixelVelocity Velocity { get; set; }

    /// <summary>The hit it delivers.</summary>
    public Hit Hit => _hit ?? throw new InvalidOperationException("The projectile has no hit.");

    /// <summary>How it behaves.</summary>
    public ProjectileSettings Settings => _settings;

    /// <summary>Whose it is.</summary>
    public HitSide Side => Hit.Side;

    /// <summary>Whatever launched it.</summary>
    public ICollisionOwner? Launcher => _launcher;

    /// <summary>The room it is in.</summary>
    protected Room Room => _room ?? throw new InvalidOperationException("The projectile is not in a room.");

    /// <summary>The pixels it covers, in the room.</summary>
    public PixelRect Area => PixelRect.Around(Position.X.Whole, Position.Y.Whole, _settings.Width / 2, _settings.Height / 2);

    /// <summary>Whether it has ended.</summary>
    public bool HasEnded { get; private set; }

    /// <summary>How many updates it has been in its room.</summary>
    public int Age { get; private set; }

    /// <inheritdoc/>
    public void EnterRoom(Room room)
    {
        _room = room;
        _left = Countdown.Of(_settings.LastsUpdates);
        if (_settings.MetByShot != ProjectileSettings.MetBy.Ignored)
        {
            _hurtbox = new Collider
            {
                Name = "Hurtbox",
                Role = ColliderRole.Hurtbox,
                Rect = new PixelRect(-_settings.Width / 2, -_settings.Height / 2, _settings.Width, _settings.Height),
            };
            AddChild(_hurtbox);
            room.Collision.Register(_hurtbox);
        }

        string picture = Named(_settings.Picture);
        if (_settings.Timeline.Length > 0)
        {
            _timeline = new TimelinePlayer(FrameTimeline.Read(room.Content.ReadData(Named(_settings.Timeline))));
            picture = _timeline.Current.Picture;
        }

        if (picture.Length > 0 || _timeline is not null)
        {
            (int x, int y) = _settings.PictureOffset ?? (-_settings.Width / 2, -_settings.Height / 2);
            _visual = new ActorVisual { Picture = picture, PictureOffset = new Vector2I(x, y) };
            AddChild(_visual);
            _visual.Bind(room.Content);
        }

        OnLaunched(room);
    }

    /// <summary>
    /// A picture's or timeline's name with its placeholders filled in: "{direction}" with the way the projectile was
    /// launched, and "{side}" with "left" or "right", the way across it is going now.
    /// </summary>
    private string Named(string name) => name
        .Replace("{direction}", _direction, StringComparison.Ordinal)
        .Replace("{side}", Velocity.X.IsNegative ? "left" : "right", StringComparison.Ordinal);

    /// <summary>Starts the projectile on another timeline, by its data file.</summary>
    protected void PlayTimeline(string file)
    {
        _timeline = new TimelinePlayer(FrameTimeline.Read(Room.Content.ReadData(Named(file))));
        _visual?.Show(_timeline.Current.Picture);
    }

    /// <summary>Called once as the projectile enters its room.</summary>
    protected virtual void OnLaunched(Room room)
    {
    }

    /// <summary>Plays one update: the projectile changes speed as its motion says, moves, and meets what is in its way.</summary>
    internal void Step()
    {
        Room room = Room;
        if (HasEnded)
            return;

        Age++;
        if (_timeline is not null)
        {
            foreach (string cue in _timeline.Step())
            {
                OnCue(room, cue);
                if (HasEnded)
                    return;
            }

            if (_timeline.IsOver && _settings.LastsUpdates == 0)
            {
                End(room);
                return;
            }

            _visual?.Show(_timeline.Current.Picture);
        }

        BeforeTravel(room);
        if (HasEnded)
            return;
        ChangeSpeed(room);
        Travel(room);
        if (HasEnded)
            return;

        if (_settings.LastsUpdates > 0)
        {
            _left = _left.Stepped();
            if (_left.IsOver)
            {
                End(room);
                return;
            }
        }

        if (_settings.ViewMargin >= 0)
        {
            PixelRect view = room.Session.Camera.View;
            int margin = _settings.ViewMargin;
            var allowed = new PixelRect(view.Left - margin, view.Top - margin, view.Width + margin * 2, view.Height + margin * 2);
            if (!allowed.Overlaps(Area))
                Leave(room);
        }
    }

    /// <summary>Called for each cue the projectile's timeline reaches. A projectile of its own class acts on its cues here.</summary>
    protected virtual void OnCue(Room room, string cue)
    {
    }

    /// <summary>Called each update before the projectile changes speed and travels. A projectile of its own class acts here.</summary>
    protected virtual void BeforeTravel(Room room)
    {
    }

    /// <inheritdoc/>
    public bool IsHitBy(HitSide side) => _settings.MetByShot != ProjectileSettings.MetBy.Ignored && side != Side && !HasEnded;

    /// <inheritdoc/>
    public HitResult TakeHit(Hit hit, Collider part)
    {
        Room room = Room;
        switch (_settings.MetByShot)
        {
            case ProjectileSettings.MetBy.Destroyed:
                // One that leaves something when shot down rolls for it as an enemy would.
                if (_settings.Drops is { } drops)
                {
                    Inventory inventory = room.Player.Inventory;
                    string? drop = drops.Roll(room.Session.Random, inventory, file => PickupType.Read(room.Content.ReadData(file)).IsUsefulTo(inventory));
                    if (drop is not null)
                        room.Spawn(Pickup.Of(PickupType.Read(room.Content.ReadData(drop)), Position));
                }

                End(room);
                return HitResult.Destroyed;
            case ProjectileSettings.MetBy.StopsShot:
                return HitResult.Absorbed;
            default:
                return HitResult.Ignored;
        }
    }

    /// <inheritdoc/>
    public void ShowPosition() => EntityNode.Show(this, Position);

    /// <summary>The name of the way a projectile launched at <paramref name="velocity"/> is going, for choosing its picture.</summary>
    public static string DirectionName(PixelVelocity velocity)
    {
        int across = velocity.X.IsZero ? 0 : velocity.X.IsNegative ? -1 : 1;
        int down = velocity.Y.IsZero ? 0 : velocity.Y.IsNegative ? -1 : 1;
        return (across, down) switch
        {
            (1, -1) => "up_right",
            (0, -1) => "up",
            (-1, -1) => "up_left",
            (-1, 0) => "left",
            (-1, 1) => "down_left",
            (0, 1) => "down",
            (1, 1) => "down_right",
            _ => "right",
        };
    }

    /// <summary>Changes the projectile's speed for this update, as its motion says.</summary>
    private void ChangeSpeed(Room room)
    {
        switch (_settings.Motion)
        {
            case ProjectileSettings.Moves.Straight:
            case ProjectileSettings.Moves.Wave:
                break;
            case ProjectileSettings.Moves.Still:
                Velocity = PixelVelocity.Zero;
                break;
            case ProjectileSettings.Moves.Accelerating:
                Velocity = new PixelVelocity(Faster(Velocity.X), Faster(Velocity.Y));
                break;
            case ProjectileSettings.Moves.Falling:
                bool wasRising = Velocity.Y.IsNegative;
                Velocity = Velocity.WithY(Limit(Velocity.Y + _settings.Acceleration));
                if (wasRising && !Velocity.Y.IsNegative && _settings.FallingTimeline.Length > 0 && _visual is not null)
                    PlayTimeline(_settings.FallingTimeline);
                break;
            case ProjectileSettings.Moves.Homing:
                PixelPosition target = Side == HitSide.Player ? NearestEnemy(room) ?? Position : room.Player.Position;
                Velocity = new PixelVelocity(Toward(Velocity.X, target.X - Position.X), Toward(Velocity.Y, target.Y - Position.Y));
                break;
            default:
                throw new InvalidOperationException($"Undefined projectile motion {_settings.Motion}.");
        }
    }

    /// <summary>Moves the projectile for this update a pixel at a time, meeting terrain and targets as it goes.</summary>
    private void Travel(Room room)
    {
        PixelOffset offset = Velocity.OverOneUpdate;
        if (_settings.Motion == ProjectileSettings.Moves.Wave)
        {
            // The swing is across the line of travel: up and down for a shot travelling sideways, and the reverse.
            _wavePhase += _settings.WaveStep;
            Pixels swing = Turns.Sine(_wavePhase, _settings.WaveSize);
            Pixels change = swing - _waveOffset;
            _waveOffset = swing;
            offset = Velocity.X.Abs() >= Velocity.Y.Abs() ? offset with { Y = offset.Y + change } : offset with { X = offset.X + change };
        }

        PixelPosition target = Position + offset;
        int acrossPixels = target.X.Whole - Position.X.Whole, downPixels = target.Y.Whole - Position.Y.Whole;
        int steps = Math.Max(Math.Abs(acrossPixels), Math.Abs(downPixels));
        for (int step = 1; step <= steps; step++)
        {
            var next = new PixelPosition(
                Pixels.FromWhole(Position.X.Whole + Math.Sign(acrossPixels) * (Math.Abs(acrossPixels) * step / steps - Math.Abs(acrossPixels) * (step - 1) / steps)),
                Pixels.FromWhole(Position.Y.Whole + Math.Sign(downPixels) * (Math.Abs(downPixels) * step / steps - Math.Abs(downPixels) * (step - 1) / steps)));
            if (!Advance(room, next))
                return;
        }

        if (steps == 0)
        {
            if (!Advance(room, Position))
                return;
        }

        Position = new PixelPosition(target.X.WithWhole(Position.X.Whole), target.Y.WithWhole(Position.Y.Whole));
    }

    /// <summary>
    /// Moves the projectile to <paramref name="next"/> and meets what is there. Returns whether it carries on travelling
    /// this update.
    /// </summary>
    private bool Advance(Room room, PixelPosition next)
    {
        PixelPosition previous = Position;
        Position = next;
        PixelRect area = Area;
        bool passes = _settings.MeetingTerrain is ProjectileSettings.Terrain.PassesThrough or ProjectileSettings.Terrain.PassesThroughAndHits;
        bool hitsTerrain = _settings.DeliversHit &&
            _settings.MeetingTerrain is ProjectileSettings.Terrain.EndsAndHits or ProjectileSettings.Terrain.PassesThroughAndHits;

        if (passes && hitsTerrain)
            room.Terrain.TakeHit(Hit, area);

        if (!passes && (room.Terrain.IsSolidIn(area, SlopeTreatment.Wall) ||
                        room.Collision.Overlapping(area, ColliderRole.Solid, except: _launcher).Count > 0))
        {
            if (hitsTerrain)
                room.Terrain.TakeHit(Hit, area);
            if (_settings.MeetingTerrain == ProjectileSettings.Terrain.Bounces)
            {
                bool acrossBlocked = room.Terrain.IsSolidIn(Area.MovedBy(0, previous.Y.Whole - next.Y.Whole), SlopeTreatment.Wall);
                Velocity = acrossBlocked ? Velocity.WithX(-Velocity.X) : Velocity.WithY(-Velocity.Y);
                Position = previous;
                return false;
            }

            Position = previous;
            OnMetTerrain(room);
            End(room);
            return false;
        }

        if (!_settings.DeliversHit)
            return true;

        if (_settings.HitsEachUpdate)
            _alreadyHurt.Clear();
        _alreadyHurt.Add(this);
        foreach ((IHittable target, HitResult result) in room.DeliverHit(Hit, area, from: _launcher, skipping: _alreadyHurt).ToList())
        {
            OnMetTarget(room, target, result);
            if (_settings.HitsEachUpdate)
            {
                _alreadyHurt.Add(target);
                continue;
            }

            switch (result)
            {
                case HitResult.Ignored:
                    break;
                case HitResult.TurnedBack:
                    // Turned back, it flies the other way and is now the other side's.
                    Velocity = -Velocity;
                    _hit = Hit with { Side = Side == HitSide.Player ? HitSide.Enemies : HitSide.Player, Direction = -Hit.Direction };
                    _launcher = target as ICollisionOwner;
                    return false;
                case HitResult.Absorbed:
                    End(room);
                    return false;
                case HitResult.Hurt:
                case HitResult.Destroyed:
                    if (_settings.MeetingTarget == ProjectileSettings.Target.Ends)
                    {
                        End(room);
                        return false;
                    }

                    if (_settings.MeetingTarget == ProjectileSettings.Target.PassesThrough)
                        _alreadyHurt.Add(target);
                    break;
                default:
                    throw new InvalidOperationException($"Undefined hit result {result}.");
            }
        }

        return true;
    }

    /// <summary>Called as the projectile is stopped by terrain or a solid, before it ends.</summary>
    protected virtual void OnMetTerrain(Room room)
    {
    }

    /// <summary>Called for each target the projectile's hit landed on, with what the hit did.</summary>
    protected virtual void OnMetTarget(Room room, IHittable target, HitResult result)
    {
    }

    /// <summary>Ends the projectile where it is, leaving its ending effect, burst and sound if it has them.</summary>
    protected void End(Room room)
    {
        if (HasEnded)
            return;
        if (_settings.EndingEffect.Length > 0)
            room.Spawn(Effect.Of(FrameTimeline.Read(room.Content.ReadData(_settings.EndingEffect)), Position));
        if (_settings.EndingBurst.Length > 0)
            room.Spawn(ShotType.Read(room.Content.ReadData(_settings.EndingBurst)).Launch(Position, PixelVelocity.Zero, Side, _launcher, Hit.Source));
        if (_settings.EndingSound.Length > 0)
            room.Session.Sounds.Request(_settings.EndingSound);
        OnEnded(room);
        Leave(room);
    }

    /// <summary>Called as the projectile ends, after it has left what its settings say it leaves.</summary>
    protected virtual void OnEnded(Room room)
    {
    }

    /// <summary>Takes the projectile out of the room with no ending.</summary>
    protected void Leave(Room room)
    {
        HasEnded = true;
        room.Remove(this);
    }

    /// <summary>A speed along one axis, gained on in the direction it already has.</summary>
    private Pixels Faster(Pixels speed) =>
        speed.IsZero ? speed : Limit(speed.IsNegative ? speed - _settings.Acceleration : speed + _settings.Acceleration);

    /// <summary>A speed along one axis, steered toward where its target lies along that axis.</summary>
    private Pixels Toward(Pixels speed, Pixels toTarget) =>
        toTarget.IsZero ? speed : Limit(toTarget.IsNegative ? speed - _settings.Acceleration : speed + _settings.Acceleration);

    /// <summary>A speed along one axis, held to the settings' limit.</summary>
    private Pixels Limit(Pixels speed)
    {
        Pixels limit = _settings.SpeedLimit;
        if (limit.IsZero)
            return speed;
        return speed > limit ? limit : speed < -limit ? -limit : speed;
    }

    /// <summary>Where the nearest live enemy is, or null when there is none.</summary>
    private PixelPosition? NearestEnemy(Room room)
    {
        Enemy? nearest = null;
        long best = long.MaxValue;
        foreach (Enemy enemy in room.Enemies)
        {
            if (enemy.IsDead)
                continue;
            long across = enemy.Position.X.Whole - Position.X.Whole, down = enemy.Position.Y.Whole - Position.Y.Whole;
            long distance = across * across + down * down;
            if (distance < best)
            {
                best = distance;
                nearest = enemy;
            }
        }

        return nearest?.Position;
    }
}
