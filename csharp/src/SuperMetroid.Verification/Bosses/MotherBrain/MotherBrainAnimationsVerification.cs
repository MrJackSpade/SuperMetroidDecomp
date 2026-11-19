using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;
using static SuperMetroid.Verification.Bosses.MotherBrain.BabyMetroidVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainBeamWindowVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainDeathHandoffVerification;
using static SuperMetroid.TestSupport.MovieReplay.MotherBrainNativeListAddresses;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainRainbowVerification;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Samus.SamusDamageAndGrabVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.MotherBrain;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.MovieReplay.Verification;

/// <summary>
/// #1285, #1600: Mother Brain's typed body, brain, Baby, Shitroid, Zebetite, falling-tube and projectile animations
/// against the retail cartridge. Every typed segment must match its native list beat by beat and every animation the
/// lists' control flow; the brain's cadence,
/// random holds, body moves and the projectiles must behave as the native interpreters do.
/// </summary>
internal static class MotherBrainAnimationsVerification
{
    /// <summary>Every typed slot and brain program equals its native list, and the runners keep the native cadence.</summary>
    internal static void VerifyMotherBrainAnimations(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        int body = CompareBody(cartridge).Values.Sum(list => list.BeatAddresses.Length);
        int head = CompareHead(cartridge).Values.Sum(list => list.BeatAddresses.Length);
        CompareBaby(cartridge);
        CompareShitroid(cartridge);
        CompareZebetites(cartridge);
        CompareTubes(cartridge);
        VerifyRoomFlashList(cartridge);
        VerifyBrainCadence();
        VerifyBrainRandomHolds();
        VerifyBodyMoves();
        Console.WriteLine($"  Mother Brain animations: {body} body and {head} brain beats, the Baby, Shitroid, Zebetite and tube lists " +
            "match bank $A9/$A6; the brain's cadence, random holds and body moves match the native routines.");
    }

    /// <summary>The fake-death flash is fourteen frames of the reviewed duration, then a goto back to its start.</summary>
    private static void VerifyRoomFlashList(IImportCartridgeSource cartridge)
    {
        AssertEqual(MotherBrainRoomFlashDefinitions.FlashFrameCount, RoomFlashRecords.Length, "the room flash has fourteen frames");
        foreach ((int frame, int record) in RoomFlashRecords)
        {
            AssertEqual(MotherBrainRoomFlashDefinitions.FlashImageTicks, cartridge.ReadCartridgeWord(record), $"room flash frame {frame} duration");
            AssertEqual(SuperMetroid.AssetExtraction.Bosses.MotherBrain.MotherBrainRoomColorExtractor.FlashPaletteOperandAddress(frame), unchecked((ushort)(record + 2)),
                $"room flash frame {frame} palette operand");
        }
        AssertEqual(RoomFlashGoto.Opcode, cartridge.ReadCartridgeWord(RoomFlashLoop), "the room flash ends in a goto");
        AssertEqual(RoomFlashGoto.Target, cartridge.ReadCartridgeWord(RoomFlashLoop + 2), "the room flash loops to its first frame");
    }

