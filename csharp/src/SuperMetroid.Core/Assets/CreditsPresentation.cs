using System.Security.Cryptography;
using System.Text.Json;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable staff-credit text compiled into the cartridge's fixed row cadence.</summary>
public sealed class CreditsPresentation
{
    private readonly CreditsLineDocument[]? lines;
    private readonly ushort[][]? fixtureRows;

    private CreditsPresentation(CreditsLineDocument[]? lines, ushort[][]? fixtureRows, string contentIdentity)
    {
        this.lines = lines;
        this.fixtureRows = fixtureRows;
        ContentIdentity = contentIdentity;
    }

    /// <summary>Gets the uppercase SHA-256 identity of the exact loaded JSON bytes.</summary>
    public string ContentIdentity { get; }

    /// <summary>Gets the fixed number of 32-word tilemap rows in the compiled staff roll.</summary>
    public int RowCount => fixtureRows?.Length ?? CreditsPresentationDefinitions.ExpectedCompiledRows;

    /// <summary>Compiles one staff-roll tilemap row, including authored blank-row cadence.</summary>
    /// <param name="index">Zero-based compiled row index.</param>
    /// <returns>Thirty-two complete SNES BG tilemap words.</returns>
    public ReadOnlySpan<ushort> GetRow(int index)
    {
        if ((uint)index >= RowCount)
            throw new IndexOutOfRangeException();
        if (fixtureRows is not null)
            return fixtureRows[index];
        int row = 0;
        for (int line = 0; line < lines!.Length; line++)
        {
            var definition = CreditsPresentationDefinitions.Lines[line];
            row += definition.BlankRowsBefore;
            if (index < row)
                return BlankRow();
            int height = definition.Style == CreditsLineStyle.Large ? 2 : 1;
            if (index < row + height)
                return RenderLine(definition, lines[line], bottom: index != row);
            row += height;
        }
        return BlankRow();
    }
    /// <summary>Loads and validates the ordered editable staff-credit lines from JSON.</summary>
    /// <param name="json">Caller-owned stream containing a credits document.</param>
    /// <returns>The compiled credits presentation.</returns>
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
            document = JsonAssetDocument.Read<CreditsPresentationDocument>(
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

        int compiledRows = 0;
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

            compiledRows += definition.BlankRowsBefore + (definition.Style == CreditsLineStyle.Large ? 2 : 1);
            ValidateLine(definition, line);
        }
        compiledRows += CreditsPresentationDefinitions.TrailingBlankRows;
        if (compiledRows != CreditsPresentationDefinitions.ExpectedCompiledRows)
        {
            throw new InvalidDataException(
                $"Credits compiled to {compiledRows} rows; expected " +
                $"{CreditsPresentationDefinitions.ExpectedCompiledRows}.");
        }
        return new(document.Lines, null, Convert.ToHexString(SHA256.HashData(source)));
    }

    /// <summary>Validates and writes a credits document as JSON.</summary>
    /// <param name="output">Destination stream.</param>
    /// <param name="document">Document to validate and serialize.</param>
    public static void Write(Stream output, CreditsPresentationDocument document)
    {
        ArgumentNullException.ThrowIfNull(output);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(
            document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        output.Write(bytes);
    }

    private static void ValidateLine(CreditsLineDefinition definition, CreditsLineDocument line)
    {
        foreach (char character in line.Text)
        {
            try
            {
                _ = CreditsPresentationDefinitions.CompileGlyph(character, definition.Style);
                if (definition.Style == CreditsLineStyle.Large)
                    _ = CreditsPresentationDefinitions.CompileGlyph(character, definition.Style, bottom: true);
            }
            catch (ArgumentOutOfRangeException error)
            {
                throw new InvalidDataException(
                    $"Credits line '{definition.Id}' contains an unsupported glyph.", error);
            }
        }
    }

    private static ushort[] RenderLine(CreditsLineDefinition definition, CreditsLineDocument line, bool bottom)
    {
        ushort[] row = BlankRow();
        ushort attributes = (ushort)(line.Palette << CreditsPresentationDefinitions.PaletteShift);
        for (int index = 0; index < line.Text.Length; index++)
            row[line.Column + index] = ApplyAttributes(
                CreditsPresentationDefinitions.CompileGlyph(line.Text[index], definition.Style, bottom), attributes);
        return row;
    }
    private static ushort ApplyAttributes(ushort glyph, ushort attributes) =>
        glyph == CreditsPresentationDefinitions.BlankWord
            ? glyph
            : unchecked((ushort)(glyph | attributes));

    private static ushort[] BlankRow() =>
        Enumerable.Repeat(CreditsPresentationDefinitions.BlankWord,
            CreditsPresentationDefinitions.TilemapWidth).ToArray();
}

/// <summary>JSON schema for the complete ordered staff-credit script.</summary>
public sealed record CreditsPresentationDocument
{
    /// <summary>Gets the schema version required by <see cref="CreditsPresentationDefinitions.Version"/>.</summary>
    public required int Version { get; init; }

    /// <summary>Gets the editable lines in their fixed native display order.</summary>
    public required CreditsLineDocument[] Lines { get; init; }
}

/// <summary>Defines the text and tilemap placement of one staff-credit line.</summary>
public sealed record CreditsLineDocument
{
    /// <summary>Gets the stable line identifier matched against the native cadence definition.</summary>
    public required string Id { get; init; }

    /// <summary>Gets the text compiled with the line's predefined font style.</summary>
    public required string Text { get; init; }

    /// <summary>Gets the zero-based starting column in the 32-word tilemap row.</summary>
    public required int Column { get; init; }

    /// <summary>Gets the BG palette selector encoded into the compiled tilemap words.</summary>
    public required int Palette { get; init; }
}
