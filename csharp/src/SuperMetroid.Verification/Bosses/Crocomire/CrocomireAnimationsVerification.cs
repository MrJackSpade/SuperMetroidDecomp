using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Bosses.Crocomire.CrocomireNativeListAddresses;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
namespace SuperMetroid.Verification.Bosses.Crocomire;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.MovieReplay.Verification;

/// <summary>
/// #1282, #1600: Crocomire's typed body, tongue and projectile animations against the retail cartridge. Each body
/// segment must match its native bank-$A4 list beat by beat and each body animation the lists' control flow, and the
/// tongue and projectile animations must match their native lists update by update: timers, drawn frames, cues, jumps,
/// fall-through and the new-frame flag.
/// </summary>
internal static class CrocomireAnimationsVerification
{
    /// <summary>
    /// Every body segment's beats equal the native list, and each copy of it the cartridge writes out (#2399): frames by
    /// spritemap and duration, and callbacks as cues; every body animation follows the lists' gotos, sleeps, written-out
    /// repeats and runs on into the list authored next from the cartridge entry it starts at. The real body runner then
    /// plays the skeleton collapse, a fight-AI redirect and the damage wait's repeated roar beside the native list.
    /// </summary>
    internal static void VerifyCrocomireBodyAnimations(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        AssertTrue(BodyEntriesAscend, "native body entries ascend");
        AssertEqual(Enum.GetValues<CrocomireBodyAnimation>().Length, BodyStarts.Count, "every body animation has a native entry");

        Dictionary<CrocomireBodySegment, NativeEnemyList<CrocomireBodySegment>> lists = NativeEnemyAnimationOracle.Lists(cartridge, BodyBankNumber,
            BodyListEntries, CrocomireBodyAnimations.Segments, CrocomireBodyFramesTooling.Spritemap,
            new NativeEnemyCueRules<CrocomireBodyCue>(
                word => BodyCallbacks.ContainsKey(word) ? 0 : null,
                (word, _, cue, at) => AssertEqual(BodyCallbacks[word], cue, $"{at}: cue")));
        NativeEnemyAnimationOracle.CompareNamedStarts(lists, BodyListEntries,
            [.. WithNativeRoarRunOn(lists).Select(pair => (pair.Key.ToString(), pair.Value, BodyStarts[pair.Key]))]);
        AssertTrue(lists.Values.SelectMany(list => list.Repeats.Prepend(list)).All(list => list.Controls.All(control =>
                control.Kind is NativeEnemyControlKind.Goto or NativeEnemyControlKind.Sleep or NativeEnemyControlKind.RunOnIntoNext
                    or NativeEnemyControlKind.LoopStart or NativeEnemyControlKind.WrittenOutRepeat)),
            "Crocomire's body lists use no stops, counters or waits");
        AssertEqual(83, TypedBodySpritemaps(skeleton: false).Concat(TypedBodySpritemaps(skeleton: true)).Distinct().Count(),
            "the body segments draw all 50 body and 33 skeleton frames");

        VerifyCollapseMarkerPlace(lists);
        VerifySkeletonCollapse(cartridge);
        VerifyFightAiRedirect();
        VerifyRoarRepeats();
        Console.WriteLine($"  Crocomire body animations: {lists.Count} segments and {lists.Values.Sum(list => list.BeatAddresses.Length)} beats match bank $A4, " +
            "and every animation its lists' control flow; the runner matches the skeleton collapse and fight-AI redirects.");
    }

