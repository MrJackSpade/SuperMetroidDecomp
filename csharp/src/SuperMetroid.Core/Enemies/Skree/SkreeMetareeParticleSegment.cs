namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>
/// The Skree's and Metaree's death-particle programs (#641), one per animation a spawn, branch or family step enters, in authored order.
/// Their steps are <see cref="SkreeMetareeParticleAnimations"/>.
/// </summary>
internal enum SkreeMetareeParticleSegment
{
    /// <summary>The Skree animation.</summary>
    Skree,
    /// <summary>The metaree animation.</summary>
    Metaree,
}
