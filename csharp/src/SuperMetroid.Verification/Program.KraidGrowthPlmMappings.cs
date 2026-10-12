using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static SuiteTimingVerification;

internal static class KraidGrowthPlmMappingsVerification
{
    internal static void VerifyKraidGrowthPlmColumns(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidGrowthPlmField), () => VerifyKraidGrowthPlmField(rom, 4, request => request.Block.X));

    internal static void VerifyKraidGrowthPlmRows(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidGrowthPlmField), () => VerifyKraidGrowthPlmField(rom, 5, request => request.Block.Y));

    internal static void VerifyKraidGrowthPlmHeaders(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidGrowthPlmField), () => VerifyKraidGrowthPlmField(rom, 6, request => (ushort)request.Header));

    private static void VerifyKraidGrowthPlmField(SuperMetroidAddressSpace rom, int offset,
        Func<KraidPlmRequest, ushort> field)
    {
        ushort Word(int address) => (ushort)(SnesLittleEndian.Word(KraidGrowthPlmFieldRomByte(rom, address), KraidGrowthPlmFieldRomByte(rom, address + 1)));
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

    [CpuBusBoundary("exact bus test: retail ROM oracle for Kraid growth PLM field")]
    private static byte KraidGrowthPlmFieldRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
