namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>
/// The Skree's and Metaree's death-particle animations (#641): one segment per animation a spawn, branch or family step enters, in authored order.
/// Their steps are <see cref="SkreeMetareeParticleAnimations"/>.
/// </summary>
internal enum SkreeMetareeParticleSegment
{
    /// <summary>A piece of the Skree's debris, held.</summary>
    Skree,
    /// <summary>A piece of the Metaree's debris, held.</summary>
    Metaree,
}
