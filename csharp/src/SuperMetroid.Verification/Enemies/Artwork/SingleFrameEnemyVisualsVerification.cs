using SuperMetroid.Core;
using SuperMetroid.Tooling;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Enemies.Artwork.CompiledEnemyVisualSelectorAssertionsVerification;
using static SuperMetroid.Verification.Enemies.Polyp.PolypRockInstructionProgramDefinitionsVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Kzan;
using SuperMetroid.Core.Enemies.Polyp;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.AssetExtraction.Enemies;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Checks installed Kzan, Polyp, and Polyp-rock artwork and older catalog override migration.</summary>
internal static class SingleFrameEnemyVisualsVerification
{
    /// <summary>
    /// Checks the single-operand omissions identified in #1166, including OAM compositions
    /// consumed immediately after instruction selection and edits preserved from older schemas.
    /// </summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static void VerifySingleFrameEnemyVisuals()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        byte[] stockBytes = EnemySpritemapFiles.Extract(rom);
        var stock = EnemySpritemapCatalog.Load(new MemoryStream(stockBytes));
        var document = RequiredJson.Deserialize<EnemySpritemapDocument>(stockBytes, options);
        EnemySpritemapDefinition[] previous = EnemySpritemapDefinitions.Frames
            .Take(EnemySpritemapDefinitions.PreSingleFrameFrameCount).ToArray();
        string edited = previous[0].Name;
        var legacy = document with
        {
            Version = (int)EnemySpritemapSchema.PreSingleFrame,
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
            TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>(), Spritemaps = merged }),
        };
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
            enemies.Presentation.DrawEnemySpritemap(actual, new EnemySpritemapDraw { Bank = definition.Bank, Pointer = definition.Pointer, Origin = new Position16<OamPixels>((ushort)128, (ushort)112), PaletteBits = 0x0c00, BaseTileIndex = 0x20, ClipVerticalWrap = false, OriginYIsOnScreen = true });
            var expected = new OamBuffer();
            ImportedSpritemapOracle.DrawEnemy(rom, expected, new EnemySpritemapDraw { Bank = definition.Bank, Pointer = definition.Pointer, Origin = new(128, 112), PaletteBits = 0x0c00, BaseTileIndex = 0x20 });
            CompareOam(expected, actual, definition.Name);
        }

        byte[] projectileBytes = EnemyProjectileSpritemapFiles.Extract(rom);
        var projectileStock = EnemyProjectileSpritemapCatalog.Load(new MemoryStream(projectileBytes));
        var projectileDocument = RequiredJson.Deserialize<EnemyProjectileSpritemapDocument>(projectileBytes, options);
        var projectileLegacy = projectileDocument with
        {
            Version = (int)EnemyProjectileSpritemapVersion.PrePolypRock,
            ProgramFrames = EnemyProjectilePresentationFrameDefinitions.PrePolypRock
                .ToDictionary(frame => frame.Name, frame => (projectileDocument.ProgramFrames ?? throw new InvalidOperationException("Verification requires projectileDocument.ProgramFrames."))[frame.Name]),
        };
        string editedProjectile = EnemyProjectilePresentationFrameDefinitions.PrePolypRock[0].Name;
        projectileLegacy.ProgramFrames[editedProjectile] =
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
        rockActual.AddEnemySpritemap(rockParts.Span, new Position16<OamPixels>(128, 112), 0x0c00, 0x20);
        ushort rockPointer = ReadPolypRockInstructionWord(rom, PolypRockInstructionProgramDefinitions.PresentationWord);
        var rockExpected = new OamBuffer();
        ImportedSpritemapOracle.DrawEnemy(rom, rockExpected, new EnemySpritemapDraw { Bank = 0x8d, Pointer = rockPointer, Origin = new(128, 112), PaletteBits = 0x0c00, BaseTileIndex = 0x20 });
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
