using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Bosses.MotherBrain.BabyMetroidVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainBeamWindowVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainDeathHandoffVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainNativeListAddresses;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainRainbowVerification;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Samus.SamusDamageAndGrabVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.MotherBrain;

using SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>
/// #1285: Mother Brain's typed body, brain, Baby, Shitroid, Zebetite, falling-tube and projectile programs against
/// the retail cartridge. Every typed program must match its native list step by step; the brain's count-up timer,
/// random holds, body moves and the projectiles must behave as the native interpreters do.
/// </summary>
internal static class MotherBrainAnimationsVerification
{
    /// <summary>Every typed slot and brain program equals its native list, and the runners keep the native cadence.</summary>
    internal static void VerifyMotherBrainAnimations(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        int body = CompareBody(cartridge).Count;
        int head = CompareHead(cartridge).Count;
        CompareBaby(cartridge);
        CompareShitroid(cartridge);
        CompareZebetites(cartridge);
        CompareTubes(cartridge);
        VerifyRoomFlashList(cartridge);
        VerifyBrainCadence();
        VerifyBrainRandomHolds();
        VerifyBodyMoves();
        Console.WriteLine($"  Mother Brain programs: {body} body and {head} brain records, the Baby, Shitroid, Zebetite and tube lists " +
            "match bank $A9/$A6; the brain's count-up cadence, random holds and body moves match the native routines.");
    }

    /// <summary>The fake-death flash is fourteen frames of the reviewed duration, then a goto back to its start.</summary>
    private static void VerifyRoomFlashList(IImportCartridgeSource cartridge)
    {
        AssertEqual(MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount, RoomFlashRecords.Length, "the room flash has fourteen frames");
        foreach ((int frame, int record) in RoomFlashRecords)
        {
            AssertEqual(MotherBrainRoomPaletteProgramDefinitions.FlashImageTicks, cartridge.ReadCartridgeWord(record), $"room flash frame {frame} duration");
            AssertEqual(SuperMetroid.AssetExtraction.Bosses.MotherBrain.MotherBrainRoomColorExtractor.FlashPaletteOperandAddress(frame), unchecked((ushort)(record + 2)),
                $"room flash frame {frame} palette operand");
        }
        AssertEqual(RoomFlashGoto.Opcode, cartridge.ReadCartridgeWord(RoomFlashLoop), "the room flash ends in a goto");
        AssertEqual(RoomFlashGoto.Target, cartridge.ReadCartridgeWord(RoomFlashLoop + 2), "the room flash loops to its first frame");
    }

    /// <summary>Creates Mother Brain's body and head in a fresh enemy system.</summary>
    private static NewMotherBrainResult NewMotherBrain()
    {
        var enemies = new RoomEnemySystem(new EnemySession(), services: new VerificationRoomEnemyServices(null));
        RoomEnemySlot bodySlot = enemies.SlotTable.Slots[0];
        bodySlot.Bind(EnemyDefinitionId.MotherBrainBody, bodySlot.Definition);
        RoomEnemySlot headSlot = enemies.SlotTable.Slots[1];
        headSlot.Bind(EnemyDefinitionId.MotherBrainHead, headSlot.Definition);
        var state = new MotherBrainEnemyState(bodySlot) { Head = headSlot };
        enemies.MotherBrainEncounter.MotherBrain = state;
        return new NewMotherBrainResult(enemies, state);
    }

    /// <summary>
    /// The brain's timer counts up: a program's first frame shows for its duration, and each later frame the runner
    /// installs shows once more, because installing it counts as its first draw.
    /// </summary>
    private static void VerifyBrainCadence()
    {
        foreach (MotherBrainHeadProgram program in new[] { MotherBrainHeadProgram.Initial, MotherBrainHeadProgram.NeutralPhase2, MotherBrainHeadProgram.Laser })
        {
            (RoomEnemySystem enemies, MotherBrainEnemyState state) = NewMotherBrain();
            MotherBrainEncounterAccess.SetMotherBrainBrainProgram(state, program);
            var steps = MotherBrainAnimations.Head.Steps(program);
            var shown = steps.OfType<AnimationProgramStep<MotherBrainHeadProgram, MotherBrainHeadFrame, MotherBrainHeadCue>.ShowStep>().First();
            int duration = shown.Duration;
            for (int draw = 0; draw < duration; draw++)
                AssertEqual((MotherBrainHeadFrame?)shown.Frame, enemies.MotherBrainBehavior.Head.AdvanceMotherBrainBrain(state, samus: null),
                    $"{program} shows its first frame on draw {draw}");
            AssertEqual((ushort)(duration + 1), state.BrainFrameTimer, $"{program}'s timer counts up past the duration");
            enemies.MotherBrainBehavior.Head.AdvanceMotherBrainBrain(state, samus: null);
            AssertEqual(1, state.BrainFrameTimer, $"{program} installs its next frame once the timer exceeds the duration");
            AssertTrue(MotherBrainAnimations.Head.Steps((state.BrainCursor ?? throw new InvalidOperationException("Verification requires state.BrainCursor.")).Program)[state.BrainCursor.Value.Position] is AnimationProgramStep<MotherBrainHeadProgram, MotherBrainHeadFrame, MotherBrainHeadCue>.ShowStep, $"{program}'s cursor rests on the frame it shows");
        }
    }

