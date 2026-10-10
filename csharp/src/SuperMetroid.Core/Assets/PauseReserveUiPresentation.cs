using System.Buffers.Binary;
using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable reserve labels, digits and arrow appearance; energy and mode behavior stay compiled.</summary>
public sealed class PauseReserveUiPresentation
{
    /// <summary>Compiled label words keyed by the four supported reserve-screen labels.</summary>
    private readonly Dictionary<string, ReserveLabel> labels;
    /// <summary>Byte offset of the first reserve digit word in the equipment tilemap.</summary>
    private readonly int digitOffset;
    /// <summary>Non-stock digit glyph words, or null when the compiled glyphs match the native defaults.</summary>
    private readonly byte[][]? digits;
    /// <summary>Non-stock byte offsets for arrow cells, or null when native cell placement is unchanged.</summary>
    private readonly int[]? arrowOffsets;
    /// <summary>Palette indices selected for the reserve arrow's enabled and disabled states, respectively.</summary>
    private readonly int enabledPalette, disabledPalette;
    /// <summary>Solid RGB555 colors for bevel slots six and eleven when pulse animation is not used.</summary>
    private readonly ushort solidColor6, solidColor11;
    /// <summary>Optional authored overrides for the two reserve-arrow bevel colors at the first pulse step.</summary>
    private readonly (ushort? Color6, ushort? Color11) arrowStartEdits;
    /// <summary>Endpoint grey levels used to interpolate each arrow bevel channel across the pulse.</summary>
    private readonly (int Color6, int Color11) arrowGreyLevels;
    /// <summary>Source pulse colors that differ from the calculated shared fade.</summary>
    private readonly Dictionary<int, ushort> arrowColorEdits = [];

    /// <summary>Compiles editable reserve UI values while retaining only visual differences from the native defaults.</summary>
    /// <param name="labels">Resolved destination offsets and tile words for each reserve label.</param>
    /// <param name="digitOffset">Byte offset of the first numeric glyph destination.</param>
    /// <param name="digits">Compiled glyph words in decimal order.</param>
    /// <param name="arrowOffsets">Byte offsets of the ten arrow cells in draw order.</param>
    /// <param name="enabledPalette">Palette index used while the reserve arrow is enabled.</param>
    /// <param name="disabledPalette">Palette index used while the reserve arrow is disabled.</param>
    /// <param name="solidColor6">RGB555 color for arrow bevel slot six when the pulse is not animated.</param>
    /// <param name="solidColor11">RGB555 color for arrow bevel slot eleven when the pulse is not animated.</param>
    /// <param name="arrowFrames">Ordered pairs of source colors for the 32-phase arrow pulse.</param>
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
    /// <summary>Writes one selected label's native BG words into its authored equipment-page location.</summary>
    /// <param name="tilemap">Mutable little-endian equipment tilemap bytes, large enough for the entire authored label range.</param>
    /// <param name="name">Case-sensitive label identity: Mode, ReserveTank, Manual, or Auto.</param>
    /// <param name="preserveAttributes">When true, preserves each destination word's palette, priority, and flip bits while replacing only its ten-bit character selector; used for MANUAL/AUTO replacement.</param>
    /// <exception cref="InvalidDataException"><paramref name="name"/> is not an installed label identity.</exception>
    /// <exception cref="ArgumentException">The destination span does not contain the authored label range.</exception>
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

