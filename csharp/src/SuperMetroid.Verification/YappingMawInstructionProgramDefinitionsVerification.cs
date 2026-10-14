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

/// <summary>Retail-word and guarded-execution checks for Yapping Maw attack and cooldown programs.</summary>
internal static class YappingMawInstructionProgramDefinitionsVerification
{
    /// <summary>Loads retail data for the Yapping Maw instruction check.</summary>
    internal static void VerifyYappingMawInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyYappingMawInstructionProgramDefinitions), () => VerifyYappingMawInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Runs all attack and cooldown entries, checking sound, held-Samus offsets, and initial facing selection.</summary>
    internal static void VerifyYappingMawInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        AssertEqual(96, YappingMawInstructionProgramDefinitions.MechanicsWordCount,
            "Yapping Maw compiled mechanics word count");
        AssertEqual(52, YappingMawInstructionProgramDefinitions.PresentationWordCount,
            "Yapping Maw live presentation word count");
        for (int index = 0;
             index < YappingMawInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                YappingMawInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa80000 | definition.Address),
                $"Yapping Maw mechanics word $A8:{definition.Address:X4}");
        }

        var guard = NewYappingMawInstructionReadGuard(rom);
        for (int direction = 0; direction < 8; direction++)
        {
            ushort entry = YappingMawInstructionProgramDefinitions
                .AttackForDirection(direction);
            (RoomEnemySystem enemies, RoomEnemySlot slot, _) =
                NewYappingMawInstructionSystem(guard, entry);
            object?[] arguments =
                [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];
            RunYappingMawInstructionFrames(enemies, arguments, slot, count: 5);
            AssertEqual(unchecked((ushort)(entry + 4)), slot.CurrentInstruction,
                $"Yapping Maw attack direction {direction} completes its loop");
            AssertEqual((ushort?)0x002f, enemies.YappingMawBehavior.LastYappingMawSoundEffect,
                $"Yapping Maw attack direction {direction} publishes its sound");
        }

        (ushort Entry, int Calls, ushort Final, short X, short Y, string Name)[] cooldowns =
        [
            (YappingMawInstructionProgramDefinitions.CooldownFacingUpRight,
                6,
                unchecked((ushort)(
                    YappingMawInstructionProgramDefinitions.CooldownFacingUp + 6)),
                8, -8, "up-right"),
            (YappingMawInstructionProgramDefinitions.CooldownFacingUp,
                5,
                unchecked((ushort)(
                    YappingMawInstructionProgramDefinitions.CooldownFacingUp + 6)),
                0, -16, "up"),
            (YappingMawInstructionProgramDefinitions.CooldownFacingUpLeft,
                6,
                unchecked((ushort)(
                    YappingMawInstructionProgramDefinitions.CooldownFacingUpLeft + 12)),
                -8, -8, "up-left"),
            (YappingMawInstructionProgramDefinitions.CooldownFacingDownRight,
                6,
                unchecked((ushort)(
                    YappingMawInstructionProgramDefinitions.CooldownFacingDown + 6)),
                8, 8, "down-right"),
            (YappingMawInstructionProgramDefinitions.CooldownFacingDown,
                5,
                unchecked((ushort)(
                    YappingMawInstructionProgramDefinitions.CooldownFacingDown + 6)),
                0, 16, "down"),
            (YappingMawInstructionProgramDefinitions.CooldownFacingDownLeft,
                6,
                unchecked((ushort)(
                    YappingMawInstructionProgramDefinitions.CooldownFacingDownLeft + 12)),
                -8, 8, "down-left"),
        ];
        foreach ((ushort entry, int calls, ushort final, short x, short y, string name)
                 in cooldowns)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, YappingMawEnemyState state) =
                NewYappingMawInstructionSystem(guard, entry);
            object?[] arguments =
                [slot, null, null, Position16<RoomPixels>.Zero, (ushort)0];
            RunYappingMawInstructionFrames(enemies, arguments, slot, count: 1);
            AssertEqual(x, state.HeldSamusOffset.X,
                $"Yapping Maw {name} callback X offset");
            AssertEqual(y, state.HeldSamusOffset.Y,
                $"Yapping Maw {name} callback Y offset");
            RunYappingMawInstructionFrames(
                enemies,
                arguments,
                slot,
                count: calls - 1);
            AssertEqual(final, slot.CurrentInstruction,
                $"Yapping Maw {name} cooldown completes its loop");
            AssertEqual((ushort?)0x002f, enemies.YappingMawBehavior.LastYappingMawSoundEffect,
                $"Yapping Maw {name} cooldown publishes its sound");
        }

        Suite(nameof(VerifyYappingMawInitializerSelections), () => VerifyYappingMawInitializerSelections(guard));

        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids all compiled Yapping Maw mechanics and visual bytes");

        AssertThrows<InvalidDataException>(
            () => YappingMawInstructionProgramDefinitions.ReadMechanicsWord(
                YappingMawInstructionProgramDefinitions.PresentationWordAddress(0)),
            "Yapping Maw spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => YappingMawInstructionProgramDefinitions.ReadMechanicsWord(
                YappingMawInstructionProgramDefinitions.AdjacentAttackSelectorTable),
            "adjacent Yapping Maw direction table is rejected as mechanics");

        _ = ProbeYappingMawInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeYappingMawInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "Yapping Maw allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed Yapping Maw mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Yapping Maw instruction mechanics: 96 compiled words, all fourteen attack/" +
            "cooldown entries, seven callbacks, and 52 compiled visual selectors pass.");
    }

    /// <summary>Checks native parameter-two facing selection and linked body projectile setup.</summary>
    private static void VerifyYappingMawInitializerSelections(
        ISnesAddressSpace bus)
    {
        foreach ((ushort parameter2, ushort expected, string name) in new[]
        {
            ((ushort)0, YappingMawInstructionProgramDefinitions.AttackingFacingDown,
                "alternate/down"),
            ((ushort)1, YappingMawInstructionProgramDefinitions.AttackingFacingUp,
                "ordinary/up"),
        })
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(bus);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.EnemyDefinitionPointer = EnemyDefinitionId.YappingMaw;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa8 };
            slot.Position = slot.Position.WithWhole(new(128, 128));
            slot.Parameter1 = 128;
            slot.Parameter2 = parameter2;
            enemies.YappingMawBehavior.InitializeYappingMaw(slot);
            AssertEqual(expected, slot.CurrentInstruction,
                $"Yapping Maw {name} initializer program");
            YappingMawEnemyState state = enemies.YappingMawStates[0] ??
                throw new InvalidOperationException(
                    $"Yapping Maw {name} initializer did not publish typed state.");
            AssertEqual(4, state.BodyProjectiles.Count(projectile => projectile is not null),
                $"Yapping Maw {name} initializer body links");
            AssertTrue(state.RootSpriteObject is not null,
                $"Yapping Maw {name} initializer root sprite object");
        }
    }

    /// <summary>Creates a guarded Yapping Maw slot with a selected instruction entry and typed state.</summary>
    private static (
        RoomEnemySystem Enemies,
        RoomEnemySlot Slot,
        YappingMawEnemyState State) NewYappingMawInstructionSystem(
            ISnesAddressSpace bus,
            ushort entry)
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(bus);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.EnemyDefinitionPointer = EnemyDefinitionId.YappingMaw;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xa8 };
        slot.CurrentInstruction = entry;
        slot.InstructionTimer = NativeCountdown.Start(1);
        var state = new YappingMawEnemyState(slot);
        var states = (YappingMawEnemyState?[])enemies.YappingMawBehavior._yappingMawStates;
        states[0] = state;
        return (enemies, slot, state);
    }

    /// <summary>Forces each requested instruction call through the production enemy interpreter.</summary>
    private static void RunYappingMawInstructionFrames(
        RoomEnemySystem enemies,
        object?[] arguments,
        RoomEnemySlot slot,
        int count)
    {
        for (int call = 0; call < count; call++)
        {
            slot.InstructionTimer = NativeCountdown.Start(1);
            EnemyInstructionRunner.Process(enemies, arguments);
        }
    }

    /// <summary>Alternates warmed attack and cooldown control lookups for allocation accounting.</summary>
    private static int ProbeYappingMawInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += YappingMawInstructionProgramDefinitions.ReadMechanicsWord(
                (index & 1) == 0
                    ? YappingMawInstructionProgramDefinitions.AttackingFacingUp
                    : YappingMawInstructionProgramDefinitions.CooldownFacingDownLeft);
        }
        return checksum;
    }

    /// <summary>Rejects compiled Yapping Maw control and frame-selector reads during execution.</summary>
    private static CartridgeReadGuard NewYappingMawInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(YappingMawInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled Yapping Maw mechanics byte ${address:X6}.")
            .ForbidWords(0xa80000, YappingMawInstructionProgramDefinitions.PresentationWordCount, YappingMawInstructionProgramDefinitions.PresentationWordAddress,
                address => $"Production read compiled Yapping Maw visual byte ${address:X6}.");

    /// <summary>Runs the retail-backed Yapping Maw instruction migration command.</summary>
    [VerificationCommand("--yapping-maw-instruction-program-definitions")]
    internal static int YappingMawInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyYappingMawInstructionProgramDefinitions), () => VerifyYappingMawInstructionProgramDefinitions());
        return 0;
    }
}
