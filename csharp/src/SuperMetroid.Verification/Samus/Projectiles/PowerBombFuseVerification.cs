using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Samus.Projectiles.PowerBombBoundaryVerification;
using static SuperMetroid.Verification.Samus.Projectiles.PowerBombRuntimeIntegrationVerification;
using static SuperMetroid.Verification.Rendering.RenderingAndRoomsVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus.Projectiles;
namespace SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Verification.Harness;

/// <summary>Checks native Power Bomb fuse timing, list transitions, and radius-speed retention.</summary>
internal static class PowerBombFuseVerification
{
    /// <summary>Checks flag, wrap, expiration, and complete sixty-frame fuse behavior.</summary>
    internal static void VerifyPowerBombFuse()
    {
        // Expected boundary cases follow the literal C157 branches, including a
        // positive nonzero flag (native tests zero, not merely the sign bit).
        // The fuse is a countdown: zero stays stopped, one expires into the collision
        // sentinel, and the sentinel itself keeps wrapping downward one word per call.
        NativeCountdown sentinel = NativeCountdown.FromRaw(SamusPowerBombFuseData.ExplosionStartedSentinel);
        NativeCountdown sentinelAfterOneCall = NativeCountdown.FromRaw(SamusPowerBombFuseData.ExplosionStartedSentinel - 1);
        foreach (ushort flag in new ushort[] { 0, 1, 0x8000, 0xffff })
        {
            AssertEqual(new PowerBombFuseStep(NativeCountdown.Start(0), false, false, flag == 0),
                SamusPowerBombFuse.Step(NativeCountdown.Start(0), flag), "zero fuse follows native flag equality");
            AssertEqual(new PowerBombFuseStep(sentinel, false, true, false),
                SamusPowerBombFuse.Step(NativeCountdown.Start(1), flag), "expiration spawns before collision consumes sentinel");
            AssertEqual(new PowerBombFuseStep(NativeCountdown.Start(15), true, false, false),
                SamusPowerBombFuse.Step(NativeCountdown.Start(16), flag), "the fuse reaching fifteen switches to the fast list");
            AssertEqual(new PowerBombFuseStep(NativeCountdown.Start(14), false, false, false),
                SamusPowerBombFuse.Step(NativeCountdown.Start(15), flag), "fast threshold is tested after decrement");
            AssertEqual(new PowerBombFuseStep(sentinelAfterOneCall, false, false, false),
                SamusPowerBombFuse.Step(sentinel, flag), "fuse routine does not consume collision sentinel");
        }
        NativeCountdown timer = NativeCountdown.Start(60);
        int spawns = 0;
        for (int frame = 0; frame < 60; frame++)
        {
            PowerBombFuseStep result = SamusPowerBombFuse.Step(timer, 0x8000);
            timer = result.Timer;
            spawns += result.SpawnExplosion ? 1 : 0;
            AssertEqual(false, result.DeleteProjectile, "live fuse never requests deletion");
            AssertEqual(frame == 44, result.FastAnimation, "native fast-list transition frame");
            AssertEqual(frame == 59, result.SpawnExplosion, "native fuse expiration frame");
        }
        AssertEqual(1, spawns, "one expiration across complete fuse");
        // The fast lists lie $1C bytes on: each slow Power Bomb position lands on its fast counterpart.
        foreach (var (slow, fast) in new[] { (SamusProjectileProgram.PowerBomb, SamusProjectileProgram.FastPowerBomb) })
            for (int position = 0; position < SamusProjectileAnimations.Programs.Steps(slow).Count; position++)
                AssertEqual(new AnimationProgramCursor<SamusProjectileProgram>(fast, position),
                    SamusPowerBombFuse.FastAnimation(new AnimationProgramCursor<SamusProjectileProgram>(slow, position)),
                    $"{slow} position {position} switches to the same fast position");
        Suite(nameof(VerifyPowerBombRetainedRadiusSpeed), () => VerifyPowerBombRetainedRadiusSpeed());
        Console.WriteLine("Power Bomb fuse: native boundary/flag/wrap cases and complete 60-frame fuse agree.");
    }

    /// <summary>Checks that cleanup and a later allocation preserve the native radius-speed word.</summary>
    private static void VerifyPowerBombRetainedRadiusSpeed()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var explosion = new SamusPowerBombExplosionState { PresentationColors = RetailPresentationFixture().PowerBombFixedColors };
        explosion.Arm();
        explosion.Spawn(new(128, 128));
        ushort retainedSpeed = 0;
        bool completed = false;
        for (int frame = 0; frame < 400; frame++)
        {
            retainedSpeed = explosion.RadiusSpeed;
            if (!explosion.StepFrame()) continue;
            completed = true;
            break;
        }
        AssertTrue(completed, "Power Bomb cleanup was reached");
        AssertTrue(retainedSpeed != 0, "native explosion accumulated a nonzero radius speed");
        // Original-CPU #441 replay: $88:8B4E clears both radii/status, but does not
        // write $0CF0. Allocation at $88:8AA4 also leaves that word alone.
        AssertEqual(retainedSpeed, explosion.RadiusSpeed, "cleanup preserves native radius-speed word");
        explosion.Spawn(new(128, 128));
        AssertEqual(retainedSpeed, explosion.RadiusSpeed, "next allocation preserves radius speed until setup");
    }

    /// <summary>Runs focused fuse and fixed-color fixture checks.</summary>
    [VerificationCommand("--power-bomb-fixtures")]
    internal static int PowerBombFixturesCommand()
    {
     VerifyPowerBombFuse(); VerifyPowerBombColorMathWindow(); return 0; 
    }

    /// <summary>Runs fuse, boundary and renderer integration checks.</summary>
    [VerificationCommand("--power-bomb-fuse")]
    internal static int PowerBombFuseCommand()
    {
        Suite(nameof(VerifyPowerBombFuse), () => VerifyPowerBombFuse());
        Suite(nameof(VerifyPowerBombBoundary), () => VerifyPowerBombBoundary());
        Suite(nameof(VerifyPowerBombRuntimeRendererIntegration), () => VerifyPowerBombRuntimeRendererIntegration());
        return 0;
    }
}