    /// <summary>
    /// The body animations as the native lists run them (#1938, #2180): the step back and the damage wait run on from the
    /// roar's closed-mouth loop, which the cartridge writes out twice, into the Power Bomb reaction authored after it. The
    /// first fight cue of that loop always starts the roar again or steps him back (<see cref="VerifyRoarRepeats"/>), so the
    /// animations play the loop once, to that cue, and then roar again; the oracle compares them with the native lists with
    /// the unreachable second play of the loop and the tail after it restored.
    /// </summary>
    private static Dictionary<CrocomireBodyAnimation, Animation<CrocomireBodySegment>> WithNativeRoarRunOn(
        Dictionary<CrocomireBodySegment, NativeEnemyList<CrocomireBodySegment>> lists)
    {
        NativeEnemyList<CrocomireBodySegment> loopList = lists[CrocomireBodySegment.RoarCloseMouthLoop];
        AssertEqual((CrocomireBodySegment?)CrocomireBodySegment.PowerBombReactionMouthFullyOpen,
            loopList.Controls.Single(control => control.Kind == NativeEnemyControlKind.RunOnIntoNext).Target,
            "the roar's closed-mouth loop runs on into the Power Bomb reaction");
        int loopPlays = 1 + loopList.Controls.Count(control => control.Kind == NativeEnemyControlKind.WrittenOutRepeat);
        AnimationPhase<CrocomireBodySegment> playedLoop = CrocomireBodyAnimations.Segments.Whole(CrocomireBodySegment.RoarCloseMouthLoop);
        AssertTrue(CrocomireBodyAnimations.Segments.Beats(CrocomireBodySegment.RoarCloseMouthLoop)[^1] is AnimationBeat<EnemyFrameId, CrocomireBodyCue>.Event
            { Raised: CrocomireBodyCue.Signal { Kind: CrocomireBodyCueKind.FightAi } }, "the closed-mouth loop ends with its fight cue");
        Animation<CrocomireBodySegment> roar = CrocomireBodyAnimations.StartingWith(CrocomireBodyAnimation.Roar);
        Animation<CrocomireBodySegment> reaction = CrocomireBodyAnimations.StartingWith(CrocomireBodyAnimation.PowerBombReactionMouthFullyOpen);
        AnimationEnd.Loop reactionLoop = AssertIs<AnimationEnd.Loop>(reaction.End, "the Power Bomb reaction's end");
        var native = new Dictionary<CrocomireBodyAnimation, Animation<CrocomireBodySegment>>();
        foreach ((CrocomireBodyAnimation start, Animation<CrocomireBodySegment> animation) in CrocomireBodyAnimations.StartingAt)
        {
            int closed = animation.Phases.ToList().IndexOf(playedLoop);
            if (closed < 0)
            {
                native[start] = animation;
                continue;
            }
            // After the loop's first fight cue the animation roars again: it goes on into the roar animation's phases.
            AnimationEnd.Loop again = AssertIs<AnimationEnd.Loop>(animation.End, $"{start}'s end");
            AssertTrue(animation.Phases.Skip(again.Phase).SequenceEqual(roar.Phases) && roar.End == new AnimationEnd.Loop(0),
                $"{start} roars again once its roar has closed his mouth");
            native[start] = new([.. animation.Phases.Take(closed), playedLoop with { Plays = loopPlays }, .. reaction.Phases],
                new AnimationEnd.Loop(closed + 1 + reactionLoop.Phase));
        }
        return native;
    }

    /// <summary>Each body segment's native list and every copy of it, in authored order.</summary>
    private static NativeProgramEntry<CrocomireBodySegment>[] BodyListEntries =>
        [.. BodyEntries.Select(item => new NativeProgramEntry<CrocomireBodySegment>(item.Program, item.Entry))];

