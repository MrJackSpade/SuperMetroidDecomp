using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Verification's decode of raw Skree/Metaree particle operands.</summary>
internal static class SkreeMetareeParticleVisualDefinitionsAccess
{
    /// <summary>Decodes a raw bank-$86 operand, rejecting addresses outside the two particle programs.</summary>
    internal static ushort Resolve(ushort operandAddress) =>
        SkreeMetareeParticleVisualDefinitionsTooling.Resolve(ClosedNativeWords.Decode<SkreeMetareeParticleOperand>(operandAddress, "Skree/Metaree visual operand"));
}
