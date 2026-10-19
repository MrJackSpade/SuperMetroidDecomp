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
            [new(12)] = EnemyProjectileFrameId.Projectile86C76eFrame00,
            [new(13)] = EnemyProjectileFrameId.Projectile86C76eFrame01,
            [new(14)] = EnemyProjectileFrameId.Projectile86C76eFrame02,
            [new(15)] = EnemyProjectileFrameId.Projectile86C76eFrame03,
            [new(16)] = EnemyProjectileFrameId.Projectile86C76eFrame04,
            [new(17)] = EnemyProjectileFrameId.Projectile86C76eFrame05,
            [new(18)] = EnemyProjectileFrameId.Projectile86C76eFrame06,
            [new(19)] = EnemyProjectileFrameId.Projectile86C76eFrame07,
            [new(20)] = EnemyProjectileFrameId.Projectile86C76eFrame08,
            [new(21)] = EnemyProjectileFrameId.Projectile86C829Frame00,
            [new(22)] = EnemyProjectileFrameId.Projectile86C829Frame01,
            [new(23)] = EnemyProjectileFrameId.Projectile86C829Frame02,
            [new(24)] = EnemyProjectileFrameId.Projectile86C829Frame03,
            [new(25)] = EnemyProjectileFrameId.Projectile86C829Frame04,
            [new(26)] = EnemyProjectileFrameId.Projectile86C829Frame05,
            [new(27)] = EnemyProjectileFrameId.MotherBrainDroolAttached00,
            [new(28)] = EnemyProjectileFrameId.MotherBrainDroolAttached01,
            [new(29)] = EnemyProjectileFrameId.MotherBrainDroolAttached02,
            [new(30)] = EnemyProjectileFrameId.MotherBrainDroolAttached03,
            [new(31)] = EnemyProjectileFrameId.MotherBrainDroolAttached04,
            [new(32)] = EnemyProjectileFrameId.MotherBrainDroolReleased,
            [new(33)] = EnemyProjectileFrameId.Projectile86C8e1Frame00,
            [new(34)] = EnemyProjectileFrameId.Projectile86C8e1Frame01,
            [new(35)] = EnemyProjectileFrameId.Projectile86C8e1Frame02,
            [new(36)] = EnemyProjectileFrameId.Projectile86C8e1Frame03,
            [new(37)] = EnemyProjectileFrameId.Projectile86Ca22Frame00,
            [new(38)] = EnemyProjectileFrameId.Projectile86Ca22Frame01,
            [new(39)] = EnemyProjectileFrameId.Projectile86Ca22Frame02,
            [new(40)] = EnemyProjectileFrameId.Projectile86Ca22Frame03,
            [new(41)] = EnemyProjectileFrameId.Projectile86Ca22Frame04,
            [new(42)] = EnemyProjectileFrameId.Projectile86Ca22Frame05,
            [new(43)] = EnemyProjectileFrameId.Projectile86Ca22Frame06,
            [new(44)] = EnemyProjectileFrameId.Projectile86Ca22Frame07,
            [new(45)] = EnemyProjectileFrameId.Projectile86Caa4Frame00,
            [new(46)] = EnemyProjectileFrameId.Projectile86Caa4Frame01,
            [new(47)] = EnemyProjectileFrameId.Projectile86Caa4Frame02,
            [new(48)] = EnemyProjectileFrameId.Projectile86Caa4Frame03,
            [new(49)] = EnemyProjectileFrameId.Projectile86Caa4Frame04,
            [new(50)] = EnemyProjectileFrameId.Projectile86Caa4Frame05,
            [new(51)] = EnemyProjectileFrameId.Projectile86Caa4Frame06,
            [new(52)] = EnemyProjectileFrameId.Projectile86Caa4Frame07,
            [new(53)] = EnemyProjectileFrameId.Projectile86Cac8Frame00,
            [new(54)] = EnemyProjectileFrameId.Projectile86Cac8Frame01,
            [new(55)] = EnemyProjectileFrameId.Projectile86Cac8Frame02,
            [new(56)] = EnemyProjectileFrameId.Projectile86Cac8Frame03,
            [new(57)] = EnemyProjectileFrameId.Projectile86Cac8Frame04,
            [new(58)] = EnemyProjectileFrameId.Projectile86Cac8Frame05,
            [new(59)] = EnemyProjectileFrameId.Projectile86Cac8Frame06,
            [new(60)] = EnemyProjectileFrameId.Projectile86Cac8Frame07,
            [new(61)] = EnemyProjectileFrameId.Projectile86Cb0dFrame00,
        };
}
