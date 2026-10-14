using SuperMetroid.Core.Hardware;
using static SamusEaterPlmDefinitionsVerification;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Rooms.Plms;

/// <summary>Compares the bounded elevator-platform PLM program with the retail bank-$84 words.</summary>
internal static class LookupElevatorPlatformProgramVerification
{
    /// <summary>Checks the five engine-control words selected from the platform program.</summary>
    internal static void VerifyElevatorPlatformControls(SuperMetroidAddressSpace rom) => VerifyElevatorPlatformProgramField(rom, 0);
    /// <summary>Checks the four draw selectors shared with the room PLM interpreter.</summary>
    internal static void VerifyElevatorPlatformDrawSelection(SuperMetroidAddressSpace rom) => VerifyElevatorPlatformProgramField(rom, 1);
    /// <summary>Checks the final loop target and its ownership by the shared interpreter.</summary>
    internal static void VerifyElevatorPlatformLoopTarget(SuperMetroidAddressSpace rom) => VerifyElevatorPlatformProgramField(rom, 2);

    /// <summary>Checks each expected word, every 16-bit address for admission, and zero output on rejection.</summary>
    private static void VerifyElevatorPlatformProgramField(SuperMetroidAddressSpace rom, int field)
    {
        ushort[] addresses = field switch
        {
            0 => [0xafb6,0xafba,0xafbe,0xafc2,0xafc6],
            1 => [0xafb8,0xafbc,0xafc0,0xafc4],
            _ => [0xafc8],
        };
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort address = (ushort)raw;
            bool owned = raw >= 0xafb6 && raw <= 0xafc8 && (raw & 1) == 0;
            AssertEqual(owned, ElevatorPlatformPlmDefinitions.TryReadMechanicsWord(address, out ushort value),
                "Elevator platform original program word domain");
            if (!owned)
                AssertEqual((ushort)0, value, "Elevator platform rejected word output");
            if (!addresses.Contains(address)) continue;
            ushort expected = ReadSamusEaterPlmWord(rom, 0x840000 | raw);
            AssertEqual(expected, value, "Elevator platform original program field");
            AssertTrue(RoomPlmProgramDefinitions.TryReadWord(address, out ushort composed),
                "Elevator platform shared program ownership");
            AssertEqual(expected, composed, "Elevator platform composed original field");
        }
    }
}
