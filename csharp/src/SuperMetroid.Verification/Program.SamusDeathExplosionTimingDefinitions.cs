using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling;
using static SamusSpecialStatesVerification;
using static SuiteTimingVerification;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static class SamusDeathExplosionTimingDefinitionsVerification
{
    [CpuBusBoundary("exact bus test: retail ROM oracle for Samus death explosion timing definitions")]
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

    [VerificationCommand("--samus-death-explosion-timing-definitions")]
    internal static int SamusDeathExplosionTimingDefinitionsCommand()
    {
        Suite(nameof(VerifySamusDeathExplosionTimingDefinitions), () => VerifySamusDeathExplosionTimingDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        Suite(nameof(VerifySamusDeathSequence), () => VerifySamusDeathSequence());
        return 0;
    }
}
