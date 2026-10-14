using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static DoorDefinitionsVerification;
using static SuiteTimingVerification;

/// <summary>Checks compiled door-header identities and each physical bank-$83 record field.</summary>
internal static class LookupRetailDoorHeadersVerification
{
    /// <summary>Runs the full set of focused retail door-header field comparisons.</summary>
    internal static void VerifyRetailDoorHeaders(SuperMetroidAddressSpace rom)
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

    /// <summary>Accepts exactly the 597 physical door headers and rejects pseudo-doors and interior bytes.</summary>
    private static void VerifyDoorHeaderIdentities()
    {
        // Original physical record regions are independently documented in bank83. The
        // 88FC and A18A elevator pseudo-doors are door-list entries, not headers.
        var original = EnumerateRetailDoorPointers().ToHashSet();
        AssertEqual(597, original.Count, "Original physical header domain");
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

    /// <summary>Matches each door's two-byte destination room pointer at record offset zero.</summary>
    private static void VerifyDoorHeaderDestinationRoomPointer(SuperMetroidAddressSpace rom) => VerifyDoorHeaderField(rom, 0, 2, header => header.DestinationRoomPointer);
    /// <summary>Matches the encoded orientation byte at door-header offset three.</summary>
    private static void VerifyDoorHeaderOrientation(SuperMetroidAddressSpace rom) => VerifyDoorHeaderField(rom, 3, 1, header => header.Orientation.Encode());
    /// <summary>Matches the door PLM's horizontal block coordinate at offset four.</summary>
    private static void VerifyDoorHeaderPlmX(SuperMetroidAddressSpace rom) => VerifyDoorHeaderField(rom, 4, 1, header => header.PlmBlock.X);
    /// <summary>Matches the door PLM's vertical block coordinate at offset five.</summary>
    private static void VerifyDoorHeaderPlmY(SuperMetroidAddressSpace rom) => VerifyDoorHeaderField(rom, 5, 1, header => header.PlmBlock.Y);
    /// <summary>Matches the destination screen's horizontal coordinate at offset six.</summary>
    private static void VerifyDoorHeaderDestinationScreenX(SuperMetroidAddressSpace rom) => VerifyDoorHeaderField(rom, 6, 1, header => header.DestinationScreen.X);
    /// <summary>Matches the destination screen's vertical coordinate at offset seven.</summary>
    private static void VerifyDoorHeaderDestinationScreenY(SuperMetroidAddressSpace rom) => VerifyDoorHeaderField(rom, 7, 1, header => header.DestinationScreen.Y);
    /// <summary>Matches the two-byte Samus transition distance at offset eight.</summary>
    private static void VerifyDoorHeaderSamusDistance(SuperMetroidAddressSpace rom) => VerifyDoorHeaderField(rom, 8, 2, header => header.SamusDistance);
    /// <summary>Matches the setup-code word and the Ceres Mode 7 selection predicate at offset ten.</summary>
    private static void VerifyDoorHeaderSetupCodePointer(SuperMetroidAddressSpace rom) => VerifyDoorHeaderField(rom, 10, 2, header => (ushort)header.SetupCodePointer);

    /// <summary>Reads one physical field for every door and compares the compiled typed projection.</summary>
    private static void VerifyDoorHeaderField(SuperMetroidAddressSpace rom, int offset, int width,
        Func<CartridgeDoorHeader, int> field)
    {
        foreach (ushort pointer in EnumerateRetailDoorPointers())
        {
            int address = (0x830000 | pointer) + offset;
            int expected = DoorHeaderFieldRomByte(rom, address);
            if (width == 2) expected |= SnesLittleEndian.Word(0, DoorHeaderFieldRomByte(rom, address + 1));
            CartridgeDoorHeader actual = DoorDefinitions.Get(pointer);
            AssertEqual(expected, field(actual), $"Door {pointer:X4} native field at {offset}");
            if (offset == 10)
                AssertEqual(expected == 0xe4e0, actual.UsesCeresElevatorMode7,
                    "Door setup selection preserves the Ceres Mode7 consumer predicate");
        }
    }

    /// <summary>Reads one original door-header byte for field-level comparison.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for door header field")]
    private static byte DoorHeaderFieldRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
