using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>Cartridge values and helpers of <see cref="SpacePirateProjectileInstructionProgramDefinitionsTooling"/> that only verification reads.</summary>
internal static class SpacePirateProjectileInstructionProgramDefinitionsToolingConstants
{
    /// <summary>Matches either byte of a compiled pirate projectile word in bank $86.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < SpacePirateProjectileInstructionProgramDefinitionsTooling.MechanicsWordCount; index++)
        {
            ushort wordAddress = SpacePirateProjectileInstructionProgramDefinitionsTooling.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
