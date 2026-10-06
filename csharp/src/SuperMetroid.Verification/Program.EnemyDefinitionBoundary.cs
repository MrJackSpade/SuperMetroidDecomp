using System.Reflection;
using System.Text;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Verifies only the already statically identified enemy-reader conversions; no gameplay read discovery.</summary>
    private static void VerifyEnemyDefinitionBoundary(string sourceRom)
    {
        var source = CartridgeImportAddressSpace.LoadRetailRom(sourceRom);
        VerifyEnemyMappedSourceRouting();
        VerifyCompleteTorizoDefinitions(source);
        VerifyCorpseMetadataDefinitions(source);
        VerifyCrocomirePowerBombReactionDefinitions(source);
        VerifyEnemyAuxiliaryColors(source);
        using var temporary = new MapCatalogTestDirectory();
        EnemyTileArtworkCatalog artwork = GameAssetInstaller.Install(sourceRom, temporary.Root).LoadEnemyTiles();
        AssertTrue(artwork.AuxiliaryColors is not null, "complete installation supplies auxiliary palettes");
        var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
        int drawCases = 0;
        foreach (ushort frame in TorizoCollisionDefinitions.FramePointers)
        foreach ((ushort x, ushort y) in new (ushort, ushort)[] { (128, 96), (0, 0), (511, 252) })
        {
            AssertTrue(artwork.ExtendedFrames!.TryGet(TorizoInstructionProgramDefinitions.Bank, frame, out _),
                $"installed Torizo extended frame {frame:X4}");
            OamBuffer actual = DrawExtendedForBank(artwork, memory, TorizoInstructionProgramDefinitions.Bank, frame, x, y);
            var expected = new OamBuffer();
            int count = NativeWord(source, 0xaa0000 | frame);
            for (int index = 0; index < count; index++)
            {
                int component = 0xaa0000 | (frame + 2 + index * 8);
                ushort cx = unchecked((ushort)(x + (short)NativeWord(source, component)));
                ushort cy = unchecked((ushort)(y + (short)NativeWord(source, component + 2)));
                if (((cx + 128) & 0xfe00) != 0 || ((cy + 128) & 0xfe00) != 0)
                    continue;
                DrawImportedEnemySpritemap(source, expected, TorizoInstructionProgramDefinitions.Bank,
                    NativeWord(source, component + 4), cx, cy, 0, 0,
                    clipVerticalWrap: true, originYIsOnScreen: (cy >> 8) == 0);
            }
            AssertEqual(expected.NextByteOffset, actual.NextByteOffset, "Torizo composed OAM count");
            AssertTrue(expected.LowTable.SequenceEqual(actual.LowTable) &&
                expected.HighTable.SequenceEqual(actual.HighTable), "Torizo composed OAM bytes and origin arithmetic");
            drawCases++;
        }
        AssertThrows<InvalidDataException>(() => DrawExtendedForBank(null, memory,
                TorizoInstructionProgramDefinitions.Bank, BombTorizoDormantFrameDefinitions.Frame, 128, 96),
            "missing enemy artwork cannot recover an address decoder");
        Console.WriteLine($"Enemy definitions: all 1,761 Torizo mechanics words, 564 selectors, 106 physical frames, corpse metadata, 50 Crocomire reactions, 393 editable palette colors and {drawCases} composed OAM cases pass.");
    }

    private static void VerifyCompleteTorizoDefinitions(ISnesAddressSpace source)
    {
        AssertEqual(1761, TorizoInstructionProgramDefinitions.MechanicsWordCount, "complete Torizo mechanics count");
        AssertEqual(564, TorizoInstructionProgramDefinitions.PresentationWordCount, "complete Torizo visual operand count");
        var enemies = new RoomEnemySystem();
        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
        MethodInfo read = typeof(RoomEnemySystem).GetMethod("ReadEnemyInstructionMechanicsWord", flags)!;
        ushort previous = 0;
        for (int index = 0; index < TorizoInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            TorizoMechanicsWord word = TorizoInstructionProgramDefinitions.MechanicsWord(index);
            AssertTrue(word.Address > previous, "Torizo mechanics are ordered and unique");
            previous = word.Address;
            AssertEqual(NativeWord(source, 0xaa0000 | word.Address), word.Value, $"Torizo word {word.Address:X4}");
            foreach (ushort pointer in new[] { RoomEnemySystem.BombTorizoDefinition, RoomEnemySystem.GoldenTorizoDefinition })
            {
                RoomEnemySlot slot = enemies.Slots[0];
                slot.EnemyDefinitionPointer = pointer;
                AssertEqual(word.Value, (ushort)read.Invoke(enemies, [slot, word.Address])!,
                    "both Torizo interpreters resolve compiled words without a bus");
            }
        }
        for (int index = 0; index < TorizoInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            ushort operand = TorizoInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertTrue(!TorizoInstructionProgramDefinitions.TryReadMechanicsWord(operand, out _),
                "Torizo visual operands cannot be read as mechanics");
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(TorizoInstructionProgramDefinitions.Bank, operand, out ushort pointer),
                "every Torizo sprite selector is compiled");
            AssertEqual(NativeWord(source, 0xaa0000 | operand), pointer, "Torizo selector matches pinned source");
        }
        AssertEqual(106, TorizoCollisionDefinitions.FrameCount, "complete Torizo physical frame count");
        foreach (ushort frame in TorizoCollisionDefinitions.FramePointers)
        {
            TorizoCollisionComponents components = TorizoCollisionDefinitions.ComponentsAt(frame);
            AssertEqual(NativeWord(source, 0xaa0000 | frame), components.Length, "Torizo component count");
            for (int i = 0; i < components.Length; i++)
            {
                int address = 0xaa0000 | (frame + 2 + i * 8);
                AssertEqual(unchecked((short)NativeWord(source, address)), components[i].X, "Torizo physical X");
                AssertEqual(unchecked((short)NativeWord(source, address + 2)), components[i].Y, "Torizo physical Y");
                AssertEqual(NativeWord(source, address + 6), components[i].HitboxList, "Torizo hitbox identity");
            }
        }
        foreach (ushort pointer in TorizoCollisionDefinitions.HitboxPointers)
        {
            ReadOnlySpan<GoldenTorizoCollisionHitbox> boxes = TorizoCollisionDefinitions.HitboxesAt(pointer);
            AssertEqual(NativeWord(source, 0xaa0000 | pointer), boxes.Length, "Torizo rectangle count");
            for (int i = 0; i < boxes.Length; i++)
            {
                int address = 0xaa0000 | (pointer + 2 + i * 12);
                var box = boxes[i];
                ushort[] words = [unchecked((ushort)box.Left), unchecked((ushort)box.Top),
                    unchecked((ushort)box.Right), unchecked((ushort)box.Bottom), box.TouchAi, box.ShotAi];
                for (int w = 0; w < words.Length; w++)
                    AssertEqual(NativeWord(source, address + w * 2), words[w], "Torizo rectangle/callback word");
            }
        }
    }

    private static void VerifyCorpseMetadataDefinitions(ISnesAddressSpace source)
    {
        foreach ((ushort pointer, int count) in new (ushort, int)[]
            { (0xe226, 13), (0xe240, 6), (0xe24c, 3), (0xe252, 3), (0xe258, 5) })
            for (int i = 0; i < count; i++)
                AssertEqual(NativeWord(source, 0xa90000 | (pointer + i * 2)),
                    DeadMonsterRottingDefinitions.RotationOffset(pointer, (ushort)(i * 8)), "corpse rotation offset");
        foreach (ushort pointer in new ushort[] { 0xe0e0, 0xe10a, 0xe134, 0xe146, 0xe158, 0xe16a, 0xe17c, 0xe18e, 0xe1b0, 0xe1d2 })
        {
            var records = DeadMonsterRottingDefinitions.ForTransferTable(pointer);
            for (int i = 0; i < records.Length; i++)
            {
                var record = records[i];
                ushort[] words = [record.SizeInBytes, record.SourceBankWord, record.SourceOffset, record.EncodedVramDestination];
                for (int w = 0; w < words.Length; w++)
                    AssertEqual(NativeWord(source, 0xa90000 | (pointer + i * 8 + w * 2)), words[w], "corpse transfer descriptor");
            }
            AssertEqual((ushort)0, NativeWord(source, 0xa90000 | (pointer + records.Length * 8)), "corpse descriptor terminator");
        }
        var boxes = DeadMonsterRottingDefinitions.TorizoTouchHitboxes;
        AssertEqual(NativeWord(source, 0xa9d77c), boxes.Length, "dead Torizo asymmetric rectangle count");
        for (int i = 0; i < boxes.Length; i++)
        {
            ushort[] words = [boxes[i].Left, boxes[i].Top, boxes[i].Right, boxes[i].Bottom];
            for (int w = 0; w < words.Length; w++)
                AssertEqual(NativeWord(source, 0xa9d77e + i * 8 + w * 2), words[w], "dead Torizo asymmetric word");
        }
        for (ushort i = 0; i < 16; i++)
        {
            AssertEqual(NativeWord(source, 0xa9d67c + i * 2), DeadMonsterRottingDefinitions.SandDestination(i), "corpse sand destination");
            AssertEqual(NativeWord(source, 0xa9d69c + i * 2), DeadMonsterRottingDefinitions.SandSource(i), "corpse sand source");
        }
    }

    private static void VerifyCrocomirePowerBombReactionDefinitions(ISnesAddressSpace source)
    {
        int tested = 0;
        foreach (ushort frame in CrocomireBodyVisualDefinitions.Frames)
        {
            ushort expected = CrocomireInstructionProgramDefinitions.PowerBombReactionMouthNotOpen;
            int count = NativeWord(source, 0xa40000 | frame);
            for (int i = 0; i < count; i++)
            {
                ushort sprite = NativeWord(source, 0xa40000 | (frame + 6 + i * 8));
                if (sprite == 0xd600) { expected = CrocomireInstructionProgramDefinitions.PowerBombReactionMouthFullyOpen; break; }
                if (sprite == 0xd51c) { expected = CrocomireInstructionProgramDefinitions.PowerBombReactionMouthPartiallyOpen; break; }
            }
            AssertEqual(expected, CrocomirePowerBombReactionDefinitions.ForFrame(frame), "Crocomire mechanical mouth admission");
            tested++;
        }
        AssertEqual(50, tested, "all Crocomire reaction frames");
    }

    private static void VerifyEnemyAuxiliaryColors(ISnesAddressSpace source)
    {
        byte[] json = EnemyAuxiliaryColorFiles.Extract(source);
        EnemyAuxiliaryColorCatalog catalog = EnemyAuxiliaryColorCatalog.Load(new MemoryStream(json, writable: false));
        foreach (var definition in EnemyAuxiliaryColorDefinitions.All)
            for (int frame = 0; frame < definition.FrameCount; frame++)
                for (int color = 0; color < definition.ColorCount; color++)
                    AssertEqual((ushort)(NativeWord(source, definition.SourceAddress +
                        (frame * definition.NativeFrameStrideColors + color) * 2) & 0x7fff),
                        catalog.Resolve(definition.Id, frame, color), "editable auxiliary RGB5 color");
        var document = JsonSerializer.Deserialize<EnemyAuxiliaryColorDocument>(json,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter<EnemyAuxiliaryPalette>() } })!;
        document.Palettes[EnemyAuxiliaryPalette.FaceBlock][0][0] = new PaletteRgb5 { Red = 31, Green = 0, Blue = 0 };
        EnemyAuxiliaryColorCatalog edited = EnemyAuxiliaryColorCatalog.Load(
            new MemoryStream(EnemyAuxiliaryColorCatalog.Write(document), writable: false));
        AssertEqual((ushort)31, edited.Resolve(EnemyAuxiliaryPalette.FaceBlock, 0, 0), "auxiliary art override survives compilation");
        document.Palettes[EnemyAuxiliaryPalette.FaceBlock][0][0] = new PaletteRgb5 { Red = 32, Green = 0, Blue = 0 };
        AssertThrows<InvalidDataException>(() => EnemyAuxiliaryColorCatalog.Write(document), "out-of-range RGB5 rejected");
        string text = Encoding.UTF8.GetString(json);
        AssertThrows<InvalidDataException>(() => EnemyAuxiliaryColorCatalog.Load(new MemoryStream(
            Encoding.UTF8.GetBytes(text.Replace("\"version\": 1", "\"version\": 1, \"version\": 1", StringComparison.Ordinal)))),
            "duplicate palette JSON property rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => catalog.Resolve(EnemyAuxiliaryPalette.FaceBlock, 8, 0), "palette frame bounded");
    }

    private static ushort NativeWord(ISnesAddressSpace source, int address) =>
        (ushort)(source.ReadCartridgeByte(address) |
            source.ReadCartridgeByte((address & 0xff0000) | ((address + 1) & 0xffff)) << 8);
}
