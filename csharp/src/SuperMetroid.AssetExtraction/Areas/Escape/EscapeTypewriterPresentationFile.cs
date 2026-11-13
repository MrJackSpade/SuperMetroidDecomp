using System.Text.Json;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Areas.Escape;

/// <summary>Reads the installed escape-warning programs document into the programs the game types (#1502).</summary>
public static class EscapeTypewriterPresentationFile
{
    /// <summary>Loads the two editable escape-warning programs and validates their character alphabet and tile placement.</summary>
    /// <param name="json">Non-null caller-owned readable JSON stream, consumed from its current position without being disposed.</param>
    /// <returns>A catalog owning compiled line selections for the exact Ceres and Zebes key set.</returns>
    /// <remarks>Requires at least one line per program, with one through 32 characters per line drawn from space, uppercase A-Z, and exclamation mark, each line starting at a pixel of its layer from which the whole line stays inside the layer. Typewriter delay, glyph art, audio cadence, and escape behavior remain compiled.</remarks>
    /// <exception cref="InvalidDataException">The JSON, schema version, program set, line contents, or line placement is invalid.</exception>
    public static EscapeTypewriterPresentation Load(Stream json)
    {
        EscapeTypewriterDocument document;
        try
        {
            document = JsonAssetDocument.Read<EscapeTypewriterDocument>(json, ArtworkJson.Options)
                ?? throw new InvalidDataException("Escape typewriter document is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid escape typewriter JSON.", error);
        }

        if (document.Version != EscapeTypewriterDocumentDefinitions.Version || document.Programs is null)
            throw new InvalidDataException($"Escape typewriter content requires schema version {EscapeTypewriterDocumentDefinitions.Version}.");

        EscapeTypewriterMessageId[] expected = Enum.GetValues<EscapeTypewriterMessageId>()
            .Where(static id => id != EscapeTypewriterMessageId.None).ToArray();
        if (document.Programs.Count != expected.Length ||
            expected.Any(id => !document.Programs.ContainsKey(id.ToString())))
        {
            throw new InvalidDataException(
                "Escape typewriter content requires exactly the Ceres and Zebes programs.");
        }

        var programs = new Dictionary<EscapeTypewriterMessageId, EscapeTypewriterMessage>();
        foreach (EscapeTypewriterMessageId id in expected)
        {
            EscapeTypewriterProgramDocument program = document.Programs[id.ToString()]
                ?? throw new InvalidDataException($"Escape typewriter program {id} is null.");
            if (program.Lines is null || program.Lines.Length == 0)
                throw new InvalidDataException($"Escape typewriter program {id} has no lines.");
            var differences = new Dictionary<int, EscapeTypewriterLine>();
            for (int index = 0; index < program.Lines.Length; index++)
            {
                EscapeTypewriterLineDocument line = program.Lines[index]
                    ?? throw new InvalidDataException($"Escape typewriter program {id} line {index} is null.");
                if (string.IsNullOrEmpty(line.Text) || line.Text.Length > EscapeTypewriterDocumentDefinitions.MaximumLineLength ||
                    line.Text.Any(static character => character is not (' ' or '!' or >= 'A' and <= 'Z')) ||
                    !LineFitsLayer(line.Start, line.Text.Length))
                {
                    throw new InvalidDataException(
                        $"Escape typewriter program {id} line {index} has invalid text or placement.");
                }
                var supplied = new EscapeTypewriterLine(line.Start, line.Text);
                if (index >= EscapeTypewriterLines.LineCount(id) || supplied != EscapeTypewriterLines.Line(id, index))
                    differences.Add(index, supplied);
            }
            programs.Add(id, new(id, new EscapeTypewriterPresentation.LineSequence(id, program.Lines.Length, differences)));
        }

        return new(programs);
    }

    /// <summary>
    /// Whether a line of <paramref name="length"/> characters typed from layer pixel <paramref name="start"/> stays inside
    /// the layer, each character a glyph's width right of the last.
    /// </summary>
    private static bool LineFitsLayer(Position16<PlanePixels> start, int length) =>
        start.X >= 0 && start.Y >= 0 && start.X + length * EscapeTypewriterDefinitions.GlyphAdvance <= EscapeTypewriterDocumentDefinitions.LayerWidth
        && start.Y + EscapeTypewriterDocumentDefinitions.GlyphHeight <= EscapeTypewriterDocumentDefinitions.LayerHeight;
}
