using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Areas.Escape;
using SuperMetroid.Core.Areas.Escape.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Areas.Escape;

/// <summary>Extracts safe text and placement from the two native escape typewriter streams.</summary>
[CpuBusBoundary("codec: ROM escape typewriter text streams → escape typewriter text and placement")]
public static class EscapeTypewriterExtractor
{
    /// <summary>Imports the Ceres and Zebes escape-warning text and line destinations from their native typewriter streams.</summary>
    /// <param name="bus">Non-null cartridge address space supplying the two warning programs.</param>
    /// <returns>New UTF-8 JSON bytes containing ordered lines of supported text and starting VRAM word addresses, not pixel or byte coordinates.</returns>
    /// <remarks>The native character-delay header is checked, while typewriter timing, glyph artwork, and audio behavior remain compiled.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="InvalidDataException">A program has an unexpected header or command, unsupported characters, excessive lines, or invalid text placement.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        var programs = new Dictionary<string, EscapeTypewriterProgramDocument>(StringComparer.Ordinal);
        foreach (EscapeTypewriterProgramId id in Enum.GetValues<EscapeTypewriterProgramId>())
        {
            if (id == EscapeTypewriterProgramId.None) continue;
            programs.Add(id.ToString(), new() { Lines = ReadProgram(bus, id) });
        }
        using var output = new MemoryStream();
        EscapeTypewriterPresentation.Write(output, new()
        {
            Version = EscapeTypewriterDefinitions.Version,
            Programs = programs,
        });
        return output.ToArray();
    }

    /// <summary>Parses one native warning stream, requiring its delay header and at most sixteen destination-prefixed text lines.</summary>
    private static EscapeTypewriterLineDocument[] ReadProgram(
        ISnesAddressSpace bus,
        EscapeTypewriterProgramId id)
    {
        int cursor = EscapeTypewriterDefinitions.SourceAddress(id);
        if (CartridgeImportSource.Require(bus).ReadCartridgeWord(cursor) != EscapeTypewriterRomData.Delay ||
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
                case EscapeTypewriterCommand.Destination:
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
            lines.Add(new() { Destination = destination, Text = new(characters.ToArray()) });
        }
        throw new InvalidDataException($"Escape typewriter program {id} exceeded sixteen lines.");
    }
}
