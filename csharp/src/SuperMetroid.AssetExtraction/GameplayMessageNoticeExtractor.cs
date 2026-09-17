using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts completion and save-confirmation notices as bounded UTF-8 text regions.</summary>
public static class GameplayMessageNoticeExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var notices = new Dictionary<string, GameplayMessageNotice>(StringComparer.Ordinal);
        foreach (GameplayMessageId id in GameplayMessageNoticeDefinitions.MessageIds)
        {
            int definition = GameplayMessageRomData.Assets.DefinitionTable +
                ((byte)id - 1) * GameplayMessageRomData.Layout.DefinitionBytes;
            ushort setup = ReadWord(bus, definition);
            ushort draw = ReadWord(bus, definition + 2);
            ushort pointer = ReadWord(bus, definition + 4);
            int rowCount = GameplayMessageNoticeDefinitions.ContentRows(id);
            ushort expectedSetup = GameplayMessageNoticeDefinitions.IsSaveConfirmation(id)
                ? GameplayMessageRomData.Routines.SetupLarge
                : GameplayMessageRomData.Routines.SetupSmall;
            if (setup != expectedSetup ||
                draw != GameplayMessageRomData.Routines.DrawSmallTilemap)
            {
                throw new InvalidDataException(
                    $"Gameplay message {id} is not the expected completion/save notice.");
            }

            var template = new GameplayMessageTitleCell[
                rowCount * GameplayMessageRomData.Layout.TilemapWidth];
            int content = GameplayMessageRomData.Assets.BankBase | pointer;
            for (int word = 0; word < template.Length; word++)
                template[word] = new() { Raw = ReadWord(bus, content + word * sizeof(ushort)) };

            var regions = new List<GameplayMessageTextRegion>();
            foreach (GameplayMessageTextRegionDefinition region in
                GameplayMessageNoticeDefinitions.StockTextRegions(id))
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
            border[column] = new() { Raw = ReadWord(bus,
                GameplayMessageRomData.Assets.SmallBorder + column * sizeof(ushort)) };

        using var output = new MemoryStream();
        GameplayMessageNoticePresentation.Write(output, new()
        {
            Version = GameplayMessageNoticeDefinitions.Version,
            Border = border,
            Notices = notices,
        });
        return output.ToArray();
    }

    private static GameplayMessageTitleCell[] ReadSelection(
        ISnesAddressSpace bus,
        int sourceWord)
    {
        var result = new GameplayMessageTitleCell[
            GameplayMessageRomData.Layout.SaveSelectionRowWords];
        for (int word = 0; word < result.Length; word++)
        {
            result[word] = new() { Raw = ReadWord(bus,
                GameplayMessageRomData.Assets.SaveSelectionTilemap +
                (sourceWord + word) * sizeof(ushort)) };
        }
        return result;
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8));
}
