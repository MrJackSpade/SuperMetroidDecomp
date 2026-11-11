using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>
/// Editable reserve labels, digits and arrow appearance (#1467), kept as installed (#1615): the import lays the equipment
/// page's reserve pictures out from the labels, digits and arrow cells, and the game binds only the arrow's colours.
/// Energy and mode behavior stay compiled.
/// </summary>
public sealed class PauseReserveUiPresentation
{
    /// <summary>The Mode, Reserve Tank, Manual, and Auto strips as installed.</summary>
    private readonly Dictionary<PauseReserveLabel, ReserveLabel> labels;
    /// <summary>Cell of the leftmost reserve-supply digit in the equipment tilemap.</summary>
    private readonly int digitCell;
    /// <summary>The installed tile of each decimal digit.</summary>
    private readonly BackgroundTile[] digits;
    /// <summary>The installed cells of the arrow, stem before arm.</summary>
    private readonly int[] arrowCells;
    /// <summary>BG palette selections used for active and inactive arrow tile cells.</summary>
    private readonly int enabledPalette, disabledPalette;

    /// <summary>Keeps the installed labels, digits, arrow cells and arrow colours.</summary>
    private PauseReserveUiPresentation(PauseReserveUiParts parts)
    {
        this.labels = parts.Labels; this.digitCell = parts.DigitCell;
        this.digits = [.. parts.Digits];
        this.arrowCells = [.. parts.ArrowCells];
        this.enabledPalette = parts.EnabledPalette;
        this.disabledPalette = parts.DisabledPalette;
        ArrowColours = parts.ArrowColours;
    }

    /// <summary>The arrow's solid and pulse colours, which the game binds to the pause screen (#1467).</summary>
    public PauseReserveArrowColours ArrowColours { get; }

    /// <summary>The equipment-page cells <paramref name="name"/> covers, first to last (#1467).</summary>
    /// <exception cref="InvalidDataException"><paramref name="name"/> is not a reserve label.</exception>
    internal IEnumerable<int> LabelCells(PauseReserveLabel name) => labels.TryGetValue(name, out ReserveLabel? label)
        ? Enumerable.Range(label.Cell, label.Tiles.Length) : throw new InvalidDataException($"Unknown reserve label {name}.");

    /// <summary>The equipment-page cell supply digit <paramref name="position"/> shows in, hundreds first (#1467).</summary>
    internal int DigitCell(int position) => (uint)position < PauseReserveUiDefinitions.SupplyDigitPlaces
        ? digitCell + position : throw new ArgumentOutOfRangeException(nameof(position));

    /// <summary>The ten equipment-page cells the reserve arrow's tiles occupy, stem before arm (#1467).</summary>
    internal IEnumerable<int> ArrowPageCells() => Array.AsReadOnly(arrowCells);

    /// <summary>Writes one selected label's tiles into its authored equipment-page location.</summary>
    /// <param name="tilemap">The equipment tilemap, large enough for the entire authored label range.</param>
    /// <param name="name">The label to write.</param>
    /// <param name="attributes">Whether to keep each destination tile's colour variant, priority and mirroring, replacing only its character; used for MANUAL/AUTO replacement.</param>
    /// <exception cref="InvalidDataException"><paramref name="name"/> is not a reserve label.</exception>
    /// <exception cref="ArgumentException">The destination span does not contain the authored label range.</exception>
    public void ApplyLabel(Span<BackgroundTile> tilemap, PauseReserveLabel name, LabelAttributes attributes = LabelAttributes.Replace)
    {
        if (!labels.TryGetValue(name, out var label)) throw new InvalidDataException($"Unknown reserve label {name}.");
        if (label.Cell < 0 || label.Cell + label.Tiles.Length > tilemap.Length)
            throw new ArgumentException("Reserve label target is shorter than the authored tilemap range.", nameof(tilemap));
        for (int index = 0; index < label.Tiles.Length; index++)
        {
            BackgroundTile tile = label.Tiles[index];
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
        tilemap[digitCell + position] = digits[value];
    }

    /// <summary>Replaces only the colour variants of the ten authored arrow cells, preserving their characters, priority and mirroring.</summary>
    /// <param name="tilemap">The equipment tilemap containing every authored arrow cell.</param>
    /// <param name="enabled">Selects the authored enabled palette when true or disabled palette when false; does not change reserve mode or transfer energy.</param>
    /// <exception cref="IndexOutOfRangeException">The destination lacks an authored arrow cell.</exception>
    public void ApplyArrowTilePalettes(Span<BackgroundTile> tilemap, bool enabled)
    {
        int palette = enabled ? enabledPalette : disabledPalette;
        foreach (int at in arrowCells)
            tilemap[at] = tilemap[at] with { ColourVariant = palette };
    }

    /// <summary>Compiles the selected reserve labels, digit words, arrow placements and arrow colours into a presentation.</summary>
    /// <param name="selection">The selected labels, digits, arrow placements and arrow colours.</param>
    internal static PauseReserveUiPresentation Create(PauseReserveUiSelection selection)
    {
        Ensure.NotNull(selection);
        var labels = new Dictionary<PauseReserveLabel, ReserveLabel>();
        foreach (KeyValuePair<PauseReserveLabel, int> label in selection.LabelCells)
            labels.Add(label.Key, new ReserveLabel(label.Value, [.. selection.LabelTiles[label.Key]]));
        return new(new PauseReserveUiParts { Labels = labels, DigitCell = selection.DigitCell, Digits = selection.Digits, ArrowCells = selection.ArrowCells, EnabledPalette = selection.EnabledPalette, DisabledPalette = selection.DisabledPalette, ArrowColours = selection.ArrowColours });
    }

    /// <summary>One reserve label's installed placement and tiles.</summary>
    /// <param name="Cell">The equipment-page cell the label's first tile shows in.</param>
    /// <param name="Tiles">The label's installed tiles, first to last.</param>
    private sealed record ReserveLabel(int Cell, BackgroundTile[] Tiles);

    /// <summary>The pause screen's reserve-tank labels, digits and arrow colours (#633).</summary>
    private sealed record PauseReserveUiParts
    {
        /// <summary>The four installed strips.</summary>
        public required Dictionary<PauseReserveLabel, ReserveLabel> Labels { get; init; }

        /// <summary>Cell of the three-place reserve supply's first digit in the equipment tilemap.</summary>
        public required int DigitCell { get; init; }

        /// <summary>Ten installed decimal glyph tiles.</summary>
        public required BackgroundTile[] Digits { get; init; }

        /// <summary>Ten distinct arrow cells, vertical stem before horizontal arm.</summary>
        public required int[] ArrowCells { get; init; }

        /// <summary>BG palette selector used when the pause owner enables reserve control.</summary>
        public required int EnabledPalette { get; init; }

        /// <summary>BG palette selector used when reserve control is unavailable.</summary>
        public required int DisabledPalette { get; init; }

        /// <summary>The arrow's solid and pulse colours.</summary>
        public required PauseReserveArrowColours ArrowColours { get; init; }
    }
}
