using System.Text.Json;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Areas.Escape;

/// <summary>Extracts safe text and placement from the two native escape typewriter streams.</summary>
public static class EscapeTypewriterExtractor
{
    /// <summary>$A6:C2D6: the control word that sets the character delay from the word that follows.</summary>
    internal const int DelayCommand = 1;

    /// <summary>Imports the Ceres and Zebes escape-warning text and line destinations from their native typewriter streams.</summary>
    /// <param name="bus">Non-null cartridge address space supplying the two warning programs.</param>
    /// <returns>New UTF-8 JSON bytes containing ordered lines of supported text and the pixel of the layer each starts at.</returns>
    /// <remarks>The native character-delay header is checked, while typewriter timing, glyph artwork, and audio behavior remain compiled.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="InvalidDataException">A program has an unexpected header or command, unsupported characters, excessive lines, or invalid text placement.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        var messages = new Dictionary<string, EscapeTypewriterMessageDocument>(StringComparer.Ordinal);
        foreach (EscapeTypewriterMessageId id in Enum.GetValues<EscapeTypewriterMessageId>())
        {
            if (id == EscapeTypewriterMessageId.None) continue;
            messages.Add(id.ToString(), new() { Lines = ReadProgram(bus, id) });
        }
        using var output = new MemoryStream();
        Write(output, new()
        {
            Version = EscapeTypewriterDocumentDefinitions.Version,
            Messages = messages,
        });
        return output.ToArray();
    }

    /// <summary>Parses one native warning stream, requiring its delay header and at most sixteen destination-prefixed text lines.</summary>
    private static EscapeTypewriterLineDocument[] ReadProgram(
        ISnesAddressSpace bus,
        EscapeTypewriterMessageId id)
    {
        int cursor = id switch
        {
            EscapeTypewriterMessageId.Ceres => (int)EscapeSourceDefinitions.CeresWarningText,
            EscapeTypewriterMessageId.Zebes => (int)EscapeSourceDefinitions.ZebesWarningText,
            EscapeTypewriterMessageId.None => throw new ArgumentOutOfRangeException(nameof(id), id, "No native escape text source exists."),
            _ => throw new ArgumentOutOfRangeException(nameof(id), id, "No native escape text source exists."),
        };
        if (CartridgeImportSource.Require(bus).ReadCartridgeWord(cursor) != DelayCommand ||
            CartridgeImportSource.Require(bus).ReadCartridgeWord((cursor + 2)) != EscapeTypewriterDefinitions.CharacterDelayFrames)
        {
            throw new InvalidDataException($"Escape typewriter program {id} has an unexpected delay header.");
        }
        cursor += 4;
        var lines = new List<EscapeTypewriterLineDocument>();
        for (int lineIndex = 0; lineIndex < 16; lineIndex++)
        {
            var command = (EscapeTypewriterCommand)CartridgeImportSource.Require(bus).ReadCartridgeWord(cursor);
            if (!Enum.IsDefined(command))
                throw new InvalidDataException($"Escape typewriter program {id} expected a destination at ${cursor:X6}.");
            switch (command)
            {
                case EscapeTypewriterCommand.End:
                    return lines.ToArray();
                case EscapeTypewriterCommand.LineStart:
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(EscapeTypewriterCommand)} {command}.");
            }
            int destination = CartridgeImportSource.Require(bus).ReadCartridgeWord((cursor + 2));
            cursor += 4;
            var characters = new List<char>();
            while (true)
            {
                // Either control word ends the line's characters.
                if (Enum.IsDefined((EscapeTypewriterCommand)CartridgeImportSource.Require(bus).ReadCartridgeWord(cursor))) break;
                char character = (char)bus.ReadCartridgeByte(cursor++);
                if (character is not (' ' or '!' or >= 'A' and <= 'Z'))
                    throw new InvalidDataException(
                        $"Escape typewriter program {id} has unsupported byte ${(byte)character:X2} at ${cursor - 1:X6}.");
                characters.Add(character);
            }
            Position16<PlanePixels> start = StartOf(destination - TilemapWordOf(id));
            lines.Add(new() { Start = start, Text = new(characters.ToArray()) });
        }
        throw new InvalidDataException($"Escape typewriter program {id} exceeded sixteen lines.");
    }

    /// <summary>
    /// The pixel of the layer the tilemap cell <paramref name="cell"/> words past the program's tilemap start shows at
    /// (#1514, #1601): the tilemap counts its eight-pixel cells row by row through the left screen and then through the
    /// right one.
    /// </summary>
    private static Position16<PlanePixels> StartOf(int cell)
    {
        int screen = cell / TilemapScreenGeometry.CellCount, within = cell % TilemapScreenGeometry.CellCount;
        return new((screen * TilemapScreenGeometry.WidthInTiles + within % TilemapScreenGeometry.WidthInTiles) * ScreenGeometry.BackgroundTileSizePixels,
            within / TilemapScreenGeometry.WidthInTiles * ScreenGeometry.BackgroundTileSizePixels);
    }

    /// <summary>
    /// The VRAM word of the tilemap <paramref name="id"/>'s program types over: the Ceres warning types over BG1, the Zebes
    /// warning over BG2.
    /// </summary>
    private static int TilemapWordOf(EscapeTypewriterMessageId id) => id switch
    {
        EscapeTypewriterMessageId.Ceres => GameplayVramLayout.Bg1TilemapWord,
        EscapeTypewriterMessageId.Zebes => GameplayVramLayout.Bg2TilemapWord,
        EscapeTypewriterMessageId.None => throw new ArgumentOutOfRangeException(nameof(id), id, "No native escape text source exists."),
        _ => throw new ArgumentOutOfRangeException(nameof(id), id, "No native escape text source exists."),
    };

    /// <summary>Serializes both warning programs and writes their UTF-8 document bytes to the destination.</summary>
    /// <param name="output">Non-null caller-owned writable stream, written at its current position without being disposed.</param>
    /// <param name="document">Selected programs to serialize.</param>
    public static void Write(Stream output, EscapeTypewriterDocument document) =>
        output.Write(JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options));
}
