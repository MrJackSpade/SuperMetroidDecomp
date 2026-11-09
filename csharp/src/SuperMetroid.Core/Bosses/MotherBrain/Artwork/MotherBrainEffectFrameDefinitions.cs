using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// The presentation boundary of Mother Brain's onion rings, bombs, breath, drool, door fragments and subtitle programs (#641): the projectile frame each
/// <see cref="MotherBrainEffectFrame"/> pose shows.
/// </summary>
internal static class MotherBrainEffectFrameDefinitions
{
    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<MotherBrainEffectFrame, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<MotherBrainEffectFrame, EnemyProjectileFrameId>
        {
            [new(0)] = EnemyProjectileFrameId.MotherBrainBlueRing00,
            [new(1)] = EnemyProjectileFrameId.MotherBrainBlueRing01,
            [new(2)] = EnemyProjectileFrameId.MotherBrainBlueRing02,
            [new(3)] = EnemyProjectileFrameId.MotherBrainBlueRing03,
            [new(4)] = EnemyProjectileFrameId.MotherBrainBlueRing04,
            [new(5)] = EnemyProjectileFrameId.MotherBrainBlueRing05,
            [new(6)] = EnemyProjectileFrameId.MotherBrainBlueRingImpact00,
            [new(7)] = EnemyProjectileFrameId.MotherBrainBlueRingImpact01,
            [new(8)] = EnemyProjectileFrameId.MotherBrainBlueRingImpact02,
            [new(9)] = EnemyProjectileFrameId.MotherBrainBlueRingImpact03,
            [new(10)] = EnemyProjectileFrameId.MotherBrainBlueRingImpact04,
            [new(11)] = EnemyProjectileFrameId.MotherBrainBlueRingImpact05,
            [new(12)] = EnemyProjectileFrameId.MotherBrainBombFrame00,
            [new(13)] = EnemyProjectileFrameId.MotherBrainBombFrame01,
            [new(14)] = EnemyProjectileFrameId.MotherBrainBombFrame02,
            [new(15)] = EnemyProjectileFrameId.MotherBrainBombFrame03,
            [new(16)] = EnemyProjectileFrameId.MotherBrainBombFrame04,
            [new(17)] = EnemyProjectileFrameId.MotherBrainBombFrame05,
            [new(18)] = EnemyProjectileFrameId.MotherBrainBombFrame06,
            [new(19)] = EnemyProjectileFrameId.MotherBrainBombFrame07,
            [new(20)] = EnemyProjectileFrameId.MotherBrainBombFrame08,
            [new(21)] = EnemyProjectileFrameId.MotherBrainRainbowBeamChargingFrame00,
            [new(22)] = EnemyProjectileFrameId.MotherBrainRainbowBeamChargingFrame01,
            [new(23)] = EnemyProjectileFrameId.MotherBrainRainbowBeamChargingFrame02,
            [new(24)] = EnemyProjectileFrameId.MotherBrainRainbowBeamChargingFrame03,
            [new(25)] = EnemyProjectileFrameId.MotherBrainRainbowBeamChargingFrame04,
            [new(26)] = EnemyProjectileFrameId.MotherBrainRainbowBeamChargingFrame05,
            [new(27)] = EnemyProjectileFrameId.MotherBrainDroolAttached00,
            [new(28)] = EnemyProjectileFrameId.MotherBrainDroolAttached01,
            [new(29)] = EnemyProjectileFrameId.MotherBrainDroolAttached02,
            [new(30)] = EnemyProjectileFrameId.MotherBrainDroolAttached03,
            [new(31)] = EnemyProjectileFrameId.MotherBrainDroolAttached04,
            [new(32)] = EnemyProjectileFrameId.MotherBrainDroolReleased,
            [new(33)] = EnemyProjectileFrameId.MotherBrainDroolHitFloorFrame00,
            [new(34)] = EnemyProjectileFrameId.MotherBrainDroolHitFloorFrame01,
            [new(35)] = EnemyProjectileFrameId.MotherBrainDroolHitFloorFrame02,
            [new(36)] = EnemyProjectileFrameId.MotherBrainDroolHitFloorFrame03,
            [new(37)] = EnemyProjectileFrameId.MotherBrainEscapeDoorParticleFrame00,
            [new(38)] = EnemyProjectileFrameId.MotherBrainEscapeDoorParticleFrame01,
            [new(39)] = EnemyProjectileFrameId.MotherBrainEscapeDoorParticleFrame02,
            [new(40)] = EnemyProjectileFrameId.MotherBrainEscapeDoorParticleFrame03,
            [new(41)] = EnemyProjectileFrameId.MotherBrainEscapeDoorParticleFrame04,
            [new(42)] = EnemyProjectileFrameId.MotherBrainEscapeDoorParticleFrame05,
            [new(43)] = EnemyProjectileFrameId.MotherBrainEscapeDoorParticleFrame06,
            [new(44)] = EnemyProjectileFrameId.MotherBrainEscapeDoorParticleFrame07,
            [new(45)] = EnemyProjectileFrameId.MotherBrainPurpleBreathBigFrame00,
            [new(46)] = EnemyProjectileFrameId.MotherBrainPurpleBreathBigFrame01,
            [new(47)] = EnemyProjectileFrameId.MotherBrainPurpleBreathBigFrame02,
            [new(48)] = EnemyProjectileFrameId.MotherBrainPurpleBreathBigFrame03,
            [new(49)] = EnemyProjectileFrameId.MotherBrainPurpleBreathBigFrame04,
            [new(50)] = EnemyProjectileFrameId.MotherBrainPurpleBreathBigFrame05,
            [new(51)] = EnemyProjectileFrameId.MotherBrainPurpleBreathBigFrame06,
            [new(52)] = EnemyProjectileFrameId.MotherBrainPurpleBreathBigFrame07,
            [new(53)] = EnemyProjectileFrameId.MotherBrainPurpleBreathSmallFrame00,
            [new(54)] = EnemyProjectileFrameId.MotherBrainPurpleBreathSmallFrame01,
            [new(55)] = EnemyProjectileFrameId.MotherBrainPurpleBreathSmallFrame02,
            [new(56)] = EnemyProjectileFrameId.MotherBrainPurpleBreathSmallFrame03,
            [new(57)] = EnemyProjectileFrameId.MotherBrainPurpleBreathSmallFrame04,
            [new(58)] = EnemyProjectileFrameId.MotherBrainPurpleBreathSmallFrame05,
            [new(59)] = EnemyProjectileFrameId.MotherBrainPurpleBreathSmallFrame06,
            [new(60)] = EnemyProjectileFrameId.MotherBrainPurpleBreathSmallFrame07,
            [new(61)] = EnemyProjectileFrameId.MotherBrainEscapeSubtitleFrame00,
        };
}
