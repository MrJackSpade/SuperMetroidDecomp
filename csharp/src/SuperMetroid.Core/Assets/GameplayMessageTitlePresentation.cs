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
    /// <summary>Validated title text and palette compiled from the authored message-name entries.</summary>
    private readonly Dictionary<GameplayMessageId, CompiledTitle> titles;

    /// <summary>Creates the runtime presentation from already validated title entries and border words.</summary>
    /// <param name="titles">Per-message strings and palette selectors used to build title rows.</param>
    /// <param name="border">The 32 native BG3 words copied to the top and bottom rows.</param>
    /// <param name="contentIdentity">Uppercase SHA-256 identity of the exact source JSON bytes.</param>
    private GameplayMessageTitlePresentation(
        Dictionary<GameplayMessageId, CompiledTitle> titles,
        ushort[] border,
        string contentIdentity)
    {
        this.titles = titles;
        Border = border;
        ContentIdentity = contentIdentity;
    }

    /// <summary>Native border words reused for both outer rows of every built message tilemap.</summary>
    private ushort[] Border { get; }
    /// <summary>Uppercase SHA-256 of the exact source JSON bytes read during loading, including formatting and property order rather than only compiled title content.</summary>
    public string ContentIdentity { get; }

    /// <summary>Reports whether this catalog owns the message's one-row title; large item panels and separate notices are not owned here.</summary>
    /// <param name="messageId">Message identity to query without constructing a tilemap.</param>
    /// <returns>True for the fifteen supported one-row item and status titles.</returns>
    public bool Contains(GameplayMessageId messageId) => titles.ContainsKey(messageId);

    /// <summary>Builds the complete three-row message tilemap without cartridge reads.</summary>
    /// <param name="messageId">Supported one-row message whose installed title and palette are selected.</param>
    /// <returns>A new caller-owned 96-word BG3 tilemap in 32-column row-major order, with the same supplied border above and below its title row.</returns>
    /// <remarks>The title is centered within columns 6..24, rounding its starting column down when padding is uneven. Six left and seven right outer cells remain transparent.</remarks>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="messageId"/> is not owned by this title catalog.</exception>
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

    /// <summary>Validates the complete one-row message set and retains independent border words and title data for later tilemap construction.</summary>
    /// <param name="json">UTF-8 JSON read from its current position to the end and left open; its exact bytes determine <see cref="ContentIdentity"/>.</param>
    /// <returns>An immutable presentation that does not retain the document's mutable title dictionary or border-cell array.</returns>
    /// <exception cref="InvalidDataException">JSON is invalid or ambiguous, its version, message set or border width is wrong, or a title is empty, too wide, uses unsupported glyphs or has an invalid palette.</exception>
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
            document = JsonAssetDocument.Read<GameplayMessageTitleDocument>(
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

    /// <summary>Serializes and validates every installed title and the shared border before writing any UTF-8 JSON bytes.</summary>
    /// <param name="output">Destination stream written at its current position and left open; existing trailing bytes are not truncated.</param>
    /// <param name="document">Authored title dictionary and border array read for serialization, not retained by the writer.</param>
    /// <exception cref="InvalidDataException">The serialized document fails <see cref="Load"/>'s schema or title validation.</exception>
    public static void Write(Stream output, GameplayMessageTitleDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        output.Write(bytes);
    }

    /// <summary>Encodes one supported title character as a BG3 tile word with the requested palette.</summary>
    /// <param name="character">Uppercase letter, space, hyphen, period, or question mark to encode.</param>
    /// <param name="palette">BG palette selector in the range 0 through 7.</param>
    /// <returns>Tile word containing the native character index, palette, and priority bit.</returns>
    /// <exception cref="InvalidDataException"><paramref name="character"/> is not a supported title glyph.</exception>
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

    /// <summary>Decodes a BG3 tile word's character index into a supported title glyph.</summary>
    /// <param name="word">Tile word whose low ten bits contain the character index.</param>
    /// <returns>The uppercase letter or punctuation represented by the character index.</returns>
    /// <exception cref="InvalidDataException">The character index does not belong to the supported title alphabet.</exception>
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

    /// <summary>Validated runtime title data retained after the mutable JSON document is compiled.</summary>
    /// <param name="Text">Nonempty, supported title glyphs to place in the message's title row.</param>
    /// <param name="Palette">BG palette selector applied to each encoded glyph.</param>
    private sealed record CompiledTitle(string Text, int Palette);
}

/// <summary>Editable schema for the fifteen one-row gameplay messages and their shared border; collections remain caller-mutable until compilation.</summary>
public sealed record GameplayMessageTitleDocument
{
    /// <summary>Schema revision; loading requires <see cref="GameplayMessageTitleDefinitions.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Thirty-two native BG3 words copied unchanged to both outer rows; the stock row is Small_MessageBox_TopBottomBorder_Tilemap at $85:8040.</summary>
    public required GameplayMessageTitleCell[] Border { get; init; }
    /// <summary>Exactly the fifteen case-sensitive message-name keys listed by <see cref="GameplayMessageTitleDefinitions.MessageIds"/>, each with a nonnull title definition.</summary>
    public required Dictionary<string, GameplayMessageTitle> Titles { get; init; }
}

/// <summary>One selected title string and BG palette; its placement is centered by the builder rather than supplied as gameplay state.</summary>
public sealed record GameplayMessageTitle
{
    /// <summary>Nonempty text of at most nineteen characters, using uppercase A..Z, space, hyphen, period or question mark; no arbitrary Unicode glyph substitution is performed.</summary>
    public required string Text { get; init; }
    /// <summary>BG palette selector 0..7 applied to title glyphs and the space-filled interior, with BG priority set and no reflection flags.</summary>
    public required int Palette { get; init; }
}

/// <summary>One lossless native BG tilemap word used by authored message borders and templates, not an atlas coordinate or controller instruction.</summary>
public sealed record GameplayMessageTitleCell
{
    /// <summary>Complete 16-bit word: character index in bits 0..9, palette in 10..12, priority in 13 and horizontal/vertical reflections in 14/15.</summary>
    public required ushort Raw { get; init; }
}
