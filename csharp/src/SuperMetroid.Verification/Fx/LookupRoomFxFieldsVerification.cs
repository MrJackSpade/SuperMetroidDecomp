using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Fx;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Verification.Fx;

/// <summary>Audits compiled room FX records, their native field layout, and door-list selection.</summary>
internal static class LookupRoomFxFieldsVerification
{
    /// <summary>
    /// Independent set of accepted bank-$83 record pointers captured before catalog conversion, the terminator words that end
    /// headers without a default record among them; the compiled headers hold no terminator record (#1546).
    /// </summary>
    private static readonly ushort[] OriginalFxRecordPointers =
    [
        0x8000, 0x8010, 0x8020, 0x8030, 0x8040, 0x8050, 0x8060, 0x8070, 0x8080, 0x8090, 0x80A0, 0x80B0,
        0x80C0, 0x80D0, 0x80E0, 0x80F0, 0x8100, 0x8110, 0x8112, 0x8122, 0x8132, 0x8142, 0x8144, 0x8154,
        0x8156, 0x8166, 0x8168, 0x816A, 0x817A, 0x817C, 0x818C, 0x818E, 0x819E, 0x81AE, 0x81BE, 0x81C0,
        0x81C2, 0x81C4, 0x81D4, 0x81E4, 0x81F4, 0x8204, 0x8214, 0x8224, 0x8226, 0x8228, 0x822A, 0x822C,
        0x823C, 0x823E, 0x824E, 0x825E, 0x826E, 0x827E, 0x828E, 0x8290, 0x82A0, 0x82B0, 0x82C0, 0x82D0,
        0x82D2, 0x82E2, 0x82F2, 0x82F4, 0x8304, 0x8314, 0x8324, 0x8334, 0x8336, 0x8338, 0x8348, 0x834A,
        0x834C, 0x834E, 0x835E, 0x836E, 0x8370, 0x8380, 0x8390, 0x83A0, 0x83B0, 0x83C0, 0x83D0, 0x83D2,
        0x83E2, 0x83F2, 0x83F4, 0x83F6, 0x8406, 0x8408, 0x840A, 0x841A, 0x842A, 0x842C, 0x842E, 0x8430,
        0x8440, 0x8450, 0x8460, 0x8470, 0x8480, 0x8490, 0x84A0, 0x84B0, 0x84C0, 0x84D0, 0x84E0, 0x84F0,
        0x8500, 0x8510, 0x8520, 0x8530, 0x8540, 0x8550, 0x8560, 0x8570, 0x8580, 0x8590, 0x85A0, 0x85B0,
        0x85C0, 0x85D0, 0x85E0, 0x85F0, 0x8600, 0x8610, 0x8620, 0x8630, 0x8640, 0x8650, 0x8660, 0x8670,
        0x8680, 0x8690, 0x86A0, 0x86B0, 0x86C0, 0x86D0, 0x86E0, 0x86F0, 0x8700, 0x8710, 0x8720, 0x8730,
        0x8740, 0x8750, 0x8760, 0x8762, 0x8764, 0x8766, 0x8768, 0x876A, 0x877A, 0x878A, 0x879A, 0x87AA,
        0x87BA, 0x87BC, 0x87CC, 0x87DC, 0x87EC, 0x87FC, 0x880C, 0x881C, 0x882C, 0x883C, 0x884C, 0x885C,
        0x886C, 0x887C, 0x888C, 0x889C, 0x88AC, 0x88BC, 0x88CC, 0x88DC, 0x88EC, 0x9AC2, 0x9AD2, 0x9AE2,
        0x9AF2, 0x9B02, 0x9B12, 0x9B22, 0x9B32, 0x9B42, 0x9B52, 0x9B62, 0x9B64, 0x9B74, 0x9B84, 0x9B94,
        0x9BA4, 0x9BB4, 0x9BC4, 0x9BD4, 0x9BE4, 0x9BF4, 0x9C04, 0x9C14, 0x9C24, 0x9C34, 0x9C44, 0x9C54,
        0x9C64, 0x9C74, 0x9C84, 0x9C94, 0x9CA4, 0x9CB4, 0x9CC4, 0x9CD4, 0x9CE4, 0x9CF4, 0x9D04, 0x9D14,
        0x9D24, 0x9D34, 0x9D44, 0x9D54, 0x9D64, 0x9D74, 0x9D84, 0x9D94, 0x9DA4, 0x9DB4, 0x9DC4, 0x9DD4,
        0x9DE4, 0x9DF4, 0x9E04, 0x9E14, 0x9E24, 0x9E34, 0x9E44, 0x9E54, 0x9E64, 0x9E74, 0x9E84, 0x9E94,
        0x9EA4, 0x9EB4, 0x9EC4, 0x9ED4, 0x9EE4, 0x9EF4, 0x9F04, 0x9F14, 0x9F24, 0x9F34, 0x9F44, 0x9F54,
        0x9F64, 0x9F74, 0x9F84, 0x9F94, 0x9FA4, 0x9FB4, 0x9FC4, 0x9FD4, 0x9FE4, 0x9FF4, 0xA004, 0xA014,
        0xA024, 0xA034, 0xA044, 0xA054, 0xA064, 0xA074, 0xA084, 0xA094, 0xA0A4, 0xA0B4, 0xA0C4, 0xA0D4,
        0xA0E4, 0xA0F4, 0xA104, 0xA114, 0xA124, 0xA134, 0xA144, 0xA146, 0xA156, 0xA158, 0xA15A, 0xA15C,
        0xA15E, 0xA17E, 0xA180, 0xA182, 0xA184, 0xA186, 0xA188,
    ];

