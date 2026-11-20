using SuperMetroid.AssetExtraction.Enemies;
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
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.ColourEffects;

/// <summary>Checks the shutters' and Kamer's typed animations, their initial function tables and their export frames.</summary>
internal static class ShutterBehaviorVerification
{
    /// <summary>
    /// Each initializer starts its animation: the growing shutter its quarter-grown pose, the shootable and destroyable vertical
    /// shutters the shared fully grown pose, and Kamer its four-frame platform loop. A
    /// pose shows once and is held (#2276): it is new only on the update it shows.
    /// </summary>
    internal static void VerifyShutterBehavior()
    {
        VerifyPose(EnemyDefinitionId.ShutterGrowing, ShutterSegment.QuarterGrown,
            (enemies, place) =>
            {
                place.SetSpawnSettings(new GrowingShutterSpawnSettings(
                    Wait: GrowingShutterWait.SamusNear, Growth: VerticalDirection.Down, ActivationDistance: 144, ActivationWait: 0,
                    GrowthSpeed: SignedSixteenSixteen.FromSixteenthsOfPixel(17)));
                ShuttersBehavior.InitializeGrowingShutter(place, place.SpawnSettings<GrowingShutterSpawnSettings>());
            },
            enemies => enemies.Places.InUpdateOrder[0].EntityState<ShuttersEntity, GrowingShutterEnemyState>()?.Playback);
        foreach (EnemyDefinitionId definition in new[] { EnemyDefinitionId.ShutterShootable, EnemyDefinitionId.ShutterDestroyable })
        {
            VerifyPose(definition, ShutterSegment.FullyGrown,
                (enemies, place) =>
                {
                    place.InitializeAsPlaced(RoomEnemyFrameFixtures.StandaloneSamus());
                    VerticalShutterEnemyState state = SuperMetroid.Core.Enemies.EnemyPartners.Listed<VerticalShutterEntity>(place).State;
                    AssertEqual(ShuttersBehavior.InitialAction(place.SpawnSettings<ShutterSpawnSettings>().TriggerMode), state.Action,
                        $"shutter {definition} starts in its trigger's initial action (#2075)");
                },
                enemies => enemies.Places.InUpdateOrder[0].EntityState<ShuttersEntity, VerticalShutterEnemyState>()?.Playback);
        }

        RoomEnemySystem kamerSystem = NewSystem();
        RoomEnemyPlace kamer = NewPlace(kamerSystem, EnemyDefinitionId.Kamer);
        kamer.Bind(EnemyDefinitionId.Kamer, kamer.Definition);
        // The Kamer's own entity sets it up, showing its platform (#2179).
        AssertTrue(kamer.Entity is KamerEntity, "the Kamer is its own entity");
        kamer.InitializeAsPlaced(RoomEnemyFrameFixtures.StandaloneSamus());
        AssertEqual(new AnimationPlayback<ShutterSegment>(ShutterAnimations.StartingWith(ShutterSegment.KamerPlatform), AnimationPlace.StartOfAnimation),
            kamerSystem.Places.InUpdateOrder[0].EntityState<ShuttersEntity, VerticalShutterEnemyState>()?.Playback, "Kamer vertical platform starts its loop");
        AssertEqual(EnemyInstructionRunner.ExpectedLoop(ShutterAnimations.Segments, ShutterSegment.KamerPlatform, ShutterFrameToolingDefinitions.Spritemap),
            EnemyInstructionRunner.ShownFrames(kamer, EnemyInstructionRunner.LoopUpdates(ShutterAnimations.Segments, ShutterSegment.KamerPlatform)),
            "Kamer platform loop and its goto");
        Console.WriteLine("  Shutter behavior: every shutter starts its pose and Kamer runs its platform loop.");

        void VerifyPose(EnemyDefinitionId definition, ShutterSegment program, Action<RoomEnemySystem, RoomEnemyPlace> initialize,
            Func<RoomEnemySystem, AnimationPlayback<ShutterSegment>?> playback)
        {
            RoomEnemySystem enemies = NewSystem();
            RoomEnemyPlace place = NewPlace(enemies, definition);
            initialize(enemies, place);
            AssertEqual(new AnimationPlayback<ShutterSegment>(ShutterAnimations.StartingWith(program), AnimationPlace.StartOfAnimation), playback(enemies), $"shutter {definition} initial animation");
            ushort pose = ShutterAnimationsConstants.Spritemaps(program).Single();
            AssertEqual(EnemyInstructionRunner.ExpectedFrames([pose], [1]),
                EnemyInstructionRunner.ShownFrames(place, 3), $"shutter {definition} shows and holds its pose");
        }

        RoomEnemySystem NewSystem()
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            return enemies;
        }
    }

    /// <summary>A slot of <paramref name="enemies"/> holding <paramref name="definition"/> with population words that select trigger mode one.</summary>
    private static RoomEnemyPlace NewPlace(RoomEnemySystem enemies, EnemyDefinitionId definition)
    {
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(definition, place.Definition);
        place.Bind(place.DefinitionId, default(RoomEnemyDefinition));
        place.SetSpawnWords(EnemyDefinitionId.ShutterShootable, _ => SuperMetroid.AssetExtraction.Enemies.PopulationWords.Parameters(2, 0));
        return place;
    }

    /// <summary>
    /// Checks that each trigger mode's initial action is the native trigger table's entry for that mode, and invalid
    /// trigger-mode rejection.
    /// </summary>
    internal static void VerifyVerticalShutterInitialFunctionSelection(CartridgeImportAddressSpace rom)
    {
        foreach (ShutterTriggerMode mode in Enum.GetValues<ShutterTriggerMode>())
        {
            ushort native = ReadShutterWord(rom, ShutterNativeTableAddresses.VerticalInitialFunctions.AddWithinBank(ShutterTriggerModeCodec.ToNativeOffset(mode)));
            AssertEqual(ShuttersNativeWords.VerticalShutterActions.Decode(native, "vertical shutter initial action"), ShuttersBehavior.InitialAction(mode),
                "vertical shutter native initial function pointer");
        }
        foreach (int mode in new[] { 5, 6, 128, 255 })
            AssertThrows<InvalidDataException>(() => SuperMetroid.AssetExtraction.Enemies.EnemySpawnSettingsCodec.Decode(EnemyDefinitionId.ShutterShootable,
                SuperMetroid.AssetExtraction.Enemies.PopulationWords.Parameters(mode, 0)), "vertical shutter invalid trigger mode");
    }

    /// <summary>
    /// The four exported shutter compositions are the poses the growing and fully grown programs show, and the
    /// native growing maps between them hold the sprite counts their calculated addresses assume.
    /// </summary>
    internal static void VerifyShutterExportFrames(CartridgeImportAddressSpace rom)
    {
        ShutterSegment[] programs =
        [
            ShutterSegment.QuarterGrown, ShutterSegment.HalfGrown, ShutterSegment.ThreeQuartersGrown,
            ShutterSegment.FullyGrown,
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
        bus.ReadCartridgeWord(address.Value);

    /// <summary>
    /// The growing shutter's timer-mode wait holds its true duration (#2281): a placement's wait of two activates on the
    /// third call, the one that takes it to zero.
    /// </summary>
    internal static void VerifyGrowingShutterActivationTimer()
    {
        var state = new GrowingShutterEnemyState(GrowingShutterAction.WaitForTimer, spawnY: 0, VerticalDirection.Up, new GrowingShutterSpawnSettings(default, VerticalDirection.Up, ActivationDistance: 0, ActivationWait: 0, SignedSixteenSixteen.Zero));
        state.ActivationTimer = UpdateCountdown.For(2 + EnemyWaitDefinitions.RunOutUpdate);
        AssertEqual(false, ShuttersBehavior.AdvanceGrowingShutterTimer(state), "a two-update wait does not activate on its first call");
        AssertEqual(2, state.ActivationTimer.UpdatesLeft, "the first call counts the wait down by one");
        AssertEqual(false, ShuttersBehavior.AdvanceGrowingShutterTimer(state), "a two-update wait does not activate on its second call");
        AssertEqual(true, ShuttersBehavior.AdvanceGrowingShutterTimer(state), "a two-update wait activates on its third call");
        AssertEqual(0, state.ActivationTimer.UpdatesLeft, "activation leaves the wait run out");
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
    /// distance, which every placement keeps inside the room: an upward shutter's travel ends above its spawn and a
    /// downward one's below it, the downward one spawned eight pixels below the room's top edge as the Spiky Platforms
    /// Tunnel's is. Its two velocities are the selected linear-speed record's downward and upward entries.
    /// </summary>
    internal static void VerifyVerticalShutterEndpoints()
    {
        const byte travelDistance = 16;
        const byte speedIndex = 3;
        const ushort spawnX = 100;
        Confirm(primaryDirection: 0, spawnY: 37, expectedUpperY: 21, expectedLowerY: 37);
        Confirm(primaryDirection: 1, spawnY: 8, expectedUpperY: 8, expectedLowerY: 24);

        static void Confirm(byte primaryDirection, ushort spawnY, ushort expectedUpperY, ushort expectedLowerY)
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemyPlace place = NewPlace(enemies, EnemyDefinitionId.ShutterShootable);
            place.Position = FixedPosition<RoomPixels>.FromWhole(new Position16<RoomPixels>(spawnX, spawnY));
            place.SetSpawnWords(EnemyDefinitionId.ShutterShootable, _ => new SuperMetroid.AssetExtraction.Enemies.PopulationWords(
                LittleEndianBytes.Word(speedIndex, primaryDirection), 0, 0, LittleEndianBytes.Word(0, travelDistance), 0));
            ShuttersBehavior.InitializeVerticalShutter(place, ShutterSegment.FullyGrown, place.SpawnSettings<ShutterSpawnSettings>());
            VerticalShutterEnemyState state = SuperMetroid.Core.Enemies.EnemyPartners.Listed<VerticalShutterEntity>(place).State;
            string context = $"primary direction {primaryDirection} at Y {spawnY}";
            AssertEqual(expectedUpperY, state.UpperEndpointY, $"{context}: upper endpoint");
            AssertEqual(expectedLowerY, state.LowerEndpointY, $"{context}: lower endpoint");
            SignedSixteenSixteen down = EnemyLinearSpeedTable.Speed(speedIndex);
            SignedSixteenSixteen up = -down;
            AssertEqual(down, ShuttersBehavior.VerticalShutterVelocity(place, VerticalDirection.Down),
                $"{context}: downward velocity is the speed record's first entry");
            AssertEqual(up, ShuttersBehavior.VerticalShutterVelocity(place, VerticalDirection.Up),
                $"{context}: upward velocity is the speed record's second entry");
        }
    }

    /// <summary>
    /// The escape's shutter flash shows on the destroyable shutters the room set up, which the shutters' owner keeps
    /// (#2380): a shootable shutter beside them shows none, and a destroyed one keeps the colours it last showed.
    /// </summary>
    internal static void VerifyEscapeShutterFlash()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace destroyable = SetUp(0, EnemyDefinitionId.ShutterDestroyable);
        RoomEnemyPlace shootable = SetUp(1, EnemyDefinitionId.ShutterShootable);

        var first = new ColourEffectFrame(ColourEffectId.TourianShutterRedFlashing, 0);
        enemies.ShowEscapeShutterFlash(first);
        AssertEqual(PictureShading.Standard.With(ColourEffectShades.Of(ColourEffectColours.TourianShutters, first)), destroyable.Shading,
            "the destroyable shutter shows the flash's frame");
        AssertEqual(PictureShading.Standard, shootable.Shading, "the shootable shutter shows no flash");

        var second = new ColourEffectFrame(ColourEffectId.TourianShutterRedFlashing, 1);
        enemies.ShowEscapeShutterFlash(second);
        AssertEqual(PictureShading.Standard.With(ColourEffectShades.Of(ColourEffectColours.TourianShutters, second)), destroyable.Shading,
            "the flash's next frame replaces the one before it");

        destroyable.Unbind();
        enemies.ShowEscapeShutterFlash(first);
        AssertEqual(PictureShading.Standard.With(ColourEffectShades.Of(ColourEffectColours.TourianShutters, second)), destroyable.Shading,
            "a shutter that has left its place shows the flash no more");
        Console.WriteLine("  Escape shutter flash: shows on the destroyable shutters the room set up while they hold their places.");

        RoomEnemyPlace SetUp(int ordinal, EnemyDefinitionId definition)
        {
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[ordinal];
            place.Bind(definition, place.Definition);
            place.SetSpawnWords(EnemyDefinitionId.ShutterShootable, _ => SuperMetroid.AssetExtraction.Enemies.PopulationWords.Parameters(2, 0));
            place.InitializeAsPlaced(RoomEnemyFrameFixtures.StandaloneSamus());
            place.Shading = PictureShading.Standard;
            return place;
        }
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
        Suite(nameof(VerifyEscapeShutterFlash), () => VerifyEscapeShutterFlash());
        Suite(nameof(ShutterAnimationsVerification.VerifyShutterAnimations), () => ShutterAnimationsVerification.VerifyShutterAnimations(rom));
        Suite(nameof(VerifyShutterExportFrames), () => VerifyShutterExportFrames(rom));
        return 0;
    }
}
