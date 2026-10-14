using System.Collections;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Three centered sixteen-pixel baby poses,packed at the end of the intro egg atlas.</summary>
internal sealed class IntroConfusedBabyParts(int frame) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Replaces matching intro Baby poses with calculated placement while retaining supplied artwork edits.</summary>
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        for (int frame = 0; frame < 3; frame++)
            if (IntroDiscoveryActorSpriteDefinitions.BabyFramePointer(frame) == pointer)
                return supplied.CalculateIfMatching(new IntroConfusedBabyParts(frame));
        return supplied;
    }
    /// <summary>The centered pose is represented by one large OAM part.</summary>
    public int Count => 1;
    /// <summary>Selects the first pose or one of two adjacent lower-row atlas tiles at X=-8, Y=-8.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfNotEqual(index, 0);
            int tile = frame == 0 ? IntroConfusedBabyCellDefinitions.FirstPose
                : IntroConfusedBabyCellDefinitions.LowerPair + 2 * (frame - 1);
            return new(SnesSpritemapXWord.Create(-8, true), unchecked((byte)-8),
                SnesObjAttributeWord.Create(tile, 0, 3, 0), true);
        }
    }
    /// <summary>Emits the single centered Baby part for the selected pose.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        yield return this[0];
    }
    /// <summary>Uses the same one-part traversal for non-generic enumeration.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
