using System.Text.Json;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>Editable label artwork and placement for the pause equipment page.</summary>
public sealed class PauseEquipmentLabelPresentation
{
    /// <summary>Each selector anchor retains only cells and placement that differ from stock pause artwork.</summary>
    private readonly Dictionary<PauseEquipmentLabelId, CompiledLabel> labels;
    /// <summary>Edits to the nine-cell native blank strip used when inventory text is absent, by cell.</summary>
    internal readonly Dictionary<int, BackgroundTile> blankEdits;

    /// <summary>Captures compiled label differences and the disabled palette.</summary>
    private PauseEquipmentLabelPresentation(Dictionary<PauseEquipmentLabelId, CompiledLabel> labels, Dictionary<int, BackgroundTile> blankEdits,
        int disabledPalette)
    {
        this.labels = labels;
        this.blankEdits = blankEdits;
        DisabledPalette = disabledPalette;
    }

    /// <summary>Colour variant 0..7 substituted into collected but unequipped labels; the retail gray-text variant is three.</summary>
    public int DisabledPalette { get; }
    /// <summary>
    /// The equipment-page cells a write of <paramref name="label"/> covers, first to last (#1467): an ordinary label's own
    /// width, and Hyper's five displayed cells.
    /// </summary>
    internal IEnumerable<int> PageCells(PauseEquipmentLabelId label)
    {
        CompiledLabel compiled = labels[label];
        return Enumerable.Range(compiled.PageCell, label == PauseEquipmentLabelId.Hyper ? PauseEquipmentLabelDefinitions.BeamWords : compiled.CellCount);
    }

    /// <summary>The cells after Plasma's own that its nine-cell button overrun writes Varia's first cells to (#1467).</summary>
    internal IEnumerable<int> PlasmaOverrunCells()
    {
        CompiledLabel plasma = labels[PauseEquipmentLabelId.Plasma];
        return Enumerable.Range(plasma.PageCell + plasma.CellCount, PauseEquipmentLabelDefinitions.EquipmentWords - plasma.CellCount);
    }

    /// <summary>Clears uncollected strips, recolors collected but unequipped strips, and replaces ordinary beam labels with Hyper when active.</summary>
    public void ApplyInventory(Span<BackgroundTile> tilemap, SuperMetroid.Core.Progression.SamusBeamFlags collectedBeams, SuperMetroid.Core.Progression.SamusBeamFlags equippedBeams,
        SuperMetroid.Core.Progression.SamusEquipmentFlags collectedItems, SuperMetroid.Core.Progression.SamusEquipmentFlags equippedItems, bool hyperBeam)
    {
        ValidateTilemap(tilemap);
        if (hyperBeam)
        {
            for (int item = 0; item < PauseEquipmentLabelTiles.ItemCount(PauseEquipmentCategory.Beams); item++)
            {
                CompiledLabel destination = labels[PauseEquipmentLabelTiles.Label(PauseEquipmentCategory.Beams, item)];
                WriteBlank(tilemap.Slice(destination.PageCell, PauseEquipmentLabelDefinitions.BeamWords));
            }
            CompiledLabel hyper = labels[PauseEquipmentLabelId.Hyper];
            hyper.CopyTo(tilemap.Slice(hyper.PageCell, PauseEquipmentLabelDefinitions.BeamWords));
        }
        foreach (PauseEquipmentCategory category in PauseEquipmentCategories.InventoryCategories)
        for (int item = 0; item < PauseEquipmentLabelTiles.ItemCount(category); item++)
        {
            if (category == PauseEquipmentCategory.Beams && hyperBeam) continue;

            SuperMetroid.Core.Frontend.Pause.PauseEquipmentBit mask = SuperMetroid.Core.Frontend.Pause.PauseEquipmentRules.Mask(category, item);
            CompiledLabel label = labels[PauseEquipmentLabelTiles.Label(category, item)];
            Span<BackgroundTile> destination = tilemap.Slice(label.PageCell, label.CellCount);
            if (!mask.IsSetIn(collectedBeams, collectedItems))
                WriteBlank(destination);
            else
            {
                label.CopyTo(destination);
                if (!mask.IsSetIn(equippedBeams, equippedItems)) Recolor(destination, DisabledPalette);
            }
        }
    }

