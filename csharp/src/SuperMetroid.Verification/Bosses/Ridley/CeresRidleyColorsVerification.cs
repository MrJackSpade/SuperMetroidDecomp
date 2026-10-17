using System.Reflection;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Frontend.Map.MapPresentationVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Verification.Bosses.Ridley;

/// <summary>Checks Ceres and Norfair Ridley palette installation against native CGRAM rows.</summary>
internal static class CeresRidleyColorsVerification
{
    /// <summary>Verifies extracted colors, edited palette effects, legacy override migration, and guarded ROM access.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    internal static void VerifyCeresRidleyColorOverride(string stockDirectory,
        string overrideDirectory, AreaMapPresentationCatalog original, ISnesAddressSpace rom,
        GameplayBasePaletteCatalog initialPalettes,
        MapPresentationInstalledRoomAssets fixtureAssets)
    {
        byte[] extracted = SuperMetroid.AssetExtraction.Bosses.Ridley.CeresRidleyColorExtractor.Extract(rom);
        CeresRidleyColorCatalog native = CeresRidleyColorCatalog.Load(
            new MemoryStream(extracted, writable: false));
        var forbidden = new HashSet<int>();
        Check(CeresRidleyPaletteRomData.StartColors,
            CeresRidleyPaletteRomData.StartColorCount, native.ResolveStart);
        for (int row = 0; row < CeresRidleyPaletteRomData.EyeFadeRowCount; row++)
            Check(CeresRidleyPaletteRomData.EyeFadeColors + row *
                CeresRidleyPaletteRomData.EyeFadeColorCount * sizeof(ushort),
                CeresRidleyPaletteRomData.EyeFadeColorCount,
                color => native.ResolveEyeFade(row, color));
        for (int row = 0; row < CeresRidleyPaletteRomData.BodyFadeRowCount; row++)
            Check(CeresRidleyPaletteRomData.BodyFadeColors + row *
                CeresRidleyPaletteRomData.BodyFadeColorCount * sizeof(ushort),
                CeresRidleyPaletteRomData.BodyFadeColorCount,
                color => native.ResolveBodyFade(row, color));
        for (int row = 0; row < CeresRidleyPaletteRomData.HealthRowCount; row++)
            Check(CeresRidleyPaletteRomData.HealthColors + row *
                CeresRidleyPaletteRomData.HealthColorCount * sizeof(ushort),
                CeresRidleyPaletteRomData.HealthColorCount,
                color => native.ResolveHealth(row, color));
        for (int row = 0; row < CeresRidleyPaletteRomData.AlarmRowCount; row++)
            Check(CeresRidleyPaletteRomData.AlarmColors + row *
                CeresRidleyPaletteRomData.AlarmColorCount * sizeof(ushort),
                CeresRidleyPaletteRomData.AlarmColorCount,
                color => native.ResolveAlarm(row, color));
        Check(CeresRidleyPaletteRomData.RetreatBgColors,
            CeresRidleyPaletteRomData.RetreatBgColorCount, native.ResolveRetreatBg);
        Check(CeresRidleyPaletteRomData.RetreatSharedColors,
            CeresRidleyPaletteRomData.RetreatSharedColorCount, native.ResolveRetreatShared);
        for (int row = 0; row < CeresRidleyPaletteRomData.BabyRowCount; row++)
            Check(CeresRidleyPaletteRomData.BabyColors + row *
                    CeresRidleyPaletteRomData.BabyColorCount * sizeof(ushort),
                CeresRidleyPaletteRomData.BabyColorCount,
                color => native.ResolveBaby(row, color));

        CeresRidleyColorDocument document = JsonSerializer.Deserialize<CeresRidleyColorDocument>(
            File.ReadAllBytes(Path.Combine(stockDirectory, CeresRidleyColorFormat.FileName)),
            ArtworkJson.Options)
            ?? throw new InvalidDataException("Stock Ceres Ridley colors are null.");
        Paint(document.Start);
        Paint(document.EyeFade[15]);
        Paint(document.BodyFade[0]);
        Paint(document.Health[0]);
        Paint(document.Health[1]);
        Paint(document.Health[2]);
        Paint((document.Alarm ?? throw new InvalidOperationException("Verification requires document.Alarm."))[7]);
        Paint(document.RetreatBg);
        Paint(document.RetreatShared);
        Paint((document.Baby ?? throw new InvalidOperationException("Verification requires document.Baby."))[1]);
        string replacement = Path.Combine(overrideDirectory, CeresRidleyColorFormat.FileName);
        File.WriteAllBytes(replacement, CeresRidleyColorCatalog.Write(document));
        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(stockDirectory, overrideDirectory);
        AssertTrue(edited.ContentIdentity != original.ContentIdentity,
            "Ceres Ridley palette edit changes installed identity");
        var runtime = SuperMetroid.Core.Runtime.SuperMetroidRuntime.Create(rom, HostGameplayOverrides.None, initialPaletteArt: initialPalettes);
        runtime.Artwork.MapPresentation = edited;
        AssertTrue(ReferenceEquals(edited.CeresRidleyColors, runtime.Enemies.Presentation.CeresRidleyColors),
            "installed runtime binds Ceres Ridley colors to enemy owner");
        runtime.Artwork.MapPresentation = original;
        AssertTrue(ReferenceEquals(original.CeresRidleyColors, runtime.Enemies.Presentation.CeresRidleyColors),
            "runtime rebind drops previous Ceres Ridley colors");
        var guarded = NewCeresRidleyColorReadGuard(rom, forbidden);
        var enemies = new RoomEnemySystem
        {
            Presentation = { CeresRidleyColors = edited.CeresRidleyColors },
            TileArtwork = fixtureAssets.EnemyTiles,
        };
        var cgram = new SnesCgram();
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        enemies.Presentation._cgram = cgram;
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        Action<RoomEnemySlot> initialize = enemies.CeresRidleyBehavior.InitializeCeresRidley;
        initialize(slot);
        CheckCgram(document.Start.Length, CeresRidleyPaletteRomData.StartCgramIndex,
            edited.CeresRidleyColors.ResolveStart, "Ceres Ridley initialization");

        Action<RidleyEnemyState> eyeTick = enemies.CeresRidleyBehavior.TickCeresRidleyEyeFade;
        Action<RoomEnemySlot, RidleyEnemyState> bodyTick = enemies.CeresRidleyBehavior.TickCeresRidleyBodyFade;
        Action<RidleyEnemyState> healthTick = enemies.CeresRidleyBehavior.UpdateCeresRidleyHealthPalette;
        var state = new RidleyEnemyState { Function = RidleyAiFunction.FadeInEyes };
        for (ushort step = 0; step < CeresRidleyEyeFadeDefinitions.PaletteStepCount; step++)
        {
            state.FadePaletteOffset = step;
            state.FunctionTimer = NativeCountdown.Start(0);
            eyeTick(state);
            int row = CeresRidleyEyeFadeDefinitions.Get(step).PaletteRow;
            AssertEqual(unchecked((ushort)(step + 1)), state.FadePaletteOffset,
                $"edited eye fade step {step} advances");
            CheckCgram(CeresRidleyPaletteRomData.EyeFadeColorCount,
                CeresRidleyPaletteRomData.EyeFadeCgramIndex,
                color => edited.CeresRidleyColors.ResolveEyeFade(row, color),
                $"Ceres Ridley eye fade step {step}");
        }
        state.Function = RidleyAiFunction.FadeInBody;
        for (int row = 0; row < CeresRidleyPaletteRomData.BodyFadeRowCount; row++)
        {
            state.FadePaletteOffset = row;
            state.FunctionTimer = NativeCountdown.Start(1);
            bodyTick(slot, state);
            AssertEqual(row == CeresRidleyPaletteRomData.BodyFadeRowCount - 1 ? 0 : row + 1,
                state.FadePaletteOffset, $"edited body fade row {row} advances to the next row, or resets after the last");
            CheckCgram(CeresRidleyPaletteRomData.BodyFadeColorCount,
                CeresRidleyPaletteRomData.BodyFadeBgCgramIndex,
                color => edited.CeresRidleyColors.ResolveBodyFade(row, color),
                $"Ceres Ridley body BG fade row {row}");
            CheckCgram(CeresRidleyPaletteRomData.BodyFadeColorCount,
                CeresRidleyPaletteRomData.BodyFadeObjCgramIndex,
                color => edited.CeresRidleyColors.ResolveBodyFade(row, color),
                $"Ceres Ridley body OBJ fade row {row}");
        }
        state.FightMode = RidleyFightMode.Fighting;
        foreach (ushort count in new ushort[] { 50, 69, 70, 99 })
        {
            state.HitCounter = count;
            healthTick(state);
            int row = count < 70 ? CeresRidleyPaletteRomData.HealthMidRow :
                CeresRidleyPaletteRomData.HealthLateRow;
            CheckCgram(CeresRidleyPaletteRomData.HealthColorCount,
                CeresRidleyPaletteRomData.HealthCgramIndex,
                color => edited.CeresRidleyColors.ResolveHealth(row, color),
                $"Ceres Ridley hit count {count}");
        }
        Suite(nameof(VerifyNorfairHealthPalette), () => VerifyNorfairHealthPalette());
        Suite(nameof(VerifyCeresAlarmPalette), () => VerifyCeresAlarmPalette());
        slot.Bind(EnemyDefinitionId.RidleyCeres, slot.Definition);
        RidleyEnemyState activeRidley = enemies.RidleyEncounter.Ridley ??
            throw new InvalidOperationException("Ceres Ridley initialization did not publish state.");
        activeRidley.Function = RidleyAiFunction.CeresRetreatDelay;
        activeRidley.FunctionTimer = NativeCountdown.Start(0);
        activeRidley.MovementAnimationEnabled = 0;
        enemies.CeresRidleyBehavior.RunCeresRidleyMain(slot, null, null);
        AssertEqual(RidleyAiFunction.CeresPublishEscapeHandoff, activeRidley.Function,
            "Ceres Ridley retreat publishes the Mode-7 handoff");
        CheckCgram(CeresRidleyPaletteRomData.RetreatBgColorCount,
            CeresRidleyPaletteRomData.RetreatBgCgramIndex,
            edited.CeresRidleyColors.ResolveRetreatBg, "Ceres Ridley retreat BG");
        CheckCgram(CeresRidleyPaletteRomData.RetreatSharedColorCount,
            CeresRidleyPaletteRomData.RetreatSharedBgCgramIndex,
            edited.CeresRidleyColors.ResolveRetreatShared, "Ceres Ridley retreat shared BG");
        CheckCgram(CeresRidleyPaletteRomData.RetreatSharedColorCount,
            CeresRidleyPaletteRomData.RetreatSharedObjCgramIndex,
            edited.CeresRidleyColors.ResolveRetreatShared, "Ceres Ridley retreat shared OBJ");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "Ceres Ridley installed colors avoid their original source ranges");

        // A version-one override predates the Baby rows. Preserve the user's
        // Ridley edits and supply only the new Baby colors from verified stock.
        byte[] legacyJson = JsonSerializer.SerializeToUtf8Bytes(
            document with { Version = (int)CeresRidleyColorVersion.PreBaby,
                Baby = null, Alarm = null },
            ArtworkJson.Options);
        File.WriteAllBytes(replacement, legacyJson);
        AreaMapPresentationCatalog migrated =
            AreaMapPresentationCatalog.Load(stockDirectory, overrideDirectory);
        AssertEqual(edited.CeresRidleyColors.ResolveStart(1),
            migrated.CeresRidleyColors.ResolveStart(1),
            "version-one Ceres Ridley override retains an edited color");
        AssertEqual(original.CeresRidleyColors.ResolveBaby(1, 0),
            migrated.CeresRidleyColors.ResolveBaby(1, 0),
            "version-one Ceres Ridley override inherits stock Baby colors");
        AssertEqual(original.CeresRidleyColors.ResolveAlarm(7, 1),
            migrated.CeresRidleyColors.ResolveAlarm(7, 1),
            "version-one Ceres Ridley override inherits stock alarm colors");
        byte[] preAlarmJson = JsonSerializer.SerializeToUtf8Bytes(
            document with { Version = (int)CeresRidleyColorVersion.PreAlarm, Alarm = null },
            ArtworkJson.Options);
        File.WriteAllBytes(replacement, preAlarmJson);
        AreaMapPresentationCatalog preAlarm =
            AreaMapPresentationCatalog.Load(stockDirectory, overrideDirectory);
        AssertEqual(edited.CeresRidleyColors.ResolveBaby(1, 1),
            preAlarm.CeresRidleyColors.ResolveBaby(1, 1),
            "version-two Ridley override retains its edited Baby colors");
        AssertEqual(original.CeresRidleyColors.ResolveAlarm(7, 1),
            preAlarm.CeresRidleyColors.ResolveAlarm(7, 1),
            "version-two Ridley override inherits stock alarm colors");
        File.WriteAllBytes(replacement, CeresRidleyColorCatalog.Write(document));

        document.EyeFade[0][0] = document.EyeFade[0][0] with { Red = 32 };
        AssertThrows<InvalidDataException>(() => CeresRidleyColorCatalog.Write(document),
            "Ceres Ridley colors reject invalid RGB5 channel");
        string replacementStock = Path.Combine(
            Path.GetDirectoryName(stockDirectory) ?? throw new InvalidOperationException("Stock maps have no parent."),
            "ceres-ridley-reextract");
        byte[] overrideBeforeRepair = File.ReadAllBytes(replacement);
        SuperMetroid.AssetExtraction.Frontend.Map.MapPresentationExtractor.Extract(rom, replacementStock, "test-provenance");
        AreaMapPresentationCatalog repaired = AreaMapPresentationCatalog.Load(replacementStock, overrideDirectory);
        AssertEqual(edited.ContentIdentity, repaired.ContentIdentity,
            "Ceres Ridley stock repair preserves override identity");
        AssertTrue(overrideBeforeRepair.SequenceEqual(File.ReadAllBytes(replacement)),
            "Ceres Ridley stock repair does not rewrite override");
        File.Delete(replacement);
        AssertEqual(original.ContentIdentity,
            AreaMapPresentationCatalog.Load(stockDirectory, overrideDirectory).ContentIdentity,
            "removing Ceres Ridley override restores stock identity");
        Console.WriteLine("Ceres/Ridley colors: 429 native words, Ceres initialization/fades/alarm/retreat/Baby, Norfair health thresholds, ROM guard and stock repair pass.");

        void VerifyCeresAlarmPalette()
        {
            // Compare every callback, including the three skipped frame counters
            // between each alarm color upload. Only the authored RGB5 row is editable.
            var nativeEnemy = new RoomEnemySystem { Presentation = { CeresRidleyColors = original.CeresRidleyColors } };
            var stockEnemy = new RoomEnemySystem { Presentation = { CeresRidleyColors = original.CeresRidleyColors } };
            var editedEnemy = new RoomEnemySystem { Presentation = { CeresRidleyColors = edited.CeresRidleyColors } };
            var nativeCgram = new SnesCgram();
            var stockCgram = new SnesCgram();
            var editedCgram = new SnesCgram();
            foreach ((RoomEnemySystem owner, ISnesAddressSpace addressSpace, SnesCgram palette)
                in new (RoomEnemySystem, ISnesAddressSpace, SnesCgram)[]
                {
                    (nativeEnemy, rom, nativeCgram),
                    (stockEnemy, guarded, stockCgram),
                    (editedEnemy, guarded, editedCgram),
                })
            {
                owner.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
                owner.Presentation._cgram = palette;
            }
            var nativeTick = new Action<RidleyEnemyState, ushort>(nativeEnemy.CeresRidleyBehavior.UpdateCeresSelfDestructPalette);
            var stockTick = new Action<RidleyEnemyState, ushort>(stockEnemy.CeresRidleyBehavior.UpdateCeresSelfDestructPalette);
            var editedTick = new Action<RidleyEnemyState, ushort>(editedEnemy.CeresRidleyBehavior.UpdateCeresSelfDestructPalette);
            var nativeState = new RidleyEnemyState();
            var stockState = new RidleyEnemyState();
            var editedState = new RidleyEnemyState();
            for (ushort frame = 0; frame < CeresRidleyPaletteRomData.AlarmRowCount * 4; frame++)
            {
                nativeTick(nativeState, frame);
                stockTick(stockState, frame);
                editedTick(editedState, frame);
                AssertEqual(nativeState.CeresEscapePaletteFrame, stockState.CeresEscapePaletteFrame,
                    $"stock Ceres alarm frame {frame} selection");
                AssertEqual(nativeState.CeresEscapePaletteFrame, editedState.CeresEscapePaletteFrame,
                    $"edited Ceres alarm frame {frame} selection");
                for (int color = 0; color < SnesCgram.ColorCount; color++)
                {
                    AssertEqual(nativeCgram.Colors[color], stockCgram.Colors[color],
                        $"stock Ceres alarm frame {frame} CGRAM {color}");
                    Bgr555 expected = editedState.CeresEscapePaletteFrame == 7 &&
                        color >= CeresRidleyPaletteRomData.AlarmCgramIndex &&
                        color < CeresRidleyPaletteRomData.AlarmCgramIndex +
                            CeresRidleyPaletteRomData.AlarmColorCount
                        ? edited.CeresRidleyColors.ResolveAlarm(7,
                            color - CeresRidleyPaletteRomData.AlarmCgramIndex)
                        : nativeCgram.Colors[color];
                    AssertEqual(expected, editedCgram.Colors[color],
                        $"edited Ceres alarm frame {frame} CGRAM {color}");
                }
            }
            AssertEqual((ushort)0, editedState.CeresEscapePaletteFrame,
                "Ceres alarm frame sixteen wraps to zero");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                "installed Ceres alarm never reads its source color rows");
        }

        void VerifyNorfairHealthPalette()
        {
            // Drive the real Norfair palette callback at both sides of every
            // health threshold. The extracted stock catalog supplies full-CGRAM
            // parity; the guarded installed path proves no copied ROM colors are read.
            var nativeEnemy = new RoomEnemySystem { Presentation = { CeresRidleyColors = original.CeresRidleyColors } };
            var stockEnemy = new RoomEnemySystem { Presentation = { CeresRidleyColors = original.CeresRidleyColors } };
            var editedEnemy = new RoomEnemySystem { Presentation = { CeresRidleyColors = edited.CeresRidleyColors } };
            var nativeCgram = new SnesCgram();
            var stockCgram = new SnesCgram();
            var editedCgram = new SnesCgram();
            foreach ((RoomEnemySystem owner, ISnesAddressSpace addressSpace, SnesCgram palette)
                in new (RoomEnemySystem, ISnesAddressSpace, SnesCgram)[]
                {
                    (nativeEnemy, rom, nativeCgram),
                    (stockEnemy, guarded, stockCgram),
                    (editedEnemy, guarded, editedCgram),
                })
            {
                owner.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
                owner.Presentation._cgram = palette;
            }
            var nativeTick = new Action<RoomEnemySlot, RidleyEnemyState>(nativeEnemy.RidleyBehavior.UpdateNorfairRidleyHealthPalette);
            var stockTick = new Action<RoomEnemySlot, RidleyEnemyState>(stockEnemy.RidleyBehavior.UpdateNorfairRidleyHealthPalette);
            var editedTick = new Action<RoomEnemySlot, RidleyEnemyState>(editedEnemy.RidleyBehavior.UpdateNorfairRidleyHealthPalette);
            RoomEnemySlot nativeSlot = nativeEnemy.SlotTable.Slots[0];
            RoomEnemySlot stockSlot = stockEnemy.SlotTable.Slots[0];
            RoomEnemySlot editedSlot = editedEnemy.SlotTable.Slots[0];
            var nativeState = new RidleyEnemyState();
            var stockState = new RidleyEnemyState();
            var editedState = new RidleyEnemyState();
            foreach (ushort health in new ushort[] { 9000, 8999, 5400, 5399, 1800, 1799, 0 })
            {
                nativeSlot.Health = stockSlot.Health = editedSlot.Health = health;
                Bgr555[] before = editedCgram.Colors.ToArray();
                nativeTick(nativeSlot, nativeState);
                stockTick(stockSlot, stockState);
                editedTick(editedSlot, editedState);
                ushort stage = health switch
                {
                    < 1800 => 3,
                    < 5400 => 2,
                    < 9000 => 1,
                    _ => 0,
                };
                AssertEqual(stage, nativeState.HealthStage,
                    $"native Norfair Ridley health {health} stage");
                AssertEqual(stage, stockState.HealthStage,
                    $"stock Norfair Ridley health {health} stage");
                AssertEqual(stage, editedState.HealthStage,
                    $"edited Norfair Ridley health {health} stage");
                for (int color = 0; color < SnesCgram.ColorCount; color++)
                {
                    AssertEqual(nativeCgram.Colors[color], stockCgram.Colors[color],
                        $"stock Norfair Ridley health {health} CGRAM {color}");
                    Bgr555 expected = stage != 0 &&
                        color >= CeresRidleyPaletteRomData.HealthCgramIndex &&
                        color < CeresRidleyPaletteRomData.HealthCgramIndex +
                            CeresRidleyPaletteRomData.HealthColorCount
                        ? edited.CeresRidleyColors.ResolveHealth(
                            stage - 1, color - CeresRidleyPaletteRomData.HealthCgramIndex)
                        : before[color];
                    AssertEqual(expected, editedCgram.Colors[color],
                        $"edited Norfair Ridley health {health} CGRAM {color}");
                }
            }
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                "Norfair Ridley installed health colors avoid their original ROM rows");
        }

        void Check(int source, int count, Func<int, Bgr555> resolve)
        {
            for (int color = 0; color < count; color++)
            {
                int address = source + color * sizeof(ushort);
                forbidden.Add(address);
                forbidden.Add(address + 1);
                AssertEqual(ReadNativeCeresRidleyColor(rom, address), resolve(color),
                    $"Ceres Ridley source ${source:X6} color {color}");
            }
        }
        void CheckCgram(int count, int destination, Func<int, Bgr555> resolve, string context)
        {
            for (int color = 0; color < count; color++)
                AssertEqual(resolve(color), cgram.Colors[destination + color],
                    $"{context} color {color}");
        }
        static void Paint(PaletteRgb5[] colors)
        {
            PaletteRgb5 previous = colors[1];
            colors[1] = previous with { Blue = previous.Blue == 31 ? 30 : previous.Blue + 1 };
        }
    }

    /// <summary>Rejects reads from every retail color word covered by installed Ceres Ridley palettes.</summary>
    private static CartridgeReadGuard NewCeresRidleyColorReadGuard(ISnesAddressSpace source, HashSet<int> forbidden) =>
        new MutableCartridgeReadGuard(source)
            .Forbid(forbidden.Contains, address => $"Ceres Ridley color reread ${address:X6}.");

    /// <summary>Reads one BGR555 word from the pinned retail palette oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Ceres Ridley's native palette rows")]
    private static ushort ReadNativeCeresRidleyColor(ISnesAddressSpace rom, int address) =>
        RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), address);
}
