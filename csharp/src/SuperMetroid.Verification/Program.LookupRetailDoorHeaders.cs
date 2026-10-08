using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyRetailDoorHeaders(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyDoorHeaderIdentities), () => VerifyDoorHeaderIdentities());
        Suite(nameof(VerifyDoorHeaderDestinationRoomPointer), () => VerifyDoorHeaderDestinationRoomPointer(rom));
        Suite(nameof(VerifyDoorHeaderOrientation), () => VerifyDoorHeaderOrientation(rom));
        Suite(nameof(VerifyDoorHeaderPlmX), () => VerifyDoorHeaderPlmX(rom));
        Suite(nameof(VerifyDoorHeaderPlmY), () => VerifyDoorHeaderPlmY(rom));
        Suite(nameof(VerifyDoorHeaderDestinationScreenX), () => VerifyDoorHeaderDestinationScreenX(rom));
        Suite(nameof(VerifyDoorHeaderDestinationScreenY), () => VerifyDoorHeaderDestinationScreenY(rom));
        Suite(nameof(VerifyDoorHeaderSamusDistance), () => VerifyDoorHeaderSamusDistance(rom));
        Suite(nameof(VerifyDoorHeaderSetupCodePointer), () => VerifyDoorHeaderSetupCodePointer(rom));
    }

    private static void VerifyDoorHeaderIdentities()
    {
        // Original physical record regions are independently documented in bank83;
        // the separate88FC and A18A overlaps are also supported door identities.
        var original = EnumerateRetailDoorPointers().ToHashSet();
        AssertEqual(597, original.Count, "Original physical header domain");
        original.Add(0x88fc);
        original.Add(0xa18a);
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            ushort pointer = (ushort)value;
            if (original.Contains(pointer))
                AssertEqual(pointer, DoorDefinitions.Get(pointer).Pointer, "Selected door identity");
            else
                AssertThrows<ArgumentOutOfRangeException>(() => DoorDefinitions.Get(pointer),
                    $"Non-door identity {pointer:X4}, including interiors and FX data, rejects");
        }
    }

    private static void VerifyDoorHeaderDestinationRoomPointer(SuperMetroidAddressSpace rom) => VerifyDoorHeaderField(rom, 0, 2, header => header.DestinationRoomPointer);
    private static void VerifyDoorHeaderOrientation(SuperMetroidAddressSpace rom) => VerifyDoorHeaderField(rom, 3, 1, header => header.Orientation);
    private static void VerifyDoorHeaderPlmX(SuperMetroidAddressSpace rom) => VerifyDoorHeaderField(rom, 4, 1, header => header.PlmX);
    private static void VerifyDoorHeaderPlmY(SuperMetroidAddressSpace rom) => VerifyDoorHeaderField(rom, 5, 1, header => header.PlmY);
    private static void VerifyDoorHeaderDestinationScreenX(SuperMetroidAddressSpace rom) => VerifyDoorHeaderField(rom, 6, 1, header => header.DestinationScreenX);
    private static void VerifyDoorHeaderDestinationScreenY(SuperMetroidAddressSpace rom) => VerifyDoorHeaderField(rom, 7, 1, header => header.DestinationScreenY);
    private static void VerifyDoorHeaderSamusDistance(SuperMetroidAddressSpace rom) => VerifyDoorHeaderField(rom, 8, 2, header => header.SamusDistance);
    private static void VerifyDoorHeaderSetupCodePointer(SuperMetroidAddressSpace rom) => VerifyDoorHeaderField(rom, 10, 2, header => header.SetupCodePointer);

    private static void VerifyDoorHeaderField(SuperMetroidAddressSpace rom, int offset, int width,
        Func<CartridgeDoorHeader, int> field)
    {
        foreach (ushort pointer in EnumerateRetailDoorPointers().Prepend((ushort)0x88fc).Prepend((ushort)0xa18a))
        {
            int address = (0x830000 | pointer) + offset;
            int expected = rom.ReadByte(address);
            if (width == 2) expected |= rom.ReadByte(address + 1) << 8;
            CartridgeDoorHeader actual = DoorDefinitions.Get(pointer);
            AssertEqual(expected, field(actual), $"Door {pointer:X4} native field at {offset}");
            if (offset == 10)
                AssertEqual(expected == 0xe4e0, actual.UsesCeresElevatorMode7,
                    "Door setup selection preserves the Ceres Mode7 consumer predicate");
        }
    }
}
