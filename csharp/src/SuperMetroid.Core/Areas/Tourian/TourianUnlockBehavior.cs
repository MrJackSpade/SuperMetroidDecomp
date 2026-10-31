using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>The Tourian unlock species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class TourianUnlockBehavior
{
    /// <summary>Live room randomness and installed statue eye artwork.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocator and initializer for statue, dust, particle, and splash projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Earthquake state written by the statue release instruction.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Audio queue receiving the statue eye release sound.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>CGRAM owner receiving installed statue eye colors.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>The session's installed area artwork.</summary>
    private readonly AreaArtworkBinding _areaArtwork;


    /// <summary>Binds the unlock effects to the current room's projectile, audio, FX, and artwork owners.</summary>
    internal TourianUnlockBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles, RoomEarthquake earthquake, EnemyAudioQueue audio, EnemyPresentation presentation, AreaArtworkBinding areaArtwork)
    {
        _areaArtwork = areaArtwork;
        _room = room;
        _projectiles = projectiles;
        _earthquake = earthquake;
        _audio = audio;
        _presentation = presentation;
    }

    /// <summary>How far above the water surface a splash sits.</summary>
    private static readonly Offset16<RoomPixels> SplashAboveSurface = new(0, -4);

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
                default);
            projectile.State = new AnchoredProjectileState(TourianDescentDustAnchor);
        }
    }

    /// <summary>$87:8320 spawns a statue eye's glow from the bank-$86 definition and lights the eye's colours.</summary>
    public void SpawnTourianEyeGlow(int parameter)
    {
        if (SpawnAtStatueEye(RoomEnemyProjectileKind.TourianStatueEyeGlow, parameter) is null) return;
        (_areaArtwork.Artwork?.TourianStatueColors ?? throw new InvalidOperationException(
            "Tourian statue eye requires installed colors."))
            .ApplyEye(_presentation.Colours, parameter);
    }

    /// <summary>$87:832F spawns a statue's released soul from the bank-$86 definition, rising from the eye.</summary>
    public void SpawnTourianSoul(int parameter)
    {
        if (SpawnAtStatueEye(RoomEnemyProjectileKind.TourianStatueSoul, parameter) is not { } soul) return;
        soul.Velocity = soul.Velocity.WithY(SignedEightEight.Wrapped(-1024));
    }

    /// <summary>Allocates an unlock effect at the statue's native eye position; null when every slot is busy.</summary>
    private RoomEnemyProjectileSlot? SpawnAtStatueEye(RoomEnemyProjectileKind kind, int parameter)
    {
        Position16<RoomPixels> position = TourianStatueUnlock.EyePosition(parameter);
        var projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null) return null;
        TourianStatueProjectileStarts.Initialize(projectile, kind, default);
        projectile.Position = projectile.Position.WithWhole(position);
        return projectile;
    }

    /// <summary>Spawns a splash, particle, or tail at its parent, applying water height or randomized motion.</summary>
    internal void SpawnTourianParticleChild(RoomEnemyProjectileSlot parent, RoomEnemyProjectileKind kind)
    {
        if (kind is not (RoomEnemyProjectileKind.TourianStatueSplash or RoomEnemyProjectileKind.TourianStatueParticle or
                RoomEnemyProjectileKind.TourianStatueTail))
            throw new InvalidOperationException($"{kind} is not a Tourian statue child projectile.");
        var projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null) return;
        TourianStatueProjectileStarts.Initialize(projectile, kind, default);
        projectile.Position = projectile.Position.WithWhole(parent.Position.Whole);
        if (kind == RoomEnemyProjectileKind.TourianStatueSplash)
            projectile.Position = projectile.Position.WithWholeY((TourianStatueWaterSurface + SplashAboveSurface).Y);
        if (kind != RoomEnemyProjectileKind.TourianStatueParticle) return;
        int angle = WordWrap.LowByte((_room.Random.Generate() & 63) - 32);
        projectile.Velocity = projectile.Velocity.WithX(SignedEightEight.Wrapped(Trigonometry.SignedNegativeCosineWord(angle + 64)));
        projectile.Velocity = projectile.Velocity.WithY(SignedEightEight.Wrapped(4 * Trigonometry.SignedNegativeCosineWord(angle)));
    }

    /// <summary>Holds a splash at the water surface.</summary>
    private void HoldTourianStatueSplash(RoomEnemyProjectileSlot projectile) =>
        projectile.Position = projectile.Position.WithWholeY((TourianStatueWaterSurface + SplashAboveSurface).Y);

    /// <summary>Raises a soul, accelerating upward, and deletes it once it leaves the room.</summary>
    internal static void RaiseTourianStatueSoul(RoomEnemyProjectileSlot projectile)
    {
        projectile.Position = projectile.Position.AdvanceY(projectile.Velocity.Y.ToSixteenSixteenDelta());
        // The soul is deleted while its Y lies in an odd 256-pixel screen row. Rising from the statues'
        // top screen row, that first happens when it wraps above the room's top edge.
        int soulScreenRow = RoomGridConversions.PixelToScreen(projectile.Position.Y.Whole);
        if (soulScreenRow % 2 != 0)
        {
            projectile.StartAnimation(new TourianStatueProjectileAnimation(TourianStatueProjectileProgram.Delete));
        }
        projectile.Velocity = projectile.Velocity.WithY(SignedEightEight.Wrapped(projectile.Velocity.Y.RawValue - 128));
    }

    /// <summary>Drops a particle under gravity, splashing as it crosses the water surface and deleting it below the room.</summary>
    private void DropTourianStatueParticle(RoomEnemyProjectileSlot projectile)
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
            projectile.StartAnimation(new TourianStatueProjectileAnimation(TourianStatueProjectileProgram.Delete));
        }
        else projectile.Velocity = projectile.Velocity.WithY(SignedEightEight.Wrapped(projectile.Velocity.Y.RawValue + (16)));
    }

    /// <summary>Registers the statue effects' movers and program cues.</summary>
    internal void AddTourianUnlockProjectiles(EnemyProjectileHandlerTable table)
    {
        table.AddFrameBehavior(context => HoldTourianStatueSplash(context.Projectile), EnemyProjectileFrameBehavior.TourianStatueSplashMotion);
        table.AddFrameBehavior(context => DropTourianStatueParticle(context.Projectile), EnemyProjectileFrameBehavior.TourianStatueParticleMotion);
        table.AddFrameBehavior(context => RaiseTourianStatueSoul(context.Projectile), EnemyProjectileFrameBehavior.TourianStatueSoulMotion);
        table.AddCue<TourianStatueProjectileCue>((context, cue) =>
        {
            RoomEnemyProjectileSlot projectile = context.Projectile;
            switch (cue)
            {
                case TourianStatueProjectileCue.Command { Kind: TourianStatueProjectileCueKind.Delete }:
                    projectile.Clear();
                    break;
                case TourianStatueProjectileCue.Command { Kind: TourianStatueProjectileCueKind.PlayReleaseSound }:
                    _audio.QueueEnemySound(Library2Sound.TourianStatueParticle, priority: SoundRequestPriority.Normal);
                    break;
                case TourianStatueProjectileCue.Command { Kind: TourianStatueProjectileCueKind.Earthquake }:
                    _earthquake.EarthquakeType = TourianStatueSequenceDefinitions.ReleaseEarthquakeType;
                    _earthquake.AddTimerBits(TourianStatueSequenceDefinitions.ReleaseEarthquakeTimer);
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
