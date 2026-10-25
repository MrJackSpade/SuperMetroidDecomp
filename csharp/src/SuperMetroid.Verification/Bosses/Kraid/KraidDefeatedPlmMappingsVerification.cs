using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>Checks both defeated-room PLM requests against Kraid's inline native arguments.</summary>
internal static class KraidDefeatedPlmMappingsVerification
{
    /// <summary>Compares the native block-column bytes for both post-fight PLM operations.</summary>
    internal static void VerifyKraidDefeatedPlmColumns(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidDefeatedPlmField), () => VerifyKraidDefeatedPlmField(rom, 0, request => request.Block.X));

    /// <summary>Compares the native block-row bytes for both post-fight PLM operations.</summary>
    internal static void VerifyKraidDefeatedPlmRows(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidDefeatedPlmField), () => VerifyKraidDefeatedPlmField(rom, 1, request => request.Block.Y));

    /// <summary>Compares the native two-byte PLM header identities for both post-fight operations.</summary>
    internal static void VerifyKraidDefeatedPlmHeaders(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidDefeatedPlmField), () => VerifyKraidDefeatedPlmField(rom, 2, request => PlmNativeWords.Word(request.Header)));

    /// <summary>Checks the selected field in native request order and rejects indexes outside the two operations.</summary>
    private static void VerifyKraidDefeatedPlmField(SuperMetroidAddressSpace rom, int offset,
        Func<PlmSpawnRequest, ushort> field)
    {
        int[] arguments = [0xa7c16c, 0xa7c175];
        var requests = KraidPlmDefinitions.DefeatedRoom;
        var enumerated = requests.ToArray();
        AssertEqual(2, requests.Count, "Two defeated-room operations");
        AssertEqual(2, enumerated.Length, "Defeated-room enumeration count");
        for (int index = 0; index < arguments.Length; index++)
        {
            int address = arguments[index] + offset;
            ushort expected = offset == 2
                ? SnesLittleEndian.Word(KraidDefeatedPlmFieldRomByte(rom, address), KraidDefeatedPlmFieldRomByte(rom, address + 1))
                : KraidDefeatedPlmFieldRomByte(rom, address);
            AssertEqual(expected, field(requests[index]), "Native defeated-room inline field");
            AssertEqual(expected, field(enumerated[index]), "Native defeated-room operation order");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 2, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => _ = requests[invalid], "Defeated-room list bounds");
    }

    /// <summary>Reads an inline bank-$A7 argument byte from the cartridge oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Kraid defeated PLM field")]
    private static byte KraidDefeatedPlmFieldRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
