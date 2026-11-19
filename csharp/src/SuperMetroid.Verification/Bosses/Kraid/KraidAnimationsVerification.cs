using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Bosses.Kraid.KraidArmCollisionDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidFootCollisionDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidNativeListAddresses;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Kago;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.Kraid;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.MovieReplay.Verification;

/// <summary>
/// #1284, #1600: Kraid's typed foot, arm, lint, fingernail and rock animations against the retail cartridge. Each part
/// segment must match its native bank-$A7 list beat by beat and each part animation the lists' control flow; the runner
/// must switch the arm to its slow swing exactly as the native callback does; and the rocks must match their bank-$86
/// lists update by update.
/// </summary>
internal static class KraidAnimationsVerification
{
    /// <summary>The native duration of the longest frame a list gives, which a typed held pose stands for.</summary>
    private const ushort NativeHeldFrameDuration = 0x7fff;

    /// <summary>
    /// Every part segment's beats equal its native list and every part animation follows the lists' control flow, the
    /// arm switches to its slow swing below half health, and the arm and foot collision tables still cover every typed frame.
    /// </summary>
    internal static void VerifyKraidPartAnimations(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        AssertEqual(Enum.GetValues<KraidPartSegment>().Length, PartEntries.Length, "every typed Kraid part program has a native entry");
        AssertTrue(PartEntriesAscend, "native Kraid part entries ascend");

        Dictionary<KraidPartSegment, NativeEnemyList<KraidPartSegment>> lists =
            PartEntries.ToDictionary(item => item.Program, item => ReadPartList(cartridge, item.Program, item.Entry));
        NativeEnemyAnimationOracle.CompareAnimations(lists, KraidAnimations.StartingAt);

        VerifyArmSpeedSwitch();
        VerifyLimbMarks();
        VerifyHeldAndLongestPoses();
        VerifyArmWaitsForMouthReopens();
        VerifyNailSetUpPose();
        Suite(nameof(VerifyKraidArmCollisionDefinitions), () => VerifyKraidArmCollisionDefinitions(rom));
        Suite(nameof(VerifyKraidFootCollisionDefinitions), () => VerifyKraidFootCollisionDefinitions());
        Console.WriteLine($"  Kraid part animations: {PartEntries.Length} segments and {lists.Values.Sum(list => list.BeatAddresses.Length)} beats match bank $A7, " +
            "and every animation its lists' control flow; the arm's slow-swing switch matches the native callback.");
    }

