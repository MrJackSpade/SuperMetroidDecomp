namespace SuperMetroid.Core.Assets;

/// <summary>Operand resolution for the shared bank-$86 visual scans in the development audits.</summary>
internal static class SkreeMetareeParticleVisualDefinitionsTooling
{
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
    }
}
