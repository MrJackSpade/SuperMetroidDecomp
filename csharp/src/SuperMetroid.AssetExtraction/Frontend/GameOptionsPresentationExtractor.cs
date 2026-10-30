using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Frontend.Options.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Map.Artwork;
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
            [GameOptionsPresentationDefinitions.BackgroundPageName] = Cells(
                RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), MapSourceDefinitions.Menu.Background,
                    GameOptionsDefinitions.TilemapByteCount)),
            [GameOptionsNames.TilemapName(GameOptionsScreen.Primary)] = Page(GameOptionsSourceDefinitions.Pages[GameOptionsScreen.Primary]),
            [GameOptionsNames.TilemapName(GameOptionsScreen.ControllerEnglish)] = Page(GameOptionsSourceDefinitions.Pages[GameOptionsScreen.ControllerEnglish]),
            [GameOptionsNames.TilemapName(GameOptionsScreen.ControllerJapanese)] = Page(GameOptionsSourceDefinitions.Pages[GameOptionsScreen.ControllerJapanese]),
            [GameOptionsNames.TilemapName(GameOptionsScreen.SpecialEnglish)] = Page(GameOptionsSourceDefinitions.Pages[GameOptionsScreen.SpecialEnglish]),
            [GameOptionsNames.TilemapName(GameOptionsScreen.SpecialJapanese)] = Page(GameOptionsSourceDefinitions.Pages[GameOptionsScreen.SpecialJapanese]),
        };

        var labels = new Dictionary<string, MapPresentationCell[]>(StringComparer.Ordinal);
        for (int index = 0; index < GameOptionsDefinitions.Rows.ControllerActionCount; index++)
        {
            labels.Add(GameOptionsNames.ControllerLabelName(GameOptionsNames.ControllerLabelButton(index)), Cells(
                RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
                    GameOptionsSourceDefinitions.MenuBank | GameOptionsSourceDefinitions.ControllerLabelSources[index],
                    GameOptionsPageDefinitions.ControllerLabelCellCount * sizeof(ushort))));
        }

        Position16<TilemapCells>[] controllerAnchors = Enumerable.Range(0, GameOptionsDefinitions.Rows.ControllerActionCount)
            .Select(GameOptionsPageCells.ControllerLabelAnchor)
            .ToArray();
        GameOptionsLanguageRegionDocument[] languageRegions =
            Enumerable.Range(0, GameOptionsDefinitions.LanguagePaletteRegionCount)
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

        var headings = new Dictionary<string, Position16<SpritePixels>>(StringComparer.Ordinal)
        {
            [GameOptionsNames.MenuName(GameOptionsPage.Primary)] = new(
                GameOptionsPositions.HeadingX(GameOptionsPage.Primary),
                GameOptionsDefinitions.Headings.HeadingY),
            [GameOptionsNames.MenuName(GameOptionsPage.Controller)] = new(
                GameOptionsPositions.HeadingX(GameOptionsPage.Controller),
                GameOptionsDefinitions.Headings.HeadingY),
            [GameOptionsNames.MenuName(GameOptionsPage.Special)] = new(
                GameOptionsPositions.HeadingX(GameOptionsPage.Special),
                GameOptionsDefinitions.Headings.HeadingY),
        };
        var cursors = new Dictionary<string, Position16<SpritePixels>[]>(StringComparer.Ordinal)
        {
            [GameOptionsNames.MenuName(GameOptionsPage.Primary)] = Points(
                GameOptionsDefinitions.Cursors.PrimaryX, GameOptionsDefinitions.Rows.PrimaryCount, GameOptionsPositions.PrimaryY),
            [GameOptionsNames.MenuName(GameOptionsPage.Controller)] = Points(
                GameOptionsDefinitions.Cursors.ControllerX, GameOptionsDefinitions.Rows.ControllerCount, GameOptionsPositions.ControllerY),
            [GameOptionsNames.MenuName(GameOptionsPage.Special)] = Points(
                GameOptionsDefinitions.Cursors.SpecialX, GameOptionsDefinitions.Rows.SpecialCount, GameOptionsPositions.SpecialY),
        };

        using var output = new MemoryStream();
        GameOptionsSourcePresentation.Write(output, new()
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
            HiddenCursor = new(GameOptionsDefinitions.Cursors.HiddenX, GameOptionsDefinitions.Cursors.HiddenY),
            SelectedPalette = GameOptionsPageDefinitions.TilePalettes.Selected,
            UnselectedPalette = GameOptionsPageDefinitions.TilePalettes.Unselected,
            CursorPalette = MenuRenderDefinitions.ObjectPalette,
            CursorFrameDuration = MenuMissileAnimationDefinitions.FrameDuration,
        });
        return output.ToArray();

        MapPresentationCell[] Page(GameOptionsPageResource resource)
        {
            byte[] bytes = RomDataReader.Decompress(CartridgeImportSource.Require(bus), resource.Address,
                maximumOutputBytes: GameOptionsDefinitions.TilemapByteCount);
            if (bytes.Length != GameOptionsDefinitions.TilemapByteCount)
            {
                throw new InvalidDataException(
                    $"The {resource.Description} options screen expanded to " +
                    $"${bytes.Length:X} bytes, expected ${GameOptionsDefinitions.TilemapByteCount:X}.");
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

    /// <summary>Builds row-specific cursor anchors at one native horizontal coordinate.</summary>
    private static Position16<SpritePixels>[] Points(int x, int count, Func<int, ushort> rowY)
    {
        var result = new Position16<SpritePixels>[count];
        for (int index = 0; index < result.Length; index++)
            result[index] = Position16<SpritePixels>.Of(x, rowY(index));
        return result;
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
