using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// Complete body-frame roots selected by the compiled Mother Brain locomotion,
/// posture, initial-dummy and hand-beam programs. Native roots interleave OAM
/// limbs and BG2 body streams; their hitboxes never enter either visual asset.
/// </summary>
internal static class MotherBrainBodyVisualDefinitions
{
    /// <summary>Largest native root: ten components, including two BG2 streams.</summary>
    internal const int MaximumComponents = EnemyBg2FrameLayout.MaximumComponents;
    /// <summary>Standing, walking, crouch, dummy, and death-beam body roots selected by compiled programs.</summary>
    internal const int FrameCount = 17;
    /// <summary>Schema revision of the selected BG2 body-frame document.</summary>
    internal const int Bg2Version = 2;
    /// <summary>Installed JSON filename for Mother Brain's selected BG2 body frames.</summary>
    internal const string Bg2FileName = "mother-brain-body-bg2-frames.json";
    /// <summary>Every body frame with a BG2 stream, omitting the OAM-only initial dummy.</summary>
    internal static ImmutableArray<EnemySpritemapDefinition> Bg2Frames { get; } =
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

    /// <summary>Whether <paramref name="frame"/> is a body frame with a BG2 stream.</summary>
    internal static bool HasBg2(EnemyFrameId frame) => Bg2Frames.Any(definition => definition.Frame == frame);

    /// <summary>The installed frame each <see cref="MotherBrainBodyFrame"/> shows.</summary>
    internal static EnemyFrameTable<MotherBrainBodyFrame> ShownFrames { get; } = new()
    {
        { new(Index: 0), EnemyFrameId.MotherBrainBodyStanding },
        { new(Index: 1), EnemyFrameId.MotherBrainBodyWalking0 },
        { new(Index: 10), EnemyFrameId.MotherBrainBodyUncrouching },
        { new(Index: 11), EnemyFrameId.MotherBrainBodyLeaningDown },
        { new(Index: 12), EnemyFrameId.MotherBrainBodyInitialDummy },
        { new(Index: 13), EnemyFrameId.MotherBrainBodyDeathBeam0 },
        { new(Index: 14), EnemyFrameId.MotherBrainBodyDeathBeam1 },
        { new(Index: 15), EnemyFrameId.MotherBrainBodyDeathBeam2 },
        { new(Index: 16), EnemyFrameId.MotherBrainBodyDeathBeam3 },
        { new(Index: 2), EnemyFrameId.MotherBrainBodyWalking1 },
        { new(Index: 3), EnemyFrameId.MotherBrainBodyWalking2 },
        { new(Index: 4), EnemyFrameId.MotherBrainBodyWalking3 },
        { new(Index: 5), EnemyFrameId.MotherBrainBodyWalking4 },
        { new(Index: 6), EnemyFrameId.MotherBrainBodyWalking5 },
        { new(Index: 7), EnemyFrameId.MotherBrainBodyWalking6 },
        { new(Index: 8), EnemyFrameId.MotherBrainBodyWalking7 },
        { new(Index: 9), EnemyFrameId.MotherBrainBodyCrouched },
    };
}
