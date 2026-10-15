using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies;

/// <summary>
/// The shared #641 ROM oracle for typed enemy-projectile programs: walks each family's native bank-$86 list beside its
/// typed program and requires every frame's duration and presentation key (its operand's address), every control command
/// and every family command's cue to match. The family supplies its commands' operand byte counts and its cue comparison.
/// </summary>
internal static class NativeEnemyProjectileProgramOracle
{
    /// <summary>The control commands every projectile list shares, and what each becomes.</summary>
    private static readonly Dictionary<EnemyProjectileInstruction, AnimationProgramStepKind> ControlWords = new()
    {
        [EnemyProjectileInstruction.GotoY] = AnimationProgramStepKind.GoTo,
        [EnemyProjectileInstruction.Sleep] = AnimationProgramStepKind.Hold,
        [EnemyProjectileInstruction.TimerInY] = AnimationProgramStepKind.SetCounter,
        [EnemyProjectileInstruction.DecrementTimer_GotoYIfNonZero] = AnimationProgramStepKind.RepeatWhileCounting,
    };

    /// <summary>Compares every program with its native list under <paramref name="rules"/> and returns the total step count.</summary>
    /// <param name="cartridge">The pinned cartridge.</param>
    /// <param name="entries">Each program's native entry, in authored order.</param>
    /// <param name="programs">The typed programs.</param>
    /// <param name="presentationKey">The installed artwork key a frame shows: its native operand's address.</param>
    /// <param name="rules">How the family's commands are read and compared.</param>
    /// <param name="reflectedPresentationKey">
    /// The installed artwork key of a flicker frame's reflected pose, or null for a frame without one; required when
    /// <paramref name="rules"/> name flicker commands.
    /// </param>
    internal static int CompareAll<TProgram, TFrame, TCue>(
        IImportCartridgeSource cartridge,
        (TProgram Program, ushort Entry)[] entries,
        AnimationProgramSet<TProgram, TFrame, TCue> programs,
        Func<TFrame, ushort> presentationKey,
        NativeEnemyProjectileCueRules<TCue> rules,
        Func<TFrame, ushort?>? reflectedPresentationKey = null)
        where TProgram : struct, Enum
        where TFrame : struct
        where TCue : notnull
    {
        AssertEqual(Enum.GetValues<TProgram>().Length, entries.Length, $"every typed {typeof(TProgram).Name} has a native entry");
        for (int index = 0; index < entries.Length; index++)
            AssertEqual(Enum.GetValues<TProgram>()[index], entries[index].Program, $"the {typeof(TProgram).Name} values are declared in authored order");
        AssertTrue(entries.Zip(entries.Skip(1)).All(pair => pair.First.Entry < pair.Second.Entry), $"native {typeof(TProgram).Name} entries ascend");
        var family = new Family<TProgram, TFrame, TCue>(cartridge, entries, programs, presentationKey, rules, reflectedPresentationKey);
        return entries.Sum(item => Compare(family, item.Program, item.Entry));
    }

    /// <summary>One family's programs and how its native lists are read and compared.</summary>
    private sealed record Family<TProgram, TFrame, TCue>(
        IImportCartridgeSource Cartridge,
        (TProgram Program, ushort Entry)[] Entries,
        AnimationProgramSet<TProgram, TFrame, TCue> Programs,
        Func<TFrame, ushort> PresentationKey,
        NativeEnemyProjectileCueRules<TCue> Rules,
        Func<TFrame, ushort?>? ReflectedPresentationKey)
        where TProgram : struct, Enum
        where TFrame : struct
        where TCue : notnull;

    /// <summary>The program whose native list starts at <paramref name="address"/>, if one does.</summary>
    private static TProgram? ProgramAt<TProgram>((TProgram Program, ushort Entry)[] entries, ushort address)
        where TProgram : struct, Enum =>
        entries.Any(item => item.Entry == address) ? entries.Single(item => item.Entry == address).Program : null;

