using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// Complete body-frame roots selected by the compiled Mother Brain locomotion,
/// posture, initial-dummy and hand-beam programs. Native roots interleave OAM
/// limbs and BG2 body streams; their hitboxes never enter either visual asset.
/// </summary>
internal static class MotherBrainBodyVisualDefinitions
{
    /// <summary>Largest native root: ten components, including two BG2 streams.</summary>
    internal const int MaximumNativeComponents = EnemyBg2FrameLayout.MaximumComponents;
    /// <summary>Standing, walking, crouch, dummy, and death-beam body roots selected by compiled programs.</summary>
    internal const int FrameCount = 17;
    /// <summary>Schema revision of the selected BG2 body-frame document.</summary>
    internal const int Bg2Version = 1;
    /// <summary>Installed JSON filename for Mother Brain's selected BG2 body frames.</summary>
    internal const string Bg2FileName = "mother-brain-body-bg2-frames.json";
    /// <summary>Every body frame with a BG2 stream, omitting the OAM-only initial dummy.</summary>
    internal static ImmutableArray<EnemySpritemapDefinition> Bg2Frames { get; } =
    [
        new(EnemyFrameId.MotherBrainBodyOamStanding, "mother_brain_body_bg2_standing"),
        new(EnemyFrameId.MotherBrainBodyOamWalking0, "mother_brain_body_bg2_walking_0"),
        new(EnemyFrameId.MotherBrainBodyOamWalking1, "mother_brain_body_bg2_walking_1"),
        new(EnemyFrameId.MotherBrainBodyOamWalking2, "mother_brain_body_bg2_walking_2"),
        new(EnemyFrameId.MotherBrainBodyOamWalking3, "mother_brain_body_bg2_walking_3"),
        new(EnemyFrameId.MotherBrainBodyOamWalking4, "mother_brain_body_bg2_walking_4"),
        new(EnemyFrameId.MotherBrainBodyOamWalking5, "mother_brain_body_bg2_walking_5"),
        new(EnemyFrameId.MotherBrainBodyOamWalking6, "mother_brain_body_bg2_walking_6"),
        new(EnemyFrameId.MotherBrainBodyOamWalking7, "mother_brain_body_bg2_walking_7"),
        new(EnemyFrameId.MotherBrainBodyOamCrouched, "mother_brain_body_bg2_crouched"),
        new(EnemyFrameId.MotherBrainBodyOamUncrouching, "mother_brain_body_bg2_uncrouching"),
        new(EnemyFrameId.MotherBrainBodyOamLeaningDown, "mother_brain_body_bg2_leaning_down"),
        new(EnemyFrameId.MotherBrainBodyOamDeathBeam0, "mother_brain_body_bg2_death_beam_0"),
        new(EnemyFrameId.MotherBrainBodyOamDeathBeam1, "mother_brain_body_bg2_death_beam_1"),
        new(EnemyFrameId.MotherBrainBodyOamDeathBeam2, "mother_brain_body_bg2_death_beam_2"),
        new(EnemyFrameId.MotherBrainBodyOamDeathBeam3, "mother_brain_body_bg2_death_beam_3"),
    ];

    /// <summary>Whether <paramref name="frame"/> is a body frame with a BG2 stream.</summary>
    internal static bool HasBg2(EnemyFrameId frame) => Bg2Frames.Any(definition => definition.Frame == frame);

    /// <summary>The installed frame each <see cref="MotherBrainBodyFrame"/> shows.</summary>
    internal static EnemyFrameTable<MotherBrainBodyFrame> ShownFrames { get; } = new()
    {
        { new(Index: 0), EnemyFrameId.MotherBrainBodyOamStanding },
        { new(Index: 1), EnemyFrameId.MotherBrainBodyOamWalking0 },
        { new(Index: 10), EnemyFrameId.MotherBrainBodyOamUncrouching },
        { new(Index: 11), EnemyFrameId.MotherBrainBodyOamLeaningDown },
        { new(Index: 12), EnemyFrameId.MotherBrainBodyOamInitialDummy },
        { new(Index: 13), EnemyFrameId.MotherBrainBodyOamDeathBeam0 },
        { new(Index: 14), EnemyFrameId.MotherBrainBodyOamDeathBeam1 },
        { new(Index: 15), EnemyFrameId.MotherBrainBodyOamDeathBeam2 },
        { new(Index: 16), EnemyFrameId.MotherBrainBodyOamDeathBeam3 },
        { new(Index: 2), EnemyFrameId.MotherBrainBodyOamWalking1 },
        { new(Index: 3), EnemyFrameId.MotherBrainBodyOamWalking2 },
        { new(Index: 4), EnemyFrameId.MotherBrainBodyOamWalking3 },
        { new(Index: 5), EnemyFrameId.MotherBrainBodyOamWalking4 },
        { new(Index: 6), EnemyFrameId.MotherBrainBodyOamWalking5 },
        { new(Index: 7), EnemyFrameId.MotherBrainBodyOamWalking6 },
        { new(Index: 8), EnemyFrameId.MotherBrainBodyOamWalking7 },
        { new(Index: 9), EnemyFrameId.MotherBrainBodyOamCrouched },
    };
}
