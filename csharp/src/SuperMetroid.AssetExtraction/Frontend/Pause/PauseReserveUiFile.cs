using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Pause;

/// <summary>Reads and writes the installed pause reserve-tank interface document.</summary>
public static class PauseReserveUiFile
{
    /// <summary>Compiles editable reserve labels, digit words, arrow placements, and RGB5 pulse colors into a selected presentation.</summary>
    /// <param name="json">Caller-owned readable JSON stream, consumed from its current position without being disposed.</param>
    /// <returns>A presentation retaining compiled visual data; reserve capacity, mode selection, transfer, and animation cadence remain in the pause owner.</returns>
    /// <remarks>Requires four exact label keys, seven-word Mode/ReserveTank labels, four-word Manual/Auto labels, ten digit glyphs, ten distinct arrow locations, and 32 color frames. Anchors must be within the 32-by-32 equipment page; application separately checks the actual target ranges.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">The JSON, schema, resource counts, labels, atlas references, coordinates, palettes, or RGB5 colors are invalid.</exception>
    public static PauseReserveUiPresentation Read(Stream json)
    {
        PauseReserveUiDocument document;
        try { document = JsonAssetDocument.Read<PauseReserveUiDocument>(json, ArtworkJson.Options)
            ?? throw new InvalidDataException("Pause reserve UI document is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid pause reserve UI JSON.", error); }
        if (document.Version != PauseReserveUiDefinitions.Version || document.Labels is null || document.Labels.Count != 4 ||
            document.Digits is null || document.Digits.Cells is null || document.Digits.Cells.Length != PauseReserveUiDefinitions.DigitCount ||
            document.Arrow is null || document.Arrow.Cells is null || document.Arrow.Cells.Length != 10 ||
            document.Arrow.Frames is null || document.Arrow.Frames.Length != PauseReserveArrowDefinitions.ArrowFrames ||
            (uint)document.Arrow.EnabledPalette > 7 || (uint)document.Arrow.DisabledPalette > 7)
            throw new InvalidDataException("Pause reserve UI requires four labels, ten digits, ten arrow cells, and 32 arrow frames.");
        var labelCells = new Dictionary<PauseReserveLabel, int>();
        var labelTiles = new Dictionary<PauseReserveLabel, BackgroundTile[]>();
        foreach (PauseReserveLabel expected in PauseReserveUiDefinitions.Labels)
        {
            string name = PauseReserveUiTiles.Name(expected);
            if (!document.Labels.TryGetValue(name, out var label) || label is null || label.Cells is null ||
                label.Cells.Length != PauseReserveUiTiles.StockLabelWords(expected))
                throw new InvalidDataException($"Pause reserve UI label {name} has the wrong shape.");
            labelCells.Add(expected, Cell(label.Anchor, name));
            labelTiles.Add(expected, PauseTileGrid.Compile(label.Cells, $"Reserve.{name}"));
        }
        BackgroundTile[] digits = PauseTileGrid.Compile(document.Digits.Cells, "Reserve.Digits");
        int[] arrowCells = document.Arrow.Cells.Select((point, index) => Cell(point, $"Arrow.{index}")).ToArray();
        if (arrowCells.Distinct().Count() != arrowCells.Length) throw new InvalidDataException("Reserve arrow cells must be unique.");
        var frames = document.Arrow.Frames.Select((frame, index) =>
            new ReserveArrowColors(Color(frame?.Color6, $"arrow frame {index} color 6"), Color(frame?.Color11, $"arrow frame {index} color 11"))).ToArray();
        int digitCell = Cell(document.Digits.Anchor, "Digits");
        RgbColor solidColor6 = Color(document.Arrow.SolidColor6, "solid color 6");
        RgbColor solidColor11 = Color(document.Arrow.SolidColor11, "solid color 11");
        return PauseReserveUiPresentation.Create(new PauseReserveUiSelection
        {
            LabelCells = labelCells, LabelTiles = labelTiles, DigitCell = digitCell, Digits = digits,
            ArrowCells = arrowCells, EnabledPalette = document.Arrow.EnabledPalette,
            DisabledPalette = document.Arrow.DisabledPalette,
            ArrowColours = new PauseReserveArrowColours(solidColor6, solidColor11, frames),
        });

        // Converts a bounded equipment-page cell coordinate to its row-major cell.
        static int Cell(PauseGridPoint? point, string name)
        {
            if (point is null || (uint)point.Column >= PauseReserveUiDefinitions.TilemapColumns ||
                (uint)point.Row >= PauseReserveUiDefinitions.TilemapRows)
                throw new InvalidDataException($"Pause reserve UI {name} anchor is outside the 32x32 tilemap.");
            return point.Row * PauseReserveUiDefinitions.TilemapColumns + point.Column;
        }
        // Requires nonnull five-bit RGB components before packing an arrow color.
        static RgbColor Color(PaletteRgb5? color, string name)
        {
            if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 || (uint)color.Blue > 31)
                throw new InvalidDataException($"Pause reserve UI {name} requires RGB components from zero through 31.");
            return color.ToRgbColor();
        }
    }

    /// <summary>Serializes and validates the complete reserve UI document before writing its UTF-8 bytes to the destination.</summary>
    /// <param name="output">Caller-owned writable destination stream, written at its current position without being disposed.</param>
    /// <param name="document">Selected editable visuals satisfying the same schema and field limits as <see cref="Read"/>.</param>
    /// <exception cref="InvalidDataException">The document is null or fails presentation validation.</exception>
    public static void Write(Stream output, PauseReserveUiDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        _ = Read(new MemoryStream(bytes, writable: false)); output.Write(bytes);
    }
}