    /// <summary>Writes one selected decimal glyph into the reserve-supply display without calculating the energy value.</summary>
    /// <param name="tilemap">Mutable equipment-page bytes containing the authored three-word digit range.</param>
    /// <param name="position">Zero-based left-to-right digit place, zero through two; callers supply hundreds, tens, and units in that order.</param>
    /// <param name="value">Decimal glyph identity, zero through nine.</param>
    /// <exception cref="ArgumentOutOfRangeException">The digit position/value is invalid or the destination lacks the selected two-byte word.</exception>
    public void ApplyDigit(Span<byte> tilemap, int position, int value)
    {
        if ((uint)position >= PauseReserveUiDefinitions.SupplyDigitPlaces || (uint)value >= PauseReserveUiDefinitions.DigitCount)
            throw new ArgumentOutOfRangeException();
        ushort word = digits is null ? DigitWord(value) : BinaryPrimitives.ReadUInt16LittleEndian(digits[value]);
        BinaryPrimitives.WriteUInt16LittleEndian(tilemap.Slice(digitOffset + position * sizeof(ushort)), word);
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

    /// <summary>Writes the two selected arrow bevel colors to live CGRAM, using either the pulse frame or the solid appearance.</summary>
    /// <param name="cgram">Live palette destination, modified at the two supplied color indices.</param>
    /// <param name="animated">Whether to sample the 32-phase pulse rather than the solid colors; the pause owner decides when AUTO selection should animate.</param>
    /// <param name="frame">Pulse phase, masked to its low five bits; normally the accepted eight-bit NMI frame counter and ignored for solid colors.</param>
    /// <param name="color6Index">CGRAM color index 0-255 for arrow palette slot six, not a byte offset.</param>
    /// <param name="color11Index">CGRAM color index 0-255 for arrow palette slot eleven, not a byte offset.</param>
    /// <exception cref="ArgumentOutOfRangeException">A supplied CGRAM color index is invalid.</exception>
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

    /// <summary>Resolves one arrow bevel color from the source override or the calculated pulse.</summary>
    /// <param name="frame">Pulse phase, reduced to the authored 32-frame cycle.</param>
    /// <param name="second">Selects bevel slot eleven when true or slot six when false.</param>
    /// <returns>RGB555 color for the selected bevel slot and phase.</returns>
    private ushort ArrowColor(int frame, bool second)
    {
        frame &= PauseReserveUiDefinitions.ArrowFrames - 1;
        return arrowColorEdits.TryGetValue(frame * 2 + (second ? 1 : 0), out ushort supplied)
            ? supplied : CalculateArrowColor(frame, second);
    }
    /// <summary>Compiles editable reserve labels, digit words, arrow placements, and RGB5 pulse colors into a selected presentation.</summary>
    /// <param name="json">Caller-owned readable JSON stream, consumed from its current position without being disposed.</param>
    /// <returns>A presentation retaining compiled visual data; reserve capacity, mode selection, transfer, and animation cadence remain in the pause owner.</returns>
    /// <remarks>Requires four exact label keys, seven-word Mode/ReserveTank labels, four-word Manual/Auto labels, ten digit glyphs, ten distinct arrow locations, and 32 color frames. Anchors must be within the 32-by-32 equipment page; application separately checks the actual target ranges.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">The JSON, schema, resource counts, labels, atlas references, coordinates, palettes, or RGB5 colors are invalid.</exception>
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

    /// <summary>Compiled label identity, optional placement edit, and sparse tile-word changes.</summary>
    /// <param name="name">Case-sensitive native label identity used to retrieve stock placement and words.</param>
    /// <param name="offsetEdit">Replacement byte offset, or null when native placement is unchanged.</param>
    /// <param name="edits">Only tile words that differ from the stock label definition.</param>
    private sealed class ReserveLabel(string name, int? offsetEdit, Dictionary<int, ushort> edits)
    {
        /// <summary>Destination byte offset, using the stock label position unless authoring changed it.</summary>
        internal int Offset => offsetEdit ?? PauseReserveUiDefinitions.StockLabelOffset(name);
        /// <summary>Number of tile words occupied by this label.</summary>
        internal int WordCount => PauseReserveUiDefinitions.StockLabelWords(name);

        /// <summary>Gets an edited tile word or falls back to the stock word for this label.</summary>
        /// <param name="index">Zero-based word index within the label.</param>
        /// <returns>The selected little-endian tilemap word.</returns>
        internal ushort Word(int index) => edits.TryGetValue(index, out ushort selected)
            ? selected : PauseReserveUiDefinitions.StockLabelWord(name, index);

        /// <summary>Compiles a label's supplied tile words into sparse differences from the native definition.</summary>
        /// <param name="name">Supported label identity used to compare words and default placement.</param>
        /// <param name="offset">Byte offset where the label starts in the equipment-page tilemap.</param>
        /// <param name="bytes">Little-endian tile words compiled from the editable label cells.</param>
        /// <returns>Label data retaining the supplied position and only non-stock word edits.</returns>
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

    /// <summary>Serializes and validates the complete reserve UI document before writing its UTF-8 bytes to the destination.</summary>
    /// <param name="output">Caller-owned writable destination stream, written at its current position without being disposed.</param>
    /// <param name="document">Selected editable visuals satisfying the same schema and field limits as <see cref="Load"/>.</param>
    /// <exception cref="InvalidDataException">The document is null or fails presentation validation.</exception>
    public static void Write(Stream output, PauseReserveUiDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false)); output.Write(bytes);
    }
}

