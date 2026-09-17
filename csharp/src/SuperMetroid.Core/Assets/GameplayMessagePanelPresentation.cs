using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Editable UTF-8 titles and visual tile templates for the seven large item-instruction
/// panels. Controller substitution, timing and item behavior remain compiled.
/// </summary>
public sealed class GameplayMessagePanelPresentation
{
    private readonly Dictionary<GameplayMessageId, CompiledPanel> panels;
    private readonly ushort[] border;

    private GameplayMessagePanelPresentation(
        Dictionary<GameplayMessageId, CompiledPanel> panels,
        ushort[] border,
        string contentIdentity)
    {
        this.panels = panels;
        this.border = border;
        ContentIdentity = contentIdentity;
    }

    public string ContentIdentity { get; }

    public bool Contains(GameplayMessageId messageId) => panels.ContainsKey(messageId);

    /// <summary>Builds the complete six-row panel without cartridge reads.</summary>
    public ushort[] Build(GameplayMessageId messageId)
    {
        if (!panels.TryGetValue(messageId, out CompiledPanel? panel))
            throw new ArgumentOutOfRangeException(nameof(messageId), messageId,
                "The gameplay-message panel catalog does not own this message.");

        var result = new ushort[GameplayMessagePanelDefinitions.TilemapWords];
        border.CopyTo(result, 0);
        panel.Template.CopyTo(result, GameplayMessageRomData.Layout.TilemapWidth);
        border.CopyTo(result, result.Length - GameplayMessageRomData.Layout.TilemapWidth);

        Span<ushort> titleRow = result.AsSpan(
            GameplayMessageRomData.Layout.TilemapWidth +
                GameplayMessagePanelDefinitions.OuterLeftColumns,
            GameplayMessagePanelDefinitions.VisibleColumns);
        titleRow.Fill(GameplayMessageTitlePresentation.CompileGlyph(' ', panel.Palette));
        int start = panel.Column - GameplayMessagePanelDefinitions.OuterLeftColumns;
        for (int index = 0; index < panel.Title.Length; index++)
            titleRow[start + index] = GameplayMessageTitlePresentation.CompileGlyph(
                panel.Title[index], panel.Palette);
        return result;
    }

    public static GameplayMessagePanelPresentation Load(Stream json)
    {
        byte[] source;
        using (var buffer = new MemoryStream())
        {
            json.CopyTo(buffer);
            source = buffer.ToArray();
        }

        GameplayMessagePanelDocument document;
        try
        {
            document = JsonSerializer.Deserialize<GameplayMessagePanelDocument>(
                source, MapPresentationFormat.JsonOptions)
                ?? throw new InvalidDataException("Gameplay-message panel document is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid gameplay-message panel JSON.", error);
        }

        if (document.Version != GameplayMessagePanelDefinitions.Version ||
            document.Border is null ||
            document.Border.Length != GameplayMessageRomData.Layout.TilemapWidth ||
            document.Panels is null)
        {
            throw new InvalidDataException(
                "Gameplay-message panels require version 1, one 32-cell border and every named large panel.");
        }

        string[] expected = GameplayMessagePanelDefinitions.MessageIds.ToArray()
            .Select(static id => id.ToString()).ToArray();
        if (document.Panels.Count != expected.Length ||
            expected.Any(name => !document.Panels.ContainsKey(name)))
        {
            throw new InvalidDataException(
                "Gameplay-message panel JSON must contain the exact supported message-name set.");
        }

        var compiled = new Dictionary<GameplayMessageId, CompiledPanel>();
        foreach (GameplayMessageId id in GameplayMessagePanelDefinitions.MessageIds)
        {
            GameplayMessagePanel value = document.Panels[id.ToString()]
                ?? throw new InvalidDataException($"Gameplay-message panel {id} is null.");
            if (string.IsNullOrEmpty(value.Title) ||
                value.Title.Length > GameplayMessagePanelDefinitions.VisibleColumns ||
                value.Title.Any(static character =>
                    !GameplayMessageTitleDefinitions.IsSupportedGlyph(character)) ||
                (uint)value.Palette >= 8 ||
                value.Column < GameplayMessagePanelDefinitions.OuterLeftColumns ||
                value.Column + value.Title.Length >
                    GameplayMessageRomData.Layout.TilemapWidth -
                    GameplayMessagePanelDefinitions.OuterRightColumns ||
                value.Template is null ||
                value.Template.Length != GameplayMessagePanelDefinitions.ContentWords)
            {
                throw new InvalidDataException(
                    $"Gameplay-message panel {id} has unsupported title, palette or template dimensions.");
            }

            for (int column = 0; column < GameplayMessageRomData.Layout.TilemapWidth; column++)
            {
                if ((column < GameplayMessagePanelDefinitions.OuterLeftColumns ||
                     column >= GameplayMessageRomData.Layout.TilemapWidth -
                         GameplayMessagePanelDefinitions.OuterRightColumns) &&
                    value.Template[column].Raw != GameplayMessageTitleDefinitions.TransparentWord)
                {
                    throw new InvalidDataException(
                        $"Gameplay-message panel {id} title row has a nontransparent outer cell at {column}.");
                }
            }

            compiled.Add(id, new(value.Title, value.Palette, value.Column,
                value.Template.Select(static cell => cell.Raw).ToArray()));
        }

        return new(compiled, document.Border.Select(static cell => cell.Raw).ToArray(),
            Convert.ToHexString(SHA256.HashData(source)));
    }

    public static void Write(Stream output, GameplayMessagePanelDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(
            document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        output.Write(bytes);
    }

    private sealed record CompiledPanel(
        string Title,
        int Palette,
        int Column,
        ushort[] Template);
}

public sealed record GameplayMessagePanelDocument
{
    public required int Version { get; init; }
    public required GameplayMessageTitleCell[] Border { get; init; }
    public required Dictionary<string, GameplayMessagePanel> Panels { get; init; }
}

public sealed record GameplayMessagePanel
{
    public required string Title { get; init; }
    public required int Palette { get; init; }
    public required int Column { get; init; }
    public required GameplayMessageTitleCell[] Template { get; init; }
}