    /// <summary>Compares one program with its native list and returns its step count.</summary>
    private static int Compare<TProgram, TFrame, TCue>(Family<TProgram, TFrame, TCue> family, TProgram program, ushort entry)
        where TProgram : struct, Enum
        where TFrame : struct
        where TCue : notnull
    {
        (IImportCartridgeSource cartridge, (TProgram Program, ushort Entry)[] entries, AnimationProgramSet<TProgram, TFrame, TCue> programs,
            Func<TFrame, ushort> presentationKey, NativeEnemyProjectileCueRules<TCue> rules, Func<TFrame, ushort?>? reflectedPresentationKey) = family;
        var steps = programs.Steps(program);
        ushort cursor = entry;
        int index = 0;
        AnimationProgramStep<TProgram, TFrame, TCue> Next(string what)
        {
            AssertTrue(index < steps.Count, $"{program} has a step for native {what} at $86:{cursor:X4}");
            return steps[index++];
        }
        while (true)
        {
            string at = $"{program} at $86:{cursor:X4}";
            if (cursor != entry && ProgramAt(entries, cursor) is { } following)
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
                AssertEqual(unchecked((ushort)(cursor + 2)), presentationKey(show.Frame), $"{at}: presentation key");
                cursor = unchecked((ushort)(cursor + 4));
                continue;
            }
            if (ControlWords.TryGetValue((EnemyProjectileInstruction)word, out AnimationProgramStepKind control))
            {
                var step = Next(control.ToString());
                AssertEqual(control, step.Kind, $"{at}: {control}");
                switch (control)
                {
                    case AnimationProgramStepKind.Hold:
                        break;
                    case AnimationProgramStepKind.GoTo:
                        AssertEqual(operand, EntryOf(entries, step.Target), $"{at}: goto target");
                        break;
                    case AnimationProgramStepKind.SetCounter:
                        AssertEqual((int)operand, step.Duration, $"{at}: counter");
                        cursor = unchecked((ushort)(cursor + 4));
                        continue;
                    case AnimationProgramStepKind.RepeatWhileCounting:
                        AssertEqual(operand, EntryOf(entries, step.Target), $"{at}: repeat target");
                        cursor = unchecked((ushort)(cursor + 4));
                        continue;
                    default:
                        throw new InvalidOperationException($"Undefined projectile control step {control}.");
                }
                break;
            }
            AssertTrue(rules.OperandBytes(word) is not null, $"{at}: native command ${word:X4} is known");
            int count = rules.OperandBytes(word) ?? 0;
            var cue = Next("command");
            AssertEqual(AnimationProgramStepKind.Cue, cue.Kind, $"{at}: command");
            byte[] operands = [.. Enumerable.Range(0, count).Select(item => SnesLittleEndian.LowByte(cartridge.ReadCartridgeWord(address + 2 + item)))];
            rules.CompareCue(word, operands, cue.PublishedCue, at);
            if (rules.ShowsFrame?.Invoke(word) == true)
            {
                // A flicker shows its first operand's pose (or, by parity, its second's) for one update.
                var show = Next("flicker frame");
                AssertEqual(AnimationProgramStepKind.Show, show.Kind, $"{at}: flicker frame");
                AssertEqual(1, show.Duration, $"{at}: flicker duration");
                AssertEqual(unchecked((ushort)(cursor + 2)), presentationKey(show.Frame), $"{at}: flicker presentation key");
                Func<TFrame, ushort?> reflected = reflectedPresentationKey ??
                    throw new InvalidOperationException($"{at}: a flicker family supplies its reflected presentation keys.");
                AssertEqual(count == 4 ? (ushort?)unchecked((ushort)(cursor + 4)) : null, reflected(show.Frame), $"{at}: flicker reflected presentation key");
            }
            cursor = unchecked((ushort)(cursor + 2 + count));
            if (rules.EndsList?.Invoke(cue.PublishedCue) != true)
                continue;
            AssertEqual(AnimationProgramStepKind.Hold, Next("end after a cue that always ends the list").Kind, $"{at}: the program ends at its cue");
            break;
        }
        AssertEqual(steps.Count, index, $"{program} ends where its native list does");
        return index;
    }

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    internal static ushort EntryOf<TProgram>((TProgram Program, ushort Entry)[] entries, TProgram program)
        where TProgram : struct, Enum =>
        entries.Single(item => EqualityComparer<TProgram>.Default.Equals(item.Program, program)).Entry;
}
