using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Core;
using Engine.Shared;
using Engine.World;
using Game.Data;
using Godot;

namespace Game.Enemies;

/// <summary>
/// One of the bombs Mother Brain spits. It drops to the floor and bounces along it, each bounce lower than the
/// last, turning back at the far wall; after its last bounce it bursts, and fire races away along the floor both
/// ways. Only the burst of one of Samus's bombs destroys it, and then it may leave something behind.
/// </summary>
/// <remarks>
/// Its data names the shot of its "flare", the fire it leaves, and the effects of its "dust_effect" and
/// "explosion_effect". Its settings say what it may leave when destroyed.
/// </remarks>
public partial class MotherBrainBomb : Projectile, IHasValues
{
    /// <inheritdoc/>
    [Export(PropertyHint.File)]
    public string DataFile { get; set; } = "";

    /// <summary>The data file of the shot of the fire it leaves.</summary>
    [Export, DataValue("flare")]
    public string Flare { get; set; } = "";

    /// <summary>The effect it leaves when one of Samus's bombs destroys it.</summary>
    [Export, DataValue("dust_effect")]
    public string DustEffect { get; set; } = "";

    /// <summary>The effect of its burst.</summary>
    [Export, DataValue("explosion_effect")]
    public string ExplosionEffect { get; set; } = "";

    /// <summary>The sound of its burst.</summary>
    [Export, DataValue("explosion_sound")]
    public string ExplosionSound { get; set; } = "";

    /// <summary>How far down the room the floor it bounces on is.</summary>
    [Export, DataValue("floor")]
    public int Floor { get; set; } = 208;

    /// <summary>How far across the room the wall it turns back at is.</summary>
    [Export, DataValue("right_wall")]
    public int RightWall { get; set; } = 240;

    private static readonly Pixels FallAcceleration = Pixels.FromNumber(7 / 256.0);
    private static readonly Pixels FallDrag = Pixels.FromNumber(1 / 128.0);
    private static readonly Pixels BounceRise = Pixels.FromWhole(-2);
    private static readonly Pixels BounceSpeed = Pixels.FromNumber(7 / 16.0);

    /// <summary>How hard each bounce in turn is pulled back down.</summary>
    private static readonly Pixels[] BounceGravities =
    [
        Pixels.FromNumber(1 / 16.0), Pixels.FromNumber(1 / 8.0), Pixels.FromNumber(1 / 4.0), Pixels.FromNumber(7 / 16.0),
        Pixels.FromNumber(11 / 16.0), Pixels.FromNumber(15 / 16.0), Pixels.FromNumber(19 / 16.0), Pixels.FromNumber(23 / 16.0),
    ];

    private PixelVelocity _speed;
    private int _bounce = -1;

    /// <summary>How many flames follow one another each way from its burst.</summary>
    public int Flames { get; set; }

    /// <inheritdoc/>
    public void ReadValues(DataNode data)
    {
    }

    /// <inheritdoc/>
    public override void _ValidateProperty(Godot.Collections.Dictionary property) => DataValues.Validate(this, property);

    /// <inheritdoc/>
    public override HitResult TakeHit(Hit hit, Collider part)
    {
        if (!hit.Carries(new HitKind(HitKinds.Bomb)))
            return HitResult.Ignored;
        // Its dust is one of the enemies' shots on the cartridge: it holds one of the room's places while it plays, and
        // is not made with none free.
        if (DustEffect.Length > 0)
            Room.Spawn(Effect.Of(FrameTimeline.Read(Room.Content.ReadData(DustEffect)), Position, holdsPlace: true));
        return base.TakeHit(hit, part);
    }

    /// <inheritdoc/>
    protected override void OnLaunched(Room room)
    {
        DataValues.Fill(this, room.Content);
        _speed = Velocity;
        Velocity = PixelVelocity.Zero;
    }

    /// <inheritdoc/>
    protected override void BeforeTravel(Room room)
    {
        if (_bounce >= BounceGravities.Length)
        {
            Burst(room);
            return;
        }

        Pixels pull;
        if (_bounce >= 0)
        {
            pull = BounceGravities[_bounce];
        }
        else
        {
            // On its first fall it loses its speed across as it goes.
            Pixels across = _speed.X.Abs() - FallDrag;
            _speed = _speed.WithX(across.IsNegative ? Pixels.Zero : _speed.X.IsNegative ? -across : across);
            pull = FallAcceleration;
        }

        _speed = _speed.WithY(_speed.Y + pull);
        Position += _speed.OverOneUpdate;
        if (Position.X.Whole >= RightWall)
            _speed = _speed.WithX(-_speed.X);
        if (Position.Y.Whole < Floor)
            return;
        Position = new PixelPosition(Position.X, Position.Y.WithWhole(Floor));
        _speed = new PixelVelocity(_speed.X.IsNegative ? -BounceSpeed : BounceSpeed, BounceRise);
        _bounce++;
    }

    /// <summary>Bursts where it is, and sends its fire along the floor.</summary>
    private void Burst(Room room)
    {
        // It is gone before its fire and its explosion are left, which may so take the place it held; the explosion
        // is one of the enemies' shots on the cartridge, and is not made with no place free.
        Leave(room);
        if (Flare.Length > 0)
        {
            var fire = (RidleyFireball)ShotType.Read(room.Content.ReadData(Flare)).Launch(Position, PixelVelocity.Zero, Side, Launcher, Hit.Source);
            (fire.Afterburns, fire.RunsAcross) = (Flames, true);
            room.Spawn(fire);
        }

        if (ExplosionEffect.Length > 0)
            room.Spawn(Effect.Of(FrameTimeline.Read(room.Content.ReadData(ExplosionEffect)), Position, holdsPlace: true));
        if (ExplosionSound.Length > 0)
            room.Session.Sounds.Request(ExplosionSound, priority: SoundPriority.Low);
    }
}
