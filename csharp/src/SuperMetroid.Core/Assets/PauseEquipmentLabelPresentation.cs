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
    private readonly byte[] blank;

    private PauseEquipmentLabelPresentation(Dictionary<string, CompiledLabel> labels, byte[] blank,
        int disabledPalette, string contentIdentity)
    {
        this.labels = labels;
        this.blank = blank;
        DisabledPalette = disabledPalette;
        ContentIdentity = contentIdentity;
    }

    public int DisabledPalette { get; }
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
            for (int item = 0; item < PauseEquipmentLabelDefinitions.Keys[1].Length; item++)
            {
                CompiledLabel destination = labels[PauseEquipmentLabelDefinitions.Key(1, item)];
                blank.AsSpan(0, PauseEquipmentLabelDefinitions.BeamWords * sizeof(ushort))
                    .CopyTo(tilemap.Slice(destination.DestinationByte,
                        PauseEquipmentLabelDefinitions.BeamWords * sizeof(ushort)));
            }
            CompiledLabel hyper = labels[PauseEquipmentLabelDefinitions.HyperKey];
            hyper.Bytes.AsSpan(0, PauseEquipmentLabelDefinitions.BeamWords * sizeof(ushort))
                .CopyTo(tilemap.Slice(hyper.DestinationByte,
                    PauseEquipmentLabelDefinitions.BeamWords * sizeof(ushort)));
        }
        for (int category = 1; category <= 3; category++)
        for (int item = 0; item < PauseEquipmentLabelDefinitions.Keys[category].Length; item++)
        {
            if (category == 1 && hyperBeam) continue;

            ushort mask = Frontend.PauseEquipmentRules.Mask(category, item);
            ushort collected = category == 1 ? collectedBeams : collectedItems;
            ushort equipped = category == 1 ? equippedBeams : equippedItems;
            string key = PauseEquipmentLabelDefinitions.Key(category, item);
            CompiledLabel label = labels[key];
            Span<byte> destination = tilemap.Slice(label.DestinationByte, label.WordCount * sizeof(ushort));
            if ((collected & mask) == 0)
                blank.AsSpan(0, destination.Length).CopyTo(destination);
            else
            {
                label.Bytes.CopyTo(destination);
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
        int ordinaryBytes = Math.Min(destination.Length, label.Bytes.Length);
        label.Bytes.AsSpan(0, ordinaryBytes).CopyTo(destination);
        if (ordinaryBytes < destination.Length)
        {
            if (key != PauseEquipmentLabelDefinitions.PlasmaKey)
                throw new InvalidDataException($"Pause label {key} cannot supply a {wordCount}-word native overrun.");
            labels[PauseEquipmentLabelDefinitions.VariaKey].Bytes.AsSpan(0, destination.Length - ordinaryBytes)
                .CopyTo(destination[ordinaryBytes..]);
        }
        if (disabled) Recolor(destination, DisabledPalette);
    }

    public static PauseEquipmentLabelPresentation Load(Stream json)
    {
        byte[] source;
        using (var buffer = new MemoryStream()) { json.CopyTo(buffer); source = buffer.ToArray(); }
        PauseEquipmentLabelDocument document;
        try
        {
            document = JsonSerializer.Deserialize<PauseEquipmentLabelDocument>(source, MapPresentationFormat.JsonOptions)
                ?? throw new InvalidDataException("Pause equipment label document is null.");
        }
        catch (JsonException error) { throw new InvalidDataException("Invalid pause equipment label JSON.", error); }

        if (document.Version != PauseEquipmentLabelDefinitions.Version || document.Labels is null ||
            document.Blank is null || document.Blank.Length != PauseEquipmentLabelDefinitions.EquipmentWords ||
            (uint)document.DisabledPalette >= 8)
            throw new InvalidDataException("Pause equipment labels require version 1, nine blank cells and a valid disabled palette.");

        string[] expected = [.. PauseEquipmentLabelDefinitions.Keys.Skip(1).SelectMany(value => value),
            PauseEquipmentLabelDefinitions.HyperKey];
        if (document.Labels.Count != expected.Length || expected.Any(key => !document.Labels.ContainsKey(key)))
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
            compiled.Add(key, new(destinationCell * sizeof(ushort), words,
                PauseTileGrid.Compile(label.Cells, key)));
        }

        return new(compiled, PauseTileGrid.Compile(document.Blank, "Equipment.Blank"), document.DisabledPalette,
            Convert.ToHexString(SHA256.HashData(source)));
    }

    public static void Write(Stream output, PauseEquipmentLabelDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        output.Write(bytes);
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

    private sealed record CompiledLabel(int DestinationByte, int WordCount, byte[] Bytes);
}

public sealed record PauseEquipmentLabelDocument
{
    public required int Version { get; init; }
    public required int DisabledPalette { get; init; }
    public required PauseBackdropCell[] Blank { get; init; }
    public required Dictionary<string, PauseEquipmentLabel> Labels { get; init; }
}

public sealed record PauseEquipmentLabel
{
    public required int Column { get; init; }
    public required int Row { get; init; }
    public required PauseBackdropCell[] Cells { get; init; }
}