    /// <summary>
    /// Applies one native equipment-button patch. A nine-cell Plasma write deliberately
    /// continues into the first four Varia cells, retaining the retail VAR glitch.
    /// </summary>
    public void ApplyLabel(Span<BackgroundTile> tilemap, PauseEquipmentCategory category, int item, int wordCount, bool disabled)
    {
        ValidateTilemap(tilemap);
        PauseEquipmentLabelId id = PauseEquipmentLabelTiles.Label(category, item);
        CompiledLabel label = labels[id];
        if (wordCount is < 0 or > PauseEquipmentLabelDefinitions.EquipmentWords)
            throw new ArgumentOutOfRangeException(nameof(wordCount));
        Span<BackgroundTile> destination = tilemap.Slice(label.PageCell, wordCount);
        int ordinaryCells = Math.Min(destination.Length, label.CellCount);
        label.CopyTo(destination[..ordinaryCells]);
        if (ordinaryCells < destination.Length)
        {
            if (id != PauseEquipmentLabelId.Plasma)
                throw new InvalidDataException($"Pause label {id} cannot supply a {wordCount}-cell native overrun.");
            labels[PauseEquipmentLabelId.Varia].CopyTo(destination[ordinaryCells..]);
        }
        if (disabled) Recolor(destination, DisabledPalette);
    }

    /// <summary>Validates and compiles pause-equipment-labels.json into placement and tile edits relative to the native $82:BF32-C01A label artwork.</summary>
    /// <param name="json">UTF-8 JSON source read from its current position to the end and left open.</param>
    /// <returns>Compiled inventory-label artwork independent of the mutable document collections and cartridge memory.</returns>
    /// <exception cref="InvalidDataException">The JSON, version, exact label set, cell references, placement, disabled palette, or permitted label/live-state footprints are invalid.</exception>
    public static PauseEquipmentLabelPresentation Load(Stream json)
    {
        byte[] source;
        using (var buffer = new MemoryStream()) { json.CopyTo(buffer); source = buffer.ToArray(); }
        PauseEquipmentLabelDocument document;
        try
        {
            document = JsonAssetDocument.Read<PauseEquipmentLabelDocument>(source, ArtworkJson.Options)
                ?? throw new InvalidDataException("Pause equipment label document is null.");
        }
        catch (JsonException error) { throw new InvalidDataException("Invalid pause equipment label JSON.", error); }

        if (document.Version != PauseEquipmentLabelDefinitions.Version || document.Labels is null ||
            document.Blank is null || document.Blank.Length != PauseEquipmentLabelDefinitions.EquipmentWords ||
            (uint)document.DisabledPalette >= 8)
            throw new InvalidDataException("Pause equipment labels require version 1, nine blank cells and a valid disabled palette.");

        PauseEquipmentLabelId[] expected = Enum.GetValues<PauseEquipmentLabelId>();
        if (document.Labels.Count != expected.Length ||
            expected.Any(id => !document.Labels.ContainsKey(PauseEquipmentLabelTiles.Name(id))))
            throw new InvalidDataException("Pause equipment labels require the fourteen ordinary labels and Beam.Hyper exactly once.");

        var compiled = new Dictionary<PauseEquipmentLabelId, CompiledLabel>();
        var occupied = new HashSet<int>();
        foreach (PauseEquipmentLabelId id in expected)
        {
            string key = PauseEquipmentLabelTiles.Name(id);
            PauseEquipmentLabel label = document.Labels[key] ??
                throw new InvalidDataException($"Pause equipment label {key} is null.");
            int words = PauseEquipmentLabelTiles.IsOrdinaryBeam(id)
                ? PauseEquipmentLabelDefinitions.BeamWords : PauseEquipmentLabelDefinitions.EquipmentWords;
            if (label.Cells is null || label.Cells.Length != words ||
                (uint)label.Column >= PauseEquipmentLabelDefinitions.TilemapColumns ||
                (uint)label.Row >= PauseEquipmentLabelDefinitions.TilemapRows ||
                label.Column + (id == PauseEquipmentLabelId.Hyper ? PauseEquipmentLabelDefinitions.BeamWords : words) > PauseEquipmentLabelDefinitions.TilemapColumns)
                throw new InvalidDataException($"Pause equipment label {key} has invalid cells or placement.");
            int destinationCell = label.Row * PauseEquipmentLabelDefinitions.TilemapColumns + label.Column;
            if (id != PauseEquipmentLabelId.Hyper)
            {
                int footprint = id == PauseEquipmentLabelId.Plasma
                    ? PauseEquipmentLabelDefinitions.EquipmentWords : words;
                for (int cell = destinationCell; cell < destinationCell + footprint; cell++)
                {
                    bool plasmaWireframeOverlap = id == PauseEquipmentLabelId.Plasma &&
                        destinationCell == PauseEquipmentLabelDefinitions.PlasmaCell &&
                        cell == PauseEquipmentLabelDefinitions.PlasmaWireframeOverlapCell;
                    if (PauseEquipmentBaseTiles.IsNonInventoryLiveOwnedCell(cell) &&
                        !plasmaWireframeOverlap)
                        throw new InvalidDataException($"Pause equipment label {key} overlaps wireframe or reserve state at cell {cell}.");
                    if (!occupied.Add(cell)) throw new InvalidDataException($"Pause equipment label {key} overlaps another label.");
                }
            }
            compiled.Add(id, CompiledLabel.Load(id, destinationCell, PauseTileGrid.Compile(label.Cells, key)));
        }

        BackgroundTile[] blank = PauseTileGrid.Compile(document.Blank, "Equipment.Blank");
        var blankEdits = new Dictionary<int, BackgroundTile>();
        for (int cell = 0; cell < PauseEquipmentLabelDefinitions.EquipmentWords; cell++)
            if (blank[cell] != default) blankEdits.Add(cell, blank[cell]);
        return new(compiled, blankEdits, document.DisabledPalette);
    }

