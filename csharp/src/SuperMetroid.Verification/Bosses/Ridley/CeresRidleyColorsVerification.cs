using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Frontend.Map.MapPresentationVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
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
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Bosses.Ridley;

/// <summary>Checks Ceres and Norfair Ridley palette installation against native CGRAM rows.</summary>
internal static class CeresRidleyColorsVerification
{
    /// <summary>Verifies extracted colors, installed palette effects, and guarded ROM access.</summary>
    internal static void VerifyCeresRidleyColors(string stockDirectory,
        InstalledScreenContent original, ISnesAddressSpace rom,
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
        for (int row = 0; row < RidleyShadeDefinitions.BodyFadeShadeCount; row++)
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
        var runtime = SuperMetroid.Core.Runtime.SuperMetroidRuntime.Create(GameplayAssists.None, InstalledFixtures.GameArtwork.Gameplay);
        runtime.Artwork.ScreenContent = original;
        var guarded = NewCeresRidleyColorReadGuard(rom, forbidden);
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(fixtureAssets.EnemyTiles.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, installedBosses: fixtureAssets.EnemyTiles.Bosses);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        // Binding gives the place a fresh occupant, so the body is bound before it begins the encounter it holds (#2231).
        place.Bind(EnemyDefinitionId.RidleyCeres, place.KeptDefinition);
        enemies.CeresRidleyBehavior.Flight.InitializeCeresRidley(place);
        CheckCgram(enemies.CeresRidley() ?? throw new InvalidOperationException("Ceres Ridley initialization did not publish state."),
            document.Start.Length, CeresRidleyShadeDefinitionsConstants.StartCgramIndex,
            InstalledFixtures.ImportOf(original).CeresRidleyColors.ResolveStart, "Ceres Ridley initialization");

        Action<RidleyEncounterState> eyeTick = RidleyReveal.TickEyeFadeIn;
        Action<RoomEnemyPlace, RidleyEncounterState> bodyTick = enemies.RidleyBody.Reveal.TickBodyFadeIn;
        Action<CeresRidleyState> healthTick = CeresRidleyHits.UpdateCeresRidleyHealthBand;
        var state = new CeresRidleyState(Position16<RoomPixels>.Zero) { RevealPhase = RidleyRevealPhase.FadeInEyes };
        // The eye fade shows its sixteen shades from the darkest, the cartridge's last row, then holds his own eye colours,
        // its first row (#2247, #2409).
        for (int step = 0; step < RidleyEyeFadeDefinitions.ShadeCount + RidleyEyeFadeDefinitions.HoldUpdates; step++)
        {
            eyeTick(state);
            int row = Math.Max(0, RidleyEyeFadeDefinitions.ShadeCount - 1 - step);
            AssertEqual(RidleyRevealPhase.FadeInEyes, state.RevealPhase, $"installed eye fade update {step} holds the eye fade");
            CheckCgram(state, CeresRidleyColorFormat.EyeFadeColorCount,
                CeresRidleyShadeDefinitionsConstants.EyeFadeCgramIndex,
                color => InstalledFixtures.ImportOf(original).CeresRidleyColors.ResolveEyeFade(row, color),
                $"Ceres Ridley eye fade step {step}");
        }
        eyeTick(state);
        AssertEqual(RidleyRevealPhase.FadeInBody, state.RevealPhase, "the eye fade's rest starts the body fade");
        for (int row = 0; row < RidleyShadeDefinitions.BodyFadeShadeCount; row++)
        {
            // The first of each shade's two updates only waits; the second shows it (#2233, #2409).
            bodyTick(place, state);
            AssertEqual(row == 0 ? null : RidleyFadeDefinitions.BodyFade.Images[row - 1], state.BodyFade.Shown,
                $"installed body fade row {row} waits an update first");
            bodyTick(place, state);
            AssertEqual(RidleyFadeDefinitions.BodyFade.Images[row], state.BodyFade.Shown, $"installed body fade shows row {row}");
            // The last row ends the fade as it shows (#2228).
            AssertEqual(row == RidleyShadeDefinitions.BodyFadeShadeCount - 1 ? RidleyRevealPhase.WaitBeforeRoar : RidleyRevealPhase.FadeInBody,
                state.RevealPhase, $"installed body fade row {row} ends the fade only at the last row");
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
            // The installed health colours' first row (wounded) before his seventieth hit, their last (nearly dead) from
            // it on: the game omits the branch after its ninety-hit comparison, so the middle row never shows in Ceres.
            int row = count < 70 ? 0 : 2;
            CheckCgram(state, CeresRidleyColorFormat.HealthColorCount,
                CeresRidleyShadeDefinitionsConstants.HealthCgramIndex,
                color => InstalledFixtures.ImportOf(original).CeresRidleyColors.ResolveHealth(row, color),
                $"Ceres Ridley hit count {count}");
        }
        Suite(nameof(VerifyNorfairHealthPalette), () => VerifyNorfairHealthPalette());
        Suite(nameof(VerifyCeresAlarmPalette), () => VerifyCeresAlarmPalette());
        CeresRidleyState activeRidley = enemies.CeresRidley() ??
            throw new InvalidOperationException("Ceres Ridley initialization did not publish state.");
        activeRidley.Phase = CeresRidleyPhase.RetreatDelay;
        activeRidley.PhaseTimer.LastFor(1);
        activeRidley.MovementAnimationEnabled = false;
        enemies.CeresRidleyBehavior.RunCeresRidleyMain(place, activeRidley, SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.StandaloneSamus(), InstalledFixtures.CreateProjectileSystem());
        AssertEqual(CeresRidleyPhase.PublishEscapeHandoff, activeRidley.Phase,
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
            // The station's escape steps the alarm (#2228).
            CeresEscape nativeEscape = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom()).CeresEscape;
            CeresEscape stockEscape = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom()).CeresEscape;
            var nativeState = new CeresRidleyState(Position16<RoomPixels>.Zero);
            var stockState = new CeresRidleyState(Position16<RoomPixels>.Zero);
            for (ushort frame = 0; frame < CeresRidleyColorFormat.AlarmStepCount * 4; frame++)
            {
                nativeEscape.StepAlarm(frame);
                stockEscape.StepAlarm(frame);
                RgbColor[] nativeCgram = RidleyLaidColours.Ceres(nativeState, nativeEscape, original);
                RgbColor[] stockCgram = RidleyLaidColours.Ceres(stockState, stockEscape, original);
                AssertEqual(nativeEscape.Warning.Alarm.Shown, stockEscape.Warning.Alarm.Shown,
                    $"stock Ceres alarm frame {frame} selection");
                for (int color = 0; color < SnesCgram.ColorCount; color++)
                    AssertEqual(nativeCgram[color], stockCgram[color],
                        $"stock Ceres alarm frame {frame} CGRAM {color}");
            }
            AssertEqual(RidleyFadeDefinitions.SelfDestructAlarm.Images[0], stockEscape.Warning.Alarm.Shown,
                "Ceres alarm frame sixteen wraps to zero");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                "installed Ceres alarm never reads its source color rows");
        }

        void VerifyNorfairHealthPalette()
        {
            // Drive the real Norfair palette callback at both sides of every
            // health threshold. The extracted stock catalog supplies full-CGRAM
            // parity; the guarded installed path proves no copied ROM colors are read.
            var nativeEnemy = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            var stockEnemy = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            var nativeTick = new Action<RoomEnemyPlace, NorfairRidleyState>(RidleyInteractions.UpdateNorfairRidleyHealthBand);
            var stockTick = new Action<RoomEnemyPlace, NorfairRidleyState>(RidleyInteractions.UpdateNorfairRidleyHealthBand);
            RoomEnemyPlace nativePlace = nativeEnemy.Places.InUpdateOrder[0];
            RoomEnemyPlace stockPlace = stockEnemy.Places.InUpdateOrder[0];
            var nativeState = new NorfairRidleyState();
            var stockState = new NorfairRidleyState();
            foreach (ushort health in new ushort[] { 9000, 8999, 5400, 5399, 1800, 1799, 0 })
            {
                nativePlace.Health = stockPlace.Health = health;
                nativeTick(nativePlace, nativeState);
                stockTick(stockPlace, stockState);
                RgbColor[] nativeCgram = RidleyLaidColours.Ceres(nativeState, escape: null, original);
                RgbColor[] stockCgram = RidleyLaidColours.Ceres(stockState, escape: null, original);
                ushort stage = health switch
                {
                    < 1800 => 3,
                    < 5400 => 2,
                    < 9000 => 1,
                    _ => 0,
                };
                AssertEqual(stage, RidleyNativeWords.HealthBands.Word(nativeState.HealthBand),
                    $"native Norfair Ridley health {health} band");
                AssertEqual(stage, RidleyNativeWords.HealthBands.Word(stockState.HealthBand),
                    $"stock Norfair Ridley health {health} band");
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
        void CheckCgram(CeresRidleyState shown, int count, int destination, Func<int, RgbColor> resolve, string context)
        {
            // The colours the encounter shows are the import's shades of the installed colours, laid as its AI laid them (#1512).
            RgbColor[] cgram = RidleyLaidColours.Ceres(shown, escape: null, original);
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
