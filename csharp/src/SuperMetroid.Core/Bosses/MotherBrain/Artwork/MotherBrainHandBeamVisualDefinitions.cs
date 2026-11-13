using SuperMetroid.Core.Enemies.Projectiles;


namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// The presentation boundary of the red hand beam (#641): the projectile frame every typed
/// frame shows.
/// </summary>
internal static class MotherBrainHandBeamVisualDefinitions
{

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<MotherBrainHandBeamFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<MotherBrainHandBeamFrame, EnemyProjectileFrameId>
        {
            [new(0, 0)] = EnemyProjectileFrameId.MotherBrainHandBeamFirstStageFirstPhase,
            [new(0, 1)] = EnemyProjectileFrameId.MotherBrainHandBeamFirstStageSecondPhase,
            [new(0, 2)] = EnemyProjectileFrameId.MotherBrainHandBeamFirstStageThirdPhase,
            [new(0, 3)] = EnemyProjectileFrameId.MotherBrainHandBeamFirstStageFourthPhase,
            [new(0, 4)] = EnemyProjectileFrameId.MotherBrainHandBeamFirstStageFifthPhase,
            [new(0, 5)] = EnemyProjectileFrameId.MotherBrainHandBeamFirstStageSixthPhase,
            [new(0, 6)] = EnemyProjectileFrameId.MotherBrainHandBeamFirstStageSeventhPhase,
            [new(1, 0)] = EnemyProjectileFrameId.MotherBrainHandBeamSecondStageFirstPhase,
            [new(1, 1)] = EnemyProjectileFrameId.MotherBrainHandBeamSecondStageSecondPhase,
            [new(1, 2)] = EnemyProjectileFrameId.MotherBrainHandBeamSecondStageThirdPhase,
            [new(1, 3)] = EnemyProjectileFrameId.MotherBrainHandBeamSecondStageFourthPhase,
            [new(1, 4)] = EnemyProjectileFrameId.MotherBrainHandBeamSecondStageFifthPhase,
            [new(1, 5)] = EnemyProjectileFrameId.MotherBrainHandBeamSecondStageSixthPhase,
            [new(1, 6)] = EnemyProjectileFrameId.MotherBrainHandBeamSecondStageSeventhPhase,
            [new(2, 0)] = EnemyProjectileFrameId.MotherBrainHandBeamThirdStageFirstPhase,
            [new(2, 1)] = EnemyProjectileFrameId.MotherBrainHandBeamThirdStageSecondPhase,
            [new(2, 2)] = EnemyProjectileFrameId.MotherBrainHandBeamThirdStageThirdPhase,
            [new(2, 3)] = EnemyProjectileFrameId.MotherBrainHandBeamThirdStageFourthPhase,
            [new(2, 4)] = EnemyProjectileFrameId.MotherBrainHandBeamThirdStageFifthPhase,
            [new(2, 5)] = EnemyProjectileFrameId.MotherBrainHandBeamThirdStageSixthPhase,
            [new(2, 6)] = EnemyProjectileFrameId.MotherBrainHandBeamThirdStageSeventhPhase,
        };
}
