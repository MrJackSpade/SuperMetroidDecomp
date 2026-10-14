using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Assets;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static SuiteTimingVerification;

/// <summary>Compares Shaktool projectile control words with bank-$86 retail data and guards production reads.</summary>
internal static class ShaktoolProjectileInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge for the focused Shaktool attack-circle program comparison.</summary>
    internal static void VerifyShaktoolProjectileInstructionProgramDefinitions() =>
        Suite(nameof(VerifyShaktoolProjectileInstructionProgramDefinitions), () => VerifyShaktoolProjectileInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));

    /// <summary>Checks every compiled mechanics word against its exact native bank-$86 address.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Shaktool projectile instruction program definitions")]
    internal static void VerifyShaktoolProjectileInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < ShaktoolProjectileInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                ShaktoolProjectileInstructionProgramDefinitionsTooling.MechanicsWord(index);
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(EnemyProjectileCodePointers.BankBase | definition.Address);
            AssertEqual(definition.Value, native,
                $"Shaktool attack-circle mechanics word $86:{definition.Address:X4}");
        }
    }

    /// <summary>Repeatedly reads the front and back compiled entry words for allocation measurement.</summary>
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

    /// <summary>Forbids production reads of compiled control bytes while observing artwork operand reads.</summary>
    private static CartridgeReadGuard NewShaktoolProjectileInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(ShaktoolProjectileInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Shaktool mechanics byte ${address:X6}.")
            .ObserveWords(EnemyProjectileCodePointers.BankBase, ShaktoolProjectileInstructionProgramDefinitions.PresentationWordCount, ShaktoolProjectileInstructionProgramDefinitions.PresentationWordAddress);

    /// <summary>Runs the focused attack-circle program comparison from the verification command line.</summary>
    [VerificationCommand("--shaktool-projectile-instruction-mechanics")]
    internal static int ShaktoolProjectileInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyShaktoolProjectileInstructionProgramDefinitions), () => VerifyShaktoolProjectileInstructionProgramDefinitions());
        return 0;
    }
}
