using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>Cartridge values and helpers of <see cref="TourianStatueProjectileInstructionProgramDefinitionsTooling"/> that only verification reads.</summary>
internal static class TourianStatueProjectileInstructionProgramDefinitionsToolingConstants
{
    /// <summary>Matches either byte of a statue projectile mechanics word in bank $86.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < TourianStatueProjectileInstructionProgramDefinitionsTooling.MechanicsWordCount; index++)
        {
            var word = TourianStatueProjectileInstructionProgramDefinitionsTooling.MechanicsWord(index);
            if (bankAddress == word.Address || bankAddress == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}
