using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies.YappingMaws.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.YappingMaws;
using SuperMetroid.Core.Programs;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.YappingMaws;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the Yapping Maw's typed attack and cooldown programs.</summary>
internal static class YappingMawInstructionProgramDefinitionsVerification
{
    /// <summary>Loads retail data for the Yapping Maw instruction check.</summary>
    internal static void VerifyYappingMawInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyYappingMawInstructionProgramDefinitions), () => VerifyYappingMawInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Runs all attack and cooldown entries, checking sound, held-Samus offsets, and initial facing selection.</summary>
    internal static void VerifyYappingMawInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        for (int direction = 0; direction < 8; direction++)
        {
            YappingMawProgram entry = YappingMawAnimationDefinitions.AttackForDirection(direction);
            (RoomEnemySystem enemies, RoomEnemySlot slot, _) =
                NewYappingMawInstructionSystem(entry);
            object?[] arguments =
                [slot, null, null, Position16<RoomPixels>.Zero, SnesButton.None];
            AssertEqual(EnemyInstructionRunner.ExpectedLoop(YappingMawAnimations.Programs, entry, YappingMawFrameDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(slot, EnemyInstructionRunner.LoopUpdates(YappingMawAnimations.Programs, entry)),
                $"Yapping Maw attack direction {direction} loop");
            AssertEqual((ushort?)0x002f, enemies.YappingMawBehavior.LastYappingMawSoundEffect,
                $"Yapping Maw attack direction {direction} publishes its sound");
        }

        (YappingMawProgram Entry, YappingMawProgram Loop, int Calls, short X, short Y, string Name)[] cooldowns =
        [
            (YappingMawProgram.CooldownUpRight, YappingMawProgram.CooldownUpLoop, 6, 8, -8, "up-right"),
            (YappingMawProgram.CooldownUp, YappingMawProgram.CooldownUpLoop, 5, 0, -16, "up"),
            (YappingMawProgram.CooldownUpLeft, YappingMawProgram.CooldownUpLeftLoop, 6, -8, -8, "up-left"),
            (YappingMawProgram.CooldownDownRight, YappingMawProgram.CooldownDownLoop, 6, 8, 8, "down-right"),
            (YappingMawProgram.CooldownDown, YappingMawProgram.CooldownDownLoop, 5, 0, 16, "down"),
            (YappingMawProgram.CooldownDownLeft, YappingMawProgram.CooldownDownLeftLoop, 6, -8, 8, "down-left"),
        ];
        foreach ((YappingMawProgram entry, YappingMawProgram loop, int calls, short x, short y, string name) in cooldowns)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, YappingMawEnemyState state) =
                NewYappingMawInstructionSystem(entry);
            object?[] arguments =
                [slot, null, null, Position16<RoomPixels>.Zero, SnesButton.None];
            RunYappingMawInstructionFrames(arguments, slot, count: 1);
            AssertEqual(x, state.HeldSamusOffset.X,
                $"Yapping Maw {name} callback X offset");
            AssertEqual(y, state.HeldSamusOffset.Y,
                $"Yapping Maw {name} callback Y offset");
            RunYappingMawInstructionFrames(arguments,
                slot,
                count: calls - 1);
            AssertEqual(YappingMawAnimations.Spritemaps(loop).First(), slot.SpritemapPointer,
                $"Yapping Maw {name} cooldown loops back to its first pose");
            AssertEqual((ushort?)0x002f, enemies.YappingMawBehavior.LastYappingMawSoundEffect,
                $"Yapping Maw {name} cooldown publishes its sound");
        }

        Suite(nameof(VerifyYappingMawInitializerSelections), () => VerifyYappingMawInitializerSelections());
        Suite(nameof(YappingMawAnimationsVerification.VerifyYappingMawAnimations), () => YappingMawAnimationsVerification.VerifyYappingMawAnimations(rom));

        Console.WriteLine(
            "Yapping Maw: all eight attack loops, six cooldown programs with their held-Samus offsets and sounds, " +
            "and the initial facing selection pass.");
    }

    /// <summary>Checks native parameter-two facing selection and linked body projectile setup.</summary>
    private static void VerifyYappingMawInitializerSelections(
)
    {
        foreach ((ushort parameter2, YappingMawProgram expected, string name) in new[]
        {
            ((ushort)0, YappingMawProgram.AttackingDown, "alternate/down"),
            ((ushort)1, YappingMawProgram.AttackingUp, "ordinary/up"),
        })
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(EnemyDefinitionId.YappingMaw, slot.Definition);
            slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = 0xa8 });
            slot.Position = slot.Position.WithWhole(new(128, 128));
            slot.Parameter1 = 128;
            slot.Parameter2 = parameter2;
            enemies.YappingMawBehavior.InitializeYappingMaw(slot);
            YappingMawEnemyState state = enemies.YappingMawStates[0] ??
                throw new InvalidOperationException(
                    $"Yapping Maw {name} initializer did not publish typed state.");
            AssertEqual(AnimationProgramCursor<YappingMawProgram>.Start(expected), state.Program,
                $"Yapping Maw {name} initializer program");
            AssertEqual(4, state.BodyProjectiles.Count(projectile => projectile is not null),
                $"Yapping Maw {name} initializer body links");
            AssertTrue(state.RootSpriteObject is not null,
                $"Yapping Maw {name} initializer root sprite object");
        }
    }

    /// <summary>Creates a Yapping Maw slot running a selected program with typed state.</summary>
    private static (
        RoomEnemySystem Enemies,
        RoomEnemySlot Slot,
        YappingMawEnemyState State) NewYappingMawInstructionSystem(
            YappingMawProgram entry)
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.YappingMaw, slot.Definition);
        slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = 0xa8 });
        slot.InstructionTimer = NativeCountdown.Start(1);
        var state = new YappingMawEnemyState(slot) { Program = AnimationProgramCursor<YappingMawProgram>.Start(entry) };
        slot.SetSpeciesState(state);
        return (enemies, slot, state);
    }

    /// <summary>Forces each requested instruction call through the production enemy interpreter.</summary>
    private static void RunYappingMawInstructionFrames(object?[] arguments,
        RoomEnemySlot slot,
        int count)
    {
        for (int call = 0; call < count; call++)
        {
            slot.InstructionTimer = NativeCountdown.Start(1);
            EnemyInstructionRunner.Process(arguments);
        }
    }

    /// <summary>Runs the retail-backed Yapping Maw instruction migration command.</summary>
    [VerificationCommand("--yapping-maw-instruction-program-definitions")]
    internal static int YappingMawInstructionProgramDefinitionsCommand()
    {
        Suite(nameof(VerifyYappingMawInstructionProgramDefinitions), () => VerifyYappingMawInstructionProgramDefinitions());
        return 0;
    }
}
