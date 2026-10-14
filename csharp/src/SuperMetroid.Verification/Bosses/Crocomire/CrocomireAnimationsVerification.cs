using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using static SuperMetroid.Verification.Bosses.Crocomire.CrocomireNativeListAddresses;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
namespace SuperMetroid.Verification.Bosses.Crocomire;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;

/// <summary>
/// #1282: Crocomire's typed body, tongue and projectile programs against the retail cartridge. Minimal readers walk
/// the native bank-$A4 and bank-$86 lists the cartridge selects, and the typed programs must match them step by
/// step and update by update: timers, drawn frames, cues, jumps, fall-through and the new-frame flag.
/// </summary>
internal static class CrocomireAnimationsVerification
{
    /// <summary>
    /// Every body program's typed steps equal the native list: frames by spritemap and duration, callbacks as cues,
    /// gotos, sleeps, and the fall-through into the program authored next. The real body runner then plays the
    /// skeleton collapse, a fight-AI redirect and the roar's resident-program test beside the native list.
    /// </summary>
    internal static void VerifyCrocomireBodyAnimations(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        AssertEqual(Enum.GetValues<CrocomireBodyProgram>().Length, BodyEntries.Length, "every typed body program has a native entry");
        for (int index = 0; index < BodyEntries.Length; index++)
        {
            AssertEqual((CrocomireBodyProgram)index, BodyEntries[index].Program,
                "the body programs are declared in the cartridge's authored order, which the fight AI compares");
        }
        AssertTrue(BodyEntriesAscend, "native body entries ascend");

        var drawn = new HashSet<ushort>();
        int stepCount = 0;
        foreach ((CrocomireBodyProgram program, ushort entry) in BodyEntries)
            stepCount += CompareBodyProgram(cartridge, program, entry, drawn);
        AssertEqual(83, drawn.Count, "the body programs draw all 50 body and 33 skeleton frames");

        VerifySkeletonCollapse(cartridge);
        VerifyFightAiRedirect();
        VerifyRoarResidentProgram();
        Console.WriteLine($"  Crocomire body programs: 34 programs and {stepCount} steps match bank $A4; the runner matches the skeleton collapse and fight-AI redirects.");
    }

    /// <summary>Compares one typed body program with its native list and returns its step count.</summary>
    private static int CompareBodyProgram(IImportCartridgeSource cartridge, CrocomireBodyProgram program, ushort entry, HashSet<ushort> drawn)
    {
        var steps = CrocomireBodyAnimations.Programs.Steps(program);
        ushort cursor = entry;
        for (int index = 0; index < steps.Count; index++)
        {
            var step = steps[index];
            string at = $"Crocomire {program} step {index}";
            if (index > 0 && BodyProgramAt(cursor) is { } next)
            {
                AssertEqual(AnimationProgramStepKind.FallInto, step.Kind, $"{at}: runs on into the next authored program");
                AssertEqual(next, step.Target, $"{at}: fall-through target");
                AssertEqual(steps.Count, index + 1, $"{at}: fall-through ends the program");
                return steps.Count;
            }

            ushort word = cartridge.ReadCartridgeWord(BodyBank | cursor);
            switch (NativeEnemyListDefinitions.Classify(word))
            {
                case NativeEnemyListDefinitions.WordKind.Frame:
                {
                    AssertEqual(AnimationProgramStepKind.Show, step.Kind, $"{at}: frame");
                    AssertEqual((int)word, step.Duration, $"{at}: duration");
                    ushort spritemap = cartridge.ReadCartridgeWord(BodyBank | (cursor + 2));
                    AssertEqual(spritemap, step.Frame.Spritemap, $"{at}: spritemap");
                    drawn.Add(spritemap);
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                }
                case NativeEnemyListDefinitions.WordKind.GoTo:
                    AssertEqual(AnimationProgramStepKind.GoTo, step.Kind, $"{at}: goto");
                    AssertEqual(BodyProgramAt(cartridge.ReadCartridgeWord(BodyBank | (cursor + 2))), step.Target, $"{at}: goto target");
                    AssertEqual(steps.Count, index + 1, $"{at}: goto ends the program");
                    return steps.Count;
                case NativeEnemyListDefinitions.WordKind.Sleep:
                    AssertEqual(AnimationProgramStepKind.Hold, step.Kind, $"{at}: sleep");
                    AssertEqual(steps.Count, index + 1, $"{at}: sleep ends the program");
                    return steps.Count;
                case NativeEnemyListDefinitions.WordKind.Callback:
                    AssertTrue(BodyCallbacks.TryGetValue(word, out CrocomireBodyCue cue), $"{at}: native callback ${word:X4} is a known Crocomire callback");
                    AssertEqual(AnimationProgramStepKind.Cue, step.Kind, $"{at}: callback");
                    AssertEqual(cue, step.Cue, $"{at}: cue");
                    cursor = unchecked((ushort)(cursor + 2));
                    continue;
                case NativeEnemyListDefinitions.WordKind.Stop:
                case NativeEnemyListDefinitions.WordKind.SetCounter:
                case NativeEnemyListDefinitions.WordKind.RepeatWhileCounting:
                case NativeEnemyListDefinitions.WordKind.Wait:
                    throw new InvalidDataException($"{at}: Crocomire's body lists use no stops, counters or waits.");
                default:
                    throw new InvalidOperationException($"Undefined {nameof(NativeEnemyListDefinitions.WordKind)}.");
            }
        }
        throw new InvalidDataException($"Crocomire {program} ran past its typed steps.");
    }

