using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Cartridge values and helpers of <see cref="FakeKraidProjectileInstructionProgramDefinitionsTooling"/> that only verification reads.</summary>
internal static class FakeKraidProjectileInstructionProgramDefinitionsToolingConstants
{
    /// <summary>Matches the first and last two bytes of each six-byte pose in bank $86, leaving the spritemap operand editable.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase)
            return false;
        int offset = (ushort)address - FakeKraidProjectileInstructionProgramToolingDefinitions.Spit;
        return (uint)offset < 18 && offset % 6 is 0 or 1 or 4 or 5;
    }
}
