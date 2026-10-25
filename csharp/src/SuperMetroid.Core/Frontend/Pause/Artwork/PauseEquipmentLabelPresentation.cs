using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>Editable label artwork and placement for the pause equipment page.</summary>
public sealed class PauseEquipmentLabelPresentation
{
    /// <summary>Each selector anchor retains only cells and placement that differ from stock pause artwork.</summary>
    private readonly Dictionary<PauseEquipmentLabelId, CompiledLabel> labels;
    /// <summary>Nonzero edits to the nine-cell native blank strip used when inventory text is absent.</summary>
    internal readonly Dictionary<int, ushort> blankEdits;

    /// <summary>Captures compiled label differences, the disabled palette, and the hash of the source JSON bytes.</summary>
    private PauseEquipmentLabelPresentation(Dictionary<PauseEquipmentLabelId, CompiledLabel> labels, Dictionary<int, ushort> blankEdits,
        int disabledPalette, string contentIdentity)
    {
        this.labels = labels;
        this.blankEdits = blankEdits;
        DisabledPalette = disabledPalette;
        ContentIdentity = contentIdentity;
    }

    /// <summary>BG palette selector 0..7 substituted into collected but unequipped labels; the retail gray-text selector is three.</summary>
    public int DisabledPalette { get; }
    /// <summary>Uppercase hexadecimal SHA-256 of the exact source JSON bytes, used by pause-menu rebinding to decide whether to rebuild inventory labels.</summary>
    public string ContentIdentity { get; }

    /// <summary>Includes Hyper's five-word display and Plasma's nine-word overrun when deciding whether rebinding may replace a live tilemap cell.</summary>
    internal bool OwnsLiveCell(int cell)
    {
        foreach ((PauseEquipmentLabelId id, CompiledLabel label) in labels)
        {
            int words = id == PauseEquipmentLabelId.Plasma
                ? PauseEquipmentLabelDefinitions.EquipmentWords
                : id == PauseEquipmentLabelId.Hyper
                    ? PauseEquipmentLabelDefinitions.BeamWords
                    : label.WordCount;
            int first = label.PageByte / sizeof(ushort);
            if ((uint)(cell - first) < words) return true;
        }
        return false;
    }

