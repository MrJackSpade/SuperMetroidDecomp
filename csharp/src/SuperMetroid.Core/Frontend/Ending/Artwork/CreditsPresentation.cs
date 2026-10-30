using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Editable staff-credit text compiled into the cartridge's fixed row cadence.</summary>
public sealed class CreditsPresentation
{
    /// <summary>Validated editable lines rendered on demand at the fixed cartridge cadence.</summary>
    private readonly CreditsLineDocument[] lines;

    /// <summary>Retains the lines and the identity of their original JSON bytes.</summary>
    private CreditsPresentation(CreditsLineDocument[] lines, string contentIdentity)
    {
        this.lines = lines;
        ContentIdentity = contentIdentity;
    }

    /// <summary>Gets the uppercase SHA-256 identity of the exact loaded JSON bytes.</summary>
    public string ContentIdentity { get; }

    /// <summary>Gets the fixed number of 32-tile rows in the compiled staff roll.</summary>
    public int RowCount => CreditsPresentationDefinitions.ExpectedCompiledRows;

    /// <summary>Compiles one staff-roll tilemap row, including authored blank-row cadence.</summary>
    /// <param name="index">Zero-based compiled row index.</param>
    /// <returns>Thirty-two tiles.</returns>
    public ReadOnlySpan<BackgroundTile> GetRow(int index)
    {
        if ((uint)index >= RowCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        int row = 0;
        CreditsLineDocument[] authored = lines;
        for (int line = 0; line < authored.Length; line++)
        {
            var definition = CreditsRoles.Lines[line];
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
        Ensure.NotNull(json);
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
                source, ArtworkJson.Options)
                ?? throw new InvalidDataException("Credits document is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid credits JSON.", error);
        }

        IReadOnlyList<CreditsLineDefinition> definitions =
            CreditsRoles.Lines;
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
            if ((uint)line.Palette >= BackgroundTile.ColourVariants)
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
        return new(document.Lines, Convert.ToHexString(SHA256.HashData(source)));
    }

    /// <summary>Validates and writes a credits document as JSON.</summary>
    /// <param name="output">Destination stream.</param>
    /// <param name="document">Document to validate and serialize.</param>
    public static void Write(Stream output, CreditsPresentationDocument document)
    {
        Ensure.NotNull(output);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(
            document, ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        output.Write(bytes);
    }

    /// <summary>Rejects characters unsupported by the line's small or two-row large font.</summary>
    private static void ValidateLine(CreditsLineDefinition definition, CreditsLineDocument line)
    {
        foreach (char character in line.Text)
        {
            try
            {
                _ = CreditsGlyphs.CompileGlyph(character, definition.Style);
                if (definition.Style == CreditsLineStyle.Large)
                    _ = CreditsGlyphs.CompileGlyph(character, definition.Style, bottom: true);
            }
            catch (ArgumentOutOfRangeException error)
            {
                throw new InvalidDataException(
                    $"Credits line '{definition.Id}' contains an unsupported glyph.", error);
            }
        }
    }

    /// <summary>Places a line's glyphs in its colour variant into one fixed-width row.</summary>
    private static BackgroundTile[] RenderLine(CreditsLineDefinition definition, CreditsLineDocument line, bool bottom)
    {
        BackgroundTile[] row = BlankRow();
        for (int index = 0; index < line.Text.Length; index++)
            row[line.Column + index] = InVariant(
                CreditsGlyphs.CompileGlyph(line.Text[index], definition.Style, bottom), line.Palette);
        return row;
    }
    /// <summary>Shows a visible glyph in <paramref name="colourVariant"/> while leaving blank tiles untouched.</summary>
    private static BackgroundTile InVariant(BackgroundTile glyph, int colourVariant) =>
        glyph == CreditsPresentationDefinitions.BlankTile
            ? glyph
            : glyph with { ColourVariant = colourVariant };

    /// <summary>Allocates a 32-tile row filled with the credits' compiled blank tile.</summary>
    private static BackgroundTile[] BlankRow() =>
        Enumerable.Repeat(CreditsPresentationDefinitions.BlankTile,
            CreditsPresentationDefinitions.TilemapWidth).ToArray();
}
