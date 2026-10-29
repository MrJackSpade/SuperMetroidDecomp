using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// Complete body-frame roots selected by the compiled Mother Brain locomotion,
/// posture, initial-dummy and hand-beam programs. Native roots interleave OAM
/// limbs and BG2 body streams; their hitboxes never enter either visual asset.
/// </summary>
internal static class MotherBrainBodyVisualDefinitions
{
    /// <summary>Standing, walking, crouch, dummy, and death-beam body roots selected by compiled programs.</summary>
    internal const int FrameCount = 17;

    /// <summary>The body frames drawn on Mother Brain's body layer: every pose but the sprite-only initial dummy (#1466).</summary>
    internal static IReadOnlySet<EnemyFrameId> BodyLayerFrames { get; } = new HashSet<EnemyFrameId>
    {
        EnemyFrameId.MotherBrainBodyStanding,
        EnemyFrameId.MotherBrainBodyWalking0,
        EnemyFrameId.MotherBrainBodyWalking1,
        EnemyFrameId.MotherBrainBodyWalking2,
        EnemyFrameId.MotherBrainBodyWalking3,
        EnemyFrameId.MotherBrainBodyWalking4,
        EnemyFrameId.MotherBrainBodyWalking5,
        EnemyFrameId.MotherBrainBodyWalking6,
        EnemyFrameId.MotherBrainBodyWalking7,
        EnemyFrameId.MotherBrainBodyCrouched,
        EnemyFrameId.MotherBrainBodyUncrouching,
        EnemyFrameId.MotherBrainBodyLeaningDown,
        EnemyFrameId.MotherBrainBodyDeathBeam0,
        EnemyFrameId.MotherBrainBodyDeathBeam1,
        EnemyFrameId.MotherBrainBodyDeathBeam2,
        EnemyFrameId.MotherBrainBodyDeathBeam3,
    };

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
