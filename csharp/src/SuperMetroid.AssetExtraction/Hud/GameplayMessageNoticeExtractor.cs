using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Hud;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Hud;

/// <summary>Extracts completion and save-confirmation notices as bounded UTF-8 text regions.</summary>
public static class GameplayMessageNoticeExtractor
{
    /// <summary>Imports completion and save-confirmation notice templates, editable text regions, borders, and save-selection rows.</summary>
    /// <param name="bus">Non-null cartridge address space supplying the native message definitions and tilemap words.</param>
    /// <returns>New UTF-8 JSON bytes containing tile templates, text-region row and column coordinates, alignment, widths, and palette indices.</returns>
    /// <remarks>Verifies the expected notice setup and draw routines; confirmation input and message behavior remain compiled.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="InvalidDataException">A message uses unexpected routines, has incompatible glyph or palette data, or fails notice-schema validation.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        var notices = new Dictionary<string, GameplayMessageNotice>(StringComparer.Ordinal);
        foreach (GameplayMessageId id in GameplayMessageNoticeDefinitions.MessageIds)
        {
            int definition = HudSourceDefinitions.Messages.DefinitionTable +
                ((byte)id - 1) * HudSourceDefinitions.Messages.DefinitionBytes;
            int setup = CartridgeImportSource.Require(bus).ReadCartridgeWord(definition);
            int draw = CartridgeImportSource.Require(bus).ReadCartridgeWord((definition + 2));
            int pointer = CartridgeImportSource.Require(bus).ReadCartridgeWord((definition + 4));
            int rowCount = GameplayMessageNotices.ContentRows(id);
            GameplayMessageRoutine expectedSetup = GameplayMessageNotices.IsSaveConfirmation(id)
                ? GameplayMessageRoutine.SetupLarge
                : GameplayMessageRoutine.SetupSmall;
            if (GameplayMessageRoutineWords.Routines.Decode(setup, "message setup routine") != expectedSetup ||
                GameplayMessageRoutineWords.Routines.Decode(draw, "message draw routine") != GameplayMessageRoutine.DrawSmallTilemap)
            {
                throw new InvalidDataException(
                    $"Gameplay message {id} is not the expected completion/save notice.");
            }

            var template = new BackgroundTile[
                rowCount * GameplayMessageBoxDefinitions.Layout.TilemapWidth];
            int content = HudSourceDefinitions.Messages.BankBase | pointer;
            for (int word = 0; word < template.Length; word++)
                template[word] = BackgroundTileWords.ToTile(CartridgeImportSource.Require(bus).ReadCartridgeWord((content + word * sizeof(ushort))));

            var regions = new List<GameplayMessageTextRegion>();
            foreach (GameplayMessageTextRegionDefinition region in
                GameplayMessageNotices.StockTextRegions(id, template))
            {
                BackgroundTile[] cells = template.AsSpan(
                    region.Row * GameplayMessageBoxDefinitions.Layout.TilemapWidth + region.Column,
                    region.Width).ToArray();
                int palette = cells.First(cell =>
                    GameplayMessageTitlePresentation.DecodeGlyph(cell) != ' ').ColourVariant;
                string padded = new(cells.Select((cell, column) =>
                {
                    if (cell.ColourVariant != palette)
                        throw new InvalidDataException(
                            $"Gameplay-message notice {id} mixes palettes in text region {region.Row}:{region.Column}, column {column}.");
                    return GameplayMessageTitlePresentation.DecodeGlyph(cell);
                }).ToArray());
                regions.Add(new()
                {
                    Row = region.Row,
                    Column = region.Column,
                    Width = region.Width,
                    Alignment = region.Alignment,
                    Text = region.Alignment == GameplayMessageTextAlignment.Center
                        ? padded.Trim()
                        : padded.TrimEnd(),
                    Palette = palette,
                });
            }

            BackgroundTile[]? yes = null;
            BackgroundTile[]? no = null;
            if (GameplayMessageNotices.IsSaveConfirmation(id))
            {
                yes = ReadSelection(bus, HudSourceDefinitions.Messages.SaveSelectionYesSourceWord);
                no = ReadSelection(bus, HudSourceDefinitions.Messages.SaveSelectionNoSourceWord);
            }
            notices.Add(id.ToString(), new()
            {
                RowCount = rowCount,
                Template = template,
                Text = regions.ToArray(),
                YesSelection = yes,
                NoSelection = no,
            });
        }

        var border = new BackgroundTile[GameplayMessageBoxDefinitions.Layout.TilemapWidth];
        for (int column = 0; column < border.Length; column++)
            border[column] = BackgroundTileWords.ToTile(CartridgeImportSource.Require(bus).ReadCartridgeWord((HudSourceDefinitions.Messages.SmallBorder + column * sizeof(ushort))));

        using var output = new MemoryStream();
        GameplayMessageNoticePresentation.Write(output, new()
        {
            Version = GameplayMessageNoticeDefinitions.Version,
            Border = border,
            Notices = notices,
        });
        return output.ToArray();
    }

    /// <summary>Copies one native save-confirmation choice row as tiles so YES and NO selection graphics remain distinct
    /// from editable text.</summary>
    private static BackgroundTile[] ReadSelection(
        ISnesAddressSpace bus,
        int sourceWord)
    {
        var result = new BackgroundTile[
            GameplayMessageTilemapDefinitions.SaveSelectionRowWords];
        for (int word = 0; word < result.Length; word++)
        {
            result[word] = BackgroundTileWords.ToTile(CartridgeImportSource.Require(bus).ReadCartridgeWord((HudSourceDefinitions.Messages.SaveSelectionTilemap +
                (sourceWord + word) * sizeof(ushort))));
        }
        return result;
    }
}
