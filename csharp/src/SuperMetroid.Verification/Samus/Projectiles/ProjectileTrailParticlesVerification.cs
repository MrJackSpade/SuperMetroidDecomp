using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>Checks that projectile trails are unbounded presentation particles in spawn order (#1625).</summary>
internal static class ProjectileTrailParticlesVerification
{
    /// <summary>How many trails the check leaves at once: more than any fixed pool the trails once had.</summary>
    private const int TrailsLeft = 30;

    /// <summary>
    /// Leaves many more trails at once than a fixed pool would hold: every one is kept, none displaces another, they draw
    /// the newest first, and each leaves the list once both its sparkles' programs have ended.
    /// </summary>
    internal static void VerifyProjectileTrailParticles()
    {
        var system = new SamusProjectileSystem(GameplayAudioRequests.Standalone(), new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftBeamStartFromLastShot(SamusProjectileSystem.PlaceCount), new SamusShotCount(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork(), new global::SuperMetroid.Core.Fx.RoomEarthquake());
        // A charged power shot, part way through its animation, showing its first frame, in the first direction that names
        // a placement list: the charged list places a trail from the charged shot's frames, not the uncharged one's.
        SamusProjectile shot = Enum.GetValues<SamusProjectileDirection>()
            .Select(direction =>
            {
                var charged = new SamusProjectile(0);
                charged.OccupyAsFired(SamusProjectileType.Beam(SamusBeamCombination.Power, charged: true));
                charged.HeldShot().Direction = direction;
                charged.HeldShot().Animation = SamusProjectileProgramAddresses.ShowingBeatBefore(SamusProjectilePlayback.PlaybackAt(new SamusProjectileSegmentBeat(SamusProjectileSegment.ChargedPowerUp, 1)));
                charged.HeldShot().Position = new(new(100, 0), new(200, 0));
                return charged;
            })
            .First(candidate =>
            {
                system.Reset();
                system.Trails.SpawnTrail(candidate.HeldShot());
                return system.LiveTrails.Count == 1;
            });
        system.Reset();

        for (int spawn = 0; spawn < TrailsLeft; spawn++)
            system.Trails.SpawnTrail(shot.HeldShot());
        AssertEqual(TrailsLeft, system.LiveTrails.Count, $"all {TrailsLeft} trails left at once are kept");
        AssertEqual(TrailsLeft, system.LiveTrails.Distinct().Count(), "every trail is its own particle; none displaces another");
        SamusProjectileTrail oldest = system.LiveTrails[0];

        system.AdvanceTrails(timeIsFrozen: false);
        AssertEqual(TrailsLeft, system.LiveTrails.Count, "a trail whose left sparkle still runs stays");
        var sprites = new SpriteFrameBuffer();
        sprites.BeginFrame();
        system.DrawTrails(sprites, Position16<RoomPixels>.Zero, InstalledFixtures.ProjectileArtwork());
        sprites.FinalizeFrame();
        SpriteDraw[] drawn = [.. sprites.Draws.Where(draw => draw.Source.Kind == SpriteSourceKind.SamusProjectileTrail)];
        AssertTrue(drawn.Length >= TrailsLeft, "every trail's running sparkle is drawn");
        AssertEqual(system.LiveTrails[^1].PlaceOnScreen(system.LiveTrails[^1].Left, Position16<RoomPixels>.Zero), drawn[0].Origin,
            "the newest trail's left sparkle draws first, in front, at its place");

        int advances = 1;
        while (system.LiveTrails.Count > 0)
        {
            AssertTrue(advances < 1024, "every trail's programs end");
            AssertTrue(ReferenceEquals(oldest, system.LiveTrails[0]), "a running trail keeps its place in spawn order");
            system.AdvanceTrails(timeIsFrozen: false);
            advances++;
        }
        AssertTrue(oldest.Left.FrameHold == 0 && oldest.Right.FrameHold == 0, "a trail leaves once both sparkles' programs end");
        sprites = new SpriteFrameBuffer();
        sprites.BeginFrame();
        system.DrawTrails(sprites, Position16<RoomPixels>.Zero, InstalledFixtures.ProjectileArtwork());
        sprites.FinalizeFrame();
        AssertEqual(0, sprites.Draws.Length, "ended trails draw nothing");
        Console.WriteLine($"Projectile trails: {TrailsLeft} simultaneous trails kept in spawn order, drawn newest first, and removed after {advances} updates.");
    }
}
