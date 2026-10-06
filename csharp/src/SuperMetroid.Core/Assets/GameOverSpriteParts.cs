using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>$82:CFE0-D00A: a centered sixteen-pixel Baby and four reflected shell caps.
/// The one selected tile per composition remains an independent supplied artwork input.</summary>
internal sealed class GameOverSpriteParts(bool shell, int tile) : IReadOnlyList<CompiledSpritePart>
{
    internal static SpriteComposition CalculateIfMatching(string name, SpriteComposition supplied)
    {
        bool shell = name == GameOverPresentationDefinitions.EggFrame;
        if (!shell && name is not ("Baby.Closed" or "Baby.Middle" or "Baby.Open")) return supplied;
        if (supplied.PartCount != (shell ? 4 : 1)) return supplied;
        return supplied.CalculateIfMatching(new GameOverSpriteParts(shell, supplied.Part(0).Attributes.TileNumber));
    }

    public int Count => shell ? 4 : 1;
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            if (!shell)
                return new(SnesSpritemapXWord.Create(-8, true), unchecked((byte)-8),
                    SnesObjAttributeWord.Create(tile, 0, 3, SnesTileFlipFlags.None), true);
            // The eight-pixel caps lie outside the centered sixteen-pixel Baby region.
            bool left = (index & 1) != 0;
            bool upper = index >= 2;
            var flips = (left ? SnesTileFlipFlags.None : SnesTileFlipFlags.Horizontal) |
                (upper ? SnesTileFlipFlags.None : SnesTileFlipFlags.Vertical);
            return new(SnesSpritemapXWord.Create(left ? -8 : 0, false), unchecked((byte)(upper ? -16 : 8)),
                SnesObjAttributeWord.Create(tile, 0, 3, flips), true);
        }
    }
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
