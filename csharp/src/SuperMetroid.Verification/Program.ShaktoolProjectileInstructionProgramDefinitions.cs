using SuperMetroid.Core.Assets;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyShaktoolProjectileInstructionProgramDefinitions() =>
        Suite(nameof(VerifyShaktoolProjectileInstructionProgramDefinitions), () => VerifyShaktoolProjectileInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));

    [CpuBusBoundary("exact bus test: retail ROM oracle for Shaktool projectile instruction program definitions")]
    private static void VerifyShaktoolProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < ShaktoolProjectileInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                ShaktoolProjectileInstructionProgramDefinitionsTooling.MechanicsWord(index);
            ushort native = unchecked((ushort)(
                rom.ReadCartridgeByte(EnemyProjectileCodePointers.BankBase | definition.Address) |
                rom.ReadCartridgeByte(EnemyProjectileCodePointers.BankBase |
                    unchecked((ushort)(definition.Address + 1))) << 8));
            AssertEqual(definition.Value, native,
                $"Shaktool attack-circle mechanics word $86:{definition.Address:X4}");
        }
    }

    private static int ProbeShaktoolProjectileInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += ShaktoolProjectileInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? ShaktoolProjectileInstructionProgramDefinitions.Front
                    : ShaktoolProjectileInstructionProgramDefinitions.Back);
        }
        return checksum;
    }

    private static CartridgeReadGuard NewShaktoolProjectileInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(ShaktoolProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Shaktool mechanics byte ${address:X6}.")
            .ObserveWords(EnemyProjectileCodePointers.BankBase, ShaktoolProjectileInstructionProgramDefinitions.PresentationWordCount, ShaktoolProjectileInstructionProgramDefinitions.PresentationWordAddress);
}
