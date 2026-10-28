using SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies;

/// <summary>
/// The shared #641 ROM oracle for typed enemy-projectile programs: walks each family's native bank-$86 list beside its
/// typed program and requires every frame's duration and presentation key (its operand's address), every control command
/// and every family command's cue to match. The family supplies its commands' operand byte counts and its cue comparison.
/// </summary>
internal static class NativeEnemyProjectileProgramOracle
{
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
        NativeProgramEntry<TProgram>[] entries,
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
        AssertTrue(entries.Zip(entries.Skip(1), (first, second) => first.Entry < second.Entry).All(ascends => ascends), $"native {typeof(TProgram).Name} entries ascend");
        var family = new Family<TProgram, TFrame, TCue>(cartridge, entries, programs, presentationKey, rules, reflectedPresentationKey);
        return entries.Sum(item => Compare(family, item.Program, item.Entry, addresses: null));
    }

    /// <summary>
    /// The native instruction address of every typed step, from the same walk <see cref="CompareAll"/> makes: a typed
    /// cursor's step stands where the native instruction pointer would, for comparisons with native recordings.
    /// </summary>
    internal static Dictionary<TProgram, ushort[]> StepAddresses<TProgram, TFrame, TCue>(
        IImportCartridgeSource cartridge,
        NativeProgramEntry<TProgram>[] entries,
        AnimationProgramSet<TProgram, TFrame, TCue> programs,
        Func<TFrame, ushort> presentationKey,
        NativeEnemyProjectileCueRules<TCue> rules,
        Func<TFrame, ushort?>? reflectedPresentationKey = null)
        where TProgram : struct, Enum
        where TFrame : struct
        where TCue : notnull
    {
        var family = new Family<TProgram, TFrame, TCue>(cartridge, entries, programs, presentationKey, rules, reflectedPresentationKey);
        return entries.ToDictionary(item => item.Program, item =>
        {
            List<ushort> addresses = [];
            Compare(family, item.Program, item.Entry, addresses);
            return addresses.ToArray();
        });
    }

    /// <summary>One family's programs and how its native lists are read and compared.</summary>
    private sealed record Family<TProgram, TFrame, TCue>(
        IImportCartridgeSource Cartridge,
        NativeProgramEntry<TProgram>[] Entries,
        AnimationProgramSet<TProgram, TFrame, TCue> Programs,
        Func<TFrame, ushort> PresentationKey,
        NativeEnemyProjectileCueRules<TCue> Rules,
        Func<TFrame, ushort?>? ReflectedPresentationKey)
        where TProgram : struct, Enum
        where TFrame : struct
        where TCue : notnull;

    /// <summary>The program whose native list starts at <paramref name="address"/>, if one does.</summary>
    private static TProgram? ProgramAt<TProgram>(NativeProgramEntry<TProgram>[] entries, ushort address)
        where TProgram : struct, Enum =>
        entries.Any(item => item.Entry == address) ? entries.Single(item => item.Entry == address).Program : null;

    /// <summary>Compares one program with its native list and returns its step count; records each step's native address when asked.</summary>
    private static int Compare<TProgram, TFrame, TCue>(Family<TProgram, TFrame, TCue> family, TProgram program, ushort entry, List<ushort>? addresses)
        where TProgram : struct, Enum
        where TFrame : struct
        where TCue : notnull
    {
        (IImportCartridgeSource cartridge, NativeProgramEntry<TProgram>[] entries, AnimationProgramSet<TProgram, TFrame, TCue> programs,
            Func<TFrame, ushort> presentationKey, NativeEnemyProjectileCueRules<TCue> rules, Func<TFrame, ushort?>? reflectedPresentationKey) = family;
        var steps = programs.Steps(program);
        ushort cursor = entry;
        int index = 0;
        AnimationProgramStep<TProgram, TFrame, TCue> Next(string what)
        {
            AssertTrue(index < steps.Count, $"{program} has a step for native {what} at $86:{cursor:X4}");
            addresses?.Add(cursor);
            return steps[index++];
        }
        while (true)
        {
            string at = $"{program} at $86:{cursor:X4}";
            if (cursor != entry && ProgramAt(entries, cursor) is { } following)
            {
                var fallInto = AssertIs<AnimationProgramStep<TProgram, TFrame, TCue>.FallIntoStep>(Next("fall-through"), $"{at}: runs on into the next authored program");
                AssertEqual(following, fallInto.Target, $"{at}: fall-through target");
                break;
            }
            if (rules.EndsAt?.Invoke(cursor) == true)
            {
                _ = AssertIs<AnimationProgramStep<TProgram, TFrame, TCue>.HoldStep>(Next("end where routine code follows"), $"{at}: the program ends where its list does");
                break;
            }
            int address = NativeEnemyProjectileListDefinitions.ProjectileBank | cursor;
            ushort word = cartridge.ReadCartridgeWord(address);
            ushort operand = cartridge.ReadCartridgeWord(address + 2);
            if ((word & NativeEnemyListDefinitions.InstructionBit) == 0)
            {
                var show = AssertIs<AnimationProgramStep<TProgram, TFrame, TCue>.ShowStep>(Next("frame"), $"{at}: frame");
                AssertEqual(word, show.Duration, $"{at}: duration");
                AssertEqual(unchecked((ushort)(cursor + 2)), presentationKey(show.Frame), $"{at}: presentation key");
                cursor = unchecked((ushort)(cursor + 4));
                continue;
            }
            // The control commands every projectile list shares.
            var instruction = (NativeEnemyProjectileInstruction)word;
            if (instruction == NativeEnemyProjectileInstruction.Sleep)
            {
                _ = AssertIs<AnimationProgramStep<TProgram, TFrame, TCue>.HoldStep>(Next("sleep"), $"{at}: sleep");
                break;
            }
            if (instruction == NativeEnemyProjectileInstruction.GotoY)
            {
                var goTo = AssertIs<AnimationProgramStep<TProgram, TFrame, TCue>.GoToStep>(Next("goto"), $"{at}: goto");
                AssertEqual(operand, EntryOf(entries, goTo.Target), $"{at}: goto target");
                break;
            }
            if (instruction == NativeEnemyProjectileInstruction.TimerInY)
            {
                var setCounter = AssertIs<AnimationProgramStep<TProgram, TFrame, TCue>.SetCounterStep>(Next("counter load"), $"{at}: counter load");
                AssertEqual(operand, setCounter.Count, $"{at}: counter");
                cursor = unchecked((ushort)(cursor + 4));
                continue;
            }
            if (instruction == NativeEnemyProjectileInstruction.DecrementTimer_GotoYIfNonZero)
            {
                var repeat = AssertIs<AnimationProgramStep<TProgram, TFrame, TCue>.RepeatWhileCountingStep>(Next("counted repeat"), $"{at}: counted repeat");
                AssertEqual(operand, EntryOf(entries, repeat.Target), $"{at}: repeat target");
                cursor = unchecked((ushort)(cursor + 4));
                continue;
            }
            AssertTrue(rules.OperandBytes(word) is not null, $"{at}: native command ${word:X4} is known");
            int count = rules.OperandBytes(word) ?? 0;
            var cueStep = AssertIs<AnimationProgramStep<TProgram, TFrame, TCue>.CueStep>(Next("command"), $"{at}: command");
            byte[] operands = [.. Enumerable.Range(0, count).Select(item => LittleEndianBytes.LowByte(cartridge.ReadCartridgeWord(address + 2 + item)))];
            rules.CompareCue(word, operands, cueStep.Cue, at);
            if (rules.ShowsFrame?.Invoke(word) == true)
            {
                // A flicker shows its first operand's pose (or, by parity, its second's) for one update.
                var flicker = AssertIs<AnimationProgramStep<TProgram, TFrame, TCue>.ShowStep>(Next("flicker frame"), $"{at}: flicker frame");
                AssertEqual(1, flicker.Duration, $"{at}: flicker duration");
                AssertEqual(unchecked((ushort)(cursor + 2)), presentationKey(flicker.Frame), $"{at}: flicker presentation key");
                Func<TFrame, ushort?> reflected = reflectedPresentationKey ??
                    throw new InvalidOperationException($"{at}: a flicker family supplies its reflected presentation keys.");
                AssertEqual(count == 4 ? unchecked((ushort)(cursor + 4)) : null, reflected(flicker.Frame), $"{at}: flicker reflected presentation key");
            }
            cursor = unchecked((ushort)(cursor + 2 + count));
            if (rules.EndsList?.Invoke(cueStep.Cue) != true)
                continue;
            _ = AssertIs<AnimationProgramStep<TProgram, TFrame, TCue>.HoldStep>(Next("end after a cue that always ends the list"), $"{at}: the program ends at its cue");
            break;
        }
        AssertEqual(steps.Count, index, $"{program} ends where its native list does");
        return index;
    }

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    internal static ushort EntryOf<TProgram>(NativeProgramEntry<TProgram>[] entries, TProgram program)
        where TProgram : struct, Enum =>
        entries.Single(item => EqualityComparer<TProgram>.Default.Equals(item.Program, program)).Entry;
}