    /// <summary>Creates Mother Brain's body and head in a fresh enemy system.</summary>
    private static NewMotherBrainResult NewMotherBrain()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), services: new VerificationRoomEnemyServices(null));
        RoomEnemyPlace bodyPlace = enemies.Places.InUpdateOrder[0];
        bodyPlace.Bind(EnemyDefinitionId.MotherBrainBody, bodyPlace.Definition);
        RoomEnemyPlace headPlace = enemies.Places.InUpdateOrder[1];
        headPlace.Bind(EnemyDefinitionId.MotherBrainHead, headPlace.Definition);
        var state = new MotherBrainEnemyState(bodyPlace, headPlace, new MotherBrainWalkCounter(), new MotherBrainBurstCycle(), new MotherBrainBrainAnimation());
        enemies.MotherBrainEncounter.BeginAsSetUp(state);
        return new NewMotherBrainResult(enemies, state);
    }

    /// <summary>
    /// The brain's cadence (#2317): each frame it moves on to shows for the updates its beat states, its list's duration and
    /// one draw more; an animation started at a frame shows it from the start, the start counting as its first update, and
    /// one started at a cue plays it, and shows the frame after it, at the first draw.
    /// </summary>
    private static void VerifyBrainCadence()
    {
        foreach (MotherBrainHeadSegment program in new[] { MotherBrainHeadSegment.NeutralPhase2, MotherBrainHeadSegment.Laser, MotherBrainHeadSegment.Corpse })
        {
            (RoomEnemySystem enemies, MotherBrainEnemyState state) = NewMotherBrain();
            MotherBrainEncounterAccess.SetMotherBrainBrainAnimation(state, program);
            var frames = MotherBrainAnimations.HeadSegments.Beats(program).OfType<AnimationBeat<EnemyFrameId, MotherBrainHeadCue>.Frame>().ToArray();
            AssertEqual<EnemyFrameId?>(frames[0].Shown, state.Brain.ShownFrame, $"{program} shows its first frame from its start");
            for (int draw = 1; draw < frames[0].Duration; draw++)
            {
                AssertEqual((EnemyFrameId?)frames[0].Shown, enemies.MotherBrainBehavior.Head.AdvanceMotherBrainBrain(state, samus: RoomEnemyFrameFixtures.StandaloneSamus()),
                    $"{program} shows its first frame on draw {draw}");
                AssertTrue(!state.Brain.Clock.ShowsNewFrame, $"{program} holds its first frame on draw {draw}");
            }
            for (int draw = 0; draw < 2; draw++)
            {
                AssertEqual((EnemyFrameId?)frames[1].Shown, enemies.MotherBrainBehavior.Head.AdvanceMotherBrainBrain(state, samus: RoomEnemyFrameFixtures.StandaloneSamus()),
                    $"{program} shows its second frame on its draw {draw}");
                AssertEqual(draw == 0, state.Brain.Clock.ShowsNewFrame, $"{program} moves on to its second frame on its draw {draw} alone");
            }
        }
        {
            (RoomEnemySystem enemies, MotherBrainEnemyState state) = NewMotherBrain();
            MotherBrainEncounterAccess.SetMotherBrainBrainAnimation(state, MotherBrainHeadSegment.StretchingPhase2);
            var opening = MotherBrainAnimations.HeadSegments.Beats(MotherBrainHeadSegment.StretchingPhase2).OfType<AnimationBeat<EnemyFrameId, MotherBrainHeadCue>.Frame>().First();
            AssertEqual<EnemyFrameId?>(null, state.Brain.ShownFrame, "a stretch, which opens with a cue, shows nothing before its first draw");
            for (int draw = 0; draw < opening.Duration; draw++)
            {
                AssertEqual((EnemyFrameId?)opening.Shown, enemies.MotherBrainBehavior.Head.AdvanceMotherBrainBrain(state, samus: RoomEnemyFrameFixtures.StandaloneSamus()),
                    $"the stretch shows its first frame on draw {draw}");
                AssertEqual(draw == 0, state.Brain.Clock.ShowsNewFrame, $"the stretch's first frame is shown on its first draw alone, draw {draw}");
            }
            enemies.MotherBrainBehavior.Head.AdvanceMotherBrainBrain(state, samus: RoomEnemyFrameFixtures.StandaloneSamus());
            AssertTrue(state.Brain.Clock.ShowsNewFrame, "the stretch moves on past its first frame once its updates have passed");
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
            MotherBrainEncounterAccess.SetMotherBrainBrainAnimation(state, MotherBrainHeadSegment.NeutralPhase2Hold);
            for (int draw = 0; draw <= 8; draw++)
                enemies.MotherBrainBehavior.Head.AdvanceMotherBrainBrain(state, samus: RoomEnemyFrameFixtures.StandaloneSamus());
            // Holding restarts the hold's animation and shows its first frame; leaving shows the hold's last frame before its goto.
            AssertEqual(new AnimationPlayback<MotherBrainHeadSegment>(MotherBrainAnimations.HeadStartingWith(MotherBrainHeadSegment.NeutralPhase2Hold),
                    new AnimationPlace(0, holds ? 1 : 3)),
                state.Brain.Playback, $"random {random} {(holds ? "holds" : "leaves")} the phase-two neutral pose");
        }
        foreach ((ushort random, bool bursts) in new[] { new VerifyBrainRandomHoldsCase(4063, false), new VerifyBrainRandomHoldsCase(4064, true) })
        {
            (RoomEnemySystem enemies, MotherBrainEnemyState state) = NewMotherBrain();
            enemies.VerificationServices().ReadRandom = () => random;
            // The dying drool's loop shows its first frame, which the burst's last drool has just moved on to.
            var loopFrame = (AnimationBeat<EnemyFrameId, MotherBrainHeadCue>.Frame)MotherBrainAnimations.HeadSegments.Beats(MotherBrainHeadSegment.DyingDroolLoop)[0];
            state.Brain.Playback = new(MotherBrainAnimations.HeadStartingWith(MotherBrainHeadSegment.DyingDrool), new AnimationPlace(2, 1));
            state.Brain.ShownFrame = loopFrame.Shown;
            state.Brain.Clock.Show(loopFrame.Duration);
            for (int draw = 0; draw < loopFrame.Duration; draw++)
                enemies.MotherBrainBehavior.Head.AdvanceMotherBrainBrain(state, samus: RoomEnemyFrameFixtures.StandaloneSamus());
            AnimationPlayback<MotherBrainHeadSegment> drooling = state.Brain.Playback ?? throw new InvalidOperationException("Verification requires a brain playback.");
            AssertEqual(bursts ? MotherBrainHeadSegment.DyingDroolBurst : MotherBrainHeadSegment.DyingDroolLoop, drooling.Animation.Phases[drooling.Place.Phase].Segment,
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
        state.BodyPlacement = MotherBrainBodyPlacement.FollowsBody;
        state.Body.Position = FixedPosition<RoomPixels>.FromWhole(new(64, 112));
        Position16<RoomPixels> origin = state.Body.Position.Whole;
        void Run(MotherBrainBodySegment program)
        {
            MotherBrainEncounterAccess.SetMotherBrainBodyAnimation(state, program);
            for (int update = 0; update < 256; update++)
            {
                enemies.MotherBrainBehavior.Body.AdvanceMotherBrainBodyAnimation(state, RoomEnemyFrameFixtures.StandaloneSamus());
                // Every move carries her body with her (#1949, #2031): her posture changes no longer move it across.
                AssertEqual<Position16<RoomPixels>?>(state.Body.Position.Whole, state.BodyPlacedAt,
                    $"{program}: her body stays placed at her");
            }
        }
        Run(MotherBrainBodySegment.WalkForwardReallyFast);
        AssertEqual(origin + new Offset16<RoomPixels>(24, 0), state.Body.Position.Whole, "a forward walk moves the body 24 pixels right");
        AssertEqual(MotherBrainBodyPose.Standing, state.Pose, "a forward walk ends standing");
        Run(MotherBrainBodySegment.WalkBackwardMedium);
        AssertEqual(origin, state.Body.Position.Whole, "a backward walk returns the body");
        Run(MotherBrainBodySegment.CrouchSlow);
        AssertEqual(origin + new Offset16<RoomPixels>(0, 38), state.Body.Position.Whole, "the slow crouch lowers the body 38 pixels");
        AssertEqual(MotherBrainBodyPose.Crouched, state.Pose, "the slow crouch ends crouched");
        Run(MotherBrainBodySegment.StandAfterCrouchSlow);
        AssertEqual(origin, state.Body.Position.Whole, "the slow stand-up raises the body again");
        AssertEqual(MotherBrainBodyPose.Standing, state.Pose, "the slow stand-up ends standing");
    }

    /// <summary>
    /// The glass shards and sparkle, the hand beam, and the turrets' poses and impact smoke match their bank-$86 lists
    /// update by update through the real projectile engine.
    /// </summary>
    internal static void VerifyMotherBrainProjectileAnimations(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        int compared = 0;
        foreach ((MotherBrainGlassSegment program, ushort entry) in GlassEntries)
            compared += CompareProjectile(cartridge, RoomEnemyProjectileKind.MotherBrainGlassShard,
                enemies => new MotherBrainGlassAnimation(enemies.MotherBrainBehavior.Projectiles.Projectile, program), entry, 64, $"glass {program}");
        foreach ((MotherBrainTurretDirection direction, ushort turretPose, ushort bulletPose, int selectorEntry) in TurretEntries)
        {
            compared += CompareProjectile(cartridge, RoomEnemyProjectileKind.MotherBrainRoomTurret,
                enemies => new MotherBrainTurretAnimation(enemies.MotherBrainBehavior.Projectiles.Projectile, MotherBrainTurretPose.Turret(direction)),
                turretPose, 4, $"turret {direction}", timesItsFrames: false);
            compared += CompareProjectile(cartridge, RoomEnemyProjectileKind.MotherBrainRoomTurretBullet,
                enemies => new MotherBrainTurretAnimation(enemies.MotherBrainBehavior.Projectiles.TurretBullet, MotherBrainTurretPose.Bullet(direction)),
                bulletPose, 4, $"bullet {direction}", timesItsFrames: false);
            // The bullet list's direction selector jumps to this pose; the typed bullet starts it directly.
            AssertEqual(bulletPose, cartridge.ReadCartridgeWord(selectorEntry),
                $"the bullet selector's {direction} entry");
        }
        compared += CompareProjectile(cartridge, RoomEnemyProjectileKind.MotherBrainRoomTurretBullet,
            enemies => new MotherBrainTurretAnimation(enemies.MotherBrainBehavior.Projectiles.TurretBullet, MotherBrainTurretPose.BulletImpact),
            BulletImpact, 64, "bullet impact");
        compared += CompareHandBeam(cartridge);
        Console.WriteLine($"  Mother Brain projectile programs: {compared} updates of glass, turret, bullet and hand-beam frames match bank $86.");
    }

    /// <summary>
    /// Runs <paramref name="animation"/> on a projectile of <paramref name="kind"/> beside its native list; returns the
    /// updates compared. A still pose, which holds its frame for good (#2318), times nothing, so its timer is not compared.
    /// </summary>
    private static int CompareProjectile(IImportCartridgeSource cartridge, RoomEnemyProjectileKind kind, Func<RoomEnemySystem, EnemyProjectileAnimation> animation,
        ushort entry, int updates, string name, bool timesItsFrames = true)
    {
        (RoomEnemySystem enemies, _) = NewMotherBrain();
        EnemyProjectile projectile = enemies.Projectiles.ProjectileInPlace(17);
        FamilyProjectileStarts.Initialize(enemies, projectile, kind, MotherBrainTurretDefinitions.Graphics);
        projectile.StartAnimation(animation(enemies));
        var native = new NativeEnemyProjectileListReader(entry, projectile.FrameHold.RawValue);
        int update = 0;
        for (; update < updates && !native.Deleted; update++)
        {
            enemies.Projectiles.AdvanceEnemyProjectileAnimations(projectile, RoomEnemyFrameFixtures.StandaloneSamus(), Position16<RoomPixels>.Zero, shortCounter: 0);
            native.Step(cartridge);
            string at = $"{name} update {update}";
            AssertEqual(!native.Deleted, projectile.IsActive, $"{at}: deletion");
            if (native.Deleted)
                break;
            if (timesItsFrames)
                SuperMetroid.Verification.Enemies.NativeEnemyProjectileParity.AssertFrameHold(native.Timer, projectile, at);
            AssertEqual(native.Operand, projectile.PresentationOperandAddress, $"{at}: frame");
            if (native.PaletteCleared)
                AssertEqual(SuperMetroid.Core.Enemies.SpriteGraphics.SharedStandard, projectile.ShownGraphics, $"{at}: the frame after the cleared graphics word draws from the room's sheet in its standard colours");
            if (native.FrameBehaviorCleared)
                AssertEqual<Enum?>(null, projectile.MotionValue, $"{at}: pre-instruction cleared");
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
        EnemyProjectile beam = enemies.Projectiles.ProjectileInPlace(17);
        FamilyProjectileStarts.Initialize(enemies, beam, RoomEnemyProjectileKind.MotherBrainHandBeamCharging, default);
        // The charging beam holds the encounter that spawned it, as its spawn gives it (#2218).
        enemies.MotherBrainBehavior.Projectiles.Projectile.MotherBrains.Start(beam, new MotherBrainProjectileState(state));
        beam.StartAnimation(new MotherBrainHandBeamAnimation(enemies.MotherBrainBehavior.Projectiles.Projectile));
        var native = new NativeEnemyProjectileListReader(HandBeamProgram, beam.FrameHold.RawValue);
        int spawns = 0;
        int update = 0;
        for (; update < 64 && !native.Deleted; update++)
        {
            FixedPosition<RoomPixels> aim = state.HandBeamNextPosition;
            enemies.Projectiles.AdvanceEnemyProjectileAnimations(beam, RoomEnemyFrameFixtures.StandaloneSamus(), Position16<RoomPixels>.Zero, shortCounter: 0);
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
            SuperMetroid.Verification.Enemies.NativeEnemyProjectileParity.AssertFrameHold(native.Timer, beam, at);
            // The second and third stages draw the first stage's pictures from operands of their own (#2210).
            AssertEqual<EnemyProjectileFrameId?>(EnemyProjectileProgramFrameOperands.Frame(native.Operand, beam.Graphics.Owner), beam.Frame, $"{at}: frame");
        }
        AssertEqual(MotherBrainHandBeamAnimationsConstants.StageCount, spawns, "the hand beam spawns the next beam once a stage");
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
        Suite(nameof(MotherBrainRainbowSamusCooldownVerification.VerifyMotherBrainRainbowSamusCooldown), () => MotherBrainRainbowSamusCooldownVerification.VerifyMotherBrainRainbowSamusCooldown());
        Suite(nameof(MotherBrainEffectBehaviorVerification.VerifyMotherBrainEffectBehavior), () => MotherBrainEffectBehaviorVerification.VerifyMotherBrainEffectBehavior(LoadRepositoryRom()));
        Suite(nameof(VerifyBabyMetroidCutsceneEntrance), () => VerifyBabyMetroidCutsceneEntrance());
        return 0;
    }
}
