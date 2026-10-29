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
    /// <summary>Schema revision of the selected BG2 body-frame document.</summary>
    internal const int Version = 2;
    /// <summary>Installed JSON filename for Mother Brain's selected BG2 body frames.</summary>
    internal const string FileName = "mother-brain-body-bg2-frames.json";
    /// <summary>Every body frame with a BG2 stream, omitting the OAM-only initial dummy.</summary>
    internal static ImmutableArray<EnemySpritemapDefinition> Frames { get; } =
    [
        new(EnemyFrameId.MotherBrainBodyStanding, "mother_brain_body_bg2_standing"),
        new(EnemyFrameId.MotherBrainBodyWalking0, "mother_brain_body_bg2_walking_0"),
        new(EnemyFrameId.MotherBrainBodyWalking1, "mother_brain_body_bg2_walking_1"),
        new(EnemyFrameId.MotherBrainBodyWalking2, "mother_brain_body_bg2_walking_2"),
        new(EnemyFrameId.MotherBrainBodyWalking3, "mother_brain_body_bg2_walking_3"),
        new(EnemyFrameId.MotherBrainBodyWalking4, "mother_brain_body_bg2_walking_4"),
        new(EnemyFrameId.MotherBrainBodyWalking5, "mother_brain_body_bg2_walking_5"),
        new(EnemyFrameId.MotherBrainBodyWalking6, "mother_brain_body_bg2_walking_6"),
        new(EnemyFrameId.MotherBrainBodyWalking7, "mother_brain_body_bg2_walking_7"),
        new(EnemyFrameId.MotherBrainBodyCrouched, "mother_brain_body_bg2_crouched"),
        new(EnemyFrameId.MotherBrainBodyUncrouching, "mother_brain_body_bg2_uncrouching"),
        new(EnemyFrameId.MotherBrainBodyLeaningDown, "mother_brain_body_bg2_leaning_down"),
        new(EnemyFrameId.MotherBrainBodyDeathBeam0, "mother_brain_body_bg2_death_beam_0"),
        new(EnemyFrameId.MotherBrainBodyDeathBeam1, "mother_brain_body_bg2_death_beam_1"),
        new(EnemyFrameId.MotherBrainBodyDeathBeam2, "mother_brain_body_bg2_death_beam_2"),
        new(EnemyFrameId.MotherBrainBodyDeathBeam3, "mother_brain_body_bg2_death_beam_3"),
    ];
}
