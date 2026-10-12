using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Seven shell-remnant poses: a sixteen-pixel left piece and two small right pieces.</summary>
internal sealed class IntroEggRemnantParts(int frame) : IReadOnlyList<CompiledSpritePart>
{
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        for (int frame = 0; frame < 7; frame++)
            if (IntroDiscoveryActorSpriteDefinitions.EggFramePointer(9 + frame) == pointer)
                return supplied.CalculateIfMatching(new IntroEggRemnantParts(frame));
        return supplied;
    }
    public int Count => 3;
    public CompiledSpritePart this[int index]
    {
        get
        {
            if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            int tile;
            if (frame < 3)
            {
                int topRight = IntroEggRemnantAtlas.EarlyStrip + 2 * frame;
                tile = index == 2 ? topRight + 16 : index == 1 ? topRight
                    : frame == 1 ? IntroEggRemnantAtlas.EarlyStrip + 1 : topRight + 1;
            }
            else
            {
                int patch = IntroEggRemnantAtlas.LatePatches + 3 * (frame - 3);
                tile = patch + (index == 2 ? 0 : index == 1 ? 2 : 18);
            }
            return new(SnesSpritemapXWord.Create(index == 2 ? -12 : 4, index == 2),
                unchecked((byte)(index == 0 ? 4 : -4)),
                SnesObjAttributeWord.Create(tile, 0, 3, 0), true);
        }
    }
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

internal static class IntroEggRemnantAtlas
{
    /// <summary>Tile$107, first early top-right remnant drawing;left pieces occupy the next atlas row.
    /// The second pose reuses the first pose lower-right tile$108.</summary>
    internal const int EarlyStrip = 0x107;
    /// <summary>Tile$130, first of four adjacent three-by-two late-remnant atlas patches.</summary>
    internal const int LatePatches = 0x130;
}
