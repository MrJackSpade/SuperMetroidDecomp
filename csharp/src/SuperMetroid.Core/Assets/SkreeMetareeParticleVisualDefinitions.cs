namespace SuperMetroid.Core.Assets;

/// <summary>
/// Stable visual-frame identities for the two one-frame bank-$86 Skree/Metaree
/// projectile programs. The compositions themselves live in editable asset JSON.
/// </summary>
internal static class SkreeMetareeParticleVisualDefinitions
{
    /// <summary>Spritemap operand of <c>InstList_EnemyProjectile_MetalSkreeParticle</c> at $86:8ABF.</summary>
    internal const ushort SkreeOperand = 0x8abf;

    /// <summary>Spritemap operand of <c>InstList_EnemyProjectile_MetareeParticle</c> at $86:8AC7.</summary>
    internal const ushort MetareeOperand = 0x8ac7;

    /// <summary>Bank-$8D Skree-debris composition selected by $86:8ABF.</summary>
    internal const ushort SkreeComposition = 0x8023;

    /// <summary>Bank-$8D Metaree-debris composition selected by $86:8AC7.</summary>
    internal const ushort MetareeComposition = 0x8435;

    internal static ushort Resolve(ushort operandAddress) => operandAddress switch
    {
        SkreeOperand => SkreeComposition,
        MetareeOperand => MetareeComposition,
        _ => throw new InvalidDataException(
            $"Skree/Metaree visual operand $86:{operandAddress:X4} is not catalogued."),
    };
}
