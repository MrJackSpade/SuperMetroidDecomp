using static SuperMetroid.Verification.Samus.Projectiles.ProjectileTrailCoordinatesVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
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
            var projectiles = new SamusProjectileSystem(new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftMuzzleSubpixelCarry(SamusProjectileSystem.SlotCount));
            var projectile = new SamusProjectile(0)
            {
                Type = SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeType(selection),
                Playback = SamusProjectileProgramAddresses.PlaybackAt(0x86e3),
                Position = new(new(100, 0), new(200, 0)),
            };
            projectiles.Trails.SpawnTrail(projectile);
            // An empty trail family (programs, direction list or coordinates outside the compiled data) draws no trail (#1318).
            if (projectiles.LiveTrails.SingleOrDefault() is not { } trail)
                continue;
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), start + selection * 2), trail.Left.InstructionPointer, "Real spawn selects the left table's trail");
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), SamusProjectileRomDataTrailsConstants.RightInstructionPointers + selection * 2), trail.Right.InstructionPointer, "Real spawn selects the right table's trail");
            AssertEqual(1, trail.Left.FrameHold, "Selector extraction leaves allocation timer unchanged");
            AssertEqual(96, trail.Left.Position.X, "Selector extraction leaves origin offset unchanged");
        }
        var echoProjectiles = new SamusProjectileSystem(new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftMuzzleSubpixelCarry(SamusProjectileSystem.SlotCount));
        var echo = new SamusProjectile(0)
        {
            Type = SamusProjectileType.EchoOf(SamusBeamEcho.ShinesparkEcho),
            Playback = SamusProjectileProgramAddresses.PlaybackAt(0x86e3),
            Position = new(new(100, 0), new(200, 0)),
        };
        AssertThrows<ArgumentOutOfRangeException>(() => echoProjectiles.Trails.SpawnTrail(echo), "A Shinespark echo's selection past the trail tables is rejected");
        Console.WriteLine($"Trail selectors: {spawned} real spawn selections and the rejected echo pass.");
    }
}
