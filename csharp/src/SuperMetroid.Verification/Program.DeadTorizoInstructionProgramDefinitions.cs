using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static SuiteTimingVerification;

internal static class DeadTorizoInstructionProgramDefinitionsVerification
{
    internal static void VerifyDeadTorizoInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyDeadTorizoInstructionProgramDefinitions), () => VerifyDeadTorizoInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    internal static void VerifyDeadTorizoInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int index = 0;
             index < DeadTorizoInstructionProgramDefinitionsTooling.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                DeadTorizoInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa90000 | definition.Address),
                $"Dead Torizo mechanics word $A9:{definition.Address:X4}");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        bool sawPresentationWord = false;
        var guard = NewDeadTorizoInstructionReadGuard(rom, () => sawPresentationWord = true);
        var enemies = new RoomEnemySystem { TileArtwork = RepositoryInstallation.EnemyTiles };
        Type type = typeof(RoomEnemySystem);
        type.GetField("_bus", flags)!.SetValue(enemies, guard);
        var initialize = type.GetMethod("InitializeDeadTorizo", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies);
        MethodInfo process = type.GetMethod("ProcessInstructions", flags)!;

        RoomEnemySlot corpse = enemies.Slots[0];
        corpse.EnemyDefinitionPointer = EnemyDefinitionId.CorpseTorizo;
        corpse.Definition = default(RoomEnemyDefinition) with { Bank = 0xa9 };
        initialize(corpse);
        AssertEqual(DeadTorizoInstructionProgramDefinitions.Stationary,
            corpse.CurrentInstruction,
            "real Dead Torizo initializer installs compiled stationary program");

        object?[] processArguments =
            [corpse, null, null, Position16<RoomPixels>.Zero, (ushort)0];
        corpse.InstructionTimer = 1;
        process.Invoke(enemies, processArguments);
        corpse.InstructionTimer = 1;
        process.Invoke(enemies, processArguments);
        AssertEqual(DeadTorizoInstructionProgramDefinitions.SleepOpcode,
            corpse.CurrentInstruction,
            "Dead Torizo program reaches terminal sleep");
        AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa90000 | DeadTorizoInstructionProgramDefinitionsTooling.PresentationWord),
            corpse.SpritemapPointer,
            "compiled Dead Torizo spritemap selector matches the cartridge");
        AssertTrue(!sawPresentationWord,
            "Dead Torizo execution uses the compiled spritemap selector");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids both compiled Dead Torizo mechanics words");
        AssertThrows<InvalidDataException>(
            () => DeadTorizoInstructionProgramDefinitions.ReadMechanicsWord(
                DeadTorizoInstructionProgramDefinitionsTooling.PresentationWord),
            "Dead Torizo spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => DeadTorizoInstructionProgramDefinitions.ReadMechanicsWord(
                DeadTorizoInstructionProgramDefinitions.FirstAdjacentPresentationData),
            "adjacent Dead Torizo spritemap data is rejected as instruction mechanics");

        _ = ProbeDeadTorizoInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeDeadTorizoInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Dead Torizo allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Dead Torizo mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Dead Torizo instruction mechanics: two compiled words, the real initializer, " +
            "terminal sleep, and the compiled spritemap selector pass with mechanics bytes forbidden.");
    }

    private static int ProbeDeadTorizoInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += DeadTorizoInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? DeadTorizoInstructionProgramDefinitions.Stationary
                    : DeadTorizoInstructionProgramDefinitions.SleepOpcode);
        }
        return checksum;
    }

    private static CartridgeReadGuard NewDeadTorizoInstructionReadGuard(ISnesAddressSpace source,
        Action sawPresentationWord) =>
        new CartridgeReadGuard(source)
            .Forbid(DeadTorizoInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Dead Torizo mechanics byte ${address:X6}.")
            .Observe(address =>
                {
                    int presentation =
                        0xa90000 | DeadTorizoInstructionProgramDefinitionsTooling.PresentationWord;
                    return address == presentation || address == presentation + 1;
                },
                _ => sawPresentationWord());

    [VerificationCommand("--dead-torizo-instruction-mechanics")]
    internal static int DeadTorizoInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyDeadTorizoInstructionProgramDefinitions), () => VerifyDeadTorizoInstructionProgramDefinitions());
        return 0;
    }
}