    /// <summary>Creates a Crocomire body with its encounter state in a fresh enemy system.</summary>
    private static (RoomEnemySystem Enemies, CrocomireEnemyState State) NewCrocomire()
    {
        var enemies = new RoomEnemySystem();
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        body.EnemyDefinitionPointer = EnemyDefinitionId.Crocomire;
        body.Position = body.Position.WithWhole(new(576, 144));
        var state = new CrocomireEnemyState(body);
        enemies.CrocomireBehavior.Crocomire = state;
        enemies.CrocomireBehavior.CrocomireDeath = new CrocomireDeathState();
        enemies.VerificationServices().ReadRandom = (Func<ushort>)(() => ushort.MaxValue);
        enemies.VerificationServices().NextRandom = (Func<ushort>)(() => ushort.MaxValue);
        return (enemies, state);
    }

    /// <summary>Starts <paramref name="program"/> on the body as a native list install does.</summary>
    private static void Start(CrocomireEnemyState state, CrocomireBodyProgram program)
    {
        state.BodyCursor = AnimationProgramCursor<CrocomireBodyProgram>.Start(program);
        state.Body.InstructionTimer = NativeCountdown.Start(1);
    }

    /// <summary>Runs one body update through the production interpreter.</summary>
    private static void Pass(RoomEnemySystem enemies, CrocomireEnemyState state, SamusState? samus = null)
    {
        enemies.CrocomireBehavior.BeginEnemyFrame(new RoomEnemyFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false });
        enemies.Instructions.ProcessInstructions(state.Body, samus, level: null, Position16<RoomPixels>.Zero, controllerInput: 0);
    }

    /// <summary>
    /// The skeleton falls apart and settles exactly as the native list does, update by update: timer, spritemap,
    /// new-frame flag and sounds. The death phase's test sees the stable skeleton as soon as the last collapse frame
    /// shows, because the native cursor then already points at the stable list.
    /// </summary>
    private static void VerifySkeletonCollapse(IImportCartridgeSource cartridge)
    {
        (RoomEnemySystem enemies, CrocomireEnemyState state) = NewCrocomire();
        Start(state, CrocomireBodyProgram.SkeletonFallsApart);
        var native = new NativeEnemyListDefinitions.Reader(BodyBankNumber, BodyEntry(CrocomireBodyProgram.SkeletonFallsApart), 1, _ => 0);
        for (int update = 0; update < 200; update++)
        {
            Pass(enemies, state);
            List<ushort> callbacks = [.. native.Step(cartridge).Select(callback => callback.Word)];
            string at = $"Crocomire skeleton collapse update {update}";
            AssertEqual(native.Timer, state.Body.InstructionTimer.RawValue, $"{at}: timer");
            AssertEqual(native.Spritemap, state.Body.SpritemapPointer, $"{at}: frame");
            AssertEqual(native.NewFrame, state.Body.ExtraProperties.HasAny(EnemyExtraProperties.NewInstructionFrame), $"{at}: new-frame flag");
            AssertEqual(callbacks.Select(BodySound).LastOrDefault(sound => sound is not null), enemies.CrocomireBehavior.LastCrocomireSoundEffect,
                $"{at}: sound");
            AssertEqual(HasReached(native, CrocomireBodyProgram.SkeletonStable), state.BodyResidentProgram >= CrocomireBodyProgram.SkeletonStable,
                $"{at}: the stable-skeleton test reads the native cursor");
        }
        AssertEqual(CrocomireBodyProgram.SkeletonStable, state.BodyResidentProgram, "the collapse settles on the stable skeleton");
    }

    /// <summary>
    /// A fight-AI cue that starts another program shows that program's first frame in the same update, as the
    /// native instruction redirects the interpreter: Samus near the sleeping body starts the damage wait.
    /// </summary>
    private static void VerifyFightAiRedirect()
    {
        (RoomEnemySystem enemies, CrocomireEnemyState state) = NewCrocomire();
        state.FightFunction = CrocomireFightFunction.Sleeping;
        Start(state, CrocomireBodyProgram.Initial);
        Pass(enemies, state);
        AssertEqual(new CrocomireBodyFrame(CrocomireBodyFrameSet.Body, 12).Spritemap, state.Body.SpritemapPointer,
            "the sleeping body shows its initial frame");
        AssertEqual((ushort)1, state.Body.InstructionTimer.RawValue, "the initial frame lasts one update");

        var samus = new SamusState();
        samus.Position = samus.Position.WithWholeX(state.Body.Position.X.Whole);
        Pass(enemies, state, samus);
        AssertEqual(CrocomireFightFunction.WaitingForFirstDamage, state.FightFunction, "Samus within range wakes Crocomire");
        AssertTrue((state.FightFlags & CrocomireFightFlags.Engaged) != 0, "waking engages Samus");
        AssertEqual(new CrocomireBodyFrame(CrocomireBodyFrameSet.Body, 18).Spritemap, state.Body.SpritemapPointer,
            "the redirect shows the damage wait's first frame in the same update");
        AssertEqual((ushort)34, state.Body.InstructionTimer.RawValue, "the damage wait's first frame lasts 34 updates");
        AssertEqual(CrocomireBodyProgram.WaitForFirstSecondDamage, state.BodyCursor.Program, "the damage wait program runs");
    }

    /// <summary>
    /// While waiting for damage, the fight AI restarts the roar only once the cursor has reached the closing loop:
    /// the cues in the closing program continue, and the first cue of the loop starts the roar.
    /// </summary>
    private static void VerifyRoarResidentProgram()
    {
        (RoomEnemySystem enemies, CrocomireEnemyState state) = NewCrocomire();
        state.FightFunction = CrocomireFightFunction.WaitingForFirstDamage;
        Start(state, CrocomireBodyProgram.RoarCloseMouth);
        (int Frame, int Duration)[] expected = [(26, 32), (25, 5), (25, 8), (24, 2), (24, 1), (24, 48)];
        for (int index = 0; index < expected.Length; index++)
        {
            state.Body.InstructionTimer = NativeCountdown.Start(1);
            Pass(enemies, state);
            AssertEqual(new CrocomireBodyFrame(CrocomireBodyFrameSet.Body, expected[index].Frame).Spritemap, state.Body.SpritemapPointer,
                $"roar close frame {index}");
            AssertEqual((ushort)expected[index].Duration, state.Body.InstructionTimer.RawValue, $"roar close duration {index}");
        }
        AssertEqual(CrocomireBodyProgram.Roar, state.BodyCursor.Program, "the closing loop's fight-AI cue restarts the roar");
    }

    /// <summary>Each tongue program plays its native list update by update through the production interpreter.</summary>
    internal static void VerifyCrocomireTongueAnimations(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        AssertEqual(Enum.GetValues<CrocomireTongueProgram>().Length, TongueEntries.Length, "every tongue program has a native entry");
        foreach ((CrocomireTongueProgram program, ushort entry) in TongueEntries)
        {
            (RoomEnemySystem enemies, CrocomireEnemyState state) = NewCrocomire();
            RoomEnemySlot tongue = enemies.SlotTable.Slots[1];
            tongue.EnemyDefinitionPointer = EnemyDefinitionId.CrocomireTongue;
            state.Tongue = tongue;
            state.TongueProgram = program;
            state.TonguePosition = 0;
            tongue.InstructionTimer = NativeCountdown.Start(1);
            var native = new NativeEnemyListDefinitions.Reader(BodyBankNumber, entry, 1, _ => 0);
            for (int update = 0; update < 60; update++)
            {
                enemies.Instructions.ProcessInstructions(tongue, samus: null, level: null, Position16<RoomPixels>.Zero, controllerInput: 0);
                List<NativeEnemyListDefinitions.Callback> callbacks = native.Step(cartridge);
                string at = $"Crocomire tongue {program} update {update}";
                AssertEqual(0, callbacks.Count, $"{at}: the tongue lists have no callbacks");
                AssertEqual(native.Timer, tongue.InstructionTimer.RawValue, $"{at}: timer");
                AssertEqual(native.Spritemap, tongue.SpritemapPointer, $"{at}: frame");
                AssertEqual(native.NewFrame, tongue.ExtraProperties.HasAny(EnemyExtraProperties.NewInstructionFrame), $"{at}: new-frame flag");
            }
        }
        Console.WriteLine("  Crocomire tongue programs: fight, sleep and melting match bank $A4 every update.");
    }

    /// <summary>
    /// The real mouth, bridge and spike producers start typed programs that match their native lists, and the
    /// shot program explodes, drops with Crocomire's chances and deletes on the native updates.
    /// </summary>
    internal static void VerifyCrocomireProjectileAnimations(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);

        (RoomEnemySystem mouthSystem, CrocomireEnemyState mouthState) = NewCrocomire();
        mouthSystem.CrocomireBehavior.SpawnCrocomireProjectile(mouthState.Body, 2);
        RoomEnemyProjectileSlot mouth = mouthSystem.Projectiles.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.CrocomireProjectile);
        CompareProjectile(cartridge, mouthSystem, mouth, RoomEnemyProjectileKind.CrocomireProjectile, shot: false, "mouth");

        (RoomEnemySystem bridgeSystem, _) = NewCrocomire();
        bridgeSystem.CrocomireBehavior.SpawnNextCrocomireBridgeFragment();
        RoomEnemyProjectileSlot bridge = bridgeSystem.Projectiles.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.CrocomireBridgeCrumbling);
        CompareProjectile(cartridge, bridgeSystem, bridge, RoomEnemyProjectileKind.CrocomireBridgeCrumbling, shot: false, "bridge");

        (RoomEnemySystem spikeSystem, _) = NewCrocomire();
        spikeSystem.CrocomireBehavior.SpawnCrocomireSpikeWallPieces();
        RoomEnemyProjectileSlot[] spikes = [.. spikeSystem.Projectiles.EnemyProjectiles.Where(
            projectile => projectile.Kind == RoomEnemyProjectileKind.CrocomireSpikeWallPieces)];
        AssertEqual(8, spikes.Length, "the spike wall breaks into eight pieces");
        foreach (RoomEnemyProjectileSlot spike in spikes)
            CompareProjectile(cartridge, spikeSystem, spike, RoomEnemyProjectileKind.CrocomireSpikeWallPieces, shot: false, "spike");

        (RoomEnemySystem shotSystem, CrocomireEnemyState shotState) = NewCrocomire();
        shotSystem.BindFixtureBus(rom);
        shotSystem.DeathsAndDrops._samusForEnemyDrops = new SamusState { Health = 99, MaxHealth = 99 };
        shotSystem.CrocomireBehavior.SpawnCrocomireProjectile(shotState.Body, 2);
        RoomEnemyProjectileSlot exploding = shotSystem.Projectiles.EnemyProjectiles.Single(
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

    /// <summary>Runs a typed projectile beside the native list of its kind for fifty updates and compares every observable.</summary>
    private static void CompareProjectile(IImportCartridgeSource cartridge, RoomEnemySystem enemies, RoomEnemyProjectileSlot projectile,
        RoomEnemyProjectileKind kind, bool shot, string label)
    {
        AssertTrue(projectile.Animation is CrocomireProjectileAnimation, $"Crocomire {label} runs a typed program");
        AssertEqual((ushort)0, projectile.InstructionPointer, $"Crocomire {label} keeps no native list");
        EnemyProjectileDefinition definition = EnemyProjectileDefinitionCatalog.Get(kind);
        var native = new NativeProjectileList(shot ? definition.ShotInstructionList : definition.InitialInstructionList,
            projectile.InstructionTimer.RawValue);
        int dropsBefore = enemies.Projectiles.EnemyProjectiles.Count(item => item.Kind == RoomEnemyProjectileKind.EnemyDeathPickup);
        for (int update = 0; update < 50 && !native.Deleted; update++)
        {
            enemies.Projectiles.ProcessEnemyProjectileInstructions(projectile, null, Position16<RoomPixels>.Zero);
            native.Step(cartridge);
            string at = $"Crocomire {label} update {update}";
            AssertEqual(!native.Deleted, projectile.IsActive, $"{at}: deletion");
            AssertEqual(native.Drops,
                enemies.Projectiles.EnemyProjectiles.Count(item => item.Kind == RoomEnemyProjectileKind.EnemyDeathPickup) - dropsBefore,
                $"{at}: drops");
            if (native.Deleted)
                break;
            AssertEqual(native.Timer, projectile.InstructionTimer.RawValue, $"{at}: timer");
            AssertEqual(native.Operand, projectile.PresentationOperandAddress, $"{at}: frame");
        }
        AssertEqual(shot, native.Deleted, $"Crocomire {label}: only the shot program deletes its projectile");
    }

    /// <summary>
    /// The spritemaps the typed body programs draw from <paramref name="set"/>; the body oracle matches each to the
    /// operand of its native frame.
    /// </summary>
    internal static IEnumerable<ushort> TypedBodySpritemaps(CrocomireBodyFrameSet set) =>
        Enum.GetValues<CrocomireBodyProgram>()
            .SelectMany(program => CrocomireBodyAnimations.Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show && step.Frame.Set == set)
            .Select(step => step.Frame.Spritemap)
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
