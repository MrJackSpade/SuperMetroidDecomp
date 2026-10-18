namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Operand resolution for the shared bank-$86 visual scans in the development audits.</summary>
internal static class SkreeMetareeParticleVisualDefinitionsTooling
{
    /// <summary>Makes particle operand resolution available to the shared bank-$86 visual audit.</summary>
    extension(SkreeMetareeParticleVisualDefinitions)
    {
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
            composition = SkreeMetareeParticleVisualDefinitions.Resolve((SkreeMetareeParticleOperand)operandAddress);
            return true;
        }

        /// <summary>Maps the typed Skree or Metaree selector to its bank-$8D composition pointer.</summary>
        internal static ushort Resolve(SkreeMetareeParticleOperand operand) => operand switch
        {
            SkreeMetareeParticleOperand.Skree => SkreeMetareeParticleVisualDefinitions.SkreeComposition,
            SkreeMetareeParticleOperand.Metaree => SkreeMetareeParticleVisualDefinitions.MetareeComposition,
            _ => throw new InvalidOperationException($"Undefined Skree/Metaree visual operand {operand}."),
        };
    }
}