/// <summary>Complete editable reserve UI schema; nested arrays and dictionaries are mutable authoring data copied into compiled presentation values.</summary>
public sealed record PauseReserveUiDocument
{
    /// <summary>Schema revision; loading requires <see cref="PauseReserveUiDefinitions.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly four case-sensitive entries named Mode, ReserveTank, Manual, and Auto, defining placement and ordered character words.</summary>
    public required Dictionary<string, PauseReserveLabelVisual> Labels { get; init; }
    /// <summary>Decimal digit atlas references and starting location of the three-place reserve-energy display.</summary>
    public required PauseReserveDigitVisual Digits { get; init; }
    /// <summary>Arrow cells, enabled/disabled palettes, solid bevel colors, and the 32-phase pulse.</summary>
    public required PauseReserveArrowVisual Arrow { get; init; }
}
/// <summary>One reserve label's equipment-page anchor and consecutive native BG tile references.</summary>
public sealed record PauseReserveLabelVisual
{
    /// <summary>First destination tile cell in the 32-by-32 equipment page; subsequent words continue in native linear tilemap order.</summary>
    public required PauseGridPoint Anchor { get; init; }
    /// <summary>Ordered glyph/attribute definitions: seven for Mode or ReserveTank, four for Manual or Auto.</summary>
    public required PauseBackdropCell[] Cells { get; init; }
}
/// <summary>Editable decimal glyphs and placement for the reserve-supply display, independent of its numeric value.</summary>
public sealed record PauseReserveDigitVisual
{
    /// <summary>Destination of the leftmost supply digit; the next two places occupy consecutive native words.</summary>
    public required PauseGridPoint Anchor { get; init; }
    /// <summary>Exactly ten BG glyph/attribute definitions in decimal-value order, zero through nine; not ten destination digit places.</summary>
    public required PauseBackdropCell[] Cells { get; init; }
}
/// <summary>Editable arrow placement and palette behavior corresponding to native reserve-arrow owner <c>$82:AD0A</c>.</summary>
public sealed record PauseReserveArrowVisual
{
    /// <summary>Exactly ten distinct equipment-page cell locations; stock layout is an eight-cell vertical stem followed by a two-cell horizontal arm.</summary>
    public required PauseGridPoint[] Cells { get; init; }
    /// <summary>BG palette selector 0-7 applied when the pause owner enables the arrow; stock uses six.</summary>
    public required int EnabledPalette { get; init; }
    /// <summary>BG palette selector 0-7 applied when the pause owner disables the arrow; stock uses seven.</summary>
    public required int DisabledPalette { get; init; }
    /// <summary>RGB5 bevel color for arrow slot six when pulse animation is disabled.</summary>
    public required PaletteRgb5 SolidColor6 { get; init; }
    /// <summary>RGB5 bevel color for arrow slot eleven when pulse animation is disabled.</summary>
    public required PaletteRgb5 SolidColor11 { get; init; }
    /// <summary>Exactly 32 ordered pulse frames, selected by the low five bits of the accepted NMI counter; native colors originate at $82:AD5D/$AD9D.</summary>
    public required PauseReserveArrowFrame[] Frames { get; init; }
}
/// <summary>One pulse phase's two arrow bevel colors, each with red, green, and blue components from zero through 31.</summary>
public sealed record PauseReserveArrowFrame
{
    /// <summary>RGB5 color written to arrow palette slot six for this phase.</summary>
    public required PaletteRgb5 Color6 { get; init; }
    /// <summary>RGB5 color written to arrow palette slot eleven for this phase.</summary>
    public required PaletteRgb5 Color11 { get; init; }
}
/// <summary>Zero-based destination tile-cell coordinates on a 32-by-32 pause equipment page, distinct from character-atlas coordinates.</summary>
public sealed record PauseGridPoint
{
    /// <summary>Destination column in tile cells, zero through 31.</summary>
    public required int Column { get; init; }
    /// <summary>Destination row in tile cells, zero through 31; the native word offset is <c>Row * 32 + Column</c>.</summary>
    public required int Row { get; init; }
}
