using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Shutters;
using SuperMetroid.Core.Enemies.Shutters.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Shutters;
using SuperMetroid.Verification.Harness;
using SuperMetroid.TestSupport.ScenarioFixtures;
using SuperMetroid.Core.World;

/// <summary>Checks the shutters' and Kamer's typed animations, their initial function tables and their export frames.</summary>
internal static class ShutterBehaviorVerification
{
    /// <summary>
    /// Each initializer starts its animation: the growing shutter its ten-pixel pose, the shootable and destroyable vertical
    /// shutters the shared forty-pixel pose, and Kamer its four-frame platform loop. A
    /// pose shows once and holds; the update that reaches the native sleep still reports it as new.
    /// </summary>
    internal static void VerifyShutterBehavior()
    {
        VerifyPose(EnemyDefinitionId.ShutterGrowing, ShutterSegment.GrowingTenPixels,
            (enemies, slot) =>
            {
                slot.SetSpawnSettings(new GrowingShutterSpawnSettings(
                    Wait: GrowingShutterWait.SamusNear, Growth: GrowingShutterGrowth.Down, ActivationDistance: 144, ActivationWait: 0,
                    GrowthSpeed: SignedSixteenSixteen.FromSixteenthsOfPixel(17)));
                ShuttersBehavior.InitializeGrowingShutter(slot);
            },
            enemies => enemies.SlotTable.Slots[0].EntityState<ShuttersEntity, GrowingShutterEnemyState>()?.Playback);
        foreach (EnemyDefinitionId definition in new[] { EnemyDefinitionId.ShutterShootable, EnemyDefinitionId.ShutterDestroyable })
        {
            VerifyPose(definition, ShutterSegment.FortyPixels,
                (enemies, slot) => ShuttersBehavior.InitializeVerticalShutter(slot),
                enemies => enemies.SlotTable.Slots[0].EntityState<ShuttersEntity, VerticalShutterEnemyState>()?.Playback);
        }

        RoomEnemySystem kamerSystem = NewSystem();
        RoomEnemySlot kamer = NewSlot(kamerSystem, EnemyDefinitionId.Kamer);
        kamer.Bind(EnemyDefinitionId.Kamer, kamer.Definition);
        ShuttersBehavior.InitializeVerticalShutter(kamer);
        AssertEqual(new AnimationPlayback<ShutterSegment>(ShutterAnimations.StartingWith(ShutterSegment.KamerPlatform), AnimationPlace.StartOfAnimation),
            kamerSystem.SlotTable.Slots[0].EntityState<ShuttersEntity, VerticalShutterEnemyState>()?.Playback, "Kamer vertical platform starts its loop");
        AssertEqual(EnemyInstructionRunner.ExpectedLoop(ShutterAnimations.Segments, ShutterSegment.KamerPlatform, ShutterFrameToolingDefinitions.Spritemap),
            EnemyInstructionRunner.ShownFrames(kamer, EnemyInstructionRunner.LoopUpdates(ShutterAnimations.Segments, ShutterSegment.KamerPlatform)),
            "Kamer platform loop and its goto");
        Console.WriteLine("  Shutter behavior: every shutter starts its pose and Kamer runs its platform loop.");

        void VerifyPose(EnemyDefinitionId definition, ShutterSegment program, Action<RoomEnemySystem, RoomEnemySlot> initialize,
            Func<RoomEnemySystem, AnimationPlayback<ShutterSegment>?> playback)
        {
            RoomEnemySystem enemies = NewSystem();
            RoomEnemySlot slot = NewSlot(enemies, definition);
            initialize(enemies, slot);
            AssertEqual(new AnimationPlayback<ShutterSegment>(ShutterAnimations.StartingWith(program), AnimationPlace.StartOfAnimation), playback(enemies), $"shutter {definition} initial animation");
            ushort pose = ShutterAnimationsConstants.Spritemaps(program).Single();
            AssertEqual(EnemyInstructionRunner.ExpectedFrames([pose, pose], [1, 1]),
                EnemyInstructionRunner.ShownFrames(slot, 3), $"shutter {definition} shows and holds its pose");
        }

        RoomEnemySystem NewSystem()
        {
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            return enemies;
        }
    }

