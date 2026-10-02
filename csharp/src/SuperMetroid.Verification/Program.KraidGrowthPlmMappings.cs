using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyKraidGrowthPlmColumns(SuperMetroidAddressSpace rom) =>
        VerifyKraidGrowthPlmField(rom, 4, request => request.BlockX);

    private static void VerifyKraidGrowthPlmRows(SuperMetroidAddressSpace rom) =>
        VerifyKraidGrowthPlmField(rom, 5, request => request.BlockY);

    private static void VerifyKraidGrowthPlmHeaders(SuperMetroidAddressSpace rom) =>
        VerifyKraidGrowthPlmField(rom, 6, request => request.Header);

    private static void VerifyKraidGrowthPlmField(SuperMetroidAddressSpace rom, int offset,
        Func<KraidPlmRequest, ushort> field)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var requests = KraidPlmDefinitions.GrowthCeiling;
        var enumerated = requests.ToArray();
        AssertEqual(9, requests.Count, "Nine native growth callbacks");
        AssertEqual(9, enumerated.Length, "Growth request enumeration count");
        for (int index = 0; index < 9; index++)
        {
            int callback = 0xa70000 | Word(0xa7acc5 + 2 * index);
            ushort expected = offset == 6 ? Word(callback + offset) : rom.ReadByte(callback + offset);
            AssertEqual(expected, field(requests[index]), "Native callback inline PLM field");
            AssertEqual(expected, field(enumerated[index]), "Ordered calculated PLM enumeration");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 9, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => _ = requests[invalid], "Growth request list bounds");
    }
}
