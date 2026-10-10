using System.Reflection;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    // #1166: confirm the statically identified single-operand omissions, including
    // the installed compositions consumed immediately after instruction selection.
    /// <summary>Verifies cartridge-matching enemy and projectile OAM for the Kzan, Polyp, and lava rock compositions, while checking that prior-schema edits merge onto new stock data and obsolete stock requests reimport.</summary>
    private static void VerifySingleFrameEnemyVisuals()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        byte[] stockBytes = EnemySpritemapFiles.Extract(rom);
        var stock = EnemySpritemapCatalog.Load(new MemoryStream(stockBytes));
        var document = JsonSerializer.Deserialize<EnemySpritemapDocument>(stockBytes, options)!;
        EnemySpritemapDefinition[] previous = EnemySpritemapDefinitions.Frames
            .ToArray().Take(EnemySpritemapDefinitions.PreSingleFrameFrameCount).ToArray();
        string edited = previous[0].Name;
        var legacy = document with
        {
            Version = EnemySpritemapDefinitions.PreSingleFrameVersion,
            Frames = previous.ToDictionary(frame => frame.Name, frame => document.Frames[frame.Name]),
            DisplayFrames = previous.ToDictionary(frame => frame.Name, frame => frame.Name),
        };
        legacy.Frames[edited] = [legacy.Frames[edited][0] with { OffsetX = -11 }];
        byte[] legacyBytes = JsonSerializer.SerializeToUtf8Bytes(legacy, options);
        var merged = EnemySpritemapCatalog.Load(new MemoryStream(legacyBytes), stock);
        AssertTrue(merged.TryGetDisplay(previous[0].Bank, previous[0].Pointer, out var editedParts),
            "schema-67 override retains its existing frame");
        AssertEqual(-11, editedParts[0].X.SignedOffset, "schema-67 edit survives new stock");
        AssertThrows<InvalidDataException>(() => EnemySpritemapCatalog.Load(new MemoryStream(legacyBytes)),
            "old stock requests reimport instead of omitting the new compositions");

        var enemies = new RoomEnemySystem
        {
            TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(
                new Dictionary<ushort, RoomCharacterAtlas>(), new Dictionary<ushort, EnemyPaletteSheet>(),
                spritemaps: merged),
        };
        MethodInfo draw = typeof(RoomEnemySystem).GetMethod("DrawEnemySpritemap",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        foreach (var (definition, operand, count) in new[]
        {
            (SingleFrameEnemyVisualDefinitions.Kzan, KzanInstructionProgramDefinitionsTooling.PresentationWord, 4),
            (SingleFrameEnemyVisualDefinitions.Polyp, PolypInstructionProgramDefinitions.PresentationWord, 1),
        })
        {
            AssertCompiledEnemyVisualSelector(rom, definition.Bank, operand, definition.Name);
            AssertTrue(merged.TryGetDisplay(definition.Bank, definition.Pointer, out var parts),
                definition.Name + " inherits installed stock");
            AssertEqual(count, parts.Length, definition.Name + " native sprite count");
            var actual = new OamBuffer();
            draw.Invoke(enemies, [actual, definition.Bank, definition.Pointer,
                (ushort)128, (ushort)112, (ushort)0x0c00, (ushort)0x20, false, true]);
            var expected = new OamBuffer();
            ImportedSpritemapOracle.DrawEnemy(rom, expected, definition.Bank, definition.Pointer,
                128, 112, 0x0c00, 0x20);
            CompareOam(expected, actual, definition.Name);
        }

        byte[] projectileBytes = EnemyProjectileSpritemapFiles.Extract(rom);
        var projectileStock = EnemyProjectileSpritemapCatalog.Load(new MemoryStream(projectileBytes));
        var projectileDocument = JsonSerializer.Deserialize<EnemyProjectileSpritemapDocument>(projectileBytes, options)!;
        var projectileLegacy = projectileDocument with
        {
            Version = EnemyProjectileSpritemapDefinitions.PrePolypRockVersion,
            ProgramFrames = EnemyProjectilePresentationFrameDefinitions.PrePolypRock.ToArray()
                .ToDictionary(frame => frame.Name, frame => projectileDocument.ProgramFrames![frame.Name]),
        };
        string editedProjectile = EnemyProjectilePresentationFrameDefinitions.PrePolypRock[0].Name;
        projectileLegacy.ProgramFrames![editedProjectile] =
            [projectileLegacy.ProgramFrames[editedProjectile][0] with { OffsetX = -12 }];
        byte[] projectileLegacyBytes = JsonSerializer.SerializeToUtf8Bytes(projectileLegacy, options);
        var projectileMerged = EnemyProjectileSpritemapCatalog.Load(new MemoryStream(projectileLegacyBytes), projectileStock);
        AssertEqual(-12, projectileMerged.GetProgramFrame(
            EnemyProjectilePresentationFrameDefinitions.PrePolypRock[0].OperandAddress).Span[0].X.SignedOffset,
            "schema-12 projectile edit survives new stock");
        AssertThrows<InvalidDataException>(() => EnemyProjectileSpritemapCatalog.Load(new MemoryStream(projectileLegacyBytes)),
            "old projectile stock requests reimport");
        var rockParts = projectileMerged.GetProgramFrame(PolypRockInstructionProgramDefinitions.PresentationWord);
        var rockActual = new OamBuffer();
        rockActual.AddEnemySpritemap(rockParts.Span, 128, 112, 0x0c00, 0x20);
        ushort rockPointer = ReadPolypRockInstructionWord(rom, PolypRockInstructionProgramDefinitions.PresentationWord);
        var rockExpected = new OamBuffer();
        ImportedSpritemapOracle.DrawEnemy(rom, rockExpected, 0x8d, rockPointer, 128, 112, 0x0c00, 0x20);
        CompareOam(rockExpected, rockActual, "Polyp rock");
        Console.WriteLine("Single-frame artwork: Kzan, Polyp and lava-rock OAM match the cartridge; prior overrides retain edits and inherit new frames.");

        static void CompareOam(OamBuffer expected, OamBuffer actual, string name)
        {
            AssertTrue(expected.NextByteOffset > 0, name + " reference is visible");
            AssertEqual(expected.NextByteOffset, actual.NextByteOffset, name + " OAM part count");
            AssertTrue(expected.LowTable.SequenceEqual(actual.LowTable), name + " exact OAM low bytes");
            AssertTrue(expected.HighTable.SequenceEqual(actual.HighTable), name + " exact OAM size/X bits");
        }
    }
}
