using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Checks that the six authored control words in the Speed Booster escape PLM match retail and have exact address ownership.</summary>
    /// <param name="rom">Retail cartridge address space used to read the original PLM program words.</param>
    private static void VerifySpeedEscapeProgramControls(SuperMetroidAddressSpace rom) =>
        VerifySpeedEscapeProgramField(rom, false);

    /// <summary>Checks that the three authored callback words in the Speed Booster escape PLM match retail and have exact address ownership.</summary>
    /// <param name="rom">Retail cartridge address space used to read the original PLM program words.</param>
    private static void VerifySpeedEscapeProgramCallbacks(SuperMetroidAddressSpace rom) =>
        VerifySpeedEscapeProgramField(rom, true);

    /// <summary>Compares the selected Speed Booster escape PLM word class against retail across every possible 16-bit bank address and verifies shared-reader agreement.</summary>
    /// <param name="rom">Retail cartridge address space containing the original bank-$84 instruction list.</param>
    /// <param name="callback"><see langword="true"/> selects callback operands; otherwise selects control-flow and timing words.</param>
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
