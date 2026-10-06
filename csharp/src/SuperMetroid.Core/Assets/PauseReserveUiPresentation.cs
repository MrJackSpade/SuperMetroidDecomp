using System.Buffers.Binary;
using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable reserve labels, digits and arrow appearance; energy and mode behavior stay compiled.</summary>
public sealed class PauseReserveUiPresentation
{
    private readonly Dictionary<string, ReserveLabel> labels;
    private readonly int digitOffset;
    private readonly byte[][]? digits;
    private readonly int[]? arrowOffsets;
    private readonly int enabledPalette, disabledPalette;
    private readonly ushort solidColor6, solidColor11;
    private readonly (ushort? Color6, ushort? Color11) arrowStartEdits;
    private readonly (int Color6, int Color11) arrowGreyLevels;
    private readonly Dictionary<int, ushort> arrowColorEdits = [];

    private PauseReserveUiPresentation(Dictionary<string, ReserveLabel> labels, int digitOffset,
        byte[][] digits, int[] arrowOffsets, int enabledPalette, int disabledPalette,
        ushort solidColor6, ushort solidColor11, (ushort, ushort)[] arrowFrames)
    {
        this.labels = labels; this.digitOffset = digitOffset;
        bool stockDigits = true;
        for (int digit = 0; digit < digits.Length; digit++)
            stockDigits &= BinaryPrimitives.ReadUInt16LittleEndian(digits[digit]) == DigitWord(digit);
        this.digits = stockDigits ? null : digits;
        bool stockArrow = true;
        for (int cell = 0; cell < arrowOffsets.Length; cell++)
            stockArrow &= arrowOffsets[cell] == ArrowOffset(cell);
        this.arrowOffsets = stockArrow ? null : arrowOffsets;
        this.enabledPalette = enabledPalette;
        this.disabledPalette = disabledPalette; this.solidColor6 = solidColor6;
        this.solidColor11 = solidColor11;
        arrowStartEdits = (arrowFrames[0].Item1 == solidColor11 ? null : arrowFrames[0].Item1,
            arrowFrames[0].Item2 == solidColor6 ? null : arrowFrames[0].Item2);
        var middle = arrowFrames[PauseReserveUiDefinitions.ArrowFrames / 2 - 1];
        arrowGreyLevels = (middle.Item1 & 31, middle.Item2 & 31);
        for (int frame = 0; frame < arrowFrames.Length; frame++)
        {
            if (arrowFrames[frame].Item1 != CalculateArrowColor(frame, false))
                arrowColorEdits.Add(frame * 2, arrowFrames[frame].Item1);
            if (arrowFrames[frame].Item2 != CalculateArrowColor(frame, true))
                arrowColorEdits.Add(frame * 2 + 1, arrowFrames[frame].Item2);
        }
    }

    /// <summary>$82:8F70 selects the ten consecutive digit glyphs starting with native word $0804.</summary>
    private static ushort DigitWord(int digit) => (ushort)(PauseReserveUiDefinitions.DigitZeroWord + digit);

    /// <summary>
    /// Native reserve-arrow cells form an eight-cell vertical stem followed by a two-cell
    /// horizontal arm; each tilemap cell occupies two bytes in the32-column equipment page.
    /// </summary>
    private static int ArrowOffset(int cell) => sizeof(ushort) * (cell < PauseReserveUiDefinitions.VerticalCount
        ? PauseReserveUiDefinitions.VerticalStartCell + cell * PauseReserveUiDefinitions.RowStrideCells
        : PauseReserveUiDefinitions.HorizontalStartCell + cell - PauseReserveUiDefinitions.VerticalCount);
    public void ApplyLabel(Span<byte> tilemap, string name, bool preserveAttributes = false)
    {
        if (!labels.TryGetValue(name, out var label)) throw new InvalidDataException($"Unknown reserve label {name}.");
        if (label.Offset < 0 || label.Offset + label.WordCount * sizeof(ushort) > tilemap.Length)
            throw new ArgumentException("Reserve label target is shorter than the authored tilemap range.", nameof(tilemap));
        for (int index = 0; index < label.WordCount * sizeof(ushort); index += sizeof(ushort))
        {
            ushort word = label.Word(index / sizeof(ushort));
            if (preserveAttributes)
                word = (ushort)((BinaryPrimitives.ReadUInt16LittleEndian(tilemap.Slice(label.Offset + index)) &
                    PauseReserveUiDefinitions.TileAttributeMask) | (word & ~PauseReserveUiDefinitions.TileAttributeMask));
            BinaryPrimitives.WriteUInt16LittleEndian(tilemap.Slice(label.Offset + index), word);
        }
    }

    public void ApplyDigit(Span<byte> tilemap, int position, int value)
    {
        if ((uint)position >= PauseReserveUiDefinitions.SupplyDigitPlaces || (uint)value >= PauseReserveUiDefinitions.DigitCount)
            throw new ArgumentOutOfRangeException();
        ushort word = digits is null ? DigitWord(value) : BinaryPrimitives.ReadUInt16LittleEndian(digits[value]);
        BinaryPrimitives.WriteUInt16LittleEndian(tilemap.Slice(digitOffset + position * sizeof(ushort)), word);
    }

    public void ApplyArrowTilePalettes(Span<byte> tilemap, bool enabled)
    {
        int palette = enabled ? enabledPalette : disabledPalette;
        int cells = PauseReserveUiDefinitions.VerticalCount + PauseReserveUiDefinitions.HorizontalCount;
        for (int cell = 0; cell < cells; cell++)
        {
            int offset = arrowOffsets is null ? ArrowOffset(cell) : arrowOffsets[cell];
            var word = new SnesBgTilemapWord(BinaryPrimitives.ReadUInt16LittleEndian(tilemap.Slice(offset)));
            BinaryPrimitives.WriteUInt16LittleEndian(tilemap.Slice(offset), word.WithPaletteIndex(palette).Raw);
        }
    }

