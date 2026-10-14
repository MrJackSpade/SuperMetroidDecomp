using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Verification.Bosses.Crocomire;

/// <summary>Checks complete ownership and cartridge values of Crocomire arena PLM program words.</summary>
internal static class LookupCrocomireProgramsVerification
{
    /// <summary>Compares ten native control addresses with the compiled Crocomire PLM reader.</summary>
    internal static void VerifyCrocomireProgramControls(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyCrocomireProgramField), () => VerifyCrocomireProgramField(rom, false));

    /// <summary>Compares five native draw operands with the compiled Crocomire PLM reader.</summary>
    internal static void VerifyCrocomireProgramDraws(ISnesAddressSpace rom) =>
        Suite(nameof(VerifyCrocomireProgramField), () => VerifyCrocomireProgramField(rom, true));

    /// <summary>Checks owned and unowned bank-$84 addresses, then compares the selected field kind with retail bytes.</summary>
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
                ushort expected = (ushort)(SnesLittleEndian.Word(CrocomireProgramFieldRomByte(rom, 0x840000 | raw), CrocomireProgramFieldRomByte(rom, 0x840000 | (raw + 1))));
                AssertEqual(expected, actual, "Crocomire original program field");
                AssertTrue(RoomPlmProgramDefinitions.TryReadWord(address, out ushort shared), "Crocomire shared reader owns field");
                AssertEqual(expected, shared, "Crocomire shared reader native field");
            }
        }
    }

    /// <summary>Reads one retail bank-$84 byte for the exact Crocomire program oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Crocomire program field")]
    private static byte CrocomireProgramFieldRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
