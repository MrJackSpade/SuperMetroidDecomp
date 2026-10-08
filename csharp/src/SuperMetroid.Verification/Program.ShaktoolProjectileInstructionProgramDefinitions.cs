using SuperMetroid.Core.Assets;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyShaktoolProjectileInstructionProgramDefinitions() =>
        Suite(nameof(VerifyShaktoolProjectileInstructionProgramDefinitions), () => VerifyShaktoolProjectileInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));

    private static void VerifyShaktoolProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < ShaktoolProjectileInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                ShaktoolProjectileInstructionProgramDefinitions.MechanicsWord(index);
            ushort native = unchecked((ushort)(
                rom.ReadByte(EnemyProjectileCodePointers.BankBase | definition.Address) |
                rom.ReadByte(EnemyProjectileCodePointers.BankBase |
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

    private sealed class ShaktoolProjectileInstructionReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        internal HashSet<ushort> ObservedPresentationWords { get; } = [];
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if (ShaktoolProjectileInstructionProgramDefinitions.IsCompiledMechanicsByte(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production read compiled Shaktool mechanics byte ${address:X6}.");
            }
            if ((address & 0xff0000) == EnemyProjectileCodePointers.BankBase)
            {
                ushort bankAddress = unchecked((ushort)address);
                for (int index = 0;
                     index < ShaktoolProjectileInstructionProgramDefinitions.PresentationWordCount;
                     index++)
                {
                    ushort presentation = ShaktoolProjectileInstructionProgramDefinitions
                        .PresentationWordAddress(index);
                    if (bankAddress == presentation ||
                        bankAddress == unchecked((ushort)(presentation + 1)))
                    {
                        ObservedPresentationWords.Add(presentation);
                        break;
                    }
                }
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
