using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Frontend.Map.MapPresentationVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Bosses.Ridley;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Verification.Areas.Ceres;
using SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork;
using SuperMetroid.AssetExtraction;

namespace SuperMetroid.Verification.Bosses.Ridley;

/// <summary>Checks Ceres and Norfair Ridley palette installation against native CGRAM rows.</summary>
internal static class CeresRidleyColorsVerification
{
    /// <summary>Verifies extracted colors, installed palette effects, and guarded ROM access.</summary>
    internal static void VerifyCeresRidleyColors(string stockDirectory,
        AreaMapPresentation original, ISnesAddressSpace rom,
        MapPresentationInstalledRoomAssets fixtureAssets)
    {
        byte[] extracted = SuperMetroid.AssetExtraction.Bosses.Ridley.CeresRidleyColorExtractor.Extract(rom);
        CeresRidleyColorCatalog native = CeresRidleyColorFile.Read(
            new MemoryStream(extracted, writable: false));
        var forbidden = new HashSet<int>();
        Check(RidleySourceDefinitions.Ceres.StartColors,
            CeresRidleyColorFormat.StartColorCount, native.ResolveStart);
        for (int row = 0; row < CeresRidleyColorFormat.EyeFadeShadeCount; row++)
            Check(EnemySourceDefinitions.CeresRidleyEyeFadeRows + row *
                CeresRidleyColorFormat.EyeFadeColorCount * sizeof(ushort),
                CeresRidleyColorFormat.EyeFadeColorCount,
                color => native.ResolveEyeFade(row, color));
        for (int row = 0; row < CeresRidleyShadeDefinitions.BodyFadeShadeCount; row++)
            Check(RidleySourceDefinitions.Ceres.BodyFadeColors + row *
                CeresRidleyColorFormat.BodyFadeColorCount * sizeof(ushort),
                CeresRidleyColorFormat.BodyFadeColorCount,
                color => native.ResolveBodyFade(row, color));
        for (int row = 0; row < CeresRidleyColorFormat.HealthBandCount; row++)
            Check(RidleySourceDefinitions.Ceres.HealthColors + row *
                CeresRidleyColorFormat.HealthColorCount * sizeof(ushort),
                CeresRidleyColorFormat.HealthColorCount,
                color => native.ResolveHealth(row, color));
        for (int row = 0; row < CeresRidleyColorFormat.AlarmStepCount; row++)
            Check(RidleySourceDefinitions.Ceres.AlarmColors + row *
                CeresRidleyColorFormat.AlarmColorCount * sizeof(ushort),
                CeresRidleyColorFormat.AlarmColorCount,
                color => native.ResolveAlarm(row, color));
        Check(RidleySourceDefinitions.Ceres.RetreatBgColors,
            CeresRidleyColorFormat.RetreatBgColorCount, native.ResolveRetreatBg);
        Check(RidleySourceDefinitions.Ceres.RetreatSharedColors,
            CeresRidleyColorFormat.RetreatSharedColorCount, native.ResolveRetreatShared);
        // Each Baby shade is read where the native draw list's palette operand for that shade points.
        foreach (CeresBabyNativeListAddressesShadeOperand shadeOperand in CeresBabyNativeListAddresses.ShadeOperands)
            Check(CeresBabyNativeListAddresses.Bank << 16 | shadeOperand.Operand,
                CeresRidleyColorFormat.BabyColorCount,
                color => native.ResolveBaby(shadeOperand.Shade, color));

        CeresRidleyColorDocument document = JsonSerializer.Deserialize<CeresRidleyColorDocument>(
            File.ReadAllBytes(Path.Combine(stockDirectory, CeresRidleyColorFormat.FileName)),
            ArtworkJson.Options)
            ?? throw new InvalidDataException("Stock Ceres Ridley colors are null.");
        var runtime = SuperMetroid.Core.Runtime.SuperMetroidRuntime.Create(GameplayAssists.None, InstalledFixtures.BareRuntimeArtwork);
        runtime.Artwork.MapPresentation = original;
        var guarded = NewCeresRidleyColorReadGuard(rom, forbidden);
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(fixtureAssets.EnemyTiles.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance)
        {
            BossArtwork = fixtureAssets.EnemyTiles.Bosses,
            
        };
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        Action<RoomEnemySlot> initialize = enemies.CeresRidleyBehavior.Flight.InitializeCeresRidley;
        initialize(slot);
        CheckCgram(enemies.RidleyEncounter.Ridley ?? throw new InvalidOperationException("Ceres Ridley initialization did not publish state."),
            document.Start.Length, CeresRidleyShadeDefinitionsConstants.StartCgramIndex,
            InstalledFixtures.ImportOf(original).CeresRidleyColors.ResolveStart, "Ceres Ridley initialization");

        Action<RidleyEnemyState> eyeTick = RidleyFadeIn.TickEyeFadeIn;
        Action<RoomEnemySlot, RidleyEnemyState> bodyTick = enemies.CeresRidleyBehavior.FadeIn.TickBodyFadeIn;
        Action<RidleyEnemyState> healthTick = CeresRidleyHits.UpdateCeresRidleyHealthBand;
        var state = new RidleyEnemyState { Phase = RidleyPhase.FadeInEyes };
        for (ushort step = 0; step < CeresRidleyEyeFadeDefinitions.PaletteStepCount; step++)
        {
            state.FadeStep = step;
            state.PhaseTimer = 0;
            eyeTick(state);
            int row = CeresRidleyEyeFade.Get(step).Shade;
            AssertEqual(unchecked((ushort)(step + 1)), state.FadeStep,
                $"installed eye fade step {step} advances");
            CheckCgram(state, CeresRidleyColorFormat.EyeFadeColorCount,
                CeresRidleyShadeDefinitionsConstants.EyeFadeCgramIndex,
                color => InstalledFixtures.ImportOf(original).CeresRidleyColors.ResolveEyeFade(row, color),
                $"Ceres Ridley eye fade step {step}");
        }
        state.Phase = RidleyPhase.FadeInBody;
        for (int row = 0; row < CeresRidleyShadeDefinitions.BodyFadeShadeCount; row++)
        {
            state.FadeStep = row;
            state.PhaseTimer = 1;
            bodyTick(slot, state);
            AssertEqual(row == CeresRidleyShadeDefinitions.BodyFadeShadeCount - 1 ? 0 : row + 1,
                state.FadeStep, $"installed body fade row {row} advances to the next row, or resets after the last");
            CheckCgram(state, CeresRidleyColorFormat.BodyFadeColorCount,
                CeresRidleyShadeDefinitionsConstants.BodyFadeBgCgramIndex,
                color => InstalledFixtures.ImportOf(original).CeresRidleyColors.ResolveBodyFade(row, color),
                $"Ceres Ridley body BG fade row {row}");
            CheckCgram(state, CeresRidleyColorFormat.BodyFadeColorCount,
                CeresRidleyShadeDefinitionsConstants.BodyFadeObjCgramIndex,
                color => InstalledFixtures.ImportOf(original).CeresRidleyColors.ResolveBodyFade(row, color),
                $"Ceres Ridley body OBJ fade row {row}");
        }
        state.FightMode = RidleyFightMode.Fighting;
        foreach (ushort count in new ushort[] { 50, 69, 70, 99 })
        {
            state.HitCounter = count;
            healthTick(state);
            int row = count < 70 ? CeresRidleyShadeDefinitions.HealthMidBand :
                CeresRidleyShadeDefinitions.HealthLateBand;
            CheckCgram(state, CeresRidleyColorFormat.HealthColorCount,
                CeresRidleyShadeDefinitionsConstants.HealthCgramIndex,
                color => InstalledFixtures.ImportOf(original).CeresRidleyColors.ResolveHealth(row, color),
                $"Ceres Ridley hit count {count}");
        }
        Suite(nameof(VerifyNorfairHealthPalette), () => VerifyNorfairHealthPalette());
        Suite(nameof(VerifyCeresAlarmPalette), () => VerifyCeresAlarmPalette());
        slot.Bind(EnemyDefinitionId.RidleyCeres, slot.Definition);
        RidleyEnemyState activeRidley = enemies.RidleyEncounter.Ridley ??
            throw new InvalidOperationException("Ceres Ridley initialization did not publish state.");
        activeRidley.Phase = RidleyPhase.CeresRetreatDelay;
        activeRidley.PhaseTimer = 0;
        activeRidley.MovementAnimationEnabled = false;
        enemies.CeresRidleyBehavior.RunCeresRidleyMain(slot, null);
        AssertEqual(RidleyPhase.CeresPublishEscapeHandoff, activeRidley.Phase,
            "Ceres Ridley retreat publishes the Mode-7 handoff");
        CheckCgram(activeRidley, CeresRidleyColorFormat.RetreatBgColorCount,
            CeresRidleyShadeDefinitionsConstants.RetreatBgCgramIndex,
            InstalledFixtures.ImportOf(original).CeresRidleyColors.ResolveRetreatBg, "Ceres Ridley retreat BG");
        CheckCgram(activeRidley, CeresRidleyColorFormat.RetreatSharedColorCount,
            CeresRidleyShadeDefinitionsConstants.RetreatSharedBgCgramIndex,
            InstalledFixtures.ImportOf(original).CeresRidleyColors.ResolveRetreatShared, "Ceres Ridley retreat shared BG");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "Ceres Ridley installed colors avoid their original source ranges");

