using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    // Original identities captured from f2e63273eef07b384814af455e129d560fb8e109.
    // Field expectations come from original bank-$8F bytes via the import-only reader.
    private static readonly ushort[] OriginalRoomStatePointers =
    [
        0x9213, 0x922D, 0x9247, 0x9261, 0x92C5, 0x92DF, 0x9314, 0x932E, 0x9348, 0x93B7, 0x93E2, 0x940B,
        0x946E, 0x9499, 0x94D9, 0x950A, 0x955F, 0x958A, 0x95B5, 0x95E1, 0x960C, 0x9637, 0x9668, 0x969C,
        0x96D1, 0x96EB, 0x9705, 0x976D, 0x9787, 0x97C6, 0x97E0, 0x981B, 0x9835, 0x984F, 0x9890, 0x98AA,
        0x98C4, 0x98EF, 0x991A, 0x9945, 0x9976, 0x99A1, 0x99CA, 0x9A06, 0x9A56, 0x9A70, 0x9AA2, 0x9ABC,
        0x9AE6, 0x9B68, 0x9BAA, 0x9BD5, 0x9C14, 0x9C42, 0x9C6B, 0x9C96, 0x9CC0, 0x9D26, 0x9DA9, 0x9DD9,
        0x9DF3, 0x9E1E, 0x9E5F, 0x9EB1, 0x9ECB, 0x9F23, 0x9F3D, 0x9F76, 0x9F90, 0x9FC7, 0x9FF2, 0xA01E,
        0xA05E, 0xA088, 0xA0B1, 0xA0DF, 0xA114, 0xA13D, 0xA168, 0xA191, 0xA1BA, 0xA1E5, 0xA20E, 0xA237,
        0xA260, 0xA2A0, 0xA2DB, 0xA304, 0xA32F, 0xA389, 0xA3BB, 0xA3EA, 0xA415, 0xA454, 0xA47E, 0xA4BE,
        0xA4E7, 0xA533, 0xA54D, 0xA578, 0xA5B1, 0xA5CB, 0xA5FA, 0xA625, 0xA64E, 0xA677, 0xA6AE, 0xA6EF,
        0xA718, 0xA741, 0xA76A, 0xA795, 0xA7C0, 0xA7EB, 0xA822, 0xA872, 0xA89D, 0xA8C6, 0xA905, 0xA930,
        0xA99F, 0xA9B9, 0xA9F2, 0xAA1B, 0xAA4E, 0xAA8F, 0xAAC2, 0xAAEB, 0xAB14, 0xAB48, 0xAB71, 0xAB9C,
        0xABDF, 0xAC0D, 0xAC38, 0xAC67, 0xAC90, 0xACC0, 0xACFD, 0xAD28, 0xAD6B, 0xADBA, 0xADEB, 0xAE14,
        0xAE3F, 0xAE81, 0xAEC1, 0xAEEC, 0xAF21, 0xAF4C, 0xAF7F, 0xAFB0, 0xAFDB, 0xB008, 0xB033, 0xB05E,
        0xB087, 0xB0C1, 0xB0EA, 0xB113, 0xB146, 0xB174, 0xB19F, 0xB1C8, 0xB1F2, 0xB243, 0xB295, 0xB2AF,
        0xB2E7, 0xB312, 0xB340, 0xB35A, 0xB387, 0xB3B2, 0xB3EE, 0xB417, 0xB464, 0xB48F, 0xB4BA, 0xB4F2,
        0xB51D, 0xB567, 0xB592, 0xB5E2, 0xB638, 0xB663, 0xB6A5, 0xB6CE, 0xB6FB, 0xB74E, 0xC9A0, 0xC9BA,
        0xCA1A, 0xCA34, 0xCA64, 0xCA7E, 0xCAC0, 0xCADA, 0xCB08, 0xCB22, 0xCB9D, 0xCBB7, 0xCBE7, 0xCC01,
        0xCC39, 0xCC53, 0xCC81, 0xCC9B, 0xCCDD, 0xCCF7, 0xCD25, 0xCD3F, 0xCD6E, 0xCD88, 0xCDBA, 0xCDD4,
        0xCE03, 0xCE1D, 0xCE52, 0xCE6C, 0xCE9C, 0xCEB6, 0xCEDF, 0xCF0D, 0xCF27, 0xCF61, 0xCF8D, 0xCFD6,
        0xD024, 0xD062, 0xD097, 0xD0C6, 0xD111, 0xD148, 0xD17A, 0xD1B0, 0xD1EA, 0xD229, 0xD25F, 0xD28B,
        0xD2B7, 0xD2E6, 0xD318, 0xD34D, 0xD394, 0xD3C3, 0xD3EC, 0xD415, 0xD440, 0xD46E, 0xD49B, 0xD4CF,
        0xD4FC, 0xD52B, 0xD55A, 0xD587, 0xD5B4, 0xD5F9, 0xD624, 0xD653, 0xD6A7, 0xD6DD, 0xD70A, 0xD737,
        0xD772, 0xD7A1, 0xD7BB, 0xD7F1, 0xD827, 0xD852, 0xD87B, 0xD8A5, 0xD8D7, 0xD8F1, 0xD920, 0xD970,
        0xD98A, 0xD9B7, 0xD9E1, 0xDA0B, 0xDA38, 0xDA72, 0xDA8C, 0xDABB, 0xDAF3, 0xDB0D, 0xDB43, 0xDB5D,
        0xDB8F, 0xDBA9, 0xDBDF, 0xDBF9, 0xDC2B, 0xDC45, 0xDC77, 0xDC91, 0xDCC3, 0xDCDD, 0xDD0C, 0xDD3B,
        0xDD6E, 0xDD88, 0xDDA2, 0xDDD1, 0xDE00, 0xDE30, 0xDE5A, 0xDE87, 0xDEB4, 0xDEEB, 0xDF28, 0xDF57,
        0xDF71, 0xDF9F, 0xDFB9, 0xDFE9, 0xE003, 0xE033, 0xE04D, 0xE07D, 0xE097, 0xE0C7, 0xE0E1,
    ];

    private static void VerifyRoomStateSettings(SuperMetroidAddressSpace rom)
    {
        VerifyRoomStateIdentities();
        VerifyRoomStateCompressedLevelDataAddress(rom);
        VerifyRoomStateGraphicsSet(rom);
        VerifyRoomStateMusicDataIndex(rom);
        VerifyRoomStateMusicTrackIndex(rom);
        VerifyRoomStateFxPointer(rom);
        VerifyRoomStateEnemyPopulationPointer(rom);
        VerifyRoomStateEnemyTilesetPointer(rom);
        VerifyRoomStateLayer2ScrollX(rom);
        VerifyRoomStateLayer2ScrollY(rom);
        VerifyRoomStateScrollPointer(rom);
        VerifyRoomStateXrayPointer(rom);
        VerifyRoomStateMainCodePointer(rom);
        VerifyRoomStatePlmPointer(rom);
        VerifyRoomStateBackgroundDataPointer(rom);
        VerifyRoomStateSetupCodePointer(rom);
    }

    private static void VerifyRoomStateIdentities()
    {
        var original = OriginalRoomStatePointers.ToHashSet();
        AssertEqual(323, original.Count, "Original room-state identities");
        AssertTrue(RoomStateDefinitions.All.Select(state => state.Pointer).SequenceEqual(OriginalRoomStatePointers),
            "Room-state enumeration preserves the complete original sorted domain");
        for (int value = 0; value <= ushort.MaxValue; value++)
        {
            ushort pointer = (ushort)value;
            if (original.Contains(pointer))
                AssertEqual(pointer, RoomStateDefinitions.Get(pointer).Pointer, "Selected room-state identity");
            else
                AssertThrows<ArgumentOutOfRangeException>(() => RoomStateDefinitions.Get(pointer),
                    $"Unknown or interior state pointer {pointer:X4} is rejected");
        }
    }

    private static void VerifyRoomStateCompressedLevelDataAddress(SuperMetroidAddressSpace rom)
    {
        VerifyRoomStateField(rom, state => state.CompressedLevelDataAddress, "CompressedLevelDataAddress");
        int[] original = OriginalRoomStatePointers.Select(pointer => CartridgeRoomStateImporter.Load(rom, pointer).CompressedLevelDataAddress)
            .Distinct().Order().ToArray();
        AssertTrue(RoomVisualLayoutSourceDefinitions.All.SequenceEqual(original),
            "Required layout-source view matches every distinct original state source in order");
        AssertTrue(RoomVisualLayoutSourceDefinitions.All.SequenceEqual(original),
            "Layout-source view is repeatable after complete enumeration");
    }
    private static void VerifyRoomStateGraphicsSet(SuperMetroidAddressSpace rom) => VerifyRoomStateField(rom, state => state.GraphicsSet, "GraphicsSet");
    private static void VerifyRoomStateMusicDataIndex(SuperMetroidAddressSpace rom) => VerifyRoomStateField(rom, state => state.MusicDataIndex, "MusicDataIndex");
    private static void VerifyRoomStateMusicTrackIndex(SuperMetroidAddressSpace rom) => VerifyRoomStateField(rom, state => state.MusicTrackIndex, "MusicTrackIndex");
    private static void VerifyRoomStateFxPointer(SuperMetroidAddressSpace rom) => VerifyRoomStateField(rom, state => state.FxPointer, "FxPointer");
    private static void VerifyRoomStateEnemyPopulationPointer(SuperMetroidAddressSpace rom) => VerifyRoomStateField(rom, state => state.EnemyPopulationPointer, "EnemyPopulationPointer");
    private static void VerifyRoomStateEnemyTilesetPointer(SuperMetroidAddressSpace rom) => VerifyRoomStateField(rom, state => state.EnemyTilesetPointer, "EnemyTilesetPointer");
    private static void VerifyRoomStateLayer2ScrollX(SuperMetroidAddressSpace rom) => VerifyRoomStateField(rom, state => state.Layer2ScrollX, "Layer2ScrollX");
    private static void VerifyRoomStateLayer2ScrollY(SuperMetroidAddressSpace rom) => VerifyRoomStateField(rom, state => state.Layer2ScrollY, "Layer2ScrollY");
    private static void VerifyRoomStateScrollPointer(SuperMetroidAddressSpace rom) => VerifyRoomStateField(rom, state => state.ScrollPointer, "ScrollPointer");
    private static void VerifyRoomStateXrayPointer(SuperMetroidAddressSpace rom)
    {
        VerifyRoomStateField(rom, state => state.XrayPointer, "XrayPointer");
        ushort[] original = OriginalRoomStatePointers.Select(pointer => CartridgeRoomStateImporter.Load(rom, pointer).XrayPointer)
            .Where(pointer => pointer != 0).Distinct().Order().ToArray();
        AssertTrue(XrayRoomOverlaySourceDefinitions.All.SequenceEqual(original),
            "Required X-ray source view matches distinct original nonzero pointers in order");
        AssertTrue(XrayRoomOverlaySourceDefinitions.All.SequenceEqual(original),
            "X-ray source view is repeatable after complete enumeration");
    }
    private static void VerifyRoomStateMainCodePointer(SuperMetroidAddressSpace rom) => VerifyRoomStateField(rom, state => state.MainCodePointer, "MainCodePointer");
    private static void VerifyRoomStatePlmPointer(SuperMetroidAddressSpace rom) => VerifyRoomStateField(rom, state => state.PlmPointer, "PlmPointer");
    private static void VerifyRoomStateBackgroundDataPointer(SuperMetroidAddressSpace rom) => VerifyRoomStateField(rom, state => state.BackgroundDataPointer, "BackgroundDataPointer");
    private static void VerifyRoomStateSetupCodePointer(SuperMetroidAddressSpace rom) => VerifyRoomStateField(rom, state => state.SetupCodePointer, "SetupCodePointer");

    private static void VerifyRoomStateField(SuperMetroidAddressSpace rom,
        Func<CartridgeRoomState, int> field, string name)
    {
        CartridgeRoomState[] enumerated = RoomStateDefinitions.All.ToArray();
        for (int index = 0; index < OriginalRoomStatePointers.Length; index++)
        {
            ushort pointer = OriginalRoomStatePointers[index];
            CartridgeRoomState original = CartridgeRoomStateImporter.Load(rom, pointer);
            int expected = field(original);
            AssertEqual(expected, field(RoomStateDefinitions.Get(pointer)), $"State {pointer:X4} {name}");
            AssertEqual(expected, field(enumerated[index]), $"Enumerated state {pointer:X4} {name}");
        }
    }
}
