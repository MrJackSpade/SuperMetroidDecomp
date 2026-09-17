using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Imports the five options pages, shared background, dynamic labels and menu actors.
/// Input handling, binding swaps, page transitions and option semantics remain compiled.
/// </summary>
public static class GameOptionsPresentationExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);

        var pages = new Dictionary<string, MapPresentationCell[]>(StringComparer.Ordinal)
        {
            [GameOptionsPresentationDefinitions.BackgroundPage] = Cells(
                RomDataReader.ReadFixedBank(bus, FileSelectMapRomData.InitialMenuBackground,
                    GameOptionsRomData.TilemapByteCount)),
            [GameOptionsPresentationDefinitions.PrimaryPage] = Page(GameOptionsRomData.Pages.Primary),
            [GameOptionsPresentationDefinitions.ControllerEnglishPage] = Page(GameOptionsRomData.Pages.ControllerEnglish),
            [GameOptionsPresentationDefinitions.ControllerJapanesePage] = Page(GameOptionsRomData.Pages.ControllerJapanese),
            [GameOptionsPresentationDefinitions.SpecialEnglishPage] = Page(GameOptionsRomData.Pages.SpecialEnglish),
            [GameOptionsPresentationDefinitions.SpecialJapanesePage] = Page(GameOptionsRomData.Pages.SpecialJapanese),
        };

        var labels = new Dictionary<string, MapPresentationCell[]>(StringComparer.Ordinal);
        ReadOnlySpan<ushort> sources = GameOptionsRomData.ControllerLabels.Sources;
        for (int index = 0; index < sources.Length; index++)
        {
            labels.Add(GameOptionsPresentationDefinitions.ControllerLabelName(index), Cells(
                RomDataReader.ReadFixedBank(bus, GameOptionsRomData.MenuBank | sources[index],
                    GameOptionsPresentationDefinitions.ControllerLabelCellCount * sizeof(ushort))));
        }

        MapLabelPoint[] controllerAnchors = GameOptionsRomData.ControllerLabels.Destinations
            .ToArray()
            .Select(ByteOffsetPoint)
            .ToArray();
        GameOptionsLanguageRegionDocument[] languageRegions =
            GameOptionsRomData.LanguagePaletteRegions.ToArray()
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
                MenuSpriteExtractor.Read(bus, GameOptionsRomData.Spritemaps.OptionModeBorder),
            [GameOptionsPresentationDefinitions.HeadingFrameName(GameOptionsPresentationDefinitions.ControllerMenu)] =
                MenuSpriteExtractor.Read(bus, GameOptionsRomData.Spritemaps.ControllerModeBorder),
            [GameOptionsPresentationDefinitions.HeadingFrameName(GameOptionsPresentationDefinitions.SpecialMenu)] =
                MenuSpriteExtractor.Read(bus, GameOptionsRomData.Spritemaps.SpecialModeBorder),
        };
        for (int frame = 0; frame < GameOptionsRomData.Spritemaps.MissileFrameIds.Length; frame++)
        {
            sprites.Add(GameOptionsPresentationDefinitions.CursorFrameName(frame),
                MenuSpriteExtractor.Read(bus, GameOptionsRomData.Spritemaps.MissileFrameIds[frame]));
        }

        var headings = new Dictionary<string, MapLabelPoint>(StringComparer.Ordinal)
        {
            [GameOptionsPresentationDefinitions.PrimaryMenu] = new(
                GameOptionsRomData.Spritemaps.OptionModeBorderX,
                GameOptionsRomData.Spritemaps.OptionModeBorderY),
            [GameOptionsPresentationDefinitions.ControllerMenu] = new(
                GameOptionsRomData.Spritemaps.ControllerModeBorderX,
                GameOptionsRomData.Spritemaps.OptionModeBorderY),
            [GameOptionsPresentationDefinitions.SpecialMenu] = new(
                GameOptionsRomData.Spritemaps.SpecialModeBorderX,
                GameOptionsRomData.Spritemaps.OptionModeBorderY),
        };
        var cursors = new Dictionary<string, MapLabelPoint[]>(StringComparer.Ordinal)
        {
            [GameOptionsPresentationDefinitions.PrimaryMenu] = Points(
                GameOptionsRomData.Cursors.PrimaryX, GameOptionsRomData.Cursors.PrimaryY),
            [GameOptionsPresentationDefinitions.ControllerMenu] = Points(
                GameOptionsRomData.Cursors.ControllerX, GameOptionsRomData.Cursors.ControllerY),
            [GameOptionsPresentationDefinitions.SpecialMenu] = Points(
                GameOptionsRomData.Cursors.SpecialX, GameOptionsRomData.Cursors.SpecialY),
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
            CursorFrameDuration = GameOptionsRomData.Spritemaps.MissileFrameDuration,
        });
        return output.ToArray();

        MapPresentationCell[] Page(GameOptionsPageResource resource)
        {
            byte[] bytes = RomDataReader.Decompress(bus, resource.Address,
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

    private static MapPresentationCell[] Cells(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length % sizeof(ushort) != 0)
            throw new InvalidDataException("Options tilemap content must contain whole words.");
        var cells = new MapPresentationCell[bytes.Length / sizeof(ushort)];
        for (int index = 0; index < cells.Length; index++)
        {
            var word = new SnesBgTilemapWord(unchecked((ushort)(
                bytes[index * 2] | bytes[index * 2 + 1] << 8)));
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

    private static MapLabelPoint[] Points(ushort x, ReadOnlySpan<ushort> rows)
    {
        var result = new MapLabelPoint[rows.Length];
        for (int index = 0; index < result.Length; index++)
            result[index] = new(x, rows[index]);
        return result;
    }

    private static MapLabelPoint ByteOffsetPoint(ushort byteOffset)
    {
        int cell = byteOffset / sizeof(ushort);
        return new(cell % GameOptionsRomData.MenuTilemapWidth,
            cell / GameOptionsRomData.MenuTilemapWidth);
    }

    private static int[] CellRange(int byteOffset, int byteCount) =>
        Enumerable.Range(byteOffset / sizeof(ushort), byteCount / sizeof(ushort)).ToArray();

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
