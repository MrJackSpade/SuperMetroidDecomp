using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyElevatorPlatformControls(SuperMetroidAddressSpace rom) => VerifyElevatorPlatformProgramField(rom, 0);
    private static void VerifyElevatorPlatformDrawSelection(SuperMetroidAddressSpace rom) => VerifyElevatorPlatformProgramField(rom, 1);
    private static void VerifyElevatorPlatformLoopTarget(SuperMetroidAddressSpace rom) => VerifyElevatorPlatformProgramField(rom, 2);

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
