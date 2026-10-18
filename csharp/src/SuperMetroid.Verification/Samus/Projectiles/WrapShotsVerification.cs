using static SuperMetroid.Verification.Samus.Projectiles.BeamCallbackTablesVerification;
using static SuperMetroid.Verification.Samus.Projectiles.BeamSpeedRowsVerification;
using static SuperMetroid.Verification.Samus.Projectiles.BombChargeRejectionVerification;
using static SuperMetroid.Verification.Samus.Movement.CeilingWrapRoomVerification;
using static SuperMetroid.Verification.Rooms.Plms.GateBeamCollisionVerification;
using static SuperMetroid.Verification.Rooms.Plms.GModeGateGlitchVerification;
using static SuperMetroid.Verification.Samus.Movement.GroundedBombSpreadVerification;
using static SuperMetroid.Verification.Runtime.HeroShotsVerification;
using static SuperMetroid.Verification.Samus.Projectiles.MissileEdgeVerification;
using static SuperMetroid.Verification.Rooms.Plms.RightFacingGateGlitchVerification;
using static SuperMetroid.Verification.Samus.Abilities.SamusMorphBallVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Samus.Projectiles.WrapShotEnemiesVerification;
using static SuperMetroid.Verification.Samus.Projectiles.WrapShotWidthsVerification;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
namespace SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Verification.Harness;

/// <summary>Checks projectile wrapping at both room edges against a preserved native trace.</summary>
internal static class WrapShotsVerification
{
    /// <summary>Compares 240 shot frames, including remote block writes and non-Wave controls.</summary>
    internal static void VerifyWrapShotTrace(string nativeTrace)
    {
        var native = File.ReadLines(nativeTrace).Skip(1).ToArray();
        AssertEqual(240, native.Length, "Complete wrap-shot native matrix");
        int mismatches = 0;
        for (int left = 0; left < 2; left++)
        for (int beam = 0; beam < 3; beam++)
        {
            var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            var words = new ushort[64 * 128];
            for (int row = 0; row < 128; row++)
            {
                words[row * 64 + (left != 0 ? 63 : 0)] = 0x4000;
                words[row * 64 + (left != 0 ? 0 : 63)] = 0x8000;
            }
            var level = CreateRoom(new(64, 128), words, new byte[words.Length]);
            var samus = new SamusState
            {
                PoseId = left != 0 ? SamusPoseId.StandingAimDiagonalDownLeftPose : SamusPoseId.StandingAimDiagonalDownRightPose,
                Position = FixedPosition<RoomPixels>.FromWhole(new((ushort)(left != 0 ? 32 : 992), 128)),
                EquippedBeams = (SamusBeamFlags)((ushort)(beam == 0 ? 0 : beam == 1 ? 1 : 5)),
            };
            var projectiles = CreateProjectileFixture();
            var shared = CreateBombFixture();
            var plms = new RoomPlmSystem();
            int firstRemoteHit = -1;
            for (int frame = 0; frame < 40; frame++)
            {
                SnesButton input = frame == 0 ? SnesButton.X : (ushort)0;
                shared.StepFrame(new SamusTerrain { Level = level }, samus, input, input);
                projectiles.StepFrame(new SamusTerrain { Level = level, Plms = plms }, samus, new ProjectileButtonInput { ControllerInput = input, ControllerNewInput = input }, new((ushort)(left != 0 ? 0 : 768), 0), shared);
                var shot = projectiles.Slots[0];
                string actual = $"{left},{beam},{frame},{shot.Position.X.Whole:X4},{shot.Position.X.Fraction:X4},{shot.Position.Y.Whole:X4},{shot.Position.Y.Fraction:X4},{shot.Type:X4},{shot.InstructionPointer:X4},";
                for (int row = 0; row < 128; row++)
                {
                    var block = level.GetCollisionBlock(new(checked((ushort)(left != 0 ? 63 : 0)), checked((ushort)row)));
                    if (block.PackedWord.Raw != 0x4000) actual += $"{block.Index:X4}:{block.PackedWord.Raw:X4};";
                }
                int target = left != 0 ? 0x123f : 0x0280;
                if (firstRemoteHit < 0 && level.GetCollisionBlock(new(checked((ushort)(target % 64)), checked((ushort)(target / 64)))).PackedWord.Raw == 0x0052)
                {
                    firstRemoteHit = frame;
                    AssertTrue(shot.IsActive, "Remote reaction occurs while the real projectile is retained");
                }
                string expected = native[(left * 3 + beam) * 40 + frame];
                if (actual != expected && mismatches++ < 8) Console.WriteLine($"Wrap mismatch:\n{actual}\n{expected}");
            }
            AssertEqual(beam == 0 ? -1 : left != 0 ? 3 : 4, firstRemoteHit, "Exact native remote-tile reaction and non-Wave negative");
        }
        AssertEqual(0, mismatches, "Original cartridge wrap-shot positions, lifetime and remote block writes");
        Console.WriteLine("PASS wrap-shot addressing: 240 original-cartridge frames, both edges and non-Wave negatives.");
    }

    /// <summary>Runs the grouped projectile collision, speed, input, and camera fixtures.</summary>
    [VerificationCommand("--projectile-fixtures")]
    internal static int ProjectileFixturesCommand()
    {
    Suite(nameof(VerifyWrapShotTrace), () => VerifyWrapShotTrace("csharp/test-fixtures/movement-release/wrap-shot-409.csv"));
    Suite(nameof(VerifyWrapShotEnemySeparation), () => VerifyWrapShotEnemySeparation());
    Suite(nameof(VerifyWrapShotWidths), () => VerifyWrapShotWidths("csharp/test-fixtures/movement-release/wrap-width-409.csv"));
    Suite(nameof(VerifyKronicGateBeamCollision), () => VerifyKronicGateBeamCollision());
    Suite(nameof(VerifyRightFacingGateGlitches), () => VerifyRightFacingGateGlitches());
    Suite(nameof(VerifyGModeGateGlitch), () => VerifyGModeGateGlitch());
    Suite(nameof(VerifyFrogSpeedwayPoolCollision), () => VerifyFrogSpeedwayPoolCollision());
    Suite(nameof(VerifyHeroShotCameraLifetime), () => VerifyHeroShotCameraLifetime("csharp/test-fixtures/movement-release/hero-shot-411.csv"));
    Suite(nameof(VerifyMissileImpactCameraEdge), () => VerifyMissileImpactCameraEdge("csharp/test-fixtures/movement-release/missile-edge-602.csv"));
    Suite(nameof(VerifyBeamSpeedRows), () => VerifyBeamSpeedRows());
    Suite(nameof(VerifyBeamCallbackTables), () => VerifyBeamCallbackTables());
    Suite(nameof(VerifySamusMorphBallMovement), () => VerifySamusMorphBallMovement());
    Suite(nameof(VerifyBombChargeRejection), () => VerifyBombChargeRejection());
    Suite(nameof(VerifyGroundedBombSpread), () => VerifyGroundedBombSpread());
    return 0; 
    }

    /// <summary>Runs the focused two-edge wrap-shot trace comparison.</summary>
    [VerificationCommand("--wrap-shots")]
    internal static int WrapShotsCommand()
    {
        Suite(nameof(VerifyWrapShotTrace), () => VerifyWrapShotTrace("csharp/test-fixtures/movement-release/wrap-shot-409.csv"));
        return 0;
    }
}
