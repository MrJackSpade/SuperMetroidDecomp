using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Hud.Artwork;

/// <summary>
/// Editable UTF-8 titles and visual tile templates for the seven large item-instruction
/// panels. Controller substitution, timing and item behavior remain compiled.
/// </summary>
public sealed class GameplayMessagePanelPresentation
{
    /// <summary>Validated titles and four-row templates for the seven large item-instruction messages.</summary>
    private readonly Dictionary<GameplayMessageId, CompiledPanel> panels;
    /// <summary>Thirty-two tiles copied above and below each panel's content.</summary>
    private readonly BackgroundTile[] border;

    /// <summary>Retains compiled panel tiles and a digest of the exact source JSON document.</summary>
    private GameplayMessagePanelPresentation(
        Dictionary<GameplayMessageId, CompiledPanel> panels,
        BackgroundTile[] border,
        string contentIdentity)
    {
        this.panels = panels;
        this.border = border;
        ContentIdentity = contentIdentity;
    }

    /// <summary>Uppercase SHA-256 of the exact source JSON bytes read during loading, including whitespace and property order rather than only compiled tile content.</summary>
    public string ContentIdentity { get; }

    /// <summary>Reports whether this catalog owns the message's large item-instruction panel; unrelated title and notice IDs return false.</summary>
    /// <param name="messageId">Message identity to query without building a tilemap.</param>
    /// <returns>True for the seven supported large item-instruction messages.</returns>
    public bool Contains(GameplayMessageId messageId) => panels.ContainsKey(messageId);

    /// <summary>Builds the complete six-row panel without cartridge reads.</summary>
    /// <param name="messageId">Supported large item-instruction message whose title and four-row template are selected.</param>
    /// <returns>A new caller-owned 192-tile message in 32-column row-major order; the message-box state subsequently patches its compiled controller-button slot.</returns>
    /// <remarks>The same border row is copied above and below the four content rows. The title replaces columns 3..28 of the first content row without changing the template's other rows.</remarks>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="messageId"/> is not owned by this panel catalog.</exception>
    public BackgroundTile[] Build(GameplayMessageId messageId)
    {
        if (!panels.TryGetValue(messageId, out CompiledPanel? panel))
            throw new ArgumentOutOfRangeException(nameof(messageId), messageId,
                "The gameplay-message panel catalog does not own this message.");

        var result = new BackgroundTile[GameplayMessagePanelDefinitions.TilemapWords];
        border.CopyTo(result, 0);
        panel.Template.CopyTo(result, GameplayMessageBoxDefinitions.Layout.TilemapWidth);
        border.CopyTo(result, result.Length - GameplayMessageBoxDefinitions.Layout.TilemapWidth);

        Span<BackgroundTile> titleRow = result.AsSpan(
            GameplayMessageBoxDefinitions.Layout.TilemapWidth +
                GameplayMessagePanelDefinitions.OuterLeftColumns,
            GameplayMessagePanelDefinitions.VisibleColumns);
        titleRow.Fill(GameplayMessageTitlePresentation.CompileGlyph(' ', panel.Palette));
        int start = panel.Column - GameplayMessagePanelDefinitions.OuterLeftColumns;
        for (int index = 0; index < panel.Title.Length; index++)
            titleRow[start + index] = GameplayMessageTitlePresentation.CompileGlyph(
                panel.Title[index], panel.Palette);
        return result;
    }

    /// <summary>Validates all seven large-panel definitions and retains independently owned border/template tiles and title data for later tilemap construction.</summary>
    /// <param name="json">UTF-8 JSON read from its current position to the end and left open; the exact consumed bytes determine <see cref="ContentIdentity"/>.</param>
    /// <returns>An immutable panel presentation that does not retain the document's mutable dictionary or tile-cell arrays.</returns>
    /// <exception cref="InvalidDataException">JSON is invalid or ambiguous, its version, message set or dimensions are wrong, or a title, palette, column or outer title-row tile violates the schema.</exception>
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
            document = JsonAssetDocument.Read<GameplayMessagePanelDocument>(
                source, ArtworkJson.Options)
                ?? throw new InvalidDataException("Gameplay-message panel document is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid gameplay-message panel JSON.", error);
        }

        if (document.Version != GameplayMessagePanelDefinitions.Version ||
            document.Border is null ||
            document.Border.Length != GameplayMessageBoxDefinitions.Layout.TilemapWidth ||
            document.Panels is null)
        {
            throw new InvalidDataException(
                $"Gameplay-message panels require version {GameplayMessagePanelDefinitions.Version}, one 32-cell border and every named large panel.");
        }

        string[] expected = GameplayMessagePanelDefinitions.MessageIds
            .SelectToArray(static id => id.ToString());
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
                    GameplayMessageBoxDefinitions.Layout.TilemapWidth -
                    GameplayMessagePanelDefinitions.OuterRightColumns ||
                value.Template is null ||
                value.Template.Length != GameplayMessagePanelDefinitions.ContentWords)
            {
                throw new InvalidDataException(
                    $"Gameplay-message panel {id} has unsupported title, palette or template dimensions.");
            }

            for (int column = 0; column < GameplayMessageBoxDefinitions.Layout.TilemapWidth; column++)
            {
                if ((column < GameplayMessagePanelDefinitions.OuterLeftColumns ||
                     column >= GameplayMessageBoxDefinitions.Layout.TilemapWidth -
                         GameplayMessagePanelDefinitions.OuterRightColumns) &&
                    value.Template[column] != GameplayMessageTitleDefinitions.TransparentTile)
                {
                    throw new InvalidDataException(
                        $"Gameplay-message panel {id} title row has a nontransparent outer cell at {column}.");
                }
            }

            compiled.Add(id, new(value.Title, value.Palette, value.Column, [.. value.Template]));
        }

        return new(compiled, [.. document.Border],
            Convert.ToHexString(SHA256.HashData(source)));
    }

    /// <summary>Serializes and validates the complete panel document before writing any UTF-8 JSON bytes.</summary>
    /// <param name="output">Destination stream written at its current position and left open; existing trailing bytes are not truncated.</param>
    /// <param name="document">Authored titles and tile arrays read for serialization, not retained by the writer.</param>
    /// <exception cref="InvalidDataException">The serialized document fails <see cref="Load"/>'s schema, title or layout validation.</exception>
    public static void Write(Stream output, GameplayMessagePanelDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(
            document, ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        output.Write(bytes);
    }

    /// <summary>Immutable title, palette, placement, and owned tiles for one large instruction panel.</summary>
    private sealed record CompiledPanel(
        string Title,
        int Palette,
        int Column,
        BackgroundTile[] Template);
}
