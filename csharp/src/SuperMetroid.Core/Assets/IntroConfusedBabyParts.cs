using System.Collections;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Three centered sixteen-pixel baby poses,packed at the end of the intro egg atlas.</summary>
internal sealed class IntroConfusedBabyParts(int frame) : IReadOnlyList<CompiledSpritePart>
{
    /// <summary>Calculates if matching.</summary>
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        for (int frame = 0; frame < 3; frame++)
            if (IntroDiscoveryActorSpriteDefinitions.BabyFramePointer(frame) == pointer)
                return supplied.CalculateIfMatching(new IntroConfusedBabyParts(frame));
        return supplied;
    }
    /// <summary>Gets count.</summary>
    public int Count => 1;
    /// <summary>Gets or sets an indexed value.</summary>
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
    /// <summary>Gets enumerator.</summary>
    public IEnumerator<CompiledSpritePart> GetEnumerator()
    {
        yield return this[0];
    }
    /// <summary>Gets enumerator.</summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>Represents intro confused baby atlas.</summary>
internal static class IntroConfusedBabyAtlas
{
    /// <summary>Tile$11E,first sixteen-pixel baby drawing in the upper atlas-row pair final columns.</summary>
    internal const int FirstPose = 0x11e; // magic-number-audit: allow(PoseOrMovement) - tile atlas indices and strides for cinematic sprite composition.
    /// <summary>Tile$13C,start of two adjacent sixteen-pixel drawings in the next atlas-row pair.</summary>
    internal const int LowerPair = 0x13c;
}
