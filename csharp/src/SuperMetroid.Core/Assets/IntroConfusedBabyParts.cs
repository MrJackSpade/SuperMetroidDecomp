using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Three centered sixteen-pixel baby poses,packed at the end of the intro egg atlas.</summary>
/// <param name="frame">Pose number from zero through two, selecting the matching baby drawing in the atlas.</param>
internal sealed class IntroConfusedBabyParts(int frame) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Replaces the supplied composition with the centered baby pose when its pointer identifies a known frame.</summary>
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        for (int frame = 0; frame < 3; frame++)
            if (IntroDiscoveryActorSpriteDefinitions.BabyFramePointer(frame) == pointer)
                return supplied.CalculateIfMatching(new IntroConfusedBabyParts(frame));
        return supplied;
    }

    /// <summary>The baby pose is represented by one composite sprite part.</summary>
    public int Count => 1;

    /// <summary>Gets the centered sprite part; this single-part list accepts only index zero.</summary>
    public CompiledSpritePart this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfNotEqual(index, 0);
            int tile = frame == 0 ? IntroConfusedBabyAtlas.FirstPose
                : IntroConfusedBabyAtlas.LowerPair + 2 * (frame - 1);
            return new(SnesSpritemapXWord.Create(-8, true), unchecked((byte)-8),
                SnesObjAttributeWord.Create(tile, 0, 3, 0), true);
        }
    }

    /// <summary>Enumerates the single compiled part that makes up the selected baby pose.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        yield return this[0];
    }
    /// <summary>Returns a non-generic enumerator over this sequence.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>Tile indices for the three baby pose drawings stored in the intro egg atlas.</summary>
internal static class IntroConfusedBabyAtlas
{
    /// <summary>Tile$11E,first sixteen-pixel baby drawing in the upper atlas-row pair final columns.</summary>
    internal const int FirstPose = 0x11e; // magic-number-audit: allow(PoseOrMovement) - tile atlas indices and strides for cinematic sprite composition.
    /// <summary>Tile$13C,start of two adjacent sixteen-pixel drawings in the next atlas-row pair.</summary>
    internal const int LowerPair = 0x13c;
}
