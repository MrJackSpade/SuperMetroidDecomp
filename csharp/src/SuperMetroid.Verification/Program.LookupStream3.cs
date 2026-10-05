using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyStream3GameOverText(ISnesAddressSpace rom)
    {
        byte[] imported = SuperMetroid.AssetExtraction.GameOverPresentationExtractor.Extract(rom);
        var document = System.Text.Json.JsonSerializer.Deserialize<GameOverPresentationDocument>(imported, MapPresentationFormat.JsonOptions)!;
        GameOverPresentation Load() => GameOverPresentation.Load(new MemoryStream(
            System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions)));
        var stock = Load();
        AssertTrue(typeof(GameOverPresentation).GetField("tilemap", System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic)!.GetValue(stock) is null, "game-over stock text retains no tilemap row");
        Confirm(stock);
        foreach (GameOverTextStream stream in GameOverRomData.Text.All)
        {
            int cell = stream.DestinationByteOffset / sizeof(ushort);
            document.Tilemap[cell] = document.Tilemap[cell] with { Palette = 1, FlipY = true };
        }
        document.Tilemap[0] = document.Tilemap[0] with { TileColumn = 1 };
        Confirm(Load());
        AssertThrows<ArgumentOutOfRangeException>(() => GameOverPresentationDefinitions.TilemapWord(-1), "game-over text negative cell");
        AssertThrows<ArgumentOutOfRangeException>(() => GameOverPresentationDefinitions.TilemapWord(1024), "game-over text final cell boundary");

        void Confirm(GameOverPresentation presentation)
        {
            var vram = new SnesVram();
            presentation.LoadTilemapTo(vram, 0x1000);
            for (int cell = 0; cell < document.Tilemap.Length; cell++)
            {
                MapPresentationCell source = document.Tilemap[cell];
                ushort expected = SnesBgTilemapWord.Create(source.TileRow * MapTileAtlasFormat.TileColumns + source.TileColumn,
                    source.Palette, source.Priority, (source.FlipX ? SnesTileFlipFlags.Horizontal : SnesTileFlipFlags.None) |
                    (source.FlipY ? SnesTileFlipFlags.Vertical : SnesTileFlipFlags.None)).Raw;
                AssertEqual(expected, vram.ReadWord(0x1000 + cell), "game-over native/edited text word and VRAM placement");
            }
        }
    }
    private static void VerifyStream3OptionsGeometry(ISnesAddressSpace rom)
    {
        ushort LookupWord(ISnesAddressSpace source, int address) => (ushort)(source.ReadByte(address) | source.ReadByte(address + 1) << 8);
        byte[] imported = SuperMetroid.AssetExtraction.GameOptionsPresentationExtractor.Extract(rom);
        GameOptionsPresentationDocument ReadDocument() => System.Text.Json.JsonSerializer.Deserialize<GameOptionsPresentationDocument>(
            imported, MapPresentationFormat.JsonOptions)!;
        GameOptionsPresentation Load(GameOptionsPresentationDocument document) => GameOptionsPresentation.Load(new MemoryStream(
            System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions)));
        var stock = ReadDocument();
        var loaded = Load(stock);
        foreach (string field in new[] { "controllerLabelAnchors", "languageRegions", "specialToggles", "headingAnchors", "cursorAnchors" })
            AssertTrue(typeof(GameOptionsPresentation).GetField(field, System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic)!.GetValue(loaded) is null, "stock options geometry calculates " + field);
        for (int action = 0; action < 7; action++)
        {
            ushort offset = LookupWord(rom, 0x82f639 + action * 2);
            AssertEqual(new MapLabelPoint(offset / 2 % 32, offset / 2 / 32),
                GameOptionsPresentationDefinitions.ControllerAnchor(action), "native options label anchor");
        }
        foreach ((string page, int address, int count) in new[]
        {
            (GameOptionsPresentationDefinitions.PrimaryMenu, 0x82f307, 5),
            (GameOptionsPresentationDefinitions.ControllerMenu, 0x82f31b, 9),
            (GameOptionsPresentationDefinitions.SpecialMenu, 0x82f33f, 3),
        })
            for (int row = 0; row < count; row++)
                AssertEqual(new MapLabelPoint(LookupWord(rom, address + row * 4), LookupWord(rom, address + row * 4 + 2)),
                    loaded.CursorPosition(page, row), "native options cursor anchor");
        var stockLabels = (Dictionary<string, ushort[]>)typeof(GameOptionsPresentation).GetField("controllerLabels",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(loaded)!;
        AssertEqual(0, stockLabels.Count, "options stock button glyphs retain no tilemap rows");
        for (int button = 0; button < 7; button++)
        for (int cell = 0; cell < 6; cell++)
            AssertEqual(LookupWord(rom, 0x82f659 + button * 12 + cell * 2),
                GameOptionsPresentationDefinitions.ControllerLabelWord(GameOptionsPresentationDefinitions.ControllerLabelName(button), cell),
                "native options glyph composition preserves each tile and flip");
        Confirm(stock, loaded);
        var edited = ReadDocument();
        for (int action = 0; action < edited.ControllerLabelAnchors.Length; action++)
        {
            MapLabelPoint old = edited.ControllerLabelAnchors[action];
            edited.ControllerLabelAnchors[action] = new(old.X + 1, old.Y);
        }
        foreach (MapPresentationCell[] label in edited.ControllerLabels.Values)
            for (int cell = 0; cell < label.Length; cell++)
                label[cell] = label[cell] with { FlipX = !label[cell].FlipX, Palette = 1 };
        edited.LanguageRegions[0].Cells[0] = 0;
        edited.SpecialToggles[GameOptionsPresentationDefinitions.IconCancelToggle].EnabledCells[0] = 0;
        foreach (string page in edited.HeadingAnchors.Keys.ToArray())
        {
            MapLabelPoint old = edited.HeadingAnchors[page];
            edited.HeadingAnchors[page] = new(old.X + 1, old.Y);
            for (int row = 0; row < edited.CursorAnchors[page].Length; row++)
            {
                old = edited.CursorAnchors[page][row];
                edited.CursorAnchors[page][row] = new(old.X + 1, old.Y);
            }
        }
        Confirm(edited, Load(edited));

        void Confirm(GameOptionsPresentationDocument document, GameOptionsPresentation presentation)
        {
            foreach (bool japanese in new[] { false, true })
            {
                byte[] actual = presentation.CreatePage(GameOptionsPresentationDefinitions.PrimaryPage);
                byte[] expected = (byte[])actual.Clone();
                foreach (var region in document.LanguageRegions)
                    Paint(expected, region.Cells, japanese == region.HighlightWhenJapanese ? document.SelectedPalette : document.UnselectedPalette);
                presentation.ApplyLanguage(actual, japanese);
                AssertTrue(actual.AsSpan().SequenceEqual(expected), "options language boxes preserve exact stock/edited cells");
            }
            foreach (var pair in document.SpecialToggles)
            foreach (bool enabled in new[] { false, true })
            {
                byte[] actual = presentation.CreatePage(GameOptionsPresentationDefinitions.SpecialEnglishPage);
                byte[] expected = (byte[])actual.Clone();
                Paint(expected, pair.Value.EnabledCells, enabled ? document.SelectedPalette : document.UnselectedPalette);
                Paint(expected, pair.Value.DisabledCells, enabled ? document.UnselectedPalette : document.SelectedPalette);
                presentation.ApplySpecialToggle(actual, pair.Key, enabled);
                AssertTrue(actual.AsSpan().SequenceEqual(expected), "options toggle boxes preserve exact stock/edited cells");
            }
            for (int action = 0; action < 7; action++)
            for (int button = 0; button < 7; button++)
            {
                byte[] actual = presentation.CreatePage(GameOptionsPresentationDefinitions.ControllerEnglishPage);
                byte[] expected = (byte[])actual.Clone();
                MapLabelPoint point = document.ControllerLabelAnchors[action];
                for (int cell = 0; cell < 6; cell++)
                {
                    MapPresentationCell source = document.ControllerLabels[GameOptionsPresentationDefinitions.ControllerLabelName(button)][cell];
                    ushort word = SnesBgTilemapWord.Create(source.TileRow * MapTileAtlasFormat.TileColumns + source.TileColumn, source.Palette, source.Priority,
                        (source.FlipX ? SnesTileFlipFlags.Horizontal : SnesTileFlipFlags.None) |
                        (source.FlipY ? SnesTileFlipFlags.Vertical : SnesTileFlipFlags.None)).Raw;
                    System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(expected.AsSpan(((point.Y + cell / 3) * 32 + point.X + cell % 3) * 2), word);
                }
                presentation.ApplyControllerLabel(actual, action, button);
                AssertTrue(actual.AsSpan().SequenceEqual(expected), "options controller boxes preserve exact stock/edited placement");
            }
            foreach (var pair in document.CursorAnchors)
                for (int row = 0; row < pair.Value.Length; row++)
                    AssertEqual(pair.Value[row], presentation.CursorPosition(pair.Key, row), "options stock/edited cursor");
            foreach (var pair in document.HeadingAnchors)
            {
                var actual = new OamBuffer();
                var expected = new OamBuffer();
                presentation.DrawHeading(actual, pair.Key, 3);
                MenuSpriteCompiler.Compile(document.Sprites[GameOptionsPresentationDefinitions.HeadingFrameName(pair.Key)], "heading oracle")
                    .DrawOnScreen(expected, checked((ushort)pair.Value.X), unchecked((ushort)(pair.Value.Y - 3)),
                        SnesObjAttributeWord.Create(0, document.CursorPalette, 0).PaletteBits);
                AssertTrue(actual.LowTable.SequenceEqual(expected.LowTable) && actual.HighTable.SequenceEqual(expected.HighTable),
                    "options stock/edited heading origin and scroll");
            }
            AssertThrows<ArgumentOutOfRangeException>(() => presentation.ApplyControllerLabel(new byte[2048], -1, 0), "options invalid action");
            AssertThrows<InvalidDataException>(() => presentation.CursorPosition("invalid", 0), "options invalid page");
        }
        static void Paint(byte[] page, int[] cells, int palette)
        {
            foreach (int cell in cells)
            {
                Span<byte> destination = page.AsSpan(cell * 2);
                ushort old = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(destination);
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(destination, (ushort)((old & ~0x1c00) | palette << 10));
            }
        }
    }
    private static void VerifyLookupStream3(ISnesAddressSpace rom)
    {
        VerifyStream3GameOverText(rom);
        VerifyStream3OptionsGeometry(rom);
        ushort[] expectedDoorCallbacks =
        [
            DoorCodes.DoorCode_Scroll6_Green,
            DoorCodes.DoorASM_Scroll_0_Blue,
            DoorCodes.DoorASM_Scroll_13_Blue,
            DoorCodes.DoorASM_Scroll_4_Red_8_Green,
            DoorCodes.DoorASM_Scroll_8_9_A_B_Red,
            DoorCodes.DoorASM_Scroll_2_3_4_5_B_C_D_11_Red,
            DoorCodes.DoorASM_Scroll_1_4_Green,
            DoorCodes.DoorASM_Scroll_2_Blue,
            DoorCodes.DoorASM_Scroll_17_Blue,
            DoorCodes.DoorASM_Scroll_4_Blue,
            DoorCodes.DoorASM_Scroll_6_Green_duplicate,
            DoorCodes.DoorASM_Scroll_3_Green,
            DoorCodes.DoorASM_Scroll_18_1C_Green,
            DoorCodes.DoorASM_Scroll_5_6_Blue,
            DoorCodes.DoorASM_Scroll_1D_Blue,
            DoorCodes.DoorASM_Scroll_2_3_Green,
            DoorCodes.DoorASM_Scroll_0_Red_1_Green,
            DoorCodes.DoorASM_Scroll_B_Green,
            DoorCodes.DoorASM_Scroll_Scroll_1C_Red_1D_Blue,
            DoorCodes.DoorASM_Scroll_4_Red,
            DoorCodes.DoorASM_Scroll_20_24_25_Green,
            DoorCodes.DoorASM_Scroll_2_Blue_duplicate,
            DoorCodes.DoorASM_Scroll_0_Green,
            DoorCodes.DoorASM_Scroll_6_7_Green,
            DoorCodes.DoorASM_Scroll_1_Blue_2_Red,
            DoorCodes.DoorASM_Scroll_1_Blue_3_Red,
            DoorCodes.DoorASM_Scroll_0_Red_4_Blue,
            DoorCodes.DoorASM_Scroll_2_3_Blue,
            DoorCodes.DoorASM_Scroll_0_1_Green,
            DoorCodes.DoorASM_Scroll_1_Green,
            DoorCodes.DoorASM_Scroll_F_12_Green,
            DoorCodes.DoorASM_Scroll_6_Green_duplicate_again,
            DoorCodes.DoorASM_Scroll_0_Green_1_Blue,
            DoorCodes.DoorASM_Scroll_2_Green,
            DoorCodes.DoorASM_Scroll_3_4_Red_6_7_8_Blue,
            DoorCodes.DoorASM_Scroll_1_2_3_Blue_4_Green_6_Red,
            DoorCodes.DoorASM_Scroll_0_1_Blue,
            DoorCodes.DoorASM_Scroll_0_Blue_1_Red,
            DoorCodes.DoorASM_Scroll_A_Green,
            DoorCodes.DoorASM_Scroll_0_2_Green,
            DoorCodes.DoorASM_Scroll_6_7_Blue_8_Red,
            DoorCodes.DoorASM_Scroll_2_Red_3_Blue,
            DoorCodes.DoorASM_Scroll_7_Green,
            DoorCodes.DoorASM_Scroll_1_Red_2_Blue,
            DoorCodes.DoorASM_Scroll_0_Blue_3_Red,
            DoorCodes.DoorASM_Scroll_1_Blue_4_Red,
            DoorCodes.DoorASM_Scroll_0_Blue_1_2_3_Red,
            DoorCodes.DoorASM_Scroll_0_Green_duplicate,
            DoorCodes.DoorASM_Scroll_0_1_Blue_4_Red,
            DoorCodes.DoorASM_Scroll_0_Blue_3_Red_duplicate,
            DoorCodes.DoorASM_Scroll_0_Blue_duplicate,
            DoorCodes.DoorASM_Scroll_0_Blue_1_Red_duplicate,
            DoorCodes.DoorASM_Scroll_18_Blue,
            DoorCodes.DoorASM_Scroll_2_Blue_3_Red,
            DoorCodes.DoorASM_Scroll_E_Red,
            DoorCodes.DoorASM_Scroll_1_Blue,
            DoorCodes.DoorASM_Scroll_0_Green_duplicate_again,
            DoorCodes.DoorASM_Scroll_3_Red_4_Blue,
            DoorCodes.DoorASM_Scroll_29_Blue,
            DoorCodes.DoorASM_Scroll_28_2E_Green,
            DoorCodes.DoorASM_Scroll_6_7_8_9_A_B_Red,
            DoorCodes.DoorASM_Scroll_A_Red_B_Blue,
            DoorCodes.DoorASM_Scroll_0_Red_4_Blue_duplicate,
            DoorCodes.DoorASM_Scroll_0_Red_1_Blue,
            DoorCodes.DoorASM_Scroll_9_Red_A_Blue,
            DoorCodes.DoorASM_Scroll_0_2_Red_1_Blue,
            DoorCodes.DoorASM_Scroll_1_Blue_duplicate,
            DoorCodes.DoorASM_Scroll_6_Blue,
            DoorCodes.DoorASM_Scroll_4_Red_duplicate,
            DoorCodes.DoorASM_Scroll_4_7_Red,
            DoorCodes.DoorASM_Scroll_1_Blue_2_Red_duplicate,
            DoorCodes.DoorASM_Scroll_0_2_Green_duplicate,
            DoorCodes.DoorASM_Scroll_0_1_Green_duplicate,
            DoorCodes.DoorASM_Scroll_18_Blue_19_Red,
        ];
        AssertTrue(expectedDoorCallbacks.SequenceEqual(SuperMetroid.Core.Rooms.DoorScrollPrograms.Pointers),
            "stream 3 pure door callback original registration order");
        var expectedCallbacks = expectedDoorCallbacks.ToHashSet();
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            AssertEqual(expectedCallbacks.Contains((ushort)pointer),
                SuperMetroid.Core.Rooms.DoorScrollPrograms.Contains((ushort)pointer), "stream 3 exact callback ownership");
        foreach (ushort pointer in expectedDoorCallbacks)
        {
            byte[] expected = Enumerable.Repeat((byte)0x7f, RoomScrollGrid.StorageByteCount).ToArray();
            var actual = RoomScrollGrid.LoadCompiled(new TestAddressSpace(), expected, 10, 5);
            ExecuteStream3DoorScroll(rom, pointer, expected);
            AssertTrue(SuperMetroid.Core.Rooms.DoorScrollPrograms.TryApply(pointer, actual), "stream 3 callback recognized");
            AssertTrue(actual.Storage.SequenceEqual(expected), "stream 3 native ordered door writes and all untouched cells");
        }
        var unchangedScroll = RoomScrollGrid.LoadCompiled(new TestAddressSpace(), new byte[50], 10, 5);
        AssertTrue(!SuperMetroid.Core.Rooms.DoorScrollPrograms.TryApply(0, unchangedScroll), "stream 3 unknown callback no-op");
        AssertThrows<ArgumentNullException>(() => SuperMetroid.Core.Rooms.DoorScrollPrograms.TryApply(0, null!),
            "stream 3 door callback null argument remains rejected before dispatch");
        for (int pair = 0; pair < 16; pair++)
        {
            int address = 0xadde5f + 2 * pair;
            int pointer = rom.ReadByte(address) | rom.ReadByte(address + 1) << 8;
            var expected = pointer switch
            {
                0xe1a6 => MotherBrainBeamRomData.Direction.Down,
                0xde7f => MotherBrainBeamRomData.Direction.Right,
                0xdf6e => MotherBrainBeamRomData.Direction.Up,
                0xde5e => MotherBrainBeamRomData.Direction.Retain,
                0 => MotherBrainBeamRomData.Direction.Unsupported,
                _ => throw new InvalidDataException("Unexpected native beam quadrant target."),
            };
            AssertEqual(expected, MotherBrainBeamRomData.DirectionForQuadrants(pair),
                "stream 3 native beam quadrant dispatcher including null and retaining entries");
        }
        foreach (int invalid in new[] { -1, 16, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => MotherBrainBeamRomData.DirectionForQuadrants(invalid),
                "stream 3 beam dispatcher preserves span index bounds");
        VerifyMotherBrainFallingTubeInstructionDefinitions();
        IntroCinematicRomData.Palette.Regions[] introRegions =
        [IntroCinematicRomData.Palette.Gameplay, IntroCinematicRomData.Palette.GameplayClear,
         IntroCinematicRomData.Palette.Narration, IntroCinematicRomData.Palette.Discovery];
        IntroPaletteSpan[][] expectedIntroRegions =
        [
            [new(0, 20), new(96, 16), new(466, 6)],
            [new(0, 16), new(96, 16), new(466, 6)],
            [new(40, 3), new(224, 16), new(384, 32), new(480, 16)],
            [new(64, 16), new(448, 9)],
        ];
        int[][] nativeIntroOperands =
        [ [0x8bb258, 0x8bb261, 0x8bb26a], [0x8bb3c8, 0x8bb3d1, 0x8bb3da],
          [0x8bb273, 0x8bb27c, 0x8bb285, 0x8bb28e], [0x8bb2f5, 0x8bb2fe] ];
        for (int group = 0; group < introRegions.Length; group++)
        {
            AssertTrue(introRegions[group].SequenceEqual(expectedIntroRegions[group]),
                "stream 3 intro scene region order, offsets and color counts");
            for (int index = 0; index < nativeIntroOperands[group].Length; index++)
            {
                int address = nativeIntroOperands[group][index];
                AssertEqual((byte)0xa2, rom.ReadByte(address), "stream 3 intro native LDX region");
                AssertEqual((byte)0xa0, rom.ReadByte(address + 3), "stream 3 intro native LDY color count");
                AssertEqual((int)introRegions[group][index].ByteOffset,
                    rom.ReadByte(address + 1) | rom.ReadByte(address + 2) << 8, "stream 3 intro native offset");
                AssertEqual((int)introRegions[group][index].ByteCount,
                    rom.ReadByte(address + 4) | rom.ReadByte(address + 5) << 8, "stream 3 intro native count");
            }
            foreach (int invalid in new[] { -1, introRegions[group].Count, int.MaxValue })
                AssertThrows<IndexOutOfRangeException>(() => _ = introRegions[group][invalid], "stream 3 intro region bounds");
        }
        ushort[] motherBrainRoots =
        [
            0xa586, 0xa5bf, 0xa5f8, 0xa62c, 0xa660, 0xa694, 0xa69b, 0xa6d9, 0xa717,
            0xa750, 0xa789, 0xad3e, 0xad6d, 0xada1, 0xadd5, 0xae09, 0xae33, 0xae5d,
        ];
        int[] nativeCharacterCounts = [11, 11, 10, 10, 10, 1, 12, 12, 11, 11, 11, 9, 10, 10, 10, 8, 8, 26];
        AssertTrue(MotherBrainVisualDefinitions.Frames().Select(frame => frame.Pointer).SequenceEqual(motherBrainRoots),
            "stream 3 all original Mother Brain root identities and enumeration");
        for (int index = 0; index < motherBrainRoots.Length; index++)
        {
            var actual = MotherBrainVisualDefinitions.Frame(index);
            AssertEqual((byte)0xa9, actual.Bank, "stream 3 Mother Brain visual bank");
            AssertEqual($"mother_brain_a9_{motherBrainRoots[index]:x4}", actual.Name, "stream 3 Mother Brain visual identity name");
            int address = 0xa90000 | actual.Pointer;
            AssertEqual(nativeCharacterCounts[index], rom.ReadByte(address) | rom.ReadByte(address + 1) << 8,
                "stream 3 native OAM record widths used for root strides");
        }
        foreach (int invalid in new[] { -1, 18, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => _ = MotherBrainVisualDefinitions.Frame(invalid), "stream 3 visual root bounds");
        MotherBrainSpecialSpriteSheetDefinition[] expectedSheets =
        [
            new("mother-brain-leg-tiles.png", 0xb79000, 8, 0x7400),
            new("mother-brain-baby-tiles.png", 0xb18800, 4, 0x7c00),
            new("mother-brain-attack-tiles.png", 0xb7a000, 4, 0x7c00),
            new("mother-brain-exploded-door-tiles.png", 0xabf400, 2, 0x7000),
        ];
        AssertTrue(expectedSheets.SequenceEqual(MotherBrainSpecialSpriteArtworkDefinitions.All),
            "stream 3 named special sheet order and every field");
        foreach (var sheet in expectedSheets)
            foreach (int source in new[] { sheet.SourceAddress, sheet.SourceAddress + sheet.ByteCount - 1 })
            {
                AssertTrue(MotherBrainSpecialSpriteArtworkDefinitions.TryForSource((uint)source, out var found),
                    "stream 3 special sheet boundary lookup");
                AssertEqual(sheet, found, "stream 3 special sheet source owner");
            }
        foreach (int invalid in new[] { -1, 4, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => _ = MotherBrainSpecialSpriteArtworkDefinitions.All[invalid],
                "stream 3 special sheet original index exception");
        // Confirm the identified copy-list conversion against native STA operands.
        int[][] nativeDoorCopies =
        [
            [0x82e1f4, 0x82e1fa, 0x82e200, 0x82e206, 0x82e20c, 0x82e212, 0x82e218, 0x82e21e],
            [0x82e22f, 0x82e235, 0x82e23b, 0x82e241, 0x82e247],
            [0x82e252, 0x82e258, 0x82e25e, 0x82e264],
        ];
        int targetPaletteBase = rom.ReadByte(0x82e1f5) + (rom.ReadByte(0x82e1f6) << 8) - 18;
        ushort[] sourceColors = Enumerable.Range(1, 256).Select(value => (ushort)value).ToArray();
        for (int group = 0; group < nativeDoorCopies.Length; group++)
        {
            ushort[] actual = new ushort[256];
            ushort[] expected = new ushort[256];
            foreach (int instruction in nativeDoorCopies[group])
            {
                AssertEqual((byte)0x8d, rom.ReadByte(instruction), "stream 3 native door color store opcode");
                int destination = rom.ReadByte(instruction + 1) + (rom.ReadByte(instruction + 2) << 8);
                int color = (destination - targetPaletteBase) / 2;
                expected[color] = sourceColors[color];
            }
            switch (group)
            {
                case 0: DoorTransitionPaletteDefinitions.PreserveHud(sourceColors, actual); break;
                case 1: DoorTransitionPaletteDefinitions.PreserveCommonCre(sourceColors, actual); break;
                case 2: DoorTransitionPaletteDefinitions.PreserveEscapeTimer(sourceColors, actual); break;
            }
            AssertTrue(expected.SequenceEqual(actual), "stream 3 exact native door fade preserved and black slots");
        }
        // Confirm the replaced selector for its complete ushort input domain.
        for (int angle = 0; angle <= ushort.MaxValue; angle++)
        {
            int address = 0x9bc346 + 2 * (angle >> 10);
            int expected = 0x9a0000 | rom.ReadByte(address) | rom.ReadByte(address + 1) << 8;
            var asset = GrappleTileDefinitions.SegmentAssetFor((ushort)angle);
            AssertEqual(expected, GrappleTileDefinitions.TransferFor(asset).SourceAddress,
                "stream 3 native grapple angle sector");
        }

        GrappleTileTransfer[] expectedTransfers =
        [
            new(VramAssetId.GrapplePointFirstTiles, 0x9a8200, 0, 32),
            new(VramAssetId.GrapplePointSecondTiles, 0x9a8400, 32, 32),
            new(VramAssetId.GrapplePointThirdTiles, 0x9a8600, 64, 32),
            new(VramAssetId.GrapplePointFourthTiles, 0x9a8800, 96, 32),
            new(VramAssetId.GrappleHorizontalSegmentTiles, 0x9a8220, 128, 128),
            new(VramAssetId.GrappleDiagonalSegmentTiles, 0x9a8a20, 256, 128),
            new(VramAssetId.GrappleVerticalSegmentTiles, 0x9a9220, 384, 128),
        ];
        var actualTransfers = GrappleTileDefinitions.Transfers;
        AssertTrue(expectedTransfers.SequenceEqual(actualTransfers),
            "stream 3 grapple transfer enumeration and every field");
        for (int index = 0; index < expectedTransfers.Length; index++)
        {
            AssertEqual(expectedTransfers[index], actualTransfers[index],
                "stream 3 grapple transfer index");
            AssertEqual(expectedTransfers[index], GrappleTileDefinitions.TransferFor(expectedTransfers[index].Asset),
                "stream 3 grapple transfer asset dispatch");
        }
        foreach (int invalid in new[] { -1, 7, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => _ = actualTransfers[invalid],
                "stream 3 grapple transfer bounds");
        AssertThrows<InvalidDataException>(() => GrappleTileDefinitions.TransferFor((VramAssetId)(-1)),
            "stream 3 invalid grapple asset");

        for (int parameter = 0; parameter <= ushort.MaxValue; parameter++)
        {
            int offset = 2 * (parameter & 3);
            ushort list = (ushort)(rom.ReadByte(0xa8e682 + offset) | rom.ReadByte(0xa8e683 + offset) << 8);
            var function = (SparkEnemyFunction)(rom.ReadByte(0xa8e688 + offset) | rom.ReadByte(0xa8e689 + offset) << 8);
            AssertEqual(new SparkMovementDefinition(list, function),
                SparkMovementDefinitions.InitialState((ushort)parameter),
                "stream 3 native Spark selector including adjacent-word case");
        }
        for (int angle = 0; angle <= byte.MaxValue; angle++)
        {
            int offset = 2 * (angle >> 5);
            short x = (short)(rom.ReadByte(0x86bde3 + offset) | rom.ReadByte(0x86bde4 + offset) << 8);
            short y = (short)(rom.ReadByte(0x86bdf3 + offset) | rom.ReadByte(0x86bdf4 + offset) << 8);
            AssertEqual((x, y), ShaktoolProjectilePlacementDefinitions.Offset((byte)angle),
                "stream 3 native Shaktool circle offset");
        }
        for (int bucket = 0; bucket <= ushort.MaxValue; bucket++)
        {
            ushort direction = (ushort)bucket;
            if ((bucket & 31) != 0 || bucket > 224)
                AssertThrows<InvalidDataException>(() => ShaktoolInstructionDefinitions.ForOrientationBucket(direction),
                    "stream 3 invalid Shaktool orientation");
            else
            {
                int address = 0xaadd15 + 2 * (bucket >> 5);
                ushort expected = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
                AssertEqual(expected, ShaktoolInstructionDefinitions.ForOrientationBucket(direction),
                    "stream 3 native Shaktool orientation");
            }
        }
        for (int segment = 0; segment < 7; segment++)
        {
            int offset = segment * 2;
            ushort collision = (ushort)(rom.ReadByte(0xaadf13 + offset) | rom.ReadByte(0xaadf14 + offset) << 8);
            ushort attack = (ushort)(rom.ReadByte(0xaadf21 + offset) | rom.ReadByte(0xaadf22 + offset) << 8);
            AssertEqual(collision, ShaktoolInstructionDefinitions.CollisionForSegment(segment),
                "stream 3 native Shaktool collision program");
            AssertEqual(attack, ShaktoolInstructionDefinitions.AttackForSegment(segment),
                "stream 3 native Shaktool dormant attack program");
        }
        foreach (int invalid in new[] { -1, 7, int.MinValue, int.MaxValue })
        {
            AssertThrows<InvalidDataException>(() => ShaktoolInstructionDefinitions.CollisionForSegment(invalid),
                "stream 3 invalid Shaktool collision segment");
            AssertThrows<InvalidDataException>(() => ShaktoolInstructionDefinitions.AttackForSegment(invalid),
                "stream 3 invalid Shaktool attack segment");
        }
        VerifyStream3WorkRobotColors(rom);
        VerifyStream3PickupAndFirefleaPrograms(rom);
        VerifyStream3ChootControl(rom);
        VerifyStream3RipperMappings(rom);
        VerifyStream3UniformEnemyLoops(rom);
        VerifyStream3MotherBrainFades(rom);
        VerifyStream3BabyFade(rom);
        VerifyStream3DrainFades(rom);
        VerifyStream3ShitroidPulse(rom);
        VerifyStream3HealthTint(rom);
        VerifyStream3RecoveryLights(rom);
        VerifyStream3RoomFlash(rom);
        VerifyMotherBrainRoomPaletteProgramDefinitions();
        VerifyStream3CorpseGeometry(rom);
        VerifyStream3EscapeGeometry(rom);
        VerifyStream3PainfulWalking(rom);
        VerifyStream3DeathSelectors(rom);
        VerifyMotherBrainContactHitboxes();
        byte[] grappleSpriteJson = SuperMetroid.AssetExtraction.GrappleSpriteExtractor.Extract(rom);
        var grappleSprites = GrappleSpriteCatalog.Load(new MemoryStream(grappleSpriteJson));
        AssertTrue(typeof(GrappleSpriteCatalog).GetField("segments", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .GetValue(grappleSprites) is null, "stream 3 stock rope attribute rows discarded");
        AssertTrue(GrappleSpriteDefinitions.SegmentAttributeAddresses.SequenceEqual(new[] { 0x94b18d, 0x94b191, 0x94b195, 0x94b199 }),
            "stream 3 segment address enumeration");
        for (int frame = 0; frame < 4; frame++)
        {
            int address = 0x94b18d + 4 * frame;
            AssertEqual((ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8), grappleSprites.Segment(frame), "stream 3 calculated native rope attributes");
            foreach (string field in new[] { "tileColumn", "tileRow", "palette", "priority", "flipX", "flipY" })
            {
                var node = System.Text.Json.Nodes.JsonNode.Parse(grappleSpriteJson)!;
                var style = node["segments"]![frame]!;
                int bit = field switch { "tileColumn" => 1, "tileRow" => 16, "palette" => 0x200, "priority" => 0x1000, "flipX" => 0x4000, _ => 0x8000 };
                if (field is "flipX" or "flipY") style[field] = !style[field]!.GetValue<bool>();
                else style[field] = style[field]!.GetValue<int>() ^ 1;
                var edited = GrappleSpriteCatalog.Load(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(node.ToJsonString())));
                for (int check = 0; check < 4; check++)
                    AssertEqual((ushort)(grappleSprites.Segment(check) ^ (check == frame ? bit : 0)), edited.Segment(check), "stream 3 independent rope attribute edit");
            }
        }
        foreach (int invalid in new[] { -1, 4, int.MaxValue })
        {
            AssertThrows<IndexOutOfRangeException>(() => _ = GrappleSpriteDefinitions.SegmentAttributeAddresses[invalid], "stream 3 rope operand address bounds");
            AssertThrows<InvalidDataException>(() => grappleSprites.Segment(invalid), "stream 3 rope attribute bounds");
        }
        var swingFrames = GrappleSwingFrameCatalog.Load(new MemoryStream(
            SuperMetroid.AssetExtraction.GrappleSwingFrameExtractor.Extract(rom)));
        for (int angle = 0; angle < 256; angle++)
            AssertEqual(rom.ReadByte(0x9bc1c2 + angle), swingFrames.Resolve((byte)angle), "stream 3 native calculated swing art frame");
        AssertTrue(typeof(GrappleSwingFrameCatalog).GetField("frames", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .GetValue(swingFrames) is null, "stream 3 stock swing frame lookup discarded");
        int[] editedFrames = Enumerable.Range(0, 256).Select(angle => (int)rom.ReadByte(0x9bc1c2 + angle)).ToArray();
        for (int editedAngle = 0; editedAngle < 256; editedAngle++)
        {
            int original = editedFrames[editedAngle];
            editedFrames[editedAngle] = (original + 1) & 31;
            var edited = GrappleSwingFrameCatalog.Load(new MemoryStream(GrappleSwingFrameCatalog.Write(new()
            { Version = 1, Frames = editedFrames })));
            for (int angle = 0; angle < 256; angle++)
                AssertEqual((byte)editedFrames[angle], edited.Resolve((byte)angle), "stream 3 independently edited swing art frame");
            editedFrames[editedAngle] = original;
        }
        byte[] fileSelectJson = SuperMetroid.AssetExtraction.FileSelectPresentationExtractor.Extract(rom);
        var fileSelect = FileSelectPresentation.Load(new MemoryStream(fileSelectJson));
        foreach (string field in new[] { "digits", "slotLetters" })
            AssertTrue(typeof(FileSelectPresentation).GetField(field, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .GetValue(fileSelect) is null, "stream 3 stock file-select glyph sequence discarded");
        var glyphTilemap = new ushort[1024];
        for (int glyph = 0; glyph < 13; glyph++)
        {
            bool digit = glyph < 10;
            int index = digit ? glyph : glyph - 10;
            ushort expected = (ushort)(digit ? 0x2060 + index : 0x206a + index);
            var anchor = new MapLabelPoint(4, 3);
            if (digit) fileSelect.WriteDigit(glyphTilemap, anchor, 2, index);
            else fileSelect.WriteSlotLetter(glyphTilemap, anchor, index);
            AssertEqual(expected, glyphTilemap[3 * 32 + 4 + (digit ? 2 : 0)], "stream 3 actual file-select calculated glyph write");
            foreach (string field in new[] { "tileColumn", "tileRow", "palette", "priority", "flipX", "flipY" })
            {
                var node = System.Text.Json.Nodes.JsonNode.Parse(fileSelectJson)!;
                var cell = node[digit ? "digits" : "slotLetters"]![index]!;
                int bit = field switch { "tileColumn" => 1, "tileRow" => 32, "palette" => 0x400, "priority" => 0x2000, "flipX" => 0x4000, _ => 0x8000 };
                if (field is "priority" or "flipX" or "flipY") cell[field] = !cell[field]!.GetValue<bool>();
                else cell[field] = cell[field]!.GetValue<int>() ^ 1;
                var edited = FileSelectPresentation.Load(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(node.ToJsonString())));
                if (digit) edited.WriteDigit(glyphTilemap, anchor, 2, index);
                else edited.WriteSlotLetter(glyphTilemap, anchor, index);
                AssertEqual((ushort)(expected ^ bit), glyphTilemap[3 * 32 + 4 + (digit ? 2 : 0)], "stream 3 independent file-select glyph attribute edit");
            }
        }
        string[] expectedCreditRoles = ["staff-heading", "producer-heading", "producer-name", "director-heading", "director-name", "background-designers-heading", "background-designer-1", "background-designer-2", "background-designer-3", "object-designers-heading", "object-designer-1", "object-designer-2", "samus-original-designer-heading", "samus-original-designer-name", "samus-designer-heading", "samus-designer-name", "sound-program-heading", "sound-effects-heading", "sound-programmer-name", "music-composers-heading", "music-composer-1", "music-composer-2", "program-director-heading", "program-director-name", "system-coordinator-heading", "system-coordinator-name", "system-programmer-heading", "system-programmer-name", "samus-programmer-heading", "samus-programmer-name", "event-programmer-heading", "event-programmer-name", "enemy-programmer-heading", "enemy-programmer-name", "map-programmer-heading", "map-programmer-name", "assistant-programmer-heading", "assistant-programmer-name", "coordinators-heading", "coordinator-1", "coordinator-2", "printed-art-work-heading", "printed-art-work-1", "printed-art-work-2", "printed-art-work-3", "printed-art-work-4", "printed-art-work-5", "printed-art-work-6", "special-thanks-heading", "special-thanks-01", "special-thanks-02", "special-thanks-03", "special-thanks-04", "special-thanks-05", "special-thanks-06", "special-thanks-07", "special-thanks-08", "special-thanks-09", "special-thanks-10", "special-thanks-11", "special-thanks-12", "special-thanks-13", "special-thanks-14", "special-thanks-15", "special-thanks-r-and-d", "general-manager-heading", "general-manager-name"];
        AssertTrue(CreditsPresentationDefinitions.Lines.Select(line => line.Id).SequenceEqual(expectedCreditRoles), "stream 3 all credit role identities and order");
        VerifyCreditsPresentation(Path.GetFullPath("Super Metroid.smc"));
        byte[] creditsJson = SuperMetroid.AssetExtraction.CreditsPresentationExtractor.Extract(rom);
        var credits = CreditsPresentation.Load(new MemoryStream(creditsJson));
        AssertTrue(typeof(CreditsPresentation).GetField("fixtureRows", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .GetValue(credits) is null, "stream 3 credits stock has no cached row storage");
        AssertEqual(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(creditsJson)), credits.ContentIdentity, "stream 3 credits source identity preserved");
        foreach (int invalid in new[] { -1, credits.RowCount, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => _ = credits.GetRow(invalid).Length, "stream 3 calculated credits row bounds");
        VerifyCrateriaLightningPaletteFxProgramMechanicsDefinitions((SuperMetroid.AssetExtraction.CartridgeImportAddressSpace)rom);
        foreach (var lightning in CrateriaLightningPaletteFxProgramMechanicsDefinitions.All)
        {
            foreach (int invalid in new[] { -1, lightning.Frames.Count, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => _ = lightning.Frames[invalid], "stream 3 lightning frame bounds");
            foreach (int invalid in new[] { -1, lightning.MechanicsWords.Count, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => _ = lightning.MechanicsWords[invalid], "stream 3 lightning mechanics bounds");
            foreach (int invalid in new[] { -1, 2, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => _ = lightning.MechanicsBytes[invalid], "stream 3 lightning timer bounds");
        }
        VerifyCrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions((SuperMetroid.AssetExtraction.CartridgeImportAddressSpace)rom);
        var escapePrograms = CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions.All;
        AssertEqual(2, escapePrograms.Count, "stream 3 escape lightning owner count");
        AssertTrue(escapePrograms.Select(program => program.Owner).SequenceEqual(new[] { CrateriaEscapeLightningPaletteOwner.YellowLightning, CrateriaEscapeLightningPaletteOwner.CreBlockPixel }), "stream 3 escape lightning owner order");
        foreach (int invalid in new[] { -1, 2, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => _ = escapePrograms[invalid], "stream 3 escape lightning owner bounds");
        foreach (int invalid in new[] { -1, 11, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions.Duration(invalid), "stream 3 escape lightning duration bounds");
        VerifyGrappleConnectionDefinitions((SuperMetroidAddressSpace)rom);
        VerifyWorkRobotLaserInstructionProgramDefinitions((SuperMetroidAddressSpace)rom);
        VerifyMotherBrainTurretDefinitions((SuperMetroidAddressSpace)rom);
        VerifyMotherBrainTurretInstructionProgramDefinitions((SuperMetroidAddressSpace)rom);
        var turretMechanics = new HashSet<int>();
        for (int direction = 0; direction < 8; direction++)
        {
            turretMechanics.Add(0xc101 + 6 * direction);
            turretMechanics.Add(0xc105 + 6 * direction);
            turretMechanics.Add(0xc143 + 6 * direction);
            turretMechanics.Add(0xc147 + 6 * direction);
        }
        for (int selector = 0; selector < 9; selector++)
            turretMechanics.Add(0xc131 + 2 * selector);
        foreach (int address in new[] { 0xc19a, 0xc19c, 0xc19e, 0xc1a2, 0xc1a6, 0xc1aa, 0xc1ae, 0xc1b2 })
            turretMechanics.Add(address);
        foreach (int address in turretMechanics)
            AssertEqual((ushort)(rom.ReadByte(0x860000 | address) | rom.ReadByte(0x860000 | (address + 1)) << 8),
                MotherBrainTurretInstructionProgramDefinitions.ReadMechanicsWord((ushort)address), "stream 3 turret calculated word read");
        for (int address = 0; address <= ushort.MaxValue; address++)
            AssertEqual(turretMechanics.Contains(address) || turretMechanics.Contains(address - 1),
                MotherBrainTurretInstructionProgramDefinitions.IsCompiledMechanicsByte(0x860000 | address),
                "stream 3 turret mechanics byte ownership domain");
        foreach (int invalid in new[] { -1, 49, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => MotherBrainTurretInstructionProgramDefinitions.MechanicsWord(invalid), "stream 3 turret mechanics index bounds");
        foreach (int invalid in new[] { -1, 21, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => MotherBrainTurretInstructionProgramDefinitions.PresentationWordAddress(invalid), "stream 3 turret visual index bounds");
        foreach (MotherBrainContactPart part in Enum.GetValues<MotherBrainContactPart>())
        {
            var regions = MotherBrainContactHitboxDefinitions.Get(part);
            foreach (int invalid in new[] { -1, regions.Count, int.MaxValue })
                AssertThrows<IndexOutOfRangeException>(() => _ = regions[invalid], "stream 3 contact region bounds");
        }
        Console.WriteLine("Lookup stream 3: all implemented mapping conversions match their original values and accepted domains.");
    }

    private static void VerifyStream3DeathSelectors(ISnesAddressSpace rom)
    {
        ushort Read(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (ushort parameter = 0; parameter < 3; parameter++)
            AssertEqual(Read(0x86c929 + 2 * parameter), MotherBrainDeathExplosionDefinitions.InstructionList(parameter),
                "stream 3 native death explosion variant");
        foreach (ushort invalid in new ushort[] { 3, 4, ushort.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => MotherBrainDeathExplosionDefinitions.InstructionList(invalid),
                "stream 3 death explosion selector bounds");
        // Confirm the retained visual payload's catalog move and the independent group arithmetic.
        for (int index = 0; index < 28; index++)
            AssertEqual(((short)Read(0xa9b099 + 4 * index), (short)Read(0xa9b09b + 4 * index)),
                MotherBrainDeathExplosionDefinitions.Anchor(index), "stream 3 native decorative death anchor");
        var generate = typeof(MotherBrainRainbowBeamAttackSequence).GetMethod("GenerateDeathExplosions",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        foreach (bool mixed in new[] { false, true })
        {
            var scatter = new MotherBrainRainbowBeamAttackSequence();
            scatter.Body.XPosition = 320;
            scatter.Body.YPosition = 192;
            for (int burst = 0; burst < 8; burst++)
            {
                typeof(MotherBrainRainbowBeamAttackSequence).GetProperty("DeathExplosionIntervalTimer")!.SetValue(scatter, (ushort)0);
                int calls = 0;
                Func<ushort> random = () => { calls++; return ushort.MaxValue; };
                var requests = new List<MotherBrainDeathExplosionRequest>();
                generate.Invoke(scatter, [mixed, random, requests]);
                int group = 6 - burst % 7;
                AssertEqual(mixed ? 4 : 2, requests.Count, "stream 3 death scatter burst size");
                AssertEqual(requests.Count, calls, "stream 3 one RNG call per death projectile");
                for (int item = 0; item < requests.Count; item++)
                {
                    short x = (short)Read(0xa9b099 + 16 * group + 4 * item);
                    short y = (short)Read(0xa9b09b + 16 * group + 4 * item);
                    AssertEqual((ushort)group, requests[item].PatternIndex, "stream 3 death scatter reverse group order");
                    AssertEqual((x, y), (requests[item].XOffset, requests[item].YOffset), "stream 3 selected death scatter anchors");
                    AssertEqual((ushort)(320 + x), requests[item].XPosition, "stream 3 death scatter body-relative X");
                    AssertEqual((ushort)(192 + y), requests[item].YPosition, "stream 3 death scatter body-relative Y");
                    AssertEqual(mixed ? (ushort)2 : (ushort)1, requests[item].ProjectileParameter, "stream 3 independent death type case");
                }
            }
        }
        var sequence = new MotherBrainRainbowBeamAttackSequence();
        typeof(MotherBrainRainbowBeamAttackSequence).GetProperty("Phase")!.SetValue(sequence,
            MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceStartEscape);
        var bus = new TestAddressSpace();
        var samus = new SamusState { Health = 99 };
        var firstPage = sequence.Step(bus, samus, 0, 0);
        AssertEqual(0, firstPage.EscapePaletteFxRequests.Count, "stream 3 escape palette effects wait for door graphics");
        var handoff = sequence.Step(bus, samus, 0, 0);
        AssertEqual(4, handoff.EscapePaletteFxRequests.Count, "stream 3 four escape palette registrations");
        for (int effect = 0; effect < 4; effect++)
            AssertEqual(Read(0xa9b296 + 7 * effect), handoff.EscapePaletteFxRequests[effect],
                "stream 3 native escape palette registration order");
        var text = sequence.Step(bus, samus, 0, 0);
        AssertEqual(0, text.EscapePaletteFxRequests.Count, "stream 3 escape palette registrations occur once");
    }

    private static void VerifyStream3PainfulWalking(ISnesAddressSpace rom)
    {
        ushort Read(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (int stage = 0; stage < 8; stage++)
        {
            AssertEqual(Read(0xa9beee + 2 * stage), MotherBrainPainfulWalkingDefinitions.AnimationDelay(stage), "stream 3 native stagger animation delay");
            AssertEqual(Read(0xa9befe + 2 * stage), MotherBrainPainfulWalkingDefinitions.NeckAngleDelta(stage), "stream 3 native stagger neck angle delta");
            AssertEqual(Read(0xa9c049 + 2 * stage), MotherBrainPainfulWalkingDefinitions.FunctionTimer(stage), "stream 3 native stagger pause timer");
        }
        foreach (int invalid in new[] { -1, 8, int.MaxValue })
        {
            AssertThrows<IndexOutOfRangeException>(() => MotherBrainPainfulWalkingDefinitions.AnimationDelay(invalid), "stream 3 stagger delay bounds");
            AssertThrows<IndexOutOfRangeException>(() => MotherBrainPainfulWalkingDefinitions.NeckAngleDelta(invalid), "stream 3 stagger neck bounds");
            AssertThrows<IndexOutOfRangeException>(() => MotherBrainPainfulWalkingDefinitions.FunctionTimer(invalid), "stream 3 stagger timer bounds");
        }
        var step = typeof(MotherBrainRainbowBeamAttackSequence).GetMethod("StepPainfulWalking",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        foreach (ushort stage in new ushort[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, ushort.MaxValue })
        {
            var sequence = new MotherBrainRainbowBeamAttackSequence();
            sequence.Body.XPosition = 0;
            typeof(MotherBrainRainbowBeamAttackSequence).GetProperty("PainfulWalkingStage")!.SetValue(sequence, stage);
            step.Invoke(sequence, null);
            AssertEqual(Read(0xa9c049 + 2 * Math.Min(stage, (ushort)7)), sequence.PainfulWalkingFunctionTimer,
                "stream 3 real stagger timer clamps terminal stages");
        }
    }

    private static void VerifyStream3EscapeGeometry(ISnesAddressSpace rom)
    {
        ushort Read(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var sequence = new MotherBrainRainbowBeamAttackSequence();
        const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        var timer = typeof(MotherBrainRainbowBeamAttackSequence).GetMethod("CreateNextEscapeTimerTileTransfer", flags)!;
        var door = typeof(MotherBrainRainbowBeamAttackSequence).GetMethod("CreateNextExplodedDoorTileTransfer", flags)!;
        foreach (bool timerList in new[] { true, false })
        {
            int count = timerList ? 7 : 2;
            int start = timerList ? 0xa6c4cb : 0xa9902f;
            var method = timerList ? timer : door;
            for (int index = 0; index < count; index++)
            {
                int address = start + index * 7;
                var expected = new MotherBrainSpriteTileTransferRequest((ushort)index, Read(address),
                    (uint)(Read(address + 2) | rom.ReadByte(address + 4) << 16), Read(address + 5));
                var calculated = timerList ? MotherBrainEscapeTextArtworkDefinitions.Transfer(index)
                    : MotherBrainSpecialSpriteArtworkDefinitions.ExplodedDoor.Transfer(index);
                AssertEqual(expected, calculated, "stream 3 native escape graphics record");
                AssertEqual(expected, (MotherBrainSpriteTileTransferRequest)method.Invoke(sequence, null)!,
                    "stream 3 production escape transfer selection");
                AssertEqual((ushort)(index + 1), timerList ? sequence.EscapeTimerTileTransferIndex : sequence.ExplodedDoorTileTransferIndex,
                    "stream 3 production escape cursor advances once");
            }
            try
            {
                method.Invoke(sequence, null);
                throw new InvalidDataException("Completed escape transfer list unexpectedly produced another entry.");
            }
            catch (System.Reflection.TargetInvocationException error) when (error.InnerException is InvalidOperationException) { }
        }
        foreach (int invalid in new[] { -1, 5, int.MaxValue })
        {
            AssertThrows<IndexOutOfRangeException>(() => MotherBrainEscapeTextArtworkDefinitions.PageSource(invalid), "stream 3 text page source bounds");
            AssertThrows<IndexOutOfRangeException>(() => MotherBrainEscapeTextArtworkDefinitions.PageDestination(invalid), "stream 3 text page destination bounds");
            AssertThrows<IndexOutOfRangeException>(() => MotherBrainEscapeTextArtworkDefinitions.PageByteCount(invalid), "stream 3 text page size bounds");
        }
        foreach (int invalid in new[] { -1, 7, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => MotherBrainEscapeTextArtworkDefinitions.Transfer(invalid), "stream 3 escape transfer bounds");
        foreach (int invalid in new[] { -1, 2, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => MotherBrainSpecialSpriteArtworkDefinitions.ExplodedDoor.Transfer(invalid), "stream 3 door transfer bounds");
        using var temporary = new MapCatalogTestDirectory();
        SuperMetroid.AssetExtraction.EnemyTileArtworkFiles.Extract(rom, temporary.Root, SuperMetroid.AssetExtraction.SupportedCartridge.Sha256);
        VerifyInstalledMotherBrainEscapeTextArtwork(temporary.Root,
            SuperMetroid.AssetExtraction.EnemyTileArtworkFiles.Load(temporary.Root, null));
    }

    private static void VerifyStream3CorpseGeometry(ISnesAddressSpace rom)
    {
        ushort Read(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (int row = 0; row < 8; row++)
            AssertEqual((int)Read(0xa9e262 + row * 2), MotherBrainCorpseArtworkDefinitions.TileRowOffset(row),
                "stream 3 corpse native tile row offset");
        var transfers = MotherBrainCorpseArtworkDefinitions.RotTransfers;
        AssertEqual(6, transfers.Count, "stream 3 corpse transfer count");
        var enumerated = transfers.ToArray();
        for (int row = 0; row < 6; row++)
        {
            int entry = 0xa9e1f4 + row * 8;
            var expected = new MotherBrainSpriteTileTransferRequest((ushort)row, Read(entry),
                (uint)(Read(entry + 4) | (Read(entry + 2) >> 8) << 16), Read(entry + 6));
            AssertEqual(expected, transfers[row], "stream 3 native corpse rot transfer");
            AssertEqual(expected, enumerated[row], "stream 3 corpse transfer enumeration order");
            int pageEntry = 0xa99003 + row * 7;
            uint source = (uint)(Read(pageEntry + 2) | rom.ReadByte(pageEntry + 4) << 16);
            AssertEqual(source, MotherBrainCorpseArtworkDefinitions.VramPageSource(row), "stream 3 corpse source page");
            AssertEqual(Read(pageEntry + 5), MotherBrainCorpseArtworkDefinitions.VramPageDestination(row), "stream 3 corpse destination page");
        }
        int[] minimumY = [16, 8, 0, 0, 0, 8, 32]; // CMP/BCC gates in native $A9:EA40/$EB0B.
        for (int column = 0; column < 7; column++)
            AssertEqual(minimumY[column], MotherBrainCorpseArtworkDefinitions.ColumnMinimumY(column), "stream 3 corpse native outline gate");
        var copy = typeof(MotherBrainCorpseRottingState).GetMethod("CopyOrMovePixelRow",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!;
        // Confirm the changed row/column geometry through the real pixel-copy path.
        for (ushort y = 0; y < 48; y++)
        foreach (bool move in new[] { false, true })
        {
            var actual = new TestAddressSpace();
            var expected = new TestAddressSpace();
            for (int offset = 0; offset < 0x600; offset++)
            {
                byte value = (byte)(offset * 37 + 11);
                actual.WriteByte(0x7e9000 + offset, value);
                expected.WriteByte(0x7e9000 + offset, value);
            }
            int source = Read(0xa9e262 + (y >> 3) * 2) + (y & 7) * 2;
            int destination = source + ((y & 7) < 6 ? 0 : 0xd4);
            for (int column = 0; column < 7; column++)
            {
                if (y < minimumY[column]) continue;
                int start = 0x7e9000 + column * 32;
                if (y < 46)
                {
                    // Read both plane words before copying or clearing either source.
                    byte lo = expected.ReadByte(start + source);
                    byte hi = expected.ReadByte(start + source + 1);
                    byte lo2 = expected.ReadByte(start + source + 16);
                    byte hi2 = expected.ReadByte(start + source + 17);
                    expected.WriteByte(start + destination + 2, lo);
                    expected.WriteByte(start + destination + 3, hi);
                    expected.WriteByte(start + destination + 18, lo2);
                    expected.WriteByte(start + destination + 19, hi2);
                }
                if (move)
                {
                    expected.WriteByte(start + source, 0);
                    expected.WriteByte(start + source + 1, 0);
                    expected.WriteByte(start + source + 16, 0);
                    expected.WriteByte(start + source + 17, 0);
                }
            }
            copy.Invoke(null, [actual, actual, y, move]);
            for (int offset = 0; offset < 0x600; offset++)
                AssertEqual(expected.ReadByte(0x7e9000 + offset), actual.ReadByte(0x7e9000 + offset), "stream 3 corpse row geometry");
        }
        foreach (int invalid in new[] { -1, 6, int.MaxValue })
        {
            AssertThrows<IndexOutOfRangeException>(() => MotherBrainCorpseArtworkDefinitions.VramPageSource(invalid), "stream 3 corpse source bounds");
            AssertThrows<IndexOutOfRangeException>(() => MotherBrainCorpseArtworkDefinitions.VramPageDestination(invalid), "stream 3 corpse destination bounds");
            AssertThrows<IndexOutOfRangeException>(() => _ = transfers[invalid], "stream 3 corpse transfer bounds");
        }
        VerifyMotherBrainCorpseStockArtwork();
    }

    private static void VerifyStream3ShitroidPulse(ISnesAddressSpace rom)
    {
        ushort Read(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        PaletteRgb5 Rgb(ushort word) => new() { Red = word & 31, Green = word >> 5 & 31, Blue = word >> 10 & 31 };
        ushort[] Words(int address, int count) => Enumerable.Range(0, count).Select(i => Read(address + 2 * i)).ToArray();
        var normal = Enumerable.Range(0, 8).Select(frame => Words(0xa9f6d1 + 8 * frame, 4)).ToArray();
        var sidehopper = Words(0xa9f8c6, 16);
        var shitroid = Words(0xa9f8e6, 16);
        var dead = Words(0xa9f8a6, 16);
        var rows = normal.Select(row => row.Select(Rgb).ToArray()).ToArray();
        var document = new ShitroidColorDocument
        {
            Version = 1, Normal = rows, Sidehopper = sidehopper.Select(Rgb).ToArray(),
            Shitroid = shitroid.Select(Rgb).ToArray(), DeadSidehopper = dead.Select(Rgb).ToArray(),
        };
        ShitroidColorCatalog Load() => ShitroidColorCatalog.Load(new MemoryStream(ShitroidColorCatalog.Write(document)));
        void Check(ShitroidColorCatalog colors)
        {
            for (int frame = 0; frame < 8; frame++)
            for (int color = 0; color < 4; color++)
                AssertEqual(normal[frame][color], colors.NormalColor(frame, color), "stream 3 Shitroid pulse RGB5");
            string identity = SelectedPresentationHash.Create("ShitroidColorCatalog-v1", content =>
            {
                content.AppendWords("sidehopper", sidehopper);
                content.AppendWords("shitroid", shitroid);
                content.AppendWords("deadSidehopper", dead);
                content.AppendWordFrames("normal", normal);
            });
            AssertEqual(identity, colors.ContentIdentity, "stream 3 Shitroid pulse original identity framing");
        }
        var stock = Load();
        Check(stock);
        var pulse = typeof(ShitroidColorCatalog).GetField("normal",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(stock)!;
        AssertTrue(pulse.GetType().GetField("supplied", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .GetValue(pulse) is null, "stream 3 Shitroid original pulse rows discarded");
        for (int frame = 0; frame < 8; frame++)
        for (int color = 0; color < 4; color++)
        for (int channel = 0; channel < 3; channel++)
        {
            ushort original = normal[frame][color];
            normal[frame][color] ^= (ushort)(1 << (5 * channel));
            rows[frame][color] = Rgb(normal[frame][color]);
            Check(Load());
            normal[frame][color] = original;
            rows[frame][color] = Rgb(original);
        }
        foreach (int invalid in new[] { -1, 8, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.NormalColor(invalid, 0), "stream 3 Shitroid pulse frame bounds");
        foreach (int invalid in new[] { -1, 4, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.NormalColor(0, invalid), "stream 3 Shitroid pulse color bounds");
    }

    private static void VerifyStream3DrainFades(ISnesAddressSpace rom)
    {
        ushort Read(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        PaletteRgb5 Color(ushort word) => new() { Red = word & 31, Green = word >> 5 & 31, Blue = word >> 10 & 31 };
        ushort Word(PaletteRgb5 color) => (ushort)(color.Red | color.Green << 5 | color.Blue << 10);

        MotherBrainRainbowPaletteFrameDocument ReadFull(int bodySource, int legSource) => new()
        {
            Body = Enumerable.Range(0, 15).Select(color => Color(Read(bodySource + 2 * color))).ToArray(),
            BackLegs = Enumerable.Range(0, 15).Select(color => Color(Read(legSource + 2 * color))).ToArray(),
        };
        var rainbow = Enumerable.Range(0, 10).Select(frame =>
        {
            int source = 0xad0000 | Read(0xade434 + 2 * frame);
            return ReadFull(source, source + 30);
        }).ToArray();
        var drain = new MotherBrainRainbowPaletteFrameDocument[8];
        var fake = new PaletteRgb5[8][];
        var revival = new MotherBrainRainbowPaletteFrameDocument[8];
        for (int frame = 0; frame < 8; frame++)
        {
            int source = 0xad0000 | Read(0xadef87 + 2 * frame);
            drain[frame] = new()
            {
                Body = Enumerable.Range(0, 15).Select(color => Color(Read(source + 2 * color))).ToArray(),
                BackLegs = Enumerable.Range(0, 5).Select(color => Color(Read(source + 30 + 2 * color))).ToArray(),
                TrailingColor = Color(Read(source + 40)),
            };
            int revivalSource = 0xad0000 | Read(0xaded9c + 2 * frame);
            revival[frame] = new()
            {
                Body = Enumerable.Range(0, 13).Select(color => Color(Read(revivalSource + 2 * color))).ToArray(),
                BackLegs = Enumerable.Range(0, 5).Select(color => Color(Read(revivalSource + 26 + 2 * color))).ToArray(),
                TrailingColor = Color(Read(revivalSource + 36)),
            };
            int fakeSource = 0xad0000 | Read(0xaded8a + 2 * frame);
            fake[frame] = Enumerable.Range(0, 3).Select(color => Color(Read(fakeSource + 2 * color))).ToArray();
        }
        var document = new MotherBrainRainbowPaletteDocument
        {
            Version = 3, Rainbow = rainbow,
            ToGrey = drain, FromGrey = revival,
            FakeDeathToGrey = fake, Normal = ReadFull(0xa99474, 0xa99494), BeamInitial = Color(0x3ce0),
            BeamCycle = Enumerable.Range(0, 38).Select(index => Color(Read(0x88e833 + 4 * index))).ToArray(),
        };
        MotherBrainRainbowPalettePresentation Load(MotherBrainRainbowPaletteDocument value,
            MotherBrainRainbowPalettePresentation? stock = null) => MotherBrainRainbowPalettePresentation.Load(
                new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(value, MapPresentationFormat.JsonOptions)), stock);
        void Check(MotherBrainRainbowPalettePresentation palette)
        {
            var cgram = new SnesCgram();
            var bus = new TestAddressSpace();
            for (int frame = 0; frame < 8; frame++)
            {
                palette.ApplyToGrey(bus, cgram, frame);
                for (int color = 0; color < 15; color++)
                {
                    AssertEqual(Word(drain[frame].Body[color]), cgram.Colors[0x41 + color], "stream 3 drain body RGB5");
                    AssertEqual(Word(drain[frame].Body[color]), cgram.Colors[0x91 + color], "stream 3 drain brain RGB5");
                }
                for (int color = 0; color < 5; color++)
                    AssertEqual(Word(drain[frame].BackLegs[color]), cgram.Colors[180 + color], "stream 3 drain legs RGB5");
                AssertEqual(Word(drain[frame].TrailingColor!), (ushort)(bus.ReadByte(0x7e017c) | bus.ReadByte(0x7e017d) << 8),
                    "stream 3 drain trailing word");
                palette.ApplyFakeDeathToGrey(cgram, frame);
                for (int color = 0; color < 3; color++)
                    AssertEqual(Word(fake[frame][color]), cgram.Colors[0x91 + color], "stream 3 fake-death brain RGB5");
            }
        }
        var stock = Load(document);
        void CheckBeam(MotherBrainRainbowPalettePresentation palette)
        {
            for (int index = 0; index < 38; index++)
                AssertEqual(Word(document.BeamCycle[index]), palette.BeamColorWord(index * 4),
                    "stream 3 beam wheel matches every native sampled color");
            AssertEqual(ushort.MaxValue, palette.BeamColorWord(152), "stream 3 beam signed loop terminator");
            AssertEqual((ushort)0x3ce0, palette.BeamInitialColor, "stream 3 beam initial fixed color unchanged");
        }
        CheckBeam(stock);
        const System.Reflection.BindingFlags beamFields = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        object beamColors = typeof(MotherBrainRainbowPalettePresentation).GetField("beamCycle", beamFields)!.GetValue(stock)!;
        AssertTrue(beamColors.GetType().GetField("supplied", beamFields)!.GetValue(beamColors) is null,
            "stream 3 beam wheel retains no stock lookup row");
        for (int index = 0; index < 38; index++)
        for (int channel = 0; channel < 3; channel++)
        {
            PaletteRgb5 original = document.BeamCycle[index];
            document.BeamCycle[index] = Color((ushort)(Word(original) ^ 1 << (5 * channel)));
            CheckBeam(Load(document));
            document.BeamCycle[index] = original;
        }
        foreach (int invalid in new[] { -1, 1, 2, 3, 153, 156, int.MaxValue })
            AssertThrows<InvalidDataException>(() => stock.BeamColorWord(invalid), "stream 3 beam cursor bounds and alignment");
        Check(stock);
        void CheckRainbow(MotherBrainRainbowPalettePresentation palette)
        {
            var cgram = new SnesCgram();
            for (int frame = 0; frame <= 10; frame++)
            {
                var expected = frame == 10 ? document.Normal : rainbow[frame];
                if (frame == 10) palette.ApplyNormal(cgram);
                else palette.ApplyRainbow(cgram, frame);
                for (int color = 0; color < 15; color++)
                {
                    AssertEqual(Word(expected.Body[color]), cgram.Colors[0x41 + color], "stream 3 rainbow body");
                    AssertEqual(Word(expected.Body[color]), cgram.Colors[0x91 + color], "stream 3 rainbow brain");
                    AssertEqual(Word(expected.BackLegs[color]), cgram.Colors[0xb1 + color], "stream 3 rainbow shadow");
                }
            }
        }
        void CheckRevival(MotherBrainRainbowPalettePresentation palette)
        {
            var bus = new TestAddressSpace();
            var cgram = new SnesCgram();
            for (int frame = 0; frame < 8; frame++)
            {
                cgram.SetColor(0x4e, 123);
                cgram.SetColor(0x9e, 123);
                palette.ApplyFromGrey(bus, cgram, frame);
                for (int color = 0; color < 13; color++)
                {
                    AssertEqual(Word(revival[frame].Body[color]), cgram.Colors[0x41 + color], "stream 3 revival body");
                    AssertEqual(Word(revival[frame].Body[color]), cgram.Colors[0x91 + color], "stream 3 revival brain");
                }
                AssertEqual((ushort)123, cgram.Colors[0x4e], "stream 3 revival preserves body tail");
                AssertEqual((ushort)123, cgram.Colors[0x9e], "stream 3 revival preserves brain tail");
                for (int color = 0; color < 5; color++)
                    AssertEqual(Word(revival[frame].BackLegs[color]), cgram.Colors[180 + color], "stream 3 revival legs");
                AssertEqual(Word(revival[frame].TrailingColor!), (ushort)(bus.ReadByte(0x7e017c) | bus.ReadByte(0x7e017d) << 8), "stream 3 revival trailing word");
                palette.ApplyFakeDeathFromGrey(cgram, frame);
                for (int color = 0; color < 3; color++)
                    AssertEqual(Word(revival[frame].Body[color]), cgram.Colors[0x91 + color], "stream 3 fake-death revival");
            }
        }
        CheckRevival(stock);
        const System.Reflection.BindingFlags privateFields = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        object revivalFade = typeof(MotherBrainRainbowPalettePresentation).GetField("fromGrey", privateFields)!.GetValue(stock)!;
        var revivalChannels = (Array)revivalFade.GetType().GetField("channels", privateFields)!.GetValue(revivalFade)!;
        int suppliedChannels = 0;
        foreach (object channel in revivalChannels)
            if (channel.GetType().GetField("supplied", privateFields)!.GetValue(channel) is not null)
                suppliedChannels++;
        AssertEqual(1, suppliedChannels, "stream 3 calculates 56 revival channel curves; only unmatched green remains supplied");
        for (int frame = 0; frame < 8; frame++)
        for (int color = 0; color < 19; color++)
        for (int component = 0; component < 3; component++)
        {
            var row = revival[frame];
            PaletteRgb5 original = color < 13 ? row.Body[color] : color < 18 ? row.BackLegs[color - 13] : row.TrailingColor!;
            void Set(PaletteRgb5 value)
            {
                if (color < 13) row.Body[color] = value;
                else if (color < 18) row.BackLegs[color - 13] = value;
                else revival[frame] = row with { TrailingColor = value };
            }
            Set(Color((ushort)(Word(original) ^ 1 << (5 * component))));
            CheckRevival(Load(document));
            Set(original);
        }
        CheckRainbow(stock);
        object fakeFade = typeof(MotherBrainRainbowPalettePresentation).GetField("fakeDeathToGrey", privateFields)!.GetValue(stock)!;
        foreach (string endpointName in new[] { "first", "last" })
        {
            object endpoint = fakeFade.GetType().GetField(endpointName, privateFields)!.GetValue(fakeFade)!;
            object body = endpoint.GetType().GetProperty("Body")!.GetValue(endpoint)!;
            AssertTrue(body.GetType().GetField("supplied", privateFields)!.GetValue(body) is null,
                "stream 3 fake-death endpoints reuse approved normal/drained cortex paint without stored rows");
        }
        object drainFade = typeof(MotherBrainRainbowPalettePresentation).GetField("toGrey", privateFields)!.GetValue(stock)!;
        object drainEnd = drainFade.GetType().GetField("last", privateFields)!.GetValue(drainFade)!;
        object drainBody = drainEnd.GetType().GetProperty("Body")!.GetValue(drainEnd)!;
        var drainedPaint = (MotherBrainRainbowPalettePresentation.DrainedBodyColors)drainBody.GetType()
            .GetField("drained", privateFields)!.GetValue(drainBody)!;
        AssertTrue(drainedPaint.Calculated, "stream 3 drained final gray body palette has no stored row");
        object normalFrame = typeof(MotherBrainRainbowPalettePresentation).GetField("normal", privateFields)!.GetValue(stock)!;
        object normalBody = normalFrame.GetType().GetProperty("Body")!.GetValue(normalFrame)!;
        AssertTrue(normalBody.GetType().GetField("supplied", privateFields)!.GetValue(normalBody) is null,
            "stream 3 normal body calculates from shared health paint without a stored row");
        AssertTrue(normalFrame.GetType().GetField("backLegs", privateFields)!.GetValue(normalFrame) is null,
            "stream 3 normal rear palette calculates from shared health lighting without a stored row");
        var storedRainbow = (Array)typeof(MotherBrainRainbowPalettePresentation).GetField("rainbow",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(stock)!;
        foreach (object frame in storedRainbow)
            AssertTrue(frame.GetType().GetField("backLegs", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .GetValue(frame) is null, "stream 3 rainbow shadow tables discarded");
        for (int frame = 0; frame <= 10; frame++)
        for (int color = 0; color < 30; color++)
        for (int channel = 0; channel < 3; channel++)
        {
            var selected = frame == 10 ? document.Normal : rainbow[frame];
            PaletteRgb5[] row = color < 15 ? selected.Body : selected.BackLegs;
            int index = color % 15;
            PaletteRgb5 original = row[index];
            row[index] = Color((ushort)(Word(original) ^ 1 << (5 * channel)));
            CheckRainbow(Load(document));
            row[index] = original;
        }
        foreach (int invalid in new[] { -1, 10, int.MaxValue })
            AssertThrows<InvalidDataException>(() => stock.ApplyRainbow(new SnesCgram(), invalid), "stream 3 rainbow bounds");
        foreach (string field in new[] { "toGrey", "fakeDeathToGrey" })
        {
            var fade = typeof(MotherBrainRainbowPalettePresentation).GetField(field,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(stock)!;
            AssertTrue(fade.GetType().GetField("supplied", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .GetValue(fade) is null, "stream 3 drain stock intermediate rows discarded");
        }
        Check(Load(document with { Version = 2, FakeDeathToGrey = null }, stock));
        // Independently edited channels must still be delivered exactly, including endpoints and WRAM.
        for (int frame = 0; frame < 8; frame++)
        for (int color = 0; color < 24; color++)
        for (int channel = 0; channel < 3; channel++)
        {
            var row = drain[frame];
            PaletteRgb5 original = color < 15 ? row.Body[color] : color < 20 ? row.BackLegs[color - 15]
                : color == 20 ? row.TrailingColor! : fake[frame][color - 21];
            void Set(PaletteRgb5 value)
            {
                if (color < 15) row.Body[color] = value;
                else if (color < 20) row.BackLegs[color - 15] = value;
                else if (color == 20) drain[frame] = row with { TrailingColor = value };
                else fake[frame][color - 21] = value;
            }
            Set(Color((ushort)(Word(original) ^ 1 << (5 * channel))));
            Check(Load(document));
            Set(original);
        }
        foreach (int invalid in new[] { -1, 8, int.MaxValue })
        {
            AssertThrows<InvalidDataException>(() => stock.ApplyToGrey(new TestAddressSpace(), new SnesCgram(), invalid), "stream 3 drain bounds");
            AssertThrows<InvalidDataException>(() => stock.ApplyFakeDeathToGrey(new SnesCgram(), invalid), "stream 3 fake-death bounds");
        }
    }

    private static void VerifyStream3BabyFade(ISnesAddressSpace rom)
    {
        ushort Read(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var initial = new ushort[15];
        var fade = new ushort[6][];
        for (int color = 0; color < initial.Length; color++)
            initial[color] = Read(0xa994d4 + 2 * color);
        for (int frame = 0; frame < fade.Length; frame++)
        {
            fade[frame] = new ushort[14];
            for (int color = 0; color < fade[frame].Length; color++)
                fade[frame][color] = Read(0xade90c + 28 * frame + 2 * color);
        }
        static PaletteRgb5 Rgb(ushort value) => new()
        {
            Red = value & 31, Green = (value >> 5) & 31, Blue = (value >> 10) & 31,
        };
        BabyMetroidCutsceneColorCatalog Load() => BabyMetroidCutsceneColorCatalog.Load(new MemoryStream(
            BabyMetroidCutsceneColorCatalog.Write(new BabyMetroidCutsceneColorDocument
            {
                Version = 1,
                Initial = initial.Select(Rgb).ToArray(),
                Fade = fade.Select(row => row.Select(Rgb).ToArray()).ToArray(),
            }), writable: false));
        void Check(BabyMetroidCutsceneColorCatalog catalog)
        {
            for (int color = 0; color < initial.Length; color++)
                AssertEqual(initial[color], catalog.InitialColor(color), "stream 3 Baby initial color");
            for (int frame = 0; frame < fade.Length; frame++)
            for (int color = 0; color < fade[frame].Length; color++)
                AssertEqual(fade[frame][color], catalog.FadeColor(frame + 1, color), "stream 3 Baby fade selected color");
            string identity = SelectedPresentationHash.Create("BabyMetroidCutsceneColorCatalog-v1", content =>
            {
                content.AppendWords("initial", initial);
                content.AppendWordFrames("fade", fade);
            });
            AssertEqual(identity, catalog.ContentIdentity, "stream 3 Baby fade selected identity");
        }
        var stock = Load();
        Check(stock);
        const System.Reflection.BindingFlags fields = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        object selectedFade = typeof(BabyMetroidCutsceneColorCatalog).GetField("fade", fields)!.GetValue(stock)!;
        AssertTrue(selectedFade.GetType().GetField("supplied", fields)!.GetValue(selectedFade) is null,
            "stream 3 original Baby fade discards its stored frame table");
        uint[] endpoints = (uint[])selectedFade.GetType().GetField("endpointColors", fields)!.GetValue(selectedFade)!;
        for (int color = 0; color < endpoints.Length; color++)
        {
            uint value = endpoints[color];
            ushort rgb5 = (ushort)(((value & 255) >> 3) | (((value >> 8 & 255) >> 3) << 5) | (((value >> 16 & 255) >> 3) << 10));
            AssertEqual(Read(0xade8f0 + 2 * color), rgb5,
                "stream 3 compatible RGB8 endpoint also matches original undisplayed RGB5 palette");
        }
        for (int frame = 0; frame < fade.Length; frame++)
        for (int color = 0; color < fade[frame].Length; color++)
        for (int channel = 0; channel < 3; channel++)
        {
            ushort original = fade[frame][color];
            fade[frame][color] ^= (ushort)(1 << (5 * channel));
            Check(Load());
            fade[frame][color] = original;
        }
        foreach (int invalid in new[] { -1, 0, 7, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.FadeColor(invalid, 0), "stream 3 Baby fade frame bounds");
        foreach (int invalid in new[] { -1, 14, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.FadeColor(1, invalid), "stream 3 Baby fade color bounds");
    }

    private static void VerifyStream3MotherBrainFades(ISnesAddressSpace rom)
    {
        ushort Read(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var body = new ushort[16][];
        var leg = new ushort[16][];
        var corpse = new ushort[8][];
        for (int frame = 0; frame < 16; frame++)
        {
            body[frame] = new ushort[14];
            leg[frame] = new ushort[14];
            for (int color = 0; color < 14; color++)
            {
                body[frame][color] = Read(0xadea0a + 56 * frame + 2 * color);
                leg[frame][color] = Read(0xadea26 + 56 * frame + 2 * color);
            }
        }
        for (int frame = 0; frame < 8; frame++)
        {
            corpse[frame] = new ushort[15];
            for (int color = 0; color < 15; color++)
                corpse[frame][color] = Read(0xadf119 + 30 * frame + 2 * color);
        }
        var door = new ushort[14];
        for (int color = 0; color < door.Length; color++)
            door[color] = Read(0xa99534 + 2 * color);

        static PaletteRgb5 Rgb(ushort value) => new()
        {
            Red = value & 31, Green = (value >> 5) & 31, Blue = (value >> 10) & 31,
        };
        MotherBrainDeathColorCatalog Load() => MotherBrainDeathColorCatalog.Load(new MemoryStream(
            MotherBrainDeathColorCatalog.Write(new MotherBrainDeathColorDocument
            {
                Version = 1,
                BodyFade = body.Select(row => row.Select(Rgb).ToArray()).ToArray(),
                LegFade = leg.Select(row => row.Select(Rgb).ToArray()).ToArray(),
                CorpseFade = corpse.Select(row => row.Select(Rgb).ToArray()).ToArray(),
                ExplodedDoor = door.Select(Rgb).ToArray(),
            }), writable: false));
        void Check(MotherBrainDeathColorCatalog catalog)
        {
            for (int frame = 0; frame < 16; frame++)
            for (int color = 0; color < 14; color++)
            {
                AssertEqual(body[frame][color], catalog.BodyColor(frame, color), "stream 3 selected body fade color");
                AssertEqual(leg[frame][color], catalog.LegColor(frame, color), "stream 3 selected leg fade color");
            }
            for (int frame = 0; frame < 8; frame++)
            for (int color = 0; color < 15; color++)
                AssertEqual(corpse[frame][color], catalog.CorpseColor(frame, color), "stream 3 selected corpse fade color");
            for (int color = 0; color < door.Length; color++)
                AssertEqual(door[color], catalog.ExplodedDoorColor(color), "stream 3 unchanged door color");
            string identity = SelectedPresentationHash.Create("MotherBrainDeathColorCatalog-v1", content =>
            {
                content.AppendWords("explodedDoor", door);
                content.AppendWordFrames("bodyFade", body);
                content.AppendWordFrames("legFade", leg);
                content.AppendWordFrames("corpseFade", corpse);
            });
            AssertEqual(identity, catalog.ContentIdentity, "stream 3 death fade identity keeps exact selected values");
        }
        var stock = Load();
        Check(stock);
        const System.Reflection.BindingFlags fields = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        foreach (string name in new[] { "bodyFade", "legFade", "corpseFade" })
        {
            object fade = typeof(MotherBrainDeathColorCatalog).GetField(name, fields)!.GetValue(stock)!;
            AssertTrue(fade.GetType().GetField("first", fields)!.GetValue(fade) is null,
                "stream 3 death starts reuse calculated health state three without stored endpoint rows");
            AssertTrue(fade.GetType().GetField("supplied", fields)!.GetValue(fade) is null,
                "stream 3 original death fade discards its stored frame table");
            if (name == "corpseFade")
            {
                var endpoint = (MotherBrainRainbowPalettePresentation.DrainedBodyColors)fade.GetType()
                    .GetField("last", fields)!.GetValue(fade)!;
                AssertTrue(endpoint.Calculated, "stream 3 corpse final gray palette has no stored row");
                AssertEqual(0, endpoint[14], "stream 3 corpse neutral black endpoint");
            }
        }
        foreach (ushort[][] frames in new[] { body, leg, corpse })
        for (int frame = 0; frame < frames.Length; frame++)
        for (int color = 0; color < frames[frame].Length; color++)
        for (int channel = 0; channel < 3; channel++)
        {
            ushort original = frames[frame][color];
            frames[frame][color] ^= (ushort)(1 << (5 * channel));
            Check(Load());
            frames[frame][color] = original;
        }
        foreach (int invalid in new[] { -1, 16, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => stock.BodyColor(invalid, 0), "stream 3 body fade frame bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => stock.LegColor(invalid, 0), "stream 3 leg fade frame bounds");
        }
        foreach (int invalid in new[] { -1, 8, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.CorpseColor(invalid, 0), "stream 3 corpse fade frame bounds");
        foreach (int invalid in new[] { -1, 14, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => stock.BodyColor(0, invalid), "stream 3 body fade color bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => stock.LegColor(0, invalid), "stream 3 leg fade color bounds");
        }
        foreach (int invalid in new[] { -1, 15, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.CorpseColor(0, invalid), "stream 3 corpse fade color bounds");
    }

    private static void VerifyStream3UniformEnemyLoops(ISnesAddressSpace rom)
    {
        VerifyMochtroidInstructionProgramDefinitions();
        VerifyYellowPipeBugInstructionProgramDefinitions();
        Check(0xa30000, [0xa745, 0xa759],
            index =>
            {
                var word = MochtroidInstructionProgramDefinitions.MechanicsWord(index);
                return (word.Address, word.Value);
            }, MochtroidInstructionProgramDefinitions.PresentationWordAddress,
            MochtroidInstructionProgramDefinitions.ReadMechanicsWord,
            MochtroidInstructionProgramDefinitions.IsCompiledMechanicsByte);
        Check(0xb30000, [0x8efc, 0x8f10, 0x8f24, 0x8f38],
            index =>
            {
                var word = YellowPipeBugInstructionProgramDefinitions.MechanicsWord(index);
                return (word.Address, word.Value);
            }, YellowPipeBugInstructionProgramDefinitions.PresentationWordAddress,
            YellowPipeBugInstructionProgramDefinitions.ReadMechanicsWord,
            YellowPipeBugInstructionProgramDefinitions.IsCompiledMechanicsByte);

        void Check(int bank, ushort[] starts,
            Func<int, (ushort Address, ushort Value)> wordAt, Func<int, ushort> visualAt,
            Func<ushort, ushort> read, Func<int, bool> ownsByte)
        {
            var bytes = new HashSet<int>();
            int wordIndex = 0, visualIndex = 0;
            foreach (ushort start in starts)
            {
                foreach (int offset in new[] { 0, 4, 8, 12, 16, 18 })
                {
                    ushort address = (ushort)(start + offset);
                    ushort value = (ushort)(rom.ReadByte(bank | address) | rom.ReadByte(bank | (address + 1)) << 8);
                    AssertEqual((address, value), wordAt(wordIndex++), "stream 3 uniform loop native mechanic");
                    AssertEqual(value, read(address), "stream 3 uniform loop mechanic dispatch");
                    bytes.Add(address);
                    bytes.Add(address + 1);
                }
                for (int frame = 0; frame < 4; frame++)
                {
                    ushort address = (ushort)(start + 4 * frame + 2);
                    AssertEqual(address, visualAt(visualIndex++), "stream 3 uniform loop visual order");
                    AssertThrows<InvalidDataException>(() => read(address), "stream 3 uniform loop visual excluded");
                }
            }
            for (int address = 0; address <= ushort.MaxValue; address++)
                AssertEqual(bytes.Contains(address), ownsByte(bank | address), "stream 3 uniform loop byte ownership");
            foreach (int invalid in new[] { -1, starts.Length * 6, int.MaxValue })
                AssertThrows<IndexOutOfRangeException>(() => wordAt(invalid), "stream 3 uniform loop mechanic bounds");
            foreach (int invalid in new[] { -1, starts.Length * 4, int.MaxValue })
                AssertThrows<IndexOutOfRangeException>(() => visualAt(invalid), "stream 3 uniform loop visual bounds");
        }
    }

    private static void VerifyStream3RipperMappings(ISnesAddressSpace rom)
    {
        VerifyRipperInstructionProgramDefinitions();
        ushort[] programs = [0xe19b, 0xe1af, 0xe2e0, 0xe2f4, 0xe477, 0xe48b];
        var bytes = new HashSet<int>();
        int wordIndex = 0, visualIndex = 0;
        foreach (ushort start in programs)
        {
            foreach (int offset in new[] { 0, 4, 8, 12, 16, 18 })
            {
                ushort address = (ushort)(start + offset);
                ushort value = (ushort)(rom.ReadByte(0xa20000 | address) | rom.ReadByte(0xa20000 | (address + 1)) << 8);
                AssertEqual(new RipperInstructionMechanicsWord(address, value), RipperInstructionProgramDefinitions.MechanicsWord(wordIndex++),
                    "stream 3 Ripper mechanic order and value");
                bytes.Add(address);
                bytes.Add(address + 1);
            }
            for (int frame = 0; frame < 4; frame++)
            {
                ushort address = (ushort)(start + 4 * frame + 2);
                AssertEqual(address, RipperInstructionProgramDefinitions.PresentationWordAddress(visualIndex++),
                    "stream 3 Ripper visual operand order");
                if (start < 0xe477)
                {
                    ushort expected = (ushort)(rom.ReadByte(0xa20000 | address) | rom.ReadByte(0xa20000 | (address + 1)) << 8);
                    AssertEqual(expected, RipperVisualDefinitions.FrameAt(RoomEnemySystem.GRipperDefinition, address),
                        "stream 3 GRipper preserves shared operand domain");
                    AssertEqual(expected, RipperVisualDefinitions.FrameAt(RoomEnemySystem.Ripper2Definition, address),
                        "stream 3 Ripper II preserves shared operand domain");
                }
            }
        }
        for (int address = 0; address <= ushort.MaxValue; address++)
            AssertEqual(bytes.Contains(address), RipperInstructionProgramDefinitions.IsCompiledMechanicsByte(0xa20000 | address),
                "stream 3 Ripper byte ownership domain");
        foreach (int invalid in new[] { -1, 36, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => RipperInstructionProgramDefinitions.MechanicsWord(invalid),
                "stream 3 Ripper mechanic bounds");
        foreach (int invalid in new[] { -1, 24, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => RipperInstructionProgramDefinitions.PresentationWordAddress(invalid),
                "stream 3 Ripper visual bounds");
        foreach (ushort invalid in new ushort[] { 0xe19b, 0xe1ad, 0xe1bf, 0xe477, 0xffff })
            AssertThrows<InvalidDataException>(() => RipperVisualDefinitions.FrameAt(RoomEnemySystem.GRipperDefinition, invalid),
                "stream 3 Ripper rejects nonvisual and foreign-family operands");
        AssertThrows<InvalidDataException>(() => RipperVisualDefinitions.FrameAt(0, 0xe19d),
            "stream 3 Ripper rejects foreign enemy");
    }

    private static void VerifyStream3ChootControl(ISnesAddressSpace rom)
    {
        ushort Read(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (ushort pattern = 0; pattern < 5; pattern++)
        {
            ushort pointer = Read(0xa2df5e + 2 * pattern);
            ushort distancePointer = Read(0xa2df6a + 2 * pattern);
            AssertEqual(new ChootPatternDefinition(pointer, Read(0xa20000 | distancePointer)),
                ChootPatternDefinitions.ForIndex(pattern), "stream 3 Choot pattern identity and loop advance");
        }
        foreach (ushort invalid in new ushort[] { 5, 6, ushort.MaxValue })
            AssertThrows<InvalidDataException>(() => ChootPatternDefinitions.ForIndex(invalid),
                "stream 3 Choot rejects alias and invalid patterns");
        ushort[] mechanics = [0xd82c, 0xd82e, 0xd832, 0xd834, 0xd836, 0xd83a, 0xd83e, 0xd840, 0xd842, 0xd846, 0xd84a];
        ushort[] presentation = [0xd830, 0xd838, 0xd83c, 0xd844, 0xd848];
        AssertEqual(mechanics.Length, ChootInstructionProgramDefinitions.MechanicsWordCount, "stream 3 Choot mechanics count");
        AssertEqual(presentation.Length, ChootInstructionProgramDefinitions.PresentationWordCount, "stream 3 Choot visual count");
        for (int index = 0; index < mechanics.Length; index++)
        {
            ushort address = mechanics[index];
            ushort value = Read(0xa20000 | address);
            AssertEqual(new ChootInstructionMechanicsWord(address, value), ChootInstructionProgramDefinitions.MechanicsWord(index),
                "stream 3 Choot native control instruction");
            AssertEqual(value, ChootInstructionProgramDefinitions.ReadMechanicsWord(address), "stream 3 Choot control dispatch");
        }
        for (int index = 0; index < presentation.Length; index++)
        {
            ushort address = presentation[index];
            AssertEqual(address, ChootInstructionProgramDefinitions.PresentationWordAddress(index), "stream 3 Choot visual operand");
            AssertThrows<InvalidDataException>(() => ChootInstructionProgramDefinitions.ReadMechanicsWord(address),
                "stream 3 Choot visual operand remains excluded");
        }
        var bytes = mechanics.SelectMany(address => new[] { (int)address, address + 1 }).ToHashSet();
        var visualWords = presentation.ToHashSet();
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            AssertEqual(bytes.Contains(address), ChootInstructionProgramDefinitions.IsCompiledMechanicsByte(0xa20000 | address),
                "stream 3 Choot byte ownership");
            AssertEqual(visualWords.Contains((ushort)address), ChootInstructionProgramDefinitions.IsPresentationWord((ushort)address),
                "stream 3 Choot presentation ownership");
        }
        foreach (int invalid in new[] { -1, 11, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => ChootInstructionProgramDefinitions.MechanicsWord(invalid),
                "stream 3 Choot mechanics bounds");
        foreach (int invalid in new[] { -1, 5, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => ChootInstructionProgramDefinitions.PresentationWordAddress(invalid),
                "stream 3 Choot visual bounds");
    }

    private static void VerifyStream3PickupAndFirefleaPrograms(ISnesAddressSpace rom)
    {
        ushort Read(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (int kind = 0; kind < 6; kind++)
            AssertEqual(new EnemyPickupAnimationDefinition((ushort)(2 * kind), Read(0x86ef04 + 2 * kind)),
                EnemyPickupDefinitions.Animation((EnemyPickupKind)kind), "stream 3 pickup kind dispatch");
        for (ushort animation = 0; animation < 5; animation++)
            AssertEqual(Read(0x86efd5 + 2 * animation), EnemyDeathExplosionDefinitions.InstructionPointer(animation),
                "stream 3 death variant dispatch");
        foreach (ushort invalid in new ushort[] { 6, 7, ushort.MaxValue })
            AssertThrows<InvalidDataException>(() => EnemyPickupDefinitions.Animation((EnemyPickupKind)invalid),
                "stream 3 invalid pickup kind");
        foreach (ushort invalid in new ushort[] { 5, 6, ushort.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => EnemyDeathExplosionDefinitions.InstructionPointer(invalid),
                "stream 3 invalid death variant");

        ushort[] pickupMechanics =
        [
            0xed8d, 0xed91, 0xed95, 0xed99, 0xed9d, 0xed9f, 0xeda1,
            0xeda3, 0xeda7, 0xedab, 0xedaf, 0xedb3, 0xedb5, 0xedb7,
            0xedb9, 0xedbd, 0xedc1, 0xedc3, 0xedc5,
            0xeddd, 0xede1, 0xede5, 0xede7, 0xede9,
            0xedeb, 0xedef, 0xedf3, 0xedf7, 0xedfb, 0xedfd,
        ];
        ushort[] pickupPresentation =
        [
            0xed8f, 0xed93, 0xed97, 0xed9b, 0xeda5, 0xeda9, 0xedad, 0xedb1,
            0xedbb, 0xedbf, 0xeddf, 0xede3, 0xeded, 0xedf1, 0xedf5, 0xedf9,
        ];
        AssertEqual(pickupMechanics.Length, EnemyPickupInstructionProgramDefinitions.MechanicsWordCount,
            "stream 3 pickup mechanic count");
        AssertEqual(pickupPresentation.Length, EnemyPickupInstructionProgramDefinitions.PresentationWordCount,
            "stream 3 pickup visual operand count");
        for (int index = 0; index < pickupMechanics.Length; index++)
        {
            ushort address = pickupMechanics[index];
            AssertEqual(new EnemyPickupInstructionMechanicsWord(address, Read(0x860000 | address)),
                EnemyPickupInstructionProgramDefinitions.MechanicsWord(index), "stream 3 native pickup mechanic");
            AssertEqual(Read(0x860000 | address), EnemyPickupInstructionProgramDefinitions.ReadMechanicsWord(address),
                "stream 3 pickup mechanic dispatch");
        }
        for (int index = 0; index < pickupPresentation.Length; index++)
        {
            ushort address = pickupPresentation[index];
            AssertEqual(address, EnemyPickupInstructionProgramDefinitions.PresentationWordAddress(index),
                "stream 3 pickup visual operand address");
            AssertThrows<InvalidDataException>(() => EnemyPickupInstructionProgramDefinitions.ReadMechanicsWord(address),
                "stream 3 pickup visual operand remains excluded");
        }
        var mechanicsSet = pickupMechanics.ToHashSet();
        var byteSet = pickupMechanics.SelectMany(address => new[] { (int)address, address + 1 }).ToHashSet();
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            bool owned = mechanicsSet.Contains((ushort)address);
            AssertEqual(owned, EnemyPickupInstructionProgramDefinitions.Owns(RoomEnemyProjectileKind.EnemyDeathPickup, (ushort)address),
                "stream 3 pickup ownership domain");
            AssertEqual(owned, EnemyPickupInstructionProgramDefinitions.Owns(RoomEnemyProjectileKind.EnemyDeathExplosion, (ushort)address),
                "stream 3 explosion pickup ownership domain");
            AssertEqual(byteSet.Contains(address), EnemyPickupInstructionProgramDefinitions.IsCompiledMechanicsByte(0x860000 | address),
                "stream 3 pickup byte ownership domain");
        }
        AssertTrue(!EnemyPickupInstructionProgramDefinitions.Owns(RoomEnemyProjectileKind.ShaktoolAttackFrontCircle, pickupMechanics[0]),
            "stream 3 unrelated actor does not own pickup instructions");
        AssertTrue(!EnemyPickupInstructionProgramDefinitions.IsCompiledMechanicsByte(0xa3ed8d),
            "stream 3 pickup excludes other bank");

        AssertEqual(54, FirefleaInstructionProgramDefinitions.MechanicsWordCount, "stream 3 Fireflea mechanic count");
        AssertEqual(52, FirefleaInstructionProgramDefinitions.PresentationWordCount, "stream 3 Fireflea visual count");
        var fireBytes = new HashSet<int>();
        for (int index = 0; index < 54; index++)
        {
            ushort address = (ushort)(index < 52 ? 0x8c2f + index * 4 : 0x8cff + (index - 52) * 2);
            ushort value = Read(0xa30000 | address);
            AssertEqual(new FirefleaInstructionMechanicsWord(address, value), FirefleaInstructionProgramDefinitions.MechanicsWord(index),
                "stream 3 native Fireflea mechanic");
            AssertEqual(value, FirefleaInstructionProgramDefinitions.ReadMechanicsWord(address),
                "stream 3 Fireflea mechanic dispatch");
            fireBytes.Add(address);
            fireBytes.Add(address + 1);
            if (index < 52)
            {
                ushort visual = (ushort)(address + 2);
                AssertEqual(visual, FirefleaInstructionProgramDefinitions.PresentationWordAddress(index),
                    "stream 3 Fireflea visual operand");
                AssertThrows<InvalidDataException>(() => FirefleaInstructionProgramDefinitions.ReadMechanicsWord(visual),
                    "stream 3 Fireflea visual remains excluded");
            }
        }
        for (int address = 0; address <= ushort.MaxValue; address++)
            AssertEqual(fireBytes.Contains(address), FirefleaInstructionProgramDefinitions.IsCompiledMechanicsByte(0xa30000 | address),
                "stream 3 Fireflea byte ownership domain");
        foreach (int invalid in new[] { -1, 30, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => EnemyPickupInstructionProgramDefinitions.MechanicsWord(invalid),
                "stream 3 pickup mechanic bounds");
        foreach (int invalid in new[] { -1, 16, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => EnemyPickupInstructionProgramDefinitions.PresentationWordAddress(invalid),
                "stream 3 pickup visual bounds");
        foreach (int invalid in new[] { -1, 54, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => FirefleaInstructionProgramDefinitions.MechanicsWord(invalid),
                "stream 3 Fireflea mechanic bounds");
        foreach (int invalid in new[] { -1, 52, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => FirefleaInstructionProgramDefinitions.PresentationWordAddress(invalid),
                "stream 3 Fireflea visual bounds");
    }

    private static void VerifyStream3WorkRobotColors(ISnesAddressSpace rom)
    {
        var words = new ushort[6][];
        var colors = new PaletteRgb5[6][];
        for (int frame = 0; frame < 6; frame++)
        {
            words[frame] = new ushort[4];
            colors[frame] = new PaletteRgb5[4];
            for (int color = 0; color < 4; color++)
            {
                int address = 0xa8ccc1 + 10 * frame + 2 * color;
                ushort value = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
                words[frame][color] = value;
                colors[frame][color] = new PaletteRgb5
                {
                    Red = value & 31,
                    Green = (value >> 5) & 31,
                    Blue = (value >> 10) & 31,
                };
            }
        }
        var document = new WorkRobotPaletteCycleDocument { Version = 1, Frames = colors };
        WorkRobotPaletteCycle Load() => WorkRobotPaletteCycle.Load(
            new MemoryStream(WorkRobotPaletteCycle.Write(document), writable: false));
        void Check(WorkRobotPaletteCycle cycle)
        {
            var cgram = new SnesCgram();
            for (int frame = 0; frame < 6; frame++)
            {
                cycle.ApplyFrame(cgram, frame, 9);
                for (int color = 0; color < 4; color++)
                {
                    AssertEqual(words[frame][color], cycle.Resolve(frame, color),
                        "stream 3 Work Robot selected color");
                    AssertEqual(words[frame][color], cgram.Colors[9 + color],
                        "stream 3 Work Robot applied color");
                }
            }
            string expectedIdentity = SelectedPresentationHash.Create("WorkRobotPaletteCycle-v1",
                content => content.AppendWordFrames("frames", words));
            AssertEqual(expectedIdentity, cycle.ContentIdentity,
                "stream 3 Work Robot identity preserves original row framing");
        }
        var stock = Load();
        Check(stock);
        AssertTrue(typeof(WorkRobotPaletteCycle).GetField("frames",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(stock) is null,
            "stream 3 original Work Robot colors discard their stored frame table");
        for (int frame = 0; frame < 6; frame++)
        for (int color = 0; color < 4; color++)
        for (int channel = 0; channel < 3; channel++)
        {
            PaletteRgb5 original = colors[frame][color];
            ushort originalWord = words[frame][color];
            colors[frame][color] = channel switch
            {
                0 => original with { Red = original.Red ^ 1 },
                1 => original with { Green = original.Green ^ 1 },
                _ => original with { Blue = original.Blue ^ 1 },
            };
            words[frame][color] ^= (ushort)(1 << (channel * 5));
            Check(Load());
            colors[frame][color] = original;
            words[frame][color] = originalWord;
        }
        foreach (int invalid in new[] { -1, 6, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve(invalid, 0),
                "stream 3 Work Robot frame bounds");
        foreach (int invalid in new[] { -1, 4, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve(0, invalid),
                "stream 3 Work Robot color bounds");
    }
    private static void VerifyStream3HealthTint(ISnesAddressSpace rom)
    {
        byte[] json = SuperMetroid.AssetExtraction.MotherBrainHealthPaletteExtractor.Extract(rom);
        var stock = MotherBrainHealthPalettePresentation.Load(new MemoryStream(json));
        var node = System.Text.Json.Nodes.JsonNode.Parse(json)!;
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        foreach (string name in new[] { "body", "backLegs" })
        {
            object palette = typeof(MotherBrainHealthPalettePresentation).GetField(name, flags)!.GetValue(stock)!;
            AssertTrue(palette.GetType().GetField("supplied", flags)!.GetValue(palette) is null,
                "stream 3 native health tint has no stored intermediate rows");
            object basis = palette.GetType().GetField("basis", flags)!.GetValue(palette)!;
            AssertTrue(basis.GetType().GetField("supplied", flags)!.GetValue(basis) is null,
                "stream 3 native health shade ramps calculated from paint endpoints");
            ushort[] anchors = basis.GetType().GetFields(flags).Where(field => field.FieldType == typeof(ushort))
                .Select(field => (ushort)field.GetValue(basis)!).ToArray();
            AssertEqual(6, anchors.Length, "stream 3 health base exposes only six possible paint anchors");
            AssertTrue(name == "body" ? anchors.All(value => value != 0) : anchors.All(value => value == 0),
                "stream 3 stock rear palette stores no independent paint colors");
        }
        for (int state = 0; state < 4; state++)
        {
            var actual = new SnesCgram();
            stock.Apply(actual, state);
            foreach (var (table, destination) in new[]
            {
                (MotherBrainHealthPaletteRomData.BrainTable, MotherBrainRainbowPaletteRomData.BodyColor),
                (MotherBrainHealthPaletteRomData.BackLegTable, MotherBrainRainbowPaletteRomData.SecondaryColor),
            })
            {
                int pointer = rom.ReadByte(table + state * 2) | rom.ReadByte(table + state * 2 + 1) << 8;
                for (int color = 0; color < 15; color++)
                {
                    int address = 0xad0000 | pointer + color * 2;
                    ushort expected = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
                    AssertEqual(expected, actual.Colors[destination + color], "stream 3 all native health tint words");
                }
            }
            for (int color = 0; color < 15; color++)
                AssertEqual(actual.Colors[MotherBrainRainbowPaletteRomData.BodyColor + color],
                    actual.Colors[MotherBrainRainbowPaletteRomData.BrainColor + color], "stream 3 health tint body/brain copies");
        }
        foreach (string group in new[] { "body", "backLegs" })
            for (int state = 0; state < 4; state++)
                for (int color = 0; color < 15; color++)
                    foreach (string component in new[] { "red", "green", "blue" })
                    {
                        var editedNode = node.DeepClone();
                        var rgb = editedNode[group]![state]![color]!;
                        rgb[component] = rgb[component]!.GetValue<int>() ^ 1;
                        var edited = MotherBrainHealthPalettePresentation.Load(new MemoryStream(
                            System.Text.Encoding.UTF8.GetBytes(editedNode.ToJsonString())));
                        for (int checkState = 0; checkState < 4; checkState++)
                        {
                            var actual = new SnesCgram();
                            edited.Apply(actual, checkState);
                            foreach (var (checkGroup, destination) in new[]
                            {
                                ("body", MotherBrainRainbowPaletteRomData.BodyColor),
                                ("backLegs", MotherBrainRainbowPaletteRomData.SecondaryColor),
                            })
                                for (int checkColor = 0; checkColor < 15; checkColor++)
                                {
                                    var expectedRgb = editedNode[checkGroup]![checkState]![checkColor]!;
                                    ushort expected = (ushort)(expectedRgb["red"]!.GetValue<int>() |
                                        expectedRgb["green"]!.GetValue<int>() << 5 |
                                        expectedRgb["blue"]!.GetValue<int>() << 10);
                                    AssertEqual(expected, actual.Colors[destination + checkColor],
                                        "stream 3 independent health palette edit and unaffected channels");
                                }
                        }
                    }
    }
    private static void VerifyStream3RecoveryLights(ISnesAddressSpace rom)
    {
        byte[] json = SuperMetroid.AssetExtraction.MotherBrainRoomColorExtractor.Extract(rom);
        var stock = MotherBrainRoomColorPresentation.Load(new MemoryStream(json));
        var node = System.Text.Json.Nodes.JsonNode.Parse(json)!;
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        object fade = typeof(MotherBrainRoomColorPresentation).GetField("recoveryLights", flags)!.GetValue(stock)!;
        AssertTrue(fade.GetType().GetField("supplied", flags)!.GetValue(fade) is null,
            "stream 3 stock recovery light rows discarded");
        AssertTrue(ReferenceEquals(fade.GetType().GetField("finalRoom", flags)!.GetValue(fade),
            typeof(MotherBrainRoomColorPresentation).GetField("finalRoom", flags)!.GetValue(stock)),
            "stream 3 recovery endpoint reuses the final room palette");
        AssertEqual(4, fade.GetType().GetFields(flags).Count(field => field.FieldType == typeof(ushort)),
            "stream 3 recovery keeps only four additional paint endpoints");        for (int frame = 0; frame < 7; frame++)
        {
            var actual = new SnesCgram();
            stock.ApplyRecoveryLights(actual, frame);
            for (int color = 0; color < 28; color++)
            {
                int address = 0xadf3d3 - frame * 0x38 + color * 2;
                ushort expected = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
                int destination = color < 14 ? MotherBrainRoomColorRomData.RecoveryLightsFirstColor + color :
                    MotherBrainRoomColorRomData.RecoveryLightsSecondColor + color - 14;
                AssertEqual(expected, actual.Colors[destination], "stream 3 all native recovery light words");
            }
        }
        for (int frame = 0; frame < 7; frame++)
            for (int color = 0; color < 28; color++)
                foreach (string component in new[] { "red", "green", "blue" })
                {
                    var editedNode = node.DeepClone();
                    var rgb = editedNode["recoveryLights"]![frame]![color]!;
                    rgb[component] = rgb[component]!.GetValue<int>() ^ 1;
                    var edited = MotherBrainRoomColorPresentation.Load(new MemoryStream(
                        System.Text.Encoding.UTF8.GetBytes(editedNode.ToJsonString())));
                    for (int checkFrame = 0; checkFrame < 7; checkFrame++)
                    {
                        var actual = new SnesCgram();
                        edited.ApplyRecoveryLights(actual, checkFrame);
                        for (int checkColor = 0; checkColor < 28; checkColor++)
                        {
                            var expectedRgb = editedNode["recoveryLights"]![checkFrame]![checkColor]!;
                            ushort expected = (ushort)(expectedRgb["red"]!.GetValue<int>() |
                                expectedRgb["green"]!.GetValue<int>() << 5 |
                                expectedRgb["blue"]!.GetValue<int>() << 10);
                            int destination = checkColor < 14 ? MotherBrainRoomColorRomData.RecoveryLightsFirstColor + checkColor :
                                MotherBrainRoomColorRomData.RecoveryLightsSecondColor + checkColor - 14;
                            AssertEqual(expected, actual.Colors[destination], "stream 3 independent recovery light edit");
                        }
                    }
                }
        for (int color = 0; color < 24; color++)
            foreach (string component in new[] { "red", "green", "blue" })
            {
                var editedNode = node.DeepClone();
                var rgb = editedNode["finalRoom"]![color]!;
                rgb[component] = rgb[component]!.GetValue<int>() ^ 1;
                var edited = MotherBrainRoomColorPresentation.Load(new MemoryStream(
                    System.Text.Encoding.UTF8.GetBytes(editedNode.ToJsonString())));
                for (int frame = 0; frame < 7; frame++)
                {
                    var expected = new SnesCgram();
                    var actual = new SnesCgram();
                    stock.ApplyRecoveryLights(expected, frame);
                    edited.ApplyRecoveryLights(actual, frame);
                    AssertTrue(expected.Colors.SequenceEqual(actual.Colors),
                        "stream 3 independent final-room edit cannot change recovery content");
                    for (int phase = 0; phase < 2; phase++)
                    {
                        ushort pointer = (ushort)(0xd046 + (frame * 2 + phase) * 4);
                        stock.ApplyFlash(expected, pointer);
                        edited.ApplyFlash(actual, pointer);
                        AssertTrue(expected.Colors.SequenceEqual(actual.Colors),
                            "stream 3 independent final-room edit cannot change flash content");
                    }
                }
            }
        var legacy = node.DeepClone();
        legacy["version"] = MotherBrainRoomColorFormat.PreRecoveryLightsVersion;
        legacy.AsObject().Remove("recoveryLights");
        var oldOverride = MotherBrainRoomColorPresentation.Load(new MemoryStream(
            System.Text.Encoding.UTF8.GetBytes(legacy.ToJsonString())), stock);
        for (int frame = 0; frame < 7; frame++)
        {
            var expected = new SnesCgram();
            var actual = new SnesCgram();
            stock.ApplyRecoveryLights(expected, frame);
            oldOverride.ApplyRecoveryLights(actual, frame);
            AssertTrue(expected.Colors.SequenceEqual(actual.Colors), "stream 3 legacy room override reuses calculated stock recovery");
        }
    }
    private static void VerifyStream3RoomFlash(ISnesAddressSpace rom)
    {
        byte[] json = SuperMetroid.AssetExtraction.MotherBrainRoomColorExtractor.Extract(rom);
        var stock = MotherBrainRoomColorPresentation.Load(new MemoryStream(json));
        var node = System.Text.Json.Nodes.JsonNode.Parse(json)!;
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        object flash = typeof(MotherBrainRoomColorPresentation).GetField("flash", flags)!.GetValue(stock)!;
        AssertTrue(flash.GetType().GetField("supplied", flags)!.GetValue(flash) is null,
            "stream 3 stock flash rows discarded");
        AssertTrue(ReferenceEquals(flash.GetType().GetField("basis", flags)!.GetValue(flash),
            typeof(MotherBrainRoomColorPresentation).GetField("finalRoom", flags)!.GetValue(stock)),
            "stream 3 stock flash reuses final room paint basis");
        for (int offset = 0; offset <= ushort.MaxValue; offset++)
        {
            bool mechanics = offset >= 0xd046 && offset < 0xd07e && (offset - 0xd046) % 4 < 2 ||
                offset is >= 0xd07e and < 0xd082;
            bool presentation = offset >= 0xd046 && offset < 0xd07e && (offset - 0xd046) % 4 >= 2;
            AssertEqual(mechanics, MotherBrainRoomPaletteProgramDefinitions.IsCompiledMechanicsByte(0xa90000 | offset),
                "stream 3 room palette mechanics byte ownership");
            AssertEqual(presentation, MotherBrainRoomPaletteProgramDefinitions.TryGetPresentationWord(0xa90000 | offset, out ushort word),
                "stream 3 room palette operand byte ownership");
            if (presentation) AssertEqual((ushort)(offset & 0xfffe), word, "stream 3 room palette canonical operand");
        }
        foreach (int invalid in new[] { -1, 14, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => _ = MotherBrainRoomPaletteProgramDefinitions.PresentationWordAddress(invalid),
                "stream 3 room palette operand index bounds");
        foreach (int invalid in new[] { -1, 16, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => _ = MotherBrainRoomPaletteProgramDefinitions.MechanicsWord(invalid),
                "stream 3 room palette mechanics index bounds");
        for (int frame = 0; frame < 14; frame++)
            for (int color = 0; color < 24; color++)
                foreach (string component in new[] { "red", "green", "blue" })
                {
                    var editedNode = node.DeepClone();
                    var rgb = editedNode["flash"]![frame]![color]!;
                    rgb[component] = rgb[component]!.GetValue<int>() ^ 1;
                    var edited = MotherBrainRoomColorPresentation.Load(new MemoryStream(
                        System.Text.Encoding.UTF8.GetBytes(editedNode.ToJsonString())));
                    for (int checkFrame = 0; checkFrame < 14; checkFrame++)
                    {
                        var actual = new SnesCgram();
                        edited.ApplyFlash(actual, (ushort)(0xd046 + checkFrame * 4));
                        for (int checkColor = 0; checkColor < 24; checkColor++)
                        {
                            var expectedRgb = editedNode["flash"]![checkFrame]![checkColor]!;
                            ushort expected = (ushort)(expectedRgb["red"]!.GetValue<int>() |
                                expectedRgb["green"]!.GetValue<int>() << 5 |
                                expectedRgb["blue"]!.GetValue<int>() << 10);
                            int destination = checkColor < 12 ? 0x34 + checkColor : 0x53 + checkColor - 12;
                            AssertEqual(expected, actual.Colors[destination], "stream 3 independent room flash edit");
                            if (checkColor >= 12)
                                AssertEqual(expected, actual.Colors[0x73 + checkColor - 12], "stream 3 edited room flash mirror");
                        }
                    }
                }
    }
    private static void ExecuteStream3DoorScroll(
        ISnesAddressSpace bus,
        ushort pointer,
        Span<byte> scrolls)
    {
        int pc = 0x8f0000 | pointer;
        bool accumulatorIsEightBit = false;
        ushort accumulator = 0;

        for (int instruction = 0; instruction < 32; instruction++)
        {
            byte opcode = bus.ReadByte(pc++);
            switch (opcode)
            {
                case 0x08: // PHP
                case 0x28: // PLP
                    break;

                case 0xe2: // SEP #$20
                    byte sepMask = bus.ReadByte(pc++);
                    if (sepMask != 0x20)
                        throw UnsupportedStream3DoorScroll(pointer, opcode, pc - 2);
                    accumulatorIsEightBit = true;
                    break;

                case 0xa9: // LDA immediate
                    accumulator = bus.ReadByte(pc++);
                    if (!accumulatorIsEightBit)
                        accumulator |= unchecked((ushort)(bus.ReadByte(pc++) << 8));
                    break;

                case 0x8f: // STA long
                    int destination = bus.ReadByte(pc) |
                        (bus.ReadByte(pc + 1) << 8) |
                        (bus.ReadByte(pc + 2) << 16);
                    pc += 3;
                    int storageIndex = destination - RoomScrollGrid.WorkRamAddress;
                    if ((uint)storageIndex >= RoomScrollGrid.StorageByteCount)
                        throw UnsupportedStream3DoorScroll(pointer, opcode, pc - 4);
                    scrolls[storageIndex] = unchecked((byte)accumulator);
                    if (!accumulatorIsEightBit)
                    {
                        if (storageIndex + 1 >= scrolls.Length)
                            throw UnsupportedStream3DoorScroll(pointer, opcode, pc - 4);
                        scrolls[storageIndex + 1] = unchecked((byte)(accumulator >> 8));
                    }
                    break;

                case 0x60: // RTS
                    return;

                default:
                    throw UnsupportedStream3DoorScroll(pointer, opcode, pc - 1);
            }
        }

        throw new InvalidDataException(
            $"Door callback $8F:{pointer:X4} did not return within 32 instructions.");
    }

    private static InvalidDataException UnsupportedStream3DoorScroll(
        ushort pointer,
        byte opcode,
        int opcodeAddress) =>
        new(
            $"Door callback $8F:{pointer:X4} uses unsupported reference-audit opcode " +
            $"${opcode:X2} at ${opcodeAddress >> 16:X2}:{opcodeAddress & 0xffff:X4}.");
}
