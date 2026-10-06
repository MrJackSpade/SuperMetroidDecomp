using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Import-only oracle for the declared native roots. It never passes its ROM
    /// source to the production draw/collision/instruction paths.
    /// </summary>
    private static void VerifyBossDisplayStock()
    {
        var source = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        using var temporary = new MapCatalogTestDirectory();
        string path = Path.Combine(temporary.Root, "stock"), overrides = Path.Combine(temporary.Root, "overrides");
        EnemyTileArtworkFiles.Extract(source, path, SupportedCartridge.Sha256);
        EnemyTileArtworkFiles.ValidateStock(path);
        EnemyTileArtworkCatalog installed = EnemyTileArtworkFiles.Load(path, null);
        int frames = 0, parts = 0, runs = 0, tiles = 0;
        var nativeLists = new HashSet<int>();
        foreach ((ushort definition, EnemyExtendedFrameDefinition[] family) in BossDisplayDocuments.Families())
        {
            var fixture = new BossDisplayFixture(definition, installed);
            foreach (EnemyExtendedFrameDefinition frame in family)
            {
                frames++;
                var native = ReadBossDisplayOracle(source, frame, nativeLists);
                AssertTrue(installed.ExtendedFrames!.TryGetDisplay(frame.Bank, frame.Pointer, out var actual), "stock root is installed");
                AssertEqual(native.Oam.Length, actual.Length, frame.Name + " exact OAM component count");
                var expectedOam = new OamBuffer(); expectedOam.BeginFrame();
                for (int index = 0; index < native.Oam.Length; index++)
                {
                    EnemyExtendedDrawComponent original = native.Oam[index], imported = actual.Span[index];
                    AssertEqual(original.OffsetX, imported.OffsetX, frame.Name + " native component X");
                    AssertEqual(original.OffsetY, imported.OffsetY, frame.Name + " native component Y");
                    AssertTrue(original.Parts.SequenceEqual(imported.Parts), frame.Name + " exact native sprite records");
                    parts += original.Parts.Length;
                    expectedOam.AddEnemySpritemap(original.Parts,
                        unchecked((ushort)(fixture.Actor.XPosition + original.OffsetX)),
                        unchecked((ushort)(fixture.Actor.YPosition + original.OffsetY)), 0, 0,
                        clipVerticalWrap: true, originYIsOnScreen: ((fixture.Actor.YPosition + original.OffsetY) >> 8) == 0);
                }
                ReadOnlyMemory<EnemyBg2TilemapWrite> writes = StockBossWrites(installed, frame);
                AssertEqual(native.Bg2.Length, writes.Length, frame.Name + " exact BG2 run count");
                for (int index = 0; index < native.Bg2.Length; index++)
                {
                    AssertEqual(native.Bg2[index].DestinationWord, writes.Span[index].DestinationWord, frame.Name + " native BG2 destination/order");
                    AssertTrue(native.Bg2[index].Tiles.Span.SequenceEqual(writes.Span[index].Tiles.Span), frame.Name + " native BG2 payload/boundaries");
                    runs++; tiles += native.Bg2[index].Tiles.Length;
                }
                fixture.SetFrame(frame.Pointer, true); fixture.ClearBg2();
                OamBuffer rendered = fixture.Draw();
                AssertEqual(expectedOam.NextByteOffset, rendered.NextByteOffset, "stock packed sprite count");
                AssertTrue(expectedOam.LowTable.SequenceEqual(rendered.LowTable) && expectedOam.HighTable.SequenceEqual(rendered.HighTable),
                    "stock packed OAM matches independently decoded native records");
                AssertBossBg2(fixture.Vram, native.Bg2.Select(write => new EnemyBg2WriteDocument
                { X = write.DestinationWord % EnemyBg2FrameLayout.TilemapWidth, Y = write.DestinationWord / EnemyBg2FrameLayout.TilemapWidth,
                    Tiles = write.Tiles.ToArray().Select(value => (int)value).ToArray() }).ToArray());
            }
        }
        VerifyBossDisplayInstallation(path, overrides, installed);
        Console.WriteLine($"PASS stock boss import: {frames} native roots, {parts} sprites, {runs} BG2 runs/{tiles} words, " +
            $"{nativeLists.Count} compiled collision lists; exact RAM-only packed drawing and installation acceptance.");
    }

    private static ReadOnlyMemory<EnemyBg2TilemapWrite> StockBossWrites(EnemyTileArtworkCatalog catalog, EnemyExtendedFrameDefinition frame)
    {
        ReadOnlyMemory<EnemyBg2TilemapWrite> writes = default;
        bool exists = frame.Bank switch
        {
            CrocomireBg2FrameDefinitions.Bank => catalog.CrocomireBg2Frames!.TryGet(frame.Pointer, out writes),
            PhantoonBg2FrameDefinitions.Bank => catalog.PhantoonBg2Frames!.TryGet(frame.Pointer, out writes),
            DraygonBg2FrameDefinitions.Bank => catalog.DraygonBg2Frames!.TryGet(frame.Pointer, out writes),
            _ => throw new InvalidDataException("Unknown stock fixture family."),
        };
        return exists ? writes : ReadOnlyMemory<EnemyBg2TilemapWrite>.Empty;
    }

    private static void VerifyBossDisplayInstallation(string stock, string overrides, EnemyTileArtworkCatalog baseline)
    {
        var json = new EnemyIdentityFixture();
        string stockFile = Path.Combine(stock, EnemyExtendedFrameDefinitions.FileName);
        var document = JsonSerializer.Deserialize<EnemyExtendedFrameDocument>(File.ReadAllBytes(stockFile),
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })!;
        Directory.CreateDirectory(overrides);
        string file = Path.Combine(overrides, EnemyExtendedFrameDefinitions.FileName);
        var families = BossDisplayDocuments.Families().ToArray();
        foreach ((_, EnemyExtendedFrameDefinition[] family) in families)
            document.DisplayFrames![family[0].Name] = family[1].Name;
        using (var bytes = json.Json(document)) File.WriteAllBytes(file, bytes.ToArray());
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(stock, overrides);
        AssertTrue(baseline.ContentIdentity != edited.ContentIdentity, "selected BG2 display bindings affect bundle identity");
        AssertEqual(edited.ContentIdentity, EnemyTileArtworkFiles.Load(stock, overrides).ContentIdentity, "installed edits persist across reload");
        foreach ((_, EnemyExtendedFrameDefinition[] family) in families)
            AssertEqual(family[1].Pointer, edited.ExtendedFrames!.GetDisplayPointer(family[0].Bank, family[0].Pointer), "loader retains exact selected root");
        using (var bytes = json.Json(document with { Version = document.Version + 1 })) File.WriteAllBytes(file, bytes.ToArray());
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(stock, overrides), "future/malformed binding schema fails explicitly");
        using (var bytes = json.Json(document)) File.WriteAllBytes(file, bytes.ToArray());
        File.Delete(Path.Combine(stock, PhantoonBg2FrameDefinitions.FileName));
        AssertThrows<FileNotFoundException>(() => EnemyTileArtworkFiles.ValidateStock(stock), "missing stock BG2 file fails manifest validation");
        AssertThrows<FileNotFoundException>(() => EnemyTileArtworkFiles.Load(stock, overrides), "missing stock BG2 file fails installation loading");
        Console.WriteLine("PASS boss installation: validated stock manifest, selected remaps/identity/reload and malformed/missing-resource rejection.");
    }
}