    public void ApplyArrowColors(SnesCgram cgram, bool animated, int frame, int color6Index, int color11Index)
    {
        var colors = animated ? (ArrowColor(frame, false), ArrowColor(frame, true)) : (solidColor6, solidColor11);
        cgram.SetColor(color6Index, colors.Item1); cgram.SetColor(color11Index, colors.Item2);
    }

    /// <summary>
    /// $82:AD5D/AD9D mirror two16-phase RGB ramps around the repeated midpoint.
    /// Starts share the reversed solid-arrow colors; grey midpoint channels share one level.
    /// Repeated normalized fade subtraction explains the below-integer blue crossings at5/10.
    /// The penultimate bright-red/dark-blue channels hold their preceding shade. Endpoint
    /// colors/levels and this exact two-channel hold are authored bevel-color/pulse content:
    /// different choices paint a different pulse. Held magnitudes derive from the prior shade;
    /// independently supplied differences stay sparse.
    /// </summary>
    private ushort CalculateArrowColor(int frame, bool second)
    {
        int last = PauseReserveUiDefinitions.ArrowFrames - 1;
        int phase = Math.Min(frame, last - frame);
        int steps = last / 2;
        ushort start = second ? arrowStartEdits.Color11 ?? solidColor6 : arrowStartEdits.Color6 ?? solidColor11;
        int grey = second ? arrowGreyLevels.Color11 : arrowGreyLevels.Color6;
        if (phase == steps) return (ushort)(grey | grey << 5 | grey << 10);
        int value = 0;
        for (int shift = 0; shift < 15; shift += 5)
        {
            int channelPhase = phase == steps - 1 && shift == (second ? 10 : 0) ? phase - 1 : phase;
            float remaining = 1;
            float step = (float)(1d / steps);
            for (int tick = 0; tick < channelPhase; tick++)
                remaining = (float)((double)remaining - step);
            float amount = (float)(1d - remaining);
            int channelStart = start >> shift & 31;
            // Explicit binary32 roundings separate multiplication and addition: an FMA
            // cannot remove the intermediate rounding or alter the integral crossings.
            float delta = (float)((double)(grey - channelStart) * amount);
            int channel = (int)(float)(channelStart + (double)delta);
            value |= channel << shift;
        }
        return (ushort)value;
    }

    private ushort ArrowColor(int frame, bool second)
    {
        frame &= PauseReserveUiDefinitions.ArrowFrames - 1;
        return arrowColorEdits.TryGetValue(frame * 2 + (second ? 1 : 0), out ushort supplied)
            ? supplied : CalculateArrowColor(frame, second);
    }
    public static PauseReserveUiPresentation Load(Stream json)
    {
        PauseReserveUiDocument document;
        try { document = JsonAssetDocument.Read<PauseReserveUiDocument>(json, MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Pause reserve UI document is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid pause reserve UI JSON.", error); }
        if (document.Version != PauseReserveUiDefinitions.Version || document.Labels is null || document.Labels.Count != 4 ||
            document.Digits is null || document.Digits.Cells is null || document.Digits.Cells.Length != PauseReserveUiDefinitions.DigitCount ||
            document.Arrow is null || document.Arrow.Cells is null || document.Arrow.Cells.Length != 10 ||
            document.Arrow.Frames is null || document.Arrow.Frames.Length != PauseReserveUiDefinitions.ArrowFrames ||
            (uint)document.Arrow.EnabledPalette > 7 || (uint)document.Arrow.DisabledPalette > 7)
            throw new InvalidDataException("Pause reserve UI requires four labels, ten digits, ten arrow cells, and 32 arrow frames.");
        var labels = new Dictionary<string, ReserveLabel>();
        foreach (var expected in new[] { "Mode", "ReserveTank", "Manual", "Auto" })
        {
            if (!document.Labels.TryGetValue(expected, out var label) || label is null || label.Cells is null ||
                label.Cells.Length != (expected is "Manual" or "Auto" ? PauseReserveUiDefinitions.ModeWords : PauseReserveUiDefinitions.LabelWords))
                throw new InvalidDataException($"Pause reserve UI label {expected} has the wrong shape.");
            labels.Add(expected, ReserveLabel.Load(expected, Offset(label.Anchor, expected),
                PauseTileGrid.Compile(label.Cells, $"Reserve.{expected}")));
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

    private sealed class ReserveLabel(string name, int? offsetEdit, Dictionary<int, ushort> edits)
    {
        internal int Offset => offsetEdit ?? PauseReserveUiDefinitions.StockLabelOffset(name);
        internal int WordCount => PauseReserveUiDefinitions.StockLabelWords(name);
        internal ushort Word(int index) => edits.TryGetValue(index, out ushort selected)
            ? selected : PauseReserveUiDefinitions.StockLabelWord(name, index);

        internal static ReserveLabel Load(string name, int offset, ReadOnlySpan<byte> bytes)
        {
            var edits = new Dictionary<int, ushort>();
            for (int index = 0; index < bytes.Length / sizeof(ushort); index++)
            {
                ushort value = BinaryPrimitives.ReadUInt16LittleEndian(bytes[(index * sizeof(ushort))..]);
                if (value != PauseReserveUiDefinitions.StockLabelWord(name, index)) edits.Add(index, value);
            }
            int? editedOffset = offset == PauseReserveUiDefinitions.StockLabelOffset(name) ? null : offset;
            return new(name, editedOffset, edits);
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
