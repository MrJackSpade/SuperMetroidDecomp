using System.Collections;

namespace SuperMetroid.Core.Assets;

/// <summary>Seven shell-remnant poses: a sixteen-pixel left piece and two small right pieces.</summary>
internal sealed class IntroEggRemnantParts(int frame) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Uses calculated shell parts only for one of the seven remnant frame pointers.</summary>
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        for (int frame = 0; frame < 7; frame++)
            if (IntroDiscoveryActorSpriteDefinitions.EggFramePointer(9 + frame) == pointer)
                return supplied.CalculateIfMatching(new IntroEggRemnantParts(frame));
        return supplied;
    }
    /// <summary>Each remnant pose consists of one large left piece and two small right pieces.</summary>
    public int Count => 3;
    /// <summary>Selects early strip or late patch tiles and native position for one shell piece.</summary>
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
    /// <summary>Enumerates the three shell pieces in source order.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        for (int index = 0; index < Count; index++) yield return this[index];
    }
    /// <summary>Exposes the same shell pieces through non-generic enumeration.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
