using SuperMetroid.Core.Programs;
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
    internal static void VerifyShutterBehavior()
    {
        VerifyPose(EnemyDefinitionId.ShutterGrowing, ShutterProgram.GrowingTenPixels,
            (enemies, slot) => ShuttersBehavior.InitializeGrowingShutter(slot),
            enemies => enemies.SlotTable.Slots[0].EntityState<ShuttersEntity, GrowingShutterEnemyState>()?.Program);
        foreach (EnemyDefinitionId definition in new[] { EnemyDefinitionId.ShutterShootable, EnemyDefinitionId.ShutterDestroyable })
        {
            VerifyPose(definition, ShutterProgram.FortyPixels,
                (enemies, slot) => ShuttersBehavior.InitializeVerticalShutter(slot),
                enemies => enemies.SlotTable.Slots[0].EntityState<ShuttersEntity, VerticalShutterEnemyState>()?.Program);
        }
        VerifyPose(EnemyDefinitionId.ShutterHorizShootable, ShutterProgram.Horizontal,
            (enemies, slot) => ShuttersBehavior.InitializeHorizontalShutter(slot),
            enemies => enemies.SlotTable.Slots[0].EntityState<ShuttersEntity, HorizontalShutterEnemyState>()?.Program);

        RoomEnemySystem kamerSystem = NewSystem();
        RoomEnemySlot kamer = NewSlot(kamerSystem, EnemyDefinitionId.Kamer);
        kamer.Bind(EnemyDefinitionId.Kamer, kamer.Definition);
        ShuttersBehavior.InitializeVerticalShutter(kamer);
        AssertEqual(AnimationProgramCursor<ShutterProgram>.Start(ShutterProgram.KamerPlatform),
            kamerSystem.SlotTable.Slots[0].EntityState<ShuttersEntity, VerticalShutterEnemyState>()?.Program, "Kamer vertical platform starts its loop");
        AssertEqual(EnemyInstructionRunner.ExpectedLoop(ShutterAnimations.Programs, ShutterProgram.KamerPlatform, ShutterFrameDefinitions.Spritemap),
            EnemyInstructionRunner.ShownFrames(kamer, EnemyInstructionRunner.LoopUpdates(ShutterAnimations.Programs, ShutterProgram.KamerPlatform)),
            "Kamer platform loop and its goto");
        Console.WriteLine("  Shutter behavior: every shutter starts its pose and Kamer runs its platform loop.");

        void VerifyPose(EnemyDefinitionId definition, ShutterProgram program, Action<RoomEnemySystem, RoomEnemySlot> initialize,
            Func<RoomEnemySystem, AnimationProgramCursor<ShutterProgram>?> cursor)
        {
            RoomEnemySystem enemies = NewSystem();
            RoomEnemySlot slot = NewSlot(enemies, definition);
            initialize(enemies, slot);
            AssertEqual(AnimationProgramCursor<ShutterProgram>.Start(program), cursor(enemies), $"shutter ${(int)definition:X4} initial animation");
            ushort pose = ShutterAnimationsConstants.Spritemaps(program).Single();
            AssertEqual(EnemyInstructionRunner.ExpectedFrames([pose, pose], [1, 1]),
                EnemyInstructionRunner.ShownFrames(slot, 3), $"shutter ${(int)definition:X4} shows and holds its pose");
        }

        RoomEnemySystem NewSystem()
        {
            var enemies = new RoomEnemySystem();
            enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
            return enemies;
        }
    }

    /// <summary>A slot of <paramref name="enemies"/> holding <paramref name="definition"/> with population words that select trigger mode one.</summary>
    private static RoomEnemySlot NewSlot(RoomEnemySystem enemies, EnemyDefinitionId definition)
    {
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(definition, slot.Definition);
        slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = ShutterFrameToolingDefinitions.Bank });
        slot.CurrentInstruction = 0;
        slot.Parameter1 = 2;
        return slot;
    }

    /// <summary>Checks native vertical shutter function selection and invalid offset rejection.</summary>
    internal static void VerifyVerticalShutterInitialFunctionSelection(SuperMetroidAddressSpace rom)
    {
        Func<VerticalShutterEnemyState, VerticalShutterFunction> select = ShuttersBehavior.SelectInitialVerticalShutterFunction;
        var state = new VerticalShutterEnemyState(new RoomEnemySlot(0, EnemySlotFixtures.Registry))
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
        var state = new HorizontalShutterEnemyState(new RoomEnemySlot(0, EnemySlotFixtures.Registry));
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
            AssertEqual(ShutterAnimationsConstants.Spritemaps(programs[index]).Single(), frames[index].Pointer, "shutter export pointer is its program's pose");
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

    /// <summary>
    /// The growing shutter's timer-mode wait tests for expiry before it decrements: a wait of two activates on the
    /// third call, and an expired wait stays at zero (activating again) instead of wrapping to a 65535-update wait.
    /// The wait stays in parameter 1, where other enemies' aliased enemy-RAM reads observe it.
    /// </summary>
    internal static void VerifyGrowingShutterActivationTimer()
    {
        var slot = new RoomEnemySlot(0, EnemySlotFixtures.Registry);
        var state = new GrowingShutterEnemyState(slot);
        slot.Parameter1 = 2;
        AssertEqual(false, ShuttersBehavior.AdvanceGrowingShutterTimer(state), "a two-update wait does not activate on its first call");
        AssertEqual(NativeCountdown.Start(1), state.ActivationTimer, "the first call counts the wait down by one");
        AssertEqual(false, ShuttersBehavior.AdvanceGrowingShutterTimer(state), "a two-update wait does not activate on its second call");
        AssertEqual(true, ShuttersBehavior.AdvanceGrowingShutterTimer(state), "a two-update wait activates on its third call");
        AssertEqual(NativeCountdown.Start(0), state.ActivationTimer, "activation leaves the wait expired");
        AssertEqual(true, ShuttersBehavior.AdvanceGrowingShutterTimer(state), "an expired wait activates again rather than wrapping");
        AssertEqual(0, slot.Parameter1, "an expired wait stays zero in parameter 1");
    }

    /// <summary>
    /// Upward growth applies the exact signed 16.16 negation of every authored growth speed: the fraction is
    /// negated as a sixteen-bit word and the whole word borrows one pixel whenever that fraction is nonzero.
    /// </summary>
    internal static void VerifyGrowingShutterUpwardVelocity()
    {
        for (int index = 0; index < GrowingShutterSpeedRecordCount; index++)
        {
            (short whole, ushort fraction) = GrowingShutterDefinitions.Speed((ushort)index);
            SnesSignedSixteenSixteen upward = ShuttersBehavior.UpwardGrowthVelocity(SnesSignedSixteenSixteen.FromParts(whole, fraction));
            AssertEqual(unchecked((short)(-whole - (fraction == 0 ? 0 : 1))), upward.WholePixels, $"growth speed {index}: upward whole pixels borrow for a nonzero fraction");
            AssertEqual(unchecked((ushort)-fraction), upward.Fraction, $"growth speed {index}: upward fraction is the negated fraction word");
        }
    }

    /// <summary>
    /// A horizontal shutter's travel endpoints are room positions offset from its spawn position by the travel
    /// distance, wrapping at sixteen bits: a leftward shutter spawned five pixels from the room's left edge ends
    /// its travel at the wrapped X, and a rightward one spawned near the right limit wraps past zero.
    /// </summary>
    internal static void VerifyHorizontalShutterEndpoints()
    {
        const byte travelDistance = 16;
        const ushort spawnY = 100;
        Confirm(primaryDirection: 0, spawnX: 5, expectedLeftX: 65525, expectedRightX: 5);
        Confirm(primaryDirection: 1, spawnX: 65530, expectedLeftX: 65530, expectedRightX: 10);

        static void Confirm(byte primaryDirection, ushort spawnX, ushort expectedLeftX, ushort expectedRightX)
        {
            var slot = new RoomEnemySlot(0, EnemySlotFixtures.Registry);
            slot.Position = FixedPosition<RoomPixels>.FromWhole(new Position16<RoomPixels>(spawnX, spawnY));
            slot.CurrentInstruction = SnesLittleEndian.Word(0, primaryDirection);
            slot.Parameter1 = SnesLittleEndian.Word(0, travelDistance);
            ShuttersBehavior.InitializeHorizontalShutter(slot);
            HorizontalShutterEnemyState state = slot.SpeciesState<HorizontalShutterEnemyState>()
                ?? throw new InvalidOperationException("Verification requires the horizontal shutter state.");
            string context = $"primary direction {primaryDirection} at X {spawnX}";
            AssertEqual(new Position16<RoomPixels>(expectedLeftX, spawnY), state.LeftEndpoint, $"{context}: left endpoint");
            AssertEqual(new Position16<RoomPixels>(expectedRightX, spawnY), state.RightEndpoint, $"{context}: right endpoint");
        }
    }

    /// <summary>
    /// The rightward and leftward 16.16 velocities each round-trip exactly through their own pair of slot words,
    /// including a negative whole word and a full fraction, and writing one leaves the other unchanged.
    /// </summary>
    internal static void VerifyHorizontalShutterVelocityWords()
    {
        var state = new HorizontalShutterEnemyState(new RoomEnemySlot(0, EnemySlotFixtures.Registry));
        SnesSignedSixteenSixteen right = SnesSignedSixteenSixteen.FromParts(-2, 40000);
        SnesSignedSixteenSixteen left = SnesSignedSixteenSixteen.FromParts(short.MinValue, ushort.MaxValue);
        state.RightVelocity = right;
        state.LeftVelocity = left;
        AssertEqual(right, state.RightVelocity, "rightward velocity round-trips through its slot words");
        AssertEqual(left, state.LeftVelocity, "leftward velocity round-trips through its own slot words");
    }

    /// <summary>Number of authored growing-shutter speed records.</summary>
    private const int GrowingShutterSpeedRecordCount = 24;

    /// <summary>Runs the shutter behavior checks and program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--shutter-behavior")]
    internal static int ShutterBehaviorCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyShutterBehavior), () => VerifyShutterBehavior());
        Suite(nameof(VerifyGrowingShutterActivationTimer), () => VerifyGrowingShutterActivationTimer());
        Suite(nameof(VerifyGrowingShutterUpwardVelocity), () => VerifyGrowingShutterUpwardVelocity());
        Suite(nameof(VerifyHorizontalShutterEndpoints), () => VerifyHorizontalShutterEndpoints());
        Suite(nameof(VerifyHorizontalShutterVelocityWords), () => VerifyHorizontalShutterVelocityWords());
        Suite(nameof(ShutterAnimationsVerification.VerifyShutterAnimations), () => ShutterAnimationsVerification.VerifyShutterAnimations(rom));
        Suite(nameof(VerifyShutterExportFrames), () => VerifyShutterExportFrames(rom));
        return 0;
    }
}
