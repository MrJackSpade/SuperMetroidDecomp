using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>Editable reserve labels, digits and arrow appearance; energy and mode behavior stay compiled.</summary>
public sealed class PauseReserveUiPresentation
{
    /// <summary>Mode, Reserve Tank, Manual, and Auto strips retaining only departures from stock artwork.</summary>
    private readonly Dictionary<PauseReserveLabel, ReserveLabel> labels;
    /// <summary>Cell of the leftmost reserve-supply digit in the equipment tilemap.</summary>
    private readonly int digitCell;
    /// <summary>Optional authored digit tiles; null reuses the consecutive stock glyph sequence.</summary>
    internal readonly BackgroundTile[]? digits;
    /// <summary>Optional authored arrow cells; null uses the native stem and arm layout.</summary>
    internal readonly int[]? arrowCells;
    /// <summary>BG palette selections used for active and inactive arrow tile cells.</summary>
    private readonly int enabledPalette, disabledPalette;
    /// <summary>Two bevel colors shown when AUTO pulse animation is inactive.</summary>
    private readonly RgbColor solidColor6, solidColor11;
    /// <summary>Only pulse starting colors that differ from the reversed solid bevel colors.</summary>
    private readonly ReserveArrowColorEdits arrowStartEdits;
    /// <summary>Grey midpoint levels from which each mirrored pulse ramp is calculated.</summary>
    private readonly ReserveArrowGreyLevels arrowGreyLevels;
    /// <summary>Independently edited pulse colors that depart from the mirrored ramp.</summary>
    internal readonly Dictionary<ColourCell, RgbColor> arrowColorEdits = [];

    /// <summary>Compacts stock-equivalent digit/arrow positions and derives sparse label and pulse-color edits.</summary>
    private PauseReserveUiPresentation(PauseReserveUiParts parts)
    {
        this.labels = parts.Labels; this.digitCell = parts.DigitCell;
        bool stockDigits = true;
        for (int digit = 0; digit < parts.Digits.Length; digit++)
            stockDigits &= parts.Digits[digit] == PauseReserveUiTiles.DigitTile(digit);
        this.digits = stockDigits ? null : parts.Digits;
        bool stockArrow = true;
        for (int cell = 0; cell < parts.ArrowCells.Length; cell++)
            stockArrow &= parts.ArrowCells[cell] == ArrowCell(cell);
        this.arrowCells = stockArrow ? null : parts.ArrowCells;
        this.enabledPalette = parts.EnabledPalette;
        this.disabledPalette = parts.DisabledPalette; this.solidColor6 = parts.SolidColor6;
        this.solidColor11 = parts.SolidColor11;
        arrowStartEdits = new ReserveArrowColorEdits(parts.ArrowFrames[0].Color6 == parts.SolidColor11 ? null : parts.ArrowFrames[0].Color6,
            parts.ArrowFrames[0].Color11 == parts.SolidColor6 ? null : parts.ArrowFrames[0].Color11);
        var middle = parts.ArrowFrames[PauseReserveUiDefinitions.ArrowFrames / 2 - 1];
        arrowGreyLevels = new ReserveArrowGreyLevels(middle.Color6.Red, middle.Color11.Red);
        for (int frame = 0; frame < parts.ArrowFrames.Length; frame++)
        {
            if (parts.ArrowFrames[frame].Color6 != CalculateArrowColor(frame, ReserveArrowBevel.Shade))
                arrowColorEdits.Add(new ColourCell(frame, 0), parts.ArrowFrames[frame].Color6);
            if (parts.ArrowFrames[frame].Color11 != CalculateArrowColor(frame, ReserveArrowBevel.Light))
                arrowColorEdits.Add(new ColourCell(frame, 1), parts.ArrowFrames[frame].Color11);
        }
    }

    /// <summary>
    /// Native reserve-arrow cells form an eight-cell vertical stem followed by a two-cell
    /// horizontal arm in the 32-column equipment page.
    /// </summary>
    private static int ArrowCell(int cell) => cell < PauseReserveUiDefinitions.VerticalCount
        ? PauseReserveUiDefinitions.VerticalStartCell + cell * PauseReserveUiDefinitions.RowStrideCells
        : PauseReserveUiDefinitions.HorizontalStartCell + cell - PauseReserveUiDefinitions.VerticalCount;
    /// <summary>The equipment-page cells <paramref name="name"/> covers, first to last (#1467).</summary>
    /// <exception cref="InvalidDataException"><paramref name="name"/> is not a reserve label.</exception>
    internal IEnumerable<int> LabelCells(PauseReserveLabel name) => labels.TryGetValue(name, out ReserveLabel? label)
        ? Enumerable.Range(label.Cell, label.CellCount) : throw new InvalidDataException($"Unknown reserve label {name}.");