        document.EyeFade[0][0] = document.EyeFade[0][0] with { Red = 32 };
        AssertThrows<InvalidDataException>(() => CeresRidleyColorFile.Read(new MemoryStream(CeresRidleyColorFile.Write(document), writable: false)),
            "reading Ceres Ridley colors rejects an invalid RGB5 channel");
        Console.WriteLine("Ceres/Ridley colors: 429 native words, Ceres initialization/fades/alarm/retreat/Baby, Norfair health thresholds and ROM guard pass.");

        void VerifyCeresAlarmPalette()
        {
            // Compare every callback, including the three skipped frame counters
            // between each alarm color upload.
            var nativeTick = new Action<RidleyEnemyState, int>(CeresRidleyGetaway.UpdateCeresSelfDestructAlarm);
            var stockTick = new Action<RidleyEnemyState, int>(CeresRidleyGetaway.UpdateCeresSelfDestructAlarm);
            var nativeState = new RidleyEnemyState();
            var stockState = new RidleyEnemyState();
            for (ushort frame = 0; frame < CeresRidleyColorFormat.AlarmStepCount * 4; frame++)
            {
                nativeTick(nativeState, frame);
                stockTick(stockState, frame);
                RgbColor[] nativeCgram = RidleyLaidColours.Ceres(nativeState, original);
                RgbColor[] stockCgram = RidleyLaidColours.Ceres(stockState, original);
                AssertEqual(nativeState.SelfDestructAlarmStep, stockState.SelfDestructAlarmStep,
                    $"stock Ceres alarm frame {frame} selection");
                for (int color = 0; color < SnesCgram.ColorCount; color++)
                    AssertEqual(nativeCgram[color], stockCgram[color],
                        $"stock Ceres alarm frame {frame} CGRAM {color}");
            }
            AssertEqual(0, stockState.SelfDestructAlarmStep,
                "Ceres alarm frame sixteen wraps to zero");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                "installed Ceres alarm never reads its source color rows");
        }

        void VerifyNorfairHealthPalette()
        {
            // Drive the real Norfair palette callback at both sides of every
            // health threshold. The extracted stock catalog supplies full-CGRAM
            // parity; the guarded installed path proves no copied ROM colors are read.
            var nativeEnemy = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            var stockEnemy = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            var nativeTick = new Action<RoomEnemySlot, RidleyEnemyState>(RidleyInteractions.UpdateNorfairRidleyHealthBand);
            var stockTick = new Action<RoomEnemySlot, RidleyEnemyState>(RidleyInteractions.UpdateNorfairRidleyHealthBand);
            RoomEnemySlot nativeSlot = nativeEnemy.SlotTable.Slots[0];
            RoomEnemySlot stockSlot = stockEnemy.SlotTable.Slots[0];
            var nativeState = new RidleyEnemyState();
            var stockState = new RidleyEnemyState();
            foreach (ushort health in new ushort[] { 9000, 8999, 5400, 5399, 1800, 1799, 0 })
            {
                nativeSlot.Health = stockSlot.Health = health;
                nativeTick(nativeSlot, nativeState);
                stockTick(stockSlot, stockState);
                RgbColor[] nativeCgram = RidleyLaidColours.Ceres(nativeState, original);
                RgbColor[] stockCgram = RidleyLaidColours.Ceres(stockState, original);
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
                for (int color = 0; color < SnesCgram.ColorCount; color++)
                    AssertEqual(nativeCgram[color], stockCgram[color],
                        $"stock Norfair Ridley health {health} CGRAM {color}");
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
        void CheckCgram(RidleyEnemyState shown, int count, int destination, Func<int, RgbColor> resolve, string context)
        {
            // The colours the encounter shows are the import's shades of the installed colours, laid as its AI laid them (#1512).
            RgbColor[] cgram = RidleyLaidColours.Ceres(shown, original);
            for (int color = 0; color < count; color++)
                AssertEqual(resolve(color), cgram[destination + color],
                    $"{context} color {color}");
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
