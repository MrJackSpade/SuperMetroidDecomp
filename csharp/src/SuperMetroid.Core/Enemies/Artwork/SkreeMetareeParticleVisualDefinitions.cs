namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Stable visual-frame identities for the two one-frame bank-$86 Skree/Metaree
/// projectile programs. The compositions themselves live in editable asset JSON.
/// </summary>
internal static class SkreeMetareeParticleVisualDefinitions
{
    /// <summary>Bank-$8D Skree-debris composition selected by $86:8ABF.</summary>
    internal const ushort SkreeComposition = 0x8023;

    /// <summary>Bank-$8D Metaree-debris composition selected by $86:8AC7.</summary>
    internal const ushort MetareeComposition = 0x8435;
}