    /// <summary>Serializes the editable label schema to UTF-8 JSON, validates it through <see cref="Load"/>, then writes the validated bytes.</summary>
    /// <param name="output">Destination written at its current position and left open; existing trailing bytes are not truncated.</param>
    /// <param name="document">Label artwork and placement to serialize; the writer does not retain the document.</param>
    /// <exception cref="InvalidDataException">The serialized document fails the loader's schema, cell, or placement validation.</exception>
    public static void Write(Stream output, PauseEquipmentLabelDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        output.Write(bytes);
    }

    /// <summary>
    /// $82:C01A-C02B supplies nine empty cells. Uncollected labels and Hyper's
    /// discarded beam slots clear their selected width; only explicit asset edits
    /// need per-cell data. The source JSON remains the content-identity contract.
    /// </summary>
    private void WriteBlank(Span<BackgroundTile> destination)
    {
        destination.Clear();
        foreach (KeyValuePair<int, BackgroundTile> edit in blankEdits)
            if (edit.Key < destination.Length)
                destination[edit.Key] = edit.Value;
    }

    /// <summary>Rejects partial tilemaps so fixed label destinations remain within the complete 32-by-32 pause tilemap.</summary>
    private static void ValidateTilemap(Span<BackgroundTile> tilemap)
    {
        if (tilemap.Length != PauseEquipmentLabelDefinitions.TilemapColumns * PauseEquipmentLabelDefinitions.TilemapRows)
            throw new ArgumentException("Pause equipment labels require a complete 32x32 tilemap.", nameof(tilemap));
    }

    /// <summary>Changes each tile's colour variant while retaining its character, priority and mirroring.</summary>
    private static void Recolor(Span<BackgroundTile> tiles, int palette)
    {
        foreach (ref BackgroundTile tile in tiles)
            tile = tile with { ColourVariant = palette };
    }

    /// <summary>Stores placement and tile differences while resolving unchanged cells from stock label definitions.</summary>
    private sealed class CompiledLabel(PauseEquipmentLabelId id, int? destinationEdit, Dictionary<int, BackgroundTile> edits)
    {
        /// <summary>Returns the edited cell or the stock selector's equipment-page cell.</summary>
        internal int PageCell => destinationEdit ?? PauseEquipmentLabelTiles.StockPageByte(id) / sizeof(ushort);
        /// <summary>Uses the native selector's width even when its authored strip has edited cells.</summary>
        internal int CellCount => PauseEquipmentLabelTiles.StockWordCount(id);

        /// <summary>Retains only tiles and destination coordinates that differ from the native selector.</summary>
        internal static CompiledLabel Load(PauseEquipmentLabelId id, int destinationCell, ReadOnlySpan<BackgroundTile> tiles)
        {
            var edits = new Dictionary<int, BackgroundTile>();
            for (int cell = 0; cell < tiles.Length; cell++)
                if (tiles[cell] != PauseEquipmentLabelTiles.StockTile(id, cell)) edits.Add(cell, tiles[cell]);
            int? changedDestination = destinationCell == PauseEquipmentLabelTiles.StockPageByte(id) / sizeof(ushort)
                ? null : destinationCell;
            return new(id, changedDestination, edits);
        }

        /// <summary>Materializes edited cells over stock artwork for the requested prefix, including Plasma's extended write.</summary>
        internal void CopyTo(Span<BackgroundTile> destination)
        {
            for (int cell = 0; cell < destination.Length; cell++)
                destination[cell] = edits.TryGetValue(cell, out BackgroundTile edited) ? edited
                    : PauseEquipmentLabelTiles.StockTile(id, cell);
        }
    }
}
