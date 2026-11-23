using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Samus.Colours;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
namespace SuperMetroid.Verification.Samus.Abilities;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Crystal Flash body-palette durations and the full production color cycle.</summary>
internal static class CrystalFlashPaletteTimingDefinitionsVerification
{
    /// <summary>
    /// Compares ten native timer words with the body pulse's beats (#2387) and checks 100 palette updates without runtime
    /// timer reads.
    /// </summary>
    internal static void VerifyCrystalFlashPaletteTimingDefinitions(
        CartridgeImportAddressSpace rom)
    {
        IReadOnlyList<AnimationBeat<PictureShade, NoAnimationCue>> pulse =
            SamusGlowAnimationDefinitions.Segments.Beats(SamusGlowSegment.Of(SamusGlow.CrystalFlashBody));
        AssertEqual(CrystalFlashPaletteTimingDefinitionsConstants.RecordCount, pulse.Count, "the body pulse has one shade per native record");
        for (int record = 0; record < CrystalFlashPaletteTimingDefinitionsConstants.RecordCount; record++)
        {
            int address = CrystalFlashPaletteTimingDefinitionsConstants.NativeFirstTimerAddress +
                record * CrystalFlashPaletteTimingDefinitionsConstants.RecordByteCount;
            ushort nativeDuration = rom.ReadCartridgeWord(address);
            var beat = (AnimationBeat<PictureShade, NoAnimationCue>.Frame)pulse[record];
            AssertEqual(nativeDuration, beat.Duration, $"Crystal Flash body-palette duration {record}");
            AssertEqual(SamusShades.CrystalFlashBody[record], beat.Shown, $"Crystal Flash body pulse shade {record}");
        }

        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork())
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
            Health = 1,
            MaxHealth = 99,
            Missiles = 10,
            SuperMissiles = 10,
            PowerBombs = 10,
        }.InPose(SamusPoseId.FacingRightNormalPose);
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        AssertTrue(
            samus.CrystalFlash.TryBegin(samus, SamusCrystalFlashState.RequiredInput),
            "Crystal Flash timing production fixture begins");

        var guard = NewCrystalFlashPaletteTimingReadGuard(rom);
        var samusColours = new SamusColours();
        const int shadeUpdates = 10;
        int callCount = CrystalFlashPaletteTimingDefinitionsConstants.RecordCount * shadeUpdates;
        for (int call = 0; call < callCount; call++)
        {
            AssertTrue(samus.CrystalFlash.UpdateColouring(samusColours, samus),
                $"Crystal Flash timing call {call} retains palette ownership");

            AssertEqual(shadeUpdates - call % shadeUpdates, samus.CrystalFlash.BodyPulse.UpdatesLeft,
                $"production Crystal Flash body timer call {call}");
            AssertEqual(SamusShades.CrystalFlashBody[call / shadeUpdates], samus.CrystalFlash.BodyPulse.Shown,
                $"production Crystal Flash body shade call {call}");
        }

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production Crystal Flash performs no body-duration ROM reads");
        Console.WriteLine(
            "Crystal Flash palette timing: all ten native durations and the complete " +
            "100-call production cycle pass with timer-word reads forbidden.");
    }

    /// <summary>Blocks the timer word of each migrated body-palette record during updates.</summary>
    private static CartridgeReadGuard NewCrystalFlashPaletteTimingReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address =>
            {
                int relative = address -
                    CrystalFlashPaletteTimingDefinitionsConstants.NativeFirstTimerAddress;
                return relative >= 0 &&
                    relative < CrystalFlashPaletteTimingDefinitionsConstants.RecordCount *
                        CrystalFlashPaletteTimingDefinitionsConstants.RecordByteCount &&
                    relative % CrystalFlashPaletteTimingDefinitionsConstants.RecordByteCount < 2;
            },
            address => $"Crystal Flash palette handler attempted timer read ${address:X6}.");

    /// <summary>Runs the Crystal Flash palette timing verification command.</summary>
    [VerificationCommand("--crystal-flash-palette-timing-definitions")]
    internal static int CrystalFlashPaletteTimingDefinitionsCommand()
    {
        Suite(nameof(VerifyCrystalFlashPaletteTimingDefinitions), () => VerifyCrystalFlashPaletteTimingDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
