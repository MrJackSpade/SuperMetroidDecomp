using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Bosses.Crocomire.CrocomireNativeListAddresses;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Enemies;
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
    /// Every body segment's beats equal the native list: frames by spritemap and duration, and callbacks as cues; every
    /// body animation follows the lists' gotos, sleeps and runs on into the list authored next. The real body runner then
    /// plays the skeleton collapse, a fight-AI redirect and the roar's resident-segment test beside the native list.
    /// </summary>
    internal static void VerifyCrocomireBodyAnimations(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        AssertEqual(Enum.GetValues<CrocomireBodySegment>().Length, BodyEntries.Length, "every typed body segment has a native entry");
        for (int index = 0; index < BodyEntries.Length; index++)
        {
            AssertEqual((CrocomireBodySegment)index, BodyEntries[index].Program,
                "the body programs are declared in the cartridge's authored order, which the fight AI compares");
        }
        AssertTrue(BodyEntriesAscend, "native body entries ascend");

        Dictionary<CrocomireBodySegment, NativeEnemyList<CrocomireBodySegment>> lists = NativeEnemyAnimationOracle.Lists(cartridge, BodyBankNumber,
            BodyListEntries, CrocomireBodyAnimations.Segments, CrocomireBodyFramesTooling.Spritemap,
            new NativeEnemyCueRules<CrocomireBodyCue>(
                word => BodyCallbacks.ContainsKey(word) ? 0 : null,
                (word, _, cue, at) => AssertEqual(BodyCallbacks[word], cue, $"{at}: cue")));
        NativeEnemyAnimationOracle.CompareAnimations(lists, CrocomireBodyAnimations.StartingAt);
        AssertTrue(lists.Values.All(list => list.Controls.All(control =>
                control.Kind is NativeEnemyControlKind.Goto or NativeEnemyControlKind.Sleep or NativeEnemyControlKind.RunOnIntoNext)),
            "Crocomire's body lists use no stops, counters or waits");
        HashSet<CrocomireBodySegment> runOn = NativeEnemyAnimationOracle.RunsOnIntoNextAfterLastBeat(lists);
        AssertTrue(runOn.SetEquals(CrocomireBodyAnimations.RunsOnInto.Keys), "the run-on segments are the lists that run on into the next after their last beat");
        foreach (CrocomireBodySegment segment in runOn)
        {
            AssertEqual(lists[segment].Controls.Single(control => control.Kind == NativeEnemyControlKind.RunOnIntoNext).Target, (CrocomireBodySegment?)CrocomireBodyAnimations.RunsOnInto[segment],
                $"{segment} runs on into the list authored next");
        }
        AssertEqual(83, TypedBodySpritemaps(CrocomireBodyFrameSet.Body).Concat(TypedBodySpritemaps(CrocomireBodyFrameSet.Skeleton)).Distinct().Count(),
            "the body segments draw all 50 body and 33 skeleton frames");

        VerifySkeletonCollapse(cartridge);
        VerifyFightAiRedirect();
        VerifyRoarResidentProgram();
        Console.WriteLine($"  Crocomire body animations: {lists.Count} segments and {lists.Values.Sum(list => list.BeatAddresses.Length)} beats match bank $A4, " +
            "and every animation its lists' control flow; the runner matches the skeleton collapse and fight-AI redirects.");
    }

    /// <summary>Each body segment's native entry, in authored order.</summary>
    private static NativeProgramEntry<CrocomireBodySegment>[] BodyListEntries =>
        [.. BodyEntries.Select(item => new NativeProgramEntry<CrocomireBodySegment>(item.Program, item.Entry))];

    /// <summary>Creates a Crocomire body with its encounter state in a fresh enemy system.</summary>
    private static NewCrocomireResult NewCrocomire()
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        body.Bind(EnemyDefinitionId.Crocomire, body.Definition);
        body.Position = body.Position.WithWhole(new(576, 144));
        var state = new CrocomireEnemyState(body);
        enemies.CrocomireBehavior.Access.Crocomire = state;
        enemies.CrocomireBehavior.Access.CrocomireDeath = new CrocomireDeathState();
        enemies.VerificationServices().ReadRandom = () => ushort.MaxValue;
        enemies.VerificationServices().NextRandom = () => ushort.MaxValue;
        return new NewCrocomireResult(enemies, state);
    }

    /// <summary>Starts <paramref name="program"/> on the body as a native list install does.</summary>
    private static void Start(CrocomireEnemyState state, CrocomireBodySegment program)
    {
        state.BodyPlayback = new AnimationPlayback<CrocomireBodySegment>(CrocomireBodyAnimations.StartingWith(program), AnimationPlace.StartOfAnimation);
        state.Body.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>Runs one body update through the production interpreter.</summary>
    private static void Pass(RoomEnemySystem enemies, CrocomireEnemyState state, SamusState? samus = null)
    {
        enemies.Audio.BeginFrame();
        EnemyAnimationPlayer.AdvanceAnimation(state.Body, samus, level: null, Position16<RoomPixels>.Zero, controllerInput: 0);
    }

    /// <summary>
    /// The skeleton falls apart and settles exactly as the native list does, update by update: timer, spritemap,
    /// new-frame flag and sounds. The death phase's test sees the stable skeleton as soon as the last collapse frame
    /// shows, because the native cursor then already points at the stable list.
    /// </summary>
    private static void VerifySkeletonCollapse(IImportCartridgeSource cartridge)
    {
        (RoomEnemySystem enemies, CrocomireEnemyState state) = NewCrocomire();
        Start(state, CrocomireBodySegment.SkeletonFallsApart);
        var native = new NativeEnemyListReader(BodyBankNumber, BodyEntry(CrocomireBodySegment.SkeletonFallsApart), 1, _ => 0);
        for (int update = 0; update < 200; update++)
        {
            Pass(enemies, state);
            List<ushort> callbacks = [.. native.Step(cartridge).Select(callback => callback.Word)];
            string at = $"Crocomire skeleton collapse update {update}";
            // The stable skeleton is a held frame (#1627), whose timer rests apart from the native list's longest frame.
            if (!SuperMetroid.Verification.Enemies.NativeEnemyProjectileParity.RestsOnHeldFrame(native.Timer, state.Body.AnimationClock.Hold))
                AssertEqual(native.Timer, state.Body.AnimationClock.Hold.RawValue, $"{at}: timer");
            AssertEqual(native.Spritemap, state.Body.SpritemapPointer, $"{at}: frame");
            AssertEqual(native.NewFrame, state.Body.AnimationClock.ShowsNewFrame, $"{at}: new-frame flag");
            AssertEqual(EnemyAudioQueueAccess.SoundList(callbacks.Select(BodySound).OfType<SoundEffectId>()), enemies.Audio.QueuedSoundList,
                $"{at}: every sound the native list queues, in its order");
            AssertEqual(HasReached(native, CrocomireBodySegment.SkeletonStable), state.BodyResidentAnimation >= CrocomireBodySegment.SkeletonStable,
                $"{at}: the stable-skeleton test reads the native cursor");
        }
        AssertEqual(CrocomireBodySegment.SkeletonStable, state.BodyResidentAnimation, "the collapse settles on the stable skeleton");
    }

    /// <summary>
    /// A fight-AI cue that starts another program shows that program's first frame in the same update, as the
    /// native instruction redirects the interpreter: Samus near the sleeping body starts the damage wait.
    /// </summary>
    private static void VerifyFightAiRedirect()
    {
        (RoomEnemySystem enemies, CrocomireEnemyState state) = NewCrocomire();
        state.Phase = CrocomirePhase.Sleeping;
        Start(state, CrocomireBodySegment.Initial);
        Pass(enemies, state);
        AssertEqual(CrocomireBodyFramesTooling.Spritemap(new CrocomireBodyFrame(CrocomireBodyFrameSet.Body, 12)), state.Body.SpritemapPointer,
            "the sleeping body shows its initial frame");
        AssertEqual((ushort)1, state.Body.AnimationClock.Hold.RawValue, "the initial frame lasts one update");

        var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState());
        samus.Position = samus.Position.WithWholeX(state.Body.Position.X.Whole);
        Pass(enemies, state, samus);
        AssertEqual(CrocomirePhase.WaitingForFirstDamage, state.Phase, "Samus within range wakes Crocomire");
        AssertEqual(CrocomireBodyFramesTooling.Spritemap(new CrocomireBodyFrame(CrocomireBodyFrameSet.Body, 18)), state.Body.SpritemapPointer,
            "the redirect shows the damage wait's first frame in the same update");
        AssertEqual((ushort)34, state.Body.AnimationClock.Hold.RawValue, "the damage wait's first frame lasts 34 updates");
        AssertEqual(CrocomireBodyAnimations.StartingWith(CrocomireBodySegment.WaitForFirstSecondDamage), state.BodyPlayback.Animation, "the damage wait animation plays");
    }

    /// <summary>
    /// While waiting for damage, the fight AI restarts the roar only once the cursor has reached the closing loop:
    /// the cues in the closing program continue, and the first cue of the loop starts the roar.
    /// </summary>
    private static void VerifyRoarResidentProgram()
    {
        (RoomEnemySystem enemies, CrocomireEnemyState state) = NewCrocomire();
        state.Phase = CrocomirePhase.WaitingForFirstDamage;
        Start(state, CrocomireBodySegment.RoarCloseMouth);
        CrocomireAnimationsExpected[] expected = [new CrocomireAnimationsExpected(26, 32), new CrocomireAnimationsExpected(25, 5), new CrocomireAnimationsExpected(25, 8), new CrocomireAnimationsExpected(24, 2), new CrocomireAnimationsExpected(24, 1), new CrocomireAnimationsExpected(24, 48)];
        for (int index = 0; index < expected.Length; index++)
        {
            state.Body.AnimationClock.StepOnNextUpdate();
            Pass(enemies, state);
            AssertEqual(CrocomireBodyFramesTooling.Spritemap(new CrocomireBodyFrame(CrocomireBodyFrameSet.Body, expected[index].Frame)), state.Body.SpritemapPointer,
                $"roar close frame {index}");
            AssertEqual((ushort)expected[index].Duration, state.Body.AnimationClock.Hold.RawValue, $"roar close duration {index}");
        }
        AssertEqual(CrocomireBodyAnimations.StartingWith(CrocomireBodySegment.Roar), state.BodyPlayback.Animation, "the closing loop's fight-AI cue restarts the roar");
    }

    /// <summary>Each tongue program plays its native list update by update through the production interpreter.</summary>
    internal static void VerifyCrocomireTongueAnimations(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        AssertEqual(Enum.GetValues<CrocomireTongueSegment>().Length, TongueEntries.Length, "every tongue program has a native entry");
        foreach ((CrocomireTongueSegment program, ushort entry) in TongueEntries)
        {
            (RoomEnemySystem enemies, CrocomireEnemyState state) = NewCrocomire();
            RoomEnemySlot tongue = enemies.SlotTable.Slots[1];
            tongue.Bind(EnemyDefinitionId.CrocomireTongue, tongue.Definition);
            state.Tongue = tongue;
            state.TonguePlayback = new AnimationPlayback<CrocomireTongueSegment>(CrocomireAnimations.TongueStartingWith(program), AnimationPlace.StartOfAnimation);
            tongue.AnimationClock.StepOnNextUpdate();
            var native = new NativeEnemyListReader(BodyBankNumber, entry, 1, _ => 0);
            for (int update = 0; update < 60; update++)
            {
                EnemyAnimationPlayer.AdvanceAnimation(tongue, samus: null, level: null, Position16<RoomPixels>.Zero, controllerInput: 0);
                List<NativeEnemyListDefinitions.Callback> callbacks = native.Step(cartridge);
                string at = $"Crocomire tongue {program} update {update}";
                AssertEqual(0, callbacks.Count, $"{at}: the tongue lists have no callbacks");
                AssertEqual(native.Timer, tongue.AnimationClock.Hold.RawValue, $"{at}: timer");
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
        mouthSystem.CrocomireBehavior.Fight.SpawnCrocomireProjectile(mouthState.Body, shot: 1);
        EnemyProjectile mouth = mouthSystem.Projectiles.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.CrocomireProjectile);
        CompareProjectile(cartridge, mouthSystem, mouth, RoomEnemyProjectileKind.CrocomireProjectile, shot: false, "mouth");

        (RoomEnemySystem bridgeSystem, _) = NewCrocomire();
        bridgeSystem.CrocomireBehavior.BridgeCollapse.SpawnNextCrocomireBridgeFragment();
        EnemyProjectile bridge = bridgeSystem.Projectiles.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.CrocomireBridgeCrumbling);
        CompareProjectile(cartridge, bridgeSystem, bridge, RoomEnemyProjectileKind.CrocomireBridgeCrumbling, shot: false, "bridge");

        (RoomEnemySystem spikeSystem, _) = NewCrocomire();
        spikeSystem.CrocomireBehavior.Skeleton.SpawnCrocomireSpikeWallPieces();
        EnemyProjectile[] spikes = [.. spikeSystem.Projectiles.EnemyProjectiles.Where(
            projectile => projectile.Kind == RoomEnemyProjectileKind.CrocomireSpikeWallPieces)];
        AssertEqual(8, spikes.Length, "the spike wall breaks into eight pieces");
        foreach (EnemyProjectile spike in spikes)
            CompareProjectile(cartridge, spikeSystem, spike, RoomEnemyProjectileKind.CrocomireSpikeWallPieces, shot: false, "spike");

        (RoomEnemySystem shotSystem, CrocomireEnemyState shotState) = NewCrocomire();
        shotSystem.DeathsAndDrops.BindSamus(new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Health = 99, MaxHealth = 99 });
        shotSystem.CrocomireBehavior.Fight.SpawnCrocomireProjectile(shotState.Body, shot: 1);
        EnemyProjectile exploding = shotSystem.Projectiles.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.CrocomireProjectile);
        exploding.StartAnimation(new CrocomireProjectileAnimation(CrocomireProjectileFrameKind.Shot));
        CompareProjectile(cartridge, shotSystem, exploding, RoomEnemyProjectileKind.CrocomireProjectile, shot: true, "shot");
        AssertTrue(!exploding.IsActive, "the shot program deletes the projectile");
        AssertTrue(shotSystem.Projectiles.EnemyProjectiles.Any(projectile => projectile.Kind == RoomEnemyProjectileKind.EnemyDeathPickup),
            "the shot program allocates a drop before deleting the projectile");

        AssertEqual(CrocomireProjectileVisualDefinitions.ProjectileFrameCount, CrocomireProjectileVisualDefinitions.ProjectileFrames.Count,
            "the projectile catalog lists every frame");
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
            enemies.Projectiles.AdvanceEnemyProjectileAnimations(projectile, null, Position16<RoomPixels>.Zero);
            native.Step(cartridge);
            string at = $"Crocomire {label} update {update}";
            AssertEqual(!native.Deleted, projectile.IsActive, $"{at}: deletion");
            AssertEqual(native.Drops,
                enemies.Projectiles.EnemyProjectiles.Count(item => item.Kind == RoomEnemyProjectileKind.EnemyDeathPickup) - dropsBefore,
                $"{at}: drops");
            if (native.Deleted)
                break;
            // The bridge fragment and spike-wall piece hold their still frame for good (#1627), whose timer rests apart
            // from the native list's longest frame.
            if (!SuperMetroid.Verification.Enemies.NativeEnemyProjectileParity.RestsOnHeldFrame(native.Timer, projectile.FrameHold))
                AssertEqual(native.Timer, projectile.FrameHold.RawValue, $"{at}: timer");
            AssertEqual(native.Operand, projectile.PresentationOperandAddress, $"{at}: frame");
        }
        AssertEqual(shot, native.Deleted, $"Crocomire {label}: only the shot program deletes its projectile");
    }

    /// <summary>
    /// The spritemaps the typed body segments draw from <paramref name="set"/>; the body oracle matches each to the
    /// operand of its native frame.
    /// </summary>
    internal static IEnumerable<ushort> TypedBodySpritemaps(CrocomireBodyFrameSet set) =>
        Enum.GetValues<CrocomireBodySegment>()
            .SelectMany(segment => CrocomireBodyAnimations.Segments.Beats(segment))
            .Select(beat => beat.ShownFrame).OfType<CrocomireBodyFrame>().Where(frame => frame.Set == set)
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
