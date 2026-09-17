using System.Buffers.Binary;
using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable reserve labels, digits and arrow appearance; energy and mode behavior stay compiled.</summary>
public sealed class PauseReserveUiPresentation
{
    private readonly Dictionary<string, (int Offset, byte[] Cells)> labels;
    private readonly int digitOffset;
    private readonly byte[][] digits;
    private readonly int[] arrowOffsets;
    private readonly int enabledPalette, disabledPalette;
    private readonly ushort solidColor6, solidColor11;
    private readonly (ushort Color6, ushort Color11)[] arrowFrames;

    private PauseReserveUiPresentation(Dictionary<string, (int, byte[])> labels, int digitOffset,
        byte[][] digits, int[] arrowOffsets, int enabledPalette, int disabledPalette,
        ushort solidColor6, ushort solidColor11, (ushort, ushort)[] arrowFrames)
    {
        this.labels = labels; this.digitOffset = digitOffset; this.digits = digits;
        this.arrowOffsets = arrowOffsets; this.enabledPalette = enabledPalette;
        this.disabledPalette = disabledPalette; this.solidColor6 = solidColor6;
        this.solidColor11 = solidColor11; this.arrowFrames = arrowFrames;
    }

    public void ApplyLabel(Span<byte> tilemap, string name, bool preserveAttributes = false)
    {
        if (!labels.TryGetValue(name, out var label)) throw new InvalidDataException($"Unknown reserve label {name}.");
        if (label.Offset < 0 || label.Offset + label.Cells.Length > tilemap.Length)
            throw new ArgumentException("Reserve label target is shorter than the authored tilemap range.", nameof(tilemap));
        for (int index = 0; index < label.Cells.Length; index += sizeof(ushort))
        {
            ushort word = BinaryPrimitives.ReadUInt16LittleEndian(label.Cells.AsSpan(index));
            if (preserveAttributes)
                word = (ushort)((BinaryPrimitives.ReadUInt16LittleEndian(tilemap.Slice(label.Offset + index)) &
                    PauseReserveUiDefinitions.TileAttributeMask) | (word & ~PauseReserveUiDefinitions.TileAttributeMask));
            BinaryPrimitives.WriteUInt16LittleEndian(tilemap.Slice(label.Offset + index), word);
        }
    }

    public void ApplyDigit(Span<byte> tilemap, int position, int value)
    {
        if ((uint)position >= PauseReserveUiDefinitions.SupplyDigitPlaces || (uint)value >= digits.Length)
            throw new ArgumentOutOfRangeException();
        digits[value].CopyTo(tilemap.Slice(digitOffset + position * sizeof(ushort), sizeof(ushort)));
    }

    public void ApplyArrowTilePalettes(Span<byte> tilemap, bool enabled)
    {
        int palette = enabled ? enabledPalette : disabledPalette;
        foreach (int offset in arrowOffsets)
        {
            var word = new SnesBgTilemapWord(BinaryPrimitives.ReadUInt16LittleEndian(tilemap.Slice(offset)));
            BinaryPrimitives.WriteUInt16LittleEndian(tilemap.Slice(offset), word.WithPaletteIndex(palette).Raw);
        }
    }

    public void ApplyArrowColors(SnesCgram cgram, bool animated, int frame, int color6Index, int color11Index)
    {
        var colors = animated ? arrowFrames[frame & (arrowFrames.Length - 1)] : (solidColor6, solidColor11);
        cgram.SetColor(color6Index, colors.Item1); cgram.SetColor(color11Index, colors.Item2);
    }

