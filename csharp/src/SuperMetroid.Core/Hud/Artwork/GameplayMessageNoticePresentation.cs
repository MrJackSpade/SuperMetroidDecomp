using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Hud.Artwork;

/// <summary>
/// Editable bounded text regions for station-completion and save-confirmation notices.
/// Confirmation input, timing and save behavior remain compiled.
/// </summary>
public sealed class GameplayMessageNoticePresentation
{
    /// <summary>Validated templates and bounded text overlays indexed by supported message ID.</summary>
    private readonly Dictionary<GameplayMessageId, CompiledNotice> notices;
    /// <summary>The 32-cell row copied above and below every notice's content.</summary>
    private readonly BackgroundTile[] border;

    /// <summary>Captures compiled notice data together with the hash of its original JSON bytes.</summary>
    private GameplayMessageNoticePresentation(
        Dictionary<GameplayMessageId, CompiledNotice> notices,
        BackgroundTile[] border,
        string contentIdentity)
    {
        this.notices = notices;
        this.border = border;
        ContentIdentity = contentIdentity;
    }

    /// <summary>SHA-256 of the exact validated JSON bytes, including formatting differences.</summary>
    public string ContentIdentity { get; }

    /// <summary>Determines whether this catalog contains the specified gameplay message.</summary>
    /// <param name="messageId">The gameplay message to query.</param>
    /// <returns><see langword="true"/> when the message is present.</returns>
    public bool Contains(GameplayMessageId messageId) => notices.ContainsKey(messageId);

    /// <summary>Copies the shared borders and content template, then blanks and paints each bounded text field using its palette and alignment.</summary>
    /// <param name="messageId">The gameplay message to build.</param>
    /// <returns>The row-major 32-cell-wide tilemap.</returns>
    public BackgroundTile[] Build(GameplayMessageId messageId)
    {
        CompiledNotice notice = Get(messageId);
        int width = GameplayMessageBoxDefinitions.Layout.TilemapWidth;
        var result = new BackgroundTile[(notice.RowCount + GameplayMessageBoxDefinitions.Layout.BorderRows) * width];
        border.CopyTo(result, 0);
        notice.Template.CopyTo(result, width);
        border.CopyTo(result, result.Length - width);

        Span<BackgroundTile> content = result.AsSpan(width, notice.Template.Length);
        foreach (CompiledTextRegion region in notice.Text)
        {
            Span<BackgroundTile> cells = content.Slice(region.Row * width + region.Column, region.Width);
            cells.Fill(GameplayMessageTitlePresentation.CompileGlyph(' ', region.Palette));
            int start = region.Alignment == GameplayMessageTextAlignment.Center
                ? (region.Width - region.Text.Length) / 2
                : 0;
            for (int index = 0; index < region.Text.Length; index++)
                cells[start + index] = GameplayMessageTitlePresentation.CompileGlyph(
                    region.Text[index], region.Palette);
        }
        return result;
    }

    /// <summary>Replaces the fixed selection-row span in an already built save-confirmation tilemap with its YES or NO variant.</summary>
    /// <param name="messageId">The save-confirmation message that owns the rows.</param>
    /// <param name="tilemap">The complete message tilemap to update.</param>
    /// <param name="yesSelected">Whether to install the YES row instead of the NO row.</param>
    public void ApplySelection(
        GameplayMessageId messageId,
        Span<BackgroundTile> tilemap,
        bool yesSelected)
    {
        CompiledNotice notice = Get(messageId);
        BackgroundTile[] source = yesSelected
            ? notice.YesSelection ?? throw new InvalidDataException(
                $"Gameplay-message notice {messageId} has no YES selection row.")
            : notice.NoSelection ?? throw new InvalidDataException(
                $"Gameplay-message notice {messageId} has no NO selection row.");
        int firstCell = GameplayMessageBoxDefinitions.Layout.SaveSelectionRow * GameplayMessageBoxDefinitions.Layout.TilemapWidth;
        if (tilemap.Length < firstCell + GameplayMessageBoxDefinitions.Layout.SaveSelectionRowWords)
        {
            throw new InvalidDataException(
                "Save confirmation tilemap is too short for its installed selection row.");
        }
        source.CopyTo(tilemap.Slice(firstCell, GameplayMessageBoxDefinitions.Layout.SaveSelectionRowWords));
    }

