using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Editable UTF-8 titles for the cartridge's one-row gameplay message boxes.
/// Message timing, input handling and item identity remain compiled behavior.
/// </summary>
public sealed class GameplayMessageTitlePresentation
{
    private readonly Dictionary<GameplayMessageId, CompiledTitle> titles;

    private GameplayMessageTitlePresentation(
        Dictionary<GameplayMessageId, CompiledTitle> titles,
        ushort[] border,
        string contentIdentity)
    {
        this.titles = titles;
        Border = border;
        ContentIdentity = contentIdentity;
    }

    private ushort[] Border { get; }
    public string ContentIdentity { get; }

    public bool Contains(GameplayMessageId messageId) => titles.ContainsKey(messageId);

    /// <summary>Builds the complete three-row message tilemap without cartridge reads.</summary>
    public ushort[] Build(GameplayMessageId messageId)
    {
        if (!titles.TryGetValue(messageId, out CompiledTitle? title))
            throw new ArgumentOutOfRangeException(nameof(messageId), messageId,
                "The gameplay-message title catalog does not own this message.");

        var result = new ushort[GameplayMessageTitleDefinitions.TilemapWords];
        Border.CopyTo(result, 0);
        Border.CopyTo(result, GameplayMessageRomData.Layout.TilemapWidth * 2);
        Span<ushort> content = result.AsSpan(
            GameplayMessageRomData.Layout.TilemapWidth,
            GameplayMessageRomData.Layout.TilemapWidth);
        content[..GameplayMessageTitleDefinitions.OuterLeftColumns].Fill(
            GameplayMessageTitleDefinitions.TransparentWord);
        content[^GameplayMessageTitleDefinitions.OuterRightColumns..].Fill(
            GameplayMessageTitleDefinitions.TransparentWord);
        Span<ushort> visible = content.Slice(
            GameplayMessageTitleDefinitions.OuterLeftColumns,
            GameplayMessageTitleDefinitions.VisibleColumns);
        visible.Fill(CompileGlyph(' ', title.Palette));
        int start = (visible.Length - title.Text.Length) / 2;
        for (int index = 0; index < title.Text.Length; index++)
            visible[start + index] = CompileGlyph(title.Text[index], title.Palette);
        return result;
    }

    public static GameplayMessageTitlePresentation Load(Stream json)
    {
        byte[] source;
        using (var buffer = new MemoryStream())
        {
            json.CopyTo(buffer);
            source = buffer.ToArray();
        }

        GameplayMessageTitleDocument document;
        try
        {
            document = JsonSerializer.Deserialize<GameplayMessageTitleDocument>(
                source, MapPresentationFormat.JsonOptions)
                ?? throw new InvalidDataException("Gameplay-message title document is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid gameplay-message title JSON.", error);
        }

        if (document.Version != GameplayMessageTitleDefinitions.Version ||
            document.Border is null ||
            document.Border.Length != GameplayMessageRomData.Layout.TilemapWidth ||
            document.Titles is null)
        {
            throw new InvalidDataException(
                "Gameplay-message titles require version 1, one 32-cell border and every named one-row title.");
        }

        string[] expected = GameplayMessageTitleDefinitions.MessageIds.ToArray()
            .Select(static id => id.ToString()).ToArray();
        if (document.Titles.Count != expected.Length ||
            expected.Any(name => !document.Titles.ContainsKey(name)))
        {
            throw new InvalidDataException(
                "Gameplay-message title JSON must contain the exact supported message-name set.");
        }

        var compiled = new Dictionary<GameplayMessageId, CompiledTitle>();
        foreach (GameplayMessageId id in GameplayMessageTitleDefinitions.MessageIds)
        {
            GameplayMessageTitle value = document.Titles[id.ToString()]
                ?? throw new InvalidDataException($"Gameplay-message title {id} is null.");
            if (string.IsNullOrEmpty(value.Text) ||
                value.Text.Length > GameplayMessageTitleDefinitions.VisibleColumns ||
                value.Text.Any(static character => !GameplayMessageTitleDefinitions.IsSupportedGlyph(character)) ||
                (uint)value.Palette >= 8)
            {
                throw new InvalidDataException(
                    $"Gameplay-message title {id} has unsupported text, width or palette.");
            }
            compiled.Add(id, new(value.Text, value.Palette));
        }

        return new(compiled, document.Border.Select(static cell => cell.Raw).ToArray(),
            Convert.ToHexString(SHA256.HashData(source)));
    }

    public static void Write(Stream output, GameplayMessageTitleDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        output.Write(bytes);
    }

    internal static ushort CompileGlyph(char character, int palette)
    {
        int characterIndex = character switch
        {
            ' ' => GameplayMessageTitleDefinitions.SpaceCharacter,
            '-' => GameplayMessageTitleDefinitions.HyphenCharacter,
            '.' => GameplayMessageTitleDefinitions.PeriodCharacter,
            '?' => GameplayMessageTitleDefinitions.QuestionMarkCharacter,
            >= 'A' and <= 'Z' => GameplayMessageTitleDefinitions.UppercaseACharacter + character - 'A',
            _ => throw new InvalidDataException(
                $"Gameplay-message title glyph U+{(int)character:X4} is not supported."),
        };
        return unchecked((ushort)(characterIndex |
            GameplayMessageTitleDefinitions.PriorityWord | palette << 10));
    }

    internal static char DecodeGlyph(ushort word)
    {
        int character = word & 0x03ff;
        return character switch
        {
            GameplayMessageTitleDefinitions.SpaceCharacter => ' ',
            GameplayMessageTitleDefinitions.HyphenCharacter => '-',
            GameplayMessageTitleDefinitions.PeriodCharacter => '.',
            GameplayMessageTitleDefinitions.QuestionMarkCharacter => '?',
            >= GameplayMessageTitleDefinitions.UppercaseACharacter and
                <= GameplayMessageTitleDefinitions.UppercaseZCharacter =>
                (char)('A' + character - GameplayMessageTitleDefinitions.UppercaseACharacter),
            _ => throw new InvalidDataException(
                $"Gameplay-message title uses unsupported character ${character:X3}."),
        };
    }

    private sealed record CompiledTitle(string Text, int Palette);
}

public sealed record GameplayMessageTitleDocument
{
    public required int Version { get; init; }
    public required GameplayMessageTitleCell[] Border { get; init; }
    public required Dictionary<string, GameplayMessageTitle> Titles { get; init; }
}

public sealed record GameplayMessageTitle
{
    public required string Text { get; init; }
    public required int Palette { get; init; }
}

public sealed record GameplayMessageTitleCell
{
    public required ushort Raw { get; init; }
}
