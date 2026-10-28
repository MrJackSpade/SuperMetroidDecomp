using SuperMetroid.Core.Samus.Palettes;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Frontend.Map.MapPresentationVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Ridley;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Bosses.Ridley;

/// <summary>Checks Ceres and Norfair Ridley palette installation against native CGRAM rows.</summary>
internal static class CeresRidleyColorsVerification
{
    /// <summary>Verifies extracted colors, edited palette effects, legacy override migration, and guarded ROM access.</summary>
    internal static void VerifyCeresRidleyColorOverride(string stockDirectory,
        string overrideDirectory, AreaMapPresentationCatalog original, ISnesAddressSpace rom,
        GameplayBasePaletteCatalog initialPalettes,
        MapPresentationInstalledRoomAssets fixtureAssets)
    {
        byte[] extracted = SuperMetroid.AssetExtraction.Bosses.Ridley.CeresRidleyColorExtractor.Extract(rom);
        CeresRidleyColorCatalog native = CeresRidleyColorFile.Read(
            new MemoryStream(extracted, writable: false));
        var forbidden = new HashSet<int>();
        Check(RidleySourceDefinitions.Ceres.StartColors,
            CeresRidleyPaletteDefinitions.StartColorCount, native.ResolveStart);
        for (int row = 0; row < CeresRidleyPaletteDefinitions.EyeFadeRowCount; row++)
            Check(EnemySourceDefinitions.CeresRidleyEyeFadeRows + row *
                CeresRidleyPaletteDefinitions.EyeFadeColorCount * sizeof(ushort),
                CeresRidleyPaletteDefinitions.EyeFadeColorCount,
                color => native.ResolveEyeFade(row, color));
        for (int row = 0; row < CeresRidleyPaletteDefinitions.BodyFadeRowCount; row++)
            Check(RidleySourceDefinitions.Ceres.BodyFadeColors + row *
                CeresRidleyPaletteDefinitions.BodyFadeColorCount * sizeof(ushort),
                CeresRidleyPaletteDefinitions.BodyFadeColorCount,
                color => native.ResolveBodyFade(row, color));
        for (int row = 0; row < CeresRidleyPaletteDefinitions.HealthRowCount; row++)
            Check(RidleySourceDefinitions.Ceres.HealthColors + row *
                CeresRidleyPaletteDefinitions.HealthColorCount * sizeof(ushort),
                CeresRidleyPaletteDefinitions.HealthColorCount,
                color => native.ResolveHealth(row, color));
        for (int row = 0; row < CeresRidleyPaletteDefinitions.AlarmRowCount; row++)
            Check(RidleySourceDefinitions.Ceres.AlarmColors + row *
                CeresRidleyPaletteDefinitions.AlarmColorCount * sizeof(ushort),
                CeresRidleyPaletteDefinitions.AlarmColorCount,
                color => native.ResolveAlarm(row, color));
        Check(RidleySourceDefinitions.Ceres.RetreatBgColors,
            CeresRidleyPaletteDefinitions.RetreatBgColorCount, native.ResolveRetreatBg);
        Check(RidleySourceDefinitions.Ceres.RetreatSharedColors,
            CeresRidleyPaletteDefinitions.RetreatSharedColorCount, native.ResolveRetreatShared);
        for (int row = 0; row < CeresRidleyPaletteDefinitions.BabyRowCount; row++)
            Check(RidleySourceDefinitions.Ceres.BabyColors + row *
                    CeresRidleyPaletteDefinitions.BabyColorCount * sizeof(ushort),
                CeresRidleyPaletteDefinitions.BabyColorCount,
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
        File.WriteAllBytes(replacement, CeresRidleyColorFile.Write(document));
        AreaMapPresentationCatalog edited = MapPresentationFiles.Load(stockDirectory, overrideDirectory);
        AssertTrue(edited.ContentIdentity != original.ContentIdentity,
            "Ceres Ridley palette edit changes installed identity");
        var runtime = SuperMetroid.Core.Runtime.SuperMetroidRuntime.Create(HostGameplayOverrides.None, initialPaletteArt: initialPalettes);
        runtime.Artwork.MapPresentation = edited;
        AssertTrue(ReferenceEquals(edited.CeresRidleyColors, runtime.BossArtwork.CeresRidleyColors),
            "installed runtime binds Ceres Ridley colors to enemy owner");
        runtime.Artwork.MapPresentation = original;
        AssertTrue(ReferenceEquals(original.CeresRidleyColors, runtime.BossArtwork.CeresRidleyColors),
            "runtime rebind drops previous Ceres Ridley colors");
        var guarded = NewCeresRidleyColorReadGuard(rom, forbidden);
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance)
        {
            BossArtworkBinding = { CeresRidleyColors = edited.CeresRidleyColors },
            TileArtwork = fixtureAssets.EnemyTiles.Enemies,
            BossArtwork = fixtureAssets.EnemyTiles.Bosses,
            AreaArtwork = fixtureAssets.EnemyTiles.Areas,
        };
        var cgram = new SamusColourRig();
        enemies.Presentation.BindColours(cgram.Gameplay);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        Action<RoomEnemySlot> initialize = enemies.CeresRidleyBehavior.Flight.InitializeCeresRidley;
        initialize(slot);
        CheckCgram(document.Start.Length, CeresRidleyPaletteDefinitionsConstants.StartCgramIndex,
            edited.CeresRidleyColors.ResolveStart, "Ceres Ridley initialization");

        Action<RidleyEnemyState> eyeTick = enemies.CeresRidleyBehavior.Fades.TickCeresRidleyEyeFade;
        Action<RoomEnemySlot, RidleyEnemyState> bodyTick = enemies.CeresRidleyBehavior.Fades.TickCeresRidleyBodyFade;
        Action<RidleyEnemyState> healthTick = enemies.CeresRidleyBehavior.Hits.UpdateCeresRidleyHealthPalette;
        var state = new RidleyEnemyState { Function = RidleyAiFunction.FadeInEyes };
        for (ushort step = 0; step < CeresRidleyEyeFadeDefinitions.PaletteStepCount; step++)
        {
            state.FadePaletteOffset = step;
            state.FunctionTimer = WrappingCountdown.Start(0);
            eyeTick(state);
            int row = CeresRidleyEyeFade.Get(step).PaletteRow;
            AssertEqual(unchecked((ushort)(step + 1)), state.FadePaletteOffset,
                $"edited eye fade step {step} advances");
            CheckCgram(CeresRidleyPaletteDefinitions.EyeFadeColorCount,
                CeresRidleyPaletteDefinitionsConstants.EyeFadeCgramIndex,
                color => edited.CeresRidleyColors.ResolveEyeFade(row, color),
                $"Ceres Ridley eye fade step {step}");
        }
        state.Function = RidleyAiFunction.FadeInBody;
        for (int row = 0; row < CeresRidleyPaletteDefinitions.BodyFadeRowCount; row++)
        {
            state.FadePaletteOffset = row;
            state.FunctionTimer = WrappingCountdown.Start(1);
            bodyTick(slot, state);
            AssertEqual(row == CeresRidleyPaletteDefinitions.BodyFadeRowCount - 1 ? 0 : row + 1,
                state.FadePaletteOffset, $"edited body fade row {row} advances to the next row, or resets after the last");
            CheckCgram(CeresRidleyPaletteDefinitions.BodyFadeColorCount,
                CeresRidleyPaletteDefinitionsConstants.BodyFadeBgCgramIndex,
                color => edited.CeresRidleyColors.ResolveBodyFade(row, color),
                $"Ceres Ridley body BG fade row {row}");
            CheckCgram(CeresRidleyPaletteDefinitions.BodyFadeColorCount,
                CeresRidleyPaletteDefinitionsConstants.BodyFadeObjCgramIndex,
                color => edited.CeresRidleyColors.ResolveBodyFade(row, color),
                $"Ceres Ridley body OBJ fade row {row}");
        }
        state.FightMode = RidleyFightMode.Fighting;
        foreach (ushort count in new ushort[] { 50, 69, 70, 99 })
        {
            state.HitCounter = count;
            healthTick(state);
            int row = count < 70 ? CeresRidleyPaletteDefinitions.HealthMidRow :
                CeresRidleyPaletteDefinitions.HealthLateRow;
            CheckCgram(CeresRidleyPaletteDefinitions.HealthColorCount,
                CeresRidleyPaletteDefinitionsConstants.HealthCgramIndex,
                color => edited.CeresRidleyColors.ResolveHealth(row, color),
                $"Ceres Ridley hit count {count}");
        }
        Suite(nameof(VerifyNorfairHealthPalette), () => VerifyNorfairHealthPalette());
        Suite(nameof(VerifyCeresAlarmPalette), () => VerifyCeresAlarmPalette());
        slot.Bind(EnemyDefinitionId.RidleyCeres, slot.Definition);
        RidleyEnemyState activeRidley = enemies.RidleyEncounter.Ridley ??
            throw new InvalidOperationException("Ceres Ridley initialization did not publish state.");
        activeRidley.Function = RidleyAiFunction.CeresRetreatDelay;
        activeRidley.FunctionTimer = WrappingCountdown.Start(0);
        activeRidley.MovementAnimationEnabled = false;
        enemies.CeresRidleyBehavior.RunCeresRidleyMain(slot, null);
        AssertEqual(RidleyAiFunction.CeresPublishEscapeHandoff, activeRidley.Function,
            "Ceres Ridley retreat publishes the Mode-7 handoff");
        CheckCgram(CeresRidleyPaletteDefinitions.RetreatBgColorCount,
            CeresRidleyPaletteDefinitionsConstants.RetreatBgCgramIndex,
            edited.CeresRidleyColors.ResolveRetreatBg, "Ceres Ridley retreat BG");
        CheckCgram(CeresRidleyPaletteDefinitions.RetreatSharedColorCount,
            CeresRidleyPaletteDefinitionsConstants.RetreatSharedBgCgramIndex,
            edited.CeresRidleyColors.ResolveRetreatShared, "Ceres Ridley retreat shared BG");
        CheckCgram(CeresRidleyPaletteDefinitions.RetreatSharedColorCount,
            CeresRidleyPaletteDefinitionsConstants.RetreatSharedObjCgramIndex,
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
            MapPresentationFiles.Load(stockDirectory, overrideDirectory);
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
            MapPresentationFiles.Load(stockDirectory, overrideDirectory);
        AssertEqual(edited.CeresRidleyColors.ResolveBaby(1, 1),
            preAlarm.CeresRidleyColors.ResolveBaby(1, 1),
            "version-two Ridley override retains its edited Baby colors");
        AssertEqual(original.CeresRidleyColors.ResolveAlarm(7, 1),
            preAlarm.CeresRidleyColors.ResolveAlarm(7, 1),
            "version-two Ridley override inherits stock alarm colors");
        File.WriteAllBytes(replacement, CeresRidleyColorFile.Write(document));

        document.EyeFade[0][0] = document.EyeFade[0][0] with { Red = 32 };
        AssertThrows<InvalidDataException>(() => CeresRidleyColorFile.Write(document),
            "Ceres Ridley colors reject invalid RGB5 channel");
        string replacementStock = Path.Combine(
            Path.GetDirectoryName(stockDirectory) ?? throw new InvalidOperationException("Stock maps have no parent."),
            "ceres-ridley-reextract");
        byte[] overrideBeforeRepair = File.ReadAllBytes(replacement);
        SuperMetroid.AssetExtraction.Frontend.Map.MapPresentationExtractor.Extract(rom, replacementStock, "test-provenance");
        AreaMapPresentationCatalog repaired = MapPresentationFiles.Load(replacementStock, overrideDirectory);
        AssertEqual(edited.ContentIdentity, repaired.ContentIdentity,
            "Ceres Ridley stock repair preserves override identity");
        AssertTrue(overrideBeforeRepair.SequenceEqual(File.ReadAllBytes(replacement)),
            "Ceres Ridley stock repair does not rewrite override");
        File.Delete(replacement);
        AssertEqual(original.ContentIdentity,
            MapPresentationFiles.Load(stockDirectory, overrideDirectory).ContentIdentity,
            "removing Ceres Ridley override restores stock identity");
        Console.WriteLine("Ceres/Ridley colors: 429 native words, Ceres initialization/fades/alarm/retreat/Baby, Norfair health thresholds, ROM guard and stock repair pass.");

        void VerifyCeresAlarmPalette()
        {
            // Compare every callback, including the three skipped frame counters
            // between each alarm color upload. Only the authored RGB5 row is editable.
            var nativeEnemy = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { BossArtworkBinding = { CeresRidleyColors = original.CeresRidleyColors } };
            var stockEnemy = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { BossArtworkBinding = { CeresRidleyColors = original.CeresRidleyColors } };
            var editedEnemy = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { BossArtworkBinding = { CeresRidleyColors = edited.CeresRidleyColors } };
            var nativeCgram = new SamusColourRig();
            var stockCgram = new SamusColourRig();
            var editedCgram = new SamusColourRig();
            foreach ((RoomEnemySystem owner, ISnesAddressSpace addressSpace, SamusColourRig palette)
                in new VerifyCeresAlarmPaletteCase[]
                {
                    new VerifyCeresAlarmPaletteCase(nativeEnemy, rom, nativeCgram),
                    new VerifyCeresAlarmPaletteCase(stockEnemy, guarded, stockCgram),
                    new VerifyCeresAlarmPaletteCase(editedEnemy, guarded, editedCgram),
                })
            {
                owner.Presentation.BindColours(palette.Gameplay);
            }
            var nativeTick = new Action<RidleyEnemyState, int>(nativeEnemy.CeresRidleyBehavior.Getaway.UpdateCeresSelfDestructPalette);
            var stockTick = new Action<RidleyEnemyState, int>(stockEnemy.CeresRidleyBehavior.Getaway.UpdateCeresSelfDestructPalette);
            var editedTick = new Action<RidleyEnemyState, int>(editedEnemy.CeresRidleyBehavior.Getaway.UpdateCeresSelfDestructPalette);
            var nativeState = new RidleyEnemyState();
            var stockState = new RidleyEnemyState();
            var editedState = new RidleyEnemyState();
            for (ushort frame = 0; frame < CeresRidleyPaletteDefinitions.AlarmRowCount * 4; frame++)
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
                    RgbColor expected = editedState.CeresEscapePaletteFrame == 7 &&
                        color >= CeresRidleyPaletteDefinitionsConstants.AlarmCgramIndex &&
                        color < CeresRidleyPaletteDefinitionsConstants.AlarmCgramIndex +
                            CeresRidleyPaletteDefinitions.AlarmColorCount
                        ? edited.CeresRidleyColors.ResolveAlarm(7,
                            color - CeresRidleyPaletteDefinitionsConstants.AlarmCgramIndex)
                        : nativeCgram.Colors[color];
                    AssertEqual(expected, editedCgram.Colors[color],
                        $"edited Ceres alarm frame {frame} CGRAM {color}");
                }
            }
            AssertEqual(0, editedState.CeresEscapePaletteFrame,
                "Ceres alarm frame sixteen wraps to zero");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                "installed Ceres alarm never reads its source color rows");
        }

        void VerifyNorfairHealthPalette()
        {
            // Drive the real Norfair palette callback at both sides of every
            // health threshold. The extracted stock catalog supplies full-CGRAM
            // parity; the guarded installed path proves no copied ROM colors are read.
            var nativeEnemy = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { BossArtworkBinding = { CeresRidleyColors = original.CeresRidleyColors } };
            var stockEnemy = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { BossArtworkBinding = { CeresRidleyColors = original.CeresRidleyColors } };
            var editedEnemy = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { BossArtworkBinding = { CeresRidleyColors = edited.CeresRidleyColors } };
            var nativeCgram = new SamusColourRig();
            var stockCgram = new SamusColourRig();
            var editedCgram = new SamusColourRig();
            foreach ((RoomEnemySystem owner, ISnesAddressSpace addressSpace, SamusColourRig palette)
                in new VerifyCeresAlarmPaletteCase[]
                {
                    new VerifyCeresAlarmPaletteCase(nativeEnemy, rom, nativeCgram),
                    new VerifyCeresAlarmPaletteCase(stockEnemy, guarded, stockCgram),
                    new VerifyCeresAlarmPaletteCase(editedEnemy, guarded, editedCgram),
                })
            {
                owner.Presentation.BindColours(palette.Gameplay);
            }
            var nativeTick = new Action<RoomEnemySlot, RidleyEnemyState>(nativeEnemy.RidleyBehavior.Interactions.UpdateNorfairRidleyHealthPalette);
            var stockTick = new Action<RoomEnemySlot, RidleyEnemyState>(stockEnemy.RidleyBehavior.Interactions.UpdateNorfairRidleyHealthPalette);
            var editedTick = new Action<RoomEnemySlot, RidleyEnemyState>(editedEnemy.RidleyBehavior.Interactions.UpdateNorfairRidleyHealthPalette);
            RoomEnemySlot nativeSlot = nativeEnemy.SlotTable.Slots[0];
            RoomEnemySlot stockSlot = stockEnemy.SlotTable.Slots[0];
            RoomEnemySlot editedSlot = editedEnemy.SlotTable.Slots[0];
            var nativeState = new RidleyEnemyState();
            var stockState = new RidleyEnemyState();
            var editedState = new RidleyEnemyState();
            foreach (ushort health in new ushort[] { 9000, 8999, 5400, 5399, 1800, 1799, 0 })
            {
                nativeSlot.Health = stockSlot.Health = editedSlot.Health = health;
                RgbColor[] before = editedCgram.Colors.ToArray();
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
                    RgbColor expected = stage != 0 &&
                        color >= CeresRidleyPaletteDefinitionsConstants.HealthCgramIndex &&
                        color < CeresRidleyPaletteDefinitionsConstants.HealthCgramIndex +
                            CeresRidleyPaletteDefinitions.HealthColorCount
                        ? edited.CeresRidleyColors.ResolveHealth(
                            stage - 1, color - CeresRidleyPaletteDefinitionsConstants.HealthCgramIndex)
                        : before[color];
                    AssertEqual(expected, editedCgram.Colors[color],
                        $"edited Norfair Ridley health {health} CGRAM {color}");
                }
            }
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                "Norfair Ridley installed health colors avoid their original ROM rows");
        }

        void Check(int source, int count, Func<int, RgbColor> resolve)
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
        void CheckCgram(int count, int destination, Func<int, RgbColor> resolve, string context)
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
    private static ushort ReadNativeCeresRidleyColor(ISnesAddressSpace rom, int address) =>
        RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), address);
}
