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
/// #1289, #1788, #2554: Bomb and Golden Torizo's projectile animations against the retail cartridge, one animation family
/// at a time. Each animation's phases must follow the native bank-$86 lists' control flow from the list it starts at, and
/// each of its steps must match the native beat its phase plays there: every frame's duration and installed artwork key,
/// every command's cue and operands, every branch's target animation and the loop mark's own list; every native list of the
/// family must be played by some animation. The installed frame catalog, the drool's delay table and the egg's resumed
/// sleeps are checked against the cartridge too.
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
        /// <summary>A command that becomes one cue every family runs alike.</summary>
        Cue,
        /// <summary>A command that becomes one of a family's own cues.</summary>
        OwnCue,
    }

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

    /// <summary>Each native command Torizo's lists use: what it becomes and, for a cue every family runs alike, which cue.</summary>
    private static readonly Dictionary<NativeEnemyProjectileInstruction, TorizoProjectileAnimationsWord> Words = new()
    {
        [NativeEnemyProjectileInstruction.Sleep] = new TorizoProjectileAnimationsWord(ProjectileWord.Sleep, null),
        [NativeEnemyProjectileInstruction.GotoY] = new TorizoProjectileAnimationsWord(ProjectileWord.GoTo, null),
        [NativeEnemyProjectileInstruction.Delete] = new TorizoProjectileAnimationsWord(ProjectileWord.Delete, NativeCueShape<TorizoProjectileCue>.Exactly(new TorizoProjectileCue.Signal(TorizoProjectileCueKind.Delete))),
        [NativeEnemyProjectileInstruction.GoldenTorizoEgg_GoToHatched] = new TorizoProjectileAnimationsWord(ProjectileWord.Hatch, null),
        [NativeEnemyProjectileInstruction.TimerInY] = new TorizoProjectileAnimationsWord(ProjectileWord.SetCounter, null),
        [NativeEnemyProjectileInstruction.DecrementTimer_GotoYIfNonZero] = new TorizoProjectileAnimationsWord(ProjectileWord.RepeatWhileCounting, null),
        [NativeEnemyProjectileInstruction.ClearFrameBehavior] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Exactly(new TorizoProjectileCue.Signal(TorizoProjectileCueKind.StopMoving))),
        [NativeEnemyProjectileInstruction.FrameBehaviorInY] = new TorizoProjectileAnimationsWord(ProjectileWord.OwnCue, null),
        [NativeEnemyProjectileInstruction.Properties_OrY] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Of<TorizoProjectileCue.Signal>("", "a cue the property OR names", signal => PropertiesSet.ContainsKey(signal.Kind))),
        [NativeEnemyProjectileInstruction.Properties_AndY] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Of<TorizoProjectileCue.Signal>("", "a cue the property AND names", signal => PropertiesCleared.ContainsKey(signal.Kind))),
        [NativeEnemyProjectileInstruction.QueueSoundInY_Lib2_Max6] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Of<TorizoProjectileCue.PlaySound>("")),
        [NativeEnemyProjectileInstruction.QueueSoundInY_Lib3_Max6] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Of<TorizoProjectileCue.PlaySound>("")),
        [NativeEnemyProjectileInstruction.QueueSoundInY_Lib2_Max1] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Of<TorizoProjectileCue.PlaySound>("")),
        [NativeEnemyProjectileInstruction.GotoYIfEyeBeamExplosionsDisabled] = new TorizoProjectileAnimationsWord(ProjectileWord.OwnCue, null),
        [NativeEnemyProjectileInstruction.GotoY_Probability_1_4] = new TorizoProjectileAnimationsWord(ProjectileWord.OwnCue, null),
        [NativeEnemyProjectileInstruction.AimSuperMissile_Rightwards] = new TorizoProjectileAnimationsWord(ProjectileWord.OwnCue, null),
        [NativeEnemyProjectileInstruction.AimSuperMissile_Leftwards] = new TorizoProjectileAnimationsWord(ProjectileWord.OwnCue, null),
        [NativeEnemyProjectileInstruction.XYRadiusInY] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Of<TorizoProjectileCue.SetRadius>("")),
        [NativeEnemyProjectileInstruction.SpawnEnemyDropsWIthYDropChances] = new TorizoProjectileAnimationsWord(ProjectileWord.OwnCue, null),
        [NativeEnemyProjectileInstruction.MoveRandomlyWithinXRadius_YRadius] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Of<TorizoProjectileCue.Scatter>("")),
        [NativeEnemyProjectileInstruction.Torizo_ResetPosition] = new TorizoProjectileAnimationsWord(ProjectileWord.OwnCue, null),
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

    /// <summary>The cues of the drool and fragments' own family each command becomes, and the motion a fall cue starts.</summary>
    private static readonly TorizoOwnCues<TorizoFallCueKind> FallCues = new(
        new() { [NativeEnemyProjectileInstruction.FrameBehaviorInY] = [TorizoFallCueKind.StartDroolFall, TorizoFallCueKind.StartFragmentFall] },
        new() { [TorizoFallCueKind.StartDroolFall] = TorizoProjectileMotion.DroolFalling, [TorizoFallCueKind.StartFragmentFall] = TorizoProjectileMotion.StatueFragmentFalling });

    /// <summary>The explosions' own cues each command becomes.</summary>
    private static readonly TorizoOwnCues<TorizoExplosionCueKind> ExplosionCues = new(
        new()
        {
            [NativeEnemyProjectileInstruction.Torizo_ResetPosition] = [TorizoExplosionCueKind.ReturnToOrigin],
            [NativeEnemyProjectileInstruction.GotoY_Probability_1_4] = [TorizoExplosionCueKind.MayTurnToSmoke],
        },
        []);

    /// <summary>The Chozo orbs' own cues each command becomes.</summary>
    private static readonly TorizoOwnCues<TorizoChozoOrbCueKind> ChozoOrbCues = new(
        new() { [NativeEnemyProjectileInstruction.SpawnEnemyDropsWIthYDropChances] = [TorizoChozoOrbCueKind.SpawnOrbDrops] },
        []);

    /// <summary>The egg's own cues each command becomes, and the motion its charge starts.</summary>
    private static readonly TorizoOwnCues<GoldenTorizoEggCueKind> EggCues = new(
        new()
        {
            [NativeEnemyProjectileInstruction.GoldenTorizoEgg_GoToHatched] = [GoldenTorizoEggCueKind.Hatch],
            [NativeEnemyProjectileInstruction.FrameBehaviorInY] = [GoldenTorizoEggCueKind.StartEggCharge],
        },
        new() { [GoldenTorizoEggCueKind.StartEggCharge] = GoldenTorizoEggMotion.EggCharging });

    /// <summary>The super missile's and eye beam's own cues each command becomes, and the motion the throw starts.</summary>
    private static readonly TorizoOwnCues<GoldenTorizoWeaponCueKind> WeaponCues = new(
        new()
        {
            [NativeEnemyProjectileInstruction.AimSuperMissile_Rightwards] = [GoldenTorizoWeaponCueKind.AimSuperMissileTowardSamus],
            [NativeEnemyProjectileInstruction.AimSuperMissile_Leftwards] = [GoldenTorizoWeaponCueKind.AimSuperMissileAwayFromSamus],
            [NativeEnemyProjectileInstruction.FrameBehaviorInY] = [GoldenTorizoWeaponCueKind.ThrowSuperMissile],
            [NativeEnemyProjectileInstruction.GotoYIfEyeBeamExplosionsDisabled] = [GoldenTorizoWeaponCueKind.FloorImpactWaited],
        },
        new() { [GoldenTorizoWeaponCueKind.ThrowSuperMissile] = GoldenTorizoWeaponMotion.SuperMissileThrown });

    /// <summary>
    /// How the projectile oracle reads one family's commands: each becomes the shared cue its <see cref="Words"/> entry names
    /// or one of the family's <paramref name="own"/> cues, with its operands compared field by field; a deletion and the egg's
    /// hatch end their lists, and the eye beam's conditional goto to its own list is the mark its floor impact's wait loops on
    /// (#2411).
    /// </summary>
    private static NativeEnemyProjectileCueRules<TorizoFamilyCue<TOwn>> CueRules<TOwn>(TorizoOwnCues<TOwn> own)
        where TOwn : struct, Enum => new(
        word => Words.TryGetValue((NativeEnemyProjectileInstruction)word, out var meaning) &&
                (meaning.Cue is not null || own.Kinds.ContainsKey((NativeEnemyProjectileInstruction)word))
            ? OperandByteCounts.GetValueOrDefault((NativeEnemyProjectileInstruction)word)
            : null,
        (word, operands, cue, at) =>
        {
            var instruction = (NativeEnemyProjectileInstruction)word;
            switch (cue)
            {
                case TorizoFamilyCue<TOwn>.Shared shared:
                    (Words[instruction].Cue ?? throw new InvalidOperationException($"{at}: {instruction} becomes no shared cue.")).Check(shared.Cue, at);
                    CompareOperands(instruction, operands, shared.Cue, at);
                    return;
                case TorizoFamilyCue<TOwn>.Own ownCue:
                    AssertTrue(own.Kinds.TryGetValue(instruction, out TOwn[]? kinds) && kinds.Contains(ownCue.Kind), $"{at}: {instruction} becomes {ownCue.Kind}");
                    CompareOwnOperands(operands, ownCue.Kind, own, at);
                    return;
                default:
                    throw new InvalidOperationException($"Undefined Torizo family cue {cue}.");
            }
        },
        EndsList: cue => cue is TorizoFamilyCue<TOwn>.Shared { Cue: TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.Delete } }
            || cue is TorizoFamilyCue<TOwn>.Own ended && (object)ended.Kind is GoldenTorizoEggCueKind.Hatch,
        LoopMarks: cue => cue is TorizoFamilyCue<TOwn>.Own mark && (object)mark.Kind is GoldenTorizoWeaponCueKind.FloorImpactWaited,
        // The sonic boom's wall impact calls a routine that returned at once before its deletion: it has no event (#1870).
        Omitted: word => (NativeEnemyProjectileInstruction)word == NativeEnemyProjectileInstruction.RTS_8681DE,
        // The drool, statue fragments, orb, hatchlings, Super Missile and eye beam show poses again from operands of their
        // own, each the pose's one installed frame (#2234).
        PicturedAt: EnemyProjectileProgramFrameOperands.OwnOperand);

    /// <summary>Every projectile animation matches the native lists it plays, and every native list is played by some animation.</summary>
    internal static void VerifyTorizoProjectileAnimations(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = rom;
        int steps = NativeEnemyProjectileProgramOracle.CompareByEntry(cartridge, ProjectileLists, ProjectileAnimationEntries,
            TorizoProjectileAnimations.Segments, TorizoProjectileAnimations.StartingAt,
            EnemyProjectileProgramFrameOperands.KeyOf, CueRules(FallCues), named: NamedProjectileAnimationEntries);
        steps += NativeEnemyProjectileProgramOracle.CompareByEntry(cartridge, ExplosionLists, ExplosionAnimationEntries,
            TorizoExplosionAnimations.Segments, TorizoExplosionAnimations.StartingAt, EnemyProjectileProgramFrameOperands.KeyOf, CueRules(ExplosionCues));
        steps += NativeEnemyProjectileProgramOracle.CompareByEntry(cartridge, ChozoOrbLists, ChozoOrbAnimationEntries,
            TorizoChozoOrbAnimations.Segments, TorizoChozoOrbAnimations.StartingAt, EnemyProjectileProgramFrameOperands.KeyOf, CueRules(ChozoOrbCues));
        steps += NativeEnemyProjectileProgramOracle.CompareByEntry(cartridge, EggLists, EggAnimationEntries,
            GoldenTorizoEggAnimations.Segments, GoldenTorizoEggAnimations.StartingAt, EnemyProjectileProgramFrameOperands.KeyOf, CueRules(EggCues));
        steps += NativeEnemyProjectileProgramOracle.CompareByEntry(cartridge, WeaponLists, WeaponAnimationEntries,
            GoldenTorizoWeaponAnimations.Segments, GoldenTorizoWeaponAnimations.StartingAt, EnemyProjectileProgramFrameOperands.KeyOf, CueRules(WeaponCues));

        int frames = 0;
        foreach (TorizoProjectileFamily family in Enum.GetValues<TorizoProjectileFamily>())
        {
            IReadOnlyList<EnemyProjectileFrameId> installed = TorizoProjectileAnimations.InstalledFrames(family);
            var shown = ShownFrames(TorizoProjectileAnimations.Segments, TorizoProjectileAnimationsConstants.Family, family)
                .Concat(ShownFrames(TorizoExplosionAnimations.Segments, TorizoProjectileAnimationsConstants.Family, family))
                .Concat(ShownFrames(TorizoChozoOrbAnimations.Segments, TorizoProjectileAnimationsConstants.Family, family))
                .Concat(ShownFrames(GoldenTorizoEggAnimations.Segments, TorizoProjectileAnimationsConstants.Family, family))
                .Concat(ShownFrames(GoldenTorizoWeaponAnimations.Segments, TorizoProjectileAnimationsConstants.Family, family))
                .ToHashSet();
            // The drool's delays and the eye beam's floor impact show the blank frame, which is installed nowhere (#2431).
            shown.Remove(EnemyProjectileFrameId.Blank);
            AssertTrue(shown.SetEquals(installed), $"the {family} animations show exactly the installed frames");
            ushort[] keys = [.. installed.Select(EnemyProjectileProgramFrameSources.Operand)];
            AssertTrue(keys.Zip(keys.Skip(1), (first, second) => first < second).All(ascends => ascends), $"the {family} frames install in native operand order");
            frames += installed.Count;
        }

        foreach ((int index, int word) in DroolDelayListTable)
        {
            ushort random = (ushort)(index << 2);
            AssertEqual(cartridge.ReadCartridgeWord(word),
                ProjectileEntryOf(TorizoRandomDefinitions.LowHealthDroolForm.Choose(random)), $"low-health drool delay table entry {index}");
        }

        // The egg's hatch starts the hatching animation of the side the egg was laid facing; the cartridge reaches the same
        // animation by stepping the rolling egg's list past its sleep.
        foreach ((GoldenTorizoEggSegment rolling, GoldenTorizoEggSegment hatching) in new[]
            {
                (GoldenTorizoEggSegment.EggBouncingLeft, GoldenTorizoEggSegment.EggHatchingLeft),
                (GoldenTorizoEggSegment.EggBouncingRight, GoldenTorizoEggSegment.EggHatchingRight),
            })
        {
            AssertEqual(AnimationEnd.Holds, GoldenTorizoEggAnimations.StartingWith(rolling).End, $"{rolling} holds");
            ushort sleep = unchecked((ushort)(ProjectileEntry(rolling) + 4));
            AssertEqual(ProjectileWord.Sleep, Words[(NativeEnemyProjectileInstruction)cartridge.ReadCartridgeWord(NativeEnemyProjectileListDefinitions.ProjectileBank | sleep)].Word,
                $"{rolling}'s sleep follows its frame");
            AssertEqual(EggAnimationAt(unchecked((ushort)(sleep + 2))), hatching,
                $"{rolling} hatches into {hatching}");
        }
        int animations = ProjectileAnimationEntries.Count + ExplosionAnimationEntries.Count + ChozoOrbAnimationEntries.Count + EggAnimationEntries.Count
            + WeaponAnimationEntries.Count;
        int lists = ProjectileLists.Length + ExplosionLists.Length + ChozoOrbLists.Length + EggLists.Length + WeaponLists.Length;
        Console.WriteLine($"  Torizo projectile animations: {animations} animations ({steps} steps, {frames} installed frames) play all {lists} bank-$86 lists of their families as the cartridge does.");
    }

    /// <summary>The frames the segments of <paramref name="segments"/> that belong to install family <paramref name="family"/> show.</summary>
    private static IEnumerable<EnemyProjectileFrameId> ShownFrames<TSegment, TCue>(AnimationSegments<TSegment, EnemyProjectileFrameId, TCue> segments,
        Func<TSegment, TorizoProjectileFamily> familyOf, TorizoProjectileFamily family)
        where TSegment : struct, Enum
        where TCue : notnull =>
        Enum.GetValues<TSegment>().Where(segment => familyOf(segment) == family)
            .SelectMany(segment => segments.Beats(segment)).Select(beat => beat.ShownFrame).OfType<EnemyProjectileFrameId>();

    /// <summary>Compares a command's operand bytes with the typed fields of one of a family's own cues.</summary>
    private static void CompareOwnOperands<TOwn>(byte[] operands, TOwn kind, TorizoOwnCues<TOwn> own, string at)
        where TOwn : struct, Enum
    {
        ushort Word(int offset) => LittleEndianBytes.Word(operands[offset], operands[offset + 1]);
        if (own.StartedMotion.TryGetValue(kind, out Enum? started))
        {
            // Setting a movement routine is the cue that starts the motion that routine stands for.
            AssertEqual<Enum?>(started, EnemyProjectileMotionAccess.FromWord(Word(0)), $"{at}: movement routine");
            return;
        }
        switch ((object)kind)
        {
            case GoldenTorizoWeaponCueKind.FloorImpactWaited:
                // The mark's operand is the entry of its own run's list, which the oracle checks (CueRules.LoopMarks).
                AssertEqual(2, operands.Length, $"{at}: the looped list's operand");
                return;
            case TorizoExplosionCueKind.MayTurnToSmoke:
                // The native branch names the smoke the owner turns the explosion into (#2167).
                AssertEqual(Word(0), ProjectileEntry(TorizoExplosionSegment.DeathSmoke), $"{at}: branch target");
                return;
            case TorizoChozoOrbCueKind.SpawnOrbDrops:
                AssertEqual(TorizoDeathDropsVerification.BombTorizoOrbHeader, (int)Word(0), $"{at}: Bomb Torizo orb header");
                AssertEqual(TorizoDeathDropsVerification.GoldenTorizoOrbHeader, (int)Word(2), $"{at}: Golden Torizo orb header");
                return;
            case GoldenTorizoWeaponCueKind.AimSuperMissileTowardSamus or GoldenTorizoWeaponCueKind.AimSuperMissileAwayFromSamus or
                TorizoExplosionCueKind.ReturnToOrigin or GoldenTorizoEggCueKind.Hatch:
                AssertEqual(0, operands.Length, $"{at}: no operands");
                return;
            default:
                throw new InvalidOperationException($"Undefined Torizo family cue {kind}.");
        }
    }

    /// <summary>Compares a command's operand bytes with a shared cue's typed fields.</summary>
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
            case TorizoProjectileCue.SetRadius setRadius:
                AssertEqual(new Extent16<RoomPixels>(operands[0], operands[1]), setRadius.Radius, $"{at}: radius");
                return;
            case TorizoProjectileCue.Scatter scatter:
                // Each range field reads exactly the bits of its native mask operand.
                AssertEqual<int>(operands[0], (scatter.Spread.XRange.Outcomes - 1) << scatter.Spread.XRange.Shift, $"{at}: scatter X range");
                AssertEqual<int>(operands[2], (scatter.Spread.YRange.Outcomes - 1) << scatter.Spread.YRange.Shift, $"{at}: scatter Y range");
                AssertEqual(new Extent16<RoomPixels>(operands[1], operands[3]), scatter.Spread.Center, $"{at}: scatter center");
                return;
            case TorizoProjectileCue.Signal { Kind: TorizoProjectileCueKind.StopMoving or TorizoProjectileCueKind.RaiseDust or
                TorizoProjectileCueKind.StopBlockingShots or TorizoProjectileCueKind.StopHarmingSamus or TorizoProjectileCueKind.DrawInFront or
                TorizoProjectileCueKind.Delete }:
                AssertEqual(0, operands.Length, $"{at}: no operands");
                return;
            case TorizoProjectileCue.PlaySound:
                throw new InvalidDataException($"{at}: {cue} is not an operand command here.");
            default:
                throw new InvalidOperationException($"Undefined Torizo projectile cue {cue}.");
        }
    }

    /// <summary>One Torizo animation family's own cues: what each native command becomes and the motion a cue starts.</summary>
    /// <typeparam name="TOwn">The family's own cues.</typeparam>
    /// <param name="Kinds">The own cues each native command may become.</param>
    /// <param name="StartedMotion">The motion each cue that starts one starts, which the native list names by its movement routine (#1843).</param>
    private sealed record TorizoOwnCues<TOwn>(Dictionary<NativeEnemyProjectileInstruction, TOwn[]> Kinds, Dictionary<TOwn, Enum> StartedMotion)
        where TOwn : struct, Enum;
}
