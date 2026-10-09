using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>$82:CFE0-D00A: a centered sixteen-pixel Baby and four reflected specimen-container caps.
/// Selected glyph identities and cap arrangement specify the exact pictured Baby/container. Only that display design is retained; geometry derives, while pixels, palettes and timing remain separate.</summary>
/// <param name="name">Game-over frame identity selecting Baby artwork or the specimen-container shell.</param>
internal sealed class GameOverSpriteParts(string name) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Replaces matching supplied Baby or shell compositions with the catalog's canonical selected glyph arrangement.</summary>
    /// <param name="name">Frame identity used to determine whether this display design applies.</param>
    /// <param name="supplied">Existing composition whose part count is checked before replacement.</param>
    /// <returns>The canonical composition for a matching supported frame, or the supplied composition unchanged.</returns>
    internal static SpriteComposition CalculateIfMatching(string name, SpriteComposition supplied)
    {
        bool shell = name == GameOverPresentationDefinitions.EggFrame;
        if (!shell && name is not ("Baby.Closed" or "Baby.Middle" or "Baby.Open")) return supplied;
        if (supplied.PartCount != (shell ? 4 : 1)) return supplied;
        return supplied.CalculateIfMatching(new GameOverSpriteParts(name));
    }

    /// <summary>$82:CFF6: selected closed Baby sixteen-pixel tile90; the middle pose occupies the immediately adjacent two-tile-wide atlas region.</summary>
    private const int ClosedBabyTile = 0x90;
    /// <summary>$82:D004: selected open Baby sixteen-pixel tile9B.</summary>
    private const int OpenBabyTile = 0x9b;
    /// <summary>$82:CFE0: selected eight-pixel container-cap tile9A, reflected to form all four caps.</summary>
    private const int ContainerCapTile = 0x9a;
    /// <summary>Indicates whether the selected frame is the four-cap specimen-container shell.</summary>
    private bool Shell => name == GameOverPresentationDefinitions.EggFrame;

    /// <summary>Tile index selected for Baby's pose or the repeated container-cap glyph.</summary>
    private int Tile => name switch
    {
        "Baby.Closed" => ClosedBabyTile,
        "Baby.Middle" => ClosedBabyTile + 16 / 8,
        "Baby.Open" => OpenBabyTile,
        _ => ContainerCapTile,
    };
    /// <summary>Number of OBJ parts: one centered Baby glyph or four reflected shell caps.</summary>
    public int Count => Shell ? 4 : 1;

    /// <summary>Gets the selected sprite part, deriving its offset and reflection from the shell layout.</summary>
    /// <param name="index">Zero-based part index in the Baby or shell composition.</param>
    /// <returns>The compiled part at the requested index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside this composition's part count.</exception>
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            if (!Shell)
                return new(SnesSpritemapXWord.Create(-8, true), unchecked((byte)-8),
                    SnesObjAttributeWord.Create(Tile, 0, 3, SnesTileFlipFlags.None), true);
            // The eight-pixel caps lie outside the centered sixteen-pixel Baby region.
            bool left = (index & 1) != 0;
            bool upper = index >= 2;
            var flips = (left ? SnesTileFlipFlags.None : SnesTileFlipFlags.Horizontal) |
                (upper ? SnesTileFlipFlags.None : SnesTileFlipFlags.Vertical);
            return new(SnesSpritemapXWord.Create(left ? -8 : 0, false), unchecked((byte)(upper ? -16 : 8)),
                SnesObjAttributeWord.Create(Tile, 0, 3, flips), true);
        }
    }
    /// <summary>Enumerates the composition's parts in the order consumed by the sprite renderer.</summary>
    /// <returns>An enumerator over each compiled sprite part.</returns>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
