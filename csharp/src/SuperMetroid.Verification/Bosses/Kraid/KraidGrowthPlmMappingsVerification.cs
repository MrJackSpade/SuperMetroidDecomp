using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>Checks Kraid's nine growth callback PLM placements against the native bank-$A7 callback table.</summary>
internal static class KraidGrowthPlmMappingsVerification
{
    /// <summary>Compares the compiled growth PLM column coordinates with callback inline bytes.</summary>
    internal static void VerifyKraidGrowthPlmColumns(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidGrowthPlmField), () => VerifyKraidGrowthPlmField(rom, 4, request => request.Block.X));

    /// <summary>Compares the compiled growth PLM row coordinates with callback inline bytes.</summary>
    internal static void VerifyKraidGrowthPlmRows(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidGrowthPlmField), () => VerifyKraidGrowthPlmField(rom, 5, request => request.Block.Y));

    /// <summary>Compares the compiled growth PLM headers with callback inline words.</summary>
    internal static void VerifyKraidGrowthPlmHeaders(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidGrowthPlmField), () => VerifyKraidGrowthPlmField(rom, 6, request => PlmNativeWords.Word(request.Header)));

    /// <summary>Reads each callback field at the requested native offset and checks indexed and enumerated PLM requests.</summary>
    private static void VerifyKraidGrowthPlmField(SuperMetroidAddressSpace rom, int offset,
        Func<PlmSpawnRequest, ushort> field)
    {
        ushort Word(int address) => LittleEndianBytes.Word(KraidGrowthPlmFieldRomByte(rom, address), KraidGrowthPlmFieldRomByte(rom, address + 1));
        var requests = KraidPlmDefinitions.GrowthCeiling;
        var enumerated = requests.ToArray();
        AssertEqual(9, requests.Count, "Nine native growth callbacks");
        AssertEqual(9, enumerated.Length, "Growth request enumeration count");
        for (int index = 0; index < 9; index++)
        {
            int callback = 0xa70000 | Word(0xa7acc5 + 2 * index);
            ushort expected = offset == 6 ? Word(callback + offset) : KraidGrowthPlmFieldRomByte(rom, callback + offset);
            AssertEqual(expected, field(requests[index]), "Native callback inline PLM field");
            AssertEqual(expected, field(enumerated[index]), "Ordered calculated PLM enumeration");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 9, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => _ = requests[invalid], "Growth request list bounds");
    }

    /// <summary>Reads a native Kraid growth callback byte for the focused cartridge comparison.</summary>
    private static byte KraidGrowthPlmFieldRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
