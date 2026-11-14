using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Bosses.Phantoon.PhantoonNativeListAddresses;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
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
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().NextRandom = () => 0;
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        RoomEnemySlot eye = enemies.SlotTable.Slots[1];
        RoomEnemySlot tentacles = enemies.SlotTable.Slots[2];
        RoomEnemySlot mouth = enemies.SlotTable.Slots[3];
        body.Bind(EnemyDefinitionId.PhantoonBody, body.Definition);
        eye.Bind(EnemyDefinitionId.PhantoonEye, eye.Definition);
        tentacles.Bind(EnemyDefinitionId.PhantoonTentacles, tentacles.Definition);
        mouth.Bind(EnemyDefinitionId.PhantoonMouth, mouth.Definition);
        enemies.PhantoonBehavior.Combat.Phantoon = new PhantoonEnemyState(body)
        {
            Eye = eye,
            Tentacles = tentacles,
            Mouth = mouth,
        };
        return enemies;
    }

    /// <summary>The program a Phantoon part is playing.</summary>
    internal static PhantoonPartSegment PartProgram(RoomEnemySystem enemies, RoomEnemySlot part) =>
        RequireState(enemies).PlayingProgram(part);

    /// <summary>The system's Phantoon state.</summary>
    private static PhantoonEnemyState RequireState(RoomEnemySystem enemies) =>
        enemies.PhantoonBehavior.Combat.Phantoon ?? throw new InvalidOperationException("Verification requires Phantoon's state.");

    /// <summary>
    /// Every part segment's beats equal its native list and every part animation the lists' control flow, and the
    /// eye-opening and mouth animations raise their events through the production runner as the native lists do.
    /// </summary>
    internal static void VerifyPhantoonPartAnimations(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        AssertTrue(PartEntriesAscend, "native Phantoon part entries ascend");

        int stepCount = NativeEnemyAnimationOracle.CompareAll(cartridge, PartBankNumber, PartEntries, PhantoonAnimations.Segments,
            PhantoonAnimations.StartingAt, PhantoonPartFramesConstants.Spritemap,
            new NativeEnemyCueRules<PhantoonPartCue>(word => word == CallFunctionInY ? 1 : null, (word, operands, cue, at) =>
            {
                AssertTrue(PartCallbacks.TryGetValue(operands[0], out PhantoonPartCue native), $"{at}: native routine ${operands[0]:X4} is known");
                AssertEqual(native, cue, $"{at}: event");
            }));
        for (int ordinal = 0; ordinal < 8; ordinal++)
            AssertEqual(PhantoonPartSegment.EyeLookingUp + ordinal, PhantoonAnimationsConstants.Gaze(ordinal), $"gaze {ordinal} selects its program");

        VerifyEyeOpening();
        VerifyMouthFlame();
        Console.WriteLine($"  Phantoon part animations: {PartEntries.Length} segments and {stepCount} beats match bank $A7; eye-opening and mouth callbacks run as the native lists do.");
    }

    /// <summary>Runs one part update, optionally expiring the current frame first.</summary>
    private static void Pass(RoomEnemySlot part, bool forceExpiry = false)
    {
        if (forceExpiry)
            part.AnimationClock.StepOnNextUpdate();
        EnemyAnimationPlayer.AdvanceAnimation(part, samus: null, level: null, Position16<RoomPixels>.Zero, controllerInput: 0);
    }

    /// <summary>
    /// The opened eye's callbacks queue the materialization sound and install the eye-only body hitbox, while the
    /// list's sleep keeps the open eye on its own program, as the native sleep overwrites the callback's install.
    /// </summary>
    private static void VerifyEyeOpening()
    {
        RoomEnemySystem enemies = CreatePhantoonSystem();
        PhantoonEnemyState state = RequireState(enemies);
        RoomEnemySlot eye = state.Eye;
        state.PartPlayback(eye) = PhantoonEnemyState.Starting(PhantoonPartSegment.EyeOpen);
        eye.AnimationClock.StepOnNextUpdate();
        for (int frame = 0; frame < 3; frame++)
        {
            Pass(eye, forceExpiry: true);
            AssertEqual(PhantoonPartFramesConstants.Spritemap(new PhantoonPartFrame(PhantoonPartFrameKind.OpeningEye, frame)), eye.SpritemapPointer, $"opening eye frame {frame}");
        }
        AssertTrue(enemies.Audio.LatestSound is null, "the materialization sound waits for the last frame to expire");
        Pass(eye, forceExpiry: true);
        AssertTrue(enemies.Audio.LatestSound.HasValue, "the opened eye queues its materialization sound");
        AssertEqual(PhantoonPartSegment.EyeHitboxBody, PartProgram(enemies, state.Body), "the opened eye installs the eye-only body hitbox");
        AssertEqual(PhantoonPartSegment.EyeOpen, PartProgram(enemies, eye), "the list's sleep keeps the eye on its opened program");
        AssertEqual(PhantoonPartFramesConstants.Spritemap(new PhantoonPartFrame(PhantoonPartFrameKind.OpeningEye, 2)), eye.SpritemapPointer, "the opened eye keeps its last frame");
        AssertEqual((ushort)1, eye.AnimationClock.Hold.RawValue, "the callback's one-update timer survives the sleep");
    }

    /// <summary>The mouth's flame program spawns one casual flame and runs on into its initial frame.</summary>
    private static void VerifyMouthFlame()
    {
        RoomEnemySystem enemies = CreatePhantoonSystem();
        PhantoonEnemyState state = RequireState(enemies);
        RoomEnemySlot mouth = state.Mouth;
        state.PartPlayback(mouth) = PhantoonEnemyState.Starting(PhantoonPartSegment.MouthSpawnFlame);
        Pass(mouth, forceExpiry: true);
        Pass(mouth, forceExpiry: true);
        AssertEqual(0, ActiveFlames(enemies), "the mouth opens before it spawns");
        Pass(mouth, forceExpiry: true);
        AssertEqual(1, ActiveFlames(enemies), "the mouth spawns one casual flame");
        AssertEqual(PhantoonPartSegment.MouthInitial, PartProgram(enemies, mouth), "the mouth runs on into its initial program");
        AssertEqual(PhantoonPartFramesConstants.Spritemap(new PhantoonPartFrame(PhantoonPartFrameKind.Mouth, 0)), mouth.SpritemapPointer, "the mouth shows its closed frame at once");
    }

    /// <summary>Active Phantoon destroyable flames.</summary>
    private static int ActiveFlames(RoomEnemySystem enemies) =>
        enemies.Projectiles.EnemyProjectiles.Count(item => item.IsActive && item.Kind == RoomEnemyProjectileKind.PhantoonDestroyableFlame);

    /// <summary>
    /// The real flame producers start the typed programs their native lists start, and every flame program matches
    /// its bank-$86 list update by update: frames, timers, the falling loop counter, the drop request and deletion.
    /// </summary>
    internal static void VerifyPhantoonFlameAnimations(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        VerifyFlameProducers();
        foreach ((PhantoonFlameSegment program, ushort entry) in FlameEntries)
        {
            RoomEnemySystem enemies = NewFlameSystem();
            EnemyProjectile flame = SpawnDestroyable(enemies, SpawnRageFlame, "rage");
            flame.StartAnimation(new PhantoonFlameAnimation(program));
            var native = new NativeEnemyProjectileListReader(entry, flame.FrameHold.RawValue);
            int drops = 0;
            for (int update = 0; update < 64 && !native.Deleted; update++)
            {
                enemies.Projectiles.AdvanceEnemyProjectileAnimations(flame, new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()), Position16<RoomPixels>.Zero, shortCounter: 0);
                native.Step(cartridge);
                drops += native.Callbacks.Count;
                string at = $"Phantoon flame {program} update {update}";
                AssertEqual(!native.Deleted, flame.IsActive, $"{at}: deletion");
                if (native.Deleted)
                    break;
                AssertEqual(native.Timer, flame.FrameHold.RawValue, $"{at}: timer");
                AssertEqual(native.Operand, flame.PresentationOperandAddress, $"{at}: frame");
                AssertEqual(native.Counter, flame.LoopCountdown.RemainingFrames, $"{at}: loop counter");
            }
            AssertEqual(drops, enemies.ActiveEnemyDropPickups, $"Phantoon flame {program}: drop requests");
            bool deletes = program is PhantoonFlameSegment.CasualResting or PhantoonFlameSegment.Dying or PhantoonFlameSegment.RainImpact
                or PhantoonFlameSegment.Delete or PhantoonFlameSegment.DestroyableShot;
            AssertEqual(deletes, native.Deleted, $"Phantoon flame {program}: deletion matches the native list");
        }

        AssertEqual(PhantoonFlameVisualDefinitions.ProjectileFrameCount, PhantoonFlameVisualDefinitions.ProjectileFrames.Count, "the flame catalog lists every frame");
        VerifyPrePhantoonRoomAnimation(cartridge);
        Console.WriteLine($"  Phantoon flame programs: {FlameEntries.Length} programs match bank $86 every update, with the falling counter, drop request and deletion.");
    }

    /// <summary>A projectile system whose drops have a Samus to read.</summary>
    private static RoomEnemySystem NewFlameSystem()
    {
        RoomEnemySystem enemies = CreatePhantoonSystem();
        enemies.VerificationServices().NextRandom = () => 128;
        enemies.DeathsAndDrops.BindSamus(new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Health = 99, MaxHealth = 99 });
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        body.Position = body.Position.WithWhole(new(128, 96));
        return enemies;
    }

    /// <summary>Spawns one destroyable flame through the real producer.</summary>
    private static EnemyProjectile SpawnDestroyable(RoomEnemySystem enemies, Func<PhantoonPatterns, RoomEnemySlot, bool> spawn, string producer)
    {
        AssertTrue(spawn(enemies.PhantoonBehavior.Patterns, enemies.SlotTable.Slots[0]), $"the real {producer} producer allocates a flame");
        return enemies.Projectiles.EnemyProjectiles.Single(item => item.IsActive && item.Kind == RoomEnemyProjectileKind.PhantoonDestroyableFlame);
    }

    /// <summary>The starting, casual and rage producers start the programs their native initializers install.</summary>
    private static void VerifyFlameProducers()
    {
        RoomEnemySystem starting = NewFlameSystem();
        AssertTrue(starting.PhantoonBehavior.Opening.SpawnPhantoonStartingFlame(starting.SlotTable.Slots[0], PhantoonFlameSpawns.StartingFlameStart(0)), "the real starting producer allocates a flame");
        AssertStarted(starting.Projectiles.EnemyProjectiles.Single(item => item.Kind == RoomEnemyProjectileKind.PhantoonStartingFlame),
            PhantoonFlameSegment.StartingFlame, "starting flame");
        RoomEnemySystem casual = NewFlameSystem();
        AssertStarted(SpawnDestroyable(casual, (patterns, body) => patterns.SpawnPhantoonCasualFlame(body), "casual"), PhantoonFlameSegment.CasualFalling, "casual flame");
        RoomEnemySystem rage = NewFlameSystem();
        AssertStarted(SpawnDestroyable(rage, SpawnRageFlame, "rage"), PhantoonFlameSegment.DestroyableIdle, "rage flame");
    }

    /// <summary>Spawns the last flame of an even rage wave, the one the cartridge's first rage parameter starts.</summary>
    private static bool SpawnRageFlame(PhantoonPatterns patterns, RoomEnemySlot body) =>
        patterns.SpawnPhantoonRageFlame(body, PhantoonFlameSpawns.EvenRageWave[^1]);

    /// <summary>Requires <paramref name="flame"/> to run <paramref name="program"/> from its start.</summary>
    private static void AssertStarted(EnemyProjectile flame, PhantoonFlameSegment program, string label) =>
        AssertTrue(flame.Animation is PhantoonFlameAnimation started
                && started.Playback == new AnimationPlayback<PhantoonFlameSegment>(PhantoonFlameAnimations.StartingWith(program), AnimationPlace.StartOfAnimation),
            $"the {label} starts the {program} program");

    /// <summary>The pre-Phantoon room projectile's blank hold and deletion match $86:A3AA update by update.</summary>
    private static void VerifyPrePhantoonRoomAnimation(IImportCartridgeSource cartridge)
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        enemies.PrePhantoonRoomBehavior.SpawnPrePhantoonRoomProjectile();
        EnemyProjectile projectile = enemies.Projectiles.EnemyProjectiles.Single(item => item.Kind == RoomEnemyProjectileKind.PrePhantoonRoom);
        AssertTrue(projectile.Animation is PrePhantoonRoomAnimation, "the room setup starts the typed room animation");
        var native = new NativeEnemyProjectileListReader(PrePhantoonRoomEntry, projectile.FrameHold.RawValue);
        for (int update = 0; update < 40 && !native.Deleted; update++)
        {
            enemies.Projectiles.AdvanceEnemyProjectileAnimations(projectile, null, Position16<RoomPixels>.Zero, shortCounter: 0);
            native.Step(cartridge);
            AssertEqual(!native.Deleted, projectile.IsActive, $"pre-Phantoon room update {update}: deletion");
            if (native.Deleted)
                break;
            AssertEqual(native.Timer, projectile.FrameHold.RawValue, $"pre-Phantoon room update {update}: timer");
            // The native operand selects the empty projectile spritemap, which has no installed program frame.
            AssertEqual(cartridge.ReadCartridgeWord(NativeEnemyProjectileListDefinitions.ProjectileBank | native.Operand), projectile.SpritemapPointer,
                $"pre-Phantoon room update {update}: spritemap");
            AssertTrue(projectile.Frame == EnemyProjectileFrameId.Blank, $"pre-Phantoon room update {update}: the blank frame, no program frame");
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
