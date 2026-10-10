using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Spritemap operands of the two one-frame bank-$86 Skree/Metaree particle programs.</summary>
internal enum SkreeMetareeParticleOperand : ushort
{
    /// <summary>Spritemap operand of <c>InstList_EnemyProjectile_MetalSkreeParticle</c> at $86:8ABF.</summary>
    Skree = 0x8abf,

    /// <summary>Spritemap operand of <c>InstList_EnemyProjectile_MetareeParticle</c> at $86:8AC7.</summary>
    Metaree = 0x8ac7,
}

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

    internal static ushort Resolve(ushort operandAddress) =>
        Resolve(ClosedNativeWords.Decode<SkreeMetareeParticleOperand>(operandAddress, "Skree/Metaree visual operand"));

    internal static ushort Resolve(SkreeMetareeParticleOperand operand) => operand switch
    {
        SkreeMetareeParticleOperand.Skree => SkreeComposition,
        SkreeMetareeParticleOperand.Metaree => MetareeComposition,
        _ => throw new InvalidOperationException($"Undefined Skree/Metaree visual operand {operand}."),
    };

    /// <summary>
    /// Resolves an operand handed over by a shared bank-$86 visual scan; operands outside the
    /// two particle programs belong to other owners.
    /// </summary>
    internal static bool TryResolve(ushort operandAddress, out ushort composition)
    {
        if (!Enum.IsDefined((SkreeMetareeParticleOperand)operandAddress))
        {
            composition = 0;
            return false;
        }
        composition = Resolve((SkreeMetareeParticleOperand)operandAddress);
        return true;
    }
}
