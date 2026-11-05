using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.BossDisplayBindingsDocumentsVerification;
using static SuperMetroid.Verification.Bosses.BossDisplayBindingsFixtureVerification;
using static SuperMetroid.Verification.Bosses.BossDisplayBindingsOracleVerification;
using static SuperMetroid.Verification.Bosses.BossDisplayBindingsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Enemies.Drawing;
namespace SuperMetroid.Verification.Bosses;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.World.Artwork;
using SuperMetroid.Core.Rooms.Artwork;

/// <summary>Checks stock boss display roots and installed BG2 bindings against cartridge artwork.</summary>
internal static class BossDisplayBindingsStockVerification
{
    /// <summary>
    /// Import-only oracle for the declared native roots. It never passes its ROM
    /// source to the production draw/collision/instruction paths.
    /// </summary>
    internal static void VerifyBossDisplayStock()
    {
        var source = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        InstalledEnemyArtwork installed = InstalledFixtures.EnemyArtwork;
        int frames = 0, parts = 0, runs = 0, tiles = 0;
        var nativeLists = new HashSet<int>();
        foreach ((EnemyDefinitionId definition, EnemySpritemapDefinition[] family) in BossDisplayDocuments.Families())
        {
            var fixture = new BossDisplayFixture(definition, installed);
            foreach (EnemySpritemapDefinition frame in family)
            {
                frames++;
                var native = ReadBossDisplayOracle(source, frame, nativeLists);
                AssertTrue((installed.Enemies.ExtendedFrames ?? throw new InvalidOperationException("Verification requires installed.ExtendedFrames.")).TryGetDisplay(frame.Bank, frame.Pointer, out var actual), "stock root is installed");
                AssertEqual(native.Oam.Length, actual.Length, frame.Name + " exact OAM component count");
                var expectedOam = new OamBuffer(); expectedOam.BeginFrame();
                for (int index = 0; index < native.Oam.Length; index++)
                {
                    EnemyExtendedDrawComponent original = native.Oam[index], imported = actual.Span[index];
                    AssertEqual(original.Offset.X, imported.Offset.X, frame.Name + " native component X");
                    AssertEqual(original.Offset.Y, imported.Offset.Y, frame.Name + " native component Y");
                    AssertTrue(original.Parts.SequenceEqual(imported.Parts), frame.Name + " exact native sprite records");
                    parts += original.Parts.Length;
                    expectedOam.AddEnemySpritemap(original.Parts,
                        new Position16<SpritePixels>(unchecked((ushort)(fixture.Actor.Position.X.Whole + original.Offset.X)), unchecked((ushort)(fixture.Actor.Position.Y.Whole + original.Offset.Y))),
                        NativeSpriteGraphicsWords.PaletteBits(fixture.Actor.PaletteRowIn(fixture.Enemies)), checked((ushort)fixture.Actor.NativeTileBaseIn(fixture.Enemies)),
                        clipVerticalWrap: true, originYIsOnScreen: (EightEight.WholeOf(fixture.Actor.Position.Y.Whole + original.Offset.Y)) == 0);
                }
                ReadOnlyMemory<EnemyBg2TilemapWrite> writes = StockBossWrites(installed, frame);
                AssertEqual(native.Bg2.Length, writes.Length, frame.Name + " exact BG2 run count");
                for (int index = 0; index < native.Bg2.Length; index++)
                {
                    AssertEqual(native.Bg2[index].FirstCell, writes.Span[index].FirstCell, frame.Name + " native BG2 destination/order");
                    AssertTrue(native.Bg2[index].Tiles.Span.SequenceEqual(writes.Span[index].Tiles.Span), frame.Name + " native BG2 payload/boundaries");
                    runs++; tiles += native.Bg2[index].Tiles.Length;
                }
                fixture.SetFrame(frame.Pointer, true); fixture.ClearBg2();
                OamBuffer rendered = fixture.Draw();
                AssertEqual(expectedOam.NextByteOffset, rendered.NextByteOffset, "stock packed sprite count");
                AssertTrue(expectedOam.ImageEquals(rendered),
                    "stock packed OAM matches independently decoded native records");
                AssertBossBg2(fixture, native.Bg2.Select(write => new EnemyBg2WriteDocument
                { X = write.FirstCell % EnemyBg2FrameLayout.TilemapWidth, Y = write.FirstCell / EnemyBg2FrameLayout.TilemapWidth,
                    Tiles = RoomBackgroundTilemapCellWords.Cells(write.Tiles.Span) }).ToArray());
            }
        }
        Console.WriteLine($"PASS stock boss import: {frames} native roots, {parts} sprites, {runs} BG2 runs/{tiles} words, " +
            $"{nativeLists.Count} compiled collision lists; exact RAM-only packed drawing.");
    }

    /// <summary>Resolves one Crocomire, Phantoon, or Draygon frame's installed BG2 writes.</summary>
    private static ReadOnlyMemory<EnemyBg2TilemapWrite> StockBossWrites(InstalledEnemyArtwork catalog, EnemySpritemapDefinition frame)
    {
        ReadOnlyMemory<EnemyBg2TilemapWrite> writes;
        bool exists = frame.Bank switch
        {
            CrocomireBg2FrameDefinitionsConstants.Bank => (catalog.Bosses.CrocomireBg2Frames ?? throw new InvalidOperationException("Verification requires catalog.CrocomireBg2Frames.")).TryGet(frame.Pointer, out writes),
            PhantoonBg2FrameDefinitionsConstants.Bank => (catalog.Bosses.PhantoonBg2Frames ?? throw new InvalidOperationException("Verification requires catalog.PhantoonBg2Frames.")).TryGet(frame.Pointer, out writes),
            DraygonBg2FrameToolingDefinitions.Bank => (catalog.Bosses.DraygonBg2Frames ?? throw new InvalidOperationException("Verification requires catalog.DraygonBg2Frames.")).TryGet(frame.Pointer, out writes),
            _ => throw new InvalidDataException("Unknown stock fixture family."),
        };
        return exists ? writes : ReadOnlyMemory<EnemyBg2TilemapWrite>.Empty;
    }

    /// <summary>Runs the stock boss OAM and BG2 checks.</summary>
    [VerificationCommand("--boss-display-stock")]
    internal static int BossDisplayStockCommand()
    {
        Suite(nameof(VerifyBossDisplayStock), () => VerifyBossDisplayStock());
        return 0;
    }
}
