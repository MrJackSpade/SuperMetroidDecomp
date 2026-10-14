using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>Verification access to <see cref="SkreeMetareeInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class SkreeMetareeInstructionProgramDefinitionsAccess
{
    /// <summary>Recognizes either byte of compiled bank-$A3 Skree or Metaree mechanics words.</summary>
    extension(SkreeMetareeInstructionProgramDefinitions)
    {
        /// <summary>Recognizes either byte of the compiled Skree or Metaree mechanics words.</summary>
        internal static bool IsCompiledMechanicsByte(int address)
        {
            if ((SnesAddressMath.BankBase(address)) != 0xa30000) return false;
            ushort bankAddress = unchecked((ushort)address);
            for (int species = 0; species < 2; species++)
            for (int index = 0; index < 20; index++)
            {
                ushort word = SkreeMetareeInstructionProgramDefinitions.MechanicsWord(species == 0, index).Address;
                if (bankAddress == word || bankAddress == word + 1) return true;
            }
            return false;
        }
    }
}
