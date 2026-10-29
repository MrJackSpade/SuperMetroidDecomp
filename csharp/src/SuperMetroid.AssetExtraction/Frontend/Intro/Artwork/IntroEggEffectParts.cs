using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>Native8C:8F7E..8FCA contains centered single-tile effects. Fragments
/// traverse a two-column/three-row atlas patch; slime follows a three-cell row
/// then continues down its first column. Shared OAM fields are calculated.</summary>
internal static class IntroEggEffectParts
{
    /// <summary>Uses the calculated egg fragment or slime part only when it exactly matches the supplied OAM composition.</summary>
    internal static SpriteComposition CalculateIfMatching(CinematicSpritePose shown, SpriteComposition supplied)
    {
        for (int frame = 0; frame < 11; frame++)
            if (IntroEggEffectSpritePoses.FramePose(frame) == shown)
                return supplied.CalculateIfMatching(Parts(frame));
        return supplied;
    }
    /// <summary>
    /// The frame's one centered eight-pixel OBJ part: a shell-fragment or slime tile, centered four pixels about the origin.
    /// </summary>
    private static CompiledSpritePart[] Parts(int frame)
    {
        int tile;
        if (frame < 6) tile = IntroEggEffectCellDefinitions.FragmentStart + 16 * (frame / 2) + frame % 2;
        else
        {
            int slime = frame - 6;
            tile = IntroEggEffectCellDefinitions.SlimeStart + (slime < 3 ? slime : 16 * (slime - 2));
        }
        return [new(SpritePart.Create(Offset16<ScreenPixels>.Of(-4, -4), false, tile, 0, 3, 0), true)];
    }
}
