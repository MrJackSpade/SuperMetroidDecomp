using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Hud;

/// <summary>Extracts completion and save-confirmation notices as bounded UTF-8 text regions.</summary>
[CpuBusBoundary("codec: ROM completion and save-confirmation message tilemaps → gameplay message notices")]
public static class GameplayMessageNoticeExtractor
{
    /// <summary>Imports completion and save-confirmation notice templates, editable text regions, borders, and save-selection rows.</summary>
    /// <param name="bus">Non-null cartridge address space supplying the native message definitions and tilemap words.</param>
    /// <returns>New UTF-8 JSON bytes containing raw BG-cell templates, text-region row and column coordinates, alignment, widths, and palette indices.</returns>
    /// <remarks>Verifies the expected notice setup and draw routines; confirmation input and message behavior remain compiled.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="InvalidDataException">A message uses unexpected routines, has incompatible glyph or palette data, or fails notice-schema validation.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        var notices = new Dictionary<string, GameplayMessageNotice>(StringComparer.Ordinal);
        foreach (GameplayMessageId id in GameplayMessageNoticeDefinitions.MessageIds)
        {
            int definition = GameplayMessageRomData.Assets.DefinitionTable +
                ((byte)id - 1) * GameplayMessageRomData.Layout.DefinitionBytes;
            ushort setup = CartridgeImportSource.Require(bus).ReadCartridgeWord(definition);
            ushort draw = CartridgeImportSource.Require(bus).ReadCartridgeWord((definition + 2));
            ushort pointer = CartridgeImportSource.Require(bus).ReadCartridgeWord((definition + 4));
            int rowCount = GameplayMessageNoticeDefinitions.ContentRows(id);
            ushort expectedSetup = GameplayMessageNoticeDefinitions.IsSaveConfirmation(id)
                ? GameplayMessageRomData.Routines.SetupLarge
                : GameplayMessageRomData.Routines.SetupSmall;
            if (setup != expectedSetup ||
                !GameplayMessageRomData.IsSmallTilemapDraw(draw))
            {
                throw new InvalidDataException(
                    $"Gameplay message {id} is not the expected completion/save notice.");
            }

            var template = new GameplayMessageTitleCell[
                rowCount * GameplayMessageRomData.Layout.TilemapWidth];
            int content = GameplayMessageRomData.Assets.BankBase | pointer;
            for (int word = 0; word < template.Length; word++)
                template[word] = new() { Raw = CartridgeImportSource.Require(bus).ReadCartridgeWord((content + word * sizeof(ushort))) };

            var regions = new List<GameplayMessageTextRegion>();
            foreach (GameplayMessageTextRegionDefinition region in
                GameplayMessageNoticeDefinitions.StockTextRegions(id, template))
            {
                GameplayMessageTitleCell[] cells = template.AsSpan(
                    region.Row * GameplayMessageRomData.Layout.TilemapWidth + region.Column,
                    region.Width).ToArray();
                int palette = (cells.First(cell =>
                    GameplayMessageTitlePresentation.DecodeGlyph(cell.Raw) != ' ').Raw >> 10) & 7;
                string padded = new(cells.Select((cell, column) =>
                {
                    if (((cell.Raw >> 10) & 7) != palette)
                        throw new InvalidDataException(
                            $"Gameplay-message notice {id} mixes palettes in text region {region.Row}:{region.Column}, column {column}.");
                    return GameplayMessageTitlePresentation.DecodeGlyph(cell.Raw);
                }).ToArray());
                regions.Add(new()
                {
                    Row = region.Row,
                    Column = region.Column,
                    Width = region.Width,
                    Alignment = region.Alignment,
                    Text = region.Alignment == GameplayMessageNoticeDefinitions.CenterAlignment
                        ? padded.Trim()
                        : padded.TrimEnd(),
                    Palette = palette,
                });
            }

            GameplayMessageTitleCell[]? yes = null;
            GameplayMessageTitleCell[]? no = null;
            if (GameplayMessageNoticeDefinitions.IsSaveConfirmation(id))
            {
                yes = ReadSelection(bus, GameplayMessageRomData.Layout.SaveSelectionYesSourceWord);
                no = ReadSelection(bus, GameplayMessageRomData.Layout.SaveSelectionNoSourceWord);
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

        var border = new GameplayMessageTitleCell[GameplayMessageRomData.Layout.TilemapWidth];
        for (int column = 0; column < border.Length; column++)
            border[column] = new() { Raw = CartridgeImportSource.Require(bus).ReadCartridgeWord((GameplayMessageRomData.Assets.SmallBorder + column * sizeof(ushort))) };

        using var output = new MemoryStream();
        GameplayMessageNoticePresentation.Write(output, new()
        {
            Version = GameplayMessageNoticeDefinitions.Version,
            Border = border,
            Notices = notices,
        });
        return output.ToArray();
    }

    /// <summary>Copies one native save-confirmation choice row as raw tilemap
    /// cells so YES and NO selection graphics remain distinct from editable text.</summary>
    private static GameplayMessageTitleCell[] ReadSelection(
        ISnesAddressSpace bus,
        int sourceWord)
    {
        var result = new GameplayMessageTitleCell[
            GameplayMessageRomData.Layout.SaveSelectionRowWords];
        for (int word = 0; word < result.Length; word++)
        {
            result[word] = new() { Raw = CartridgeImportSource.Require(bus).ReadCartridgeWord((GameplayMessageRomData.Assets.SaveSelectionTilemap +
                (sourceWord + word) * sizeof(ushort))) };
        }
        return result;
    }
}