    public static PauseReserveUiPresentation Load(Stream json)
    {
        PauseReserveUiDocument document;
        try { document = JsonSerializer.Deserialize<PauseReserveUiDocument>(json, MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Pause reserve UI document is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid pause reserve UI JSON.", error); }
        if (document.Version != PauseReserveUiDefinitions.Version || document.Labels is null || document.Labels.Count != 4 ||
            document.Digits is null || document.Digits.Cells is null || document.Digits.Cells.Length != PauseReserveUiDefinitions.DigitCount ||
            document.Arrow is null || document.Arrow.Cells is null || document.Arrow.Cells.Length != 10 ||
            document.Arrow.Frames is null || document.Arrow.Frames.Length != PauseReserveUiDefinitions.ArrowFrames ||
            (uint)document.Arrow.EnabledPalette > 7 || (uint)document.Arrow.DisabledPalette > 7)
            throw new InvalidDataException("Pause reserve UI requires four labels, ten digits, ten arrow cells, and 32 arrow frames.");
        var labels = new Dictionary<string, (int, byte[])>();
        foreach (var expected in new[] { "Mode", "ReserveTank", "Manual", "Auto" })
        {
            if (!document.Labels.TryGetValue(expected, out var label) || label is null || label.Cells is null ||
                label.Cells.Length != (expected is "Manual" or "Auto" ? PauseReserveUiDefinitions.ModeWords : PauseReserveUiDefinitions.LabelWords))
                throw new InvalidDataException($"Pause reserve UI label {expected} has the wrong shape.");
            labels.Add(expected, (Offset(label.Anchor, expected), PauseTileGrid.Compile(label.Cells, $"Reserve.{expected}")));
        }
        var digits = document.Digits.Cells.Select((cell, index) => PauseTileGrid.Compile([cell], $"Reserve.Digit{index}")).ToArray();
        int[] arrowOffsets = document.Arrow.Cells.Select((point, index) => Offset(point, $"Arrow.{index}")).ToArray();
        if (arrowOffsets.Distinct().Count() != arrowOffsets.Length) throw new InvalidDataException("Reserve arrow cells must be unique.");
        var frames = document.Arrow.Frames.Select((frame, index) =>
            (Color(frame?.Color6, $"arrow frame {index} color 6"), Color(frame?.Color11, $"arrow frame {index} color 11"))).ToArray();
        return new(labels, Offset(document.Digits.Anchor, "Digits"), digits, arrowOffsets,
            document.Arrow.EnabledPalette, document.Arrow.DisabledPalette,
            Color(document.Arrow.SolidColor6, "solid color 6"), Color(document.Arrow.SolidColor11, "solid color 11"), frames);

        static int Offset(PauseGridPoint? point, string name)
        {
            if (point is null || (uint)point.Column >= PauseReserveUiDefinitions.TilemapColumns ||
                (uint)point.Row >= PauseReserveUiDefinitions.TilemapRows)
                throw new InvalidDataException($"Pause reserve UI {name} anchor is outside the 32x32 tilemap.");
            return (point.Row * PauseReserveUiDefinitions.TilemapColumns + point.Column) * sizeof(ushort);
        }
        static ushort Color(PaletteRgb5? color, string name)
        {
            if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 || (uint)color.Blue > 31)
                throw new InvalidDataException($"Pause reserve UI {name} requires RGB components from zero through 31.");
            return (ushort)(color.Red | color.Green << 5 | color.Blue << 10);
        }
    }

    public static void Write(Stream output, PauseReserveUiDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false)); output.Write(bytes);
    }
}

public sealed record PauseReserveUiDocument
{
    public required int Version { get; init; }
    public required Dictionary<string, PauseReserveLabelVisual> Labels { get; init; }
    public required PauseReserveDigitVisual Digits { get; init; }
    public required PauseReserveArrowVisual Arrow { get; init; }
}
public sealed record PauseReserveLabelVisual { public required PauseGridPoint Anchor { get; init; } public required PauseBackdropCell[] Cells { get; init; } }
public sealed record PauseReserveDigitVisual { public required PauseGridPoint Anchor { get; init; } public required PauseBackdropCell[] Cells { get; init; } }
public sealed record PauseReserveArrowVisual
{
    public required PauseGridPoint[] Cells { get; init; }
    public required int EnabledPalette { get; init; }
    public required int DisabledPalette { get; init; }
    public required PaletteRgb5 SolidColor6 { get; init; }
    public required PaletteRgb5 SolidColor11 { get; init; }
    public required PauseReserveArrowFrame[] Frames { get; init; }
}
public sealed record PauseReserveArrowFrame { public required PaletteRgb5 Color6 { get; init; } public required PaletteRgb5 Color11 { get; init; } }
public sealed record PauseGridPoint { public required int Column { get; init; } public required int Row { get; init; } }
