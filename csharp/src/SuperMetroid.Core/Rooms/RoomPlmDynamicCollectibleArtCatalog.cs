namespace SuperMetroid.Core.Rooms;

/// <summary>Editable pixels and tile-palette selectors for one permanent-item kind.</summary>
public sealed record RoomPlmDynamicCollectibleArtEntry(
    InWorldCollectibleKind Kind, byte[] Tiles, byte[] PaletteOffsets);

/// <summary>
/// Immutable, complete set of the 17 permanent-item uploads used by native
/// instruction <c>$84:8764</c>. Changing this art never changes item identity,
/// pickup effects, block collision, or native PLM timing.
/// </summary>
public sealed class RoomPlmDynamicCollectibleArtCatalog
{
    private readonly RoomPlmDynamicCollectibleGraphic[] graphics;

    public RoomPlmDynamicCollectibleArtCatalog(
        IEnumerable<RoomPlmDynamicCollectibleArtEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        graphics = new RoomPlmDynamicCollectibleGraphic[
            RoomPlmDynamicCollectibleGraphicsDefinitions.GraphicCount];
        var seen = new bool[graphics.Length];
        foreach (RoomPlmDynamicCollectibleArtEntry entry in entries)
        {
            if (entry is null)
                throw new InvalidDataException("Permanent-item artwork contains a null entry.");
            int index = (int)entry.Kind -
                RoomPlmDynamicCollectibleGraphicsDefinitions.FirstKind;
            if ((uint)index >= graphics.Length || seen[index])
                throw new InvalidDataException(
                    $"Permanent-item artwork has an unknown or duplicate kind {entry.Kind}.");
            if (entry.Tiles is not { Length: 0x100 } ||
                entry.PaletteOffsets is not { Length: 8 } ||
                entry.PaletteOffsets.Any(offset => offset > 7))
                throw new InvalidDataException(
                    $"Permanent-item artwork for {entry.Kind} has invalid tile or palette data.");
            RoomPlmDynamicCollectibleGraphic stock =
                RoomPlmDynamicCollectibleGraphicsDefinitions.Get(entry.Kind);
            graphics[index] = new RoomPlmDynamicCollectibleGraphic(
                entry.Kind, stock.GraphicsPointer,
                entry.PaletteOffsets.ToArray(), entry.Tiles.ToArray());
            seen[index] = true;
        }
        if (seen.Any(present => !present))
            throw new InvalidDataException(
                "Permanent-item artwork is missing one or more item kinds.");
    }

    /// <summary>Copies the compiled cartridge appearance for installations without overrides.</summary>
    public static RoomPlmDynamicCollectibleArtCatalog Stock() => new(
        RoomPlmDynamicCollectibleGraphicsDefinitions.All.ToArray().Select(graphic =>
            new RoomPlmDynamicCollectibleArtEntry(graphic.Kind,
                graphic.Tiles.ToArray(), graphic.PaletteOffsets.ToArray())));

    internal RoomPlmDynamicCollectibleGraphic Resolve(InWorldCollectibleKind kind)
    {
        int index = (int)kind -
            RoomPlmDynamicCollectibleGraphicsDefinitions.FirstKind;
        if ((uint)index >= graphics.Length)
            throw new InvalidDataException(
                $"Permanent-item kind {kind} has no dynamic artwork.");
        return graphics[index];
    }
}