    /// <summary>The equipment-page cell supply digit <paramref name="position"/> shows in, hundreds first (#1467).</summary>
    internal int DigitCell(int position) => (uint)position < PauseReserveUiDefinitions.SupplyDigitPlaces
        ? digitCell + position : throw new ArgumentOutOfRangeException(nameof(position));

    /// <summary>The ten equipment-page cells the reserve arrow's tiles occupy, stem before arm (#1467).</summary>
    internal IEnumerable<int> ArrowPageCells() =>
        Enumerable.Range(0, PauseReserveUiDefinitions.VerticalCount + PauseReserveUiDefinitions.HorizontalCount)
            .Select(cell => arrowCells is null ? ArrowCell(cell) : arrowCells[cell]);

    /// <summary>Writes one selected label's tiles into its authored equipment-page location.</summary>
    /// <param name="tilemap">The equipment tilemap, large enough for the entire authored label range.</param>
    /// <param name="name">The label to write.</param>
    /// <param name="attributes">Whether to keep each destination tile's colour variant, priority and mirroring, replacing only its character; used for MANUAL/AUTO replacement.</param>
    /// <exception cref="InvalidDataException"><paramref name="name"/> is not a reserve label.</exception>
    /// <exception cref="ArgumentException">The destination span does not contain the authored label range.</exception>
    public void ApplyLabel(Span<BackgroundTile> tilemap, PauseReserveLabel name, LabelAttributes attributes = LabelAttributes.Replace)
    {
        if (!labels.TryGetValue(name, out var label)) throw new InvalidDataException($"Unknown reserve label {name}.");
        if (label.Cell < 0 || label.Cell + label.CellCount > tilemap.Length)
            throw new ArgumentException("Reserve label target is shorter than the authored tilemap range.", nameof(tilemap));
        for (int index = 0; index < label.CellCount; index++)
        {
            BackgroundTile tile = label.Tile(index);
            tilemap[label.Cell + index] = attributes == LabelAttributes.Preserve
                ? tilemap[label.Cell + index] with { Tile = tile.Tile } : tile;
        }
    }

    /// <summary>Writes one selected decimal glyph into the reserve-supply display without calculating the energy value.</summary>
    /// <param name="tilemap">The equipment tilemap containing the authored three-cell digit range.</param>
    /// <param name="position">Zero-based left-to-right digit place, zero through two; callers supply hundreds, tens, and units in that order.</param>
    /// <param name="value">Decimal glyph identity, zero through nine.</param>
    /// <exception cref="ArgumentOutOfRangeException">The digit position/value is invalid or the destination lacks the selected cell.</exception>
    public void ApplyDigit(Span<BackgroundTile> tilemap, int position, int value)
    {
        if ((uint)position >= PauseReserveUiDefinitions.SupplyDigitPlaces || (uint)value >= PauseReserveUiDefinitions.DigitCount)
            throw new ArgumentOutOfRangeException();
        tilemap[digitCell + position] = digits is null ? PauseReserveUiTiles.DigitTile(value) : digits[value];
    }

    /// <summary>Replaces only the colour variants of the ten authored arrow cells, preserving their characters, priority and mirroring.</summary>
    /// <param name="tilemap">The equipment tilemap containing every authored arrow cell.</param>
    /// <param name="enabled">Selects the authored enabled palette when true or disabled palette when false; does not change reserve mode or transfer energy.</param>
    /// <exception cref="IndexOutOfRangeException">The destination lacks an authored arrow cell.</exception>
    public void ApplyArrowTilePalettes(Span<BackgroundTile> tilemap, bool enabled)
    {
        int palette = enabled ? enabledPalette : disabledPalette;
        int cells = PauseReserveUiDefinitions.VerticalCount + PauseReserveUiDefinitions.HorizontalCount;
        for (int cell = 0; cell < cells; cell++)
        {
            int at = arrowCells is null ? ArrowCell(cell) : arrowCells[cell];
            tilemap[at] = tilemap[at] with { ColourVariant = palette };
        }
    }

