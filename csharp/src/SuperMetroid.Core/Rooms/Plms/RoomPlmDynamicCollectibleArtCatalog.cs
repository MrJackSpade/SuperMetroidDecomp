using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Immutable, complete set of the 17 permanent-item uploads used by native
/// instruction <c>$84:8764</c>. Changing this art never changes item identity,
/// pickup effects, block collision, or native PLM timing.
/// </summary>
public sealed class RoomPlmDynamicCollectibleArtCatalog
{
    /// <summary>Copied overrides for item kinds whose characters or palette selectors differ from compiled stock artwork.</summary>
    private readonly Dictionary<InWorldCollectibleKind, RoomPlmDynamicCollectibleGraphic>? customGraphics;

    /// <summary>Validates one artwork entry for each of the seventeen dynamic permanent-item kinds and copies edited characters/selectors into owned storage, sharing compiled stock definitions when the supplied content matches.</summary>
    /// <param name="entries">Complete, duplicate-free item set; each entry supplies exactly $0100 character bytes and eight three-bit palette selectors.</param>
    /// <exception cref="ArgumentNullException"><paramref name="entries"/> is null.</exception>
    /// <exception cref="InvalidDataException">An entry or its arrays are null, a kind is unknown/duplicated/missing, or a tile/selector array has an invalid extent or selector value.</exception>
    public RoomPlmDynamicCollectibleArtCatalog(
        IEnumerable<RoomPlmDynamicCollectibleArtEntry> entries)
    {
        Ensure.NotNull(entries);
        var selected = new Dictionary<InWorldCollectibleKind, RoomPlmDynamicCollectibleGraphic>();
        var seen = new bool[RoomPlmDynamicCollectibleGraphicsDefinitions.GraphicCount];
        foreach (RoomPlmDynamicCollectibleArtEntry entry in entries)
        {
            if (entry is null)
                throw new InvalidDataException("Permanent-item artwork contains a null entry.");
            int index = RoomPlmDynamicCollectibleGraphicsDefinitions.KindIndex(entry.Kind);
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
            if (!entry.Tiles.SequenceEqual(stock.Tiles.Span) ||
                !entry.PaletteOffsets.SequenceEqual(stock.PaletteOffsets.Span))
                selected.Add(entry.Kind, new RoomPlmDynamicCollectibleGraphic(
                    entry.Kind, entry.PaletteOffsets.ToArray(), entry.Tiles.ToArray()));
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
            foreach (InWorldCollectibleKind kind in RoomPlmDynamicCollectibleGraphicsDefinitions.Kinds)
            {
                RoomPlmDynamicCollectibleGraphic graphic = Resolve(kind);
                content.AppendName("kind", graphic.Kind.ToString());
                content.Append("characters", graphic.Tiles.Span);
                content.Append("palette selectors", graphic.PaletteOffsets.Span);
            }
        });

    /// <summary>Uses the selected art for a permanent item, falling back to compiled stock data for unchanged kinds.</summary>
    internal RoomPlmDynamicCollectibleGraphic Resolve(InWorldCollectibleKind kind)
    {
        if (RoomPlmDynamicCollectibleGraphicsDefinitions.KindIndex(kind) < 0)
            throw new InvalidDataException(
                $"Permanent-item kind {kind} has no dynamic artwork.");
        return customGraphics is not null && customGraphics.TryGetValue(kind, out var graphic)
            ? graphic : RoomPlmDynamicCollectibleGraphicsDefinitions.Get(kind);
    }
}
