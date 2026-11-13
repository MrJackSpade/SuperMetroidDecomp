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
        { new(Index: 0), EnemyFrameId.MotherBrainBodyStanding },
        { new(Index: 1), EnemyFrameId.MotherBrainBodyWalkingFirst },
        { new(Index: 10), EnemyFrameId.MotherBrainBodyUncrouching },
        { new(Index: 11), EnemyFrameId.MotherBrainBodyLeaningDown },
        { new(Index: 12), EnemyFrameId.MotherBrainBodyInitialDummy },
        { new(Index: 13), EnemyFrameId.MotherBrainBodyHandBeamSecond },
        { new(Index: 14), EnemyFrameId.MotherBrainBodyHandBeamThird },
        { new(Index: 15), EnemyFrameId.MotherBrainBodyHandBeamFourth },
        { new(Index: 16), EnemyFrameId.MotherBrainBodyHandBeamFifth },
        { new(Index: 2), EnemyFrameId.MotherBrainBodyWalkingSecond },
        { new(Index: 3), EnemyFrameId.MotherBrainBodyWalkingThird },
        { new(Index: 4), EnemyFrameId.MotherBrainBodyWalkingFourth },
        { new(Index: 5), EnemyFrameId.MotherBrainBodyWalkingFifth },
        { new(Index: 6), EnemyFrameId.MotherBrainBodyWalkingSixth },
        { new(Index: 7), EnemyFrameId.MotherBrainBodyWalkingSeventh },
        { new(Index: 8), EnemyFrameId.MotherBrainBodyWalkingEighth },
        { new(Index: 9), EnemyFrameId.MotherBrainBodyCrouched },
    };
}
