using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Hud;

/// <summary>Extracts the one-row item/status messages as editable UTF-8 titles.</summary>
public static class GameplayMessageTitleExtractor
{
    /// <summary>Decodes the fifteen editable one-row item/status titles and preserves their common 32-cell border as tiles.</summary>
    /// <param name="bus">Cartridge source for the bank-$85 message definitions, small title rows, and border artwork.</param>
    /// <returns>A new UTF-8 JSON buffer containing versioned, message-name-keyed trimmed text, one BG palette selector per title, and the border cells.</returns>
    /// <remarks>Requires small-message setup/draw routines, one-row source spans, transparent outer columns, and a uniform palette across each nineteen-cell title. No message timing or input behavior is exported.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="InvalidDataException">Native layout, transparency, palette consistency, or glyphs do not fit the editable title format.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        var titles = new Dictionary<string, GameplayMessageTitle>(StringComparer.Ordinal);
        foreach (GameplayMessageId id in GameplayMessageTitleDefinitions.MessageIds)
        {
            int definition = HudSourceDefinitions.Messages.DefinitionTable +
                ((byte)id - 1) * HudSourceDefinitions.Messages.DefinitionBytes;
            int setup = CartridgeImportSource.Require(bus).ReadCartridgeWord(definition);
            int draw = CartridgeImportSource.Require(bus).ReadCartridgeWord((definition + 2));
            int pointer = CartridgeImportSource.Require(bus).ReadCartridgeWord((definition + 4));
            int next = CartridgeImportSource.Require(bus).ReadCartridgeWord((definition + HudSourceDefinitions.Messages.DefinitionBytes + 4));
            if (GameplayMessageRoutineWords.Routines.Decode(setup, "message setup routine") != GameplayMessageRoutine.SetupSmall ||
                GameplayMessageRoutineWords.Routines.Decode(draw, "message draw routine") != GameplayMessageRoutine.DrawSmallTilemap ||
                next - pointer != GameplayMessageBoxDefinitions.Layout.TilemapWidth * sizeof(ushort))
            {
                throw new InvalidDataException(
                    $"Gameplay message {id} is not the expected one-row cartridge definition.");
            }

            var visible = new BackgroundTile[GameplayMessageTitleDefinitions.VisibleColumns];
            int content = HudSourceDefinitions.Messages.BankBase | pointer;
            for (int column = 0; column < GameplayMessageBoxDefinitions.Layout.TilemapWidth; column++)
            {
                BackgroundTile cell = BackgroundTileWords.ToTile(CartridgeImportSource.Require(bus).ReadCartridgeWord((content + column * sizeof(ushort))));
                if (column is < GameplayMessageTitleDefinitions.OuterLeftColumns or
                    >= (GameplayMessageBoxDefinitions.Layout.TilemapWidth -
                        GameplayMessageTitleDefinitions.OuterRightColumns))
                {
                    if (cell != GameplayMessageTitleDefinitions.TransparentTile)
                        throw new InvalidDataException(
                            $"Gameplay message {id} has a nontransparent outer cell at {column}.");
                    continue;
                }
                visible[column - GameplayMessageTitleDefinitions.OuterLeftColumns] = cell;
            }

            int palette = visible.First(cell =>
                GameplayMessageTitlePresentation.DecodeGlyph(cell) != ' ').ColourVariant;
            string text = new(visible.Select((cell, column) =>
            {
                if (cell.ColourVariant != palette)
                    throw new InvalidDataException(
                        $"Gameplay message {id} mixes palettes inside its editable title at visible column {column}: {cell} versus palette {palette}.");
                return GameplayMessageTitlePresentation.DecodeGlyph(cell);
            }).ToArray());
            titles.Add(id.ToString(), new() { Text = text.Trim(), Palette = palette });
        }

        var border = new BackgroundTile[GameplayMessageBoxDefinitions.Layout.TilemapWidth];
        for (int column = 0; column < border.Length; column++)
            border[column] = BackgroundTileWords.ToTile(CartridgeImportSource.Require(bus).ReadCartridgeWord((HudSourceDefinitions.Messages.SmallBorder + column * sizeof(ushort))));

        using var output = new MemoryStream();
        GameplayMessageTitlePresentation.Write(output, new()
        {
            Version = GameplayMessageTitleDefinitions.Version,
            Border = border,
            Titles = titles,
        });
        return output.ToArray();
    }
}
