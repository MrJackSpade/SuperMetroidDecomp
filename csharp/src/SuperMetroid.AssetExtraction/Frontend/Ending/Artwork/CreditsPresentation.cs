using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>Editable staff-credit text compiled into the cartridge's fixed row cadence.</summary>
public sealed partial class CreditsPresentation
{
    /// <summary>Each roll line with its validated editable text, in roll order, rendered on demand at the fixed cartridge cadence.</summary>
    private readonly RollLineText[] lines;

    /// <summary>Retains the lines and the identity of their original JSON bytes.</summary>
    private CreditsPresentation(RollLineText[] lines, string contentIdentity)
    {
        this.lines = lines;
        ContentIdentity = contentIdentity;
    }

    /// <summary>Gets the uppercase SHA-256 identity of the exact loaded JSON bytes.</summary>
    public string ContentIdentity { get; }


    /// <summary>Compiles one staff-roll tilemap row, including authored blank-row cadence.</summary>
    /// <param name="index">Zero-based compiled row index.</param>
    /// <returns>Thirty-two tiles.</returns>
    public ReadOnlySpan<BackgroundTile> GetRow(int index)
    {
        if ((uint)index >= CreditsPresentationDefinitions.RowCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        foreach (RollLineText line in lines)
        {
            int row = RowsOf(line.Placed.Top);
            if (index < row)
                return BlankRow();
            if (index < row + RowsOf(line.Placed.Height))
                return RenderLine(line.Placed.Role, line.Text, bottom: index != row);
        }
        return BlankRow();
    }

    /// <summary>The rows of the cartridge's credits row stream <paramref name="pixels"/> of the roll cover.</summary>
    /// <exception cref="InvalidDataException">The distance is not whole rows.</exception>
    internal static int RowsOf(int pixels) => pixels % CreditsPresentationDefinitions.RowHeight == 0
        ? pixels / CreditsPresentationDefinitions.RowHeight
        : throw new InvalidDataException($"The credits roll's {pixels}-pixel distance is not whole rows of the cartridge's credits row stream.");
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
                source, ArtworkJson.Options);
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid credits JSON.", error);
        }

        ImmutableArray<CreditsRollLine> roll = CreditsRollLines.Lines;
        if (document.Version != CreditsPresentationDefinitions.Version ||
            document.Lines is null || document.Lines.Length != roll.Length)
        {
            throw new InvalidDataException(
                $"Credits require version {CreditsPresentationDefinitions.Version} and " +
                $"exactly {roll.Length} ordered lines.");
        }
        // Each line's text is the document's line of its role; the document lists them in roll order.
        if (!document.Lines.Select(line => line.Id).SequenceEqual(roll.Select(line => line.Role.Role), StringComparer.Ordinal))
            throw new InvalidDataException("Credits lines must retain each role's id, in roll order.");
        Dictionary<string, CreditsLineDocument> textOf = document.Lines.ToDictionary(line => line.Id, StringComparer.Ordinal);

        int compiledRows = 0;
        var lines = new List<RollLineText>(roll.Length);
        foreach (CreditsRollLine placed in roll)
        {
            CreditsLineDefinition definition = placed.Role;
            string id = definition.Role;
            CreditsLineDocument line = textOf[id];
            if (line.Text is null || line.Text.Length == 0)
                throw new InvalidDataException($"Credits line '{id}' cannot be empty.");
            if ((uint)line.Palette >= BackgroundTile.ColourVariants)
                throw new InvalidDataException($"Credits line '{id}' has invalid palette {line.Palette}.");
            if (line.Column < 0 || line.Column + line.Text.Length >
                CreditsPresentationDefinitions.TilemapWidth)
            {
                throw new InvalidDataException(
                    $"Credits line '{id}' exceeds the 32-column tilemap.");
            }

            compiledRows += RowsOf(definition.SpaceBefore) + RowsOf(CreditsRollLines.HeightOf(definition.Style));
            ValidateLine(definition, id, line);
            lines.Add(new RollLineText(placed, line));
        }
        compiledRows += RowsOf(CreditsRollDefinitions.TrailingSpace);
        if (compiledRows != CreditsPresentationDefinitions.RowCount)
        {
            throw new InvalidDataException(
                $"Credits compiled to {compiledRows} rows; expected " +
                $"{CreditsPresentationDefinitions.RowCount}.");
        }
        return new([.. lines], Convert.ToHexString(SHA256.HashData(source)));
    }

    /// <summary>Writes a credits document as JSON.</summary>
    /// <param name="output">Destination stream.</param>
    /// <param name="document">Document to serialize.</param>
    public static void Write(Stream output, CreditsPresentationDocument document)
    {
        Ensure.NotNull(output);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, global::SuperMetroid.AssetExtraction.AssetExtractionJson.Info<global::SuperMetroid.AssetExtraction.Frontend.Ending.Artwork.CreditsPresentationDocument>(ArtworkJson.Options));
        output.Write(bytes);
    }

    /// <summary>Rejects characters unsupported by the line's small or two-row large font.</summary>
    private static void ValidateLine(CreditsLineDefinition definition, string id, CreditsLineDocument line)
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
                    $"Credits line '{id}' contains an unsupported glyph.", error);
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

    /// <summary>One staff line on the roll with its editable text.</summary>
    /// <param name="Placed">The line's role and where it lies on the roll.</param>
    /// <param name="Text">The line's validated editable text, column and colour.</param>
    private sealed partial record RollLineText(CreditsRollLine Placed, CreditsLineDocument Text);

    /// <summary>Allocates a 32-tile row filled with the credits' compiled blank tile.</summary>
    private static BackgroundTile[] BlankRow() =>
        Enumerable.Repeat(CreditsPresentationDefinitions.BlankTile,
            CreditsPresentationDefinitions.TilemapWidth).ToArray();
}