    /// <summary>
    /// Reads one part segment's native list, comparing each of its beats with the segment's, and returns each beat's
    /// native address and the list's gotos, sleeps and runs on into the next authored list. The no-operation callback
    /// has no beat, and a marker beat (#2301), the port's own, occupies none: it stands at the address of what follows it.
    /// </summary>
    private static NativeEnemyList<KraidPartSegment> ReadPartList(IImportCartridgeSource cartridge, KraidPartSegment segment, ushort entry)
    {
        IReadOnlyList<AnimationBeat<KraidPartPose, KraidPartCue>> beats = KraidAnimations.Segments.Beats(segment);
        var addresses = new List<ushort>();
        var controls = new List<NativeEnemyControl<KraidPartSegment>>();
        ushort cursor = entry;
        for (int command = 0; command < 512; command++)
        {
            int beat = addresses.Count;
            string at = $"Kraid {segment} beat {beat}";
            if (cursor != entry && PartProgramAt(cursor) is { } next)
            {
                controls.Add(new(NativeEnemyControlKind.RunOnIntoNext, beat, next, null, cursor));
                return Finish();
            }
            if (beat < beats.Count && beats[beat] is AnimationBeat<KraidPartPose, KraidPartCue>.Marker)
            {
                addresses.Add(cursor);
                continue;
            }
            int address = (PartBankNumber << 16) | cursor;
            ushort word = cartridge.ReadCartridgeWord(address);
            ushort operand = cartridge.ReadCartridgeWord(address + 2);
            switch (NativeEnemyListDefinitions.Classify(word))
            {
                case NativeEnemyListDefinitions.WordKind.Frame when Beat(beat, at) is AnimationBeat<KraidPartPose, KraidPartCue>.HeldFrame held:
                    // A held pose (#1627) stands for the list's longest frame followed at once by its sleep.
                    AssertEqual(NativeHeldFrameDuration, word, $"{at}: a held pose's native duration");
                    AssertEqual<KraidPartPose?>(KraidPartFrameToolingDefinitionsConstants.PoseAt(operand), held.Shown, $"{at}: the held pose its native spritemap stores");
                    AssertEqual(NativeEnemyListDefinitions.WordKind.Sleep, NativeEnemyListDefinitions.Classify(cartridge.ReadCartridgeWord(address + 4)),
                        $"{at}: a held pose's native frame is followed by its sleep");
                    addresses.Add(cursor);
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case NativeEnemyListDefinitions.WordKind.Frame:
                {
                    var frame = AssertIs<AnimationBeat<KraidPartPose, KraidPartCue>.Frame>(Beat(beat, at), $"{at}: frame");
                    AssertEqual<KraidPartPose?>(KraidPartFrameToolingDefinitionsConstants.PoseAt(operand), frame.Shown, $"{at}: the pose its native spritemap stores");
                    addresses.Add(cursor);
                    cursor = unchecked((ushort)(cursor + 4));
                    // A pose the list shows again straight after, in its own list, past no-operation callbacks only and from
                    // either of its spritemaps, is one typed frame of both durations (#2296): nothing reads a Kraid part's
                    // frame timer or new-frame flag between them.
                    int shown = word;
                    while (shown < frame.Duration && PartProgramAt(cursor) is null)
                    {
                        ushort following = cursor;
                        while (PartCallbacks.TryGetValue(cartridge.ReadCartridgeWord((PartBankNumber << 16) | following), out KraidPartCue? skipped) && skipped is null)
                            following = unchecked((ushort)(following + 2));
                        ushort again = cartridge.ReadCartridgeWord((PartBankNumber << 16) | following);
                        if (PartProgramAt(following) is not null || NativeEnemyListDefinitions.Classify(again) != NativeEnemyListDefinitions.WordKind.Frame
                            || KraidPartFrameToolingDefinitionsConstants.PoseAt(cartridge.ReadCartridgeWord(((PartBankNumber << 16) | following) + 2)) != frame.Shown)
                            break;
                        shown += again;
                        cursor = unchecked((ushort)(following + 4));
                    }
                    AssertEqual(frame.Duration, shown, $"{at}: duration");
                    continue;
                }
                case NativeEnemyListDefinitions.WordKind.GoTo:
                    controls.Add(new(NativeEnemyControlKind.Goto, beat,
                        PartProgramAt(operand) ?? throw new InvalidDataException($"{at}: ${operand:X4} is no segment's native entry."), null, cursor));
                    return Finish();
                case NativeEnemyListDefinitions.WordKind.Sleep:
                    controls.Add(new(NativeEnemyControlKind.Sleep, beat, null, null, cursor));
                    return Finish();
                case NativeEnemyListDefinitions.WordKind.Callback:
                    AssertTrue(PartCallbacks.TryGetValue(word, out KraidPartCue? cue), $"{at}: native callback ${word:X4} is known");
                    if (cue is not { } expected)
                    {
                        cursor = unchecked((ushort)(cursor + 2));
                        continue; // The no-operation callback has no beat.
                    }
                    var raised = AssertIs<AnimationBeat<KraidPartPose, KraidPartCue>.Event>(Beat(beat, at), $"{at}: callback");
                    AssertEqual(expected, raised.Raised, $"{at}: cue");
                    addresses.Add(cursor);
                    cursor = unchecked((ushort)(cursor + 2));
                    continue;
                case NativeEnemyListDefinitions.WordKind.Stop:
                case NativeEnemyListDefinitions.WordKind.SetCounter:
                case NativeEnemyListDefinitions.WordKind.RepeatWhileCounting:
                case NativeEnemyListDefinitions.WordKind.Wait:
                    throw new InvalidDataException($"{at}: Kraid's part lists use no stops, counters or waits.");
                default:
                    throw new InvalidOperationException($"Undefined {nameof(NativeEnemyListDefinitions.WordKind)}.");
            }
        }
        throw new InvalidDataException($"Kraid {segment} ran past its native list.");

        NativeEnemyList<KraidPartSegment> Finish()
        {
            AssertEqual(beats.Count, addresses.Count, $"Kraid {segment}: its native list holds each of its beats");
            return new NativeEnemyList<KraidPartSegment>([.. addresses], [.. controls]);
        }

        AnimationBeat<KraidPartPose, KraidPartCue> Beat(int beat, string at) => beat < beats.Count
            ? beats[beat]
            : throw new InvalidDataException($"{at}: the native list holds more beats than Kraid {segment}.");
    }

