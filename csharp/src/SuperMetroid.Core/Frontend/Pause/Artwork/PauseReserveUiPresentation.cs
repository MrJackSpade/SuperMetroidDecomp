using System.Buffers.Binary;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>Editable reserve labels, digits and arrow appearance; energy and mode behavior stay compiled.</summary>
public sealed class PauseReserveUiPresentation
{
    /// <summary>Mode, Reserve Tank, Manual, and Auto strips retaining only departures from stock artwork.</summary>
    private readonly Dictionary<PauseReserveLabel, ReserveLabel> labels;
    /// <summary>Byte position of the leftmost reserve-supply digit in the equipment tilemap.</summary>
    private readonly int digitOffset;
    /// <summary>Optional authored digit words; null reuses the consecutive stock glyph sequence.</summary>
    internal readonly byte[][]? digits;
    /// <summary>Optional authored arrow-cell positions; null uses the native stem and arm layout.</summary>
    internal readonly int[]? arrowOffsets;
    /// <summary>BG palette selections used for active and inactive arrow tile cells.</summary>
    private readonly int enabledPalette, disabledPalette;
    /// <summary>Two bevel colors shown when AUTO pulse animation is inactive.</summary>
    private readonly Bgr555 solidColor6, solidColor11;
    /// <summary>Only pulse starting colors that differ from the reversed solid bevel colors.</summary>
    private readonly ReserveArrowColorEdits arrowStartEdits;
    /// <summary>Grey midpoint levels from which each mirrored pulse ramp is calculated.</summary>
    private readonly ReserveArrowGreyLevels arrowGreyLevels;
    /// <summary>Independently edited pulse colors that depart from the mirrored ramp.</summary>
    internal readonly Dictionary<ColourCell, Bgr555> arrowColorEdits = [];

