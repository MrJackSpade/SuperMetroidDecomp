using static SuperMetroid.Verification.Samus.Abilities.SamusSpecialStatesVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Death;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Samus.Death;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the compiled nine-frame Samus death explosion cadence against native timer bytes.</summary>
internal static class SamusDeathExplosionTimingDefinitionsVerification
{
    /// <summary>Compares the explosion's nine pictures and their dwell times with the native records (#2388).</summary>
    internal static void VerifySamusDeathExplosionTimingDefinitions(
        CartridgeImportAddressSpace rom)
    {
        IReadOnlyList<AnimationBeat<SamusDeathExplosionFrame, NoAnimationCue>> beats =
            SamusDeathAnimations.ExplosionSegments.Beats(SamusDeathExplosionSegment.Explosion);
        AssertEqual(SuperMetroid.Core.Samus.Death.SamusDeathExplosionTimingDefinitionsConstants.RecordCount, beats.Count,
            "the explosion shows one picture per native record");
        for (ushort index = 0;
             index < SuperMetroid.Core.Samus.Death.SamusDeathExplosionTimingDefinitionsConstants.RecordCount;
             index++)
        {
            int address = SuperMetroid.Core.Samus.Death.SamusDeathExplosionTimingDefinitionsConstants.NativeFirstTimerAddress +
                index * SuperMetroid.Core.Samus.Death.SamusDeathExplosionTimingDefinitionsConstants.RecordByteCount;
            var beat = (AnimationBeat<SamusDeathExplosionFrame, NoAnimationCue>.Frame)beats[index];
            AssertEqual(rom.ReadCartridgeByte(address), beat.Duration, $"Samus death-explosion duration {index}");
            AssertEqual(SamusDeathSheetDefinitions.Explosion[index], beat.Shown, $"Samus death-explosion picture {index}");
        }

        Console.WriteLine(
            "Samus death-explosion timing: all nine native durations match the explosion's pictures in order.");
    }

    /// <summary>Forbids runtime reads of the nine migrated timer-byte positions while leaving unrelated sequence data accessible.</summary>
    internal static CartridgeReadGuard NewSamusDeathExplosionTimingReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address =>
            {
                int relative = address -
                    SuperMetroid.Core.Samus.Death.SamusDeathExplosionTimingDefinitionsConstants.NativeFirstTimerAddress;
                return relative >= 0 &&
                    relative < SuperMetroid.Core.Samus.Death.SamusDeathExplosionTimingDefinitionsConstants.RecordCount *
                        SuperMetroid.Core.Samus.Death.SamusDeathExplosionTimingDefinitionsConstants.RecordByteCount &&
                    relative % SuperMetroid.Core.Samus.Death.SamusDeathExplosionTimingDefinitionsConstants.RecordByteCount == 0;
            },
            address => $"Samus death sequence attempted timer read ${address:X6}.");

    /// <summary>Runs both the native duration comparison and production Samus death-sequence check.</summary>
    [VerificationCommand("--samus-death-explosion-timing-definitions")]
    internal static int SamusDeathExplosionTimingDefinitionsCommand()
    {
        Suite(nameof(VerifySamusDeathExplosionTimingDefinitions), () => VerifySamusDeathExplosionTimingDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        Suite(nameof(VerifySamusDeathSequence), () => VerifySamusDeathSequence());
        return 0;
    }
}