    /// <summary>Creates Kraid's body and arm in a fresh enemy system.</summary>
    private static NewKraidResult NewKraid()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), services: new VerificationRoomEnemyServices(null));
        RoomEnemyPlace body = enemies.Places.InUpdateOrder[0];
        body.Bind(EnemyDefinitionId.Kraid, body.Definition);
        RoomEnemyPlace arm = enemies.Places.InUpdateOrder[1];
        arm.Bind(EnemyDefinitionId.KraidArm, arm.Definition);
        // The parts bind before the encounter begins, so each holds it as its setup would.
        KraidEnemyState state = enemies.KraidBehavior.BeginEncounterInRoomOrder(enemies.Places);
        state.InitialHealth = 104;
        return new NewKraidResult(enemies, state, body, arm);
    }

    /// <summary>
    /// At full health the arm's normal swing loops; below half health the pause's callback starts the slow swing and
    /// shows its first frame in the same update, and once slow the callback keeps it slow.
    /// </summary>
    private static void VerifyArmSpeedSwitch()
    {
        (_, KraidEnemyState state, RoomEnemyPlace body, RoomEnemyPlace arm) = NewKraid();
        body.Health = 104;
        state.ArmState.SetAnimation(KraidPartSegment.ArmNormalPause);
        arm.AnimationClock.StepOnNextUpdate();
        Pass(arm);
        Pass(arm, forceExpiry: true);
        AssertEqual(KraidAnimations.StartingWith(KraidPartSegment.ArmNormalPause), state.ArmState.Playback.Animation, "a healthy arm plays on in its pause's loop");
        AssertEqual(KraidPartSegment.ArmNormal, PlayingSegment(state.ArmState.Playback), "a healthy arm loops its normal swing");

        body.Health = 51;
        state.ArmState.SetAnimation(KraidPartSegment.ArmNormalPause);
        arm.AnimationClock.StepOnNextUpdate();
        Pass(arm);
        Pass(arm, forceExpiry: true);
        AssertEqual(KraidAnimations.StartingWith(KraidPartSegment.ArmSlow), state.ArmState.Playback.Animation, "below half health the pause starts the slow swing");
        AssertEqual(KraidPartFrameToolingDefinitions.Spritemap(new KraidPartPose(EnemyFrameId.KraidArmRaisedClawClosed)), arm.SpritemapPointer, "the slow swing's first frame shows at once");
        AssertEqual(8, arm.AnimationClock.Hold, "the slow swing's first frame lasts eight updates");

        state.ArmState.Playback = new(KraidAnimations.StartingWith(KraidPartSegment.ArmSlow),
            new AnimationPlace(0, KraidAnimations.Segments.Beats(KraidPartSegment.ArmSlow).Count - 2));
        arm.AnimationClock.StepOnNextUpdate();
        Pass(arm);
        Pass(arm, forceExpiry: true);
        AssertEqual(KraidAnimations.StartingWith(KraidPartSegment.ArmSlow), state.ArmState.Playback.Animation, "a slow arm's callback keeps the slow swing");
    }

    /// <summary>
    /// #2301: the arm's and the foot's marks follow their animations, update by update through the production player, exactly
    /// as the places they stand for: the arm has yet to pause while it plays its normal swing and has not shown that swing's
    /// last frame, whichever animation it starts and at either health, and the foot shows its walk's end while it plays a
    /// walk or lunge and has shown its last frame, the backward walk's laps included.
    /// </summary>
    private static void VerifyLimbMarks()
    {
        (RoomEnemySystem enemies, KraidEnemyState state, RoomEnemyPlace body, RoomEnemyPlace arm) = NewKraid();
        foreach (int health in new[] { 104, 51 })
        {
            foreach (KraidPartSegment start in new[] { KraidPartSegment.ArmNormal, KraidPartSegment.ArmNormalPause, KraidPartSegment.ArmSlow })
            {
                body.Health = health;
                state.ArmState.StartAnimation(start);
                var seen = new HashSet<bool>();
                for (int update = 0; update < 400; update++)
                {
                    AnimationPlayback<KraidPartSegment> playback = state.ArmState.Playback;
                    bool yetToPause = PlayingSegment(playback) == KraidPartSegment.ArmNormal
                        && playback.Place.Beat < playback.Animation.Phases[playback.Place.Phase].BeatCount;
                    AssertEqual(yetToPause, state.ArmState.YetToPause, $"Kraid's arm from {start} at health {health}, update {update}: yet to pause");
                    seen.Add(yetToPause);
                    Pass(arm);
                }
                AssertTrue(start != KraidPartSegment.ArmNormal || seen.Count == 2, $"Kraid's arm's normal swing at health {health} reaches its pause");
            }
        }

        RoomEnemyPlace foot = enemies.Places.InUpdateOrder[5];
        foot.Bind(EnemyDefinitionId.KraidFoot, foot.Definition);
        // The foot binds after the encounter began, and joins it as its setup would.
        KraidEncounterAccess.AttachKraidPart(state.FootState);
        // The walks' cues raise, lower and step the body: it stands clear of the room's edges while they do.
        body.Position = body.Position.WithWhole(new(512, 512));
        foreach (KraidPartSegment walk in new[] { KraidPartSegment.FootWalkingForward, KraidPartSegment.FootLungeForward, KraidPartSegment.FootWalkingBackward })
        {
            state.FootState.StartAnimation(walk);
            int shownEnds = 0;
            for (int update = 0; update < 300; update++)
            {
                AnimationPlayback<KraidPartSegment> playback = state.FootState.Playback;
                bool showsEnd = PlayingSegment(playback) == walk
                    && playback.Place.Beat >= playback.Animation.Phases[playback.Place.Phase].BeatCount;
                AssertEqual(showsEnd, state.FootState.ShowsWalkEnd, $"Kraid's foot {walk}, update {update}: shows its walk's end");
                shownEnds += showsEnd ? 1 : 0;
                Pass(foot);
            }
            AssertTrue(shownEnds > 0, $"Kraid's foot {walk} reaches its walk's end");
        }
    }

    /// <summary>
    /// #1627: a held foot pose shows once and rests for good, its frame kept and its new-frame flag clear through every
    /// later update, past the point its timer runs out and wraps; the arm's last lunge-ready pose is held the same way
    /// (#2213).
    /// </summary>
    private static void VerifyHeldAndLongestPoses()
    {
        (RoomEnemySystem enemies, KraidEnemyState state, _, RoomEnemyPlace arm) = NewKraid();
        RoomEnemyPlace foot = enemies.Places.InUpdateOrder[5];
        foot.Bind(EnemyDefinitionId.KraidFoot, foot.Definition);
        // The foot binds after the encounter began, and joins it as its setup would.
        KraidEncounterAccess.AttachKraidPart(state.FootState);
        state.FootState.Playback = Started(KraidPartSegment.FootNeutral);
        foot.AnimationClock.StepOnNextUpdate();
        Pass(foot);
        ushort neutral = KraidPartFrameToolingDefinitions.Spritemap(new KraidPartPose(EnemyFrameId.KraidFootPlanted));
        AssertEqual(neutral, foot.SpritemapPointer, "the neutral foot shows its pose");
        AssertTrue(foot.AnimationClock.ShowsNewFrame, "the held pose raises the new-frame flag as it shows");
        // Past one whole wrap of the frame timer: the held pose never plays on, shows anew or raises the flag again.
        for (int update = 0; update < 70000; update++)
        {
            Pass(foot);
            if (foot.SpritemapPointer != neutral || foot.AnimationClock.ShowsNewFrame)
                throw new InvalidOperationException($"Verification failed: the held neutral foot changed at update {update}.");
        }

        state.ArmState.SetAnimation(KraidPartSegment.ArmDyingOrPreparingToLunge);
        for (int beat = 0; beat < 3; beat++)
            Pass(arm, forceExpiry: true);
        ushort outstretched = KraidPartFrameToolingDefinitions.Spritemap(new KraidPartPose(EnemyFrameId.KraidArmOutstretched));
        AssertEqual(outstretched, arm.SpritemapPointer, "the arm shows its last lunge-ready pose");
        // #2213: the pose is held, not timed by the longest frame duration: it rests until the arm's next animation starts.
        AssertIs<AnimationBeat<KraidPartPose, KraidPartCue>.HeldFrame>(KraidAnimations.Segments.Beats(KraidPartSegment.ArmDyingOrPreparingToLunge)[^1],
            "the arm's last lunge-ready pose is held");
        for (int update = 0; update < 256; update++)
        {
            Pass(arm);
            if (arm.SpritemapPointer != outstretched)
                throw new InvalidOperationException($"Verification failed: the held lunge-ready arm changed at update {update}.");
        }

        // #2213: a lint's stopped clock never steps its animation, through any number of updates and a lengthened hold,
        // until its owner steps it again.
        RoomEnemyPlace lint = enemies.Places.InUpdateOrder[2];
        lint.Bind(EnemyDefinitionId.KraidLintTop, lint.Definition);
        // The lint binds after the encounter began, and joins it as its setup would.
        KraidEncounterAccess.AttachKraidPart(state.TopLintState);
        state.TopLintState.Playback = Started(KraidPartSegment.LintInitial);
        lint.AnimationClock.Stop();
        lint.AnimationClock.Extend(1);
        int stoppedHold = lint.AnimationClock.Hold;
        for (int update = 0; update < 256; update++)
        {
            Pass(lint);
            if (lint.Frame is not null || lint.AnimationClock.ShowsNewFrame || lint.AnimationClock.Hold != stoppedHold ||
                state.TopLintState.Playback != Started(KraidPartSegment.LintInitial))
                throw new InvalidOperationException($"Verification failed: the stopped lint's animation stepped at update {update}.");
        }
        Pass(lint, forceExpiry: true);
        AssertEqual((EnemyFrameId?)EnemyFrameId.KraidLintBudding, lint.Frame, "a stepped lint clock plays its animation again");
    }

    /// <summary>
    /// #2463: while mouth reopens are pending the arm's swing waits where it stands, its frame and that frame's remaining hold
    /// kept through every update of the wait, and once none is pending the frame shows out the rest of its hold before the
    /// swing goes on, as the game's arm did by adding back each update its animation took off its timer.
    /// </summary>
    private static void VerifyArmWaitsForMouthReopens()
    {
        (RoomEnemySystem enemies, KraidEnemyState state, _, RoomEnemyPlace arm) = NewKraid();
        state.ArmState.SetAnimation(KraidPartSegment.ArmNormal);
        arm.AnimationClock.StepOnNextUpdate();
        Update();
        Update();
        EnemyFrameId? shown = arm.Frame;
        int held = arm.AnimationClock.Hold;
        AssertTrue(shown is not null && held > 1, "the arm's swing shows a frame with updates of its hold left");
        state.ArmMouthReopens();
        for (int update = 0; update < 256; update++)
        {
            Update();
            if (arm.Frame != shown || arm.AnimationClock.Hold != held || arm.AnimationClock.ShowsNewFrame)
                throw new InvalidOperationException($"Verification failed: the waiting arm's swing moved on at update {update} of the reopens.");
        }
        state.ClearMouthReactions();
        for (int update = 1; update < held; update++)
        {
            Update();
            AssertEqual(shown, arm.Frame, $"the arm still shows its frame {update} updates after the reopens");
        }
        Update();
        AssertTrue(arm.AnimationClock.ShowsNewFrame && arm.Frame != shown, "the arm's swing shows its next frame as the rest of its hold runs out");

        // One arm update: its main update, then its animation.
        void Update()
        {
            enemies.KraidBehavior.Limbs.RunKraidArmMain(arm, state, cameraY: 0);
            Pass(arm);
        }
    }

    /// <summary>
    /// #2463: a fingernail's setup shows its spin's first pose through the spin's animation, and the spin waits there, its
    /// clock stopped, through every update until its flight starts it.
    /// </summary>
    private static void VerifyNailSetUpPose()
    {
        (RoomEnemySystem enemies, KraidEnemyState state, _, _) = NewKraid();
        KraidNailState part = state.Nail(KraidNailPart.First);
        RoomEnemyPlace nail = part.Place;
        nail.Bind(EnemyDefinitionId.KraidFirstNail, nail.Definition);
        enemies.KraidBehavior.Projectiles.InitializeKraidNail(nail, KraidNailPart.First);
        AssertEqual((EnemyFrameId?)EnemyFrameId.KraidNailTurned0, nail.Frame, "a fingernail set up shows its spin's first pose");
        AssertEqual(KraidAnimations.StartingWith(KraidPartSegment.NailLoop), part.Playback.Animation, "the pose is its spin's");
        AssertTrue(nail.AnimationClock.IsStopped, "the set-up fingernail's spin waits");
        for (int update = 0; update < 256; update++)
        {
            Pass(nail);
            if (nail.Frame != EnemyFrameId.KraidNailTurned0 || nail.AnimationClock.ShowsNewFrame)
                throw new InvalidOperationException($"Verification failed: the waiting fingernail's spin moved on at update {update}.");
        }
    }

    /// <summary>The part animation starting at <paramref name="segment"/>, before its first play.</summary>
    private static AnimationPlayback<KraidPartSegment> Started(KraidPartSegment segment) =>
        new(KraidAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);

    /// <summary>The segment <paramref name="playback"/>'s playing phase plays.</summary>
    private static KraidPartSegment PlayingSegment(AnimationPlayback<KraidPartSegment> playback) =>
        playback.Animation.Phases[playback.Place.Phase].Segment;

    /// <summary>Runs one arm update, optionally expiring the current frame first.</summary>
    private static void Pass(RoomEnemyPlace part, bool forceExpiry = false)
    {
        if (forceExpiry)
            part.AnimationClock.StepOnNextUpdate();
        EnemyAnimationPlayer.AdvanceAnimation(part, samus: PassSamus, level: PassLevel, Position16<RoomPixels>.Zero, controllerInput: 0);
    }

    /// <summary>The Samus every animation pass sees; no Kraid part animation reads her.</summary>
    private static SuperMetroid.Core.Samus.SamusState PassSamus => field ??= RoomEnemyFrameFixtures.StandaloneSamus();

    /// <summary>The open room every animation pass runs in, built once.</summary>
    private static SuperMetroid.Core.Rooms.LevelData.RoomLevelData PassLevel => field ??= RoomEnemyFrameFixtures.OpenRoom(new(64, 64));

    /// <summary>
    /// The real rock producers start typed programs that match their native lists, and the spat rock's burst sets
    /// its stopped flight, whose native palette pre-instruction moves nothing (#2260), and deletes on the native updates.
    /// </summary>
    internal static void VerifyKraidRockAnimations(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        AssertEqual(Enum.GetValues<KraidRockSegment>().Length, RockEntries.Length, "every typed rock program has a native entry");
        VerifyRockProducers();
        foreach ((KraidRockSegment program, ushort entry) in RockEntries)
        {
            (RoomEnemySystem enemies, _, _, _) = NewKraid();
            enemies.VerificationServices().ReadRandom = () => 32;
            enemies.KraidBehavior.Head.SpawnKraidSpitRock(NewRockSource());
            EnemyProjectile rock = enemies.Projectiles.EnemyProjectiles.Single(item => item.IsActive && item.Kind == RoomEnemyProjectileKind.KraidSpitRock);
            rock.StartAnimation(new KraidRockAnimation(enemies.KraidBehavior.Projectiles.SpitRock, program));
            var native = new NativeEnemyProjectileListReader(entry, rock.FrameHold.RawValue);
            for (int update = 0; update < 40 && !native.Deleted; update++)
            {
                enemies.Projectiles.AdvanceEnemyProjectileAnimations(rock, RoomEnemyFrameFixtures.StandaloneSamus(), Position16<RoomPixels>.Zero, shortCounter: 0);
                native.Step(cartridge);
                string at = $"Kraid rock {program} update {update}";
                AssertEqual(!native.Deleted, rock.IsActive, $"{at}: deletion");
                if (native.Deleted)
                    break;
                // The still poses are held frames (#1627), which stop the hold (#2380) while the native list counts its
                // longest frame down.
                SuperMetroid.Verification.Enemies.NativeEnemyProjectileParity.AssertFrameHold(native.Timer, rock, at);
                AssertEqual(native.Operand, rock.PresentationOperandAddress, $"{at}: frame");
                if (native.FrameBehavior is { } frameBehavior)
                    AssertEqual(EnemyProjectileMotionAccess.FromWord(frameBehavior), rock.MotionValue, $"{at}: pre-instruction");
            }
            AssertEqual(program == KraidRockSegment.SpitRockShot, native.Deleted, $"Kraid rock {program}: only the burst deletes its rock");
        }

        Console.WriteLine("  Kraid rock programs: the poses and burst match bank $86 every update, with the stopped flight and deletion.");
    }

    /// <summary>A Kraid body position and art for the real rock producers.</summary>
    private static RoomEnemyPlace NewRockSource() => new(0, EnemyPlaceFixtures.Registry, new RemovedEnemyLeftovers<EnemyOccupantStates>())
    {
        Position = FixedPosition<RoomPixels>.FromWhole(new(128, 224)),
        Shading = SuperMetroid.Core.Enemies.Drawing.EnemyShades.FrozenLook,
    };

    /// <summary>Every real rock producer, and Kago's bug, starts the typed pose its native list starts.</summary>
    private static void VerifyRockProducers()
    {
        (RoomEnemySystem enemies, _, _, _) = NewKraid();
        ushort random = 0;
        enemies.VerificationServices().ReadRandom = () => random;
        RoomEnemyPlace source = NewRockSource();
        random = 32;
        enemies.KraidBehavior.Head.SpawnKraidSpitRock(source);
        AssertTrue(enemies.Projectiles.EnemyProjectiles.Any(item => item.IsActive && item.Kind == RoomEnemyProjectileKind.KraidSpitRock),
            "the real spit producer allocates its rock in the enemy projectile pool");
        random = 32;
        enemies.KraidBehavior.Background.SpawnKraidCeilingRock(96);
        AssertTrue(enemies.Projectiles.EnemyProjectiles.Any(item => item.IsActive && item.Kind == RoomEnemyProjectileKind.KraidCeilingRock),
            "the real ceiling producer allocates its rock in the enemy projectile pool");
        var state = new KraidEnemyState(new KraidParts());
        random = 0;
        enemies.KraidBehavior.Rise.RequestKraidRisingRock(source);
        random = 16;
        enemies.KraidBehavior.Rise.RequestKraidRisingRock(source);
        random = 2;
        enemies.KagoBehavior.SpawnKagoBug(source);
        AssertTrue(enemies.Projectiles.EnemyProjectiles.Any(item => item.IsActive && item.Kind == RoomEnemyProjectileKind.KagoBug),
            "the real Kago producer allocates its bug");
        foreach ((RoomEnemyProjectileKind kind, KraidRockSegment program) in new[]
                 {
                     new VerifyRockProducersCase(RoomEnemyProjectileKind.KraidSpitRock, KraidRockSegment.SharedPose),
                     new VerifyRockProducersCase(RoomEnemyProjectileKind.KraidCeilingRock, KraidRockSegment.SharedPose),
                 })
        {
            EnemyProjectile projectile = enemies.Projectiles.EnemyProjectiles.Single(item => item.Kind == kind);
            AssertTrue(projectile.Animation is KraidRockAnimation started && started.Program == program, $"{kind} starts the {program} program");
        }
        // Both rising rocks are one kind, the random sample's side choosing each one's pose (#2278).
        EnemyProjectile[] rising = [.. enemies.Projectiles.EnemyProjectiles.Where(item => item.Kind == RoomEnemyProjectileKind.KraidRisingRock)];
        AssertEqual(2, rising.Length, "both rising rocks spawn as one kind");
        foreach (KraidRockSegment pose in new[] { KraidRockSegment.SharedPose, KraidRockSegment.RisingRightPose })
            AssertEqual(1, rising.Count(rock => rock.Animation is KraidRockAnimation started && started.Program == pose),
                $"one rising rock starts the {pose} program");
        EnemyProjectile bug = enemies.Projectiles.EnemyProjectiles.Single(item => item.Kind == RoomEnemyProjectileKind.KagoBug);
        AssertTrue(bug.Animation is KagoBugProjectileAnimation { PlayingSegment: KagoBugProjectileSegment.Waiting },
            "a new Kago bug starts its own waiting animation");
    }

    /// <summary>Runs Kraid's part program parity check against the repository cartridge.</summary>
    [VerificationCommand("--kraid-part-animations")]
    internal static int KraidPartAnimationsCommand()
    {
        Suite(nameof(VerifyKraidPartAnimations), () => VerifyKraidPartAnimations(LoadRepositoryRom()));
        return 0;
    }

    /// <summary>Runs Kraid's rock program parity check against the repository cartridge.</summary>
    [VerificationCommand("--kraid-rock-animations")]
    internal static int KraidRockAnimationsCommand()
    {
        Suite(nameof(VerifyKraidRockAnimations), () => VerifyKraidRockAnimations(LoadRepositoryRom()));
        return 0;
    }
}