    /// <summary>Writes the two selected arrow bevel colours to the screen's colours, using either the pulse frame or the solid appearance.</summary>
    /// <param name="colours">The screen's colours, whose reserve-arrow bevel colours change.</param>
    /// <param name="animated">Whether to sample the 32-phase pulse rather than the solid colors; the pause owner decides when AUTO selection should animate.</param>
    /// <param name="frame">Pulse phase, masked to its low five bits; normally the accepted eight-bit frame counter and ignored for solid colors.</param>
    public void ApplyArrowColors(ScreenPalette colours, bool animated, int frame)
    {
        var colors = animated ? new ReserveArrowColors(ArrowColor(frame, ReserveArrowBevel.Shade), ArrowColor(frame, ReserveArrowBevel.Light)) : new ReserveArrowColors(solidColor6, solidColor11);
        Ensure.NotNull(colours).Colours(ScreenColourGroup.PauseReserveArrowShade).Fill(colors.Color6);
        colours.Colours(ScreenColourGroup.PauseReserveArrowLight).Fill(colors.Color11);
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
    private RgbColor CalculateArrowColor(int frame, ReserveArrowBevel bevel)
    {
        int last = PauseReserveUiDefinitions.ArrowFrames - 1;
        int phase = Math.Min(frame, last - frame);
        int steps = last / 2;
        bool second = bevel == ReserveArrowBevel.Light;
        RgbColor start = second ? arrowStartEdits.Color11 ?? solidColor6 : arrowStartEdits.Color6 ?? solidColor11;
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
    private RgbColor ArrowColor(int frame, ReserveArrowBevel bevel)
    {
        frame &= PauseReserveUiDefinitions.ArrowFrames - 1;
        return arrowColorEdits.TryGetValue(new ColourCell(frame, (bevel == ReserveArrowBevel.Light ? 1 : 0)), out RgbColor supplied)
            ? supplied : CalculateArrowColor(frame, bevel);
    }

    /// <summary>Compiles the selected reserve labels, digit words, arrow placements and arrow colours into a presentation.</summary>
    /// <param name="selection">The selected labels, digits, arrow placements and arrow colours.</param>
    internal static PauseReserveUiPresentation Create(PauseReserveUiSelection selection)
    {
        Ensure.NotNull(selection);
        var labels = new Dictionary<PauseReserveLabel, ReserveLabel>();
        foreach (KeyValuePair<PauseReserveLabel, int> label in selection.LabelCells)
            labels.Add(label.Key, ReserveLabel.Load(label.Key, label.Value, selection.LabelTiles[label.Key]));
        return new(new PauseReserveUiParts { Labels = labels, DigitCell = selection.DigitCell, Digits = selection.Digits, ArrowCells = selection.ArrowCells, EnabledPalette = selection.EnabledPalette, DisabledPalette = selection.DisabledPalette, SolidColor6 = selection.SolidColor6, SolidColor11 = selection.SolidColor11, ArrowFrames = selection.ArrowFrames });
    }

    /// <summary>Resolves unchanged reserve-label cells from stock and stores only edited placement or tiles.</summary>
    private sealed class ReserveLabel(PauseReserveLabel name, int? cellEdit, Dictionary<int, BackgroundTile> edits)
    {
        /// <summary>Uses an authored placement when changed, otherwise the native label anchor.</summary>
        internal int Cell => cellEdit ?? PauseReserveUiTiles.StockLabelOffset(name) / sizeof(ushort);
        /// <summary>Uses the native strip width for the selected label.</summary>
        internal int CellCount => PauseReserveUiTiles.StockLabelWords(name);
        /// <summary>Returns an edited tile or the corresponding stock glyph and attributes.</summary>
        internal BackgroundTile Tile(int index) => edits.TryGetValue(index, out BackgroundTile edited)
            ? edited : PauseReserveUiTiles.StockLabelTile(name, index);

        /// <summary>Compares every supplied tile and the destination with stock, retaining only differences.</summary>
        internal static ReserveLabel Load(PauseReserveLabel name, int cell, ReadOnlySpan<BackgroundTile> tiles)
        {
            var edits = new Dictionary<int, BackgroundTile>();
            for (int index = 0; index < tiles.Length; index++)
                if (tiles[index] != PauseReserveUiTiles.StockLabelTile(name, index)) edits.Add(index, tiles[index]);
            int? editedCell = cell == PauseReserveUiTiles.StockLabelOffset(name) / sizeof(ushort) ? null : cell;
            return new(name, editedCell, edits);
        }
    }

    /// <summary>The pause screen's reserve-tank labels, digits and arrow colours (#633).</summary>
    private sealed record PauseReserveUiParts
    {
        /// <summary>The four compiled strips with stock-equivalent cells omitted.</summary>
        public required Dictionary<PauseReserveLabel, ReserveLabel> Labels { get; init; }

        /// <summary>Cell of the three-place reserve supply's first digit in the equipment tilemap.</summary>
        public required int DigitCell { get; init; }

        /// <summary>Ten compiled decimal glyph tiles before stock-equivalent compression.</summary>
        public required BackgroundTile[] Digits { get; init; }

        /// <summary>Ten distinct arrow cells, vertical stem before horizontal arm.</summary>
        public required int[] ArrowCells { get; init; }

        /// <summary>BG palette selector used when the pause owner enables reserve control.</summary>
        public required int EnabledPalette { get; init; }

        /// <summary>BG palette selector used when reserve control is unavailable.</summary>
        public required int DisabledPalette { get; init; }

        /// <summary>Static arrow bevel color for palette slot six outside pulse animation.</summary>
        public required RgbColor SolidColor6 { get; init; }

        /// <summary>Static arrow bevel color for palette slot eleven outside pulse animation.</summary>
        public required RgbColor SolidColor11 { get; init; }

        /// <summary>Two RGB5 bevel colors for each of the 32 mirrored pulse phases.</summary>
        public required ReserveArrowColors[] ArrowFrames { get; init; }
    }
}
