using System.Collections.Frozen;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Hud.Artwork;

/// <summary>
/// Editable UTF-8 titles for the cartridge's one-row gameplay message boxes.
/// Message timing, input handling and item identity remain compiled behavior.
/// </summary>
public sealed class GameplayMessageTitlePresentation
{
    /// <summary>Validated one-row text and palette keyed by the supported gameplay message identities.</summary>
    private readonly Dictionary<GameplayMessageId, CompiledTitle> titles;

    /// <summary>Retains copied title and border data plus the source document's byte identity.</summary>
    private GameplayMessageTitlePresentation(
        Dictionary<GameplayMessageId, CompiledTitle> titles,
        ushort[] border,
        string contentIdentity)
    {
        this.titles = titles;
        Border = border;
        ContentIdentity = contentIdentity;
    }

    /// <summary>Thirty-two native BG3 words copied to both outer rows of every built message.</summary>
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
        Border.CopyTo(result, GameplayMessageBoxDefinitions.Layout.TilemapWidth * 2);
        Span<ushort> content = result.AsSpan(
            GameplayMessageBoxDefinitions.Layout.TilemapWidth,
            GameplayMessageBoxDefinitions.Layout.TilemapWidth);
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
                source, ArtworkJson.Options)
                ?? throw new InvalidDataException("Gameplay-message title document is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid gameplay-message title JSON.", error);
        }

        if (document.Version != GameplayMessageTitleDefinitions.Version ||
            document.Border is null ||
            document.Border.Length != GameplayMessageBoxDefinitions.Layout.TilemapWidth ||
            document.Titles is null)
        {
            throw new InvalidDataException(
                "Gameplay-message titles require version 1, one 32-cell border and every named one-row title.");
        }

        string[] expected = GameplayMessageTitleDefinitions.MessageIds
            .SelectToArray(static id => id.ToString());
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

        return new(compiled, document.Border.Select(static cell => cell.Word.Raw).ToArray(),
            Convert.ToHexString(SHA256.HashData(source)));
    }

    /// <summary>Serializes and validates every installed title and the shared border before writing any UTF-8 JSON bytes.</summary>
    /// <param name="output">Destination stream written at its current position and left open; existing trailing bytes are not truncated.</param>
    /// <param name="document">Authored title dictionary and border array read for serialization, not retained by the writer.</param>
    /// <exception cref="InvalidDataException">The serialized document fails <see cref="Load"/>'s schema or title validation.</exception>
    public static void Write(Stream output, GameplayMessageTitleDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        output.Write(bytes);
    }

    /// <summary>Encodes a supported title character as a priority BG3 tilemap word with the selected palette.</summary>
    internal static ushort CompileGlyph(char character, int palette) =>
        unchecked((ushort)(GlyphTile(character) |
            GameplayMessageTitleDefinitions.PriorityWord | palette << 10));

    /// <summary>The title tile that draws a supported character.</summary>
    private static ushort GlyphTile(char character) =>
        character switch
        {
            ' ' => GameplayMessageTitleDefinitions.SpaceTile,
            '-' => GameplayMessageTitleDefinitions.HyphenTile,
            '.' => GameplayMessageTitleDefinitions.PeriodTile,
            '?' => GameplayMessageTitleDefinitions.QuestionMarkTile,
            >= 'A' and <= 'Z' => (ushort)(GameplayMessageTitleDefinitions.FirstLetterTile + character - 'A'),
            _ => throw new InvalidDataException(
                $"Gameplay-message title glyph U+{(int)character:X4} is not supported."),
        };

    /// <summary>Recovers a supported title character from the tile portion of a BG3 word.</summary>
    internal static char DecodeGlyph(int word)
    {
        int tile = new SnesBgTilemapWord(checked((ushort)word)).CharacterIndex;
        return GlyphsByTile.TryGetValue(checked((ushort)tile), out char character)
            ? character
            : throw new InvalidDataException($"${tile:X4} is not a gameplay-message title glyph.");
    }

    /// <summary>Every character a title can show.</summary>
    private const string TitleAlphabet = " -.?ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    /// <summary>Each title character keyed by the tile <see cref="GlyphTile"/> draws it with.</summary>
    private static readonly FrozenDictionary<ushort, char> GlyphsByTile =
        TitleAlphabet.ToFrozenDictionary(GlyphTile, character => character);

    /// <summary>Validated title text and palette retained independently of the mutable JSON document.</summary>
    private sealed record CompiledTitle(string Text, int Palette);
}