    /// <summary>A slot of <paramref name="enemies"/> holding <paramref name="definition"/> with population words that select trigger mode one.</summary>
    private static RoomEnemySlot NewSlot(RoomEnemySystem enemies, EnemyDefinitionId definition)
    {
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(definition, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        slot.SetSpawnWords(EnemyDefinitionId.ShutterShootable, _ => SuperMetroid.AssetExtraction.Enemies.PopulationWords.Parameters(2, 0));
        return slot;
    }

    /// <summary>Checks native vertical shutter function selection and invalid trigger-mode rejection.</summary>
    internal static void VerifyVerticalShutterInitialFunctionSelection(CartridgeImportAddressSpace rom)
    {
        Func<VerticalShutterEnemyState, VerticalShutterAction> select = ShuttersBehavior.SelectInitialVerticalShutterAction;
        var state = new VerticalShutterEnemyState()
        {
            Action = VerticalShutterAction.Initial,
        };
        foreach (ShutterTriggerMode mode in Enum.GetValues<ShutterTriggerMode>())
        {
            state.TriggerMode = mode;
            ushort native = ReadShutterWord(rom, ShutterNativeTableAddresses.VerticalInitialFunctions.AddWithinBank(ShutterTriggerModeCodec.ToNativeOffset(mode)));
            AssertEqual(ShuttersNativeWords.VerticalShutterActions.Decode(native, "vertical shutter initial action"), select(state), "vertical shutter native initial function pointer");
            AssertEqual(VerticalShutterAction.Initial, state.Action,
                "vertical initial dispatch selects a function without installing it");
        }
        foreach (int mode in new[] { 5, 6, 128, 255 })
            AssertThrows<InvalidDataException>(() => SuperMetroid.AssetExtraction.Enemies.EnemySpawnSettingsCodec.Decode(EnemyDefinitionId.ShutterShootable,
                SuperMetroid.AssetExtraction.Enemies.PopulationWords.Parameters(mode, 0)), "vertical shutter invalid trigger mode");
    }

    /// <summary>
    /// The four exported shutter compositions are the poses the growing and forty-pixel programs show, and the
    /// native growing maps between them hold the sprite counts their calculated addresses assume.
    /// </summary>
    internal static void VerifyShutterExportFrames(CartridgeImportAddressSpace rom)
    {
        ShutterSegment[] programs =
        [
            ShutterSegment.GrowingTenPixels, ShutterSegment.GrowingTwentyPixels, ShutterSegment.GrowingThirtyPixels,
            ShutterSegment.FortyPixels,
        ];
        NativeEnemySpritemapDefinition[] frames = ShutterVisualDefinitions.Frames();
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
    private static ushort ReadShutterWord(CartridgeImportAddressSpace bus, SnesAddress address) =>
        CartridgeImportSource.Require(bus).ReadCartridgeWord(address.Value);

    /// <summary>
    /// The growing shutter's timer-mode wait tests for expiry before it decrements: a wait of two activates on the
    /// third call, and an expired wait stays at zero (activating again) instead of wrapping to a 65535-update wait.
    /// </summary>
    internal static void VerifyGrowingShutterActivationTimer()
    {
        var slot = new RoomEnemySlot(0, EnemySlotFixtures.Registry);
        var state = new GrowingShutterEnemyState(slot);
        state.ActivationTimer = 2;
        AssertEqual(false, ShuttersBehavior.AdvanceGrowingShutterTimer(state), "a two-update wait does not activate on its first call");
        AssertEqual(1, state.ActivationTimer, "the first call counts the wait down by one");
        AssertEqual(false, ShuttersBehavior.AdvanceGrowingShutterTimer(state), "a two-update wait does not activate on its second call");
        AssertEqual(true, ShuttersBehavior.AdvanceGrowingShutterTimer(state), "a two-update wait activates on its third call");
        AssertEqual(0, state.ActivationTimer, "activation leaves the wait expired");
        AssertEqual(true, ShuttersBehavior.AdvanceGrowingShutterTimer(state), "an expired wait activates again rather than wrapping");
        AssertEqual(0, state.ActivationTimer, "an expired wait stays zero");
    }

    /// <summary>
    /// Upward growth applies the exact signed 16.16 negation of every authored growth speed: the fraction is
    /// negated as a sixteen-bit word and the whole word borrows one pixel whenever that fraction is nonzero.
    /// </summary>
    internal static void VerifyGrowingShutterUpwardVelocity()
    {
        for (int index = 0; index < GrowingShutterSpeedRecordCount; index++)
        {
            SignedSixteenSixteen speed = ((GrowingShutterSpawnSettings)SuperMetroid.AssetExtraction.Enemies.EnemySpawnSettingsCodec.Decode(
                EnemyDefinitionId.ShutterGrowing, SuperMetroid.AssetExtraction.Enemies.PopulationWords.Parameters(0, index))).GrowthSpeed;
            int whole = speed.WholePixels;
            ushort fraction = speed.Fraction;
            SignedSixteenSixteen upward = ShuttersBehavior.UpwardGrowthVelocity(speed);
            AssertEqual(unchecked((short)(-whole - (fraction == 0 ? 0 : 1))), upward.WholePixels, $"growth speed {index}: upward whole pixels borrow for a nonzero fraction");
            AssertEqual(unchecked((ushort)-fraction), upward.Fraction, $"growth speed {index}: upward fraction is the negated fraction word");
        }
    }

    /// <summary>
    /// A vertical shutter's travel endpoints are room positions offset from its spawn position by the travel
    /// distance, wrapping at sixteen bits: an upward shutter spawned five pixels below the room's top edge ends its
    /// travel at the wrapped Y, and a downward one spawned near the bottom limit wraps past zero. Its two velocities
    /// are the selected linear-speed record's downward and upward entries.
    /// </summary>
    internal static void VerifyVerticalShutterEndpoints()
    {
        const byte travelDistance = 16;
        const byte speedIndex = 3;
        const ushort spawnX = 100;
        Confirm(primaryDirection: 0, spawnY: 5, expectedUpperY: 65525, expectedLowerY: 5);
        Confirm(primaryDirection: 1, spawnY: 65530, expectedUpperY: 65530, expectedLowerY: 10);

        static void Confirm(byte primaryDirection, ushort spawnY, ushort expectedUpperY, ushort expectedLowerY)
        {
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemySlot slot = NewSlot(enemies, EnemyDefinitionId.ShutterShootable);
            slot.Position = FixedPosition<RoomPixels>.FromWhole(new Position16<RoomPixels>(spawnX, spawnY));
            slot.SetSpawnWords(EnemyDefinitionId.ShutterShootable, _ => new SuperMetroid.AssetExtraction.Enemies.PopulationWords(
                LittleEndianBytes.Word(speedIndex, primaryDirection), 0, 0, LittleEndianBytes.Word(0, travelDistance), 0));
            ShuttersBehavior.InitializeVerticalShutter(slot);
            VerticalShutterEnemyState state = slot.SpeciesState<VerticalShutterEnemyState>()
                ?? throw new InvalidOperationException("Verification requires the vertical shutter state.");
            string context = $"primary direction {primaryDirection} at Y {spawnY}";
            AssertEqual(new Position16<RoomPixels>(spawnX, expectedUpperY), state.UpperEndpoint, $"{context}: upper endpoint");
            AssertEqual(new Position16<RoomPixels>(spawnX, expectedLowerY), state.LowerEndpoint, $"{context}: lower endpoint");
            SignedSixteenSixteen down = EnemyLinearSpeeds.Row(speedIndex).Positive;
            SignedSixteenSixteen up = EnemyLinearSpeeds.Row(speedIndex).Negative;
            AssertEqual(down, state.DownVelocity,
                $"{context}: downward velocity is the speed record's first entry");
            AssertEqual(up, state.UpVelocity,
                $"{context}: upward velocity is the speed record's second entry");
        }
    }

    /// <summary>
    /// The downward and upward 16.16 velocities each round-trip exactly through their own pair of slot words,
    /// including a negative whole word and a full fraction, and writing one leaves the other unchanged.
    /// </summary>
    internal static void VerifyVerticalShutterVelocityWords()
    {
        var state = new VerticalShutterEnemyState();
        SignedSixteenSixteen down = SignedSixteenSixteen.FromParts(-2, 40000);
        SignedSixteenSixteen up = SignedSixteenSixteen.FromParts(short.MinValue, ushort.MaxValue);
        state.DownVelocity = down;
        state.UpVelocity = up;
        AssertEqual(down, state.DownVelocity, "downward velocity round-trips through its slot words");
        AssertEqual(up, state.UpVelocity, "upward velocity round-trips through its own slot words");
    }

    /// <summary>Number of authored growing-shutter speed records.</summary>
    private const int GrowingShutterSpeedRecordCount = 24;

    /// <summary>Runs the shutter behavior checks and program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--shutter-behavior")]
    internal static int ShutterBehaviorCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyShutterBehavior), () => VerifyShutterBehavior());
        Suite(nameof(VerifyGrowingShutterActivationTimer), () => VerifyGrowingShutterActivationTimer());
        Suite(nameof(VerifyGrowingShutterUpwardVelocity), () => VerifyGrowingShutterUpwardVelocity());
        Suite(nameof(VerifyVerticalShutterEndpoints), () => VerifyVerticalShutterEndpoints());
        Suite(nameof(VerifyVerticalShutterVelocityWords), () => VerifyVerticalShutterVelocityWords());
        Suite(nameof(ShutterAnimationsVerification.VerifyShutterAnimations), () => ShutterAnimationsVerification.VerifyShutterAnimations(rom));
        Suite(nameof(VerifyShutterExportFrames), () => VerifyShutterExportFrames(rom));
        return 0;
    }
}
