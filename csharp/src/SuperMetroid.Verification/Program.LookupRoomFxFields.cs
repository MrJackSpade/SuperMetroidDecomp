using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    // Independently captured accepted identities before replacing the original record array.
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

    private static void VerifyRoomFxFields(SuperMetroidAddressSpace rom,
        SortedDictionary<ushort, RoomFxRecordDefinition> originals)
    {
        VerifyRoomFxIdentities(originals);
        VerifyRoomFxDoorPointer(rom);
        VerifyRoomFxBaseYPosition(rom);
        VerifyRoomFxTargetYPosition(rom);
        VerifyRoomFxPackedYVelocity(rom);
        VerifyRoomFxTimer(rom);
        VerifyRoomFxType(rom);
        VerifyRoomFxDefaultLayerBlend(rom);
        VerifyRoomFxLayer3LayerBlend(rom);
        VerifyRoomFxLiquidOptions(rom);
        VerifyRoomFxPaletteFxBitset(rom);
        VerifyRoomFxAnimatedTileBitset(rom);
        VerifyRoomFxPaletteBlend(rom);
    }

    private static void VerifyRoomFxIdentities(SortedDictionary<ushort, RoomFxRecordDefinition> originals)
    {
        AssertTrue(new ushort[] { 0xa0b4, 0xa0c4 }.SequenceEqual(MotherBrainFxRecordPointers.DirectRecords),
            "Original Mother Brain direct-record subset and order");
        AssertEqual(295, OriginalFxRecordPointers.Length, "Original FX identity count");
        AssertTrue(OriginalFxRecordPointers.SequenceEqual(originals.Keys), "Native FX record membership");
        AssertTrue(OriginalFxRecordPointers.SequenceEqual(RoomFxRecordDefinitions.All.Select(x => x.Pointer)),
            "FX enumeration preserves ascending identities");
        var identities = OriginalFxRecordPointers.ToHashSet();
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            ushort pointer = (ushort)value;
            if (identities.Contains(pointer))
                AssertEqual(pointer, RoomFxRecordDefinitions.Get(pointer).Pointer, "FX selected identity");
            else
                AssertThrows<InvalidDataException>(() => RoomFxRecordDefinitions.Get(pointer),
                    "Unknown/interior FX identity rejects");
        }
        foreach (RoomFxRecordDefinition record in RoomFxRecordDefinitions.All)
        {
            foreach (int offset in new[] { int.MinValue, -1, 16, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => record.ReadByte(offset), "FX byte bound");
            foreach (int offset in new[] { int.MinValue, -1, 15, 16, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => record.ReadWord(offset), "FX word bound");
        }
    }

    private static void VerifyRoomFxDoorPointer(SuperMetroidAddressSpace rom) => VerifyRoomFxField(rom, 0, 2, record => record.DoorPointer);
    private static void VerifyRoomFxBaseYPosition(SuperMetroidAddressSpace rom) => VerifyRoomFxField(rom, 2, 2, record => record.BaseYPosition);
    private static void VerifyRoomFxTargetYPosition(SuperMetroidAddressSpace rom) => VerifyRoomFxField(rom, 4, 2, record => record.TargetYPosition);
    private static void VerifyRoomFxPackedYVelocity(SuperMetroidAddressSpace rom) => VerifyRoomFxField(rom, 6, 2, record => record.PackedYVelocity);
    private static void VerifyRoomFxTimer(SuperMetroidAddressSpace rom) => VerifyRoomFxField(rom, 8, 1, record => record.Timer);
    private static void VerifyRoomFxType(SuperMetroidAddressSpace rom) => VerifyRoomFxField(rom, 9, 1, record => record.Type);
    private static void VerifyRoomFxDefaultLayerBlend(SuperMetroidAddressSpace rom) => VerifyRoomFxField(rom, 10, 1, record => record.DefaultLayerBlend);
    private static void VerifyRoomFxLayer3LayerBlend(SuperMetroidAddressSpace rom) => VerifyRoomFxField(rom, 11, 1, record => record.Layer3LayerBlend);
    private static void VerifyRoomFxLiquidOptions(SuperMetroidAddressSpace rom) => VerifyRoomFxField(rom, 12, 1, record => record.LiquidOptions);
    private static void VerifyRoomFxPaletteFxBitset(SuperMetroidAddressSpace rom) => VerifyRoomFxField(rom, 13, 1, record => record.PaletteFxBitset);
    private static void VerifyRoomFxAnimatedTileBitset(SuperMetroidAddressSpace rom) => VerifyRoomFxField(rom, 14, 1, record => record.AnimatedTileBitset);
    private static void VerifyRoomFxPaletteBlend(SuperMetroidAddressSpace rom) => VerifyRoomFxField(rom, 15, 1, record => record.PaletteBlend);
    private static void VerifyRoomFxField(SuperMetroidAddressSpace rom, int offset, int width,
        Func<RoomFxRecordDefinition, int> field)
    {
        var enumerated = RoomFxRecordDefinitions.All.ToDictionary(x => x.Pointer);
        foreach (ushort pointer in OriginalFxRecordPointers)
        {
            int address = 0x830000 | pointer;
            bool terminator = ReadVerificationWord(rom, address) == ushort.MaxValue;
            // The pre-conversion terminator DTO has a zero-filled tail; adjacent native
            // bytes belong to another record and are not part of that caller-visible view.
            byte OriginalByte(int index) => terminator && index >= 2 ? (byte)0 : rom.ReadByte(address + index);
            int expected = OriginalByte(offset);
            if (width == 2) expected |= OriginalByte(offset + 1) << 8;
            RoomFxRecordDefinition actual = RoomFxRecordDefinitions.Get(pointer);
            AssertEqual(expected, field(actual), $"FX {pointer:X4} field at {offset}");
            AssertEqual(expected, field(enumerated[pointer]), "FX enumeration field view");
            for (int index = offset; index < offset + width; index++)
            {
                AssertEqual(OriginalByte(index), actual.ReadByte(index), "FX byte view");
                if (index < 15)
                    AssertEqual((ushort)(OriginalByte(index) | OriginalByte(index + 1) << 8),
                        actual.ReadWord(index), "FX overlapping word view");
            }
        }
    }

    private static void VerifyRoomFxListSelection(SuperMetroidAddressSpace rom)
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
                cursor = (cursor + 16) & 0xffff;
            }
            for (int value = 0; value <= ushort.MaxValue; value++)
            {
                ushort door = (ushort)value;
                ushort expected = firstByDoor.TryGetValue(door, out ushort matching) ? matching : fallback;
                ushort actual = RoomFxRecordDefinitions.Select(root, door);
                if (actual != expected)
                    throw new InvalidOperationException(
                        $"FX selector root {root:X4}, door {door:X4}: native {expected:X4}, actual {actual:X4}.");
            }
        }
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            ushort pointer = (ushort)value;
            AssertEqual((ushort)0, RoomFxRecordDefinitions.Select(0, pointer), "Null FX root for every door");
            if (pointer != 0 && !identities.Contains(pointer))
            {
                AssertThrows<InvalidDataException>(() => RoomFxRecordDefinitions.Select(pointer, 0),
                    "Unknown FX root rejects before door comparison");
                AssertThrows<InvalidDataException>(() => RoomFxRecordDefinitions.Select(pointer, ushort.MaxValue),
                    "Unknown FX root cannot become a terminator match");
            }
        }
        Console.WriteLine("FX list selection: all 295 suffixes x 65536 doors, null root and invalid roots pass.");
    }
}
