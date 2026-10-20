using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Three centered sixteen-pixel baby poses,packed at the end of the intro egg atlas.</summary>
internal static class IntroConfusedBabyParts
{
    /// <summary>Replaces matching intro Baby poses with calculated placement while retaining supplied artwork edits.</summary>
    internal static SpriteComposition CalculateIfMatching(ushort pointer, SpriteComposition supplied)
    {
        for (int frame = 0; frame < 3; frame++)
            if (IntroDiscoveryActorSpriteDefinitions.BabyFramePointer(frame) == pointer)
                return supplied.CalculateIfMatching(Parts(frame));
        return supplied;
    }
    /// <summary>
    /// The pose's one large OAM part: the first pose or one of two adjacent lower-row atlas tiles at X=-8, Y=-8.
    /// </summary>
    private static CompiledSpritePart[] Parts(int frame)
    {
        int tile = frame == 0 ? IntroConfusedBabyCellDefinitions.FirstPose
            : IntroConfusedBabyCellDefinitions.LowerPair + 2 * (frame - 1);
        return [new(SnesSpritemapXWord.Create(-8, true), SnesSpritemapYByte.Create(-8), SnesObjAttributeWord.Create(tile, 0, 3, 0), true)];
    }
}
