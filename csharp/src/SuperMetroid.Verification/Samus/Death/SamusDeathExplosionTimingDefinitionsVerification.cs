using static SuperMetroid.Verification.Samus.Abilities.SamusSpecialStatesVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus.Death;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Samus.Death;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the compiled nine-frame Samus death explosion cadence against native timer bytes.</summary>
internal static class SamusDeathExplosionTimingDefinitionsVerification
{
    /// <summary>Compares all nine authored durations and rejects a restored index beyond the final record.</summary>
    internal static void VerifySamusDeathExplosionTimingDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (ushort index = 0;
             index < SamusDeathExplosionTimingDefinitions.RecordCount;
             index++)
        {
            int address = SamusDeathExplosionTimingDefinitions.NativeFirstTimerAddress +
                index * SamusDeathExplosionTimingDefinitions.RecordByteCount;
            AssertEqual(
                rom.ReadCartridgeByte(address),
                SamusDeathExplosionTimingDefinitions.DurationForIndex(index),
                $"Samus death-explosion duration {index}");
        }

        AssertThrows<InvalidDataException>(
            () => SamusDeathExplosionTimingDefinitions.DurationForIndex(
                SamusDeathExplosionTimingDefinitions.RecordCount),
            "Samus death-explosion timing rejects a post-sequence index");

        Console.WriteLine(
            "Samus death-explosion timing: all nine native durations match the " +
            "compiled sequence and invalid restored indexes fail loudly.");
    }

    /// <summary>Forbids runtime reads of the nine migrated timer-byte positions while leaving unrelated sequence data accessible.</summary>
    internal static CartridgeReadGuard NewSamusDeathExplosionTimingReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address =>
            {
                int relative = address -
                    SamusDeathExplosionTimingDefinitions.NativeFirstTimerAddress;
                return relative >= 0 &&
                    relative < SamusDeathExplosionTimingDefinitions.RecordCount *
                        SamusDeathExplosionTimingDefinitions.RecordByteCount &&
                    relative % SamusDeathExplosionTimingDefinitions.RecordByteCount == 0;
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
