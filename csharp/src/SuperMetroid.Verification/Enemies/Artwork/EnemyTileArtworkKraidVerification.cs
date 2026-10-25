using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Tooling;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Enemies.EnemyContentIdentityFixtureVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidInstalledPresentationOracleVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.Core;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Map.Artwork;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction;

using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Checks installed Kraid BG2, BG3, head, and room-background artwork against native transfers.</summary>
internal static class EnemyTileArtworkKraidVerification
{
    /// <summary>JSON options: Camel-case property names, indented output.</summary>
    private static readonly JsonSerializerOptions CamelCaseIndentedJson = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };

    /// <summary>Matches Kraid's working BG2 map and both shown body pages, then checks an edited tile.</summary>
    internal static void VerifyInstalledKraidBackground(
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace rom, string directory, InstalledEnemyArtwork stock)
    {
        KraidBackgroundArtwork art = stock.Bosses.KraidBackground
            ?? throw new InvalidDataException("Installed enemy artwork lacks Kraid maps.");
        AssertTrue(art.Upper.Bytes.Span.SequenceEqual(RomDataReader.Decompress(
                rom, KraidSourceDefinitions.UpperBodyTilemap,
                KraidBackgroundDefinitions.SourceTilemapBytes)),
            "installed Kraid upper BG2 tile references preserve source words");
        AssertTrue(art.Lower.Bytes.Span.SequenceEqual(RomDataReader.Decompress(
                rom, KraidSourceDefinitions.LowerBodyTilemap,
                KraidBackgroundDefinitions.SourceTilemapBytes)),
            "installed Kraid lower BG2 tile references preserve source words");

        ushort[] native = ReadImportedKraidWorkingMap(rom);
        AssertTrue(BuildKraidWorkingMap(stock).SequenceEqual(native),
            "installed Kraid BG2 construction preserves all native working words");
        KraidEnemyState initialized = InitializeKraidBackground(stock);
        AssertTrue(initialized.BackgroundTilemapsPrepared && initialized.OwnsBg2Tilemap,
            "Kraid BG2 initialization prepares the map and takes BG2");
        AssertTrue(initialized.WorkingTop == KraidBodyPage.Body && initialized.WorkingBottom == KraidBodyPage.Body,
            "Kraid BG2 initialization fills both working pages with the body");
        ushort[] nativeShown = KraidBodyFixtures.Tilemap(native.AsSpan(0, KraidBackgroundDefinitions.PageCells), [],
            native.AsSpan(KraidBackgroundDefinitions.LowerPageFirstCell, KraidBackgroundDefinitions.PageCells));
        AssertTrue(ShowKraidVisiblePages(stock).SequenceEqual(nativeShown),
            "installed Kraid BG2 sources preserve both shown body pages");

        string upperPath = Path.Combine(directory, KraidBackgroundArtworkFormat.UpperFileName);
        byte[] original = File.ReadAllBytes(upperPath);
        string overrides = Path.Combine(directory, "kraid-override");
        Directory.CreateDirectory(overrides);
        RoomBackgroundTilemapDocument document =
            RequiredJson.Deserialize<RoomBackgroundTilemapDocument>(original,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        RoomBackgroundTilemapPage[] pages = (RoomBackgroundTilemapPage[])document.Pages.Clone();
        RoomBackgroundTilemapCell[] cells = (RoomBackgroundTilemapCell[])pages[0].Cells.Clone();
        cells[0] = cells[0] with { TileColumn = cells[0].TileColumn ^ 1 };
        pages[0] = pages[0] with { Cells = cells };
        string overridePath = Path.Combine(overrides, KraidBackgroundArtworkFormat.UpperFileName);
        using (var output = File.Create(overridePath))
            RoomBackgroundTilemapAtlas.Write(output, document with { Pages = pages },
                KraidBackgroundDefinitions.SourceTilemapBytes);
        InstalledEnemyArtwork edited = EnemyTileArtworkFiles.Load(directory, overrides);
        ushort[] changed = BuildKraidWorkingMap(edited);
        AssertEqual((ushort)(native[0] ^ 1), changed[0],
            "edited Kraid BG2 reference changes the visible working-map cell");
        AssertTrue(changed.AsSpan(1).SequenceEqual(native.AsSpan(1)),
            "edited Kraid BG2 cell leaves the remaining live map unchanged");
        AssertEqual((ushort)(nativeShown[0] ^ 1), ShowKraidVisiblePages(edited)[0],
            "edited Kraid BG2 reference changes the shown tile word");
        AssertTrue(BuildKraidWorkingMap(EnemyTileArtworkFiles.Load(directory, overrides)).SequenceEqual(changed),
            "Kraid BG2 override survives catalog reload");

        File.WriteAllBytes(upperPath, [0]);
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(directory, null),
            "tampered stock Kraid BG2 map fails its manifest hash");
        File.WriteAllBytes(upperPath, original);
        File.WriteAllBytes(overridePath, [0]);
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(directory, overrides),
            "malformed Kraid BG2 override fails at load");
        Suite(nameof(VerifyInstalledKraidHeadFrames), () => VerifyInstalledKraidHeadFrames(rom, directory, stock));
    }

    /// <summary>Checks each authored Kraid head frame and confirms a JSON edit reaches the shown body.</summary>
    private static void VerifyInstalledKraidHeadFrames(
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace rom, string directory, InstalledEnemyArtwork stock)
    {
        ushort[] pointers = KraidHeadFrameDefinitions.PresentationKeys.Order().ToArray();
        AssertEqual(4, pointers.Length, "four distinct Kraid head art frames");
        foreach (ushort pointer in pointers)
        {
            byte[] native = RomDataReader.ReadFixedBank(rom,
                KraidSourceDefinitions.CodeBank | pointer,
                KraidBackgroundDefinitions.HeadImageCells * sizeof(ushort));
            ReadOnlySpan<ushort> installed = (stock.Bosses.KraidBackground ?? throw new InvalidOperationException("Verification requires stock.KraidBackground.")).HeadWords(pointer);
            for (int word = 0; word < installed.Length; word++)
                AssertEqual(SnesLittleEndian.Word(native[word * 2], native[word * 2 + 1]),
                    installed[word], $"Kraid head ${pointer:X4} word {word}");

            (KraidEnemyState baseline, ushort[] nativeShown) = ReadImportedKraidHeadFrame(rom, pointer);
            (KraidEnemyState selected, ushort[] installedShown) = RunKraidHeadFrame(stock, pointer);
            AssertTrue(selected.WorkingTop == baseline.WorkingTop && selected.WorkingBottom == baseline.WorkingBottom,
                $"Kraid head ${pointer:X4} leaves the working map");
            AssertTrue(installedShown.SequenceEqual(nativeShown),
                $"Kraid head ${pointer:X4} preserves the shown head");
        }

        ushort editedPointer = pointers[0];
        string fileName = KraidBackgroundArtworkFormat.HeadFileNameAt(editedPointer);
        string stockPath = Path.Combine(directory, fileName);
        byte[] original = File.ReadAllBytes(stockPath);
        string overrides = Path.Combine(directory, "kraid-head-override");
        Directory.CreateDirectory(overrides);
        KraidHeadTilemapDocument document =
            RequiredJson.Deserialize<KraidHeadTilemapDocument>(original,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        RoomBackgroundTilemapCell[] cells = (RoomBackgroundTilemapCell[])document.Cells.Clone();
        cells[0] = cells[0] with { TileColumn = cells[0].TileColumn ^ 1 };
        string overridePath = Path.Combine(overrides, fileName);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            document with { Cells = cells }, CamelCaseIndentedJson));
        InstalledEnemyArtwork edited = EnemyTileArtworkFiles.Load(directory, overrides);
        (_, ushort[] changedShown) = RunKraidHeadFrame(edited, editedPointer);
        (_, ushort[] stockShown) = ReadImportedKraidHeadFrame(rom, editedPointer);
        AssertEqual((ushort)(stockShown[0] ^ 1), changedShown[0],
            "edited Kraid head tile changes the first shown reference");
        AssertTrue(stockShown.AsSpan(1).SequenceEqual(changedShown.AsSpan(1)),
            "Kraid head edit leaves every later shown word unchanged");
        (_, ushort[] reloadedShown) = RunKraidHeadFrame(
            EnemyTileArtworkFiles.Load(directory, overrides), editedPointer);
        AssertTrue(reloadedShown.SequenceEqual(changedShown),
            "Kraid head tilemap edit survives a catalog reload");

        File.WriteAllBytes(stockPath, [0]);
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(directory, null),
            "tampered Kraid head stock map fails manifest hash");
        File.WriteAllBytes(stockPath, original);
        File.WriteAllBytes(overridePath, [0]);
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(directory, overrides),
            "malformed Kraid head override fails at load");
        Suite(nameof(VerifyInstalledKraidRoomBackground), () => VerifyInstalledKraidRoomBackground(rom, directory, stock));
        Suite(nameof(VerifyInstalledKraidBg3Restoration), () => VerifyInstalledKraidBg3Restoration(rom, directory, stock));
    }

    /// <summary>Checks queued, direct, and restored BG3 tile uploads against installed HUD pixels.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native VRAM words")]
    private static void VerifyInstalledKraidBg3Restoration(
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace rom, string directory, InstalledEnemyArtwork enemyArtwork)
    {
        AssertEqual(24, (int)VramAssetId.GrappleVerticalSegmentTiles,
            "existing pending-VRAM asset identities remain stable");
        AssertEqual(25, (int)VramAssetId.KraidBg3RestoreQuarter0,
            "new Kraid quarter IDs append after existing debugger-state IDs");
        string mapDirectory = Path.Combine(directory, "kraid-bg3-map-art");
        MapPresentationExtractor.Extract(rom, mapDirectory, SupportedCartridge.Sha256);
        AreaMapPresentationCatalog stock = MapPresentationFiles.Load(mapDirectory, null);
        byte[] native = RomDataReader.ReadFixedBank(rom,
            KraidBackgroundDefinitionsConstants.StandardBg3TilesAddress,
            HudTileAtlasFormat.CharacterByteCount);
        AssertTrue(stock.HudTiles.Bytes.Span[..native.Length].SequenceEqual(native),
            "shared installed HUD PNG preserves all Kraid BG3 restoration source bytes");

        var baselineQueued = new SnesVram();
        baselineQueued.LoadBytes(KraidBackgroundDefinitions.StandardBg3VramWord * 2, native);
        SnesVram installed = RunKraidBg3Restoration(enemyArtwork, stock.HudTiles);
        AssertTrue(installed.ImageEquals(baselineQueued),
            "installed Kraid BG3 restoration preserves native VRAM");
        // These checks exercise asset binding/queue restoration, not starting colors.
        // Supply the constructor's explicit palette dependency; never revive its ROM fallback.
        PaletteRgb5[] Colors(int count) => Enumerable.Range(0, count)
            .Select(_ => new PaletteRgb5 { Red = 0, Green = 0, Blue = 0 }).ToArray();
        GameplayBasePaletteCatalog initial = GameplayBasePaletteFiles.Read(new EnemyIdentityFixture().Json(
            new GameplayBasePaletteDocument(GameplayBasePaletteFormat.Version, Colors(SnesCgram.ColorCount),
                Colors(GameplayBasePaletteFormat.SpriteColorCount))));
        var runtime = SuperMetroid.Core.Runtime.SuperMetroidRuntime.Create(HostGameplayOverrides.None, initialPaletteArt: initial);
        runtime.Artwork.MapPresentation = stock;
        runtime.EnemySession.TileArtwork = enemyArtwork.Enemies;
        runtime.BossArtwork.Artwork = enemyArtwork.Bosses;
        runtime.AreaArtwork.Artwork = enemyArtwork.Areas;
        AssertTrue(ReferenceEquals(stock.HudTiles, runtime.BossArtwork.HudTileArtwork),
            "runtime presentation binding supplies shared HUD art to Kraid");
        for (int quarter = 0; quarter < KraidHudRestoration.QuarterCount;
             quarter++)
        {
            VramAssetId asset = KraidHudRestoration.AssetForQuarter(quarter);
            AssertTrue(((IVramAssetProvider)runtime.Artwork).Resolve(asset).Span
                    .SequenceEqual(stock.HudTiles.KraidRestoreQuarter(quarter).Span),
                $"runtime resolves Kraid BG3 quarter {quarter} from installed HUD pixels");
        }
        string overrides = Path.Combine(directory, "kraid-bg3-map-overrides");
        Directory.CreateDirectory(overrides);
        string stockPng = Path.Combine(mapDirectory, HudTileAtlasFormat.FileName);
        using var input = new MemoryStream(File.ReadAllBytes(stockPng), writable: false);
        IndexedPngImage image = IndexedPng.Read(input,
            new(MapTileAtlasFormat.Width, MapTileAtlasFormat.Height));
        image.Pixels[0] ^= 1;
        using (var output = File.Create(Path.Combine(overrides, HudTileAtlasFormat.FileName)))
            IndexedPng.Write(output, new(image.Size.X, image.Size.Y),
                image.Pixels, image.Colours);
        AreaMapPresentationCatalog edited = MapPresentationFiles.Load(mapDirectory, overrides);
        SnesVram changed = RunKraidBg3Restoration(enemyArtwork, edited.HudTiles);
        int destination = KraidBackgroundDefinitions.StandardBg3VramWord * sizeof(ushort);
        AssertEqual((byte)(baselineQueued.ReadByte(destination) ^ 0x80),
            changed.ReadByte(destination),
            "edited shared HUD pixel changes Kraid's first restored BG3 character");
        AssertTrue(baselineQueued.Bytes[(destination + 1)..]
                .SequenceEqual(changed.Bytes[(destination + 1)..]),
            "edited Kraid BG3 character leaves all other VRAM bytes unchanged");
        AreaMapPresentationCatalog reloaded = MapPresentationFiles.Load(mapDirectory, overrides);
        AssertTrue(RunKraidBg3Restoration(enemyArtwork, reloaded.HudTiles)
            .Bytes.SequenceEqual(changed.Bytes),
            "Kraid BG3 uses the current HUD PNG override after catalog reload");
    }

    /// <summary>Invokes each death-phase BG3 quarter restoration through the live Kraid behavior.</summary>
    private static SnesVram RunKraidBg3Restoration(InstalledEnemyArtwork enemyArtwork, HudTileAtlas hud)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance)
        {
            TileArtwork = enemyArtwork.Enemies, BossArtwork = enemyArtwork.Bosses, AreaArtwork = enemyArtwork.Areas,
            BossArtworkBinding = { HudTileArtwork = hud },
        };
        var vram = new SnesVram();
        enemies.Presentation.BindBackgrounds(BgArtworkFixtures.For(vram));
        Action<RoomEnemySlot, KraidEnemyState, int, KraidAiFunction> restore = enemies.KraidBehavior.Death.RestoreKraidDeathBg3Quarter;
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        var state = new KraidEnemyState();
        for (int quarter = 0; quarter < KraidHudRestoration.QuarterCount;
             quarter++)
        {
            restore(body, state, quarter, KraidAiFunction.DeathFadeInBackground);
            AssertEqual(quarter + 1, state.RestoredBg3Quarters,
                $"Kraid BG3 quarter {quarter} retains death coroutine cadence");
        }
        // The restored characters are bound (#1352); the image a capture shows composes them.
        return BgArtworkFixtures.ComposedVram(vram);
    }

    /// <summary>Checks the Kraid room-background character upload and a selected PNG pixel edit.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    private static void VerifyInstalledKraidRoomBackground(
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace rom, string directory, InstalledEnemyArtwork stock)
    {
        byte[] native = RomDataReader.ReadFixedBank(rom,
            KraidSourceDefinitions.RoomBackgroundTiles,
            KraidSourceDefinitions.RoomBackgroundTileBytes);
        AssertTrue((stock.Bosses.KraidBackground ?? throw new InvalidOperationException("Verification requires stock.KraidBackground.")).RoomBackgroundTiles.Characters.Span.SequenceEqual(native),
            "installed Kraid room-background PNG preserves native planar characters");
        var baseline = new SnesVram();
        baseline.LoadBytes(KraidBackgroundDefinitions.RoomBackgroundTileVramWord * 2, native);
        SnesVram installed = UploadKraidRoomBackground(stock);
        AssertTrue(installed.ImageEquals(baseline),
            "installed Kraid room-background upload preserves full native VRAM");

        string fileName = KraidBackgroundArtworkFormat.RoomBackgroundFileName;
        string stockPath = Path.Combine(directory, fileName);
        byte[] original = File.ReadAllBytes(stockPath);
        string overrides = Path.Combine(directory, "kraid-room-background-override");
        Directory.CreateDirectory(overrides);
        using var input = new MemoryStream(original, writable: false);
        IndexedPngImage image = IndexedPng.Read(input, new(16 * 8, 8));
        image.Pixels[0] ^= 1;
        string overridePath = Path.Combine(overrides, fileName);
        using (var output = File.Create(overridePath))
            IndexedPng.Write(output, new(image.Size.X, image.Size.Y),
                image.Pixels, image.Colours);
        InstalledEnemyArtwork edited = EnemyTileArtworkFiles.Load(directory, overrides);
        SnesVram changed = UploadKraidRoomBackground(edited);
        int destination = KraidBackgroundDefinitions.RoomBackgroundTileVramWord * sizeof(ushort);
        AssertEqual((byte)(baseline.ReadByte(destination) ^ 0x80),
            changed.ReadByte(destination),
            "edited Kraid background pixel changes native VRAM destination");
        AssertTrue(baseline.Bytes[(destination + 1)..]
                .SequenceEqual(changed.Bytes[(destination + 1)..]),
            "Kraid backdrop PNG edit leaves later VRAM bytes unchanged");
        SnesVram reloaded = UploadKraidRoomBackground(EnemyTileArtworkFiles.Load(directory, overrides));
        AssertTrue(reloaded.ImageEquals(changed),
            "Kraid room-background PNG override survives catalog reload");

        File.WriteAllBytes(stockPath, [0]);
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(directory, null),
            "tampered Kraid room-background stock PNG fails manifest hash");
        File.WriteAllBytes(stockPath, original);
        File.WriteAllBytes(overridePath, [0]);
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(directory, overrides),
            "malformed Kraid room-background PNG override fails at load");
    }

    /// <summary>The image a capture shows after the room-background characters bind, using a cartridge-free Kraid owner.</summary>
    private static SnesVram UploadKraidRoomBackground(InstalledEnemyArtwork artwork)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = artwork.Enemies, BossArtwork = artwork.Bosses, AreaArtwork = artwork.Areas };
        var vram = new SnesVram();
        enemies.Presentation.BindBackgrounds(BgArtworkFixtures.For(vram));
        Action upload = enemies.KraidBehavior.Rise.UploadKraidRoomBackgroundTiles;
        upload();
        return BgArtworkFixtures.ComposedVram(vram);
    }

    /// <summary>The BG2 tilemap a selected installed head frame shows over blank pages.</summary>
    private static RunKraidHeadFrameResult RunKraidHeadFrame(InstalledEnemyArtwork artwork, ushort pointer)
    {
        KraidHeadFrame frame = KraidHeadFrame.ByMouthStage.Single(candidate => KraidHeadFrameDefinitions.PresentationKey(candidate) == pointer);
        var state = new KraidEnemyState();
        KraidHead.TransferKraidHeadTilemap(state, frame);
        return new RunKraidHeadFrameResult(state, KraidBodyFixtures.Shown(artwork.Bosses, state));
    }

    /// <summary>Kraid's working map as his installed art composes it, without cartridge reads.</summary>
    internal static ushort[] BuildKraidWorkingMap(InstalledEnemyArtwork artwork) => KraidBodyFixtures.WorkingMap(artwork.Bosses);

    /// <summary>Runs Kraid's background initializer against installed art without cartridge reads.</summary>
    private static KraidEnemyState InitializeKraidBackground(InstalledEnemyArtwork artwork)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = artwork.Enemies, BossArtwork = artwork.Bosses, AreaArtwork = artwork.Areas };
        Action<KraidEnemyState> initialize = enemies.KraidBehavior.Rise.InitializeKraidBackground;
        var state = new KraidEnemyState();
        initialize(state);
        return state;
    }

    /// <summary>Shows both BG2 working-map pages and checks that they are what Kraid displays.</summary>
    private static ushort[] ShowKraidVisiblePages(InstalledEnemyArtwork artwork)
    {
        KraidEnemyState state = InitializeKraidBackground(artwork);
        KraidBackground.TransferKraidTopTilemap(state);
        KraidBackground.TransferKraidBottomTilemap(state);
        AssertTrue(state.DisplayedBody.Top == state.WorkingTop && state.DisplayedBody.Bottom == state.WorkingBottom,
            "Kraid displays both working pages");
        return KraidBodyFixtures.Shown(artwork.Bosses, state);
    }
}
