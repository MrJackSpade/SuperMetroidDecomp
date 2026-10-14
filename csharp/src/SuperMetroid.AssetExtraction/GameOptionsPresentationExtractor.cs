using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Options.Artwork;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Imports the five options pages, shared background, dynamic labels and menu actors.
/// Input handling, binding swaps, page transitions and option semantics remain compiled.
/// </summary>
[CpuBusBoundary("codec: ROM options pages, background, dynamic labels and menu actors → options presentation")]
public static class GameOptionsPresentationExtractor
{
    /// <summary>Imports the five options pages, shared background, controller labels, highlight regions, headings, and animated menu cursor.</summary>
    /// <param name="bus">Non-null cartridge import address space supplying compressed options tilemaps and menu artwork.</param>
    /// <returns>New UTF-8 JSON bytes containing BG cells and tile-cell regions, pixel-space actor anchors, palette choices, and cursor-frame duration.</returns>
    /// <remarks>Visual language and toggle regions are exported; input handling, binding swaps, and option semantics remain compiled.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="InvalidDataException">A page does not decompress to the required tilemap size or the imported presentation is invalid.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);

        var pages = new Dictionary<string, MapPresentationCell[]>(StringComparer.Ordinal)
        {
            [GameOptionsPresentationDefinitions.BackgroundPage] = Cells(
                RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), FileSelectMapRomData.InitialMenuBackground,
                    GameOptionsRomData.TilemapByteCount)),
            [GameOptionsPresentationDefinitions.PrimaryPage] = Page(GameOptionsRomData.Pages.Get(GameOptionsTilemap.Primary)),
            [GameOptionsPresentationDefinitions.ControllerEnglishPage] = Page(GameOptionsRomData.Pages.Get(GameOptionsTilemap.ControllerEnglish)),
            [GameOptionsPresentationDefinitions.ControllerJapanesePage] = Page(GameOptionsRomData.Pages.Get(GameOptionsTilemap.ControllerJapanese)),
            [GameOptionsPresentationDefinitions.SpecialEnglishPage] = Page(GameOptionsRomData.Pages.Get(GameOptionsTilemap.SpecialEnglish)),
            [GameOptionsPresentationDefinitions.SpecialJapanesePage] = Page(GameOptionsRomData.Pages.Get(GameOptionsTilemap.SpecialJapanese)),
        };

        var labels = new Dictionary<string, MapPresentationCell[]>(StringComparer.Ordinal);
        for (int index = 0; index < GameOptionsRomData.Rows.ControllerActionCount; index++)
        {
            labels.Add(GameOptionsPresentationDefinitions.ControllerLabelName(index), Cells(
                RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
                    GameOptionsRomData.MenuBank | GameOptionsRomData.ControllerLabels.Source(index),
                    GameOptionsPresentationDefinitions.ControllerLabelCellCount * sizeof(ushort))));
        }

        Position16<TilemapCells>[] controllerAnchors = Enumerable.Range(0, GameOptionsRomData.Rows.ControllerActionCount)
            .Select(GameOptionsRomData.ControllerLabels.Destination)
            .Select(ByteOffsetPoint)
            .ToArray();
        GameOptionsLanguageRegionDocument[] languageRegions =
            Enumerable.Range(0, GameOptionsRomData.LanguagePaletteRegionCount)
                .Select(GameOptionsRomData.LanguagePaletteRegion)
                .Select(region => new GameOptionsLanguageRegionDocument
                {
                    Cells = CellRange(region.ByteOffset, region.ByteCount),
                    HighlightWhenJapanese = region.HighlightWhenJapanese,
                })
                .ToArray();
        var toggles = new Dictionary<string, GameOptionsToggleVisualDocument>(StringComparer.Ordinal)
        {
            [GameOptionsPresentationDefinitions.IconCancelToggle] = Toggle(GameOptionsRomData.SpecialToggles.IconCancel),
            [GameOptionsPresentationDefinitions.MoonwalkToggle] = Toggle(GameOptionsRomData.SpecialToggles.Moonwalk),
        };

        var sprites = new Dictionary<string, SpriteVisualPart[]>(StringComparer.Ordinal)
        {
            [GameOptionsPresentationDefinitions.HeadingFrameName(GameOptionsPresentationDefinitions.PrimaryMenu)] =
                MenuSpriteExtractor.Read(bus, GameOptionsRomData.Spritemaps.Heading(GameOptionsPage.Primary)),
            [GameOptionsPresentationDefinitions.HeadingFrameName(GameOptionsPresentationDefinitions.ControllerMenu)] =
                MenuSpriteExtractor.Read(bus, GameOptionsRomData.Spritemaps.Heading(GameOptionsPage.Controller)),
            [GameOptionsPresentationDefinitions.HeadingFrameName(GameOptionsPresentationDefinitions.SpecialMenu)] =
                MenuSpriteExtractor.Read(bus, GameOptionsRomData.Spritemaps.Heading(GameOptionsPage.Special)),
        };
        for (int frame = 0; frame < MenuMissileAnimationDefinitions.FrameCount; frame++)
        {
            sprites.Add(GameOptionsPresentationDefinitions.CursorFrameName(frame),
                MenuSpriteExtractor.Read(bus, MenuMissileAnimationDefinitions.SpritemapId(frame)));
        }

        var headings = new Dictionary<string, Position16<OamPixels>>(StringComparer.Ordinal)
        {
            [GameOptionsPresentationDefinitions.PrimaryMenu] = new(
                GameOptionsRomData.Spritemaps.HeadingX(GameOptionsPage.Primary),
                GameOptionsRomData.Spritemaps.HeadingY),
            [GameOptionsPresentationDefinitions.ControllerMenu] = new(
                GameOptionsRomData.Spritemaps.HeadingX(GameOptionsPage.Controller),
                GameOptionsRomData.Spritemaps.HeadingY),
            [GameOptionsPresentationDefinitions.SpecialMenu] = new(
                GameOptionsRomData.Spritemaps.HeadingX(GameOptionsPage.Special),
                GameOptionsRomData.Spritemaps.HeadingY),
        };
        var cursors = new Dictionary<string, Position16<OamPixels>[]>(StringComparer.Ordinal)
        {
            [GameOptionsPresentationDefinitions.PrimaryMenu] = Points(
                GameOptionsRomData.Cursors.PrimaryX, GameOptionsRomData.Rows.PrimaryCount, GameOptionsRomData.Cursors.PrimaryY),
            [GameOptionsPresentationDefinitions.ControllerMenu] = Points(
                GameOptionsRomData.Cursors.ControllerX, GameOptionsRomData.Rows.ControllerCount, GameOptionsRomData.Cursors.ControllerY),
            [GameOptionsPresentationDefinitions.SpecialMenu] = Points(
                GameOptionsRomData.Cursors.SpecialX, GameOptionsRomData.Rows.SpecialCount, GameOptionsRomData.Cursors.SpecialY),
        };

        using var output = new MemoryStream();
        GameOptionsPresentation.Write(output, new()
        {
            Version = GameOptionsPresentationDefinitions.Version,
            Pages = pages,
            ControllerLabels = labels,
            ControllerLabelAnchors = controllerAnchors,
            LanguageRegions = languageRegions,
            SpecialToggles = toggles,
            Sprites = sprites,
            HeadingAnchors = headings,
            CursorAnchors = cursors,
            HiddenCursor = new(GameOptionsRomData.Cursors.HiddenX, GameOptionsRomData.Cursors.HiddenY),
            SelectedPalette = GameOptionsRomData.TilePalettes.Selected,
            UnselectedPalette = GameOptionsRomData.TilePalettes.Unselected,
            CursorPalette = MenuPpuState.ObjectPaletteBits >> 9,
            CursorFrameDuration = MenuMissileAnimationDefinitions.FrameDuration,
        });
        return output.ToArray();

        MapPresentationCell[] Page(GameOptionsPageResource resource)
        {
            byte[] bytes = RomDataReader.Decompress(CartridgeImportSource.Require(bus), resource.Address,
                maximumOutputBytes: GameOptionsRomData.TilemapByteCount);
            if (bytes.Length != GameOptionsRomData.TilemapByteCount)
            {
                throw new InvalidDataException(
                    $"The {resource.Description} options screen expanded to " +
                    $"${bytes.Length:X} bytes, expected ${GameOptionsRomData.TilemapByteCount:X}.");
            }
            return Cells(bytes);
        }
    }

    /// <summary>Decodes native BG tile words into editable character, palette, priority, and flip cells.</summary>
    private static MapPresentationCell[] Cells(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length % sizeof(ushort) != 0)
            throw new InvalidDataException("Options tilemap content must contain whole words.");
        var cells = new MapPresentationCell[bytes.Length / sizeof(ushort)];
        for (int index = 0; index < cells.Length; index++)
        {
            var word = new SnesBgTilemapWord(unchecked((ushort)(
                SnesLittleEndian.Word(bytes[index * 2], bytes[index * 2 + 1]))));
            cells[index] = new()
            {
                TileColumn = word.CharacterIndex % MapTileAtlasFormat.TileColumns,
                TileRow = word.CharacterIndex / MapTileAtlasFormat.TileColumns,
                Palette = word.PaletteIndex,
                Priority = word.HasPriority,
                FlipX = word.FlipHorizontally,
                FlipY = word.FlipVertically,
            };
        }
        return cells;
    }

    /// <summary>Builds row-specific cursor anchors at one native horizontal coordinate.</summary>
    private static Position16<OamPixels>[] Points(ushort x, int count, Func<int, ushort> rowY)
    {
        var result = new Position16<OamPixels>[count];
        for (int index = 0; index < result.Length; index++)
            result[index] = new(x, rowY(index));
        return result;
    }

    /// <summary>Converts a BG tilemap byte offset to its column and row in the options page.</summary>
    private static Position16<TilemapCells> ByteOffsetPoint(ushort byteOffset)
    {
        int cell = byteOffset / sizeof(ushort);
        return new(checked((ushort)(cell % GameOptionsRomData.MenuTilemapWidth)),
            checked((ushort)(cell / GameOptionsRomData.MenuTilemapWidth)));
    }

    /// <summary>Expands a native byte span into consecutive tile-cell indices.</summary>
    private static int[] CellRange(int byteOffset, int byteCount) =>
        Enumerable.Range(byteOffset / sizeof(ushort), byteCount / sizeof(ushort)).ToArray();

    /// <summary>Combines the top and bottom palette regions for each special-option toggle state.</summary>
    private static GameOptionsToggleVisualDocument Toggle(GameOptionsToggleLayout layout) => new()
    {
        EnabledCells = CellRange(layout.EnabledTop,
            GameOptionsRomData.SpecialToggles.PaletteRegionByteCount)
            .Concat(CellRange(layout.EnabledBottom,
                GameOptionsRomData.SpecialToggles.PaletteRegionByteCount)).ToArray(),
        DisabledCells = CellRange(layout.DisabledTop,
            GameOptionsRomData.SpecialToggles.PaletteRegionByteCount)
            .Concat(CellRange(layout.DisabledBottom,
                GameOptionsRomData.SpecialToggles.PaletteRegionByteCount)).ToArray(),
    };
}