    /// <summary>Compacts stock-equivalent digit/arrow positions and derives sparse label and pulse-color edits.</summary>
    private PauseReserveUiPresentation(PauseReserveUiParts parts)
    {
        this.labels = parts.Labels; this.digitOffset = parts.DigitOffset;
        bool stockDigits = true;
        for (int digit = 0; digit < parts.Digits.Length; digit++)
            stockDigits &= BinaryPrimitives.ReadUInt16LittleEndian(parts.Digits[digit]) == DigitWord(digit);
        this.digits = stockDigits ? null : parts.Digits;
        bool stockArrow = true;
        for (int cell = 0; cell < parts.ArrowOffsets.Length; cell++)
            stockArrow &= parts.ArrowOffsets[cell] == ArrowOffset(cell);
        this.arrowOffsets = stockArrow ? null : parts.ArrowOffsets;
        this.enabledPalette = parts.EnabledPalette;
        this.disabledPalette = parts.DisabledPalette; this.solidColor6 = parts.SolidColor6;
        this.solidColor11 = parts.SolidColor11;
        arrowStartEdits = new ReserveArrowColorEdits(parts.ArrowFrames[0].Color6 == parts.SolidColor11 ? null : parts.ArrowFrames[0].Color6,
            parts.ArrowFrames[0].Color11 == parts.SolidColor6 ? null : parts.ArrowFrames[0].Color11);
        var middle = parts.ArrowFrames[PauseReserveUiDefinitions.ArrowFrames / 2 - 1];
        arrowGreyLevels = new ReserveArrowGreyLevels(middle.Color6.Red, middle.Color11.Red);
        for (int frame = 0; frame < parts.ArrowFrames.Length; frame++)
        {
            if (parts.ArrowFrames[frame].Color6 != CalculateArrowColor(frame, ReserveArrowColorSlot.Color6))
                arrowColorEdits.Add(new ColourCell(frame, 0), parts.ArrowFrames[frame].Color6);
            if (parts.ArrowFrames[frame].Color11 != CalculateArrowColor(frame, ReserveArrowColorSlot.Color11))
                arrowColorEdits.Add(new ColourCell(frame, 1), parts.ArrowFrames[frame].Color11);
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
    /// <summary>Writes one selected label's native BG words into its authored equipment-page location.</summary>
    /// <param name="tilemap">Mutable little-endian equipment tilemap bytes, large enough for the entire authored label range.</param>
    /// <param name="name">The label to write.</param>
    /// <param name="attributes">When true, preserves each destination word's palette, priority, and flip bits while replacing only its ten-bit character selector; used for MANUAL/AUTO replacement.</param>
    /// <exception cref="InvalidDataException"><paramref name="name"/> is not a reserve label.</exception>
    /// <exception cref="ArgumentException">The destination span does not contain the authored label range.</exception>
    public void ApplyLabel(Span<byte> tilemap, PauseReserveLabel name, LabelAttributes attributes = LabelAttributes.Replace)
    {
        if (!labels.TryGetValue(name, out var label)) throw new InvalidDataException($"Unknown reserve label {name}.");
        if (label.Offset < 0 || label.Offset + label.WordCount * sizeof(ushort) > tilemap.Length)
            throw new ArgumentException("Reserve label target is shorter than the authored tilemap range.", nameof(tilemap));
        for (int index = 0; index < label.WordCount * sizeof(ushort); index += sizeof(ushort))
        {
            SnesBgTilemapWord word = label.Word(index / sizeof(ushort));
            if (attributes == LabelAttributes.Preserve)
                word = new SnesBgTilemapWord(BinaryPrimitives.ReadUInt16LittleEndian(tilemap.Slice(label.Offset + index)))
                    .WithCharacterIndex(word.CharacterIndex);
            BinaryPrimitives.WriteUInt16LittleEndian(tilemap.Slice(label.Offset + index), word);
        }
    }

    /// <summary>Writes one selected decimal glyph into the reserve-supply display without calculating the energy value.</summary>
    /// <param name="tilemap">Mutable equipment-page bytes containing the authored three-word digit range.</param>
    /// <param name="position">Zero-based left-to-right digit place, zero through two; callers supply hundreds, tens, and units in that order.</param>
    /// <param name="value">Decimal glyph identity, zero through nine.</param>
    /// <exception cref="ArgumentOutOfRangeException">The digit position/value is invalid or the destination lacks the selected two-byte word.</exception>
    public void ApplyDigit(Span<byte> tilemap, int position, int value)
    {
        if ((uint)position >= PauseReserveUiDefinitions.SupplyDigitPlaces || (uint)value >= PauseReserveUiDefinitions.DigitCount)
            throw new ArgumentOutOfRangeException();
        BinaryPrimitives.WriteUInt16LittleEndian(tilemap.Slice(digitOffset + position * sizeof(ushort)),
            digits is null ? DigitWord(value) : BinaryPrimitives.ReadUInt16LittleEndian(digits[value]));
    }

    /// <summary>Replaces only the palette selectors of the ten authored arrow cells, preserving their characters, priority, and flips.</summary>
    /// <param name="tilemap">Mutable little-endian equipment-page bytes containing every authored arrow cell.</param>
    /// <param name="enabled">Selects the authored enabled palette when true or disabled palette when false; does not change reserve mode or transfer energy.</param>
    /// <exception cref="ArgumentOutOfRangeException">The destination lacks an authored arrow word.</exception>
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

    /// <summary>Writes the two selected arrow bevel colours to the screen's colours, using either the pulse frame or the solid appearance.</summary>
    /// <param name="arrowRow">The background row whose two arrow colours change.</param>
    /// <param name="animated">Whether to sample the 32-phase pulse rather than the solid colors; the pause owner decides when AUTO selection should animate.</param>
    /// <param name="frame">Pulse phase, masked to its low five bits; normally the accepted eight-bit frame counter and ignored for solid colors.</param>
    public void ApplyArrowColors(ColourRun arrowRow, bool animated, int frame)
    {
        var colors = animated ? new ReserveArrowColors(ArrowColor(frame, ReserveArrowColorSlot.Color6), ArrowColor(frame, ReserveArrowColorSlot.Color11)) : new ReserveArrowColors(solidColor6, solidColor11);
        arrowRow.Write(PauseReserveUiDefinitions.ArrowFirstColour, colors.Color6);
        arrowRow.Write(PauseReserveUiDefinitions.ArrowSecondColour, colors.Color11);
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
    private Bgr555 CalculateArrowColor(int frame, ReserveArrowColorSlot slot)
    {
        int last = PauseReserveUiDefinitions.ArrowFrames - 1;
        int phase = Math.Min(frame, last - frame);
        int steps = last / 2;
        bool second = slot == ReserveArrowColorSlot.Color11;
        Bgr555 start = second ? arrowStartEdits.Color11 ?? solidColor6 : arrowStartEdits.Color6 ?? solidColor11;
        int grey = second ? arrowGreyLevels.Color11 : arrowGreyLevels.Color6;
        if (phase == steps) return new(grey, grey, grey);
        ColorChannel held = second ? ColorChannel.Blue : ColorChannel.Red;
        return start.Map((channel, channelStart) =>
        {
            int channelPhase = phase == steps - 1 && channel == held ? phase - 1 : phase;
            float remaining = 1;
            float step = (float)(1d / steps);
            for (int tick = 0; tick < channelPhase; tick++)
                remaining = (float)((double)remaining - step);
            float amount = (float)(1d - remaining);
            // Explicit binary32 roundings separate multiplication and addition: an FMA
            // cannot remove the intermediate rounding or alter the integral crossings.
            float delta = (float)((double)(grey - channelStart) * amount);
            return (int)(float)(channelStart + (double)delta);
        });
    }

    /// <summary>Uses the supplied independent bevel shade when present; otherwise mirrors the calculated 32-phase ramp.</summary>
    private Bgr555 ArrowColor(int frame, ReserveArrowColorSlot slot)
    {
        frame &= PauseReserveUiDefinitions.ArrowFrames - 1;
        return arrowColorEdits.TryGetValue(new ColourCell(frame, (slot == ReserveArrowColorSlot.Color11 ? 1 : 0)), out Bgr555 supplied)
            ? supplied : CalculateArrowColor(frame, slot);
    }

    /// <summary>Compiles the selected reserve labels, digit words, arrow placements and arrow colours into a presentation.</summary>
    /// <param name="selection">The selected labels, digits, arrow placements and arrow colours.</param>
    internal static PauseReserveUiPresentation Create(PauseReserveUiSelection selection)
    {
        Ensure.NotNull(selection);
        var labels = new Dictionary<PauseReserveLabel, ReserveLabel>();
        foreach (KeyValuePair<PauseReserveLabel, int> label in selection.LabelOffsets)
            labels.Add(label.Key, ReserveLabel.Load(label.Key, label.Value, selection.LabelWords[label.Key]));
        return new(new PauseReserveUiParts { Labels = labels, DigitOffset = selection.DigitOffset, Digits = selection.Digits, ArrowOffsets = selection.ArrowOffsets, EnabledPalette = selection.EnabledPalette, DisabledPalette = selection.DisabledPalette, SolidColor6 = selection.SolidColor6, SolidColor11 = selection.SolidColor11, ArrowFrames = selection.ArrowFrames });
    }

    /// <summary>Resolves unchanged reserve-label cells from stock and stores only edited placement or tile words.</summary>
    private sealed class ReserveLabel(PauseReserveLabel name, int? offsetEdit, Dictionary<int, ushort> edits)
    {
        /// <summary>Uses an authored placement when changed, otherwise the native label anchor.</summary>
        internal int Offset => offsetEdit ?? PauseReserveUiDefinitions.StockLabelOffset(name);
        /// <summary>Uses the native strip width for the selected label.</summary>
        internal int WordCount => PauseReserveUiDefinitions.StockLabelWords(name);
        /// <summary>Returns an edited tile word or the corresponding stock glyph and attributes.</summary>
        internal ushort Word(int index) => edits.ContainsKey(index)
            ? edits[index] : PauseReserveUiDefinitions.StockLabelWord(name, index);

        /// <summary>Compares every supplied little-endian word and destination with stock, retaining only differences.</summary>
        internal static ReserveLabel Load(PauseReserveLabel name, int offset, ReadOnlySpan<byte> bytes)
        {
            var edits = new Dictionary<int, ushort>();
            for (int index = 0; index < bytes.Length / sizeof(ushort); index++)
            {
                int value = BinaryPrimitives.ReadUInt16LittleEndian(bytes[(index * sizeof(ushort))..]);
                if (value != PauseReserveUiDefinitions.StockLabelWord(name, index)) edits.Add(index, (ushort)value);
            }
            int? editedOffset = offset == PauseReserveUiDefinitions.StockLabelOffset(name) ? null : offset;
            return new(name, editedOffset, edits);
        }
    }

    /// <summary>The pause screen's reserve-tank labels, digits and arrow colours (#633).</summary>
    private sealed record PauseReserveUiParts
    {
        /// <summary>The four compiled strips with stock-equivalent cells omitted.</summary>
        public required Dictionary<PauseReserveLabel, ReserveLabel> Labels { get; init; }

        /// <summary>Byte anchor of the three-place reserve supply in the equipment tilemap.</summary>
        public required int DigitOffset { get; init; }

        /// <summary>Ten compiled decimal glyph words before stock-equivalent compression.</summary>
        public required byte[][] Digits { get; init; }

        /// <summary>Ten distinct arrow-cell byte positions, vertical stem before horizontal arm.</summary>
        public required int[] ArrowOffsets { get; init; }

        /// <summary>BG palette selector used when the pause owner enables reserve control.</summary>
        public required int EnabledPalette { get; init; }

        /// <summary>BG palette selector used when reserve control is unavailable.</summary>
        public required int DisabledPalette { get; init; }

        /// <summary>Static arrow bevel color for palette slot six outside pulse animation.</summary>
        public required Bgr555 SolidColor6 { get; init; }

        /// <summary>Static arrow bevel color for palette slot eleven outside pulse animation.</summary>
        public required Bgr555 SolidColor11 { get; init; }

        /// <summary>Two RGB5 bevel colors for each of the 32 mirrored pulse phases.</summary>
        public required ReserveArrowColors[] ArrowFrames { get; init; }
    }
}
