using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Verification access to <see cref="CeresBabyInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class CeresBabyInstructionProgramDefinitionsAccess
{
    /// <summary>Identifies compiled Ceres Baby spritemap and palette operand bytes for ROM-read guards.</summary>
    extension(CeresBabyInstructionProgramDefinitions)
    {
        /// <summary>Identifies either byte of a compiled Baby spritemap operand in bank $A6.</summary>
        internal static bool IsCompiledSpritemapByte(int address)
        {
            if ((SnesAddressMath.BankBase(address)) != 0xa60000)
                return false;
            ushort bankAddress = unchecked((ushort)address);
            for (int index = 0; index < CeresBabyInstructionProgramDefinitions.SpritemapOperandCount; index++)
            {
                ushort operand = CeresBabyInstructionProgramDefinitions.SpritemapOperandAddress(index);
                if (bankAddress == operand || bankAddress == unchecked((ushort)(operand + 1)))
                    return true;
            }
            return false;
        }

        /// <summary>Identifies either byte of a compiled Baby palette operand in bank $A6.</summary>
        internal static bool IsCompiledPaletteByte(int address)
        {
            if ((SnesAddressMath.BankBase(address)) != 0xa60000)
                return false;
            ushort bankAddress = unchecked((ushort)address);
            for (int index = 0; index < CeresBabyInstructionProgramDefinitions.PaletteOperandCount; index++)
            {
                ushort operand = CeresBabyInstructionProgramDefinitions.PaletteOperandAddress(index);
                if (bankAddress == operand || bankAddress == unchecked((ushort)(operand + 1)))
                    return true;
            }
            return false;
        }
    }
}
