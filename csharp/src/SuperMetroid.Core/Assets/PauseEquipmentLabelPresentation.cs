using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable label artwork and placement for the pause equipment page.</summary>
public sealed class PauseEquipmentLabelPresentation
{
    /// <summary>Compiled label artwork and placements indexed by the schema's case-sensitive label keys.</summary>
    private readonly Dictionary<string, CompiledLabel> labels;
    /// <summary>Nonzero blank-strip words that override the native all-zero tilemap cells.</summary>
    private readonly Dictionary<int, ushort> blankEdits;

    /// <summary>Creates the immutable runtime view of validated labels and their selected blank-cell words.</summary>
    /// <param name="labels">Compiled artwork and destinations keyed by label identifier.</param>
    /// <param name="blankEdits">Cell values that differ from the native zero-filled blank strip.</param>
    /// <param name="disabledPalette">Palette selector used for collected but unequipped items.</param>
    /// <param name="contentIdentity">Hash of the exact source JSON used to produce this catalog.</param>
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

    /// <summary>Reports whether an equipment label occupies a tilemap cell that must remain synchronized with inventory state.</summary>
    /// <param name="cell">Linear cell index in the pause equipment tilemap.</param>
    /// <returns><see langword="true"/> when the cell belongs to a live label footprint.</returns>
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
            for (int item = 0; item < PauseEquipmentLabelDefinitions.ItemCount(1); item++)
            {
                CompiledLabel destination = labels[PauseEquipmentLabelDefinitions.Key(1, item)];
                WriteBlank(tilemap.Slice(destination.DestinationByte,
                    PauseEquipmentLabelDefinitions.BeamWords * sizeof(ushort)));
            }
            CompiledLabel hyper = labels[PauseEquipmentLabelDefinitions.HyperKey];
            hyper.CopyTo(tilemap.Slice(hyper.DestinationByte,
                    PauseEquipmentLabelDefinitions.BeamWords * sizeof(ushort)));
        }
        for (int category = 1; category <= 3; category++)
        for (int item = 0; item < PauseEquipmentLabelDefinitions.ItemCount(category); item++)
        {
            if (category == 1 && hyperBeam) continue;

            ushort mask = Frontend.PauseEquipmentRules.Mask(category, item);
            ushort collected = category == 1 ? collectedBeams : collectedItems;
            ushort equipped = category == 1 ? equippedBeams : equippedItems;
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
    public void ApplyLabel(Span<byte> tilemap, int category, int item, int wordCount, bool disabled)
    {
        ValidateTilemap(tilemap);
        string key = PauseEquipmentLabelDefinitions.Key(category, item);
        CompiledLabel label = labels[key];
        if (wordCount < 0 || wordCount > PauseEquipmentLabelDefinitions.EquipmentWords)
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

    /// <summary>Requires a complete 32-by-32 tilemap before label operations write into it.</summary>
    /// <param name="tilemap">Tilemap buffer to validate.</param>
    /// <exception cref="ArgumentException">The buffer does not contain exactly one complete tilemap.</exception>
    private static void ValidateTilemap(Span<byte> tilemap)
    {
        if (tilemap.Length != PauseEquipmentLabelDefinitions.TilemapColumns *
            PauseEquipmentLabelDefinitions.TilemapRows * sizeof(ushort))
            throw new ArgumentException("Pause equipment labels require a complete 32x32 tilemap.", nameof(tilemap));
    }

    /// <summary>Replaces each BG tile word's palette selector while retaining its other attribute bits.</summary>
    /// <param name="bytes">Little-endian tile words to recolor.</param>
    /// <param name="palette">Palette selector written to each tile word.</param>
    private static void Recolor(Span<byte> bytes, int palette)
    {
        for (int offset = 0; offset < bytes.Length; offset += sizeof(ushort))
        {
            ushort raw = BinaryPrimitives.ReadUInt16LittleEndian(bytes[offset..]);
            BinaryPrimitives.WriteUInt16LittleEndian(bytes[offset..], new SnesBgTilemapWord(raw).WithPaletteIndex(palette).Raw);
        }
    }

    /// <summary>Stores sparse artwork edits and an optional placement override for one native label.</summary>
    /// <param name="key">Label key used to obtain stock dimensions and fallback tile words.</param>
    /// <param name="destinationEdit">Edited byte offset, or <see langword="null"/> to use the stock placement.</param>
    /// <param name="edits">Tile words that differ from the native artwork for this label.</param>
    private sealed class CompiledLabel(string key, int? destinationEdit, Dictionary<int, ushort> edits)
    {
        /// <summary>Gets the selected byte offset in the tilemap, falling back to the native label placement.</summary>
        internal int DestinationByte => destinationEdit ?? PauseEquipmentLabelDefinitions.StockDestinationByte(key);

        /// <summary>Gets the number of tile words occupied by this label in the pause layout.</summary>
        internal int WordCount => PauseEquipmentLabelDefinitions.StockWordCount(key);

        /// <summary>Compiles a label by retaining only tile words that differ from its native artwork.</summary>
        /// <param name="key">Label key that identifies the native fallback artwork and placement.</param>
        /// <param name="destination">Selected byte offset for the label in the tilemap.</param>
        /// <param name="bytes">Little-endian tile words for the supplied artwork.</param>
        /// <returns>A sparse compiled label whose unchanged values use the native definitions.</returns>
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

        /// <summary>Writes selected tile words into the destination, filling unchanged positions from native artwork.</summary>
        /// <param name="destination">Tilemap slice receiving the label's words.</param>
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
