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
    /// <remarks>Requires at least one line per program, with one through 32 characters per line drawn from space, uppercase A-Z, and exclamation mark, each line starting at an overlay cell from which the whole line stays inside the overlay. Typewriter delay, glyph art, audio cadence, and escape behavior remain compiled.</remarks>
    /// <exception cref="InvalidDataException">The JSON, schema version, program set, line contents, or overlay placement is invalid.</exception>
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
            throw new InvalidDataException("Escape typewriter content requires schema version 1.");

        EscapeTypewriterProgramId[] expected = Enum.GetValues<EscapeTypewriterProgramId>()
            .Where(static id => id != EscapeTypewriterProgramId.None).ToArray();
        if (document.Programs.Count != expected.Length ||
            expected.Any(id => !document.Programs.ContainsKey(id.ToString())))
        {
            throw new InvalidDataException(
                "Escape typewriter content requires exactly the Ceres and Zebes programs.");
        }

        var programs = new Dictionary<EscapeTypewriterProgramId, EscapeTypewriterProgram>();
        foreach (EscapeTypewriterProgramId id in expected)
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
                if (string.IsNullOrEmpty(line.Text) || line.Text.Length > EscapeTypewriterDefinitions.MaximumLineLength ||
                    line.Text.Any(static character => character is not (' ' or '!' or >= 'A' and <= 'Z')) ||
                    !LineFitsOverlay(line.Cell, line.Text.Length))
                {
                    throw new InvalidDataException(
                        $"Escape typewriter program {id} line {index} has invalid text or placement.");
                }
                var supplied = new EscapeTypewriterLine(CellOf(line.Cell), line.Text);
                if (index >= EscapeTypewriterLines.LineCount(id) || supplied != EscapeTypewriterLines.Line(id, index))
                    differences.Add(index, supplied);
            }
            programs.Add(id, new(id, new EscapeTypewriterPresentation.LineSequence(id, program.Lines.Length, differences)));
        }

        return new(programs);
    }

    /// <summary>The cells of the two side-by-side tilemap screens a document's overlay cell counts across.</summary>
    private const int OverlayCells = 2 * ScreenGeometry.TilemapScreenCellCount;

    /// <summary>Whether a line of <paramref name="length"/> glyphs typed from <paramref name="cell"/> stays inside the overlay.</summary>
    private static bool LineFitsOverlay(int cell, int length) =>
        cell >= 0 && cell + length <= OverlayCells;

    /// <summary>
    /// Where a document's overlay cell shows on its layer (#1514): the document counts cells row by row through the
    /// left screen and then through the right one.
    /// </summary>
    private static Position16<TilemapCells> CellOf(int cell)
    {
        int screen = cell / ScreenGeometry.TilemapScreenCellCount, within = cell % ScreenGeometry.TilemapScreenCellCount;
        return new(screen * ScreenGeometry.TilemapScreenWidthInTiles + within % ScreenGeometry.TilemapScreenWidthInTiles,
            within / ScreenGeometry.TilemapScreenWidthInTiles);
    }
}
