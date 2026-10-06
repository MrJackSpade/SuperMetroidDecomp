using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyCrocomireProgramControls(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyCrocomireProgramField), () => VerifyCrocomireProgramField(rom, false));

    private static void VerifyCrocomireProgramDraws(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyCrocomireProgramField), () => VerifyCrocomireProgramField(rom, true));

    private static void VerifyCrocomireProgramField(ISnesAddressSpace rom, bool draw)
    {
        ushort[] controls = [0xafca, 0xafce, 0xafd0, 0xafd4, 0xafd6, 0xafda, 0xafdc, 0xafe0, 0xafe2, 0xafe6];
        ushort[] draws = [0xafcc, 0xafd2, 0xafd8, 0xafde, 0xafe4];
        AssertTrue(controls.Concat(draws).Order().SequenceEqual(CrocomireArenaPlmProgramDefinitions.NativeWordAddresses()),
            "Crocomire native program enumeration");
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort address = (ushort)raw;
            bool owned = controls.Contains(address) || draws.Contains(address);
            AssertEqual(owned, CrocomireArenaPlmProgramDefinitions.TryReadMechanicsWord(address, out ushort actual),
                "Crocomire complete word ownership");
            if (!owned) AssertEqual((ushort)0, actual, "Crocomire missing word is zero");
            else if ((draw ? draws : controls).Contains(address))
            {
                ushort expected = (ushort)(rom.ReadByte(0x840000 | raw) | rom.ReadByte(0x840000 | (raw + 1)) << 8);
                AssertEqual(expected, actual, "Crocomire original program field");
                AssertTrue(RoomPlmProgramDefinitions.TryReadWord(address, out ushort shared), "Crocomire shared reader owns field");
                AssertEqual(expected, shared, "Crocomire shared reader native field");
            }
        }
    }
}
