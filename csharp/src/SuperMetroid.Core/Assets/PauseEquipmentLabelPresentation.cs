using SuperMetroid.Core.Frontend;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable label artwork and placement for the pause equipment page.</summary>
public sealed class PauseEquipmentLabelPresentation
{
    private readonly Dictionary<string, CompiledLabel> labels;
    private readonly Dictionary<int, ushort> blankEdits;

    private PauseEquipmentLabelPresentation(Dictionary<string, CompiledLabel> labels, Dictionary<int, ushort> blankEdits,
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

    internal bool OwnsLiveCell(int cell)
    {
        foreach ((string key, CompiledLabel label) in labels)
        {
            int words = key == PauseEquipmentLabelDefinitions.PlasmaKey
                ? PauseEquipmentLabelDefinitions.EquipmentWords
                : key == PauseEquipmentLabelDefinitions.HyperKey
                    ? PauseEquipmentLabelDefinitions.BeamWords
                    : label.WordCount;
            int first = label.DestinationByte / sizeof(ushort);
            if ((uint)(cell - first) < words) return true;
        }
        return false;
    }

    /// <summary>Builds every inventory-owned label without consulting cartridge tables.</summary>
    public void ApplyInventory(Span<byte> tilemap, ushort collectedBeams, ushort equippedBeams,
        ushort collectedItems, ushort equippedItems, bool hyperBeam)
    {
        ValidateTilemap(tilemap);
        if (hyperBeam)
        {
            for (int item = 0; item < PauseEquipmentLabelDefinitions.ItemCount(PauseEquipmentCategory.Beams); item++)
            {
                CompiledLabel destination = labels[PauseEquipmentLabelDefinitions.Key(PauseEquipmentCategory.Beams, item)];
                WriteBlank(tilemap.Slice(destination.DestinationByte,
                    PauseEquipmentLabelDefinitions.BeamWords * sizeof(ushort)));
            }
            CompiledLabel hyper = labels[PauseEquipmentLabelDefinitions.HyperKey];
            hyper.CopyTo(tilemap.Slice(hyper.DestinationByte,
                    PauseEquipmentLabelDefinitions.BeamWords * sizeof(ushort)));
        }
        foreach (PauseEquipmentCategory category in PauseEquipmentCategories.InventoryCategories)
        for (int item = 0; item < PauseEquipmentLabelDefinitions.ItemCount(category); item++)
        {
            if (category == PauseEquipmentCategory.Beams && hyperBeam) continue;

            ushort mask = Frontend.PauseEquipmentRules.Mask(category, item);
            ushort collected = category == PauseEquipmentCategory.Beams ? collectedBeams : collectedItems;
            ushort equipped = category == PauseEquipmentCategory.Beams ? equippedBeams : equippedItems;
            string key = PauseEquipmentLabelDefinitions.Key(category, item);
            CompiledLabel label = labels[key];
            Span<byte> destination = tilemap.Slice(label.DestinationByte, label.WordCount * sizeof(ushort));
            if ((collected & mask) == 0)
                WriteBlank(destination);
            else
            {
                label.CopyTo(destination);
                if ((equipped & mask) == 0) Recolor(destination, DisabledPalette);
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
        string key = PauseEquipmentLabelDefinitions.Key(category, item);
        CompiledLabel label = labels[key];
        if (wordCount is < 0 or > PauseEquipmentLabelDefinitions.EquipmentWords)
            throw new ArgumentOutOfRangeException(nameof(wordCount));
        Span<byte> destination = tilemap.Slice(label.DestinationByte, wordCount * sizeof(ushort));
        int ordinaryBytes = Math.Min(destination.Length, label.WordCount * sizeof(ushort));
        label.CopyTo(destination[..ordinaryBytes]);
        if (ordinaryBytes < destination.Length)
        {
            if (key != PauseEquipmentLabelDefinitions.PlasmaKey)
                throw new InvalidDataException($"Pause label {key} cannot supply a {wordCount}-word native overrun.");
            labels[PauseEquipmentLabelDefinitions.VariaKey].CopyTo(destination[ordinaryBytes..]);
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
            document = JsonAssetDocument.Read<PauseEquipmentLabelDocument>(source, MapPresentationFormat.JsonOptions)
                ?? throw new InvalidDataException("Pause equipment label document is null.");
        }
        catch (JsonException error) { throw new InvalidDataException("Invalid pause equipment label JSON.", error); }

        if (document.Version != PauseEquipmentLabelDefinitions.Version || document.Labels is null ||
            document.Blank is null || document.Blank.Length != PauseEquipmentLabelDefinitions.EquipmentWords ||
            (uint)document.DisabledPalette >= 8)
            throw new InvalidDataException("Pause equipment labels require version 1, nine blank cells and a valid disabled palette.");

        IEnumerable<string> expected = PauseEquipmentLabelDefinitions.Labels().Select(label => label.Key)
            .Append(PauseEquipmentLabelDefinitions.HyperKey);
        if (document.Labels.Count != expected.Count() || expected.Any(key => !document.Labels.ContainsKey(key)))
            throw new InvalidDataException("Pause equipment labels require the fourteen ordinary labels and Beam.Hyper exactly once.");

        var compiled = new Dictionary<string, CompiledLabel>(StringComparer.Ordinal);
        var occupied = new HashSet<int>();
        foreach (string key in expected)
        {
            PauseEquipmentLabel label = document.Labels[key] ??
                throw new InvalidDataException($"Pause equipment label {key} is null.");
            int words = key.StartsWith("Beam.", StringComparison.Ordinal) && key != PauseEquipmentLabelDefinitions.HyperKey
                ? PauseEquipmentLabelDefinitions.BeamWords : PauseEquipmentLabelDefinitions.EquipmentWords;
            if (key == PauseEquipmentLabelDefinitions.HyperKey) words = PauseEquipmentLabelDefinitions.EquipmentWords;
            if (label.Cells is null || label.Cells.Length != words ||
                (uint)label.Column >= PauseEquipmentLabelDefinitions.TilemapColumns ||
                (uint)label.Row >= PauseEquipmentLabelDefinitions.TilemapRows ||
                label.Column + (key == PauseEquipmentLabelDefinitions.HyperKey ? PauseEquipmentLabelDefinitions.BeamWords : words) > PauseEquipmentLabelDefinitions.TilemapColumns)
                throw new InvalidDataException($"Pause equipment label {key} has invalid cells or placement.");
            int destinationCell = label.Row * PauseEquipmentLabelDefinitions.TilemapColumns + label.Column;
            if (key != PauseEquipmentLabelDefinitions.HyperKey)
            {
                int footprint = key == PauseEquipmentLabelDefinitions.PlasmaKey
                    ? PauseEquipmentLabelDefinitions.EquipmentWords : words;
                for (int cell = destinationCell; cell < destinationCell + footprint; cell++)
                {
                    bool nativePlasmaWireframeOverlap = key == PauseEquipmentLabelDefinitions.PlasmaKey &&
                        destinationCell == PauseEquipmentLabelDefinitions.NativePlasmaDestinationCell &&
                        cell == PauseEquipmentLabelDefinitions.NativePlasmaWireframeOverlapCell;
                    if (PauseEquipmentBaseDefinitions.IsNonInventoryLiveOwnedCell(cell) &&
                        !nativePlasmaWireframeOverlap)
                        throw new InvalidDataException($"Pause equipment label {key} overlaps wireframe or reserve state at cell {cell}.");
                    if (!occupied.Add(cell)) throw new InvalidDataException($"Pause equipment label {key} overlaps another label.");
                }
            }
            compiled.Add(key, CompiledLabel.Load(key, destinationCell * sizeof(ushort),
                PauseTileGrid.Compile(label.Cells, key)));
        }

        byte[] blankBytes = PauseTileGrid.Compile(document.Blank, "Equipment.Blank");
        var blankEdits = new Dictionary<int, ushort>();
        for (int cell = 0; cell < PauseEquipmentLabelDefinitions.EquipmentWords; cell++)
        {
            ushort word = BinaryPrimitives.ReadUInt16LittleEndian(blankBytes.AsSpan(cell * sizeof(ushort)));
            if (word != 0) blankEdits.Add(cell, word);
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
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
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
        foreach ((int cell, ushort word) in blankEdits)
            if (cell < destination.Length / sizeof(ushort))
                BinaryPrimitives.WriteUInt16LittleEndian(destination[(cell * sizeof(ushort))..], word);
    }

    private static void ValidateTilemap(Span<byte> tilemap)
    {
        if (tilemap.Length != PauseEquipmentLabelDefinitions.TilemapColumns *
            PauseEquipmentLabelDefinitions.TilemapRows * sizeof(ushort))
            throw new ArgumentException("Pause equipment labels require a complete 32x32 tilemap.", nameof(tilemap));
    }

    private static void Recolor(Span<byte> bytes, int palette)
    {
        for (int offset = 0; offset < bytes.Length; offset += sizeof(ushort))
        {
            ushort raw = BinaryPrimitives.ReadUInt16LittleEndian(bytes[offset..]);
            BinaryPrimitives.WriteUInt16LittleEndian(bytes[offset..], new SnesBgTilemapWord(raw).WithPaletteIndex(palette).Raw);
        }
    }

    private sealed class CompiledLabel(string key, int? destinationEdit, Dictionary<int, ushort> edits)
    {
        internal int DestinationByte => destinationEdit ?? PauseEquipmentLabelDefinitions.StockDestinationByte(key);
        internal int WordCount => PauseEquipmentLabelDefinitions.StockWordCount(key);

        internal static CompiledLabel Load(string key, int destination, ReadOnlySpan<byte> bytes)
        {
            var edits = new Dictionary<int, ushort>();
            for (int cell = 0; cell < bytes.Length / sizeof(ushort); cell++)
            {
                ushort value = BinaryPrimitives.ReadUInt16LittleEndian(bytes[(cell * sizeof(ushort))..]);
                if (value != PauseEquipmentLabelDefinitions.StockWord(key, cell)) edits.Add(cell, value);
            }
            int? changedDestination = destination == PauseEquipmentLabelDefinitions.StockDestinationByte(key)
                ? null : destination;
            return new(key, changedDestination, edits);
        }

        internal void CopyTo(Span<byte> destination)
        {
            for (int cell = 0; cell < destination.Length / sizeof(ushort); cell++)
                BinaryPrimitives.WriteUInt16LittleEndian(destination[(cell * sizeof(ushort))..],
                    edits.TryGetValue(cell, out ushort selected) ? selected
                        : PauseEquipmentLabelDefinitions.StockWord(key, cell));
        }
    }
}

/// <summary>Editable pause equipment-page label schema, separating inventory-owned text from the backdrop, wireframe, and reserve controls.</summary>
public sealed record PauseEquipmentLabelDocument
{
    /// <summary>Schema revision; loading requires version one from <see cref="PauseEquipmentLabelDefinitions.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>BG palette selector 0..7 applied to collected but unequipped labels while preserving their character, priority, and flip bits.</summary>
    public required int DisabledPalette { get; init; }
    /// <summary>Nine nonnull cells replacing the native zero-word blank strip at $82:C01A; uncollected labels and discarded Hyper beam slots use the needed prefix.</summary>
    public required PauseBackdropCell[] Blank { get; init; }
    /// <summary>Exactly fourteen case-sensitive ordinary selector-anchor keys plus Beam.Hyper, with no reserve labels; entries specify editable tile artwork and destinations.</summary>
    public required Dictionary<string, PauseEquipmentLabel> Labels { get; init; }
}

/// <summary>One horizontal strip of pause BG tile artwork and its destination within the equipment page's 32x32 tilemap.</summary>
public sealed record PauseEquipmentLabel
{
    /// <summary>Zero-based destination tile column 0..31, not an artwork-atlas column; the displayed label must fit within the tilemap row.</summary>
    public required int Column { get; init; }
    /// <summary>Zero-based destination tile row 0..31 in the equipment page, with each tile occupying eight screen pixels vertically.</summary>
    public required int Row { get; init; }
    /// <summary>Left-to-right nonnull artwork cells: five for ordinary beams and nine for equipment, boots, and Beam.Hyper; Hyper displays only its first five cells.</summary>
    public required PauseBackdropCell[] Cells { get; init; }
}
