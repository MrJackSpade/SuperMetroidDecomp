namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>
/// The Skree's and Metaree's death-particle programs (#641), one per animation a spawn, branch or family routine enters, in authored order.
/// Their steps are <see cref="SkreeMetareeParticleAnimations"/>.
/// </summary>
internal enum SkreeMetareeParticleProgram
{
    /// <summary>The skree animation.</summary>
    Skree,
    /// <summary>The metaree animation.</summary>
    Metaree,
}
