using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

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

    /// <summary>Decodes a raw bank-$86 operand, rejecting addresses outside the two particle programs.</summary>
    internal static ushort Resolve(ushort operandAddress) =>
        Resolve(ClosedNativeWords.Decode<SkreeMetareeParticleOperand>(operandAddress, "Skree/Metaree visual operand"));

    /// <summary>Maps the typed Skree or Metaree selector to its bank-$8D composition pointer.</summary>
    internal static ushort Resolve(SkreeMetareeParticleOperand operand) => operand switch
    {
        SkreeMetareeParticleOperand.Skree => SkreeComposition,
        SkreeMetareeParticleOperand.Metaree => MetareeComposition,
        _ => throw new InvalidOperationException($"Undefined Skree/Metaree visual operand {operand}."),
    };
}
