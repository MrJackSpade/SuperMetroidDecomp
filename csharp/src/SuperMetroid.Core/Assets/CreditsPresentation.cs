using System.Security.Cryptography;
using System.Text.Json;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable staff-credit text compiled into the cartridge's fixed row cadence.</summary>
public sealed class CreditsPresentation
{
    private readonly ushort[][] rows;

    private CreditsPresentation(ushort[][] rows, string contentIdentity)
    {
        this.rows = rows;
        ContentIdentity = contentIdentity;
    }

    public string ContentIdentity { get; }
    public int RowCount => rows.Length;
    public ReadOnlySpan<ushort> GetRow(int index) => rows[index];

    public static CreditsPresentation Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] source;
        using (var buffer = new MemoryStream())
        {
            json.CopyTo(buffer);
            source = buffer.ToArray();
        }

        CreditsPresentationDocument document;
        try
        {
            document = JsonSerializer.Deserialize<CreditsPresentationDocument>(
                source, MapPresentationFormat.JsonOptions)
                ?? throw new InvalidDataException("Credits document is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid credits JSON.", error);
        }

        IReadOnlyList<CreditsLineDefinition> definitions =
            CreditsPresentationDefinitions.Lines;
        if (document.Version != CreditsPresentationDefinitions.Version ||
            document.Lines is null || document.Lines.Length != definitions.Count)
        {
            throw new InvalidDataException(
                $"Credits require version {CreditsPresentationDefinitions.Version} and " +
                $"exactly {definitions.Count} ordered lines.");
        }

        var compiled = new List<ushort[]>(
            CreditsPresentationDefinitions.ExpectedCompiledRows);
        for (int index = 0; index < definitions.Count; index++)
        {
            CreditsLineDefinition definition = definitions[index];
            CreditsLineDocument line = document.Lines[index];
            if (!string.Equals(line.Id, definition.Id, StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    $"Credits line {index} must retain id '{definition.Id}'.");
            }
            if (line.Text is null || line.Text.Length == 0)
                throw new InvalidDataException($"Credits line '{definition.Id}' cannot be empty.");
            if ((uint)line.Palette > CreditsPresentationDefinitions.MaximumPalette)
                throw new InvalidDataException($"Credits line '{definition.Id}' has invalid palette {line.Palette}.");
            if (line.Column < 0 || line.Column + line.Text.Length >
                CreditsPresentationDefinitions.TilemapWidth)
            {
                throw new InvalidDataException(
                    $"Credits line '{definition.Id}' exceeds the 32-column tilemap.");
            }

            AddBlankRows(compiled, definition.BlankRowsBefore);
            CompileLine(compiled, definition, line);
        }
        AddBlankRows(compiled, CreditsPresentationDefinitions.TrailingBlankRows);
        if (compiled.Count != CreditsPresentationDefinitions.ExpectedCompiledRows)
        {
            throw new InvalidDataException(
                $"Credits compiled to {compiled.Count} rows; expected " +
                $"{CreditsPresentationDefinitions.ExpectedCompiledRows}.");
        }
        return new(compiled.ToArray(), Convert.ToHexString(SHA256.HashData(source)));
    }

    public static void Write(Stream output, CreditsPresentationDocument document)
    {
        ArgumentNullException.ThrowIfNull(output);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(
            document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        output.Write(bytes);
    }

    internal static CreditsPresentation FromCompiledRowsForVerification(
        params ushort[][] rows)
    {
        if (rows.Length == 0 || rows.Any(row =>
            row.Length != CreditsPresentationDefinitions.TilemapWidth))
        {
            throw new ArgumentException(
                "Verification credits rows must be nonempty 32-word rows.", nameof(rows));
        }
        return new(rows.Select(row => row.ToArray()).ToArray(), "VERIFICATION");
    }

    private static void CompileLine(List<ushort[]> target,
        CreditsLineDefinition definition, CreditsLineDocument line)
    {
        ushort attributes = unchecked((ushort)(line.Palette <<
            CreditsPresentationDefinitions.PaletteShift));
        ushort[] top = BlankRow();
        ushort[]? bottom = definition.Style == CreditsLineStyle.Large
            ? BlankRow()
            : null;
        for (int index = 0; index < line.Text.Length; index++)
        {
            char character = line.Text[index];
            ushort topGlyph;
            ushort? bottomGlyph;
            try
            {
                topGlyph = CreditsPresentationDefinitions.CompileGlyph(
                    character, definition.Style);
                bottomGlyph = bottom is null ? null :
                    CreditsPresentationDefinitions.CompileGlyph(
                        character, definition.Style, bottom: true);
            }
            catch (ArgumentOutOfRangeException error)
            {
                throw new InvalidDataException(
                    $"Credits line '{definition.Id}' contains an unsupported glyph.", error);
            }
            int column = line.Column + index;
            top[column] = ApplyAttributes(topGlyph, attributes);
            if (bottomGlyph is { } glyph)
                bottom![column] = ApplyAttributes(glyph, attributes);
        }
        target.Add(top);
        if (bottom is not null)
            target.Add(bottom);
    }

    private static ushort ApplyAttributes(ushort glyph, ushort attributes) =>
        glyph == CreditsPresentationDefinitions.BlankWord
            ? glyph
            : unchecked((ushort)(glyph | attributes));

    private static void AddBlankRows(List<ushort[]> target, int count)
    {
        for (int index = 0; index < count; index++)
            target.Add(BlankRow());
    }

    private static ushort[] BlankRow() =>
        Enumerable.Repeat(CreditsPresentationDefinitions.BlankWord,
            CreditsPresentationDefinitions.TilemapWidth).ToArray();
}

public sealed record CreditsPresentationDocument
{
    public required int Version { get; init; }
    public required CreditsLineDocument[] Lines { get; init; }
}

public sealed record CreditsLineDocument
{
    public required string Id { get; init; }
    public required string Text { get; init; }
    public required int Column { get; init; }
    public required int Palette { get; init; }
}