    /// <summary>Requires the exact supported notice set and dimensions, nonoverlapping bounded text regions, valid glyphs, and selection rows only for save confirmations.</summary>
    /// <param name="json">The stream containing the JSON document.</param>
    /// <returns>The compiled notice presentation.</returns>
    public static GameplayMessageNoticePresentation Load(Stream json)
    {
        byte[] source;
        using (var buffer = new MemoryStream())
        {
            json.CopyTo(buffer);
            source = buffer.ToArray();
        }

        GameplayMessageNoticeDocument document;
        try
        {
            document = JsonAssetDocument.Read<GameplayMessageNoticeDocument>(
                source, ArtworkJson.Options)
                ?? throw new InvalidDataException("Gameplay-message notice document is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid gameplay-message notice JSON.", error);
        }

        if (document.Version != GameplayMessageNoticeDefinitions.Version ||
            document.Border is null ||
            document.Border.Length != GameplayMessageBoxDefinitions.Layout.TilemapWidth ||
            document.Notices is null)
        {
            throw new InvalidDataException(
                $"Gameplay-message notices require version {GameplayMessageNoticeDefinitions.Version}, one 32-cell border and every named notice.");
        }

        string[] expected = GameplayMessageNoticeDefinitions.MessageIds
            .SelectToArray(static id => id.ToString());
        if (document.Notices.Count != expected.Length ||
            expected.Any(name => !document.Notices.ContainsKey(name)))
        {
            throw new InvalidDataException(
                "Gameplay-message notice JSON must contain the exact supported message-name set.");
        }

        var compiled = new Dictionary<GameplayMessageId, CompiledNotice>();
        foreach (GameplayMessageId id in GameplayMessageNoticeDefinitions.MessageIds)
        {
            GameplayMessageNotice value = document.Notices[id.ToString()]
                ?? throw new InvalidDataException($"Gameplay-message notice {id} is null.");
            int rowCount = GameplayMessageNotices.ContentRows(id);
            int expectedWords = rowCount * GameplayMessageBoxDefinitions.Layout.TilemapWidth;
            bool save = GameplayMessageNotices.IsSaveConfirmation(id);
            if (value.RowCount != rowCount || value.Template is null ||
                value.Template.Length != expectedWords || value.Text is null ||
                value.Text.Length == 0 ||
                save != (value.YesSelection is not null && value.NoSelection is not null) ||
                value.YesSelection is { Length: not GameplayMessageBoxDefinitions.Layout.SaveSelectionRowWords } ||
                value.NoSelection is { Length: not GameplayMessageBoxDefinitions.Layout.SaveSelectionRowWords })
            {
                throw new InvalidDataException(
                    $"Gameplay-message notice {id} has invalid dimensions or selection rows.");
            }

            var occupied = new bool[expectedWords];
            var text = new List<CompiledTextRegion>(value.Text.Length);
            foreach (GameplayMessageTextRegion region in value.Text)
            {
                if (string.IsNullOrEmpty(region.Text) || region.Width <= 0 ||
                    region.Text.Length > region.Width || (uint)region.Palette >= 8 ||
                    region.Row < 0 || region.Row >= rowCount || region.Column < 0 ||
                    region.Column + region.Width > GameplayMessageBoxDefinitions.Layout.TilemapWidth ||
                    region.Text.Any(static character =>
                        !GameplayMessageGlyphs.IsSupportedGlyph(character)) ||
                    !Enum.IsDefined(region.Alignment))
                {
                    throw new InvalidDataException(
                        $"Gameplay-message notice {id} has an invalid text region.");
                }
                int first = region.Row * GameplayMessageBoxDefinitions.Layout.TilemapWidth +
                    region.Column;
                for (int index = first; index < first + region.Width; index++)
                {
                    if (occupied[index])
                        throw new InvalidDataException(
                            $"Gameplay-message notice {id} has overlapping text regions.");
                    occupied[index] = true;
                }
                text.Add(new(region.Row, region.Column, region.Width, region.Alignment,
                    region.Text, region.Palette));
            }

            compiled.Add(id, new(rowCount,
                [.. value.Template],
                text.ToArray(),
                value.YesSelection?.ToArray(),
                value.NoSelection?.ToArray()));
        }

        return new(compiled, [.. document.Border],
            Convert.ToHexString(SHA256.HashData(source)));
    }

    /// <summary>Serializes to memory and passes the result through normal loading validation before writing any bytes to the destination.</summary>
    /// <param name="output">The destination stream.</param>
    /// <param name="document">The document to validate and serialize.</param>
    public static void Write(Stream output, GameplayMessageNoticeDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(
            document, ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        output.Write(bytes);
    }

    /// <summary>Returns the owned message or rejects an unsupported ID before modifying its tilemap.</summary>
    private CompiledNotice Get(GameplayMessageId messageId) =>
        notices.TryGetValue(messageId, out CompiledNotice? notice)
            ? notice
            : throw new ArgumentOutOfRangeException(nameof(messageId), messageId,
                "The gameplay-message notice catalog does not own this message.");

    /// <summary>Validated tilemap content with optional fixed save-selection variants.</summary>
    private sealed record CompiledNotice(
        int RowCount,
        BackgroundTile[] Template,
        CompiledTextRegion[] Text,
        BackgroundTile[]? YesSelection,
        BackgroundTile[]? NoSelection);

    /// <summary>A nonoverlapping text span whose position, width, glyphs, palette, and alignment have been checked.</summary>
    private sealed record CompiledTextRegion(
        int Row,
        int Column,
        int Width,
        GameplayMessageTextAlignment Alignment,
        string Text,
        int Palette);
}
