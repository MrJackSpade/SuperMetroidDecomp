using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Runs identity and native-field comparisons for every supported retail door header.</summary>
    /// <param name="rom">Retail address space used to read the original bank-$83 header words.</param>
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

    /// <summary>Checks that only documented physical door-header pointers resolve as door identities.</summary>
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

    /// <summary>Compares the two-byte destination room pointer with its native header field.</summary>
    /// <param name="rom">Retail address space containing the door headers.</param>
    private static void VerifyDoorHeaderDestinationRoomPointer(SuperMetroidAddressSpace rom) => VerifyDoorHeaderField(rom, 0, 2, header => header.DestinationRoomPointer);

    /// <summary>Compares the one-byte orientation with its native header field.</summary>
    /// <param name="rom">Retail address space containing the door headers.</param>
    private static void VerifyDoorHeaderOrientation(SuperMetroidAddressSpace rom) => VerifyDoorHeaderField(rom, 3, 1, header => header.Orientation);

    /// <summary>Compares the one-byte PLM X coordinate with its native header field.</summary>
    /// <param name="rom">Retail address space containing the door headers.</param>
    private static void VerifyDoorHeaderPlmX(SuperMetroidAddressSpace rom) => VerifyDoorHeaderField(rom, 4, 1, header => header.PlmX);

    /// <summary>Compares the one-byte PLM Y coordinate with its native header field.</summary>
    /// <param name="rom">Retail address space containing the door headers.</param>
    private static void VerifyDoorHeaderPlmY(SuperMetroidAddressSpace rom) => VerifyDoorHeaderField(rom, 5, 1, header => header.PlmY);

    /// <summary>Compares the one-byte destination screen X coordinate with its native header field.</summary>
    /// <param name="rom">Retail address space containing the door headers.</param>
    private static void VerifyDoorHeaderDestinationScreenX(SuperMetroidAddressSpace rom) => VerifyDoorHeaderField(rom, 6, 1, header => header.DestinationScreenX);

    /// <summary>Compares the one-byte destination screen Y coordinate with its native header field.</summary>
    /// <param name="rom">Retail address space containing the door headers.</param>
    private static void VerifyDoorHeaderDestinationScreenY(SuperMetroidAddressSpace rom) => VerifyDoorHeaderField(rom, 7, 1, header => header.DestinationScreenY);

    /// <summary>Compares the two-byte Samus-distance value with its native header field.</summary>
    /// <param name="rom">Retail address space containing the door headers.</param>
    private static void VerifyDoorHeaderSamusDistance(SuperMetroidAddressSpace rom) => VerifyDoorHeaderField(rom, 8, 2, header => header.SamusDistance);

    /// <summary>Compares the two-byte setup-code pointer and its Ceres Mode-7 consumer predicate with native data.</summary>
    /// <param name="rom">Retail address space containing the door headers.</param>
    private static void VerifyDoorHeaderSetupCodePointer(SuperMetroidAddressSpace rom) => VerifyDoorHeaderField(rom, 10, 2, header => header.SetupCodePointer);

    /// <summary>Reads a selected field from each native door header and compares it with the compiled header projection.</summary>
    /// <param name="rom">Retail address space containing the bank-$83 header records.</param>
    /// <param name="offset">Byte offset of the field within each header.</param>
    /// <param name="width">Number of bytes in the field, one or two.</param>
    /// <param name="field">Projection selecting the corresponding value from a decoded header.</param>
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