    /// <summary>The record's two blend bytes, which the import reads into the visor animation and the effect blend.</summary>
    private static readonly int[] BlendOffsets = [10, 11];

    /// <summary>Checks all record identities and each field's native offset and width.</summary>
    internal static void VerifyRoomFxFields(CartridgeImportAddressSpace rom,
        SortedDictionary<ushort, RoomEffectDefinition> originals, SortedSet<ushort> originalEnds)
    {
        Suite(nameof(VerifyRoomFxIdentities), () => VerifyRoomFxIdentities(originals, originalEnds));
        Suite(nameof(VerifyRoomFxDoorPointer), () => VerifyRoomFxDoorPointer(rom));
        Suite(nameof(VerifyRoomFxBaseYPosition), () => VerifyRoomFxBaseYPosition(rom));
        Suite(nameof(VerifyRoomFxTargetYPosition), () => VerifyRoomFxTargetYPosition(rom));
        Suite(nameof(VerifyRoomFxPackedYVelocity), () => VerifyRoomFxPackedYVelocity(rom));
        Suite(nameof(VerifyRoomFxTimer), () => VerifyRoomFxTimer(rom));
        Suite(nameof(VerifyRoomFxType), () => VerifyRoomFxType(rom));
        Suite(nameof(VerifyRoomFxBlends), () => VerifyRoomFxBlends(rom));
        Suite(nameof(VerifyRoomFxLiquidOptions), () => VerifyRoomFxLiquidOptions(rom));
        Suite(nameof(RoomLiquidDecisionsVerification.VerifyRoomLiquidCodec), RoomLiquidDecisionsVerification.VerifyRoomLiquidCodec);
        Suite(nameof(VerifyRoomFxPaletteFxBitset), () => VerifyRoomFxPaletteFxBitset(rom));
        Suite(nameof(VerifyRoomFxBlockAnimationBitset), () => VerifyRoomFxBlockAnimationBitset(rom));
        Suite(nameof(VerifyRoomFxPaletteBlend), () => VerifyRoomFxPaletteBlend(rom));
    }

