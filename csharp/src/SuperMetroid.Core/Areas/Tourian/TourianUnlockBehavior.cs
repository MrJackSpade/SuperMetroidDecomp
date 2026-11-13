using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>The Tourian unlock species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class TourianUnlockBehavior
{
    /// <summary>Live room randomness.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocator and initializer for statue, dust, particle, and splash projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Earthquake state written by the statue release cue.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Audio queue receiving the statue eye release sound.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>The entrance statue, whose scene look every unlock effect shows and whose eyes each release lights.</summary>
    private readonly TourianEntranceStatueBehavior _statue;


    /// <summary>Binds the unlock effects to the current room's projectile, audio, FX and statue owners.</summary>
    internal TourianUnlockBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles, RoomEarthquake earthquake, EnemyAudioQueue audio, TourianEntranceStatueBehavior statue)
    {
        _room = room;
        _projectiles = projectiles;
        _earthquake = earthquake;
        _audio = audio;
        _statue = statue;
    }

    /// <summary>How far above the water surface a splash sits.</summary>
    private static readonly Offset16<RoomPixels> SplashAboveSurface = new(0, -4);

    /// <summary>The upward acceleration of a rising statue soul each update.</summary>
    private static readonly SignedEightEight SoulRiseAcceleration = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(128);

    /// <summary>The gravity a falling statue particle gains each update.</summary>
    private static readonly SignedEightEight ParticleGravity = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(16);

    /// <summary>The water surface the unlocking particles cross and their splashes ride; only its Y axis is meaningful.</summary>
    internal Position16<RoomPixels> TourianStatueWaterSurface { get; set; }
    /// <summary>The point the descending statue's dust puffs return to whenever they restart.</summary>
    private static readonly Position16<RoomPixels> TourianDescentDustAnchor = new(128, 188);

    /// <summary>Creates four fixed-position dust actors for the descending Tourian statue sequence.</summary>
    internal void SpawnTourianDescentDust()
    {
        for (int count = 0; count < 4; count++)
        {
            var projectile = _projectiles.AllocateEnemyProjectile();
            if (projectile is null) continue;
            TourianStatueProjectileStarts.Initialize(
                projectile,
                RoomEnemyProjectileKind.TourianStatueDescentDust,
                _statue.SceneGraphics);
            projectile.State = new AnchoredProjectileState(TourianDescentDustAnchor);
        }
    }

    /// <summary>Spawns the glow of a statue's <paramref name="eye"/> and lights the eye's colours.</summary>
    public void SpawnTourianEyeGlow(TourianStatueEye eye)
    {
        if (SpawnAtStatueEye(RoomEnemyProjectileKind.TourianStatueEyeGlow, eye) is null) return;
        _statue.ShowEyeGlow(eye.Glow);
    }

    /// <summary>Spawns a statue's released soul, rising from its <paramref name="eye"/>.</summary>
    public void SpawnTourianSoul(TourianStatueEye eye)
    {
        if (SpawnAtStatueEye(RoomEnemyProjectileKind.TourianStatueSoul, eye) is not { } soul) return;
        soul.Velocity = soul.Velocity.WithY(SignedEightEight.FromPixels(-4));
    }

    /// <summary>Allocates an unlock effect at a statue's <paramref name="eye"/>; null when every slot is busy.</summary>
    private EnemyProjectile? SpawnAtStatueEye(RoomEnemyProjectileKind kind, TourianStatueEye eye)
    {
        Ensure.NotNull(eye);
        var projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null) return null;
        TourianStatueProjectileStarts.Initialize(projectile, kind, _statue.SceneGraphics);
        projectile.Position = projectile.Position.WithWhole(eye.Position);
        return projectile;
    }

    /// <summary>Spawns a splash, particle, or tail at its parent, applying water height or randomized motion.</summary>
    internal void SpawnTourianParticleChild(EnemyProjectile parent, RoomEnemyProjectileKind kind)
    {
        if (kind is not (RoomEnemyProjectileKind.TourianStatueSplash or RoomEnemyProjectileKind.TourianStatueParticle or
                RoomEnemyProjectileKind.TourianStatueTail))
            throw new InvalidOperationException($"{kind} is not a Tourian statue child projectile.");
        var projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null) return;
        TourianStatueProjectileStarts.Initialize(projectile, kind, _statue.SceneGraphics);
        projectile.Position = projectile.Position.WithWhole(parent.Position.Whole);
        if (kind == RoomEnemyProjectileKind.TourianStatueSplash)
            projectile.Position = projectile.Position.WithWholeY((TourianStatueWaterSurface + SplashAboveSurface).Y);
        if (kind != RoomEnemyProjectileKind.TourianStatueParticle) return;
        // A particle leaves within an eighth of a turn either side of the zero angle; one below zero lies near a full turn.
        TrigonometryAngle angle = TrigonometryAngle.Wrapped(_room.Random.Next(TourianStatueParticleDefinitions.LaunchAngle) - 32);
        // The samples lie within one pixel either way, so four times one stays within four. The vertical speed is the
        // sine a quarter turn back: the angle's negative cosine.
        projectile.Velocity = projectile.Velocity.WithX(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(Trigonometry.SignedSine(angle)));
        projectile.Velocity = projectile.Velocity.WithY(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(
            4 * Trigonometry.SignedSine(angle - TrigonometryAngle.QuarterTurn)));
    }

    /// <summary>Holds a splash at the water surface.</summary>
    private void HoldTourianStatueSplash(EnemyProjectile projectile) =>
        projectile.Position = projectile.Position.WithWholeY((TourianStatueWaterSurface + SplashAboveSurface).Y);

    /// <summary>Raises a soul, accelerating upward, and deletes it once it leaves the room.</summary>
    internal static void RaiseTourianStatueSoul(EnemyProjectile projectile)
    {
        projectile.Position = projectile.Position.AdvanceY(projectile.Velocity.Y.ToSixteenSixteenDelta());
        // The soul is deleted while its Y lies in an odd 256-pixel screen row. Rising from the statues'
        // top screen row, that first happens when it wraps above the room's top edge.
        int soulScreenRow = RoomGridConversions.PixelToScreen(projectile.Position.Y.Whole);
        if (soulScreenRow % 2 != 0)
        {
            projectile.StartAnimation(new TourianStatueProjectileAnimation(TourianStatueProjectileSegment.Delete));
        }
        projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y - SoulRiseAcceleration);
    }

    /// <summary>Drops a particle under gravity, splashing as it crosses the water surface and deleting it below the room.</summary>
    private void DropTourianStatueParticle(EnemyProjectile projectile)
    {
        // A splash marks the update the particle crosses the surface in either direction: the wrapped
        // signed distance from the particle down to the surface changes sign.
        bool submergedBefore = (TourianStatueWaterSurface - projectile.Position.Whole).Y < 0;
        projectile.Position = projectile.Position.Advance(projectile.Velocity.ToSixteenSixteen());
        bool submergedAfter = (TourianStatueWaterSurface - projectile.Position.Whole).Y < 0;
        if (submergedBefore != submergedAfter)
            SpawnTourianParticleChild(projectile, RoomEnemyProjectileKind.TourianStatueSplash);
        if (RoomGridConversions.PixelToScreen(projectile.Position.Y.Whole) == 1)
        {
            projectile.StartAnimation(new TourianStatueProjectileAnimation(TourianStatueProjectileSegment.Delete));
        }
        else projectile.Velocity = projectile.Velocity.WithY(projectile.Velocity.Y + ParticleGravity);
    }

    /// <summary>Registers the statue effects' movers and program cues.</summary>
    internal void AddTourianUnlockProjectiles(EnemyProjectileHandlerTable table)
    {
        table.AddFrameBehavior(context => HoldTourianStatueSplash(context.Projectile), EnemyProjectileFrameBehavior.TourianStatueSplashMotion);
        table.AddFrameBehavior(context => DropTourianStatueParticle(context.Projectile), EnemyProjectileFrameBehavior.TourianStatueParticleMotion);
        table.AddFrameBehavior(context => RaiseTourianStatueSoul(context.Projectile), EnemyProjectileFrameBehavior.TourianStatueSoulMotion);
        table.AddCue<TourianStatueProjectileCue>((context, cue) =>
        {
            EnemyProjectile projectile = context.Projectile;
            switch (cue)
            {
                case TourianStatueProjectileCue.Command { Kind: TourianStatueProjectileCueKind.Delete }:
                    projectile.Delete();
                    break;
                case TourianStatueProjectileCue.Command { Kind: TourianStatueProjectileCueKind.PlayReleaseSound }:
                    _audio.QueueEnemySound(EnemySound.TourianStatueParticle, priority: SoundRequestPriority.Normal);
                    break;
                case TourianStatueProjectileCue.Command { Kind: TourianStatueProjectileCueKind.Earthquake }:
                    _earthquake.Sustain(TourianStatueSequenceDefinitions.ReleaseEarthquake);
                    break;
                case TourianStatueProjectileCue.Command { Kind: TourianStatueProjectileCueKind.SpawnParticle }:
                    SpawnTourianParticleChild(projectile, RoomEnemyProjectileKind.TourianStatueParticle);
                    break;
                case TourianStatueProjectileCue.Command { Kind: TourianStatueProjectileCueKind.SpawnTail }:
                    SpawnTourianParticleChild(projectile, RoomEnemyProjectileKind.TourianStatueTail);
                    break;
                case TourianStatueProjectileCue.MoveDown moveDown:
                    projectile.Position = projectile.Position.WithWrappedWholeY(projectile.Position.Y.Whole + moveDown.Pixels);
                    break;
                case TourianStatueProjectileCue.StartMoving startMoving:
                    projectile.FrameBehavior = startMoving.Movement;
                    break;
                default:
                    throw new InvalidOperationException($"The Tourian statue projectiles have no {cue} cue.");
            }
        });
    }
}
