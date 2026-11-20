using static SuperMetroid.Verification.Samus.Projectiles.ProjectileTrailCoordinatesVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Samus.Projectiles;

using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>Checks compiled projectile trail selector words and actual left/right spawn selections.</summary>
internal static class ProjectileTrailDefinitionsVerification
{
    /// <summary>
    /// Compares every trailing shot's spawn choices with the selector words the trail spawner reads for it, and requires
    /// the Shinespark crash's echo, which spawns no trail, to be rejected (#1500).
    /// </summary>
    internal static void VerifyProjectileTrailDefinitions(ISnesAddressSpace bus)
    {
        Suite(nameof(VerifyProjectileTrailCoordinates), () => VerifyProjectileTrailCoordinates(bus));
        int start = SamusProjectileRomDataTrailsConstants.LeftInstructionPointers;
        int spawned = 0;
        for (int selection = 0; selection < 64; selection++)
        {
            // Of the echo selections only the Spazer special attack's trailing pair ($24) spawns trails; the Shinespark
            // crash's echo ($29) runs its own update, which spawns none, and the typed identity names no other echo (#1470).
            if ((selection & 0x20) != 0 && selection != 0x24)
                continue;
            spawned++;
            var projectiles = new SamusProjectileSystem(GameplayAudioRequests.Standalone(), new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftBeamStartFromLastShot(SamusProjectileSystem.PlaceCount), new SamusShotCount(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork(), new global::SuperMetroid.Core.Fx.RoomEarthquake());
            var projectile = new SamusProjectile(0)
            {
                Type = SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeType(selection),
                Animation = SamusProjectileProgramAddresses.StandingAt(0x86e3),
                Position = new(new(ShotColumn, 0), new(200, 0)),
            };
            projectiles.Trails.SpawnTrail(projectile);
            // An empty trail family (programs, direction list or coordinates outside the compiled data) draws no trail (#1318).
            if (projectiles.LiveTrails.SingleOrDefault() is not { } trail)
                continue;
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), start + selection * 2), trail.Left.InstructionPointer, "Real spawn selects the left table's trail");
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), SamusProjectileRomDataTrailsConstants.RightInstructionPointers + selection * 2), trail.Right.InstructionPointer, "Real spawn selects the right table's trail");
            AssertEqual(1, trail.Left.FrameHold, "Selector extraction leaves allocation timer unchanged");
            AssertSelectedTrailStandsOnShot(projectiles, trail, ShotColumn);
        }
        var echoProjectiles = new SamusProjectileSystem(GameplayAudioRequests.Standalone(), new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftBeamStartFromLastShot(SamusProjectileSystem.PlaceCount), new SamusShotCount(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork(), new global::SuperMetroid.Core.Fx.RoomEarthquake());
        var echo = new SamusProjectile(0)
        {
            Type = SamusProjectileType.EchoOf(SamusBeamEcho.ShinesparkEcho),
            Animation = SamusProjectileProgramAddresses.StandingAt(0x86e3),
            Position = new(new(100, 0), new(200, 0)),
        };
        AssertThrows<ArgumentOutOfRangeException>(() => echoProjectiles.Trails.SpawnTrail(echo), "A Shinespark echo's selection past the trail tables is rejected");
        Console.WriteLine($"Trail selectors: {spawned} real spawn selections and the rejected echo pass.");
    }

    /// <summary>The room column the selector checks' shot stands at.</summary>
    internal const int ShotColumn = 100;

    /// <summary>
    /// Checks that a trail just spawned from a shot at <paramref name="shotColumn"/>, on a frame whose placement is the
    /// shot's own column, stands its left sparkle on that column (#2573), and that once the sparkle shows a pose its picture
    /// is drawn where native draws it: half the picture up and left of the sparkle, the corner native keeps. A left program
    /// that shows no pose draws nothing to compare.
    /// </summary>
    internal static void AssertSelectedTrailStandsOnShot(SamusProjectileSystem projectiles, SamusProjectileTrail trail, int shotColumn)
    {
        AssertEqual(shotColumn, trail.Origin.X + trail.Left.Offset.X, "Selector extraction leaves origin offset unchanged");
        OamBuffer drawn = SpriteFrameBuffer.DrawnOam(frame =>
        {
            projectiles.AdvanceTrails(false);
            projectiles.DrawTrails(frame, Position16<RoomPixels>.Zero, InstalledFixtures.ProjectileArtwork());
        });
        if (!trail.Left.HasEnded && trail.Left.ShownPose is not null)
            AssertEqual(new Position16<ScreenPixels>(shotColumn - NativeTrailSide.PictureHalf,
                    trail.Origin.Y + trail.Left.DisplayedOffset.Y - NativeTrailSide.PictureHalf),
                drawn.GetEntry(0).Position, "Selector extraction's left picture is drawn where native draws it");
    }
}
