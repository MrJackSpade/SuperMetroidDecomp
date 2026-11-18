using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Bosses.Torizo.TorizoNativeListAddresses;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>
/// #1289, #1788: Bomb and Golden Torizo's projectile animations against the retail cartridge. Each animation's phases must
/// follow the native bank-$86 lists' control flow from the list it starts at, and each of its steps must match the native
/// beat its phase plays there: every frame's duration and installed artwork key, every command's cue and operands, every
/// branch's target animation and the loop mark's own list; every native list must be played by some animation. The installed
/// frame catalog, the drool's delay table and the egg's resumed sleeps are checked against the cartridge too.
/// </summary>
internal static class TorizoProjectileAnimationsVerification
{
    /// <summary>What a native Torizo projectile command becomes.</summary>
    internal enum ProjectileWord
    {
        /// <summary>The sleep: a hold.</summary>
        Sleep,
        /// <summary>The goto: a jump.</summary>
        GoTo,
        /// <summary>The deletion: a delete cue and a hold.</summary>
        Delete,
        /// <summary>The egg's hatch: a hatch cue and a hold.</summary>
        Hatch,
        /// <summary>The general-timer load.</summary>
        SetCounter,
        /// <summary>The general-timer countdown.</summary>
        RepeatWhileCounting,
        /// <summary>A command that becomes one cue.</summary>
        Cue,
    }

    /// <summary>The motion each cue that starts one starts, which the native list names by its movement routine (#1843).</summary>
    private static readonly Dictionary<TorizoProjectileCueKind, TorizoProjectileMotion> StartedMotion = new()
    {
        [TorizoProjectileCueKind.StartDroolFall] = TorizoProjectileMotion.DroolFalling,
        [TorizoProjectileCueKind.StartFragmentFall] = TorizoProjectileMotion.StatueFragmentFalling,
        [TorizoProjectileCueKind.StartEggCharge] = TorizoProjectileMotion.EggCharging,
        [TorizoProjectileCueKind.ThrowSuperMissile] = TorizoProjectileMotion.SuperMissileThrown,
    };

    /// <summary>
    /// The behaviours and depth each cue a property OR becomes turns on, as the cartridge's OR operand stores them: the cue
    /// names what the bits did.
    /// </summary>
    private static readonly Dictionary<TorizoProjectileCueKind, (EnemyProjectilePropertyFlags Properties, EnemyProjectileDepth? Depth)> PropertiesSet = new()
    {
        [TorizoProjectileCueKind.StopHarmingSamusAndDrawInFront] = (EnemyProjectilePropertyFlags.HarmlessToSamus, EnemyProjectileDepth.InFrontOfSamus),
        [TorizoProjectileCueKind.PersistOnContactAndDrawInFront] = (EnemyProjectilePropertyFlags.PersistsOnContact, EnemyProjectileDepth.InFrontOfSamus),
        [TorizoProjectileCueKind.BlockShots] = (EnemyProjectilePropertyFlags.BlocksShots, null),
    };

    /// <summary>
    /// The behaviours each cue a property AND becomes turns off, and whether it moves the projectile behind Samus, as the
    /// cartridge's AND mask clears them.
    /// </summary>
    private static readonly Dictionary<TorizoProjectileCueKind, (EnemyProjectilePropertyFlags Properties, bool MovesBehind)> PropertiesCleared = new()
    {
        [TorizoProjectileCueKind.DrawBehindSamus] = (EnemyProjectilePropertyFlags.None, true),
        [TorizoProjectileCueKind.TurnHarmful] = (EnemyProjectilePropertyFlags.HarmlessToSamus, false),
        [TorizoProjectileCueKind.TurnHarmfulAndStopBlockingShots] = (EnemyProjectilePropertyFlags.HarmlessToSamus | EnemyProjectilePropertyFlags.BlocksShots, false),
    };

