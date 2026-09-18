using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyStationAccessPlmDefinitions(SuperMetroidAddressSpace rom)
    {
        ReadOnlySpan<StationAccessPlmDefinition> definitions = StationAccessPlmDefinitions.All;
        AssertEqual(6, definitions.Length, "station-access PLM definition count");

        foreach (StationAccessPlmDefinition definition in definitions)
        {
            AssertEqual(
                definition,
                StationAccessPlmDefinitions.Resolve(definition.Behavior),
                $"station-access BTS ${(byte)definition.Behavior:X2} resolves by identity");
            AssertEqual(
                ReadStationAccessWord(
                    rom,
                    0x840000 | unchecked((ushort)(definition.HeaderPointer + 2))),
                definition.InstructionListPointer,
                $"station-access PLM ${definition.HeaderPointer:X4} initial list matches cartridge");
        }

        AssertThrows<InvalidDataException>(
            () => StationAccessPlmDefinitions.Resolve(StationAccessBehavior.SaveFloor),
            "save-floor trigger cannot enter extending station-access domain");

        // This production fixture activates map and missile access blocks and draws
        // both phases. Its sparse bus intentionally omits all six header+2 words.
        VerifySequentialRoomPlmPopulationLoader();
        Console.WriteLine(
            "Station access PLMs: all six native header/list identities and real access animations pass without runtime header reads.");
    }

    private static ushort ReadStationAccessWord(SuperMetroidAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
}
