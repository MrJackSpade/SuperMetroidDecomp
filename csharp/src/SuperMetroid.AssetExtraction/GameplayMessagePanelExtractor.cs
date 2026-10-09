using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts the seven large item-instruction panels as editable presentation data.</summary>
public static class GameplayMessagePanelExtractor
{
    /// <summary>Imports the seven large item-instruction panels with their raw templates, decoded titles, and shared border.</summary>
    /// <param name="bus">Non-null cartridge address space supplying message definitions and the four-row panel tilemaps.</param>
    /// <returns>New UTF-8 JSON bytes containing each panel's title, starting tile column, palette index, and preserved BG-cell template.</returns>
    /// <remarks>Checks native shoot/run button-patching identities and panel lengths; current controller bindings and button replacement remain runtime behavior.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="InvalidDataException">A panel has an unexpected binding, draw routine, size, glyph, or title palette, or fails schema validation.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var panels = new Dictionary<string, GameplayMessagePanel>(StringComparer.Ordinal);
        foreach (GameplayMessageId id in GameplayMessagePanelDefinitions.MessageIds)
        {
            int definition = GameplayMessageRomData.Assets.DefinitionTable +
                ((byte)id - 1) * GameplayMessageRomData.Layout.DefinitionBytes;
            ushort setup = ReadWord(bus, definition);
            ushort draw = ReadWord(bus, definition + 2);
            ushort pointer = ReadWord(bus, definition + 4);
            ushort next = ReadWord(bus,
                definition + GameplayMessageRomData.Layout.DefinitionBytes + 4);
            ushort expectedSetup = GameplayMessagePanelDefinitions.ButtonBinding(id) switch
            {
                GameplayMessagePanelButtonBinding.Shoot =>
                    GameplayMessageRomData.Routines.PatchShootButton,
                GameplayMessagePanelButtonBinding.Run =>
                    GameplayMessageRomData.Routines.PatchRunButton,
                _ => throw new InvalidDataException(
                    $"Gameplay-message panel {id} lacks a compiled button binding."),
            };
            if (setup != expectedSetup ||
                draw != GameplayMessageRomData.Routines.DrawLargeTilemap ||
                next - pointer != GameplayMessagePanelDefinitions.ContentWords * sizeof(ushort))
            {
                throw new InvalidDataException(
                    $"Gameplay message {id} is not the expected four-row cartridge panel.");
            }

            var template = new GameplayMessageTitleCell[
                GameplayMessagePanelDefinitions.ContentWords];
            int content = GameplayMessageRomData.Assets.BankBase | pointer;
            for (int word = 0; word < template.Length; word++)
                template[word] = new() { Raw = ReadWord(bus, content + word * sizeof(ushort)) };

            GameplayMessageTitleCell[] visible = template.AsSpan(
                GameplayMessagePanelDefinitions.OuterLeftColumns,
                GameplayMessagePanelDefinitions.VisibleColumns).ToArray();
            int palette = (visible.First(cell =>
                GameplayMessageTitlePresentation.DecodeGlyph(cell.Raw) != ' ').Raw >> 10) & 7;
            string paddedTitle = new(visible.Select((cell, column) =>
            {
                if (((cell.Raw >> 10) & 7) != palette)
                    throw new InvalidDataException(
                        $"Gameplay-message panel {id} mixes palettes in title column {column}.");
                return GameplayMessageTitlePresentation.DecodeGlyph(cell.Raw);
            }).ToArray());
            int leadingSpaces = paddedTitle.Length - paddedTitle.TrimStart().Length;
            panels.Add(id.ToString(), new()
            {
                Title = paddedTitle.Trim(),
                Palette = palette,
                Column = GameplayMessagePanelDefinitions.OuterLeftColumns + leadingSpaces,
                Template = template,
            });
        }

        var border = new GameplayMessageTitleCell[GameplayMessageRomData.Layout.TilemapWidth];
        for (int column = 0; column < border.Length; column++)
            border[column] = new() { Raw = ReadWord(bus,
                GameplayMessageRomData.Assets.LargeBorder + column * sizeof(ushort)) };

        using var output = new MemoryStream();
        GameplayMessagePanelPresentation.Write(output, new()
        {
            Version = GameplayMessagePanelDefinitions.Version,
            Border = border,
            Panels = panels,
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