    /// <summary>
    /// The phase-two neutral pose holds again unless the random word reaches $F000, and the dying drool bursts again
    /// only for low-twelve random values from $FE0.
    /// </summary>
    private static void VerifyBrainRandomHolds()
    {
        foreach ((ushort random, bool holds) in new[] { new VerifyBrainRandomHoldsCase(61439, true), new VerifyBrainRandomHoldsCase(61440, false) })
        {
            (RoomEnemySystem enemies, MotherBrainEnemyState state) = NewMotherBrain();
            enemies.VerificationServices().ReadRandom = () => random;
            MotherBrainEncounterAccess.SetMotherBrainBrainProgram(state, MotherBrainHeadProgram.NeutralPhase2Hold);
            for (int draw = 0; draw <= 8; draw++)
                enemies.MotherBrainBehavior.Head.AdvanceMotherBrainBrain(state, samus: null);
            AssertEqual(holds ? new AnimationProgramCursor<MotherBrainHeadProgram>(MotherBrainHeadProgram.NeutralPhase2Hold, 0)
                    : new AnimationProgramCursor<MotherBrainHeadProgram>(MotherBrainHeadProgram.NeutralPhase2Hold, 2),
                state.BrainCursor, $"random {random} {(holds ? "holds" : "leaves")} the phase-two neutral pose");
        }
        foreach ((ushort random, bool bursts) in new[] { new VerifyBrainRandomHoldsCase(4063, false), new VerifyBrainRandomHoldsCase(4064, true) })
        {
            (RoomEnemySystem enemies, MotherBrainEnemyState state) = NewMotherBrain();
            enemies.VerificationServices().ReadRandom = () => random;
            MotherBrainEncounterAccess.SetMotherBrainBrainProgram(state, MotherBrainHeadProgram.DyingDroolLoop);
            for (int draw = 0; draw <= 2; draw++)
                enemies.MotherBrainBehavior.Head.AdvanceMotherBrainBrain(state, samus: null);
            AssertEqual(bursts ? MotherBrainHeadProgram.DyingDroolBurst : MotherBrainHeadProgram.DyingDroolLoop, (state.BrainCursor ?? throw new InvalidOperationException("Verification requires state.BrainCursor.")).Program,
                $"random {random} {(bursts ? "bursts into more drool" : "keeps the drool loop")}");
        }
    }

    /// <summary>
    /// A forward walk carries the body 24 pixels right and a backward walk the same distance back, each ending
    /// standing; the slow crouch lowers it 38 pixels and the slow stand-up raises it again.
    /// </summary>
    private static void VerifyBodyMoves()
    {
        (RoomEnemySystem enemies, MotherBrainEnemyState state) = NewMotherBrain();
        var scrolls = new List<Position16<BackgroundPixels>>();
        state.Body.Position = FixedPosition<RoomPixels>.FromWhole(new(64, 112));
        Position16<RoomPixels> origin = state.Body.Position.Whole;
        void Run(MotherBrainBodyProgram program)
        {
            MotherBrainEncounterAccess.SetMotherBrainBodyProgram(state, program);
            for (int update = 0; update < 256; update++)
            {
                enemies.MotherBrainBehavior.Body.AdvanceMotherBrainSlotAnimation(state.Body, samus: null);
                if (state.PlacesBodyLayer)
                    scrolls.Add(state.BodyLayerPlacement);
            }
        }
        Run(MotherBrainBodyProgram.WalkForwardReallyFast);
        AssertEqual(origin + new Offset16<RoomPixels>(24, 0), state.Body.Position.Whole, "a forward walk moves the body 24 pixels right");
        AssertEqual(MotherBrainBodyPose.Standing, state.Pose, "a forward walk ends standing");
        Run(MotherBrainBodyProgram.WalkBackwardMedium);
        AssertEqual(origin, state.Body.Position.Whole, "a backward walk returns the body");
        Run(MotherBrainBodyProgram.CrouchSlow);
        AssertEqual(origin + new Offset16<RoomPixels>(0, 38), state.Body.Position.Whole, "the slow crouch lowers the body 38 pixels");
        AssertEqual(MotherBrainBodyPose.Crouched, state.Pose, "the slow crouch ends crouched");
        Run(MotherBrainBodyProgram.StandAfterCrouchSlow);
        AssertEqual(origin, state.Body.Position.Whole, "the slow stand-up raises the body again");
        AssertEqual(MotherBrainBodyPose.Standing, state.Pose, "the slow stand-up ends standing");
        AssertTrue(scrolls.Count > 0, "every body move publishes the BG2 scroll that keeps the body art aligned");
    }

