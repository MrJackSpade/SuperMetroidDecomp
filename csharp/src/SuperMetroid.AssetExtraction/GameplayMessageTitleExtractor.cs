using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts the one-row item/status messages as editable UTF-8 titles.</summary>
public static class GameplayMessageTitleExtractor
{
    /// <summary>Decodes the fifteen editable one-row item/status titles and preserves their common 32-cell border as raw tilemap words.</summary>
    /// <param name="bus">Cartridge source for the bank-$85 message definitions, small title rows, and border artwork.</param>
    /// <returns>A new UTF-8 JSON buffer containing versioned, message-name-keyed trimmed text, one BG palette selector per title, and the border cells.</returns>
    /// <remarks>Requires small-message setup/draw routines, one-row source spans, transparent outer columns, and a uniform palette across each nineteen-cell title. No message timing or input behavior is exported.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="InvalidDataException">Native layout, transparency, palette consistency, or glyphs do not fit the editable title format.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var titles = new Dictionary<string, GameplayMessageTitle>(StringComparer.Ordinal);
        foreach (GameplayMessageId id in GameplayMessageTitleDefinitions.MessageIds)
        {
            int definition = GameplayMessageRomData.Assets.DefinitionTable +
                ((byte)id - 1) * GameplayMessageRomData.Layout.DefinitionBytes;
            ushort setup = ReadWord(bus, definition);
            ushort draw = ReadWord(bus, definition + 2);
            ushort pointer = ReadWord(bus, definition + 4);
            ushort next = ReadWord(bus, definition + GameplayMessageRomData.Layout.DefinitionBytes + 4);
            if (setup != GameplayMessageRomData.Routines.SetupSmall ||
                draw != GameplayMessageRomData.Routines.DrawSmallTilemap ||
                next - pointer != GameplayMessageRomData.Layout.TilemapWidth * sizeof(ushort))
            {
                throw new InvalidDataException(
                    $"Gameplay message {id} is not the expected one-row cartridge definition.");
            }

            var visible = new ushort[GameplayMessageTitleDefinitions.VisibleColumns];
            int content = GameplayMessageRomData.Assets.BankBase | pointer;
            for (int column = 0; column < GameplayMessageRomData.Layout.TilemapWidth; column++)
            {
                ushort word = ReadWord(bus, content + column * sizeof(ushort));
                if (column < GameplayMessageTitleDefinitions.OuterLeftColumns ||
                    column >= GameplayMessageRomData.Layout.TilemapWidth -
                        GameplayMessageTitleDefinitions.OuterRightColumns)
                {
                    if (word != GameplayMessageTitleDefinitions.TransparentWord)
                        throw new InvalidDataException(
                            $"Gameplay message {id} has a nontransparent outer cell at {column}.");
                    continue;
                }
                visible[column - GameplayMessageTitleDefinitions.OuterLeftColumns] = word;
            }

            int palette = (visible.First(word =>
                GameplayMessageTitlePresentation.DecodeGlyph(word) != ' ') >> 10) & 7;
            string text = new(visible.Select((word, column) =>
            {
                if (((word >> 10) & 7) != palette)
                    throw new InvalidDataException(
                        $"Gameplay message {id} mixes palettes inside its editable title at visible column {column}: ${word:X4} versus palette {palette}.");
                return GameplayMessageTitlePresentation.DecodeGlyph(word);
            }).ToArray());
            titles.Add(id.ToString(), new() { Text = text.Trim(), Palette = palette });
        }

        var border = new GameplayMessageTitleCell[GameplayMessageRomData.Layout.TilemapWidth];
        for (int column = 0; column < border.Length; column++)
            border[column] = new() { Raw = ReadWord(bus,
                GameplayMessageRomData.Assets.SmallBorder + column * sizeof(ushort)) };

        using var output = new MemoryStream();
        GameplayMessageTitlePresentation.Write(output, new()
        {
            Version = GameplayMessageTitleDefinitions.Version,
            Border = border,
            Titles = titles,
        });
        return output.ToArray();
    }

    /// <summary>Reads one little-endian tilemap or definition word from cartridge space.</summary>
    /// <param name="bus">Cartridge-backed address space.</param>
    /// <param name="address">Address of the word's low byte.</param>
    /// <returns>The two source bytes combined as an unsigned word.</returns>
    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadCartridgeByte(address) | bus.ReadCartridgeByte(address + 1) << 8));
}
