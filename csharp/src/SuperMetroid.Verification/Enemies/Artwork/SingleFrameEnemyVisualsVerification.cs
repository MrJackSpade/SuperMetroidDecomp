using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.AssetExtraction.World.Artwork;
using SuperMetroid.Core.Enemies.Polyp.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Kzan;
using SuperMetroid.Core.Enemies.Polyp;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Checks installed Kzan, Polyp, and Polyp-rock artwork.</summary>
internal static class SingleFrameEnemyVisualsVerification
{
    /// <summary>
    /// Checks the single-operand omissions identified in #1166, including OAM compositions
    /// consumed immediately after instruction selection.
    /// </summary>
    internal static void VerifySingleFrameEnemyVisuals()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        byte[] stockBytes = EnemySpritemapFiles.Extract(rom);
        var stock = EnemySpritemapDocuments.Load(new MemoryStream(stockBytes));
        foreach (var (definition, spritemap, count) in new[]
        {
            new VerifySingleFrameEnemyVisualsCase(SingleFrameEnemyVisualDefinitions.Kzan, KzanAnimationsConstants.Spritemaps(KzanProgram.Idle).Single(), 4),
            new VerifySingleFrameEnemyVisualsCase(SingleFrameEnemyVisualDefinitions.Polyp, PolypAnimationsConstants.Spritemaps(PolypProgram.Stationary).Single(), 1),
        })
        {
            AssertEqual(definition.Pointer, spritemap, definition.Name + " is the frame its typed program shows");
            AssertTrue(stock.TryGetDisplay(definition.Bank, definition.Pointer, out var parts),
                definition.Name + " is installed");
            AssertEqual(count, parts.Length, definition.Name + " native sprite count");
            var actual = new OamBuffer();
            stock.DrawEnemySpritemap(actual, new NativeEnemySpritemapDraw { Bank = definition.Bank, Pointer = definition.Pointer, Origin = new Position16<SpritePixels>(128, 112), PaletteBits = 0x0c00, BaseTileIndex = 0x20, ClipVerticalWrap = false, OriginYIsOnScreen = true });
            var expected = new OamBuffer();
            ImportedSpritemapOracle.DrawEnemy(rom, expected, new NativeEnemySpritemapDraw { Bank = definition.Bank, Pointer = definition.Pointer, Origin = new(128, 112), PaletteBits = 0x0c00, BaseTileIndex = 0x20 });
            CompareOam(expected, actual, definition.Name);
        }

        byte[] projectileBytes = EnemyProjectileSpritemapFiles.Extract(rom);
        var projectileStock = EnemyProjectileSpritemapDocuments.Load(new MemoryStream(projectileBytes));
        var rockParts = projectileStock.GetProgramFrame(EnemyProjectileFrameId.PolypRockInitial);
        var rockActual = new OamBuffer();
        rockActual.AddEnemySpritemap(rockParts.Span, new Position16<SpritePixels>(128, 112), 0x0c00, 0x20);
        ushort rockPointer = PolypRockFrameToolingDefinitions.Spritemap(EnemyProjectileFrameId.PolypRockInitial);
        var rockExpected = new OamBuffer();
        ImportedSpritemapOracle.DrawEnemy(rom, rockExpected, new NativeEnemySpritemapDraw { Bank = 0x8d, Pointer = rockPointer, Origin = new(128, 112), PaletteBits = 0x0c00, BaseTileIndex = 0x20 });
        CompareOam(rockExpected, rockActual, "Polyp rock");
        Console.WriteLine("Single-frame artwork: Kzan, Polyp and lava-rock OAM match the cartridge.");

        static void CompareOam(OamBuffer expected, OamBuffer actual, string name)
        {
            AssertTrue(expected.NextByteOffset > 0, name + " reference is visible");
            AssertEqual(expected.NextByteOffset, actual.NextByteOffset, name + " OAM part count");
            AssertTrue(expected.LowTable.SequenceEqual(actual.LowTable), name + " exact OAM low bytes");
            AssertTrue(expected.HighTable.SequenceEqual(actual.HighTable), name + " exact OAM size/X bits");
        }
    }
}
