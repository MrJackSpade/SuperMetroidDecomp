using System.Reflection;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyRidleyBreakupPrograms()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, new SlopeHeightNoReadBus());
        typeof(RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(enemies, (Func<ushort>)(() => 1));
        var death = typeof(RoomEnemySystem).GetMethod("TickNorfairRidleyDeathExplosions", flags)!
            .CreateDelegate<Action<RoomEnemySlot, RidleyEnemyState, SamusState?>>(enemies);
        var process = typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!
            .CreateDelegate<Action<RoomEnemySlot, SamusState?, RoomLevelData?, ushort, ushort, ushort, byte>>(enemies);
        var body = enemies.Slots[0];
        body.EnemyDefinitionPointer = RoomEnemySystem.NorfairRidleyDefinition;
        var state = new RidleyEnemyState
        {
            GrabState = 1, FunctionTimer = 0,
            TailSegments = Enumerable.Range(0, 7).Select(index => new RidleyTailSegment
                { Angle = index == 6 ? (ushort)0x10 : (ushort)0, XPosition = 128, YPosition = 128 }).ToArray(),
        };
        death(body, state, new SamusState());
        AssertEqual((ushort)0, state.GrabState, "Ridley death releases Samus before breakup");
        var fragments = enemies.Slots.Where(slot => slot.EnemyDefinitionPointer == RidleyExplosionDefinitions.EnemyDefinition).ToArray();
        AssertEqual(12, fragments.Length, "Ridley death spawns all twelve body/tail fragments");
        AssertEqual((ushort)0xca9b, fragments[0].CurrentInstruction, "reported tail orientation selects CA9B");
        foreach (var fragment in fragments)
            process(fragment, null, null, 0, 0, 0, 0);
        var rom = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        byte[] stockBytes = EnemySpritemapFiles.Extract(rom);
        var stock = EnemySpritemapCatalog.Load(new MemoryStream(stockBytes));
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var document = JsonSerializer.Deserialize<EnemySpritemapDocument>(stockBytes, options)!;
        var previous = EnemySpritemapDefinitions.Frames.ToArray().Take(EnemySpritemapDefinitions.PreRidleyBreakupFrameCount).ToArray();
        var legacy = document with
        {
            Version = EnemySpritemapDefinitions.PreRidleyBreakupVersion,
            Frames = previous.ToDictionary(frame => frame.Name, frame => document.Frames[frame.Name]),
            DisplayFrames = previous.ToDictionary(frame => frame.Name, frame => frame.Name),
        };
        string edited = previous[0].Name;
        legacy.Frames[edited] = [legacy.Frames[edited][0] with { OffsetX = -11 }];
        byte[] legacyBytes = JsonSerializer.SerializeToUtf8Bytes(legacy, options);
        var merged = EnemySpritemapCatalog.Load(new MemoryStream(legacyBytes), stock);
        AssertTrue(merged.TryGetDisplay(previous[0].Bank, previous[0].Pointer, out var parts), "schema-68 override retains existing frame");
        AssertEqual(-11, parts[0].X.SignedOffset, "schema-68 edit survives stock refresh");
        AssertThrows<InvalidDataException>(() => EnemySpritemapCatalog.Load(new MemoryStream(legacyBytes)), "schema-68 incomplete stock requires refresh");
        enemies.TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(
            new Dictionary<ushort, RoomCharacterAtlas>(), new Dictionary<ushort, EnemyPaletteSheet>(), spritemaps: merged);
        var draw = typeof(RoomEnemySystem).GetMethod("DrawEnemySpritemap", flags)!;
        for (int index = 0; index < RidleyExplosionInstructionProgramDefinitions.ProgramCount; index++)
        {
            ushort operand = RidleyExplosionInstructionProgramDefinitions.PresentationWordAddress(index);
            ushort start = (ushort)(operand - 2);
            ushort expectedPointer = (ushort)(rom.ReadByte(0xa60000 | operand) | rom.ReadByte(0xa60000 | (operand + 1)) << 8);
            foreach (int offset in new[] { 0, 4 })
            {
                ushort address = (ushort)(start + offset);
                ushort native = (ushort)(rom.ReadByte(0xa60000 | address) | rom.ReadByte(0xa60000 | (address + 1)) << 8);
                AssertEqual(native, RidleyExplosionInstructionProgramDefinitions.ReadMechanicsWord(address), "breakup native mechanics");
            }
            var fragment = fragments[0];
            fragment.CurrentInstruction = start;
            fragment.InstructionTimer = 1;
            process(fragment, null, null, 0, 0, 0, 0);
            AssertEqual(expectedPointer, fragment.SpritemapPointer, "breakup selects native sprite");
            process(fragment, null, null, 0, 0, 0, 0);
            AssertEqual((ushort)(start + 4), fragment.CurrentInstruction, "breakup sleeps after its one frame");
            var actual = new OamBuffer();
            draw.Invoke(enemies, [actual, (byte)0xa6, expectedPointer, (ushort)128, (ushort)112, (ushort)0x0e00, (ushort)0, false, true]);
            var expected = new OamBuffer();
            ImportedSpritemapOracle.DrawEnemy(rom, expected, 0xa6, expectedPointer, 128, 112, 0x0e00, 0);
            AssertEqual(expected.NextByteOffset, actual.NextByteOffset, "breakup OAM count");
            AssertTrue(expected.LowTable.SequenceEqual(actual.LowTable), "breakup exact OAM bytes");
            AssertTrue(expected.HighTable.SequenceEqual(actual.HighTable), "breakup exact OAM size/X bits");
        }
        Console.WriteLine("Ridley breakup: death while grabbing releases Samus and runs all twelve fragment programs without cartridge access.");
        Console.WriteLine("All 29 breakup programs match native mechanics, frame selection, sleep and installed OAM.");
    }
}
