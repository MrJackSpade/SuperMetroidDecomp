using SuperMetroid.Core.Assets;

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
    private readonly Dictionary<InWorldCollectibleKind, RoomPlmDynamicCollectibleGraphic>? customGraphics;

    private RoomPlmDynamicCollectibleArtCatalog() { }

    public RoomPlmDynamicCollectibleArtCatalog(
        IEnumerable<RoomPlmDynamicCollectibleArtEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var selected = new Dictionary<InWorldCollectibleKind, RoomPlmDynamicCollectibleGraphic>();
        var seen = new bool[RoomPlmDynamicCollectibleGraphicsDefinitions.GraphicCount];
        foreach (RoomPlmDynamicCollectibleArtEntry entry in entries)
        {
            if (entry is null)
                throw new InvalidDataException("Permanent-item artwork contains a null entry.");
            int index = (int)entry.Kind -
                RoomPlmDynamicCollectibleGraphicsDefinitions.FirstKind;
            if ((uint)index >= RoomPlmDynamicCollectibleGraphicsDefinitions.GraphicCount || seen[index])
                throw new InvalidDataException(
                    $"Permanent-item artwork has an unknown or duplicate kind {entry.Kind}.");
            if (entry.Tiles is not { Length: 0x100 } ||
                entry.PaletteOffsets is not { Length: 8 } ||
                entry.PaletteOffsets.Any(offset => offset > 7))
                throw new InvalidDataException(
                    $"Permanent-item artwork for {entry.Kind} has invalid tile or palette data.");
            RoomPlmDynamicCollectibleGraphic stock =
                RoomPlmDynamicCollectibleGraphicsDefinitions.Get(entry.Kind);
            if (!entry.Tiles.AsSpan().SequenceEqual(stock.Tiles.Span) ||
                !entry.PaletteOffsets.AsSpan().SequenceEqual(stock.PaletteOffsets.Span))
                selected.Add(entry.Kind, new RoomPlmDynamicCollectibleGraphic(
                    entry.Kind, stock.GraphicsPointer,
                    entry.PaletteOffsets.ToArray(), entry.Tiles.ToArray()));
            seen[index] = true;
        }
        if (seen.Any(present => !present))
            throw new InvalidDataException(
                "Permanent-item artwork is missing one or more item kinds.");
        if (selected.Count != 0) customGraphics = selected;
    }

    /// <summary>Canonical identity of the selected item pixels and palette selectors.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(
        nameof(RoomPlmDynamicCollectibleArtCatalog), content =>
        {
            for (int kind = RoomPlmDynamicCollectibleGraphicsDefinitions.FirstKind;
                 kind < RoomPlmHeaders.PermanentCollectibleKindCount; kind++)
            {
                RoomPlmDynamicCollectibleGraphic graphic = Resolve((InWorldCollectibleKind)kind);
                content.Append("kind", (int)graphic.Kind);
                content.Append("characters", graphic.Tiles.Span);
                content.Append("palette selectors", graphic.PaletteOffsets.Span);
            }
        });

    /// <summary>Resolve stock directly; retain only customized item artwork.</summary>
    public static RoomPlmDynamicCollectibleArtCatalog Stock() => new();

    internal RoomPlmDynamicCollectibleGraphic Resolve(InWorldCollectibleKind kind)
    {
        int index = (int)kind -
            RoomPlmDynamicCollectibleGraphicsDefinitions.FirstKind;
        if ((uint)index >= RoomPlmDynamicCollectibleGraphicsDefinitions.GraphicCount)
            throw new InvalidDataException(
                $"Permanent-item kind {kind} has no dynamic artwork.");
        return customGraphics is not null && customGraphics.TryGetValue(kind, out var graphic)
            ? graphic : RoomPlmDynamicCollectibleGraphicsDefinitions.Get(kind);
    }
}