    /// <summary>
    /// The glass shards and sparkle, the hand beam, and the turrets' poses and impact smoke match their bank-$86 lists
    /// update by update through the real projectile engine.
    /// </summary>
    internal static void VerifyMotherBrainProjectileAnimations(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        int compared = 0;
        foreach ((MotherBrainGlassProgram program, ushort entry) in GlassEntries)
            compared += CompareProjectile(cartridge, RoomEnemyProjectileKind.MotherBrainGlassShard, new MotherBrainGlassAnimation(program), entry, 64, $"glass {program}");
        foreach ((MotherBrainTurretDirection direction, ushort turretPose, ushort bulletPose, int selectorEntry) in TurretEntries)
        {
            compared += CompareProjectile(cartridge, RoomEnemyProjectileKind.MotherBrainRoomTurret,
                new MotherBrainTurretAnimation(MotherBrainTurretProgram.TurretPose, direction),
                turretPose, 4, $"turret {direction}");
            compared += CompareProjectile(cartridge, RoomEnemyProjectileKind.MotherBrainRoomTurretBullet,
                new MotherBrainTurretAnimation(MotherBrainTurretProgram.BulletPose, direction),
                bulletPose, 4, $"bullet {direction}");
            // The bullet list's direction selector jumps to this pose; the typed bullet starts it directly.
            AssertEqual(bulletPose, cartridge.ReadCartridgeWord(selectorEntry),
                $"the bullet selector's {direction} entry");
        }
        compared += CompareProjectile(cartridge, RoomEnemyProjectileKind.MotherBrainRoomTurretBullet, new MotherBrainTurretAnimation(),
            BulletImpact, 64, "bullet impact");
        compared += CompareHandBeam(cartridge);
        Console.WriteLine($"  Mother Brain projectile programs: {compared} updates of glass, turret, bullet and hand-beam frames match bank $86.");
    }

    /// <summary>Runs <paramref name="animation"/> on a projectile of <paramref name="kind"/> beside its native list; returns the updates compared.</summary>
    private static int CompareProjectile(IImportCartridgeSource cartridge, RoomEnemyProjectileKind kind, EnemyProjectileAnimation animation,
        ushort entry, int updates, string name)
    {
        (RoomEnemySystem enemies, _) = NewMotherBrain();
        RoomEnemyProjectileSlot projectile = enemies.Projectiles.EnemyProjectiles[^1];
        FamilyProjectileStarts.Initialize(projectile, kind, MotherBrainTurretDefinitions.Graphics);
        projectile.StartAnimation(animation);
        var native = new NativeEnemyProjectileListDefinitions.Reader(entry, projectile.InstructionTimer.RawValue);
        int update = 0;
        for (; update < updates && !native.Deleted; update++)
        {
            enemies.Projectiles.ProcessEnemyProjectileInstructions(projectile, null, Position16<RoomPixels>.Zero);
            native.Step(cartridge);
            string at = $"{name} update {update}";
            AssertEqual(!native.Deleted, projectile.IsActive, $"{at}: deletion");
            if (native.Deleted)
                break;
            AssertEqual(native.Timer, projectile.InstructionTimer.RawValue, $"{at}: timer");
            AssertEqual(native.Operand, projectile.PresentationOperandAddress, $"{at}: frame");
            if (native.PaletteCleared)
                AssertEqual(SuperMetroid.Core.Assets.SpriteGraphics.Emptied, projectile.Graphics, $"{at}: the cleared graphics word, in the common colours");
            if (native.PreInstructionCleared)
                AssertEqual(EnemyProjectilePreInstruction.RTS_868170, projectile.PreInstruction, $"{at}: pre-instruction cleared");
        }
        return update;
    }

