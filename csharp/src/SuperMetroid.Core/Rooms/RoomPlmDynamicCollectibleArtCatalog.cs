using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms;

/// <summary>Editable pixels and tile-palette selectors for one permanent-item kind.</summary>
/// <param name="Kind">Dynamically uploaded permanent collectible kind with native header-table index 4..20; the four preceding energy/ammunition pickups use separate fixed artwork.</param>
/// <param name="Tiles">Caller-owned $0100-byte array of eight consecutive native four-bit planar characters, representing the item's two visual frames.</param>
/// <param name="PaletteOffsets">Caller-owned eight-byte array of BG palette selectors 0..7, one per uploaded character; these are not palette colors or CGRAM byte offsets.</param>
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

    /// <summary>Validates one artwork entry for each of the seventeen dynamic permanent-item kinds and copies edited characters/selectors into owned storage, sharing compiled stock definitions when the supplied content matches.</summary>
    /// <param name="entries">Complete, duplicate-free item set; each entry supplies exactly $0100 character bytes and eight three-bit palette selectors.</param>
    /// <exception cref="ArgumentNullException"><paramref name="entries"/> is null.</exception>
    /// <exception cref="InvalidDataException">An entry or its arrays are null, a kind is unknown/duplicated/missing, or a tile/selector array has an invalid extent or selector value.</exception>
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
