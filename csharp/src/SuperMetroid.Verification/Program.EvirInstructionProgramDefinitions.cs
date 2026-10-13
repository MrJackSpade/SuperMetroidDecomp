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
using static CompiledEnemyVisualSelectorAssertionsVerification;
using static SuiteTimingVerification;

/// <summary>Checks compiled Evir body, arms, and projectile instruction streams.</summary>
internal static class EvirInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge for Evir instruction comparison.</summary>
    internal static void VerifyEvirInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyEvirInstructionProgramDefinitions), () => VerifyEvirInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks native words, guarded loops, body-owned regeneration callbacks, sprite selectors, and allocation-free reads.</summary>
    internal static void VerifyEvirInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        (ushort Entry, int Frames, EnemyDefinitionId Definition, string Name)[] loops =
        [
            (EvirInstructionProgramDefinitions.BodyFacingLeft,
                EvirInstructionProgramDefinitions.BodyFrameCount,
                EnemyDefinitionId.Evir,
                "left body"),
            (EvirInstructionProgramDefinitions.ArmsFacingLeft,
                EvirInstructionProgramDefinitions.ArmsFrameCount,
                EnemyDefinitionId.Evir,
                "left arms"),
            (EvirInstructionProgramDefinitions.BodyFacingRight,
                EvirInstructionProgramDefinitions.BodyFrameCount,
                EnemyDefinitionId.Evir,
                "right body"),
            (EvirInstructionProgramDefinitions.ArmsFacingRight,
                EvirInstructionProgramDefinitions.ArmsFrameCount,
                EnemyDefinitionId.Evir,
                "right arms"),
        ];

        AssertEqual(67, EvirInstructionProgramDefinitions.MechanicsWordCount,
            "Evir compiled mechanics word count");
        AssertEqual(49, EvirInstructionProgramDefinitions.PresentationWordCount,
            "Evir live presentation word count");
        for (int index = 0;
             index < EvirInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                EvirInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa80000 | definition.Address),
                $"Evir mechanics word $A8:{definition.Address:X4}");
        }

        var guard = NewEvirInstructionGuard(rom);
        foreach ((ushort entry, int frameCount, EnemyDefinitionId definition, string name) in loops)
        {
            var enemies = NewEvirInstructionSystem(guard);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            PrepareEvirInstructionSlot(slot, definition, entry);
            object?[] arguments =
                [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];
            for (int call = 0; call < frameCount + 1; call++)
            {
                slot.InstructionTimer = 1;
                EnemyInstructionRunner.Process(enemies, arguments);
            }
            AssertEqual(unchecked((ushort)(entry + 4)), slot.CurrentInstruction,
                $"Evir {name} completes its loop and loads the first frame");
        }

        {
            var enemies = NewEvirInstructionSystem(guard);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            PrepareEvirInstructionSlot(
                slot,
                EnemyDefinitionId.EvirProjectile,
                EvirInstructionProgramDefinitions.ProjectileNormal);
            object?[] arguments =
                [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];
            EnemyInstructionRunner.Process(enemies, arguments);
            EnemyInstructionRunner.Process(enemies, arguments);
            AssertEqual(unchecked((ushort)(
                    EvirInstructionProgramDefinitions.ProjectileNormal + 4)),
                slot.CurrentInstruction,
                "normal Evir projectile reaches terminal sleep");
        }

        {
            var enemies = NewEvirInstructionSystem(guard);
            // The regeneration instructions read Evir.instList-$80,X: the body record two
            // slots before the projectile, as the retail population lays them out.
            RoomEnemySlot slot = enemies.SlotTable.Slots[2];
            PrepareEvirInstructionSlot(
                slot,
                EnemyDefinitionId.EvirProjectile,
                EvirInstructionProgramDefinitions.ProjectileRegenerating);
            // #1269: the projectile's own facing word disagrees with its body on purpose;
            // $A8:879B/$87B6 follow the body's installed list, so the offset starts at +8.
            var state = new EvirEnemyState(slot)
            {
                FacingDirection = 1,
                MovingFlag = 1,
                RegenerationFlag = 1,
                Function = EvirAiFunction.ProjectileRegenerating,
            };
            var states = (EvirEnemyState?[])enemies.EvirBehavior._evirStates;
            states[2] = state;
            states[0] = new EvirEnemyState(enemies.SlotTable.Slots[0])
            {
                InstalledInstructionList = EvirInstructionProgramDefinitions.BodyFacingLeft,
            };
            object?[] arguments =
                [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];
            for (int call = 0; call < 10; call++)
            {
                slot.InstructionTimer = 1;
                EnemyInstructionRunner.Process(enemies, arguments);
                if (call == 0)
                    AssertEqual((ushort)8, state.RegenerationXOffset,
                        "a left-facing body sets the +8 offset whatever the projectile's facing word");
            }
            AssertEqual(unchecked((ushort)(
                    EvirInstructionProgramDefinitions.ProjectileRegenerationLoop + 16)),
                slot.CurrentInstruction,
                "regenerating Evir projectile reaches terminal sleep");
            AssertEqual((ushort)0, state.RegenerationXOffset,
                "regeneration loop advances the projectile mouth offset eight times");
            AssertEqual((ushort)0, state.RegenerationFlag,
                "regeneration finish callback clears regeneration ownership");
            AssertEqual((ushort)0, state.MovingFlag,
                "regeneration finish callback leaves the projectile stationary");
            AssertEqual(EvirAiFunction.ProjectileIdle, state.Function,
                "regeneration finish callback restores idle AI");
            AssertEqual((ushort)0x005e, enemies.EvirBehavior.LastEvirSoundEffect,
                "regeneration script publishes the native spit sound");
        }

        AssertEqual(0, guard.ObservedWords.Count,
            "Evir execution uses compiled spritemap selectors");
        for (int index = 0;
             index < EvirInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = EvirInstructionProgramDefinitions.PresentationWordAddress(index);
            AssertCompiledEnemyVisualSelector(rom, (byte)0xa8,
                address, $"Evir $A8:{address:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids every compiled Evir mechanics byte");

        AssertThrows<InvalidDataException>(
            () => EvirInstructionProgramDefinitions.ReadMechanicsWord(
                EvirInstructionProgramDefinitions.PresentationWordAddress(0)),
            "Evir spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => EvirInstructionProgramDefinitions.ReadMechanicsWord(
                EvirInstructionProgramDefinitions.AdjacentCallbackCode),
            "adjacent Evir callback code is rejected as mechanics");

        _ = ProbeEvirInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeEvirInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Evir allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Evir mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Evir instruction mechanics: 67 compiled words, all six body/arms/projectile " +
            "programs, complete regeneration callbacks, and 49 compiled spritemap selectors pass.");
    }

    /// <summary>Creates an enemy system bound to the supplied guarded address space.</summary>
    internal static RoomEnemySystem NewEvirInstructionSystem(
        ISnesAddressSpace bus)
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(bus);
        return enemies;
    }

    /// <summary>Seeds an Evir body or projectile slot with its bank-$A8 definition and starting program.</summary>
    internal static void PrepareEvirInstructionSlot(
        RoomEnemySlot slot,
        EnemyDefinitionId definition,
        ushort entry)
    {
        slot.EnemyDefinitionPointer = definition;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa8 };
        slot.CurrentInstruction = entry;
        slot.InstructionTimer = 1;
    }

    /// <summary>Repeatedly reads body and regeneration control words for steady-state allocation measurement.</summary>
    private static int ProbeEvirInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += EvirInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? EvirInstructionProgramDefinitions.BodyFacingLeft
                    : EvirInstructionProgramDefinitions.ProjectileRegenerating);
        }
        return checksum;
    }

    /// <summary>Forbids compiled control reads and observes or forbids native spritemap operands.</summary>
    internal static CartridgeReadGuard NewEvirInstructionGuard(ISnesAddressSpace rom, bool forbidPresentation = false)
    {
        CartridgeReadGuard guard = new CartridgeReadGuard(rom).Forbid(
            EvirInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
            address => $"Production read compiled Evir mechanics byte ${address:X6}.");
        return forbidPresentation
            ? guard.ForbidWords(0xa80000, EvirInstructionProgramDefinitions.PresentationWordCount,
                EvirInstructionProgramDefinitions.PresentationWordAddress,
                address => $"Installed Evir read presentation byte ${address:X6}.")
            : guard.ObserveWords(0xa80000, EvirInstructionProgramDefinitions.PresentationWordCount,
                EvirInstructionProgramDefinitions.PresentationWordAddress);
    }

    /// <summary>Runs retail-cartridge Evir instruction verification.</summary>
    [VerificationCommand("--evir-instruction-program-definitions")]
    internal static int EvirInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyEvirInstructionProgramDefinitions), () => VerifyEvirInstructionProgramDefinitions());
        return 0;
    }
}