    /// <summary>Each native command Torizo's lists use: what it becomes and, for a cue, which cue.</summary>
    private static readonly Dictionary<NativeEnemyProjectileInstruction, TorizoProjectileAnimationsWord> Words = new()
    {
        [NativeEnemyProjectileInstruction.Sleep] = new TorizoProjectileAnimationsWord(ProjectileWord.Sleep, null),
        [NativeEnemyProjectileInstruction.GotoY] = new TorizoProjectileAnimationsWord(ProjectileWord.GoTo, null),
        [NativeEnemyProjectileInstruction.Delete] = new TorizoProjectileAnimationsWord(ProjectileWord.Delete, NativeCueShape<TorizoProjectileCue>.Exactly(new TorizoProjectileCue.Signal(TorizoProjectileCueKind.Delete))),
        [NativeEnemyProjectileInstruction.GoldenTorizoEgg_GoToHatched] = new TorizoProjectileAnimationsWord(ProjectileWord.Hatch, NativeCueShape<TorizoProjectileCue>.Exactly(new TorizoProjectileCue.Signal(TorizoProjectileCueKind.Hatch))),
        [NativeEnemyProjectileInstruction.TimerInY] = new TorizoProjectileAnimationsWord(ProjectileWord.SetCounter, null),
        [NativeEnemyProjectileInstruction.DecrementTimer_GotoYIfNonZero] = new TorizoProjectileAnimationsWord(ProjectileWord.RepeatWhileCounting, null),
        [NativeEnemyProjectileInstruction.ClearFrameBehavior] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Exactly(new TorizoProjectileCue.Signal(TorizoProjectileCueKind.StopMoving))),
        [NativeEnemyProjectileInstruction.FrameBehaviorInY] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Of<TorizoProjectileCue.Signal>("")),
        [NativeEnemyProjectileInstruction.Properties_OrY] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Of<TorizoProjectileCue.Signal>("", "a cue the property OR names", signal => PropertiesSet.ContainsKey(signal.Kind))),
        [NativeEnemyProjectileInstruction.Properties_AndY] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Of<TorizoProjectileCue.Signal>("", "a cue the property AND names", signal => PropertiesCleared.ContainsKey(signal.Kind))),
        [NativeEnemyProjectileInstruction.QueueSoundInY_Lib2_Max6] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Of<TorizoProjectileCue.PlaySound>("")),
        [NativeEnemyProjectileInstruction.QueueSoundInY_Lib3_Max6] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Of<TorizoProjectileCue.PlaySound>("")),
        [NativeEnemyProjectileInstruction.QueueSoundInY_Lib2_Max1] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Of<TorizoProjectileCue.PlaySound>("")),
        [NativeEnemyProjectileInstruction.GotoYIfEyeBeamExplosionsDisabled] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Exactly(new TorizoProjectileCue.Signal(TorizoProjectileCueKind.FloorImpactWaited))),
        [NativeEnemyProjectileInstruction.GotoY_Probability_1_4] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Exactly(new TorizoProjectileCue.Signal(TorizoProjectileCueKind.MayTurnToSmoke))),
        [NativeEnemyProjectileInstruction.AimSuperMissile_Rightwards] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Exactly(new TorizoProjectileCue.Signal(TorizoProjectileCueKind.AimSuperMissileTowardSamus))),
        [NativeEnemyProjectileInstruction.AimSuperMissile_Leftwards] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Exactly(new TorizoProjectileCue.Signal(TorizoProjectileCueKind.AimSuperMissileAwayFromSamus))),
        [NativeEnemyProjectileInstruction.XYRadiusInY] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Of<TorizoProjectileCue.SetRadius>("")),
        [NativeEnemyProjectileInstruction.SpawnEnemyDropsWIthYDropChances] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Exactly(new TorizoProjectileCue.Signal(TorizoProjectileCueKind.SpawnOrbDrops))),
        [NativeEnemyProjectileInstruction.MoveRandomlyWithinXRadius_YRadius] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Of<TorizoProjectileCue.Scatter>("")),
        [NativeEnemyProjectileInstruction.Torizo_ResetPosition] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Exactly(new TorizoProjectileCue.Signal(TorizoProjectileCueKind.ReturnToOrigin))),
        [NativeEnemyProjectileInstruction.TorizoLandingDustClouds] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Exactly(new TorizoProjectileCue.Signal(TorizoProjectileCueKind.RaiseDust))),
        [NativeEnemyProjectileInstruction.DisableCollisionWIthSamusProj] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Exactly(new TorizoProjectileCue.Signal(TorizoProjectileCueKind.StopBlockingShots))),
        [NativeEnemyProjectileInstruction.DisableCollisionWithSamus] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Exactly(new TorizoProjectileCue.Signal(TorizoProjectileCueKind.StopHarmingSamus))),
        [NativeEnemyProjectileInstruction.SetHighPriority] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Exactly(new TorizoProjectileCue.Signal(TorizoProjectileCueKind.DrawInFront))),
    };

    /// <summary>The library and queue limit of each sound command Torizo's lists use; its one operand byte is the sound.</summary>
    private static readonly Dictionary<NativeEnemyProjectileInstruction, SoundCommand> SoundCommands = new()
    {
        [NativeEnemyProjectileInstruction.QueueSoundInY_Lib2_Max6] = new SoundCommand(SoundEffectLibrary.Enemies, SoundRequestPriority.Normal),
        [NativeEnemyProjectileInstruction.QueueSoundInY_Lib3_Max6] = new SoundCommand(SoundEffectLibrary.Room, SoundRequestPriority.Normal),
        [NativeEnemyProjectileInstruction.QueueSoundInY_Lib2_Max1] = new SoundCommand(SoundEffectLibrary.Enemies, SoundRequestPriority.WhenIdle),
    };

    /// <summary>The operand byte count of each native command that becomes a cue; the rest carry none.</summary>
    private static readonly Dictionary<NativeEnemyProjectileInstruction, int> OperandByteCounts = new()
    {
        [NativeEnemyProjectileInstruction.QueueSoundInY_Lib2_Max6] = 1,
        [NativeEnemyProjectileInstruction.QueueSoundInY_Lib3_Max6] = 1,
        [NativeEnemyProjectileInstruction.QueueSoundInY_Lib2_Max1] = 1,
        [NativeEnemyProjectileInstruction.FrameBehaviorInY] = 2,
        [NativeEnemyProjectileInstruction.Properties_OrY] = 2,
        [NativeEnemyProjectileInstruction.Properties_AndY] = 2,
        [NativeEnemyProjectileInstruction.GotoYIfEyeBeamExplosionsDisabled] = 2,
        [NativeEnemyProjectileInstruction.GotoY_Probability_1_4] = 2,
        [NativeEnemyProjectileInstruction.XYRadiusInY] = 2,
        [NativeEnemyProjectileInstruction.SpawnEnemyDropsWIthYDropChances] = 4,
        [NativeEnemyProjectileInstruction.MoveRandomlyWithinXRadius_YRadius] = 4,
    };

    /// <summary>
    /// How the projectile oracle reads Torizo's commands: each becomes the cue its <see cref="Words"/> entry names, with
    /// its operands compared field by field; a deletion and the egg's hatch end their lists, and the eye beam's conditional
    /// goto to its own list is the mark its floor impact's wait loops on (#2411).
    /// </summary>
    private static NativeEnemyProjectileCueRules<TorizoProjectileCue> CueRules => new(
        word => Words.TryGetValue((NativeEnemyProjectileInstruction)word, out var meaning) && meaning.Cue is not null
            ? OperandByteCounts.GetValueOrDefault((NativeEnemyProjectileInstruction)word)
            : null,
        (word, operands, cue, at) =>
        {
            var instruction = (NativeEnemyProjectileInstruction)word;
            (Words[instruction].Cue ?? throw new InvalidOperationException($"{at}: a cue command names its cue.")).Check(cue, at);
            CompareOperands(instruction, operands, cue, at);
        },
        EndsList: cue => cue is TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.Delete or TorizoProjectileCueKind.Hatch },
        LoopMarks: cue => cue is TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.FloorImpactWaited },
        // The sonic boom's wall impact calls a routine that returned at once before its deletion: it has no event (#1870).
        Omitted: word => (NativeEnemyProjectileInstruction)word == NativeEnemyProjectileInstruction.RTS_8681DE,
        // The drool, statue fragments, orb, hatchlings, Super Missile and eye beam show poses again from operands of their
        // own, each the pose's one installed frame (#2234).
        PicturedAt: EnemyProjectileProgramFrameOperands.OwnOperand);

    /// <summary>Every projectile animation matches the native lists it plays, and every native list is played by some animation.</summary>
    internal static void VerifyTorizoProjectileAnimations(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        int steps = NativeEnemyProjectileProgramOracle.CompareByEntry(cartridge, ProjectileLists, ProjectileAnimationEntries,
            TorizoProjectileAnimations.Segments, TorizoProjectileAnimations.StartingAt,
            EnemyProjectileProgramFrameSources.Operand, CueRules, named: NamedProjectileAnimationEntries);

        int frames = 0;
        foreach (TorizoProjectileFamily family in Enum.GetValues<TorizoProjectileFamily>())
        {
            IReadOnlyList<EnemyProjectileFrameId> installed = TorizoProjectileAnimations.InstalledFrames(family);
            var shown = Enum.GetValues<TorizoProjectileSegment>()
                .Where(animation => TorizoProjectileAnimationsConstants.Family(animation) == family)
                .SelectMany(animation => TorizoProjectileAnimations.Segments.Beats(animation))
                .Select(beat => beat.ShownFrame).OfType<EnemyProjectileFrameId>().ToHashSet();
            AssertTrue(shown.SetEquals(installed), $"the {family} animations show exactly the installed frames");
            ushort[] keys = [.. installed.Select(EnemyProjectileProgramFrameSources.Operand)];
            AssertTrue(keys.Zip(keys.Skip(1), (first, second) => first < second).All(ascends => ascends), $"the {family} frames install in native operand order");
            frames += installed.Count;
        }

        foreach ((int index, int word) in DroolDelayListTable)
        {
            ushort random = (ushort)(index << 2);
            AssertEqual(cartridge.ReadCartridgeWord(word),
                ProjectileEntryOf(TorizoAttacks.SelectLowHealthDroolAnimation(random)), $"low-health drool delay table entry {index}");
        }

        // The egg's hatch starts the hatching animation of the side the egg was laid facing; the cartridge reaches the same
        // animation by stepping the rolling egg's list past its sleep.
        foreach ((TorizoProjectileSegment rolling, TorizoProjectileSegment hatching) in new[]
            {
                (TorizoProjectileSegment.EggBouncingLeft, TorizoProjectileSegment.EggHatchingLeft),
                (TorizoProjectileSegment.EggBouncingRight, TorizoProjectileSegment.EggHatchingRight),
            })
        {
            AssertEqual(AnimationEnd.Holds, TorizoProjectileAnimations.StartingWith(rolling).End, $"{rolling} holds");
            ushort sleep = unchecked((ushort)(ProjectileEntry(rolling) + 4));
            AssertEqual(ProjectileWord.Sleep, Words[(NativeEnemyProjectileInstruction)cartridge.ReadCartridgeWord(NativeEnemyProjectileListDefinitions.ProjectileBank | sleep)].Word,
                $"{rolling}'s sleep follows its frame");
            AssertEqual(ProjectileAnimationAt(unchecked((ushort)(sleep + 2))), hatching,
                $"{rolling} hatches into {hatching}");
        }
        Console.WriteLine($"  Torizo projectile animations: {ProjectileAnimationEntries.Count} animations ({steps} steps, {frames} installed frames) play all {ProjectileLists.Length} bank-$86 lists as the cartridge does.");
    }

    /// <summary>Compares a command's operand bytes with its cue's typed fields.</summary>
    private static void CompareOperands(NativeEnemyProjectileInstruction instruction, byte[] operands, TorizoProjectileCue cue, string at)
    {
        ushort Word(int offset) => LittleEndianBytes.Word(operands[offset], operands[offset + 1]);
        if (SoundCommands.TryGetValue(instruction, out var sound))
        {
            var playSound = AssertIs<TorizoProjectileCue.PlaySound>(cue, $"{at}: cue");
            AssertEqual(SoundEffectCommands.Decode(sound.Library, operands[0]), playSound.Sound, $"{at}: sound");
            AssertEqual(sound.Priority, playSound.Priority, $"{at}: sound queue limit");
            return;
        }
        switch (cue)
        {
            case TorizoProjectileCue.Signal signal when StartedMotion.TryGetValue(signal.Kind, out TorizoProjectileMotion started):
                // Setting a movement routine is the cue that starts the motion that routine stands for.
                AssertEqual<Enum?>(started, EnemyProjectileMotionAccess.FromWord(Word(0)), $"{at}: movement routine");
                return;
            case TorizoProjectileCue.Signal signal when PropertiesSet.TryGetValue(signal.Kind, out var set):
                // The behaviours the cue turns on, encoded by the import's property-word bits, are the OR operand the
                // cartridge stores; setting the depth bit moves the projectile in front of Samus.
                AssertEqual(Word(0), unchecked((ushort)(EnemyProjectilePropertyWords.ToNativeWord(set.Properties) |
                    (set.Depth is { } setDepth ? EnemyProjectilePropertyWords.ToNativeWord(setDepth) : 0))), $"{at}: property bits set");
                return;
            case TorizoProjectileCue.Signal signal when PropertiesCleared.TryGetValue(signal.Kind, out var cleared):
                // The behaviours the cue turns off, encoded by the import's property-word bits, are the bits the cartridge's
                // AND mask clears; clearing the depth bit moves the projectile behind Samus.
                AssertEqual(Word(0), unchecked((ushort)~(EnemyProjectilePropertyWords.ToNativeWord(cleared.Properties) |
                    (cleared.MovesBehind ? EnemyProjectilePropertyWords.ToNativeWord(EnemyProjectileDepth.InFrontOfSamus) : 0))), $"{at}: property bits kept");
                return;
            case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.FloorImpactWaited }:
                // The mark's operand is the entry of its own run's list, which the oracle checks (CueRules.LoopMarks).
                AssertEqual(2, operands.Length, $"{at}: the looped list's operand");
                return;
            case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.MayTurnToSmoke }:
                // The native branch names the smoke the owner turns the explosion into (#2167).
                AssertEqual(Word(0), ProjectileEntry(TorizoProjectileSegment.DeathSmoke), $"{at}: branch target");
                return;
            case TorizoProjectileCue.SetRadius setRadius:
                AssertEqual(new Extent16<RoomPixels>(operands[0], operands[1]), setRadius.Radius, $"{at}: radius");
                return;
            case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.SpawnOrbDrops }:
                AssertEqual(TorizoDeathDropsVerification.BombTorizoOrbHeader, (int)Word(0), $"{at}: Bomb Torizo orb header");
                AssertEqual(TorizoDeathDropsVerification.GoldenTorizoOrbHeader, (int)Word(2), $"{at}: Golden Torizo orb header");
                return;
            case TorizoProjectileCue.Scatter scatter:
                // Each range field reads exactly the bits of its native mask operand.
                AssertEqual<int>(operands[0], (scatter.Spread.XRange.Outcomes - 1) << scatter.Spread.XRange.Shift, $"{at}: scatter X range");
                AssertEqual<int>(operands[2], (scatter.Spread.YRange.Outcomes - 1) << scatter.Spread.YRange.Shift, $"{at}: scatter Y range");
                AssertEqual(new Extent16<RoomPixels>(operands[1], operands[3]), scatter.Spread.Center, $"{at}: scatter center");
                return;
            case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.StopMoving or TorizoProjectileCueKind.AimSuperMissileTowardSamus or
                TorizoProjectileCueKind.AimSuperMissileAwayFromSamus or TorizoProjectileCueKind.ReturnToOrigin or TorizoProjectileCueKind.RaiseDust or
                TorizoProjectileCueKind.StopBlockingShots or TorizoProjectileCueKind.StopHarmingSamus or TorizoProjectileCueKind.DrawInFront or
                TorizoProjectileCueKind.Delete or TorizoProjectileCueKind.Hatch }:
                AssertEqual(0, operands.Length, $"{at}: no operands");
                return;
            case TorizoProjectileCue.PlaySound:
                throw new InvalidDataException($"{at}: {cue} is not an operand command here.");
            default:
                throw new InvalidOperationException($"Undefined Torizo projectile cue {cue}.");
        }
    }
}
