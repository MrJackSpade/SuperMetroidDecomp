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
using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static SuiteTimingVerification;

internal static class SkreeMetareeInstructionProgramDefinitionsVerification
{
    internal static void VerifySkreeMetareeInstructionProgramDefinitions()
    {
        Suite(nameof(VerifySkreeMetareeInstructionProgramDefinitions), () => VerifySkreeMetareeInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    internal static void VerifySkreeMetareeInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int species = 0; species < 2; species++)
        {
            bool metaree = species == 0;
            for (int index = 0;
                 index < SkreeMetareeInstructionProgramDefinitionsTooling.MechanicsWordCount();
                 index++)
            {
                InstructionMechanicsWord definition =
                    SkreeMetareeInstructionProgramDefinitions.MechanicsWord(metaree, index);
                AssertEqual(definition.Value,
                    CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa30000 | definition.Address)),
                    $"{(metaree ? "Metaree" : "Skree")} mechanics $A3:{definition.Address:X4}");
            }
        }

        var guard = NewSkreeMetareeInstructionProgramReadGuard(rom);
        Suite(nameof(VerifySpecies), () => VerifySpecies(metaree: true));
        Suite(nameof(VerifySpecies), () => VerifySpecies(metaree: false));

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "Skree/Metaree programs never read installed visual selectors");
        for (int species = 0; species < 2; species++)
        {
            bool metaree = species == 0;
            for (int index = 0;
                 index < SkreeMetareeInstructionProgramDefinitionsTooling.PresentationWordCount();
                 index++)
            {
                ushort address =
                    SkreeMetareeInstructionProgramDefinitionsTooling.PresentationWordAddress(
                        metaree, index);
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa30000 | address)),
                    EnemySpritemapDefinitions.SkreeMetareeFrameAt(metaree, address),
                    $"compiled {(metaree ? "Metaree" : "Skree")} frame $A3:{address:X4}");
            }
        }
        AssertThrows<InvalidDataException>(
            () => EnemySpritemapDefinitions.SkreeMetareeFrameAt(true, 0xc660),
            "Metaree rejects a Skree visual operand");
        AssertThrows<InvalidDataException>(
            () => EnemySpritemapDefinitions.SkreeMetareeFrameAt(false, 0x8912),
            "Skree rejects a Metaree visual operand");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production avoids every compiled Skree/Metaree mechanics byte");

        AssertThrows<InvalidDataException>(
            () => SkreeMetareeInstructionProgramDefinitions.ReadMetareeMechanicsWord(0x8912),
            "Metaree spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => SkreeMetareeInstructionProgramDefinitions.ReadSkreeMechanicsWord(0xc660),
            "Skree spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => SkreeMetareeInstructionProgramDefinitions.ReadMetareeMechanicsWord(0xc65e),
            "Metaree cannot enter Skree's program domain");

        _ = ProbeSkreeMetareeInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeSkreeMetareeInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Skree/Metaree allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Skree/Metaree lookups allocate no per-frame storage");

        Console.WriteLine(
            "Skree/Metaree instruction mechanics: forty compiled words, all eight " +
            "programs and property/completion callbacks pass with all twenty-two " +
            "visual-selector and mechanics source words forbidden.");

        void VerifySpecies(bool metaree)
        {
            foreach (SkreeMetareeAnimationPhase phase in Enum.GetValues<SkreeMetareeAnimationPhase>())
            {
                RoomEnemySystem enemies = CreateSystem(metaree, phase, out RoomEnemySlot slot);
                RunProgram(enemies, slot, phase switch
                {
                    SkreeMetareeAnimationPhase.Idling => 45,
                    SkreeMetareeAnimationPhase.PreparingAttack => 26,
                    SkreeMetareeAnimationPhase.Diving => 10,
                    SkreeMetareeAnimationPhase.StopAnimating => 2,
                    _ => throw new InvalidDataException(),
                });

                if (phase == SkreeMetareeAnimationPhase.PreparingAttack)
                {
                    bool ready = metaree
                        ? enemies.MetareeStates[0]!.AttackReady
                        : enemies.SkreeStates[0]!.AttackReady;
                    AssertTrue(ready, $"{SpeciesName(metaree)} preparation publishes ready flag");
                }
                if (phase == SkreeMetareeAnimationPhase.Diving)
                    AssertTrue(slot.Properties.HasAny(EnemyProperties.ProcessOffScreen),
                        $"{SpeciesName(metaree)} diving enables off-screen processing");
                if (phase == SkreeMetareeAnimationPhase.StopAnimating)
                    AssertTrue(!slot.Properties.HasAny(EnemyProperties.ProcessOffScreen),
                        $"{SpeciesName(metaree)} stop program disables off-screen processing");
            }
        }

        RoomEnemySystem CreateSystem(
            bool metaree,
            SkreeMetareeAnimationPhase phase,
            out RoomEnemySlot slot)
        {
            const BindingFlags flags =
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(guard);
            slot = enemies.SlotTable.Slots[0];
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa3 };

            if (metaree)
            {
                slot.EnemyDefinitionPointer = EnemyDefinitionId.Metaree;
                enemies.MetareeBehavior.InitializeMetaree(slot);
                MetareeEnemyState state = enemies.MetareeStates[0]!;
                state.InstalledInstructionListIndex = phase == SkreeMetareeAnimationPhase.Idling
                    ? SkreeMetareeAnimationPhase.PreparingAttack
                    : SkreeMetareeAnimationPhase.Idling;
                state.RequestedInstructionListIndex = phase;
                state.AttackReady = false;
                MetareeBehavior.InstallRequestedMetareeInstruction(slot, state);
            }
            else
            {
                slot.EnemyDefinitionPointer = EnemyDefinitionId.Skree;
                enemies.SkreeBehavior.InitializeSkree(slot);
                SkreeEnemyState state = enemies.SkreeStates[0]!;
                state.InstalledInstructionIndex = phase == SkreeMetareeAnimationPhase.Idling
                    ? SkreeMetareeAnimationPhase.PreparingAttack
                    : SkreeMetareeAnimationPhase.Idling;
                state.RequestedInstructionIndex = phase;
                state.AttackReady = false;
                SkreeBehavior.InstallRequestedSkreeInstruction(slot, state);
            }
            return enemies;
        }

        static void RunProgram(RoomEnemySystem enemies, RoomEnemySlot slot, int frames)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            object?[] arguments =
                [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];
            for (int frame = 0; frame < frames; frame++)
                EnemyInstructionRunner.Process(enemies, arguments);
        }
    }

    private static string SpeciesName(bool metaree) => metaree ? "Metaree" : "Skree";

    private static int ProbeSkreeMetareeInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += SkreeMetareeInstructionProgramDefinitions.ReadMetareeMechanicsWord(
                SkreeMetareeInstructionProgramDefinitions.MetareeIdling);
            checksum += SkreeMetareeInstructionProgramDefinitions.ReadSkreeMechanicsWord(
                SkreeMetareeInstructionProgramDefinitions.SkreeIdling);
        }
        return checksum;
    }

    private static CartridgeReadGuard NewSkreeMetareeInstructionProgramReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(SkreeMetareeInstructionProgramDefinitions.IsCompiledMechanicsByte,
                address => $"Production read compiled Skree/Metaree mechanics byte ${address:X6}.")
            .ForbidWords(0xa30000, SkreeMetareeInstructionProgramDefinitionsTooling.PresentationWordCount(),
                index => SkreeMetareeInstructionProgramDefinitionsTooling.PresentationWordAddress(true, index),
                address => $"Production read installed Skree/Metaree selector ${address:X6}.")
            .ForbidWords(0xa30000, SkreeMetareeInstructionProgramDefinitionsTooling.PresentationWordCount(),
                index => SkreeMetareeInstructionProgramDefinitionsTooling.PresentationWordAddress(false, index),
                address => $"Production read installed Skree/Metaree selector ${address:X6}.");

    [VerificationCommand("--skree-metaree-instruction-mechanics")]
    internal static int SkreeMetareeInstructionMechanicsCommand()
    {
        Suite(nameof(VerifySkreeMetareeInstructionProgramDefinitions), () => VerifySkreeMetareeInstructionProgramDefinitions());
        return 0;
    }
}
