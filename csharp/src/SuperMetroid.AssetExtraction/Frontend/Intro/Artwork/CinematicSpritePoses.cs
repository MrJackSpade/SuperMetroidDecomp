using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>
/// The poses each cinematic family composes (#1638): exactly the poses its definitions name, which every document of the
/// family's compositions must hold, or the poses of the escaping animals' compiled pods. A sheet's frames are named from them without reading the compositions, which are import
/// data whose parts are the original's packed sprite placements.
/// </summary>
internal static class CinematicSpritePoses
{
    /// <summary>The poses <paramref name="family"/> composes, in definition order.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The family is undefined.</exception>
    internal static IEnumerable<CinematicSpritePose> Of(CinematicSpriteFamily family) => family switch
    {
        CinematicSpriteFamily.IntroDiscoveryActor => Poses(IntroDiscoveryActorSpriteDefinitions.Frames),
        CinematicSpriteFamily.IntroEggEffect => Poses(IntroEggEffectSpriteDefinitions.Frames),
        CinematicSpriteFamily.IntroRinka => Poses(IntroRinkaSpriteDefinitions.Frames),
        CinematicSpriteFamily.IntroScientist => Poses(IntroScientistSpriteDefinitions.Frames),
        CinematicSpriteFamily.IntroMotherBrain => Poses(IntroMotherBrainSpriteDefinitions.Frames),
        CinematicSpriteFamily.IntroMotherBrainExplosion => Poses(IntroMotherBrainExplosionSpriteDefinitions.Frames),
        CinematicSpriteFamily.IntroCaret => Poses(IntroCaretSpriteDefinitions.Frames),
        CinematicSpriteFamily.IntroNextPageArrow => Poses(IntroNextPageArrowSpriteDefinitions.Frames),
        CinematicSpriteFamily.CeresFlight => Poses(CeresFlightSpriteDefinitions.Frames),
        CinematicSpriteFamily.CeresDestruction => CeresDestructionSpriteFrameDefinitions.Frames.Select(frame => frame.Pose),
        CinematicSpriteFamily.EndingCloud => Poses(EndingCloudSpriteDefinitions.Frames),
        CinematicSpriteFamily.EndingExplosion => Poses(EndingExplosionSpriteDefinitions.Frames),
        CinematicSpriteFamily.EndingCompletionText => Poses(EndingCompletionTextSpriteDefinitions.Frames),
        CinematicSpriteFamily.EndingReward => Poses(EndingRewardSpriteDefinitions.Frames),
        CinematicSpriteFamily.EndingLogo => Poses(EndingLogoSpriteDefinitions.Frames),
        // The escaping animals' pods are compiled compositions, not a document.
        CinematicSpriteFamily.EndingAnimalEscape => ((ICinematicSpriteFrames)EndingAnimalEscapeParts.Sprites).Frames.Keys,
        _ => throw new ArgumentOutOfRangeException(nameof(family), family, "Undefined cinematic family."),
    };

    /// <summary>The poses <paramref name="frames"/> define.</summary>
    private static IEnumerable<CinematicSpritePose> Poses(IReadOnlyList<CinematicSpriteFrameDefinition> frames) => frames.Select(frame => frame.Pose);
}
