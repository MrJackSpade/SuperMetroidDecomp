using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.Torizo.TorizoNativeListAddresses;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Verification.Enemies;

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
        [NativeEnemyProjectileInstruction.ClearPreInstruction] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Exactly(new TorizoProjectileCue.Command(TorizoProjectileCueKind.ClearPreInstruction))),
        [NativeEnemyProjectileInstruction.PreInstructionInY] = new TorizoProjectileAnimationsWord(ProjectileWord.Cue, NativeCueShape<TorizoProjectileCue>.Of<TorizoProjectileCue.SetPreInstruction>("")),
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
        [NativeEnemyProjectileInstruction.QueueSoundInY_Lib2_Max6] = new SoundCommand(SoundEffectLibrary.Library2, SoundQueueLimit.Max6),
        [NativeEnemyProjectileInstruction.QueueSoundInY_Lib3_Max6] = new SoundCommand(SoundEffectLibrary.Library3, SoundQueueLimit.Max6),
        [NativeEnemyProjectileInstruction.QueueSoundInY_Lib2_Max1] = new SoundCommand(SoundEffectLibrary.Library2, SoundQueueLimit.Max1),
    };

    /// <summary>Every projectile program matches its native list.</summary>
    internal static void VerifyTorizoProjectileAnimations(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        AssertEqual(Enum.GetValues<TorizoProjectileProgram>().Length, ProjectileEntries.Length, "every typed Torizo projectile program has a native entry");
        for (int index = 0; index < ProjectileEntries.Length; index++)
            AssertEqual((TorizoProjectileProgram)index, ProjectileEntries[index].Program, "the Torizo projectile programs are declared in authored order");
        int steps = 0;
        foreach ((TorizoProjectileProgram program, ushort entry) in ProjectileEntries)
            steps += CompareProjectileProgram(cartridge, program, entry);

        int frames = 0;
        foreach (TorizoProjectileFamily family in Enum.GetValues<TorizoProjectileFamily>())
        {
            IReadOnlyList<TorizoProjectileFrame> installed = TorizoProjectileVisualDefinitions.Frames(family);
            var shown = Enum.GetValues<TorizoProjectileProgram>()
                .Where(program => TorizoProjectileAnimationsConstants.Family(program) == family)
                .SelectMany(program => TorizoProjectileAnimations.Programs.Steps(program))
                .OfType<AnimationProgramStep<TorizoProjectileProgram, TorizoProjectileFrame, TorizoProjectileCue>.ShowStep>()
                .Select(step => step.Frame).ToHashSet();
            AssertTrue(shown.SetEquals(installed), $"the {family} programs show exactly the installed frames");
            ushort[] keys = [.. installed.Select((frame => EnemyProjectileProgramFrameSources.Operand(TorizoProjectileVisualDefinitions.ShownProjectileFrames[frame])))];
            AssertTrue(keys.Zip(keys.Skip(1), (first, second) => first < second).All(ascends => ascends), $"the {family} frames install in native operand order");
            frames += installed.Count;
        }

        foreach ((int index, int word) in DroolDelayListTable)
        {
            ushort random = (ushort)(index << 2);
            AssertEqual(cartridge.ReadCartridgeWord(word),
                ProjectileEntry(TorizoAttacks.SelectLowHealthDroolProgram(random)), $"low-health drool delay table entry {index}");
        }

        foreach (TorizoProjectileProgram sleeping in new[] { TorizoProjectileProgram.EggBouncingLeft, TorizoProjectileProgram.EggBouncingRight })
        {
            var held = TorizoProjectileAnimations.Programs.Steps(sleeping);
            AssertTrue(held[^1] is AnimationProgramStep<TorizoProjectileProgram, TorizoProjectileFrame, TorizoProjectileCue>.HoldStep, $"{sleeping} sleeps");
            ushort sleep = unchecked((ushort)(ProjectileEntry(sleeping) + 4));
            AssertEqual(ProjectileWord.Sleep, Words[(NativeEnemyProjectileInstruction)cartridge.ReadCartridgeWord(NativeEnemyProjectileListDefinitions.ProjectileBank | sleep)].Word,
                $"{sleeping}'s sleep follows its frame");
            AssertEqual(ProjectileProgramAt(unchecked((ushort)(sleep + 2))), TorizoProjectileAnimations.AfterSleep(sleeping),
                $"{sleeping} resumes past its sleep");
        }
        Console.WriteLine($"  Torizo projectile programs: {ProjectileEntries.Length} programs ({steps} steps, {frames} installed frames) match bank $86.");
    }

    /// <summary>Compares one projectile program with its native list and returns its step count.</summary>
    private static int CompareProjectileProgram(IImportCartridgeSource cartridge, TorizoProjectileProgram program, ushort entry)
    {
        var steps = TorizoProjectileAnimations.Programs.Steps(program);
        ushort cursor = entry;
        int index = 0;
        AnimationProgramStep<TorizoProjectileProgram, TorizoProjectileFrame, TorizoProjectileCue> Next(string what)
        {
            AssertTrue(index < steps.Count, $"Torizo projectile {program} has a step for native {what} at $86:{cursor:X4}");
            return steps[index++];
        }
        while (true)
        {
            string at = $"Torizo projectile {program} at $86:{cursor:X4}";
            if (cursor != entry && ProjectileProgramAt(cursor) is { } following)
            {
                var fall = Next("fall-through");
                var fallInto = AssertIs<AnimationProgramStep<TorizoProjectileProgram, TorizoProjectileFrame, TorizoProjectileCue>.FallIntoStep>(fall, $"{at}: runs on into the next authored program");
                AssertEqual(following, fallInto.Target, $"{at}: fall-through target");
                break;
            }
            int address = NativeEnemyProjectileListDefinitions.ProjectileBank | cursor;
            ushort word = cartridge.ReadCartridgeWord(address);
            ushort operand = cartridge.ReadCartridgeWord(address + 2);
            if ((word & NativeEnemyListDefinitions.InstructionBit) == 0)
            {
                var show = AssertIs<AnimationProgramStep<TorizoProjectileProgram, TorizoProjectileFrame, TorizoProjectileCue>.ShowStep>(Next("frame"), $"{at}: frame");
                AssertEqual(word, show.Duration, $"{at}: duration");
                AssertEqual(unchecked((ushort)(cursor + 2)), EnemyProjectileProgramFrameSources.Operand(TorizoProjectileVisualDefinitions.ShownProjectileFrames[show.Frame]), $"{at}: installed frame");
                cursor = unchecked((ushort)(cursor + 4));
                continue;
            }
            var instruction = (NativeEnemyProjectileInstruction)word;
            AssertTrue(Words.TryGetValue(instruction, out var meaning), $"{at}: native command ${word:X4} is known");
            switch (meaning.Word)
            {
                case ProjectileWord.Sleep:
                    AssertTrue(Next("sleep") is AnimationProgramStep<TorizoProjectileProgram, TorizoProjectileFrame, TorizoProjectileCue>.HoldStep, $"{at}: sleep");
                    break;
                case ProjectileWord.GoTo:
                {
                    var jump = Next("goto");
                    var goTo = AssertIs<AnimationProgramStep<TorizoProjectileProgram, TorizoProjectileFrame, TorizoProjectileCue>.GoToStep>(jump, $"{at}: goto");
                    AssertEqual(operand, ProjectileEntry(goTo.Target), $"{at}: goto target");
                    break;
                }
                case ProjectileWord.Delete:
                case ProjectileWord.Hatch:
                {
                    var cue = Next("ending command");
                    var cueStep = AssertIs<AnimationProgramStep<TorizoProjectileProgram, TorizoProjectileFrame, TorizoProjectileCue>.CueStep>(cue, $"{at}: ending command");
                    (meaning.Cue ?? throw new InvalidOperationException($"{at}: an ending command names its cue.")).Check(cueStep.Cue, at);
                    AssertTrue(Next("hold") is AnimationProgramStep<TorizoProjectileProgram, TorizoProjectileFrame, TorizoProjectileCue>.HoldStep, $"{at}: the program ends where the list does");
                    break;
                }
                case ProjectileWord.SetCounter:
                {
                    var load = Next("timer load");
                    var setCounter = AssertIs<AnimationProgramStep<TorizoProjectileProgram, TorizoProjectileFrame, TorizoProjectileCue>.SetCounterStep>(load, $"{at}: timer load");
                    AssertEqual(operand, setCounter.Count, $"{at}: timer");
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                }
                case ProjectileWord.RepeatWhileCounting:
                {
                    var repeat = AssertIs<AnimationProgramStep<TorizoProjectileProgram, TorizoProjectileFrame, TorizoProjectileCue>.RepeatWhileCountingStep>(Next("timer countdown"), $"{at}: timer countdown");
                    AssertEqual(operand, ProjectileEntry(repeat.Target), $"{at}: countdown target");
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                }
                case ProjectileWord.Cue:
                {
                    var step = Next("command");
                    var cueStep = AssertIs<AnimationProgramStep<TorizoProjectileProgram, TorizoProjectileFrame, TorizoProjectileCue>.CueStep>(step, $"{at}: command");
                    TorizoProjectileCue cue = cueStep.Cue;
                    (meaning.Cue ?? throw new InvalidOperationException($"{at}: a cue command names its cue.")).Check(cue, at);
                    cursor = unchecked((ushort)(cursor + 2 + CompareOperands(cartridge, instruction, address, cue, at)));
                    continue;
                }
                default:
                    throw new InvalidOperationException($"Undefined {nameof(ProjectileWord)} {meaning.Word}.");
            }
            break;
        }
        AssertEqual(steps.Count, index, $"Torizo projectile {program} ends where its native list does");
        return steps.Count;
    }

    /// <summary>Compares a command's operands with its cue's typed fields and returns their byte count.</summary>
    private static int CompareOperands(IImportCartridgeSource cartridge, NativeEnemyProjectileInstruction instruction, int address, TorizoProjectileCue cue, string at)
    {
        ushort first = cartridge.ReadCartridgeWord(address + 2);
        if (SoundCommands.TryGetValue(instruction, out var sound))
        {
            var playSound = AssertIs<TorizoProjectileCue.PlaySound>(cue, $"{at}: cue");
            AssertEqual(SoundEffectCommands.Decode(sound.Library, OperandByte(cartridge, address + 2)), playSound.Sound, $"{at}: sound");
            AssertEqual(sound.MaximumQueued, playSound.MaximumQueued, $"{at}: sound queue limit");
            return 1;
        }
        switch (cue)
        {
            case TorizoProjectileCue.SetPreInstruction setPreInstruction:
                AssertEqual(first, EnemyProjectilePreInstructionAccess.Word(setPreInstruction.PreInstruction), $"{at}: movement routine");
                return 2;
            case TorizoProjectileCue.SetProperties setProperties:
                // The cue's behaviours, encoded by the import's property-word bits, are the OR operand the cartridge stores.
                AssertEqual(first, EnemyProjectilePropertyWords.ToNativeWord(setProperties.Properties), $"{at}: property bits set");
                return 2;
            case TorizoProjectileCue.ClearProperties clearProperties:
                // The cue's behaviours, encoded by the import's property-word bits, are the bits the cartridge's AND mask clears.
                AssertEqual(first, unchecked((ushort)~EnemyProjectilePropertyWords.ToNativeWord(clearProperties.Properties)), $"{at}: property bits kept");
                return 2;
            case TorizoProjectileCue.GoToIfEyeBeamExplosionsDisabled goToIfEyeBeamExplosionsDisabled:
                AssertEqual(first, ProjectileEntry(goToIfEyeBeamExplosionsDisabled.Target), $"{at}: branch target");
                return 2;
            case TorizoProjectileCue.GoToSometimes goToSometimes:
                AssertEqual(first, ProjectileEntry(goToSometimes.Target), $"{at}: branch target");
                return 2;
            case TorizoProjectileCue.SetRadius setRadius:
                AssertEqual(new Extent16<RoomPixels>(OperandByte(cartridge, address + 2), OperandByte(cartridge, address + 3)), setRadius.Radius, $"{at}: radius");
                return 2;
            case TorizoProjectileCue.Command { Kind: TorizoProjectileCueKind.SpawnOrbDrops }:
                AssertEqual(SuperMetroid.AssetExtraction.Enemies.EnemyDefinitionIds.HeaderPointer(EnemyDefinitionId.BombTorizoOrb), first, $"{at}: Bomb Torizo orb header");
                AssertEqual(SuperMetroid.AssetExtraction.Enemies.EnemyDefinitionIds.HeaderPointer(EnemyDefinitionId.GoldenTorizoOrb), cartridge.ReadCartridgeWord(address + 4), $"{at}: Golden Torizo orb header");
                return 4;
            case TorizoProjectileCue.Scatter scatter:
                AssertEqual(new EnemyProjectileScatter(
                        new(OperandByte(cartridge, address + 2), OperandByte(cartridge, address + 4)),
                        new(OperandByte(cartridge, address + 3), OperandByte(cartridge, address + 5))),
                    scatter.Spread, $"{at}: scatter");
                return 4;
            case TorizoProjectileCue.Command { Kind: TorizoProjectileCueKind.ClearPreInstruction or TorizoProjectileCueKind.AimSuperMissileTowardSamus or
                TorizoProjectileCueKind.AimSuperMissileAwayFromSamus or TorizoProjectileCueKind.ReturnToOrigin or TorizoProjectileCueKind.RaiseDust or
                TorizoProjectileCueKind.StopBlockingShots or TorizoProjectileCueKind.StopHarmingSamus or TorizoProjectileCueKind.DrawInFront or
                TorizoProjectileCueKind.NoOperation }:
                return 0;
            case TorizoProjectileCue.PlaySound or TorizoProjectileCue.Command { Kind: TorizoProjectileCueKind.Delete or TorizoProjectileCueKind.Hatch }:
                throw new InvalidDataException($"{at}: {cue} is not an operand command here.");
            default:
                throw new InvalidOperationException($"Undefined Torizo projectile cue {cue}.");
        }
    }

    /// <summary>Reads one byte of a native command's operands for the cartridge-side comparison.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Torizo projectile command operands")]
    private static byte OperandByte(IImportCartridgeSource cartridge, int address) => cartridge.ReadCartridgeByte(address);
}
