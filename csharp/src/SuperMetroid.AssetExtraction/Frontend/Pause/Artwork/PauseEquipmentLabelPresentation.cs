using System.Text.Json;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>Editable label artwork and placement for the pause equipment page, kept as installed (#1615).</summary>
public sealed class PauseEquipmentLabelPresentation
{
    /// <summary>Each label's installed placement and tiles.</summary>
    private readonly Dictionary<PauseEquipmentLabelId, CompiledLabel> labels;
    /// <summary>The installed nine-cell blank strip used when inventory text is absent.</summary>
    private readonly BackgroundTile[] blank;

    /// <summary>Keeps the installed labels, blank strip and disabled palette.</summary>
    private PauseEquipmentLabelPresentation(Dictionary<PauseEquipmentLabelId, CompiledLabel> labels, BackgroundTile[] blank,
        int disabledPalette)
    {
        this.labels = labels;
        this.blank = blank;
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
        return Enumerable.Range(compiled.PageCell, label == PauseEquipmentLabelId.Hyper ? PauseEquipmentLabelArtworkDefinitions.BeamWords : compiled.CellCount);
    }

    /// <summary>Clears uncollected strips, recolors collected but unequipped strips, and replaces ordinary beam labels with Hyper when active.</summary>
    public void ApplyInventory(Span<BackgroundTile> tilemap, SuperMetroid.Core.Progression.SamusBeamFlags collectedBeams, SuperMetroid.Core.Progression.SamusBeamFlags equippedBeams,
        SuperMetroid.Core.Progression.SamusEquipmentFlags collectedItems, SuperMetroid.Core.Progression.SamusEquipmentFlags equippedItems, bool hyperBeam)
    {
        ValidateTilemap(tilemap);
        if (hyperBeam)
        {
            foreach (PauseUpgradeControl beam in PauseEquipmentControls.Beams)
            {
                CompiledLabel destination = labels[beam.Label];
                WriteBlank(tilemap.Slice(destination.PageCell, PauseEquipmentLabelArtworkDefinitions.BeamWords));
            }
            CompiledLabel hyper = labels[PauseEquipmentLabelId.Hyper];
            hyper.CopyTo(tilemap.Slice(hyper.PageCell, PauseEquipmentLabelArtworkDefinitions.BeamWords));
        }
        foreach (PauseUpgradeControl control in PauseEquipmentControls.Upgrades)
        {
            if (control.Category == PauseEquipmentCategory.Beams && hyperBeam) continue;

            PauseEquipmentBit mask = control.Upgrade;
            CompiledLabel label = labels[control.Label];
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

    /// <summary>Validates and compiles pause-equipment-labels.json into every label's placement and tiles.</summary>
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

        if (document.Version != PauseEquipmentLabelArtworkDefinitions.Version || document.Labels is null ||
            document.Blank is null || document.Blank.Length != PauseEquipmentLabelArtworkDefinitions.EquipmentWords ||
            (uint)document.DisabledPalette >= 8)
            throw new InvalidDataException("Pause equipment labels require version 1, nine blank cells and a valid disabled palette.");

        PauseEquipmentLabelId[] expected = Enum.GetValues<PauseEquipmentLabelId>();
        if (document.Labels.Count != expected.Length ||
            expected.Any(id => !document.Labels.ContainsKey(PauseEquipmentLabelKeys.Name(id))))
            throw new InvalidDataException("Pause equipment labels require the fourteen ordinary labels and Beam.Hyper exactly once.");

        var compiled = new Dictionary<PauseEquipmentLabelId, CompiledLabel>();
        var occupied = new HashSet<int>();
        foreach (PauseEquipmentLabelId id in expected)
        {
            string key = PauseEquipmentLabelKeys.Name(id);
            PauseEquipmentLabel label = document.Labels[key] ??
                throw new InvalidDataException($"Pause equipment label {key} is null.");
            int words = PauseEquipmentLabels.IsOrdinaryBeam(id)
                ? PauseEquipmentLabelArtworkDefinitions.BeamWords : PauseEquipmentLabelArtworkDefinitions.EquipmentWords;
            if (label.Cells is null || label.Cells.Length != words ||
                (uint)label.Column >= PauseEquipmentLabelArtworkDefinitions.TilemapColumns ||
                (uint)label.Row >= PauseEquipmentLabelArtworkDefinitions.TilemapRows ||
                label.Column + (id == PauseEquipmentLabelId.Hyper ? PauseEquipmentLabelArtworkDefinitions.BeamWords : words) > PauseEquipmentLabelArtworkDefinitions.TilemapColumns)
                throw new InvalidDataException($"Pause equipment label {key} has invalid cells or placement.");
            int destinationCell = label.Row * PauseEquipmentLabelArtworkDefinitions.TilemapColumns + label.Column;
            if (id != PauseEquipmentLabelId.Hyper)
            {
                for (int cell = destinationCell; cell < destinationCell + words; cell++)
                {
                    if (PauseEquipmentBaseTiles.IsNonInventoryLiveOwnedCell(cell))
                        throw new InvalidDataException($"Pause equipment label {key} overlaps wireframe or reserve state at cell {cell}.");
                    if (!occupied.Add(cell)) throw new InvalidDataException($"Pause equipment label {key} overlaps another label.");
                }
            }
            compiled.Add(id, new CompiledLabel(destinationCell, PauseTileGrid.Compile(label.Cells, key)));
        }

        return new(compiled, PauseTileGrid.Compile(document.Blank, "Equipment.Blank"), document.DisabledPalette);
    }

    /// <summary>Serializes the editable label schema to UTF-8 JSON and writes the bytes.</summary>
    /// <param name="output">Destination written at its current position and left open; existing trailing bytes are not truncated.</param>
    /// <param name="document">Label artwork and placement to serialize; the writer does not retain the document.</param>
    public static void Write(Stream output, PauseEquipmentLabelDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        output.Write(bytes);
    }

    /// <summary>
    /// Uncollected labels and Hyper's discarded beam slots show the installed blank strip across their selected width.
    /// </summary>
    private void WriteBlank(Span<BackgroundTile> destination) => blank.AsSpan(0, destination.Length).CopyTo(destination);

    /// <summary>Rejects partial tilemaps so fixed label destinations remain within the complete 32-by-32 pause tilemap.</summary>
    private static void ValidateTilemap(Span<BackgroundTile> tilemap)
    {
        if (tilemap.Length != PauseEquipmentLabelArtworkDefinitions.TilemapColumns * PauseEquipmentLabelArtworkDefinitions.TilemapRows)
            throw new ArgumentException("Pause equipment labels require a complete 32x32 tilemap.", nameof(tilemap));
    }

    /// <summary>Changes each tile's colour variant while retaining its character, priority and mirroring.</summary>
    private static void Recolor(Span<BackgroundTile> tiles, int palette)
    {
        foreach (ref BackgroundTile tile in tiles)
            tile = tile with { ColourVariant = palette };
    }

    /// <summary>One label's installed placement and tiles.</summary>
    /// <param name="pageCell">The equipment-page cell the label's first tile shows in.</param>
    /// <param name="tiles">The label's installed tiles, first to last.</param>
    private sealed class CompiledLabel(int pageCell, BackgroundTile[] tiles)
    {
        /// <summary>The installed equipment-page cell of the label's first tile.</summary>
        internal int PageCell => pageCell;
        /// <summary>The label strip's width, as installed.</summary>
        internal int CellCount => tiles.Length;

        /// <summary>Writes the label's first <paramref name="destination"/>-length tiles.</summary>
        internal void CopyTo(Span<BackgroundTile> destination) => tiles.AsSpan(0, destination.Length).CopyTo(destination);
    }
}
