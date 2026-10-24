using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>Editable escape-warning text and visual line placement.</summary>
public sealed class EscapeTypewriterPresentation
{
    /// <summary>Loaded Ceres warning lines and VRAM destinations.</summary>
    private readonly EscapeTypewriterProgram ceres;
    /// <summary>Loaded Zebes time-bomb warning lines and VRAM destinations.</summary>
    private readonly EscapeTypewriterProgram zebes;

    /// <summary>Retains the two validated programs and the identity of their exact source document bytes.</summary>
    private EscapeTypewriterPresentation(
        Dictionary<EscapeTypewriterProgramId, EscapeTypewriterProgram> programs,
        string contentIdentity)
    {
        ceres = programs[EscapeTypewriterProgramId.Ceres];
        zebes = programs[EscapeTypewriterProgramId.Zebes];
        ContentIdentity = contentIdentity;
    }

    /// <summary>Uppercase SHA-256 of the exact loaded UTF-8 document bytes, including authored formatting; not a hash of only decoded lines.</summary>
    public string ContentIdentity { get; }

    /// <summary>Returns the installed warning text and placement for one escape scenario.</summary>
    /// <param name="id">Ceres or Zebes program identity; None does not denote an installed program.</param>
    /// <returns>The retained program with its read-only ordered line sequence and native source identity.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="id"/> is None or another undefined program value.</exception>
    public EscapeTypewriterProgram Get(EscapeTypewriterProgramId id) => id switch
    {
        EscapeTypewriterProgramId.Ceres => ceres,
        EscapeTypewriterProgramId.Zebes => zebes,
        EscapeTypewriterProgramId.None => throw new ArgumentOutOfRangeException(nameof(id), id,
            "Escape typewriter program is not present in the installed catalog."),
        _ => throw new ArgumentOutOfRangeException(nameof(id), id,
            "Escape typewriter program is not present in the installed catalog."),
    };

    /// <summary>Loads the two editable escape-warning programs and validates their character alphabet and VRAM placement.</summary>
    /// <param name="json">Non-null caller-owned readable JSON stream, consumed from its current position without being disposed.</param>
    /// <returns>A catalog owning compiled line selections for the exact Ceres and Zebes key set.</returns>
    /// <remarks>Requires at least one line per program, with one through 32 characters per line drawn from space, uppercase A-Z, and exclamation mark. Line destinations are VRAM word addresses; the full text must remain below word $8000, but placement is not constrained to a single 32-column map row. Typewriter delay, glyph art, audio cadence, and escape behavior remain compiled.</remarks>
    /// <exception cref="InvalidDataException">The JSON, schema version, program set, line contents, or VRAM range is invalid.</exception>
    public static EscapeTypewriterPresentation Load(Stream json)
    {
        byte[] source;
        using (var buffer = new MemoryStream())
        {
            json.CopyTo(buffer);
            source = buffer.ToArray();
        }

        EscapeTypewriterDocument document;
        try
        {
            document = JsonAssetDocument.Read<EscapeTypewriterDocument>(
                source, ArtworkJson.Options)
                ?? throw new InvalidDataException("Escape typewriter document is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid escape typewriter JSON.", error);
        }

        if (document.Version != EscapeTypewriterDefinitions.Version || document.Programs is null)
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
                    !EscapeTypewriterState.LineFitsOverlay(line.Cell, line.Text.Length))
                {
                    throw new InvalidDataException(
                        $"Escape typewriter program {id} line {index} has invalid text or placement.");
                }
                var supplied = new EscapeTypewriterLine(line.Cell, line.Text);
                if (index >= EscapeTypewriterDefinitions.LineCount(id) || supplied != EscapeTypewriterDefinitions.Line(id, index))
                    differences.Add(index, supplied);
            }
            programs.Add(id, new(id, new LineSequence(id, program.Lines.Length, differences)));
        }

        return new(programs, Convert.ToHexString(SHA256.HashData(source)));
    }

    /// <summary>Reads stock lines from compiled definitions and stores only authored changes by line index.</summary>
    internal sealed class LineSequence(EscapeTypewriterProgramId id, int count,
        Dictionary<int, EscapeTypewriterLine> differences) : IReadOnlyList<EscapeTypewriterLine>
    {
        /// <summary>Edited or added lines that differ from the selected native warning program.</summary>
        internal readonly Dictionary<int, EscapeTypewriterLine> overrides = differences;
        /// <summary>Number of lines selected by the editable document for this warning.</summary>
        public int Count => count;
        /// <summary>Reads an authored override when present, otherwise the compiled native line.</summary>
        public EscapeTypewriterLine this[int index] => (uint)index < Count
            ? overrides.TryGetValue(index, out EscapeTypewriterLine? line) ? line : EscapeTypewriterDefinitions.Line(id, index)
            : throw new ArgumentOutOfRangeException(nameof(index));
        /// <summary>Yields warning lines in typewriter order, resolving stock lines lazily.</summary>
        public IEnumerator<EscapeTypewriterLine> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Exposes the same ordered line sequence through the non-generic collection interface.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>Serializes and validates both warning programs before writing their UTF-8 document bytes to the destination.</summary>
    /// <param name="output">Non-null caller-owned writable stream, written at its current position without being disposed.</param>
    /// <param name="document">Selected programs satisfying the same schema, alphabet, and placement limits as <see cref="Load"/>.</param>
    /// <exception cref="InvalidDataException">The document is null or fails warning-program validation.</exception>
    public static void Write(Stream output, EscapeTypewriterDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        output.Write(bytes);
    }
}
