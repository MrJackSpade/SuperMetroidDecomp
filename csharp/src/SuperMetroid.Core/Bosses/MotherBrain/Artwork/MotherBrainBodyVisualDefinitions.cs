using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// Complete body-frame roots selected by the compiled Mother Brain locomotion,
/// posture, initial-dummy and hand-beam animations. Roots interleave sprite
/// limbs and body-layer streams; their hitboxes never enter either visual asset.
/// </summary>
internal static class MotherBrainBodyVisualDefinitions
{
    /// <summary>The body frames drawn on Mother Brain's body layer: every pose but the sprite-only initial dummy (#1466).</summary>
    internal static IReadOnlySet<EnemyFrameId> BodyLayerFrames { get; } = new HashSet<EnemyFrameId>
    {
        EnemyFrameId.MotherBrainBodyStanding,
        EnemyFrameId.MotherBrainBodyWalkingFirst,
        EnemyFrameId.MotherBrainBodyWalkingSecond,
        EnemyFrameId.MotherBrainBodyWalkingThird,
        EnemyFrameId.MotherBrainBodyWalkingFourth,
        EnemyFrameId.MotherBrainBodyWalkingFifth,
        EnemyFrameId.MotherBrainBodyWalkingSixth,
        EnemyFrameId.MotherBrainBodyWalkingSeventh,
        EnemyFrameId.MotherBrainBodyWalkingEighth,
        EnemyFrameId.MotherBrainBodyCrouched,
        EnemyFrameId.MotherBrainBodyUncrouching,
        EnemyFrameId.MotherBrainBodyLeaningDown,
        EnemyFrameId.MotherBrainBodyHandBeamSecond,
        EnemyFrameId.MotherBrainBodyHandBeamThird,
        EnemyFrameId.MotherBrainBodyHandBeamFourth,
        EnemyFrameId.MotherBrainBodyHandBeamFifth,
    };

    /// <summary>The installed frame each <see cref="MotherBrainBodyFrame"/> shows.</summary>
    internal static EnemyFrameTable<MotherBrainBodyFrame> ShownFrames { get; } = new()
    {
        { MotherBrainBodyFrame.Standing, EnemyFrameId.MotherBrainBodyStanding },
        { MotherBrainBodyFrame.WalkingFirst, EnemyFrameId.MotherBrainBodyWalkingFirst },
        { MotherBrainBodyFrame.WalkingSecond, EnemyFrameId.MotherBrainBodyWalkingSecond },
        { MotherBrainBodyFrame.WalkingThird, EnemyFrameId.MotherBrainBodyWalkingThird },
        { MotherBrainBodyFrame.WalkingFourth, EnemyFrameId.MotherBrainBodyWalkingFourth },
        { MotherBrainBodyFrame.WalkingFifth, EnemyFrameId.MotherBrainBodyWalkingFifth },
        { MotherBrainBodyFrame.WalkingSixth, EnemyFrameId.MotherBrainBodyWalkingSixth },
        { MotherBrainBodyFrame.WalkingSeventh, EnemyFrameId.MotherBrainBodyWalkingSeventh },
        { MotherBrainBodyFrame.WalkingEighth, EnemyFrameId.MotherBrainBodyWalkingEighth },
        { MotherBrainBodyFrame.Crouched, EnemyFrameId.MotherBrainBodyCrouched },
        { MotherBrainBodyFrame.Uncrouching, EnemyFrameId.MotherBrainBodyUncrouching },
        { MotherBrainBodyFrame.LeaningDown, EnemyFrameId.MotherBrainBodyLeaningDown },
        { MotherBrainBodyFrame.InitialDummy, EnemyFrameId.MotherBrainBodyInitialDummy },
        { MotherBrainBodyFrame.HandBeamSecond, EnemyFrameId.MotherBrainBodyHandBeamSecond },
        { MotherBrainBodyFrame.HandBeamThird, EnemyFrameId.MotherBrainBodyHandBeamThird },
        { MotherBrainBodyFrame.HandBeamFourth, EnemyFrameId.MotherBrainBodyHandBeamFourth },
        { MotherBrainBodyFrame.HandBeamFifth, EnemyFrameId.MotherBrainBodyHandBeamFifth },
    };
}
