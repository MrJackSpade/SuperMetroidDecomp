using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Bosses.Kraid.KraidArmCollisionDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidFootCollisionDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidNativeListAddresses;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
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
        for (int index = 0; index < PartEntries.Length; index++)
            AssertEqual((KraidPartSegment)index, PartEntries[index].Program, "the Kraid part programs are declared in authored order");
        AssertTrue(PartEntriesAscend, "native Kraid part entries ascend");

        Dictionary<KraidPartSegment, NativeEnemyList<KraidPartSegment>> lists =
            PartEntries.ToDictionary(item => item.Program, item => ReadPartList(cartridge, item.Program, item.Entry));
        NativeEnemyAnimationOracle.CompareAnimations(lists, KraidAnimations.StartingAt);
        HashSet<KraidPartSegment> runOn = NativeEnemyAnimationOracle.RunsOnIntoNextAfterLastBeat(lists);
        AssertTrue(runOn.SetEquals(KraidAnimations.RunsOnInto.Keys), "the run-on segments are the lists that run on into the next after their last beat");
        foreach (KraidPartSegment segment in runOn)
        {
            AssertEqual(lists[segment].Controls.Single(control => control.Kind == NativeEnemyControlKind.RunOnIntoNext).Target, (KraidPartSegment?)KraidAnimations.RunsOnInto[segment],
                $"{segment} runs on into the list authored next");
        }

        VerifyArmSpeedSwitch();
        VerifyHeldAndLongestPoses();
        Suite(nameof(VerifyKraidArmCollisionDefinitions), () => VerifyKraidArmCollisionDefinitions(rom));
        Suite(nameof(VerifyKraidFootCollisionDefinitions), () => VerifyKraidFootCollisionDefinitions());
        Console.WriteLine($"  Kraid part animations: {PartEntries.Length} segments and {lists.Values.Sum(list => list.BeatAddresses.Length)} beats match bank $A7, " +
            "and every animation its lists' control flow; the arm's slow-swing switch matches the native callback.");
    }

    /// <summary>
    /// Reads one part segment's native list, comparing each of its beats with the segment's, and returns each beat's
    /// native address and the list's gotos, sleeps and runs on into the next authored list. The no-operation callback
    /// has no beat.
    /// </summary>
    private static NativeEnemyList<KraidPartSegment> ReadPartList(IImportCartridgeSource cartridge, KraidPartSegment segment, ushort entry)
    {
        IReadOnlyList<AnimationBeat<KraidPartFrame, KraidPartCue>> beats = KraidAnimations.Segments.Beats(segment);
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
            int address = (PartBankNumber << 16) | cursor;
            ushort word = cartridge.ReadCartridgeWord(address);
            ushort operand = cartridge.ReadCartridgeWord(address + 2);
            switch (NativeEnemyListDefinitions.Classify(word))
            {
                case NativeEnemyListDefinitions.WordKind.Frame when Beat(beat, at) is AnimationBeat<KraidPartFrame, KraidPartCue>.HeldFrame held:
                    // A held pose (#1627) stands for the list's longest frame followed at once by its sleep.
                    AssertEqual(NativeHeldFrameDuration, word, $"{at}: a held pose's native duration");
                    AssertEqual(operand, KraidPartFrameToolingDefinitions.Spritemap(held.Shown), $"{at}: held pose's spritemap");
                    AssertEqual(NativeEnemyListDefinitions.WordKind.Sleep, NativeEnemyListDefinitions.Classify(cartridge.ReadCartridgeWord(address + 4)),
                        $"{at}: a held pose's native frame is followed by its sleep");
                    addresses.Add(cursor);
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case NativeEnemyListDefinitions.WordKind.Frame:
                    var frame = AssertIs<AnimationBeat<KraidPartFrame, KraidPartCue>.Frame>(Beat(beat, at), $"{at}: frame");
                    AssertEqual(word, frame.Duration, $"{at}: duration");
                    AssertEqual(operand, KraidPartFrameToolingDefinitions.Spritemap(frame.Shown), $"{at}: spritemap");
                    addresses.Add(cursor);
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
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
                    var raised = AssertIs<AnimationBeat<KraidPartFrame, KraidPartCue>.Event>(Beat(beat, at), $"{at}: callback");
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

        AnimationBeat<KraidPartFrame, KraidPartCue> Beat(int beat, string at) => beat < beats.Count
            ? beats[beat]
            : throw new InvalidDataException($"{at}: the native list holds more beats than Kraid {segment}.");
    }

    /// <summary>Creates Kraid's body and arm in a fresh enemy system.</summary>
    private static NewKraidResult NewKraid()
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), services: new VerificationRoomEnemyServices(null));
        var state = new KraidEnemyState { InitialHealth = 104 };
        enemies.KraidBehavior.Access.Kraid = state;
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        body.Bind(EnemyDefinitionId.Kraid, body.Definition);
        RoomEnemySlot arm = enemies.SlotTable.Slots[1];
        arm.Bind(EnemyDefinitionId.KraidArm, arm.Definition);
        return new NewKraidResult(enemies, state, body, arm);
    }

    /// <summary>
    /// At full health the arm's normal swing loops; below half health the pause's callback starts the slow swing and
    /// shows its first frame in the same update, and once slow the callback keeps it slow.
    /// </summary>
    private static void VerifyArmSpeedSwitch()
    {
        (_, KraidEnemyState state, RoomEnemySlot body, RoomEnemySlot arm) = NewKraid();
        body.Health = 104;
        state.ArmState.Playback = Started(KraidPartSegment.ArmNormalPause);
        arm.AnimationClock.StepOnNextUpdate();
        Pass(arm);
        Pass(arm, forceExpiry: true);
        AssertEqual(KraidAnimations.StartingWith(KraidPartSegment.ArmNormalPause), state.ArmState.Playback.Animation, "a healthy arm plays on in its pause's loop");
        AssertEqual(KraidPartSegment.ArmNormal, PlayingSegment(state.ArmState.Playback), "a healthy arm loops its normal swing");

        body.Health = 51;
        state.ArmState.Playback = Started(KraidPartSegment.ArmNormalPause);
        arm.AnimationClock.StepOnNextUpdate();
        Pass(arm);
        Pass(arm, forceExpiry: true);
        AssertEqual(KraidAnimations.StartingWith(KraidPartSegment.ArmSlow), state.ArmState.Playback.Animation, "below half health the pause starts the slow swing");
        AssertEqual(KraidPartFrameToolingDefinitions.Spritemap(new KraidPartFrame(KraidPartFrameKind.Arm, 0)), arm.SpritemapPointer, "the slow swing's first frame shows at once");
        AssertEqual((ushort)8, arm.AnimationClock.Hold.RawValue, "the slow swing's first frame lasts eight updates");

        state.ArmState.Playback = new(KraidAnimations.StartingWith(KraidPartSegment.ArmSlow),
            new AnimationPlace(0, KraidAnimations.Segments.Beats(KraidPartSegment.ArmSlow).Count - 2));
        arm.AnimationClock.StepOnNextUpdate();
        Pass(arm);
        Pass(arm, forceExpiry: true);
        AssertEqual(KraidAnimations.StartingWith(KraidPartSegment.ArmSlow), state.ArmState.Playback.Animation, "a slow arm's callback keeps the slow swing");
    }

    /// <summary>
    /// #1627: a held foot pose shows once and rests for good, its frame kept and its new-frame flag clear through every
    /// later update, past the point its timer runs out and wraps; the arm's last lunge-ready pose keeps the exact longest
    /// count, which the pending-reopen freeze holds standing.
    /// </summary>
    private static void VerifyHeldAndLongestPoses()
    {
        (RoomEnemySystem enemies, KraidEnemyState state, _, RoomEnemySlot arm) = NewKraid();
        RoomEnemySlot foot = enemies.SlotTable.Slots[5];
        foot.Bind(EnemyDefinitionId.KraidFoot, foot.Definition);
        state.FootState.Playback = Started(KraidPartSegment.FootNeutral);
        foot.AnimationClock.StepOnNextUpdate();
        Pass(foot);
        ushort neutral = KraidPartFrameToolingDefinitions.Spritemap(new KraidPartFrame(KraidPartFrameKind.Foot, 0));
        AssertEqual(neutral, foot.SpritemapPointer, "the neutral foot shows its pose");
        AssertTrue(foot.AnimationClock.ShowsNewFrame, "the held pose raises the new-frame flag as it shows");
        // Past one whole wrap of the frame timer: the held pose never plays on, shows anew or raises the flag again.
        for (int update = 0; update < 70000; update++)
        {
            Pass(foot);
            if (foot.SpritemapPointer != neutral || foot.AnimationClock.ShowsNewFrame)
                throw new InvalidOperationException($"Verification failed: the held neutral foot changed at update {update}.");
        }

        state.ArmState.Playback = Started(KraidPartSegment.ArmDyingOrPreparingToLunge);
        for (int beat = 0; beat < 3; beat++)
            Pass(arm, forceExpiry: true);
        AssertEqual(KraidPartFrameToolingDefinitions.Spritemap(new KraidPartFrame(KraidPartFrameKind.Arm, 21)), arm.SpritemapPointer,
            "the arm shows its last lunge-ready pose");
        AssertEqual((ushort)32767, arm.AnimationClock.Hold.RawValue, "the last lunge-ready pose keeps the longest frame's exact count");
    }

    /// <summary>The part animation starting at <paramref name="segment"/>, before its first play.</summary>
    private static AnimationPlayback<KraidPartSegment> Started(KraidPartSegment segment) =>
        new(KraidAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);

    /// <summary>The segment <paramref name="playback"/>'s playing phase plays.</summary>
    private static KraidPartSegment PlayingSegment(AnimationPlayback<KraidPartSegment> playback) =>
        playback.Animation.Phases[playback.Place.Phase].Segment;

    /// <summary>Runs one arm update, optionally expiring the current frame first.</summary>
    private static void Pass(RoomEnemySlot part, bool forceExpiry = false)
    {
        if (forceExpiry)
            part.AnimationClock.StepOnNextUpdate();
        EnemyAnimationPlayer.AdvanceAnimation(part, samus: null, level: null, Position16<RoomPixels>.Zero, controllerInput: 0);
    }

    /// <summary>
    /// The real rock producers start typed programs that match their native lists, and the spat rock's burst sets
    /// its palette pre-instruction and deletes on the native updates. The Kago bug shares the rocks' pose.
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
            rock.StartAnimation(new KraidRockAnimation(program));
            var native = new NativeEnemyProjectileListReader(entry, rock.FrameHold.RawValue);
            for (int update = 0; update < 40 && !native.Deleted; update++)
            {
                enemies.Projectiles.AdvanceEnemyProjectileAnimations(rock, null, Position16<RoomPixels>.Zero, shortCounter: 0);
                native.Step(cartridge);
                string at = $"Kraid rock {program} update {update}";
                AssertEqual(!native.Deleted, rock.IsActive, $"{at}: deletion");
                if (native.Deleted)
                    break;
                // The still poses are held frames (#1627), whose timer rests apart from the native list's longest frame.
                if (!SuperMetroid.Verification.Enemies.NativeEnemyProjectileParity.RestsOnHeldFrame(native.Timer, rock.FrameHold))
                    AssertEqual(native.Timer, rock.FrameHold.RawValue, $"{at}: timer");
                AssertEqual(native.Operand, rock.PresentationOperandAddress, $"{at}: frame");
                if (native.FrameBehavior is { } frameBehavior)
                    AssertEqual(EnemyProjectileFrameBehaviorAccess.FromWord(frameBehavior), rock.FrameBehavior, $"{at}: pre-instruction");
            }
            AssertEqual(program == KraidRockSegment.SpitRockShot, native.Deleted, $"Kraid rock {program}: only the burst deletes its rock");
        }

        AssertEqual(KraidRockVisualDefinitions.ProjectileFrameCount, KraidRockVisualDefinitions.ProjectileFrames.Count, "the rock catalog lists every frame");
        Console.WriteLine("  Kraid rock programs: the poses and burst match bank $86 every update, with the palette pre-instruction and deletion.");
    }

    /// <summary>A Kraid body position and art for the real rock producers.</summary>
    private static RoomEnemySlot NewRockSource() => new(0, EnemySlotFixtures.Registry)
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
        RoomEnemySlot source = NewRockSource();
        random = 32;
        enemies.KraidBehavior.Head.SpawnKraidSpitRock(source);
        AssertTrue(enemies.Projectiles.EnemyProjectiles.Any(item => item.IsActive && item.Kind == RoomEnemyProjectileKind.KraidSpitRock),
            "the real spit producer allocates its rock in the enemy projectile pool");
        random = 32;
        enemies.KraidBehavior.Background.SpawnKraidCeilingRock(96);
        AssertTrue(enemies.Projectiles.EnemyProjectiles.Any(item => item.IsActive && item.Kind == RoomEnemyProjectileKind.KraidCeilingRock),
            "the real ceiling producer allocates its rock in the enemy projectile pool");
        var state = new KraidEnemyState();
        random = 0;
        enemies.KraidBehavior.Rise.RequestKraidRisingRock(source);
        random = 16;
        enemies.KraidBehavior.Rise.RequestKraidRisingRock(source);
        random = 2;
        AssertTrue(enemies.KagoBehavior.SpawnKagoBug(source), "the real Kago producer allocates its bug");
        foreach ((RoomEnemyProjectileKind kind, KraidRockSegment program) in new[]
                 {
                     new VerifyRockProducersCase(RoomEnemyProjectileKind.KraidSpitRock, KraidRockSegment.SharedPose),
                     new VerifyRockProducersCase(RoomEnemyProjectileKind.KraidCeilingRock, KraidRockSegment.SharedPose),
                     new VerifyRockProducersCase(RoomEnemyProjectileKind.KraidRisingRockLeft, KraidRockSegment.SharedPose),
                     new VerifyRockProducersCase(RoomEnemyProjectileKind.KraidRisingRockRight, KraidRockSegment.RisingRightPose),
                     new VerifyRockProducersCase(RoomEnemyProjectileKind.KagoBug, KraidRockSegment.SharedPose),
                 })
        {
            EnemyProjectile projectile = enemies.Projectiles.EnemyProjectiles.Single(item => item.Kind == kind);
            AssertTrue(projectile.Animation is KraidRockAnimation started && started.Program == program, $"{kind} starts the {program} program");
        }
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
