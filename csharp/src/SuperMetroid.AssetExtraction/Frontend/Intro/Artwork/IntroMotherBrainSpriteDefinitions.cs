using SuperMetroid.Core.Frontend.Intro;
namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>Three visual frames selected by the intro Mother Brain's compiled lists.</summary>
internal static class IntroMotherBrainSpriteDefinitions
{
    /// <summary>The intro's three Mother Brain frames.</summary>
    internal static IReadOnlyList<CinematicSpriteFrameDefinition> Frames { get; } =
    [
        new("mother-brain-pulse-low", CinematicSpritePose.MotherBrainPulseLow, 9),
        new("mother-brain-pulse-middle", CinematicSpritePose.MotherBrainPulseMiddle, 9),
        new("mother-brain-pulse-high", CinematicSpritePose.MotherBrainPulseHigh, 9),
    ];
}
