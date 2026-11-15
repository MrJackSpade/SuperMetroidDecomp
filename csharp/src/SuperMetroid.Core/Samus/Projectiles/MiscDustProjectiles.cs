namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The animation of each miscellaneous dust projectile.</summary>
internal static class MiscDustProjectiles
{
    /// <summary>The animation each misc-dust selector starts.</summary>
    internal static MiscDustProjectileSegment Animation(MiscDustAnimation animation) => animation switch
    {
        MiscDustAnimation.MotherBrainElbowChargeParticle => MiscDustProjectileSegment.ElbowChargeParticle,
        MiscDustAnimation.MotherBrainElbowChargeEnergy => MiscDustProjectileSegment.ElbowChargeEnergy,
        MiscDustAnimation.SmallExplosion => MiscDustProjectileSegment.SmallExplosion,
        MiscDustAnimation.MotherBrainRainbowBeamExplosion => MiscDustProjectileSegment.RainbowBeamExplosion,
        MiscDustAnimation.DudShotTinyExplosion => MiscDustProjectileSegment.DudShot,
        MiscDustAnimation.SmallDustCloud => MiscDustProjectileSegment.SmallDustCloud,
        MiscDustAnimation.CorpseDustCloud => MiscDustProjectileSegment.CorpseDustCloud,
        MiscDustAnimation.EyeDoorSmokePuff => MiscDustProjectileSegment.EyeDoorSmokePuff,
        MiscDustAnimation.Smoke => MiscDustProjectileSegment.Smoke,
        MiscDustAnimation.RockParticles => MiscDustProjectileSegment.RockParticles,
        MiscDustAnimation.ShortBigDustCloud => MiscDustProjectileSegment.ShortBigDustCloud,
        MiscDustAnimation.BigDustCloud => MiscDustProjectileSegment.BigDustCloud,
        MiscDustAnimation.LongDraygonBreathBubbles => MiscDustProjectileSegment.DraygonBreathBubbles,
        MiscDustAnimation.BigExplosion => MiscDustProjectileSegment.BigExplosion,
        _ => throw new ArgumentOutOfRangeException(nameof(animation), animation, "Undefined misc-dust animation."),
    };
}
