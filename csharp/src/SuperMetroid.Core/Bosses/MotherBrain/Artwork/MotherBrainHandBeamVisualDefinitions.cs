using SuperMetroid.Core.Enemies.Projectiles;


namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// The presentation boundary of the red hand beam (#641): the projectile frame every typed
/// frame shows.
/// </summary>
internal static class MotherBrainHandBeamVisualDefinitions
{
    /// <summary>Distinct frames: seven in each of three stages.</summary>
    internal const int ProjectileFrameCount = MotherBrainHandBeamAnimations.StageCount * MotherBrainHandBeamAnimations.FramesPerStage;

    /// <summary>Every frame in installed-artwork order: the stages in turn.</summary>
    internal static IReadOnlyList<MotherBrainHandBeamFrame> ProjectileFrames { get; } =
    [
        .. Enumerable.Range(0, MotherBrainHandBeamAnimations.StageCount).SelectMany(stage =>
            Enumerable.Range(0, MotherBrainHandBeamAnimations.FramesPerStage).Select(phase => new MotherBrainHandBeamFrame(stage, phase))),
    ];

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<MotherBrainHandBeamFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<MotherBrainHandBeamFrame, EnemyProjectileFrameId>
        {
            [new(0, 0)] = EnemyProjectileFrameId.MotherBrainHandBeamFrame00,
            [new(0, 1)] = EnemyProjectileFrameId.MotherBrainHandBeamFrame01,
            [new(0, 2)] = EnemyProjectileFrameId.MotherBrainHandBeamFrame02,
            [new(0, 3)] = EnemyProjectileFrameId.MotherBrainHandBeamFrame03,
            [new(0, 4)] = EnemyProjectileFrameId.MotherBrainHandBeamFrame04,
            [new(0, 5)] = EnemyProjectileFrameId.MotherBrainHandBeamFrame05,
            [new(0, 6)] = EnemyProjectileFrameId.MotherBrainHandBeamFrame06,
            [new(1, 0)] = EnemyProjectileFrameId.MotherBrainHandBeamFrame07,
            [new(1, 1)] = EnemyProjectileFrameId.MotherBrainHandBeamFrame08,
            [new(1, 2)] = EnemyProjectileFrameId.MotherBrainHandBeamFrame09,
            [new(1, 3)] = EnemyProjectileFrameId.MotherBrainHandBeamFrame10,
            [new(1, 4)] = EnemyProjectileFrameId.MotherBrainHandBeamFrame11,
            [new(1, 5)] = EnemyProjectileFrameId.MotherBrainHandBeamFrame12,
            [new(1, 6)] = EnemyProjectileFrameId.MotherBrainHandBeamFrame13,
            [new(2, 0)] = EnemyProjectileFrameId.MotherBrainHandBeamFrame14,
            [new(2, 1)] = EnemyProjectileFrameId.MotherBrainHandBeamFrame15,
            [new(2, 2)] = EnemyProjectileFrameId.MotherBrainHandBeamFrame16,
            [new(2, 3)] = EnemyProjectileFrameId.MotherBrainHandBeamFrame17,
            [new(2, 4)] = EnemyProjectileFrameId.MotherBrainHandBeamFrame18,
            [new(2, 5)] = EnemyProjectileFrameId.MotherBrainHandBeamFrame19,
            [new(2, 6)] = EnemyProjectileFrameId.MotherBrainHandBeamFrame20,
        };
}
