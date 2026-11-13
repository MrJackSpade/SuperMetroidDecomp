using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The fourteen misc-dust explosion, smoke, dust and bubble animations (#641, #1686): one segment per animation a spawn, branch or family step enters, in authored order.
/// Generated from the verified animations; see <see cref="MiscDustProjectileAnimations"/>.
/// </summary>
internal enum MiscDustProjectileSegment
{
    /// <summary>The elbow charge particle animation.</summary>
    ElbowChargeParticle,
    /// <summary>The elbow charge energy animation.</summary>
    ElbowChargeEnergy,
    /// <summary>The small explosion animation.</summary>
    SmallExplosion,
    /// <summary>The rainbow beam explosion animation.</summary>
    RainbowBeamExplosion,
    /// <summary>The dud shot animation.</summary>
    DudShot,
    /// <summary>The small dust cloud animation.</summary>
    SmallDustCloud,
    /// <summary>The corpse dust cloud animation.</summary>
    CorpseDustCloud,
    /// <summary>The eye door sweat drop animation.</summary>
    EyeDoorSweatDrop,
    /// <summary>The smoke animation.</summary>
    Smoke,
    /// <summary>The big explosion animation.</summary>
    BigExplosion,
    /// <summary>The rock particles animation.</summary>
    RockParticles,
    /// <summary>The short big dust cloud animation.</summary>
    ShortBigDustCloud,
    /// <summary>The big dust cloud animation.</summary>
    BigDustCloud,
    /// <summary>The Draygon breath bubbles animation.</summary>
    DraygonBreathBubbles,
}
