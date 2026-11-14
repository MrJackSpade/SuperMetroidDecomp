using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Frontend.Options.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Frontend.Options.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Menus;
using SuperMetroid.AssetExtraction.Frontend.Map;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>
/// Imports the five options pages, shared background, dynamic labels and menu actors.
/// Input handling, binding swaps, page transitions and option semantics remain compiled.
/// </summary>
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
            [GameOptionsPresentationFormat.BackgroundPageName] = Cells(
                RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), MapSourceDefinitions.Menu.Background,
                    GameOptionsSourceDefinitions.TilemapByteCount)),
            [GameOptionsNames.PageName(GameOptionsScreen.Primary)] = Page(GameOptionsSourceDefinitions.Pages[GameOptionsScreen.Primary]),
            [GameOptionsNames.PageName(GameOptionsScreen.ControllerEnglish)] = Page(GameOptionsSourceDefinitions.Pages[GameOptionsScreen.ControllerEnglish]),
            [GameOptionsNames.PageName(GameOptionsScreen.ControllerJapanese)] = Page(GameOptionsSourceDefinitions.Pages[GameOptionsScreen.ControllerJapanese]),
            [GameOptionsNames.PageName(GameOptionsScreen.SpecialEnglish)] = Page(GameOptionsSourceDefinitions.Pages[GameOptionsScreen.SpecialEnglish]),
            [GameOptionsNames.PageName(GameOptionsScreen.SpecialJapanese)] = Page(GameOptionsSourceDefinitions.Pages[GameOptionsScreen.SpecialJapanese]),
        };

        var labels = new Dictionary<string, MapPresentationCell[]>(StringComparer.Ordinal);
        for (int index = 0; index < GameOptionsDefinitions.Rows.ControllerActionCount; index++)
        {
            labels.Add(GameOptionsNames.ControllerLabelName(GameOptionsNames.ControllerLabelButton(index)), Cells(
                RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
                    GameOptionsSourceDefinitions.MenuBank | GameOptionsSourceDefinitions.ControllerLabelSources[index],
                    GameOptionsPageDefinitions.ControllerLabelCellCount * sizeof(ushort))));
        }

        Position16<CharacterCells>[] controllerAnchors = Enumerable.Range(0, GameOptionsDefinitions.Rows.ControllerActionCount)
            .Select(GameOptionsPageCells.ControllerLabelAnchor)
            .ToArray();
        GameOptionsLanguageRegionDocument[] languageRegions =
            Enumerable.Range(0, GameOptionsPageDefinitions.LanguageRegions.RowCount)
                .Select(GameOptionsPageCells.LanguagePaletteRegion)
                .Select(region => new GameOptionsLanguageRegionDocument
                {
                    Cells = CellRange(region.ByteOffset, region.ByteCount),
                    HighlightWhenJapanese = region.HighlightWhenJapanese,
                })
                .ToArray();
        var toggles = new Dictionary<string, GameOptionsToggleVisualDocument>(StringComparer.Ordinal)
        {
            [GameOptionsNames.ToggleName(GameOptionsToggle.IconCancel)] = Toggle(GameOptionsPageCells.IconCancel),
            [GameOptionsNames.ToggleName(GameOptionsToggle.Moonwalk)] = Toggle(GameOptionsPageCells.Moonwalk),
        };

        var sprites = new Dictionary<string, SpriteVisualPart[]>(StringComparer.Ordinal)
        {
            [GameOptionsNames.HeadingSpriteName(GameOptionsPage.Primary)] =
                MenuSpriteExtractor.Read(bus, GameOptionsSourceDefinitions.HeadingSpritemaps[GameOptionsPage.Primary]),
            [GameOptionsNames.HeadingSpriteName(GameOptionsPage.Controller)] =
                MenuSpriteExtractor.Read(bus, GameOptionsSourceDefinitions.HeadingSpritemaps[GameOptionsPage.Controller]),
            [GameOptionsNames.HeadingSpriteName(GameOptionsPage.Special)] =
                MenuSpriteExtractor.Read(bus, GameOptionsSourceDefinitions.HeadingSpritemaps[GameOptionsPage.Special]),
        };
        for (int frame = 0; frame < MenuMissileAnimationDefinitions.FrameCount; frame++)
        {
            sprites.Add(GameOptionsNames.CursorSpriteName(frame),
                MenuSpriteExtractor.Read(bus, MenuMissileSpritemaps.SpritemapId(frame)));
        }

        // The headings' and cursor's anchors are read from the cartridge's page setups and cursor tables; the game keeps no
        // copy of them (#1615).
        int headingRow = Word(GameOptionsSourceDefinitions.HeadingRowOperand);
        var headings = new Dictionary<string, Position16<SpritePixels>>(StringComparer.Ordinal)
        {
            [GameOptionsPresentationFormat.MenuName(GameOptionsPage.Primary)] = Heading(GameOptionsPage.Primary),
            [GameOptionsPresentationFormat.MenuName(GameOptionsPage.Controller)] = Heading(GameOptionsPage.Controller),
            [GameOptionsPresentationFormat.MenuName(GameOptionsPage.Special)] = Heading(GameOptionsPage.Special),
        };
        var cursors = new Dictionary<string, Position16<SpritePixels>[]>(StringComparer.Ordinal)
        {
            [GameOptionsPresentationFormat.MenuName(GameOptionsPage.Primary)] = Cursor(GameOptionsPage.Primary, GameOptionsDefinitions.Rows.PrimaryCount),
            [GameOptionsPresentationFormat.MenuName(GameOptionsPage.Controller)] = Cursor(GameOptionsPage.Controller, GameOptionsDefinitions.Rows.ControllerCount),
            [GameOptionsPresentationFormat.MenuName(GameOptionsPage.Special)] = Cursor(GameOptionsPage.Special, GameOptionsDefinitions.Rows.SpecialCount),
        };

        using var output = new MemoryStream();
        GameOptionsSourcePresentation.Write(output, new()
        {
            Version = GameOptionsPresentationFormat.Version,
            Pages = pages,
            ControllerLabels = labels,
            ControllerLabelAnchors = controllerAnchors,
            LanguageRegions = languageRegions,
            SpecialToggles = toggles,
            Sprites = sprites,
            HeadingAnchors = headings,
            CursorAnchors = cursors,
            SelectedPalette = GameOptionsPageDefinitions.TilePalettes.Selected,
            UnselectedPalette = GameOptionsPageDefinitions.TilePalettes.Unselected,
            CursorPalette = MenuRenderDefinitions.ObjectPalette,
            CursorFrameDuration = MenuAnimationTimings.MissileFrameDuration(bus),
        });
        return output.ToArray();

        MapPresentationCell[] Page(GameOptionsPageResource resource)
        {
            byte[] bytes = RomDataReader.Decompress(CartridgeImportSource.Require(bus), resource.Address,
                maximumOutputBytes: GameOptionsSourceDefinitions.TilemapByteCount);
            if (bytes.Length != GameOptionsSourceDefinitions.TilemapByteCount)
            {
                throw new InvalidDataException(
                    $"The {resource.Description} options screen expanded to " +
                    $"${bytes.Length:X} bytes, expected ${GameOptionsSourceDefinitions.TilemapByteCount:X}.");
            }
            return Cells(bytes);
        }

        int Word(int address) => RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);

        Position16<SpritePixels> Heading(GameOptionsPage page) =>
            Position16<SpritePixels>.Of(Word(GameOptionsSourceDefinitions.HeadingColumnOperands[page]), headingRow);

        Position16<SpritePixels>[] Cursor(GameOptionsPage page, int rows)
        {
            var result = new Position16<SpritePixels>[rows];
            for (int row = 0; row < rows; row++)
                result[row] = Position16<SpritePixels>.Of(GameOptionsSourceDefinitions.CursorColumns[page],
                    Word(GameOptionsSourceDefinitions.CursorRowTables[page] + GameOptionsSourceDefinitions.CursorRowStride * row));
            return result;
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
            var word = new SnesBgTilemapWord(unchecked(
                LittleEndianBytes.Word(bytes[index * 2], bytes[index * 2 + 1])));
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

    /// <summary>Expands a native byte span into consecutive tile-cell indices.</summary>
    private static int[] CellRange(int byteOffset, int byteCount) =>
        Enumerable.Range(byteOffset / sizeof(ushort), byteCount / sizeof(ushort)).ToArray();

    /// <summary>Combines the top and bottom palette regions for each special-option toggle state.</summary>
    private static GameOptionsToggleVisualDocument Toggle(GameOptionsToggleLayout layout) => new()
    {
        EnabledCells = CellRange(layout.EnabledTop,
            GameOptionsPageDefinitions.SpecialToggles.PaletteRegionByteCount)
            .Concat(CellRange(layout.EnabledBottom,
                GameOptionsPageDefinitions.SpecialToggles.PaletteRegionByteCount)).ToArray(),
        DisabledCells = CellRange(layout.DisabledTop,
            GameOptionsPageDefinitions.SpecialToggles.PaletteRegionByteCount)
            .Concat(CellRange(layout.DisabledBottom,
                GameOptionsPageDefinitions.SpecialToggles.PaletteRegionByteCount)).ToArray(),
    };
}
