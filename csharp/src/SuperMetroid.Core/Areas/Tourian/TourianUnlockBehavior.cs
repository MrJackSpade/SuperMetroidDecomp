using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// The Tourian statues' unlock effects (#1801, #1956): the projectile behaviour that spawns and moves the descending
/// statue's dust, the eyes' glows, the released souls, the particles and their tails, and the splashes at the room's water
/// surface. The release's cues spawn more of the effects it spawns.
/// </summary>
[RoomLifetime]
internal sealed class TourianUnlockBehavior : TourianStatueProjectileBehavior<TourianStatueEffectMotion>
{
    /// <summary>The room's randomness, which aims the particles.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>The statue room, whose water surface the particles splash through.</summary>
    private readonly ITourianStatueRoom _statueRoom;
    /// <summary>Spawns and sets up the statue, dust, particle and splash projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Earthquake state written by the statue release cue.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Audio queue receiving the statue eye release sound.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>
    /// The entrance statue, whose scene look every unlock effect spawns in and which shows each later look on it, and
    /// whose eyes each release lights.
    /// </summary>
    private readonly TourianEntranceStatueBehavior _statue;


    /// <summary>Binds the unlock effects to the current room's projectile, audio, FX and statue owners.</summary>
    internal TourianUnlockBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles, RoomEarthquake earthquake, EnemyAudioQueue audio,
        TourianEntranceStatueBehavior statue, ITourianStatueRoom statueRoom)
        : base(StartingMotions)
    {
        _room = room;
        _statueRoom = Ensure.NotNull(statueRoom);
        _projectiles = projectiles;
        _earthquake = earthquake;
        _audio = audio;
        _statue = statue;
    }

    /// <summary>The room line of the statue room's water surface, which the particles cross and their splashes ride.</summary>
    private int WaterSurface => _statueRoom.WaterSurface;

    /// <summary>
    /// Whether <paramref name="projectile"/> lies below the water surface: the signed distance from it down to the surface,
    /// across the room's wrapping lines, is negative, so a particle thrown above the room's top lies above the water.
    /// </summary>
    private bool IsBelowWaterSurface(EnemyProjectile projectile) =>
        DistanceAcrossRoomEdge.LiesBefore(WaterSurface, projectile.Position.Y.Whole);
    /// <summary>Creates the descending Tourian statue's fixed-position dust puffs.</summary>
    internal void SpawnTourianDescentDust()
    {
        for (int count = 0; count < TourianStatueSequenceDefinitions.DescentDustPuffs; count++)
        {
            var projectile = _projectiles.AllocateEnemyProjectile();
            if (projectile is null) continue;
            EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
                projectile,
                RoomEnemyProjectileKind.TourianStatueDescentDust,
                Effects,
                _statue.SceneGraphics);
            _statue.HoldSceneProjectile(projectile);
            projectile.StartAnimation(new ChozoTourianDustAnimation(ChozoTourianDustAnimations.TourianDescentDust));
            projectile.Anchor = TourianStatueSequenceDefinitions.DescentDustAnchor;
        }
    }

    /// <summary>Spawns the glow of a statue's <paramref name="eye"/> and lights the eye's colours.</summary>
    public void SpawnTourianEyeGlow(TourianStatueEye eye)
    {
        if (SpawnAtStatueEye(RoomEnemyProjectileKind.TourianStatueEyeGlow, TourianStatueProjectileSegment.EyeGlow, eye) is null) return;
        _statue.ShowEyeGlow(eye.Glow);
    }

    /// <summary>Spawns a statue's released soul, rising from its <paramref name="eye"/>.</summary>
    public void SpawnTourianSoul(TourianStatueEye eye)
    {
        if (SpawnAtStatueEye(RoomEnemyProjectileKind.TourianStatueSoul, TourianStatueProjectileSegment.Soul, eye) is not { } soul) return;
        soul.Velocity = soul.Velocity.WithY(TourianStatueSoulDefinitions.LaunchSpeed);
    }

    /// <summary>
    /// Allocates an unlock effect of <paramref name="kind"/>, starting its animation at <paramref name="start"/>, at a
    /// statue's <paramref name="eye"/>; null when the room already holds as many enemy projectiles as it can.
    /// </summary>
    private EnemyProjectile? SpawnAtStatueEye(RoomEnemyProjectileKind kind, TourianStatueProjectileSegment start, TourianStatueEye eye)
    {
        Ensure.NotNull(eye);
        var projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null) return null;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, kind, Effects, _statue.SceneGraphics);
        _statue.HoldSceneProjectile(projectile);
        projectile.StartAnimation(new TourianStatueProjectileAnimation(this, start));
        projectile.Position = projectile.Position.WithWhole(eye.Position);
        return projectile;
    }

    /// <summary>Spawns the splash of <paramref name="parent"/>, a particle crossing the water surface, held at the surface.</summary>
    private void SpawnTourianSplash(EnemyProjectile parent)
    {
        if (SpawnAtParent(parent, RoomEnemyProjectileKind.TourianStatueSplash, TourianStatueProjectileSegment.Splash) is not { } splash)
            return;
        splash.Position = splash.Position.WithWholeY(WaterSurface - TourianStatueParticleDefinitions.SplashHeightAboveSurface);
    }

    /// <summary>Spawns a particle thrown from <paramref name="parent"/> at a random angle.</summary>
    private void SpawnTourianParticle(EnemyProjectile parent)
    {
        if (SpawnAtParent(parent, RoomEnemyProjectileKind.TourianStatueParticle, TourianStatueProjectileSegment.Particle) is not { } particle)
            return;
        // A particle leaves within an eighth of a turn either side of the zero angle; one below zero lies near a full turn.
        TrigonometryAngle angle = TrigonometryAngle.Wrapped(
            _room.Random.Next(TourianStatueParticleDefinitions.LaunchAngle) - TourianStatueParticleDefinitions.LaunchAngleCentring);
        // The particle flies its across speed at the angle's sine, and its vertical speed up or down at its negative cosine,
        // the sine a quarter turn back.
        particle.Velocity = particle.Velocity.WithX(Trigonometry.SineVelocity(angle, TourianStatueParticleDefinitions.LaunchAcrossPixels));
        particle.Velocity = particle.Velocity.WithY(Trigonometry.SineVelocity(angle - TrigonometryAngle.QuarterTurn,
            TourianStatueParticleDefinitions.LaunchVerticalPixels));
    }

    /// <summary>Spawns a tail behind <paramref name="parent"/>, which holds still where it spawned.</summary>
    private void SpawnTourianTail(EnemyProjectile parent) =>
        _ = SpawnAtParent(parent, RoomEnemyProjectileKind.TourianStatueTail, TourianStatueProjectileSegment.Tail);

    /// <summary>
    /// Allocates an unlock effect of <paramref name="kind"/>, starting its animation at <paramref name="start"/>, at
    /// <paramref name="parent"/>; null when the room already holds as many enemy projectiles as it can.
    /// </summary>
    private EnemyProjectile? SpawnAtParent(EnemyProjectile parent, RoomEnemyProjectileKind kind, TourianStatueProjectileSegment start)
    {
        Ensure.NotNull(parent);
        var projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null) return null;
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, kind, Effects, _statue.SceneGraphics);
        _statue.HoldSceneProjectile(projectile);
        projectile.StartAnimation(new TourianStatueProjectileAnimation(this, start));
        projectile.Position = projectile.Position.WithWhole(parent.Position.Whole);
        return projectile;
    }

    /// <summary>Holds a splash at the water surface.</summary>
    private void HoldTourianStatueSplash(EnemyProjectile projectile) =>
        projectile.Position = projectile.Position.WithWholeY(WaterSurface - TourianStatueParticleDefinitions.SplashHeightAboveSurface);

    /// <summary>
    /// Raises a soul, accelerating upward, and ends it once it rises above the room's top (#2424): it leaves in its own
    /// update, before its animation's step, and the velocity it leaves in its place for the next spawn there
    /// (<see cref="Compatibility.EnemyProjectileHandOver"/>) already holds this update's acceleration.
    /// </summary>
    private static void RaiseTourianStatueSoul(EnemyProjectile projectile)
    {
        projectile.Position = projectile.Position.AdvanceY(projectile.Velocity.Y.ToSixteenSixteenDelta());
        projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y - TourianStatueSoulDefinitions.RiseAcceleration);
        if (OffRoomPlaces.LiesBeforeRoom(projectile.Position.Y.Whole))
            projectile.Delete();
    }

    /// <summary>
    /// Drops a particle under gravity, splashing as it crosses the water surface and ending it, in its own update before its
    /// animation's step (#2424), once it sinks past the bottom of the statues' screen. A particle thrown above the room's
    /// top falls back through it.
    /// </summary>
    private void DropTourianStatueParticle(EnemyProjectile projectile)
    {
        // A splash marks the update the particle crosses the surface in either direction.
        bool submergedBefore = IsBelowWaterSurface(projectile);
        projectile.Position = projectile.Position.Advance(projectile.Velocity.ToDisplacement());
        bool submergedAfter = IsBelowWaterSurface(projectile);
        if (submergedBefore != submergedAfter)
            SpawnTourianSplash(projectile);
        if (OffRoomPlaces.Signed(projectile.Position.Y.Whole) >= TourianStatueParticleDefinitions.SunkDepth)
            projectile.Delete();
        else projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y + TourianStatueParticleDefinitions.Gravity);
    }

    /// <summary>
    /// The unlock's effects (#1760): the descending statue's dust, the eyes' glows, the released souls, particles and their
    /// tails, and the splashes. Their spawners start their animations.
    /// </summary>
    private EnemyProjectileBehavior Effects => this;

    /// <summary>A splash holds at the water, a particle falls and a soul rises from their spawn; the dust, the eye glows and the tails hold still.</summary>
    private static readonly Dictionary<RoomEnemyProjectileKind, TourianStatueEffectMotion?> StartingMotions = new()
    {
        [RoomEnemyProjectileKind.TourianStatueSplash] = TourianStatueEffectMotion.SplashHolding,
        [RoomEnemyProjectileKind.TourianStatueParticle] = TourianStatueEffectMotion.ParticleFalling,
        [RoomEnemyProjectileKind.TourianStatueSoul] = TourianStatueEffectMotion.SoulRising,
        [RoomEnemyProjectileKind.TourianStatueEyeGlow] = null,
        [RoomEnemyProjectileKind.TourianStatueTail] = null,
        [RoomEnemyProjectileKind.TourianStatueDescentDust] = null,
    };

    /// <summary>Moves one of the unlock's effects: the splash's hold, the particle's fall and the soul's rise.</summary>
    /// <param name="context">The projectile and what its pass hands every motion.</param>
    /// <param name="motion">The effect's motion.</param>
    internal override void Move(EnemyProjectileMotionContext context, TourianStatueEffectMotion motion)
    {
        switch (motion)
        {
            case TourianStatueEffectMotion.SplashHolding:
                HoldTourianStatueSplash(context.Projectile);
                break;
            case TourianStatueEffectMotion.ParticleFalling:
                DropTourianStatueParticle(context.Projectile);
                break;
            case TourianStatueEffectMotion.SoulRising:
                RaiseTourianStatueSoul(context.Projectile);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(TourianStatueEffectMotion)} {motion}.");
        }
    }

    /// <summary>
    /// Handles a cue of the statues' release: its sound, its earthquake, or a particle or tail spawned at the projectile.
    /// </summary>
    /// <param name="projectile">The projectile whose animation published the cue.</param>
    /// <param name="cue">The cue the animation published.</param>
    private protected override void RunReleaseCue(EnemyProjectile projectile, TourianStatueProjectileCue cue)
    {
        switch (cue)
        {
            case TourianStatueProjectileCue.Signal { Kind: TourianStatueProjectileCueKind.PlayReleaseSound }:
                _audio.QueueEnemySound(EnemySound.TourianStatueParticle, priority: SoundRequestPriority.Normal);
                break;
            case TourianStatueProjectileCue.Signal { Kind: TourianStatueProjectileCueKind.Earthquake }:
                _earthquake.Sustain(TourianStatueSequenceDefinitions.ReleaseEarthquake);
                break;
            case TourianStatueProjectileCue.Signal { Kind: TourianStatueProjectileCueKind.SpawnParticle }:
                SpawnTourianParticle(projectile);
                break;
            case TourianStatueProjectileCue.Signal { Kind: TourianStatueProjectileCueKind.SpawnTail }:
                SpawnTourianTail(projectile);
                break;
            default:
                base.RunReleaseCue(projectile, cue);
                break;
        }
    }
}
