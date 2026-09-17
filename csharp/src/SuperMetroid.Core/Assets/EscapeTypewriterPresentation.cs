using System.Security.Cryptography;
using System.Text.Json;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable escape-warning text and visual line placement.</summary>
public sealed class EscapeTypewriterPresentation
{
    private readonly Dictionary<EscapeTypewriterProgramId, EscapeTypewriterProgram> programs;

    private EscapeTypewriterPresentation(
        Dictionary<EscapeTypewriterProgramId, EscapeTypewriterProgram> programs,
        string contentIdentity)
    {
        this.programs = programs;
        ContentIdentity = contentIdentity;
    }

    public string ContentIdentity { get; }

    public EscapeTypewriterProgram Get(EscapeTypewriterProgramId id) =>
        programs.TryGetValue(id, out EscapeTypewriterProgram? program)
            ? program
            : throw new ArgumentOutOfRangeException(nameof(id), id,
                "Escape typewriter program is not present in the installed catalog.");

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
            document = JsonSerializer.Deserialize<EscapeTypewriterDocument>(
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
            var lines = new EscapeTypewriterLine[program.Lines.Length];
            for (int index = 0; index < lines.Length; index++)
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
                lines[index] = new(unchecked((ushort)line.Destination), line.Text);
            }
            programs.Add(id, new(id, EscapeTypewriterDefinitions.SourceAddress(id), lines));
        }

        return new(programs, Convert.ToHexString(SHA256.HashData(source)));
    }

    public static void Write(Stream output, EscapeTypewriterDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        output.Write(bytes);
    }
}

public enum EscapeTypewriterProgramId : byte
{
    None,
    Ceres,
    Zebes,
}

public sealed record EscapeTypewriterProgram(
    EscapeTypewriterProgramId Id,
    int SourceAddress,
    EscapeTypewriterLine[] Lines);

public sealed record EscapeTypewriterLine(ushort Destination, string Text);

public sealed record EscapeTypewriterDocument
{
    public required int Version { get; init; }
    public required Dictionary<string, EscapeTypewriterProgramDocument> Programs { get; init; }
}

public sealed record EscapeTypewriterProgramDocument
{
    public required EscapeTypewriterLineDocument[] Lines { get; init; }
}

public sealed record EscapeTypewriterLineDocument
{
    public required int Destination { get; init; }
    public required string Text { get; init; }
}
