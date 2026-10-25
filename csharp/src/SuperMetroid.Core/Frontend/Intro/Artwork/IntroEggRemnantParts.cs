using System.Collections;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Seven shell-remnant poses: a sixteen-pixel left piece and two small right pieces.</summary>
internal sealed class IntroEggRemnantParts(int frame) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Uses calculated shell parts only for one of the seven remnant frame pointers.</summary>
    internal static SpriteComposition CalculateIfMatching(CinematicSpritePose shown, SpriteComposition supplied)
    {
        for (int frame = 0; frame < 7; frame++)
            if (IntroDiscoveryActorSpriteDefinitions.EggFramePose(9 + frame) == shown)
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
                int topRight = IntroEggRemnantCellDefinitions.EarlyStrip + 2 * frame;
                tile = index == 2 ? topRight + 16 : index == 1 ? topRight
                    : frame == 1 ? IntroEggRemnantCellDefinitions.EarlyStrip + 1 : topRight + 1;
            }
            else
            {
                int patch = IntroEggRemnantCellDefinitions.LatePatches + 3 * (frame - 3);
                tile = patch + (index == 2 ? 0 : index == 1 ? 2 : 18);
            }
            return new(SpritePart.Create(index == 2 ? -12 : 4, index == 0 ? 4 : -4, index == 2,
                tile, 0, 3, 0), true);
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
