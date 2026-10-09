using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks the nine growth callbacks' PLM columns against their native inline arguments and ordered catalog entries.</summary>
    /// <param name="rom">ROM address space used to read the native callback table and PLM arguments.</param>
    private static void VerifyKraidGrowthPlmColumns(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidGrowthPlmField), () => VerifyKraidGrowthPlmField(rom, 4, request => request.BlockX));

    /// <summary>Checks the nine growth callbacks' PLM rows against their native inline arguments and ordered catalog entries.</summary>
    /// <param name="rom">ROM address space used to read the native callback table and PLM arguments.</param>
    private static void VerifyKraidGrowthPlmRows(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidGrowthPlmField), () => VerifyKraidGrowthPlmField(rom, 5, request => request.BlockY));

    /// <summary>Checks the nine growth callbacks' PLM headers against their native inline arguments and ordered catalog entries.</summary>
    /// <param name="rom">ROM address space used to read the native callback table and PLM arguments.</param>
    private static void VerifyKraidGrowthPlmHeaders(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidGrowthPlmField), () => VerifyKraidGrowthPlmField(rom, 6, request => request.Header));

    /// <summary>Compares one PLM request field with native callback arguments in both indexed and enumerated catalog access.</summary>
    /// <param name="rom">ROM address space containing the callback table and inline PLM arguments.</param>
    /// <param name="offset">Byte offset of the requested column, row, or header within each native argument block.</param>
    /// <param name="field">Selector that reads the corresponding value from a catalog request.</param>
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
