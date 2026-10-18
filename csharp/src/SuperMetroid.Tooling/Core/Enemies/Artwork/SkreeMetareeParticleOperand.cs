namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Spritemap operands of the two one-frame bank-$86 Skree/Metaree particle programs.</summary>
internal enum SkreeMetareeParticleOperand : ushort
{
    /// <summary>Spritemap operand of <c>InstList_EnemyProjectile_MetalSkreeParticle</c> at $86:8ABF.</summary>
    Skree = 0x8abf,

    /// <summary>Spritemap operand of <c>InstList_EnemyProjectile_MetareeParticle</c> at $86:8AC7.</summary>
    Metaree = 0x8ac7,
}