    /// <summary>Checks membership, ordering, the headers' ends, unknown pointers, and record byte and word bounds.</summary>
    private static void VerifyRoomFxIdentities(SortedDictionary<ushort, RoomEffectDefinition> originals, SortedSet<ushort> originalEnds)
    {
        AssertTrue(new ushort[] { 0xa0b4, 0xa0c4 }.SequenceEqual(MotherBrainFxRecordPointers.DirectRecords),
            "Original Mother Brain direct-record subset and order");
        AssertEqual(295, OriginalFxRecordPointers.Length, "Original FX identity count");
        AssertTrue(OriginalFxRecordPointers.SequenceEqual(originals.Keys.Concat(originalEnds).Order()),
            "Native FX record and terminator membership");
        AssertTrue(OriginalFxRecordPointers.SequenceEqual(RoomFxDefinitions.RecordPointers.Concat(RoomFxDefinitions.EndPointers).Order()),
            "FX enumeration preserves ascending identities");
        AssertTrue(originalEnds.SetEquals(RoomFxDefinitions.EndPointers), "Compiled headers end where the native terminators lie");
        var identities = OriginalFxRecordPointers.ToHashSet();
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            ushort pointer = (ushort)value;
            if (identities.Contains(pointer) && !originalEnds.Contains(pointer))
                AssertEqual(pointer, RoomFxDefinitions.RecordPointerOf(RoomFxDefinitions.RecordAt(pointer)), "FX selected identity");
            else
                AssertThrows<ArgumentOutOfRangeException>(() => RoomFxDefinitions.RecordAt(pointer),
                    "Unknown, interior or terminator FX identity rejects");
        }
        foreach (RoomEffectDefinition record in RoomFxDefinitions.AllRecords)
        {
            foreach (int offset in new[] { int.MinValue, -1, 16, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => record.ReadByte(offset), "FX byte bound");
            foreach (int offset in new[] { int.MinValue, -1, 15, 16, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => record.ReadWord(offset), "FX word bound");
        }
    }

    /// <summary>Checks the door selector word at record offset zero.</summary>
    private static void VerifyRoomFxDoorPointer(CartridgeImportAddressSpace rom) => VerifyRoomFxField(rom, 0, 2, record => record.DoorPointer);
    /// <summary>Checks the liquid base-height word at offset two.</summary>
    private static void VerifyRoomFxBaseYPosition(CartridgeImportAddressSpace rom) => VerifyRoomFxField(rom, 2, 2, record => RoomFxRecordEncoding.SurfaceWord(record.LiquidStartLevel));
    /// <summary>Checks the liquid target-height word at offset four.</summary>
    private static void VerifyRoomFxTargetYPosition(CartridgeImportAddressSpace rom) => VerifyRoomFxField(rom, 4, 2, record => RoomFxRecordEncoding.SurfaceWord(record.LiquidTargetLevel));
    /// <summary>Checks the packed vertical velocity word at offset six.</summary>
    private static void VerifyRoomFxPackedYVelocity(CartridgeImportAddressSpace rom) => VerifyRoomFxField(rom, 6, 2, record => record.LiquidLevelSpeed.RawValue);
    /// <summary>Checks the FX countdown byte at offset eight.</summary>
    private static void VerifyRoomFxTimer(CartridgeImportAddressSpace rom) => VerifyRoomFxField(rom, 8, 1, record => record.LiquidMoveDelay);
    /// <summary>Checks the effect discriminator byte at offset nine.</summary>
    private static void VerifyRoomFxType(CartridgeImportAddressSpace rom) => VerifyRoomFxField(rom, 9, 1, record => RoomFxRecordEncoding.TypeByte(record.Type));
    /// <summary>
    /// Checks that each record holds the visor animation and the effect blend the import reads from its blend bytes at
    /// offsets ten and eleven (#1545); the record holds no byte there, as several bytes read alike. A terminator is no
    /// record (#1546).
    /// </summary>
    private static void VerifyRoomFxBlends(CartridgeImportAddressSpace rom)
    {
        foreach (ushort pointer in OriginalFxRecordPointers)
        {
            int address = 0x830000 | pointer;
            if (ReadVerificationWord(rom, address) == ushort.MaxValue)
                continue;
            RoomEffectDefinition actual = RoomFxDefinitions.RecordAt(pointer);
            RoomVisorAnimation visor =
                RoomFxRecordCodec.VisorAnimation(RoomFxFieldRomByte(rom, address + BlendOffsets[0]), $"FX {pointer:X4}");
            RoomEffectBlend? blend =
                RoomFxRecordCodec.EffectBlend(actual.Type, RoomFxFieldRomByte(rom, address + BlendOffsets[1]), $"FX {pointer:X4}");
            AssertEqual(visor, actual.VisorAnimation, $"FX {pointer:X4} visor animation");
            AssertEqual(blend, actual.EffectBlend, $"FX {pointer:X4} effect blend");
        }
    }

    /// <summary>The record's liquid options byte, which the import reads into the liquid the effect fills its room with.</summary>
    private const int LiquidOptionsOffset = 12;

    /// <summary>
    /// Checks that each record holds the liquid the import reads from its options byte at offset twelve (#1695), and that
    /// every decision the liquid's readers make is the one the byte's bits make; the record holds no byte there, as the
    /// import drops the bits its liquid does not read. A terminator is no record (#1546).
    /// </summary>
    private static void VerifyRoomFxLiquidOptions(CartridgeImportAddressSpace rom)
    {
        foreach (ushort pointer in OriginalFxRecordPointers)
        {
            int address = 0x830000 | pointer;
            if (ReadVerificationWord(rom, address) == ushort.MaxValue)
                continue;
            RoomEffectDefinition actual = RoomFxDefinitions.RecordAt(pointer);
            byte options = RoomFxFieldRomByte(rom, address + LiquidOptionsOffset);
            AssertEqual(RoomFxRecordCodec.Liquid(actual.Type, options), actual.Liquid, $"FX {pointer:X4} liquid");
            RoomLiquidDecisionsVerification.AssertRecordLiquidDecisions(actual, options, $"FX {pointer:X4}");
            AssertThrows<NotSupportedException>(() => actual.ReadByte(LiquidOptionsOffset), $"FX {pointer:X4} holds no options byte");
        }
    }
    /// <summary>Checks the palette effects, encoded through the header's area's list, against the bits at offset thirteen.</summary>
    private static void VerifyRoomFxPaletteFxBitset(CartridgeImportAddressSpace rom) => VerifyRoomFxField(rom, 13, 1,
        record => RoomFxRecordEncoding.PaletteEffectsByte(record.PaletteEffects, record.HeaderArea));
    /// <summary>Checks the animated tiles, encoded through the header's area's list, against the bits at offset fourteen.</summary>
    private static void VerifyRoomFxBlockAnimationBitset(CartridgeImportAddressSpace rom) => VerifyRoomFxField(rom, 14, 1,
        record => RoomFxRecordEncoding.AnimatedTilesByte(record.BlockAnimations, record.HeaderArea));
    /// <summary>Checks the palette blend selector at offset fifteen.</summary>
    private static void VerifyRoomFxPaletteBlend(CartridgeImportAddressSpace rom) => VerifyRoomFxField(rom, 15, 1, record => RoomFxRecordEncoding.PaletteBlendByte(record.PaletteBlend));
    /// <summary>
    /// Compares a compiled field and overlapping byte and word views with the original record. The typed fields are
    /// encoded back through the record codec (#1415), so each check asserts the import decodes the cartridge byte to the
    /// typed value. A terminator is no record (#1546), so its word is checked as a header's end instead.
    /// </summary>
    private static void VerifyRoomFxField(CartridgeImportAddressSpace rom, int offset, int width,
        Func<RoomEffectDefinition, int> field)
    {
        var enumerated = RoomFxDefinitions.RecordPointers.ToDictionary(pointer => pointer, pointer => RoomFxDefinitions.RecordAt(pointer));
        foreach (ushort pointer in OriginalFxRecordPointers)
        {
            int address = 0x830000 | pointer;
            if (ReadVerificationWord(rom, address) == ushort.MaxValue)
                continue;
            byte OriginalByte(int index) => RoomFxFieldRomByte(rom, address + index);
            int expected = OriginalByte(offset);
            if (width == 2) expected |= LittleEndianBytes.Word(0, OriginalByte(offset + 1));
            RoomEffectDefinition actual = RoomFxDefinitions.RecordAt(pointer);
            AssertEqual(expected, field(actual), $"FX {pointer:X4} field at {offset}");
            AssertEqual(expected, field(enumerated[pointer]), "FX enumeration field view");
            for (int index = offset; index < offset + width; index++)
            {
                AssertEqual(OriginalByte(index), actual.ReadByte(index), "FX byte view");
                // A word reaching a blend byte has no view: the record holds what the import read from it.
                if (index < 15 && !BlendOffsets.Contains(index + 1))
                    AssertEqual(LittleEndianBytes.Word(OriginalByte(index), OriginalByte(index + 1)),
                        actual.ReadWord(index), "FX overlapping word view");
            }
        }
    }

    /// <summary>Reads an unmodified bank-$83 field byte for the independent record oracle.</summary>
    private static byte RoomFxFieldRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Compares every accepted list root and door value with a first-match native byte walk.</summary>
    internal static void VerifyRoomFxListSelection(CartridgeImportAddressSpace rom)
    {
        // Native $89:AB99..ABB7: zero loads, FFFF returns, then compare door.
        // Build a first-match map from original bytes; this oracle does not replay
        // the production loop or use its selected record fields.
        var identities = OriginalFxRecordPointers.ToHashSet();
        foreach (ushort root in OriginalFxRecordPointers)
        {
            var firstByDoor = new Dictionary<ushort, ushort>();
            ushort fallback = 0;
            int cursor = root;
            int recordCount = 0;
            while (true)
            {
                AssertTrue(identities.Contains((ushort)cursor), "Native FX suffix stays in reviewed domain");
                AssertTrue(++recordCount <= 256, "Native FX suffix is bounded");
                ushort door = ReadVerificationWord(rom, 0x830000 | cursor);
                if (door == 0) { fallback = (ushort)cursor; break; }
                if (door == ushort.MaxValue) break;
                firstByDoor.TryAdd(door, (ushort)cursor);
                cursor = SnesAddressMath.OffsetInBank(cursor + 16);
            }
            foreach (DoorDefinition? door in DoorDefinitionsConstants.All.Append(null))
            {
                ushort word = door is null ? (ushort)0 : DoorDefinitions.PointerOf(door);
                ushort expected = firstByDoor.TryGetValue(word, out ushort matching) ? matching : fallback;
                ushort actual = RoomFxDefinitions.SelectedRecordPointer(RoomFxDefinitions.Suffix(root), door);
                if (actual != expected)
                    throw new InvalidOperationException(
                        $"FX selector root {root:X4}, door {word:X4}: native {expected:X4}, actual {actual:X4}.");
            }
        }
        foreach (DoorDefinition? door in DoorDefinitionsConstants.All.Append(null))
            AssertEqual((ushort)0, RoomFxDefinitions.SelectedRecordPointer(null, door), "Null FX root for every door");
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            ushort pointer = (ushort)value;
            if (pointer != 0 && !identities.Contains(pointer))
                AssertThrows<ArgumentOutOfRangeException>(() => RoomFxDefinitions.Suffix(pointer),
                    "Unknown FX root rejects before door comparison");
        }
        Console.WriteLine("FX list selection: all 295 suffixes x every door, null root and invalid roots pass.");
    }
}
