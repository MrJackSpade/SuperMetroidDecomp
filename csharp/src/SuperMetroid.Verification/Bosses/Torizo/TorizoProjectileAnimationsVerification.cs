using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.Torizo.TorizoNativeListAddresses;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
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
    private enum ProjectileWord
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
    private static readonly Dictionary<EnemyProjectileInstruction, (ProjectileWord Word, TorizoProjectileCueKind Cue)> Words = new()
    {
        [EnemyProjectileInstruction.Sleep] = (ProjectileWord.Sleep, default),
        [EnemyProjectileInstruction.GotoY] = (ProjectileWord.GoTo, default),
        [EnemyProjectileInstruction.Delete] = (ProjectileWord.Delete, TorizoProjectileCueKind.Delete),
        [EnemyProjectileInstruction.GoldenTorizoEgg_GoToHatched] = (ProjectileWord.Hatch, TorizoProjectileCueKind.Hatch),
        [EnemyProjectileInstruction.TimerInY] = (ProjectileWord.SetCounter, default),
        [EnemyProjectileInstruction.DecrementTimer_GotoYIfNonZero] = (ProjectileWord.RepeatWhileCounting, default),
        [EnemyProjectileInstruction.ClearPreInstruction] = (ProjectileWord.Cue, TorizoProjectileCueKind.ClearPreInstruction),
        [EnemyProjectileInstruction.PreInstructionInY] = (ProjectileWord.Cue, TorizoProjectileCueKind.SetPreInstruction),
        [EnemyProjectileInstruction.Properties_OrY] = (ProjectileWord.Cue, TorizoProjectileCueKind.SetProperties),
        [EnemyProjectileInstruction.Properties_AndY] = (ProjectileWord.Cue, TorizoProjectileCueKind.ClearProperties),
        [EnemyProjectileInstruction.QueueSoundInY_Lib2_Max6] = (ProjectileWord.Cue, TorizoProjectileCueKind.PlaySound),
        [EnemyProjectileInstruction.QueueSoundInY_Lib3_Max6] = (ProjectileWord.Cue, TorizoProjectileCueKind.PlaySound),
        [EnemyProjectileInstruction.QueueSoundInY_Lib2_Max1] = (ProjectileWord.Cue, TorizoProjectileCueKind.PlaySound),
        [EnemyProjectileInstruction.GotoYIfEyeBeamExplosionsDisabled] = (ProjectileWord.Cue, TorizoProjectileCueKind.GoToIfEyeBeamExplosionsDisabled),
        [EnemyProjectileInstruction.GotoY_Probability_1_4] = (ProjectileWord.Cue, TorizoProjectileCueKind.GoToSometimes),
        [EnemyProjectileInstruction.AimSuperMissile_Rightwards] = (ProjectileWord.Cue, TorizoProjectileCueKind.AimSuperMissileTowardSamus),
        [EnemyProjectileInstruction.AimSuperMissile_Leftwards] = (ProjectileWord.Cue, TorizoProjectileCueKind.AimSuperMissileAwayFromSamus),
        [EnemyProjectileInstruction.XYRadiusInY] = (ProjectileWord.Cue, TorizoProjectileCueKind.SetRadius),
        [EnemyProjectileInstruction.SpawnEnemyDropsWIthYDropChances] = (ProjectileWord.Cue, TorizoProjectileCueKind.SpawnOrbDrops),
        [EnemyProjectileInstruction.MoveRandomlyWithinXRadius_YRadius] = (ProjectileWord.Cue, TorizoProjectileCueKind.Scatter),
        [EnemyProjectileInstruction.Torizo_ResetPosition] = (ProjectileWord.Cue, TorizoProjectileCueKind.ReturnToOrigin),
        [EnemyProjectileInstruction.TorizoLandingDustClouds] = (ProjectileWord.Cue, TorizoProjectileCueKind.RaiseDust),
        [EnemyProjectileInstruction.DisableCollisionWIthSamusProj] = (ProjectileWord.Cue, TorizoProjectileCueKind.StopBlockingShots),
        [EnemyProjectileInstruction.DisableCollisionWithSamus] = (ProjectileWord.Cue, TorizoProjectileCueKind.StopHarmingSamus),
        [EnemyProjectileInstruction.SetHighPriority] = (ProjectileWord.Cue, TorizoProjectileCueKind.DrawInFront),
        [EnemyProjectileInstruction.RTS_8681DE] = (ProjectileWord.Cue, TorizoProjectileCueKind.NoOperation),
    };

    /// <summary>The library and queue limit of each sound command Torizo's lists use; its one operand byte is the sound.</summary>
    private static readonly Dictionary<EnemyProjectileInstruction, (SoundEffectLibrary Library, byte MaximumQueued)> SoundCommands = new()
    {
        [EnemyProjectileInstruction.QueueSoundInY_Lib2_Max6] = (SoundEffectLibrary.Library2, 6),
        [EnemyProjectileInstruction.QueueSoundInY_Lib3_Max6] = (SoundEffectLibrary.Library3, 6),
        [EnemyProjectileInstruction.QueueSoundInY_Lib2_Max1] = (SoundEffectLibrary.Library2, 1),
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
                .Where(step => step.Kind == AnimationProgramStepKind.Show)
                .Select(step => step.Frame).ToHashSet();
            AssertTrue(shown.SetEquals(installed), $"the {family} programs show exactly the installed frames");
            ushort[] keys = [.. installed.Select(TorizoProjectileVisualDefinitions.PresentationKey)];
            AssertTrue(keys.Zip(keys.Skip(1)).All(pair => pair.First < pair.Second), $"the {family} frames install in native operand order");
            frames += installed.Count;
        }

        foreach ((int index, int word) in DroolDelayListTable)
        {
            ushort random = (ushort)(index << 2);
            AssertEqual(cartridge.ReadCartridgeWord(word),
                ProjectileEntry(BombTorizoBehavior.SelectLowHealthDroolProgram(random)), $"low-health drool delay table entry {index}");
        }

        foreach (TorizoProjectileProgram sleeping in new[] { TorizoProjectileProgram.EggBouncingLeft, TorizoProjectileProgram.EggBouncingRight })
        {
            var held = TorizoProjectileAnimations.Programs.Steps(sleeping);
            AssertEqual(AnimationProgramStepKind.Hold, held[^1].Kind, $"{sleeping} sleeps");
            ushort sleep = unchecked((ushort)(ProjectileEntry(sleeping) + 4));
            AssertEqual(ProjectileWord.Sleep, Words[(EnemyProjectileInstruction)cartridge.ReadCartridgeWord(NativeEnemyProjectileListDefinitions.ProjectileBank | sleep)].Word,
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
                AssertEqual(AnimationProgramStepKind.FallInto, fall.Kind, $"{at}: runs on into the next authored program");
                AssertEqual(following, fall.Target, $"{at}: fall-through target");
                break;
            }
            int address = NativeEnemyProjectileListDefinitions.ProjectileBank | cursor;
            ushort word = cartridge.ReadCartridgeWord(address);
            ushort operand = cartridge.ReadCartridgeWord(address + 2);
            if ((word & NativeEnemyListDefinitions.InstructionBit) == 0)
            {
                var show = Next("frame");
                AssertEqual(AnimationProgramStepKind.Show, show.Kind, $"{at}: frame");
                AssertEqual((int)word, show.Duration, $"{at}: duration");
                AssertEqual(unchecked((ushort)(cursor + 2)), TorizoProjectileVisualDefinitions.PresentationKey(show.Frame), $"{at}: installed frame");
                cursor = unchecked((ushort)(cursor + 4));
                continue;
            }
            var instruction = (EnemyProjectileInstruction)word;
            AssertTrue(Words.TryGetValue(instruction, out var meaning), $"{at}: native command ${word:X4} is known");
            switch (meaning.Word)
            {
                case ProjectileWord.Sleep:
                    AssertEqual(AnimationProgramStepKind.Hold, Next("sleep").Kind, $"{at}: sleep");
                    break;
                case ProjectileWord.GoTo:
                {
                    var jump = Next("goto");
                    AssertEqual(AnimationProgramStepKind.GoTo, jump.Kind, $"{at}: goto");
                    AssertEqual(operand, ProjectileEntry(jump.Target), $"{at}: goto target");
                    break;
                }
                case ProjectileWord.Delete:
                case ProjectileWord.Hatch:
                {
                    var cue = Next("ending command");
                    AssertEqual(AnimationProgramStepKind.Cue, cue.Kind, $"{at}: ending command");
                    AssertEqual(meaning.Cue, cue.Cue.Kind, $"{at}: ending cue");
                    AssertEqual(AnimationProgramStepKind.Hold, Next("hold").Kind, $"{at}: the program ends where the list does");
                    break;
                }
                case ProjectileWord.SetCounter:
                {
                    var load = Next("timer load");
                    AssertEqual(AnimationProgramStepKind.SetCounter, load.Kind, $"{at}: timer load");
                    AssertEqual((int)operand, load.Duration, $"{at}: timer");
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                }
                case ProjectileWord.RepeatWhileCounting:
                {
                    var repeat = Next("timer countdown");
                    AssertEqual(AnimationProgramStepKind.RepeatWhileCounting, repeat.Kind, $"{at}: timer countdown");
                    AssertEqual(operand, ProjectileEntry(repeat.Target), $"{at}: countdown target");
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                }
                case ProjectileWord.Cue:
                {
                    var step = Next("command");
                    AssertEqual(AnimationProgramStepKind.Cue, step.Kind, $"{at}: command");
                    TorizoProjectileCue cue = step.Cue;
                    AssertEqual(meaning.Cue, cue.Kind, $"{at}: cue");
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
    private static int CompareOperands(IImportCartridgeSource cartridge, EnemyProjectileInstruction instruction, int address, TorizoProjectileCue cue, string at)
    {
        ushort first = cartridge.ReadCartridgeWord(address + 2);
        if (SoundCommands.TryGetValue(instruction, out var sound))
        {
            AssertEqual(new SoundEffectId(sound.Library, OperandByte(cartridge, address + 2)), cue.Sound, $"{at}: sound");
            AssertEqual(sound.MaximumQueued, cue.MaximumQueued, $"{at}: sound queue limit");
            return 1;
        }
        switch (cue.Kind)
        {
            case TorizoProjectileCueKind.SetPreInstruction:
                AssertEqual(first, (ushort)cue.PreInstruction, $"{at}: movement routine");
                return 2;
            case TorizoProjectileCueKind.SetProperties:
                AssertEqual(first, (ushort)cue.Properties, $"{at}: property bits set");
                return 2;
            case TorizoProjectileCueKind.ClearProperties:
                AssertEqual(first, unchecked((ushort)~(ushort)cue.Properties), $"{at}: property bits kept");
                return 2;
            case TorizoProjectileCueKind.GoToIfEyeBeamExplosionsDisabled:
            case TorizoProjectileCueKind.GoToSometimes:
                AssertEqual(first, ProjectileEntry(cue.Target), $"{at}: branch target");
                return 2;
            case TorizoProjectileCueKind.SetRadius:
                AssertEqual(new Extent16<RoomPixels>(OperandByte(cartridge, address + 2), OperandByte(cartridge, address + 3)), cue.Radius, $"{at}: radius");
                return 2;
            case TorizoProjectileCueKind.SpawnOrbDrops:
                AssertEqual((ushort)EnemyDefinitionId.BombTorizoOrb, first, $"{at}: Bomb Torizo orb header");
                AssertEqual((ushort)EnemyDefinitionId.GoldenTorizoOrb, cartridge.ReadCartridgeWord(address + 4), $"{at}: Golden Torizo orb header");
                return 4;
            case TorizoProjectileCueKind.Scatter:
                AssertEqual(new EnemyProjectileScatter(
                        new(OperandByte(cartridge, address + 2), OperandByte(cartridge, address + 4)),
                        new(OperandByte(cartridge, address + 3), OperandByte(cartridge, address + 5))),
                    cue.Scatter, $"{at}: scatter");
                return 4;
            case TorizoProjectileCueKind.ClearPreInstruction:
            case TorizoProjectileCueKind.AimSuperMissileTowardSamus:
            case TorizoProjectileCueKind.AimSuperMissileAwayFromSamus:
            case TorizoProjectileCueKind.ReturnToOrigin:
            case TorizoProjectileCueKind.RaiseDust:
            case TorizoProjectileCueKind.StopBlockingShots:
            case TorizoProjectileCueKind.StopHarmingSamus:
            case TorizoProjectileCueKind.DrawInFront:
            case TorizoProjectileCueKind.NoOperation:
                return 0;
            case TorizoProjectileCueKind.PlaySound:
            case TorizoProjectileCueKind.Delete:
            case TorizoProjectileCueKind.Hatch:
                throw new InvalidDataException($"{at}: {cue.Kind} is not an operand command here.");
            default:
                throw new InvalidOperationException($"Undefined {nameof(TorizoProjectileCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Reads one byte of a native command's operands for the cartridge-side comparison.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Torizo projectile command operands")]
    private static byte OperandByte(IImportCartridgeSource cartridge, int address) => cartridge.ReadCartridgeByte(address);
}
