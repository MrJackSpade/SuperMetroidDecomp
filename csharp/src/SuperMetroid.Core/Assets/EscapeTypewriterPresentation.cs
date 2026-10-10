using System.Security.Cryptography;
using System.Text.Json;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable escape-warning text and visual line placement.</summary>
public sealed class EscapeTypewriterPresentation
{
    /// <summary>Validated Ceres warning program selected from the loaded catalog.</summary>
    private readonly EscapeTypewriterProgram ceres;

    /// <summary>Validated Zebes warning program selected from the loaded catalog.</summary>
    private readonly EscapeTypewriterProgram zebes;

    /// <summary>Stores the validated scenario programs and identity of the source document.</summary>
    /// <param name="programs">The loaded Ceres and Zebes programs keyed by scenario identity.</param>
    /// <param name="contentIdentity">Uppercase SHA-256 identity computed from the exact source document bytes.</param>
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
                source, MapPresentationFormat.JsonOptions)
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
                    line.Destination < 0 || line.Destination + line.Text.Length > 0x8000)
                {
                    throw new InvalidDataException(
                        $"Escape typewriter program {id} line {index} has invalid text or VRAM placement.");
                }
                var supplied = new EscapeTypewriterLine(unchecked((ushort)line.Destination), line.Text);
                if (index >= EscapeTypewriterDefinitions.LineCount(id) || supplied != EscapeTypewriterDefinitions.Line(id, index))
                    differences.Add(index, supplied);
            }
            programs.Add(id, new(id, EscapeTypewriterDefinitions.SourceAddress(id), new LineSequence(id, program.Lines.Length, differences)));
        }

        return new(programs, Convert.ToHexString(SHA256.HashData(source)));
    }

    /// <summary>Projects authored line replacements over the compiled stock sequence without copying unchanged lines.</summary>
    /// <param name="id">Scenario whose unchanged lines are supplied by the compiled definitions.</param>
    /// <param name="count">Number of lines exposed by this sequence.</param>
    /// <param name="differences">Authored replacements keyed by line index.</param>
    private sealed class LineSequence(EscapeTypewriterProgramId id, int count,
        Dictionary<int, EscapeTypewriterLine> differences) : IReadOnlyList<EscapeTypewriterLine>
    {
        /// <summary>Authored line replacements; absent indexes fall back to the scenario's compiled stock text.</summary>
        private readonly Dictionary<int, EscapeTypewriterLine> overrides = differences;

        /// <summary>Number of ordered lines in the selected warning program.</summary>
        public int Count => count;

        /// <summary>Gets an authored line when present, otherwise the compiled line at the same position.</summary>
        /// <param name="index">Zero-based line position in the warning.</param>
        /// <returns>The selected authored or compiled line.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The index is outside this sequence.</exception>
        public EscapeTypewriterLine this[int index] => (uint)index < Count
            ? overrides.TryGetValue(index, out EscapeTypewriterLine? line) ? line : EscapeTypewriterDefinitions.Line(id, index)
            : throw new ArgumentOutOfRangeException(nameof(index));

        /// <summary>Enumerates warning lines in display order, resolving authored replacements as they are requested.</summary>
        /// <returns>An enumerator over each selected line in sequence order.</returns>
        public IEnumerator<EscapeTypewriterLine> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Returns a non-generic enumerator over this sequence.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>Serializes and validates both warning programs before writing their UTF-8 document bytes to the destination.</summary>
    /// <param name="output">Non-null caller-owned writable stream, written at its current position without being disposed.</param>
    /// <param name="document">Selected programs satisfying the same schema, alphabet, and placement limits as <see cref="Load"/>.</param>
    /// <exception cref="InvalidDataException">The document is null or fails warning-program validation.</exception>
    public static void Write(Stream output, EscapeTypewriterDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        output.Write(bytes);
    }
}

/// <summary>Mutually exclusive escape-warning identities, independent of the editable English wording.</summary>
public enum EscapeTypewriterProgramId : byte
{
    /// <summary>No installed warning identity; used by a typewriter initialized only with a native text pointer and rejected by presentation lookup.</summary>
    None,
    /// <summary>Ceres colony self-destruct warning, native <c>TypewriterText_CeresEscapeTimer</c> at <c>$A6:C450</c>, following Ridley's retreat.</summary>
    Ceres,
    /// <summary>Zebes time-bomb warning, native <c>TypewriterText_ZebesEscapeTimer</c> at <c>$A6:C49C</c>, following Mother Brain's defeat.</summary>
    Zebes,
}

/// <summary>Selected visual warning program consumed by the shared escape typewriter, without cartridge bytecode or editable timing commands.</summary>
/// <param name="Id">Ceres or Zebes identity used to rebind content to saved playback state.</param>
/// <param name="SourceAddress">Full native SNES source identity, $A6:C450 or $A6:C49C; retained for cursor/debugger compatibility, not read during installed-program playback.</param>
/// <param name="Lines">Ordered text and VRAM placements; loaded catalogs expose a read-only compiled sequence, while this record constructor itself does not copy or validate a supplied list.</param>
public sealed record EscapeTypewriterProgram(
    EscapeTypewriterProgramId Id,
    int SourceAddress,
    IReadOnlyList<EscapeTypewriterLine> Lines);

/// <summary>One warning line's starting word address and selected text; this value record does not independently validate either field.</summary>
/// <param name="Destination">Starting VRAM word address, not a byte address or screen-pixel coordinate; loaded programs require the complete line to fit below word $8000.</param>
/// <param name="Text">One through 32 supported characters in a loaded program. Spaces advance the destination without writing a tile; other characters select the caller's glyph base.</param>
public sealed record EscapeTypewriterLine(ushort Destination, string Text);

/// <summary>Editable warning schema containing the exact two named escape programs.</summary>
public sealed record EscapeTypewriterDocument
{
    /// <summary>Schema revision; loading requires <see cref="EscapeTypewriterDefinitions.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Caller-owned mutable dictionary with exactly the case-sensitive keys Ceres and Zebes; None is not a document key.</summary>
    public required Dictionary<string, EscapeTypewriterProgramDocument> Programs { get; init; }
}

/// <summary>Editable ordered lines for one escape warning; the owning dictionary supplies its scenario identity.</summary>
public sealed record EscapeTypewriterProgramDocument
{
    /// <summary>Nonempty caller-owned mutable line array in typewriter order; stock Ceres has three lines and stock Zebes two, but authored line counts may differ.</summary>
    public required EscapeTypewriterLineDocument[] Lines { get; init; }
}

/// <summary>Editable text and linear VRAM placement for a single escape-warning line.</summary>
public sealed record EscapeTypewriterLineDocument
{
    /// <summary>Starting VRAM word address, at least zero, with <c>Destination + Text.Length</c> no greater than $8000 when loaded.</summary>
    public required int Destination { get; init; }
    /// <summary>Nonempty string of at most 32 spaces, uppercase A-Z letters, or exclamation marks; lowercase and other punctuation are rejected.</summary>
    public required string Text { get; init; }
}
