using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks that each defeated-room PLM request uses the horizontal block coordinate embedded in Kraid's native callbacks.</summary>
    private static void VerifyKraidDefeatedPlmColumns(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidDefeatedPlmField), () => VerifyKraidDefeatedPlmField(rom, 0, request => request.BlockX));

    /// <summary>Checks that each defeated-room PLM request uses the vertical block coordinate embedded in Kraid's native callbacks.</summary>
    private static void VerifyKraidDefeatedPlmRows(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidDefeatedPlmField), () => VerifyKraidDefeatedPlmField(rom, 1, request => request.BlockY));

    /// <summary>Checks that each defeated-room PLM request uses the header word embedded in Kraid's native callbacks.</summary>
    private static void VerifyKraidDefeatedPlmHeaders(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidDefeatedPlmField), () => VerifyKraidDefeatedPlmField(rom, 2, request => request.Header));

    /// <summary>Compares one request field against both inline native calls and verifies the defeated-room sequence's count, order, and bounds.</summary>
    /// <param name="rom">Address space containing Kraid's native callback arguments.</param>
    /// <param name="offset">Byte offset of the selected field within each argument block: zero for column, one for row, or two for the header word.</param>
    /// <param name="field">Selector for the corresponding value in a decoded PLM request.</param>
    private static void VerifyKraidDefeatedPlmField(SuperMetroidAddressSpace rom, int offset,
        Func<KraidPlmRequest, ushort> field)
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
                ? (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8)
                : rom.ReadByte(address);
            AssertEqual(expected, field(requests[index]), "Native defeated-room inline field");
            AssertEqual(expected, field(enumerated[index]), "Native defeated-room operation order");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 2, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => _ = requests[invalid], "Defeated-room list bounds");
    }
}
