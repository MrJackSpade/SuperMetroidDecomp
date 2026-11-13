using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Bosses.Torizo.TorizoNativeListAddresses;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>
/// #1289: Bomb and Golden Torizo's typed projectile programs against the retail cartridge. Each program must match its
/// native bank-$86 list step by step: every frame's duration and installed artwork key, every command's cue and
/// operands, and every branch's target. The installed frame catalog, the drool's delay table and the egg's resumed
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
        /// <summary>A command that becomes one cue.</summary>
        Cue,
    }

    /// <summary>Each native command Torizo's lists use: what it becomes and, for a cue, which cue.</summary>
    private static readonly Dictionary<NativeEnemyProjectileInstruction, TorizoProjectileAnimationsWord> Words = new()
    {
        [NativeEnemyProjectileInstruction.Sleep] = new TorizoProjectileAnimationsWord(ProjectileWord.Sleep, null),
        [NativeEnemyProjectileInstruction.GotoY] = new TorizoProjectileAnimationsWord(ProjectileWord.GoTo, null),
        [NativeEnemyProjectileInstruction.Delete] = new TorizoProjectileAnimationsWord(ProjectileWord.Delete, NativeCueShape<TorizoProjectileCue>.Exactly(new TorizoProjectileCue.Command(TorizoProjectileCueKind.Delete))),
        [NativeEnemyProjectileInstruction.GoldenTorizoEgg_GoToHatched] = new TorizoProjectileAnimationsWord(ProjectileWord.Hatch, NativeCueShape<TorizoProjectileCue>.Exactly(new TorizoProjectileCue.Command(TorizoProjectileCueKind.Hatch))),
        [NativeEnemyProjectileInstruction.TimerInY] = new TorizoProjectileAnimationsWord(ProjectileWord.SetCounter, null),
        [NativeEnemyProjectileInstruction.DecrementTimer_GotoYIfNonZero] = new TorizoProjectileAnimationsWord(ProjectileWord.RepeatWhileCounting, null),
        [NativeEnemyProjectileInstruction.ClearFrameBehavior] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Exactly(new TorizoProjectileCue.Command(TorizoProjectileCueKind.ClearFrameBehavior))),
        [NativeEnemyProjectileInstruction.FrameBehaviorInY] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Of<TorizoProjectileCue.SetFrameBehavior>("")),
        [NativeEnemyProjectileInstruction.Properties_OrY] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Of<TorizoProjectileCue.SetProperties>("")),
        [NativeEnemyProjectileInstruction.Properties_AndY] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Of<TorizoProjectileCue.ClearProperties>("")),
        [NativeEnemyProjectileInstruction.QueueSoundInY_Lib2_Max6] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Of<TorizoProjectileCue.PlaySound>("")),
        [NativeEnemyProjectileInstruction.QueueSoundInY_Lib3_Max6] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Of<TorizoProjectileCue.PlaySound>("")),
        [NativeEnemyProjectileInstruction.QueueSoundInY_Lib2_Max1] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Of<TorizoProjectileCue.PlaySound>("")),
        [NativeEnemyProjectileInstruction.GotoYIfEyeBeamExplosionsDisabled] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Of<TorizoProjectileCue.GoToIfEyeBeamExplosionsDisabled>("")),
        [NativeEnemyProjectileInstruction.GotoY_Probability_1_4] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Of<TorizoProjectileCue.GoToSometimes>("")),
        [NativeEnemyProjectileInstruction.AimSuperMissile_Rightwards] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Exactly(new TorizoProjectileCue.Command(TorizoProjectileCueKind.AimSuperMissileTowardSamus))),
        [NativeEnemyProjectileInstruction.AimSuperMissile_Leftwards] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Exactly(new TorizoProjectileCue.Command(TorizoProjectileCueKind.AimSuperMissileAwayFromSamus))),
        [NativeEnemyProjectileInstruction.XYRadiusInY] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Of<TorizoProjectileCue.SetRadius>("")),
        [NativeEnemyProjectileInstruction.SpawnEnemyDropsWIthYDropChances] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Exactly(new TorizoProjectileCue.Command(TorizoProjectileCueKind.SpawnOrbDrops))),
        [NativeEnemyProjectileInstruction.MoveRandomlyWithinXRadius_YRadius] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Of<TorizoProjectileCue.Scatter>("")),
        [NativeEnemyProjectileInstruction.Torizo_ResetPosition] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Exactly(new TorizoProjectileCue.Command(TorizoProjectileCueKind.ReturnToOrigin))),
        [NativeEnemyProjectileInstruction.TorizoLandingDustClouds] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Exactly(new TorizoProjectileCue.Command(TorizoProjectileCueKind.RaiseDust))),
        [NativeEnemyProjectileInstruction.DisableCollisionWIthSamusProj] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Exactly(new TorizoProjectileCue.Command(TorizoProjectileCueKind.StopBlockingShots))),
        [NativeEnemyProjectileInstruction.DisableCollisionWithSamus] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Exactly(new TorizoProjectileCue.Command(TorizoProjectileCueKind.StopHarmingSamus))),
        [NativeEnemyProjectileInstruction.SetHighPriority] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Exactly(new TorizoProjectileCue.Command(TorizoProjectileCueKind.DrawInFront))),
        [NativeEnemyProjectileInstruction.RTS_8681DE] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Exactly(new TorizoProjectileCue.Command(TorizoProjectileCueKind.NoOperation))),
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

    /// <summary>The projectile segments' native entries, in authored order.</summary>
    private static NativeProgramEntry<TorizoProjectileSegment>[] SegmentEntries =>
        [.. ProjectileEntries.Select(entry => new NativeProgramEntry<TorizoProjectileSegment>(entry.Program, entry.Entry))];

    /// <summary>
    /// How the projectile oracle reads Torizo's commands: each becomes the cue its <see cref="Words"/> entry names, with
    /// its operands compared field by field; a deletion and the egg's hatch end their lists.
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
        EndsList: cue => cue is TorizoProjectileCue.Command { Kind: TorizoProjectileCueKind.Delete or TorizoProjectileCueKind.Hatch });

    /// <summary>Every projectile segment and animation matches its native lists.</summary>
    internal static void VerifyTorizoProjectileAnimations(CartridgeImportAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        int beats = NativeEnemyProjectileProgramOracle.CompareAll(cartridge, SegmentEntries, TorizoProjectileAnimations.Segments,
            TorizoProjectileAnimations.StartingAt, frame => EnemyProjectileProgramFrameSources.Operand(TorizoProjectileVisualDefinitions.ShownProjectileFrames[frame]),
            CueRules);

        int frames = 0;
        foreach (TorizoProjectileFamily family in Enum.GetValues<TorizoProjectileFamily>())
        {
            IReadOnlyList<TorizoProjectileFrame> installed = TorizoProjectileVisualDefinitions.Frames(family);
            var shown = Enum.GetValues<TorizoProjectileSegment>()
                .Where(program => TorizoProjectileAnimationsConstants.Family(program) == family)
                .SelectMany(program => TorizoProjectileAnimations.Segments.Beats(program))
                .OfType<AnimationBeat<TorizoProjectileFrame, TorizoProjectileCue>.Frame>()
                .Select(frame => frame.Shown).ToHashSet();
            AssertTrue(shown.SetEquals(installed), $"the {family} segments show exactly the installed frames");
            ushort[] keys = [.. installed.Select((frame => EnemyProjectileProgramFrameSources.Operand(TorizoProjectileVisualDefinitions.ShownProjectileFrames[frame])))];
            AssertTrue(keys.Zip(keys.Skip(1), (first, second) => first < second).All(ascends => ascends), $"the {family} frames install in native operand order");
            frames += installed.Count;
        }

        foreach ((int index, int word) in DroolDelayListTable)
        {
            ushort random = (ushort)(index << 2);
            AssertEqual(cartridge.ReadCartridgeWord(word),
                ProjectileEntry(TorizoAttacks.SelectLowHealthDroolAnimation(random)), $"low-health drool delay table entry {index}");
        }

        foreach (TorizoProjectileSegment sleeping in new[] { TorizoProjectileSegment.EggBouncingLeft, TorizoProjectileSegment.EggBouncingRight })
        {
            AssertEqual(AnimationEnd.Holds, TorizoProjectileAnimations.StartingWith(sleeping).End, $"{sleeping} sleeps");
            ushort sleep = unchecked((ushort)(ProjectileEntry(sleeping) + 4));
            AssertEqual(ProjectileWord.Sleep, Words[(NativeEnemyProjectileInstruction)cartridge.ReadCartridgeWord(NativeEnemyProjectileListDefinitions.ProjectileBank | sleep)].Word,
                $"{sleeping}'s sleep follows its frame");
            AssertEqual(ProjectileProgramAt(unchecked((ushort)(sleep + 2))), TorizoProjectileAnimations.AfterSleep(sleeping),
                $"{sleeping} resumes past its sleep");
        }
        Console.WriteLine($"  Torizo projectile animations: {ProjectileEntries.Length} segments ({beats} beats, {frames} installed frames) and {TorizoProjectileAnimations.StartingAt.Count} animations match bank $86.");
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
            case TorizoProjectileCue.SetFrameBehavior setFrameBehavior:
                AssertEqual(setFrameBehavior.FrameBehavior, EnemyProjectileFrameBehaviorAccess.FromWord(Word(0)), $"{at}: movement routine");
                return;
            case TorizoProjectileCue.SetProperties setProperties:
                // The cue's behaviours, encoded by the import's property-word bits, are the OR operand the cartridge stores;
                // setting the depth bit moves the projectile in front of Samus.
                AssertEqual(Word(0), unchecked((ushort)(EnemyProjectilePropertyWords.ToNativeWord(setProperties.Properties) |
                    (setProperties.Depth is { } setDepth ? EnemyProjectilePropertyWords.ToNativeWord(setDepth) : 0))), $"{at}: property bits set");
                AssertTrue(setProperties.Depth is null or EnemyProjectileDepth.InFrontOfSamus, $"{at}: a property OR can only move the projectile in front of Samus");
                return;
            case TorizoProjectileCue.ClearProperties clearProperties:
                // The cue's behaviours, encoded by the import's property-word bits, are the bits the cartridge's AND mask clears;
                // clearing the depth bit moves the projectile behind Samus.
                AssertEqual(Word(0), unchecked((ushort)~(EnemyProjectilePropertyWords.ToNativeWord(clearProperties.Properties) |
                    (clearProperties.Depth is not null ? EnemyProjectilePropertyWords.ToNativeWord(EnemyProjectileDepth.InFrontOfSamus) : 0))), $"{at}: property bits kept");
                AssertTrue(clearProperties.Depth is null or EnemyProjectileDepth.BehindSamus, $"{at}: a property AND can only move the projectile behind Samus");
                return;
            case TorizoProjectileCue.GoToIfEyeBeamExplosionsDisabled goToIfEyeBeamExplosionsDisabled:
                AssertEqual(Word(0), ProjectileEntry(goToIfEyeBeamExplosionsDisabled.Target), $"{at}: branch target");
                return;
            case TorizoProjectileCue.GoToSometimes goToSometimes:
                AssertEqual(Word(0), ProjectileEntry(goToSometimes.Target), $"{at}: branch target");
                return;
            case TorizoProjectileCue.SetRadius setRadius:
                AssertEqual(new Extent16<RoomPixels>(operands[0], operands[1]), setRadius.Radius, $"{at}: radius");
                return;
            case TorizoProjectileCue.Command { Kind: TorizoProjectileCueKind.SpawnOrbDrops }:
                AssertEqual(SuperMetroid.AssetExtraction.Enemies.EnemyDefinitionIds.HeaderPointer(EnemyDefinitionId.BombTorizoOrb), Word(0), $"{at}: Bomb Torizo orb header");
                AssertEqual(SuperMetroid.AssetExtraction.Enemies.EnemyDefinitionIds.HeaderPointer(EnemyDefinitionId.GoldenTorizoOrb), Word(2), $"{at}: Golden Torizo orb header");
                return;
            case TorizoProjectileCue.Scatter scatter:
                // Each range field reads exactly the bits of its native mask operand.
                AssertEqual<int>(operands[0], (scatter.Spread.XRange.Outcomes - 1) << scatter.Spread.XRange.Shift, $"{at}: scatter X range");
                AssertEqual<int>(operands[2], (scatter.Spread.YRange.Outcomes - 1) << scatter.Spread.YRange.Shift, $"{at}: scatter Y range");
                AssertEqual(new Extent16<RoomPixels>(operands[1], operands[3]), scatter.Spread.Center, $"{at}: scatter center");
                return;
            case TorizoProjectileCue.Command { Kind: TorizoProjectileCueKind.ClearFrameBehavior or TorizoProjectileCueKind.AimSuperMissileTowardSamus or
                TorizoProjectileCueKind.AimSuperMissileAwayFromSamus or TorizoProjectileCueKind.ReturnToOrigin or TorizoProjectileCueKind.RaiseDust or
                TorizoProjectileCueKind.StopBlockingShots or TorizoProjectileCueKind.StopHarmingSamus or TorizoProjectileCueKind.DrawInFront or
                TorizoProjectileCueKind.NoOperation or TorizoProjectileCueKind.Delete or TorizoProjectileCueKind.Hatch }:
                AssertEqual(0, operands.Length, $"{at}: no operands");
                return;
            case TorizoProjectileCue.PlaySound:
                throw new InvalidDataException($"{at}: {cue} is not an operand command here.");
            default:
                throw new InvalidOperationException($"Undefined Torizo projectile cue {cue}.");
        }
    }
}
