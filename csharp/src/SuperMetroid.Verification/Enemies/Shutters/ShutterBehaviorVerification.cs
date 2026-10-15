using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Shutters;
using SuperMetroid.Core.Enemies.Shutters.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.Shutters;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the shutters' and Kamer's typed animations, their initial function tables and their export frames.</summary>
internal static class ShutterBehaviorVerification
{
    /// <summary>
    /// Each initializer starts its animation: the growing shutter its ten-pixel pose, the shootable and destroyable vertical
    /// shutters the shared forty-pixel pose, the horizontal shutter its pose, and Kamer its four-frame platform loop. A
    /// pose shows once and holds; the update that reaches the native sleep still reports it as new.
    /// </summary>
    internal static void VerifyShutterBehavior(SuperMetroidAddressSpace rom)
    {
        VerifyPose(EnemyDefinitionId.ShutterGrowing, ShutterProgram.GrowingTenPixels,
            (enemies, slot) => enemies.ShuttersBehavior.InitializeGrowingShutter(slot),
            enemies => enemies.ShuttersBehavior._growingShutterStates[0]?.Program);
        foreach (EnemyDefinitionId definition in new[] { EnemyDefinitionId.ShutterShootable, EnemyDefinitionId.ShutterDestroyable })
        {
            VerifyPose(definition, ShutterProgram.FortyPixels,
                (enemies, slot) => enemies.ShuttersBehavior.InitializeVerticalShutter(slot),
                enemies => enemies.ShuttersBehavior._verticalShutterStates[0]?.Program);
        }
        VerifyPose(EnemyDefinitionId.ShutterHorizShootable, ShutterProgram.Horizontal,
            (enemies, slot) => enemies.ShuttersBehavior.InitializeHorizontalShutter(slot, null),
            enemies => enemies.ShuttersBehavior._horizontalShutterStates[0]?.Program);

        RoomEnemySystem kamerSystem = NewSystem();
        RoomEnemySlot kamer = NewSlot(kamerSystem, EnemyDefinitionId.Kamer);
        kamerSystem.ShuttersBehavior.InitializeVerticalShutter(kamer);
        AssertEqual(AnimationProgramCursor<ShutterProgram>.Start(ShutterProgram.KamerPlatform),
            kamerSystem.ShuttersBehavior._verticalShutterStates[0]?.Program, "Kamer vertical platform starts its loop");
        AssertEqual(EnemyInstructionRunner.ExpectedLoop(ShutterAnimations.Programs, ShutterProgram.KamerPlatform, ShutterFrameDefinitions.Spritemap),
            EnemyInstructionRunner.ShownFrames(kamerSystem, kamer, EnemyInstructionRunner.LoopUpdates(ShutterAnimations.Programs, ShutterProgram.KamerPlatform)),
            "Kamer platform loop and its goto");
        Console.WriteLine("  Shutter behavior: every shutter starts its pose and Kamer runs its platform loop.");

        void VerifyPose(EnemyDefinitionId definition, ShutterProgram program, Action<RoomEnemySystem, RoomEnemySlot> initialize,
            Func<RoomEnemySystem, AnimationProgramCursor<ShutterProgram>?> cursor)
        {
            RoomEnemySystem enemies = NewSystem();
            RoomEnemySlot slot = NewSlot(enemies, definition);
            initialize(enemies, slot);
            AssertEqual(AnimationProgramCursor<ShutterProgram>.Start(program), cursor(enemies), $"shutter ${(int)definition:X4} initial animation");
            ushort pose = ShutterAnimations.Spritemaps(program).Single();
            AssertEqual(EnemyInstructionRunner.ExpectedFrames([pose, pose], [1, 1]),
                EnemyInstructionRunner.ShownFrames(enemies, slot, 3), $"shutter ${(int)definition:X4} shows and holds its pose");
        }

        RoomEnemySystem NewSystem()
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureBus(rom);
            return enemies;
        }
    }

    /// <summary>A slot of <paramref name="enemies"/> holding <paramref name="definition"/> with population words that select trigger mode one.</summary>
    private static RoomEnemySlot NewSlot(RoomEnemySystem enemies, EnemyDefinitionId definition)
    {
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.EnemyDefinitionPointer = definition;
        slot.Definition = default(RoomEnemyDefinition) with { Bank = ShutterFrameDefinitions.Bank };
        slot.CurrentInstruction = 0;
        slot.Parameter1 = 2;
        return slot;
    }

    /// <summary>Checks native vertical shutter function selection and invalid offset rejection.</summary>
    internal static void VerifyVerticalShutterInitialFunctionSelection(SuperMetroidAddressSpace rom)
    {
        Func<VerticalShutterEnemyState, VerticalShutterFunction> select = ShuttersBehavior.SelectInitialVerticalShutterFunction;
        var state = new VerticalShutterEnemyState(new RoomEnemySlot(0))
        {
            Function = VerticalShutterFunction.Initial,
        };
        for (ushort offset = 0; offset <= 8; offset += 2)
        {
            state.InitialFunctionTableOffset = offset;
            ushort native = ReadShutterWord(rom, ShutterNativeTableAddresses.VerticalInitialFunctions.AddWithinBank(offset));
            AssertEqual(native, (ushort)select(state), "vertical shutter native initial function pointer");
            AssertEqual(VerticalShutterFunction.Initial, state.Function,
                "vertical initial dispatch selects a function without installing it");
        }
        foreach (ushort offset in new ushort[] { 1, 3, 5, 7, 9, 10, 256, 65534, 65535 })
        {
            state.InitialFunctionTableOffset = offset;
            AssertThrows<InvalidDataException>(() => select(state), "vertical shutter invalid initial offset");
            AssertEqual(VerticalShutterFunction.Initial, state.Function, "invalid vertical selector preserves state");
        }
    }

    /// <summary>Checks horizontal shutter function installation and invalid offset rejection.</summary>
    internal static void VerifyHorizontalShutterInitialFunctionSelection(SuperMetroidAddressSpace rom)
    {
        Action<HorizontalShutterEnemyState> select = ShuttersBehavior.SelectInitialHorizontalShutterFunction;
        var state = new HorizontalShutterEnemyState(new RoomEnemySlot(0));
        for (ushort offset = 0; offset <= 8; offset += 2)
        {
            state.Function = HorizontalShutterFunction.Initial;
            state.InitialFunctionTableOffset = offset;
            ushort native = ReadShutterWord(rom, ShutterNativeTableAddresses.HorizontalInitialFunctions.AddWithinBank(offset));
            select(state);
            AssertEqual(native, (ushort)state.Function, "horizontal shutter installs native initial function pointer");
        }
        foreach (ushort offset in new ushort[] { 1, 3, 5, 7, 9, 10, 256, 65534, 65535 })
        {
            state.Function = HorizontalShutterFunction.Initial;
            state.InitialFunctionTableOffset = offset;
            AssertThrows<InvalidDataException>(() => select(state), "horizontal shutter invalid initial offset");
            AssertEqual(HorizontalShutterFunction.Initial, state.Function, "invalid horizontal selector preserves state");
        }
    }

    /// <summary>
    /// The five exported shutter compositions are the poses the growing, forty-pixel and horizontal programs show, and the
    /// native growing maps between them hold the sprite counts their calculated addresses assume.
    /// </summary>
    internal static void VerifyShutterExportFrames(SuperMetroidAddressSpace rom)
    {
        ShutterProgram[] programs =
        [
            ShutterProgram.GrowingTenPixels, ShutterProgram.GrowingTwentyPixels, ShutterProgram.GrowingThirtyPixels,
            ShutterProgram.FortyPixels, ShutterProgram.Horizontal,
        ];
        EnemySpritemapDefinition[] frames = ShutterVisualDefinitions.Frames();
        AssertEqual(programs.Length, frames.Length, "one exported shutter composition per pose");
        for (int index = 0; index < programs.Length; index++)
            AssertEqual(ShutterAnimations.Spritemaps(programs[index]).Single(), frames[index].Pointer, "shutter export pointer is its program's pose");
        // Stage n's used map holds n + 1 sprites; an unused map of n + 2 sprites follows it (2 + 5 per sprite bytes each).
        for (int stage = 0; stage < 4; stage++)
        {
            var usedMap = new SnesAddress(frames[stage].Bank, frames[stage].Pointer);
            AssertEqual((ushort)(stage + 1), ReadShutterWord(rom, usedMap), "native growing map sprite count");
            if (stage < 3)
                AssertEqual((ushort)(stage + 2), ReadShutterWord(rom, usedMap.AddWithinBank(2 + 5 * (stage + 1))), "native intervening map sprite count");
        }
    }

    /// <summary>Reads a native bank-$A2 shutter word for the oracles above.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for shutter tables and maps")]
    private static ushort ReadShutterWord(SuperMetroidAddressSpace bus, SnesAddress address) =>
        CartridgeImportSource.Require(bus).ReadCartridgeWord(address.Value);

    /// <summary>Runs the shutter behavior checks and program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--shutter-behavior")]
    internal static int ShutterBehaviorCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyShutterBehavior), () => VerifyShutterBehavior(rom));
        Suite(nameof(ShutterAnimationsVerification.VerifyShutterAnimations), () => ShutterAnimationsVerification.VerifyShutterAnimations(rom));
        Suite(nameof(VerifyShutterExportFrames), () => VerifyShutterExportFrames(rom));
        return 0;
    }
}
