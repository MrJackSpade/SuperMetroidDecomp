using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Editable bounded text regions for station-completion and save-confirmation notices.
/// Confirmation input, timing and save behavior remain compiled.
/// </summary>
public sealed class GameplayMessageNoticePresentation
{
    private readonly Dictionary<GameplayMessageId, CompiledNotice> notices;
    private readonly ushort[] border;

    private GameplayMessageNoticePresentation(
        Dictionary<GameplayMessageId, CompiledNotice> notices,
        ushort[] border,
        string contentIdentity)
    {
        this.notices = notices;
        this.border = border;
        ContentIdentity = contentIdentity;
    }

    public string ContentIdentity { get; }

    public bool Contains(GameplayMessageId messageId) => notices.ContainsKey(messageId);

    public ushort[] Build(GameplayMessageId messageId)
    {
        CompiledNotice notice = Get(messageId);
        int width = GameplayMessageRomData.Layout.TilemapWidth;
        var result = new ushort[(notice.RowCount + GameplayMessageRomData.Layout.BorderRows) * width];
        border.CopyTo(result, 0);
        notice.Template.CopyTo(result, width);
        border.CopyTo(result, result.Length - width);

        Span<ushort> content = result.AsSpan(width, notice.Template.Length);
        foreach (CompiledTextRegion region in notice.Text)
        {
            Span<ushort> cells = content.Slice(region.Row * width + region.Column, region.Width);
            cells.Fill(GameplayMessageTitlePresentation.CompileGlyph(' ', region.Palette));
            int start = region.Alignment == GameplayMessageNoticeDefinitions.CenterAlignment
                ? (region.Width - region.Text.Length) / 2
                : 0;
            for (int index = 0; index < region.Text.Length; index++)
                cells[start + index] = GameplayMessageTitlePresentation.CompileGlyph(
                    region.Text[index], region.Palette);
        }
        return result;
    }

    public void ApplySelection(
        GameplayMessageId messageId,
        Span<ushort> tilemap,
        bool yesSelected)
    {
        CompiledNotice notice = Get(messageId);
        ushort[] source = yesSelected
            ? notice.YesSelection ?? throw new InvalidDataException(
                $"Gameplay-message notice {messageId} has no YES selection row.")
            : notice.NoSelection ?? throw new InvalidDataException(
                $"Gameplay-message notice {messageId} has no NO selection row.");
        if (tilemap.Length < GameplayMessageRomData.Layout.SaveSelectionDestinationWord +
            GameplayMessageRomData.Layout.SaveSelectionRowWords)
        {
            throw new InvalidDataException(
                "Save confirmation tilemap is too short for its installed selection row.");
        }
        source.CopyTo(tilemap.Slice(
            GameplayMessageRomData.Layout.SaveSelectionDestinationWord,
            GameplayMessageRomData.Layout.SaveSelectionRowWords));
    }

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
            document = JsonSerializer.Deserialize<GameplayMessageNoticeDocument>(
                source, MapPresentationFormat.JsonOptions)
                ?? throw new InvalidDataException("Gameplay-message notice document is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid gameplay-message notice JSON.", error);
        }

        if (document.Version != GameplayMessageNoticeDefinitions.Version ||
            document.Border is null ||
            document.Border.Length != GameplayMessageRomData.Layout.TilemapWidth ||
            document.Notices is null)
        {
            throw new InvalidDataException(
                "Gameplay-message notices require version 1, one 32-cell border and every named notice.");
        }

        string[] expected = GameplayMessageNoticeDefinitions.MessageIds.ToArray()
            .Select(static id => id.ToString()).ToArray();
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
            int rowCount = GameplayMessageNoticeDefinitions.ContentRows(id);
            int expectedWords = rowCount * GameplayMessageRomData.Layout.TilemapWidth;
            bool save = GameplayMessageNoticeDefinitions.IsSaveConfirmation(id);
            if (value.RowCount != rowCount || value.Template is null ||
                value.Template.Length != expectedWords || value.Text is null ||
                value.Text.Length == 0 ||
                save != (value.YesSelection is not null && value.NoSelection is not null) ||
                value.YesSelection is { Length: not GameplayMessageRomData.Layout.SaveSelectionRowWords } ||
                value.NoSelection is { Length: not GameplayMessageRomData.Layout.SaveSelectionRowWords })
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
                    region.Column + region.Width > GameplayMessageRomData.Layout.TilemapWidth ||
                    region.Text.Any(static character =>
                        !GameplayMessageTitleDefinitions.IsSupportedGlyph(character)) ||
                    region.Alignment is not (GameplayMessageNoticeDefinitions.LeftAlignment or
                        GameplayMessageNoticeDefinitions.CenterAlignment))
                {
                    throw new InvalidDataException(
                        $"Gameplay-message notice {id} has an invalid text region.");
                }
                int first = region.Row * GameplayMessageRomData.Layout.TilemapWidth +
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
                value.Template.Select(static cell => cell.Raw).ToArray(),
                text.ToArray(),
                value.YesSelection?.Select(static cell => cell.Raw).ToArray(),
                value.NoSelection?.Select(static cell => cell.Raw).ToArray()));
        }

        return new(compiled, document.Border.Select(static cell => cell.Raw).ToArray(),
            Convert.ToHexString(SHA256.HashData(source)));
    }

    public static void Write(Stream output, GameplayMessageNoticeDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(
            document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        output.Write(bytes);
    }

    private CompiledNotice Get(GameplayMessageId messageId) =>
        notices.TryGetValue(messageId, out CompiledNotice? notice)
            ? notice
            : throw new ArgumentOutOfRangeException(nameof(messageId), messageId,
                "The gameplay-message notice catalog does not own this message.");

    private sealed record CompiledNotice(
        int RowCount,
        ushort[] Template,
        CompiledTextRegion[] Text,
        ushort[]? YesSelection,
        ushort[]? NoSelection);

    private sealed record CompiledTextRegion(
        int Row,
        int Column,
        int Width,
        string Alignment,
        string Text,
        int Palette);
}

public sealed record GameplayMessageNoticeDocument
{
    public required int Version { get; init; }
    public required GameplayMessageTitleCell[] Border { get; init; }
    public required Dictionary<string, GameplayMessageNotice> Notices { get; init; }
}

public sealed record GameplayMessageNotice
{
    public required int RowCount { get; init; }
    public required GameplayMessageTitleCell[] Template { get; init; }
    public required GameplayMessageTextRegion[] Text { get; init; }
    public GameplayMessageTitleCell[]? YesSelection { get; init; }
    public GameplayMessageTitleCell[]? NoSelection { get; init; }
}

public sealed record GameplayMessageTextRegion
{
    public required int Row { get; init; }
    public required int Column { get; init; }
    public required int Width { get; init; }
    public required string Alignment { get; init; }
    public required string Text { get; init; }
    public required int Palette { get; init; }
}