    /// <summary>Creates a Crocomire body with its encounter state in a fresh enemy system.</summary>
    private static NewCrocomireResult NewCrocomire()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        RoomEnemyPlace body = enemies.Places.InUpdateOrder[0];
        body.Bind(EnemyDefinitionId.Crocomire, body.Definition);
        body.Position = body.Position.WithWhole(new(576, 144));
        var state = new CrocomireEnemyState(body);
        body.SetSpeciesState(state);
        state.JoinFixtureTongue(enemies.Places.InUpdateOrder[1]);
        enemies.VerificationServices().ReadRandom = () => ushort.MaxValue;
        enemies.VerificationServices().NextRandom = () => ushort.MaxValue;
        return new NewCrocomireResult(enemies, state);
    }

    /// <summary>Starts <paramref name="animation"/> on the body as a native list install does.</summary>
    private static void Start(CrocomireEnemyState state, CrocomireBodyAnimation animation)
    {
        state.BodyPlayback = new AnimationPlayback<CrocomireBodySegment>(CrocomireBodyAnimations.StartingWith(animation), AnimationPlace.StartOfAnimation);
        state.Body.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>Runs one body update through the production interpreter.</summary>
    private static void Pass(RoomEnemySystem enemies, CrocomireEnemyState state, SamusState? samus = null)
    {
        enemies.Audio.BeginFrame();
        EnemyAnimationPlayer.AdvanceAnimation(state.Body, samus ?? RoomEnemyFrameFixtures.StandaloneSamus(), level: RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), Position16<RoomPixels>.Zero, controllerInput: 0);
    }

    /// <summary>
    /// #2301: the collapse's marker occupies no native beat: it stands at the native address of the last picture after it, and
    /// a recorded pointer at that address imports as the place before the marker, so the imported play still marks the
    /// collapse as it shows the last picture.
    /// </summary>
    private static void VerifyCollapseMarkerPlace(Dictionary<CrocomireBodySegment, NativeEnemyList<CrocomireBodySegment>> lists)
    {
        IReadOnlyList<AnimationBeat<EnemyFrameId, CrocomireBodyCue>> beats = CrocomireBodyAnimations.Segments.Beats(CrocomireBodySegment.SkeletonFallsApart);
        int marker = beats.ToList().FindIndex(beat => beat is AnimationBeat<EnemyFrameId, CrocomireBodyCue>.Marker
            { Raised: CrocomireBodyCue.Signal { Kind: CrocomireBodyCueKind.SkeletonCollapsed } });
        AssertTrue(marker >= 0 && beats[marker + 1].ShownFrame is not null, "the collapse marks its end just before its last picture");
        ushort[] addresses = lists[CrocomireBodySegment.SkeletonFallsApart].BeatAddresses;
        AssertEqual(addresses[marker + 1], addresses[marker], "the collapse's marker stands at its last picture's native address");
        Animation<CrocomireBodySegment> fallsApart = CrocomireBodyAnimations.StartingWith(CrocomireBodyAnimation.SkeletonFallsApart);
        AnimationPlayback<CrocomireBodySegment> imported = NativeEnemyAnimationOracle.PlaybackAt(lists, BodyListEntries,
            new Dictionary<CrocomireBodySegment, Animation<CrocomireBodySegment>> { [CrocomireBodySegment.SkeletonFallsApart] = fallsApart }, addresses[marker]);
        AssertEqual(new AnimationPlayback<CrocomireBodySegment>(fallsApart, new AnimationPlace(0, marker)),
            imported, "a pointer at the last picture imports as the place before the collapse's marker");
    }

    /// <summary>
    /// The skeleton falls apart and settles exactly as the native list does, update by update: timer, spritemap,
    /// new-frame flag and sounds. The death phase's test sees the collapse ended as soon as its last frame shows, where the
    /// native cursor already points at the stable list.
    /// </summary>
    private static void VerifySkeletonCollapse(IImportCartridgeSource cartridge)
    {
        (RoomEnemySystem enemies, CrocomireEnemyState state) = NewCrocomire();
        Start(state, CrocomireBodyAnimation.SkeletonFallsApart);
        var native = new NativeEnemyListReader(BodyBankNumber, BodyEntry(CrocomireBodySegment.SkeletonFallsApart), 1, _ => 0);
        for (int update = 0; update < 200; update++)
        {
            Pass(enemies, state);
            List<ushort> callbacks = [.. native.Step(cartridge).Select(callback => callback.Word)];
            string at = $"Crocomire skeleton collapse update {update}";
            // The stable skeleton is a held frame (#1627), which stops the body's clock (#2454).
            SuperMetroid.Verification.Enemies.NativeEnemyProjectileParity.AssertClockHold(native.Timer, state.Body.AnimationClock, at);
            AssertEqual(native.Spritemap, state.Body.SpritemapPointer, $"{at}: frame");
            AssertEqual(native.NewFrame, state.Body.AnimationClock.ShowsNewFrame, $"{at}: new-frame flag");
            AssertEqual(EnemyAudioQueueAccess.SoundList(callbacks.Select(BodySound).OfType<SoundEffectId>()), enemies.Audio.QueuedSoundList,
                $"{at}: every sound the native list queues, in its order");
            AssertEqual(HasReached(native, CrocomireBodySegment.SkeletonStable), state.SkeletonHasCollapsed,
                $"{at}: the collapse has ended once the native cursor reaches the stable skeleton");
        }
        AssertTrue(state.SkeletonHasCollapsed, "the collapse settles on the stable skeleton");
    }

    /// <summary>
    /// A fight-AI cue that starts another program shows that program's first frame in the same update, as the
    /// native instruction redirects the interpreter: Samus near the sleeping body starts the damage wait.
    /// </summary>
    private static void VerifyFightAiRedirect()
    {
        (RoomEnemySystem enemies, CrocomireEnemyState state) = NewCrocomire();
        state.Phase = CrocomirePhase.Dormant;
        Start(state, CrocomireBodyAnimation.IdleSway);
        Pass(enemies, state);
        AssertEqual(CrocomireBodyFramesTooling.Spritemap(EnemyFrameId.CrocomireBodyArmUprightMouthOpen), state.Body.SpritemapPointer,
            "the sleeping body shows its initial frame");
        AssertEqual(1, state.Body.AnimationClock.Hold, "the initial frame lasts one update");

        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork());
        samus.Position = samus.Position.WithWholeX(state.Body.Position.X.Whole);
        Pass(enemies, state, samus);
        AssertEqual(CrocomirePhase.WaitingForFirstDamage, state.Phase, "Samus within range wakes Crocomire");
        AssertEqual(CrocomireBodyFramesTooling.Spritemap(EnemyFrameId.CrocomireBodyArmUprightMouthShut), state.Body.SpritemapPointer,
            "the redirect shows the damage wait's first frame in the same update");
        AssertEqual(34, state.Body.AnimationClock.Hold, "the damage wait's first frame lasts 34 updates");
        AssertEqual(CrocomireBodyAnimations.StartingWith(CrocomireBodyAnimation.WaitForFirstSecondDamage), state.BodyPlayback.Animation, "the damage wait animation plays");
    }

    /// <summary>
    /// While waiting for damage, the roar plays again only once it has closed his mouth: the cues while it closes play on,
    /// and after the closed mouth's first cue the roar shows again in the same update.
    /// </summary>
    private static void VerifyRoarRepeats()
    {
        (RoomEnemySystem enemies, CrocomireEnemyState state) = NewCrocomire();
        state.Phase = CrocomirePhase.WaitingForFirstDamage;
        Start(state, CrocomireBodyAnimation.RoarCloseMouth);
        CrocomireAnimationsExpected[] expected = [new CrocomireAnimationsExpected(26, 32), new CrocomireAnimationsExpected(25, 5), new CrocomireAnimationsExpected(25, 8), new CrocomireAnimationsExpected(24, 2), new CrocomireAnimationsExpected(24, 1), new CrocomireAnimationsExpected(24, 48)];
        for (int index = 0; index < expected.Length; index++)
        {
            state.Body.AnimationClock.StepOnNextUpdate();
            Pass(enemies, state);
            AssertEqual(CrocomireBodyFramesTooling.Spritemap(CrocomireBodyFramesTooling.BodyOrder[expected[index].Frame]), state.Body.SpritemapPointer,
                $"roar close frame {index}");
            AssertEqual((ushort)expected[index].Duration, state.Body.AnimationClock.Hold, $"roar close duration {index}");
        }
        AssertEqual(CrocomireBodySegment.RoarOpenMouth, state.BodyPlayback.Animation.Phases[state.BodyPlayback.Place.Phase].Segment,
            "the closed mouth's fight cue is followed by the roar again");
    }

    /// <summary>Each tongue program plays its native list update by update through the production interpreter.</summary>
    internal static void VerifyCrocomireTongueAnimations(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        AssertEqual(Enum.GetValues<CrocomireTongueSegment>().Length, TongueEntries.Length, "every tongue program has a native entry");
        foreach ((CrocomireTongueSegment program, ushort entry) in TongueEntries)
        {
            (RoomEnemySystem enemies, CrocomireEnemyState state) = NewCrocomire();
            RoomEnemyPlace tongue = enemies.Places.InUpdateOrder[1];
            tongue.Bind(EnemyDefinitionId.CrocomireTongue, tongue.Definition);
            tongue.SetSpeciesState(new CrocomireTongueState(state));
            state.JoinTongue(tongue);
            state.TonguePlayback = new AnimationPlayback<CrocomireTongueSegment>(CrocomireAnimations.TongueStartingWith(program), AnimationPlace.StartOfAnimation);
            tongue.AnimationClock.StepOnNextUpdate();
            var native = new NativeEnemyListReader(BodyBankNumber, entry, 1, _ => 0);
            for (int update = 0; update < 60; update++)
            {
                EnemyAnimationPlayer.AdvanceAnimation(tongue, samus: RoomEnemyFrameFixtures.StandaloneSamus(), level: RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), Position16<RoomPixels>.Zero, controllerInput: 0);
                List<NativeEnemyListDefinitions.Callback> callbacks = native.Step(cartridge);
                string at = $"Crocomire tongue {program} update {update}";
                AssertEqual(0, callbacks.Count, $"{at}: the tongue lists have no callbacks");
                SuperMetroid.Verification.Enemies.NativeEnemyProjectileParity.AssertClockHold(native.Timer, tongue.AnimationClock, at);
                AssertEqual(native.Spritemap, tongue.SpritemapPointer, $"{at}: frame");
                AssertEqual(native.NewFrame, tongue.AnimationClock.ShowsNewFrame, $"{at}: new-frame flag");
            }
        }
        Console.WriteLine("  Crocomire tongue animations: fight, sleep and melting match bank $A4 every update.");
    }

    /// <summary>
    /// The real mouth, bridge and spike producers start typed programs that match their native lists, and the
    /// shot program explodes, drops with Crocomire's chances and deletes on the native updates.
    /// </summary>
    internal static void VerifyCrocomireProjectileAnimations(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);

        (RoomEnemySystem mouthSystem, CrocomireEnemyState mouthState) = NewCrocomire();
        mouthSystem.CrocomireBehavior.Fight.SpawnCrocomireProjectile(mouthState.Body, gradient: CrocomireProjectileDefinitions.AuthoredGradients[0]);
        EnemyProjectile mouth = mouthSystem.Projectiles.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.CrocomireProjectile);
        CompareProjectile(cartridge, mouthSystem, mouth, RoomEnemyProjectileKind.CrocomireProjectile, shot: false, "mouth");

        (RoomEnemySystem bridgeSystem, CrocomireEnemyState bridgeState) = NewCrocomire();
        bridgeSystem.CrocomireBehavior.BridgeCollapse.SpawnNextCrocomireBridgeFragment(bridgeState.Death);
        EnemyProjectile bridge = bridgeSystem.Projectiles.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.CrocomireBridgeCrumbling);
        CompareProjectile(cartridge, bridgeSystem, bridge, RoomEnemyProjectileKind.CrocomireBridgeCrumbling, shot: false, "bridge");

        (RoomEnemySystem spikeSystem, CrocomireEnemyState spikeState) = NewCrocomire();
        spikeSystem.CrocomireBehavior.Skeleton.SpawnCrocomireSpikeWallPieces(spikeState);
        EnemyProjectile[] spikes = [.. spikeSystem.Projectiles.EnemyProjectiles.Where(
            projectile => projectile.Kind == RoomEnemyProjectileKind.CrocomireSpikeWallPieces)];
        AssertEqual(8, spikes.Length, "the spike wall breaks into eight pieces");
        foreach (EnemyProjectile spike in spikes)
            CompareProjectile(cartridge, spikeSystem, spike, RoomEnemyProjectileKind.CrocomireSpikeWallPieces, shot: false, "spike");

        (RoomEnemySystem shotSystem, CrocomireEnemyState shotState) = NewCrocomire();
        shotSystem.CrocomireBehavior.Fight.SpawnCrocomireProjectile(shotState.Body, gradient: CrocomireProjectileDefinitions.AuthoredGradients[0]);
        EnemyProjectile exploding = shotSystem.Projectiles.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.CrocomireProjectile);
        exploding.StartAnimation(new CrocomireProjectileAnimation(shotSystem.CrocomireBehavior.Combat.MouthShot, CrocomireProjectileSegment.Shot));
        CompareProjectile(cartridge, shotSystem, exploding, RoomEnemyProjectileKind.CrocomireProjectile, shot: true, "shot");
        AssertTrue(!exploding.IsActive, "the shot program deletes the projectile");
        AssertTrue(shotSystem.Projectiles.EnemyProjectiles.Any(projectile => projectile.Kind == RoomEnemyProjectileKind.EnemyDeathPickup),
            "the shot program allocates a drop before deleting the projectile");

        Console.WriteLine("  Crocomire projectile programs: mouth, bridge, spike and shot match bank $86 every update, with the drop and deletion.");
    }

    /// <summary>Offset of the shot-reaction list in a native <c>EprojDef</c>, which production never reads.</summary>
    private const int ShotListOffset = 12;

    /// <summary>Offset of the initial instruction list in a native <c>EprojDef</c>, which production starts as a typed program.</summary>
    private const int InitialListOffset = 4;

    /// <summary>Runs a typed projectile beside the native list of its kind for fifty updates and compares every observable.</summary>
    private static void CompareProjectile(IImportCartridgeSource cartridge, RoomEnemySystem enemies, EnemyProjectile projectile,
        RoomEnemyProjectileKind kind, bool shot, string label)
    {
        AssertTrue(projectile.Animation is CrocomireProjectileAnimation, $"Crocomire {label} runs a typed program");
        AssertTrue(projectile.Animation is not null, $"Crocomire {label} keeps no native list");
        var native = new NativeProjectileList(cartridge.ReadCartridgeWord(NativeEnemyProjectileListDefinitions.ProjectileBank |
                (EnemyProjectileNativeWords.Kinds.Word(kind) + (shot ? ShotListOffset : InitialListOffset))),
            projectile.FrameHold.RawValue);
        int dropsBefore = enemies.Projectiles.EnemyProjectiles.Count(item => item.Kind == RoomEnemyProjectileKind.EnemyDeathPickup);
        for (int update = 0; update < 50 && !native.Deleted; update++)
        {
            enemies.Projectiles.AdvanceEnemyProjectileAnimations(projectile, RoomEnemyFrameFixtures.StandaloneSamus(), Position16<RoomPixels>.Zero, shortCounter: 0);
            native.Step(cartridge);
            string at = $"Crocomire {label} update {update}";
            AssertEqual(!native.Deleted, projectile.IsActive, $"{at}: deletion");
            AssertEqual(native.Drops,
                enemies.Projectiles.EnemyProjectiles.Count(item => item.Kind == RoomEnemyProjectileKind.EnemyDeathPickup) - dropsBefore,
                $"{at}: drops");
            if (native.Deleted)
                break;
            // The bridge fragment and spike-wall piece hold their still frame for good (#1627), which stops their hold
            // (#2380) while the native list counts its longest frame down.
            SuperMetroid.Verification.Enemies.NativeEnemyProjectileParity.AssertFrameHold(native.Timer, projectile, at);
            // An operand that stores a pose's picture again shows that pose (#2268).
            AssertEqual(SuperMetroid.AssetExtraction.Enemies.EnemyProjectileProgramFrameOperands.OwnOperand(native.Operand),
                projectile.PresentationOperandAddress, $"{at}: frame");
        }
        AssertEqual(shot, native.Deleted, $"Crocomire {label}: only the shot program deletes its projectile");
    }

    /// <summary>
    /// The spritemaps the typed body segments draw from the skeleton's frames when <paramref name="skeleton"/> holds, else from the fight body's; the body oracle matches each to the
    /// operand of its native frame.
    /// </summary>
    internal static IEnumerable<ushort> TypedBodySpritemaps(bool skeleton) =>
        Enum.GetValues<CrocomireBodySegment>()
            .SelectMany(segment => CrocomireBodyAnimations.Segments.Beats(segment))
            .Select(beat => beat.ShownFrame).OfType<EnemyFrameId>().Where(frame => CrocomireBodyFramesToolingConstants.IsSkeleton(frame) == skeleton)
            .Select(frame => CrocomireBodyFramesTooling.Spritemap(frame))
            .Distinct();

    /// <summary>Runs Crocomire's body program parity check against the repository cartridge.</summary>
    [VerificationCommand("--crocomire-instruction-mechanics")]
    internal static int CrocomireInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyCrocomireBodyAnimations), () => VerifyCrocomireBodyAnimations(LoadRepositoryRom()));
        return 0;
    }

    /// <summary>Runs Crocomire's tongue program parity check against the repository cartridge.</summary>
    [VerificationCommand("--crocomire-tongue-instruction-mechanics")]
    internal static int CrocomireTongueInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyCrocomireTongueAnimations), () => VerifyCrocomireTongueAnimations(LoadRepositoryRom()));
        return 0;
    }

    /// <summary>Runs Crocomire's projectile program parity check against the repository cartridge.</summary>
    [VerificationCommand("--crocomire-projectile-instruction-mechanics")]
    internal static int CrocomireProjectileInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyCrocomireProjectileAnimations), () => VerifyCrocomireProjectileAnimations(LoadRepositoryRom()));
        return 0;
    }
}
