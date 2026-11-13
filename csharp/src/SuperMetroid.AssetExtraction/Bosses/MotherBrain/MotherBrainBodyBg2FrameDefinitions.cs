using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>
/// The installed document of Mother Brain's body-layer poses (#1466): its schema, file name and the name each pose is
/// stored under. Native roots interleave sprite limbs and BG2 body streams; their hitboxes never enter the document.
/// </summary>
internal static class MotherBrainBodyBg2FrameDefinitions
{
    /// <summary>Largest native root: ten components, including two BG2 streams.</summary>
    internal const int MaximumComponents = EnemyBg2FrameLayout.MaximumComponents;
    /// <summary>Schema revision of the selected BG2 body-frame document; 3 names each frame for the animation that first shows it (#1623).</summary>
    internal const int Version = 3;
    /// <summary>Installed JSON filename for Mother Brain's selected BG2 body frames.</summary>
    internal const string FileName = "mother-brain-body-bg2-frames.json";
    /// <summary>Every body frame with a BG2 stream, omitting the OAM-only initial dummy.</summary>
    internal static ImmutableArray<EnemyFrameName> Frames { get; } =
    [
        new(EnemyFrameId.MotherBrainBodyStanding, "mother_brain_body_bg2_standing"),
        new(EnemyFrameId.MotherBrainBodyWalkingFirst, "mother_brain_body_bg2_walking_0"),
        new(EnemyFrameId.MotherBrainBodyWalkingSecond, "mother_brain_body_bg2_walking_1"),
        new(EnemyFrameId.MotherBrainBodyWalkingThird, "mother_brain_body_bg2_walking_2"),
        new(EnemyFrameId.MotherBrainBodyWalkingFourth, "mother_brain_body_bg2_walking_3"),
        new(EnemyFrameId.MotherBrainBodyWalkingFifth, "mother_brain_body_bg2_walking_4"),
        new(EnemyFrameId.MotherBrainBodyWalkingSixth, "mother_brain_body_bg2_walking_5"),
        new(EnemyFrameId.MotherBrainBodyWalkingSeventh, "mother_brain_body_bg2_walking_6"),
        new(EnemyFrameId.MotherBrainBodyWalkingEighth, "mother_brain_body_bg2_walking_7"),
        new(EnemyFrameId.MotherBrainBodyCrouched, "mother_brain_body_bg2_crouched"),
        new(EnemyFrameId.MotherBrainBodyUncrouching, "mother_brain_body_bg2_uncrouching"),
        new(EnemyFrameId.MotherBrainBodyLeaningDown, "mother_brain_body_bg2_leaning_down"),
        new(EnemyFrameId.MotherBrainBodyHandBeamSecond, "mother_brain_body_bg2_death_beam_0"),
        new(EnemyFrameId.MotherBrainBodyHandBeamThird, "mother_brain_body_bg2_death_beam_1"),
        new(EnemyFrameId.MotherBrainBodyHandBeamFourth, "mother_brain_body_bg2_death_beam_2"),
        new(EnemyFrameId.MotherBrainBodyHandBeamFifth, "mother_brain_body_bg2_death_beam_3"),
    ];
}
