using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;

/// <summary>
/// Shared single-cell star rendering for8C:975E/979C/97BC/97D2. Ordered grid
/// positions and glyph choices are retained decorative star composition under #1165.
/// Native 8B:C8B9-C991 translates whole sheets; individual stars have no functional roles.
/// small size, zero priority and no flips are common to all29 original parts.
/// </summary>
internal sealed class ZebesStarGridParts : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Grid positions and tile identities recovered from the supplied decorative star composition.</summary>
    private readonly ZebesStarPlacement[] placements;
    /// <summary>Stores each star as a tile position on the eight-pixel grid.</summary>
    private ZebesStarGridParts(SpriteComposition supplied)
    {
        placements = new ZebesStarPlacement[supplied.PartCount];
        for (int index = 0; index < placements.Length; index++)
        {
            CompiledSpritePart part = supplied.Part(index);
            placements[index] = new(new Offset16<TilemapCells>((short)(part.Part.Offset.X / 8), (short)(part.Part.Offset.Y / 8)),
                part.Part.Character);
        }
    }
    /// <summary>Uses a calculated star grid only when every reconstructed OAM field matches the edited composition.</summary>
    internal static SpriteComposition CalculateIfMatching(CeresDestructionBackdrop? backdrop, SpriteComposition supplied)
    {
        if (backdrop is not (CeresDestructionBackdrop.UpperLeftStars or
            CeresDestructionBackdrop.UpperRightStars or
            CeresDestructionBackdrop.LowerLeftStars or
            CeresDestructionBackdrop.LowerRightStars)) return supplied;
        // Full-field matching preserves off-grid and independently edited appearance.
        return supplied.CalculateIfMatching(new ZebesStarGridParts(supplied));
    }
    /// <summary>Number of decorative star cells in the reconstructed composition.</summary>
    public int Count => placements.Length;
    /// <summary>Reconstructs one star's OAM part from its grid position and tile identity.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            var point = placements[index];
            return new(SpritePart.Create(Offset16<ScreenPixels>.Of(point.Cell.X * 8, point.Cell.Y * 8), false,
                point.Tile, 0, 0), true);
        }
    }
    /// <summary>Enumerates the star parts in their original drawing order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes the same ordered star parts through the non-generic collection interface.</summary>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
