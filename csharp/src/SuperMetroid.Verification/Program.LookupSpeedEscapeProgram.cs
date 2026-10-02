using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifySpeedEscapeProgramControls(SuperMetroidAddressSpace rom) =>
        VerifySpeedEscapeProgramField(rom, false);

    private static void VerifySpeedEscapeProgramCallbacks(SuperMetroidAddressSpace rom) =>
        VerifySpeedEscapeProgramField(rom, true);

    private static void VerifySpeedEscapeProgramField(SuperMetroidAddressSpace rom, bool callback)
    {
        ushort[] controls = [0xb88a,0xb88e,0xb890,0xb894,0xb896,0xb89a];
        ushort[] callbacks = [0xb88c,0xb892,0xb898];
        AssertEqual(9, SpeedBoosterEscapePlmProgramDefinitions.WordCount, "Speed escape original word count");
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort address = (ushort)raw;
            bool owned = controls.Contains(address) || callbacks.Contains(address);
            AssertEqual(owned, SpeedBoosterEscapePlmProgramDefinitions.TryReadMechanicsWord(address, out ushort value),
                "Speed escape complete word ownership");
            if (!owned) AssertEqual((ushort)0, value, "Speed escape unowned word is zero");
            else if ((callback ? callbacks : controls).Contains(address))
            {
                ushort native = ReadSamusEaterPlmWord(rom, 0x840000 | raw);
                AssertEqual(native, value, "Speed escape original field");
                AssertTrue(RoomPlmProgramDefinitions.TryReadWord(address, out ushort shared), "Speed escape shared reader ownership");
                AssertEqual(native, shared, "Speed escape shared original field");
            }
        }
    }
}