    /// <summary>
    /// The hand beam matches its list update by update, and spawns the next beam (advancing the shared aim) exactly on
    /// the updates its native list calls <c>Spawn_MotherBrainRedBeam_Fired</c>.
    /// </summary>
    private static int CompareHandBeam(IImportCartridgeSource cartridge)
    {
        (RoomEnemySystem enemies, MotherBrainEnemyState state) = NewMotherBrain();
        // The fired beams jitter their spawn with the random number generator.
        enemies.VerificationServices().NextRandom = () => 0;
        enemies.VerificationServices().ReadRandom = () => 0;
        state.HandBeamNextPosition = FixedPosition<RoomPixels>.FromWhole(new(128, 112));
        state.HandBeamNextVelocity = new(new SignedEightEight(256), new SignedEightEight(0));
        RoomEnemyProjectileSlot beam = enemies.Projectiles.EnemyProjectiles[^1];
        FamilyProjectileStarts.Initialize(beam, RoomEnemyProjectileKind.MotherBrainHandBeamCharging, default);
        beam.StartAnimation(new MotherBrainHandBeamAnimation());
        var native = new NativeEnemyProjectileListDefinitions.Reader(HandBeamProgram, beam.InstructionTimer.RawValue);
        int spawns = 0;
        int update = 0;
        for (; update < 64 && !native.Deleted; update++)
        {
            FixedPosition<RoomPixels> aim = state.HandBeamNextPosition;
            enemies.Projectiles.ProcessEnemyProjectileInstructions(beam, null, Position16<RoomPixels>.Zero);
            native.Step(cartridge);
            string at = $"hand beam update {update}";
            bool spawned = native.Calls.Count > 0;
            AssertTrue(native.Calls.All(call => HandBeamCalls.TryGetValue(call, out MotherBrainHandBeamCue cue) && cue == MotherBrainHandBeamCue.SpawnNextBeam),
                $"{at}: the native list calls only the next-beam spawn");
            AssertEqual(spawned, state.HandBeamNextPosition != aim, $"{at}: the next beam spawns with the native call");
            spawns += native.Calls.Count;
            AssertEqual(!native.Deleted, beam.IsActive, $"{at}: deletion");
            if (native.Deleted)
                break;
            AssertEqual(native.Timer, beam.InstructionTimer.RawValue, $"{at}: timer");
            AssertEqual(native.Operand, beam.PresentationOperandAddress, $"{at}: frame");
        }
        AssertEqual(MotherBrainHandBeamAnimations.StageCount, spawns, "the hand beam spawns the next beam once a stage");
        return update;
    }

    /// <summary>Runs Mother Brain's program parity checks against the repository cartridge.</summary>
    [VerificationCommand("--mother-brain-animations")]
    internal static int MotherBrainAnimationsCommand()
    {
        Suite(nameof(VerifyMotherBrainAnimations), () => VerifyMotherBrainAnimations(LoadRepositoryRom()));
        return 0;
    }

    /// <summary>Runs Mother Brain's projectile program parity checks against the repository cartridge.</summary>
    [VerificationCommand("--mother-brain-projectile-animations")]
    internal static int MotherBrainProjectileAnimationsCommand()
    {
        Suite(nameof(VerifyMotherBrainProjectileAnimations), () => VerifyMotherBrainProjectileAnimations(LoadRepositoryRom()));
        return 0;
    }

    /// <summary>Runs the Mother Brain program, beam, death, Samus, and Baby Metroid suites.</summary>
    [VerificationCommand("--mother-brain")]
    internal static int MotherBrainCommand()
    {
        Suite(nameof(VerifyMotherBrainAnimations), () => VerifyMotherBrainAnimations(LoadRepositoryRom()));
        Suite(nameof(VerifyMotherBrainProjectileAnimations), () => VerifyMotherBrainProjectileAnimations(LoadRepositoryRom()));
        Suite(nameof(VerifyMotherBrainBeamWindow), () => VerifyMotherBrainBeamWindow());
        Suite(nameof(VerifyMotherBrainDeathHandoff), () => VerifyMotherBrainDeathHandoff());
        Suite(nameof(VerifySamusDrainedController), () => VerifySamusDrainedController());
        Suite(nameof(VerifyMotherBrainRainbowBeamSamusMovement), () => VerifyMotherBrainRainbowBeamSamusMovement());
        Suite(nameof(VerifyMotherBrainRainbowBeamAttackSequence), () => VerifyMotherBrainRainbowBeamAttackSequence());
        Suite(nameof(MotherBrainEffectBehaviorVerification.VerifyMotherBrainEffectBehavior), () => MotherBrainEffectBehaviorVerification.VerifyMotherBrainEffectBehavior(LoadRepositoryRom()));
        Suite(nameof(VerifyBabyMetroidCutsceneEntrance), () => VerifyBabyMetroidCutsceneEntrance());
        return 0;
    }
}