    /// <summary>Clears uncollected strips, recolors collected but unequipped strips, and replaces ordinary beam labels with Hyper when active.</summary>
    public void ApplyInventory(Span<byte> tilemap, SuperMetroid.Core.Progression.SamusBeamFlags collectedBeams, SuperMetroid.Core.Progression.SamusBeamFlags equippedBeams,
        SuperMetroid.Core.Progression.SamusEquipmentFlags collectedItems, SuperMetroid.Core.Progression.SamusEquipmentFlags equippedItems, bool hyperBeam)
    {
        ValidateTilemap(tilemap);
        if (hyperBeam)
        {
            for (int item = 0; item < PauseEquipmentLabelDefinitions.ItemCount(PauseEquipmentCategory.Beams); item++)
            {
                CompiledLabel destination = labels[PauseEquipmentLabelDefinitions.Label(PauseEquipmentCategory.Beams, item)];
                WriteBlank(tilemap.Slice(destination.PageByte,
                    PauseEquipmentLabelDefinitions.BeamWords * sizeof(ushort)));
            }
            CompiledLabel hyper = labels[PauseEquipmentLabelId.Hyper];
            hyper.CopyTo(tilemap.Slice(hyper.PageByte,
                    PauseEquipmentLabelDefinitions.BeamWords * sizeof(ushort)));
        }
        foreach (PauseEquipmentCategory category in PauseEquipmentCategories.InventoryCategories)
        for (int item = 0; item < PauseEquipmentLabelDefinitions.ItemCount(category); item++)
        {
            if (category == PauseEquipmentCategory.Beams && hyperBeam) continue;

            SuperMetroid.Core.Frontend.Pause.PauseEquipmentBit mask = SuperMetroid.Core.Frontend.Pause.PauseEquipmentRules.Mask(category, item);
            CompiledLabel label = labels[PauseEquipmentLabelDefinitions.Label(category, item)];
            Span<byte> destination = tilemap.Slice(label.PageByte, label.WordCount * sizeof(ushort));
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
    /// Applies one native equipment-button patch. A nine-word Plasma write deliberately
    /// continues into the first four Varia words, retaining the retail VAR glitch.
    /// </summary>
    public void ApplyLabel(Span<byte> tilemap, PauseEquipmentCategory category, int item, int wordCount, bool disabled)
    {
        ValidateTilemap(tilemap);
        PauseEquipmentLabelId id = PauseEquipmentLabelDefinitions.Label(category, item);
        CompiledLabel label = labels[id];
        if (wordCount is < 0 or > PauseEquipmentLabelDefinitions.EquipmentWords)
            throw new ArgumentOutOfRangeException(nameof(wordCount));
        Span<byte> destination = tilemap.Slice(label.PageByte, wordCount * sizeof(ushort));
        int ordinaryBytes = Math.Min(destination.Length, label.WordCount * sizeof(ushort));
        label.CopyTo(destination[..ordinaryBytes]);
        if (ordinaryBytes < destination.Length)
        {
            if (id != PauseEquipmentLabelId.Plasma)
                throw new InvalidDataException($"Pause label {id} cannot supply a {wordCount}-word native overrun.");
            labels[PauseEquipmentLabelId.Varia].CopyTo(destination[ordinaryBytes..]);
        }
        if (disabled) Recolor(destination, DisabledPalette);
    }

    /// <summary>Validates and compiles pause-equipment-labels.json into placement and tile-word edits relative to the native $82:BF32-C01A label artwork.</summary>
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
            expected.Any(id => !document.Labels.ContainsKey(PauseEquipmentLabelDefinitions.Name(id))))
            throw new InvalidDataException("Pause equipment labels require the fourteen ordinary labels and Beam.Hyper exactly once.");

        var compiled = new Dictionary<PauseEquipmentLabelId, CompiledLabel>();
        var occupied = new HashSet<int>();
        foreach (PauseEquipmentLabelId id in expected)
        {
            string key = PauseEquipmentLabelDefinitions.Name(id);
            PauseEquipmentLabel label = document.Labels[key] ??
                throw new InvalidDataException($"Pause equipment label {key} is null.");
            int words = PauseEquipmentLabelDefinitions.IsOrdinaryBeam(id)
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
                    if (PauseEquipmentBaseDefinitions.IsNonInventoryLiveOwnedCell(cell) &&
                        !plasmaWireframeOverlap)
                        throw new InvalidDataException($"Pause equipment label {key} overlaps wireframe or reserve state at cell {cell}.");
                    if (!occupied.Add(cell)) throw new InvalidDataException($"Pause equipment label {key} overlaps another label.");
                }
            }
            compiled.Add(id, CompiledLabel.Load(id, destinationCell * sizeof(ushort),
                PauseTileGrid.Compile(label.Cells, key)));
        }

        byte[] blankBytes = PauseTileGrid.Compile(document.Blank, "Equipment.Blank");
        var blankEdits = new Dictionary<int, ushort>();
        for (int cell = 0; cell < PauseEquipmentLabelDefinitions.EquipmentWords; cell++)
        {
            int word = BinaryPrimitives.ReadUInt16LittleEndian(blankBytes.AsSpan(cell * sizeof(ushort)));
            if (word != 0) blankEdits.Add(cell, (ushort)word);
        }
        return new(compiled, blankEdits, document.DisabledPalette,
            Convert.ToHexString(SHA256.HashData(source)));
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
    /// $82:C01A-C02B supplies nine empty BG cells. Uncollected labels and Hyper's
    /// discarded beam slots clear their selected width; only explicit asset edits
    /// need per-cell data. The source JSON remains the content-identity contract.
    /// </summary>
    private void WriteBlank(Span<byte> destination)
    {
        destination.Clear();
        foreach (KeyValuePair<int, ushort> edit in blankEdits)
            if (edit.Key < destination.Length / sizeof(ushort))
                BinaryPrimitives.WriteUInt16LittleEndian(destination[(edit.Key * sizeof(ushort))..], edit.Value);
    }

    /// <summary>Rejects partial buffers so fixed label destinations remain within the complete 32-by-32 pause tilemap.</summary>
    private static void ValidateTilemap(Span<byte> tilemap)
    {
        if (tilemap.Length != PauseEquipmentLabelDefinitions.TilemapColumns *
            PauseEquipmentLabelDefinitions.TilemapRows * sizeof(ushort))
            throw new ArgumentException("Pause equipment labels require a complete 32x32 tilemap.", nameof(tilemap));
    }

    /// <summary>Changes each tile word's BG palette bits while retaining its character, priority, and flip fields.</summary>
    private static void Recolor(Span<byte> bytes, int palette)
    {
        for (int offset = 0; offset < bytes.Length; offset += sizeof(ushort))
        {
            BinaryPrimitives.WriteUInt16LittleEndian(bytes[offset..],
                new SnesBgTilemapWord(BinaryPrimitives.ReadUInt16LittleEndian(bytes[offset..])).WithPaletteIndex(palette).Raw);
        }
    }

    /// <summary>Stores placement and tile-word differences while resolving unchanged cells from stock label definitions.</summary>
    private sealed class CompiledLabel(PauseEquipmentLabelId id, int? destinationEdit, Dictionary<int, ushort> edits)
    {
        /// <summary>Returns the edited byte offset or the stock selector's equipment-page byte.</summary>
        internal int PageByte => destinationEdit ?? PauseEquipmentLabelDefinitions.StockPageByte(id);
        /// <summary>Uses the native selector's width even when its authored strip has edited cells.</summary>
        internal int WordCount => PauseEquipmentLabelDefinitions.StockWordCount(id);

        /// <summary>Retains only tile words and destination coordinates that differ from the native selector.</summary>
        internal static CompiledLabel Load(PauseEquipmentLabelId id, int destination, ReadOnlySpan<byte> bytes)
        {
            var edits = new Dictionary<int, ushort>();
            for (int cell = 0; cell < bytes.Length / sizeof(ushort); cell++)
            {
                int value = BinaryPrimitives.ReadUInt16LittleEndian(bytes[(cell * sizeof(ushort))..]);
                if (value != PauseEquipmentLabelDefinitions.StockWord(id, cell)) edits.Add(cell, (ushort)value);
            }
            int? changedDestination = destination == PauseEquipmentLabelDefinitions.StockPageByte(id)
                ? null : destination;
            return new(id, changedDestination, edits);
        }

        /// <summary>Materializes edited cells over stock artwork for the requested prefix, including Plasma's extended write.</summary>
        internal void CopyTo(Span<byte> destination)
        {
            for (int cell = 0; cell < destination.Length / sizeof(ushort); cell++)
                BinaryPrimitives.WriteUInt16LittleEndian(destination[(cell * sizeof(ushort))..],
                    edits.ContainsKey(cell) ? edits[cell]
                        : PauseEquipmentLabelDefinitions.StockWord(id, cell));
        }
    }
}
