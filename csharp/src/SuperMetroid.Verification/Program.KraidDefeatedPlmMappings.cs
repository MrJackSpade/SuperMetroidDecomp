using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyKraidDefeatedPlmColumns(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidDefeatedPlmField), () => VerifyKraidDefeatedPlmField(rom, 0, request => request.BlockX));

    private static void VerifyKraidDefeatedPlmRows(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidDefeatedPlmField), () => VerifyKraidDefeatedPlmField(rom, 1, request => request.BlockY));

    private static void VerifyKraidDefeatedPlmHeaders(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyKraidDefeatedPlmField), () => VerifyKraidDefeatedPlmField(rom, 2, request => (ushort)request.Header));

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
