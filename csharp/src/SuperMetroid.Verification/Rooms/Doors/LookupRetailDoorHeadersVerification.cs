using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>Checks compiled door-header identities and each physical bank-$83 record field.</summary>
/// <remarks>
/// Since #1414 the compiled doors hold domain values: the orientation, Samus speed and setup routine checks decode the
/// cartridge field through the import codec and assert it equals the compiled door's value, rather than asserting the
/// raw byte or word.
/// </remarks>
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
        Suite(nameof(VerifyDoorHeaderSamusSpeed), () => VerifyDoorHeaderSamusSpeed(rom));
        Suite(nameof(VerifyDoorHeaderSetupRoutine), () => VerifyDoorHeaderSetupRoutine(rom));
        Suite(nameof(VerifyDoorSamusSpeedDecoding), () => VerifyDoorSamusSpeedDecoding());
    }

    /// <summary>
    /// #1414: the Samus-distance word decodes to the push the transition applied before: a negative word is the axis
    /// default (384/256 pixel per step vertically, 200/256 horizontally), any other word is its own push.
    /// </summary>
    private static void VerifyDoorSamusSpeedDecoding()
    {
        AssertEqual(DoorSamusSpeed.AxisDefault, DoorRecordCodec.DecodeSamusSpeed(0x8000), "a negative distance word is the axis default");
        AssertEqual(384, DoorSamusSpeed.AxisDefault.StepSubpixels(DoorDirection.Up), "vertical axis default push");
        AssertEqual(384, DoorSamusSpeed.AxisDefault.StepSubpixels(DoorDirection.Down), "vertical axis default push");
        AssertEqual(200, DoorSamusSpeed.AxisDefault.StepSubpixels(DoorDirection.Left), "horizontal axis default push");
        AssertEqual(200, DoorSamusSpeed.AxisDefault.StepSubpixels(DoorDirection.Right), "horizontal axis default push");
        AssertEqual(DoorSamusSpeed.SubpixelsPerStep(0x01c0), DoorRecordCodec.DecodeSamusSpeed(0x01c0), "a door's own distance word");
        AssertEqual(0x01c0, DoorRecordCodec.DecodeSamusSpeed(0x01c0).StepSubpixels(DoorDirection.Up), "a door's own push ignores the axis");
        AssertEqual(DoorSamusSpeed.None, DoorRecordCodec.DecodeSamusSpeed(0), "a zero distance word is no push");
        AssertEqual(0, DoorSamusSpeed.None.StepSubpixels(DoorDirection.Left), "no push");
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
    private static void VerifyDoorHeaderDestinationRoomPointer(SuperMetroidAddressSpace rom) =>
        VerifyDoorHeaderField<int>(rom, 0, 2, raw => raw, header => RoomHeaderDefinitions.PointerOf(header.DestinationRoom));
    /// <summary>The orientation byte at offset three decodes to the compiled direction and closing behavior.</summary>
    private static void VerifyDoorHeaderOrientation(SuperMetroidAddressSpace rom) =>
        VerifyDoorHeaderField(rom, 3, 1, raw => DoorRecordCodec.DecodeOrientation(checked((byte)raw)), header => header.Orientation);
    /// <summary>Matches the door PLM's horizontal block coordinate at offset four.</summary>
    private static void VerifyDoorHeaderPlmX(SuperMetroidAddressSpace rom) => VerifyDoorHeaderField<int>(rom, 4, 1, raw => raw, header => header.PlmBlock.X);
    /// <summary>Matches the door PLM's vertical block coordinate at offset five.</summary>
    private static void VerifyDoorHeaderPlmY(SuperMetroidAddressSpace rom) => VerifyDoorHeaderField<int>(rom, 5, 1, raw => raw, header => header.PlmBlock.Y);
    /// <summary>Matches the destination screen's horizontal coordinate at offset six.</summary>
    private static void VerifyDoorHeaderDestinationScreenX(SuperMetroidAddressSpace rom) => VerifyDoorHeaderField<int>(rom, 6, 1, raw => raw, header => header.DestinationScreen.X);
    /// <summary>Matches the destination screen's vertical coordinate at offset seven.</summary>
    private static void VerifyDoorHeaderDestinationScreenY(SuperMetroidAddressSpace rom) => VerifyDoorHeaderField<int>(rom, 7, 1, raw => raw, header => header.DestinationScreen.Y);
    /// <summary>The two-byte Samus-distance word at offset eight decodes to the compiled push speed.</summary>
    private static void VerifyDoorHeaderSamusSpeed(SuperMetroidAddressSpace rom) =>
        VerifyDoorHeaderField(rom, 8, 2, raw => DoorRecordCodec.DecodeSamusSpeed(checked((ushort)raw)), header => header.SamusSpeed);

    /// <summary>
    /// The setup word at offset ten names the compiled setup routine, and only the Ceres elevator shaft's routine
    /// selects the turning-floor view.
    /// </summary>
    private static void VerifyDoorHeaderSetupRoutine(SuperMetroidAddressSpace rom)
    {
        VerifyDoorHeaderField(rom, 10, 2,
            raw => DoorIdentityWords.SetupRoutines.Decode(checked((ushort)raw), "door setup routine"), header => header.SetupCode);
        foreach (ushort pointer in EnumerateRetailDoorPointers())
        {
            DoorDefinition door = DoorDefinitions.Get(pointer);
            AssertEqual(door.SetupCode == DoorSetupCode.DoorASM_ToCeresElevatorShaft, door.EntersCeresElevatorShaft,
                "Door setup selection preserves the Ceres elevator shaft consumer predicate");
        }
    }

    /// <summary>Reads one physical field for every door, decodes it, and compares the compiled typed projection.</summary>
    private static void VerifyDoorHeaderField<T>(SuperMetroidAddressSpace rom, int offset, int width,
        Func<int, T> decode, Func<DoorDefinition, T> field)
    {
        foreach (ushort pointer in EnumerateRetailDoorPointers())
        {
            int address = (0x830000 | pointer) + offset;
            int raw = DoorHeaderFieldRomByte(rom, address);
            if (width == 2) raw |= SnesLittleEndian.Word(0, DoorHeaderFieldRomByte(rom, address + 1));
            DoorDefinition actual = DoorDefinitions.Get(pointer);
            AssertEqual(decode(raw), field(actual), $"Door {pointer:X4} native field at {offset}");
        }
    }

    /// <summary>Reads one original door-header byte for field-level comparison.</summary>
    private static byte DoorHeaderFieldRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
