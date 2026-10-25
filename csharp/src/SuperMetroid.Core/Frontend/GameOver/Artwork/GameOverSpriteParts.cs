using System.Collections;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.GameOver.Artwork;

/// <summary>$82:CFE0-D00A: a centered sixteen-pixel Baby and four reflected specimen-container caps.
/// Selected glyph identities and cap arrangement specify the exact pictured Baby/container. Only that display design is retained; geometry derives, while pixels, palettes and timing remain separate.</summary>
internal sealed class GameOverSpriteParts : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>The Baby frame drawn, or null for the egg shell.</summary>
    private readonly GameOverBabyFrame? baby;

    /// <summary>The native parts of <paramref name="baby"/>'s frame, or of the egg shell when null.</summary>
    private GameOverSpriteParts(GameOverBabyFrame? baby) => this.baby = baby;

    /// <summary>Recognises the native Baby frame <paramref name="frame"/> in <paramref name="supplied"/>.</summary>
    internal static SpriteComposition CalculateBabyIfMatching(GameOverBabyFrame frame, SpriteComposition supplied) =>
        supplied.PartCount == 1 ? supplied.CalculateIfMatching(new GameOverSpriteParts(frame)) : supplied;

    /// <summary>Recognises the native egg shell in <paramref name="supplied"/>.</summary>
    internal static SpriteComposition CalculateEggIfMatching(SpriteComposition supplied) =>
        supplied.PartCount == 4 ? supplied.CalculateIfMatching(new GameOverSpriteParts(null)) : supplied;


    /// <summary>$82:CFF6: selected closed Baby sixteen-pixel tile90; the middle pose occupies the immediately adjacent two-tile-wide atlas region.</summary>
    private const int ClosedBabyTile = GameOverPresentationDefinitions.ClosedBabyCell;
    /// <summary>$82:D004: selected open Baby sixteen-pixel tile9B.</summary>
    private const int OpenBabyTile = GameOverPresentationDefinitions.OpenBabyCell;
    /// <summary>$82:CFE0: selected eight-pixel container-cap tile9A, reflected to form all four caps.</summary>
    private const int ContainerCapTile = GameOverPresentationDefinitions.ContainerCapCell;
    /// <summary>Selects the four-cap container arrangement instead of a single Baby pose.</summary>
    private bool Shell => baby is null;
    /// <summary>Selects the authored tile identity for the requested Baby pose or shared cap.</summary>
    private int Tile => baby switch
    {
        GameOverBabyFrame.Closed => ClosedBabyTile,
        GameOverBabyFrame.Middle => ClosedBabyTile + 16 / 8,
        GameOverBabyFrame.Open => OpenBabyTile,
        null => ContainerCapTile,
        _ => throw new InvalidOperationException($"Undefined Baby frame {baby}."),
    };
    /// <summary>The container has four reflected caps; each Baby pose has one large part.</summary>
    public int Count => Shell ? 4 : 1;
    /// <summary>Places one reviewed Baby or reflected cap part at its native offset.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            if (!Shell)
                return new(SnesSpritemapXWord.Create(-8, true), SnesSpritemapYByte.Create(-8),
                    SnesObjAttributeWord.Create(Tile, 0, 3, TileFlip.None), true);
            // The eight-pixel caps lie outside the centered sixteen-pixel Baby region.
            bool left = (index & 1) != 0;
            bool upper = index >= 2;
            var flips = (left ? TileFlip.None : TileFlip.Horizontal) |
                (upper ? TileFlip.None : TileFlip.Vertical);
            return new(SnesSpritemapXWord.Create(left ? -8 : 0, false), SnesSpritemapYByte.Create(upper ? -16 : 8),
                SnesObjAttributeWord.Create(Tile, 0, 3, flips), true);
        }
    }
    /// <summary>Yields the composition's parts in their authored OAM order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes the same authored OAM parts to non-generic collection consumers.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
