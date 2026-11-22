using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Bosses.Phantoon.PhantoonNativeListAddresses;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
namespace SuperMetroid.Verification.Bosses.Phantoon;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.MovieReplay.Verification;

/// <summary>
/// #1286: Phantoon's typed body, eye, tentacle, mouth and flame programs against the retail cartridge. The part
/// programs must match the native bank-$A7 lists step by step and run their callbacks as the native interpreter
/// does; the flames and the pre-Phantoon room projectile must match their bank-$86 lists update by update.
/// </summary>
internal static class PhantoonAnimationsVerification
{
    /// <summary>Creates the four linked Phantoon parts and a deterministic random source.</summary>
    internal static RoomEnemySystem CreatePhantoonSystem()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().NextRandom = () => 0;
        RoomEnemyPlace body = enemies.Places.InUpdateOrder[0];
        RoomEnemyPlace eye = enemies.Places.InUpdateOrder[1];
        RoomEnemyPlace tentacles = enemies.Places.InUpdateOrder[2];
        RoomEnemyPlace mouth = enemies.Places.InUpdateOrder[3];
        body.Bind(EnemyDefinitionId.PhantoonBody, body.KeptDefinition);
        eye.Bind(EnemyDefinitionId.PhantoonEye, eye.KeptDefinition);
        tentacles.Bind(EnemyDefinitionId.PhantoonTentacles, tentacles.KeptDefinition);
        mouth.Bind(EnemyDefinitionId.PhantoonMouth, mouth.KeptDefinition);
        enemies.BeginPhantoonAsSetUp(new PhantoonEnemyState(body.HeldOccupant(), body, eye, tentacles, mouth));
        return enemies;
    }

    /// <summary>The program a Phantoon part is playing: its own part of the encounter's, which its setup installed.</summary>
    internal static PhantoonPartSegment PartProgram(RoomEnemySystem enemies, RoomEnemyPlace part)
    {
        _ = RequireState(enemies);
        return part.RequireSpeciesState<PhantoonPart>().PlayingProgram();
    }

    /// <summary>The system's Phantoon state.</summary>
    private static PhantoonEnemyState RequireState(RoomEnemySystem enemies) =>
        enemies.BossEncounters.Phantoon ?? throw new InvalidOperationException("Verification requires Phantoon's state.");

    /// <summary>
    /// Every part segment's beats equal its native list and every part animation the lists' control flow, and the
    /// eye-opening and mouth animations raise their events through the production runner as the native lists do.
    /// </summary>
    internal static void VerifyPhantoonPartAnimations(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = rom;
        AssertTrue(PartEntriesAscend, "native Phantoon part entries ascend");

        int stepCount = NativeEnemyAnimationOracle.CompareAll(cartridge, PartBankNumber, PartEntries, PhantoonAnimations.Segments,
            PhantoonAnimations.StartingAt, PhantoonPartFramesConstants.Spritemap,
            new NativeEnemyCueRules<PhantoonPartCue>(word => word == CallFunctionInY ? 1 : null, (word, operands, cue, at) =>
            {
                AssertTrue(PartCallbacks.TryGetValue(operands[0], out PhantoonPartCue native), $"{at}: native routine ${operands[0]:X4} is known");
                AssertEqual(native, cue, $"{at}: event");
            }, PicturedAt: PicturedAt));
        for (int ordinal = 0; ordinal < 8; ordinal++)
            AssertEqual(PhantoonPartSegment.EyeLookingUp + ordinal, PhantoonAnimationsConstants.Gaze(ordinal), $"gaze {ordinal} selects its program");

        VerifyEyeOpening();
        VerifyMouthFlame();
        Console.WriteLine($"  Phantoon part animations: {PartEntries.Length} segments and {stepCount} beats match bank $A7; eye-opening and mouth callbacks run as the native lists do.");
    }

    /// <summary>
    /// The native frame word an installed part frame is read from, for a word whose record repeats another's picture
    /// (#2249): his eye lists show the closed eye and the open eye looking ahead from two records each.
    /// </summary>
    private static ushort PicturedAt(ushort word) =>
        PhantoonPartFramesConstants.Spritemap(RoomEnemyPlaceFrameAccess.FrameAt(PartBankNumber, word)
            ?? throw new InvalidDataException($"Phantoon's part lists show no frame at ${PartBankNumber:X2}:{word:X4}."));

    /// <summary>
    /// Runs one part update as the shared enemy animation runs a Phantoon part's, over the part the check's encounter
    /// holds, optionally expiring the current frame first.
    /// </summary>
    private static void Pass(RoomEnemySystem enemies, RoomEnemyPlace part, bool forceExpiry = false)
    {
        if (forceExpiry)
            part.AnimationClock.StepOnNextUpdate();
        enemies.PhantoonBehavior.AdvancePhantoonPartAnimation(part.RequireSpeciesState<PhantoonPart>());
    }

    /// <summary>
    /// The opened eye's callbacks queue the materialization sound and install the eye-only body hitbox, while the
    /// list's sleep keeps the open eye on its own program, as the native sleep overwrites the callback's install.
    /// </summary>
    private static void VerifyEyeOpening()
    {
        RoomEnemySystem enemies = CreatePhantoonSystem();
        PhantoonEnemyState state = RequireState(enemies);
        RoomEnemyPlace eye = state.Eye;
        eye.RequireSpeciesState<PhantoonPart>().Playback = PhantoonPart.Starting(PhantoonPartSegment.EyeOpen);
        eye.AnimationClock.StepOnNextUpdate();
        for (int frame = 0; frame < 3; frame++)
        {
            Pass(enemies, eye, forceExpiry: true);
            AssertEqual(PhantoonPartFramesConstants.Spritemap(OpeningEyeFrames[frame]), eye.SpritemapPointer, $"opening eye frame {frame}");
        }
        AssertTrue(enemies.Audio.LatestSound is null, "the materialization sound waits for the last frame to expire");
        Pass(enemies, eye, forceExpiry: true);
        AssertTrue(enemies.Audio.LatestSound is not null, "the opened eye queues its materialization sound");
        AssertEqual(PhantoonPartSegment.Body, PartProgram(enemies, state.Body), "the opened eye restarts the body's picture");
        state.BodyPictureShown();
        AssertEqual(PhantoonBodyHitArea.EyeOnly, state.BodyHitArea, "the opened eye installs the eye-only body hitbox as his body's picture shows");
        AssertEqual(PhantoonPartSegment.EyeOpen, PartProgram(enemies, eye), "the list's sleep keeps the eye on its opened program");
        AssertEqual(PhantoonPartFramesConstants.Spritemap(EnemyFrameId.PhantoonEyeballCentered), eye.SpritemapPointer, "the opened eye keeps its last frame");
        AssertEqual(1, eye.AnimationClock.Hold, "the callback's one-update timer survives the sleep");
    }

    /// <summary>The eye's three opening frames, in the order the eyelid opens.</summary>
    private static readonly EnemyFrameId[] OpeningEyeFrames =
        [EnemyFrameId.PhantoonEyeClosed, EnemyFrameId.PhantoonEyeHalfOpen, EnemyFrameId.PhantoonEyeballCentered];

    /// <summary>The mouth's flame program spawns one casual flame and runs on into its initial frame.</summary>
    private static void VerifyMouthFlame()
    {
        RoomEnemySystem enemies = CreatePhantoonSystem();
        PhantoonEnemyState state = RequireState(enemies);
        RoomEnemyPlace mouth = state.Mouth;
        mouth.RequireSpeciesState<PhantoonPart>().Playback = PhantoonPart.Starting(PhantoonPartSegment.MouthSpawnFlame);
        Pass(enemies, mouth, forceExpiry: true);
        Pass(enemies, mouth, forceExpiry: true);
        AssertEqual(0, ActiveFlames(enemies), "the mouth opens before it spawns");
        Pass(enemies, mouth, forceExpiry: true);
        AssertEqual(1, ActiveFlames(enemies), "the mouth spawns one casual flame");
        AssertEqual(PhantoonPartSegment.MouthInitial, PartProgram(enemies, mouth), "the mouth runs on into its initial program");
        AssertEqual(PhantoonPartFramesConstants.Spritemap(EnemyFrameId.PhantoonMouthOpen), mouth.SpritemapPointer, "the mouth shows its closed frame at once");
    }

    /// <summary>Active Phantoon destroyable flames.</summary>
    private static int ActiveFlames(RoomEnemySystem enemies) =>
        enemies.Projectiles.EnemyProjectiles.Count(item => item.IsActive && item.Kind == RoomEnemyProjectileKind.PhantoonDestroyableFlame);

    /// <summary>
    /// The real flame producers start the typed programs their native lists start, and every flame program matches
    /// its bank-$86 list update by update: frames, timers, the falling plays left (#2006), the drop request and deletion.
    /// </summary>
    internal static void VerifyPhantoonFlameAnimations(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = rom;
        VerifyFlameProducers();
        // A resting flame and a rain impact run on into the dying flame's poses (#2166), and their plays match its native
        // list; the shot flame's animation dies, drops and deletes from its own (#2405).
        foreach ((PhantoonFlameSegment program, ushort entry) in FlameEntries)
        {
            RoomEnemySystem enemies = NewFlameSystem();
            EnemyProjectile flame = SpawnDestroyable(enemies, SpawnRageFlame, "rage");
            flame.StartAnimation(new PhantoonFlameAnimation(enemies.PhantoonBehavior.Combat.DestroyableFlame, program));
            var native = new NativeEnemyProjectileListReader(entry, flame.FrameHold.RawValue);
            int drops = 0;
            for (int update = 0; update < 64 && !native.Deleted; update++)
            {
                enemies.Projectiles.AdvanceEnemyProjectileAnimations(flame, new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()), Position16<RoomPixels>.Zero, shortCounter: SuperMetroid.Core.Compatibility.ShortFrameCount.Zero);
                native.Step(cartridge);
                drops += native.Callbacks.Count;
                string at = $"Phantoon flame {program} update {update}";
                AssertEqual(!native.Deleted, flame.IsActive, $"{at}: deletion");
                if (native.Deleted)
                    break;
                SuperMetroid.Verification.Enemies.NativeEnemyProjectileParity.AssertFrameHold(native.Timer, flame, at);
                // A list that shows a pose again from an operand of its own shows the pose's one installed frame (#2249).
                AssertEqual(EnemyProjectileProgramFrameOperands.ShownAt(native.Operand, flame.Graphics.Owner), flame.Frame, $"{at}: frame");
                if (NativeEnemyAnimationOracle.NativePlaysLeft(AssertIs<PhantoonFlameAnimation>(flame.Animation, $"{at}: flame animation").Playback) is { } playsLeft)
                    AssertEqual<int>(native.Counter, playsLeft, $"{at}: plays left");
            }
            AssertEqual(drops, enemies.ActiveEnemyDropPickups, $"Phantoon flame {program}: drop requests");
            bool deletes = program is PhantoonFlameSegment.CasualResting or PhantoonFlameSegment.RainImpact
                or PhantoonFlameSegment.Delete or PhantoonFlameSegment.Dying;
            AssertEqual(deletes, native.Deleted, $"Phantoon flame {program}: deletion matches the native list");
        }

        VerifyPrePhantoonRoomAnimation(cartridge);
        Console.WriteLine($"  Phantoon flame programs: {FlameEntries.Length} programs match bank $86 every update, with the falling counter, drop request and deletion.");
    }

    /// <summary>A projectile system whose flames' drops roll for the Samus each projectile pass hands them.</summary>
    private static RoomEnemySystem NewFlameSystem()
    {
        RoomEnemySystem enemies = CreatePhantoonSystem();
        enemies.VerificationServices().NextRandom = () => 128;
        RoomEnemyPlace body = enemies.Places.InUpdateOrder[0];
        body.Position = body.Position.WithWhole(new(128, 96));
        return enemies;
    }

    /// <summary>Spawns one destroyable flame through the real producer.</summary>
    private static EnemyProjectile SpawnDestroyable(RoomEnemySystem enemies, Action<PhantoonPatterns, PhantoonEnemyState> spawn, string producer)
    {
        spawn(enemies.PhantoonBehavior.Patterns, RequireState(enemies));
        return enemies.Projectiles.EnemyProjectiles.SingleOrDefault(item => item.IsActive && item.Kind == RoomEnemyProjectileKind.PhantoonDestroyableFlame)
            ?? throw new InvalidOperationException($"The real {producer} producer allocates no flame.");
    }

    /// <summary>The starting, casual and rage producers start the programs their native initializers install.</summary>
    private static void VerifyFlameProducers()
    {
        RoomEnemySystem starting = NewFlameSystem();
        starting.PhantoonBehavior.Opening.SpawnPhantoonStartingFlame(RequireState(starting), PhantoonFlameSpawns.StartingFlameStarts[0]);
        AssertStarted(starting.Projectiles.EnemyProjectiles.Single(item => item.Kind == RoomEnemyProjectileKind.PhantoonStartingFlame),
            PhantoonFlameSegment.StartingFlame, "starting flame");
        RoomEnemySystem casual = NewFlameSystem();
        AssertStarted(SpawnDestroyable(casual, (patterns, phantoon) => patterns.SpawnPhantoonCasualFlame(phantoon.Body.HeldOccupant(), phantoon.Body), "casual"), PhantoonFlameSegment.CasualFallingKindling, "casual flame");
        RoomEnemySystem rage = NewFlameSystem();
        AssertStarted(SpawnDestroyable(rage, SpawnRageFlame, "rage"), PhantoonFlameSegment.Burning, "rage flame");
    }

    /// <summary>Spawns the last flame of an even rage wave, the one the cartridge's first rage parameter starts.</summary>
    private static void SpawnRageFlame(PhantoonPatterns patterns, PhantoonEnemyState phantoon) =>
        patterns.SpawnPhantoonRageFlame(phantoon, PhantoonFlameSpawns.EvenRageWave[^1]);

    /// <summary>Requires <paramref name="flame"/> to run <paramref name="program"/> from its start.</summary>
    private static void AssertStarted(EnemyProjectile flame, PhantoonFlameSegment program, string label) =>
        AssertTrue(flame.Animation is PhantoonFlameAnimation started
                && started.Playback == new AnimationPlayback<PhantoonFlameSegment>(PhantoonFlameAnimations.StartingWith(program), AnimationPlace.StartOfAnimation),
            $"the {label} starts the {program} program");

    /// <summary>
    /// The pre-Phantoon room's held place stays taken until the update $86:A3AA deletes the cartridge's blank projectile
    /// (#2561, #2556): no projectile stands in it, and the projectile pass counts its hold at its turn.
    /// </summary>
    private static void VerifyPrePhantoonRoomAnimation(IImportCartridgeSource cartridge)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        int places = SuperMetroid.Core.Compatibility.EnemyProjectilePlaces.Capacity;
        enemies.PrePhantoonRoomBehavior.HoldPrePhantoonRoomProjectilePlace();
        AssertEqual(places - 1, enemies.Projectiles.FreePlaceCount, "the room setup holds one place");
        AssertTrue(!enemies.Projectiles.EnemyProjectiles.Any(), "no projectile stands in the held place");
        // A spawned projectile reads its list's first instruction, the blank frame, on its first update.
        const ushort SpawnedListUpdatesToFirstInstruction = 1;
        var native = new NativeEnemyProjectileListReader(PrePhantoonRoomEntry, SpawnedListUpdatesToFirstInstruction);
        RoomLevelData level = RoomLevelData.CollisionOnly(new(1, 1), [RoomCollisionType.Air]);
        for (int update = 0; update < 40 && !native.Deleted; update++)
        {
            enemies.Projectiles.RunPass(level, RoomEnemyFrameFixtures.StandaloneSamus(), Position16<RoomPixels>.Zero,
                SuperMetroid.Core.Compatibility.ShortFrameCount.Zero, InstalledFixtures.CreateBombSystem(), frameCounter: 0);
            native.Step(cartridge);
            AssertEqual(native.Deleted ? places : places - 1, enemies.Projectiles.FreePlaceCount,
                $"pre-Phantoon room update {update}: the place is held until the cartridge's projectile deletes itself");
        }
        AssertTrue(native.Deleted, "the pre-Phantoon room projectile deletes itself");
    }

    /// <summary>Runs Phantoon's part program parity check against the repository cartridge.</summary>
    [VerificationCommand("--phantoon-part-animations")]
    internal static int PhantoonPartAnimationsCommand()
    {
        Suite(nameof(VerifyPhantoonPartAnimations), () => VerifyPhantoonPartAnimations(LoadRepositoryRom()));
        return 0;
    }

    /// <summary>Runs Phantoon's flame program parity check against the repository cartridge.</summary>
    [VerificationCommand("--phantoon-flame-animations")]
    internal static int PhantoonFlameAnimationsCommand()
    {
        Suite(nameof(VerifyPhantoonFlameAnimations), () => VerifyPhantoonFlameAnimations(LoadRepositoryRom()));
        return 0;
    }
}
