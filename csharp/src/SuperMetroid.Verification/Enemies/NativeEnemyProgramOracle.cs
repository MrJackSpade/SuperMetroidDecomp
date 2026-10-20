using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies;

/// <summary>
/// The shared #641 ROM oracle for typed enemy programs: walks each owner's native list beside its typed program and
/// requires every frame's duration and spritemap, every common command and every owner instruction's cue to match.
/// The owner supplies its instructions' operand counts and its cue comparison.
/// </summary>
internal static class NativeEnemyProgramOracle
{
    /// <summary>Compares every program with its native list and returns the total step count.</summary>
    /// <param name="cartridge">The pinned cartridge.</param>
    /// <param name="bank">The lists' bank.</param>
    /// <param name="entries">Each program's native entry, in authored order.</param>
    /// <param name="programs">The typed programs.</param>
    /// <param name="spritemap">The spritemap a frame shows.</param>
    /// <param name="operandWords">An owner instruction's operand word count, or null for a word the owner does not use.</param>
    /// <param name="compareCue">Compares an owner instruction's operands with the cue it became.</param>
    internal static int CompareAll<TProgram, TFrame, TCue>(
        IImportCartridgeSource cartridge,
        byte bank,
        (TProgram Program, ushort Entry)[] entries,
        AnimationProgramSet<TProgram, TFrame, TCue> programs,
        Func<TFrame, ushort> spritemap,
        Func<ushort, int?> operandWords,
        Action<ushort, ushort[], TCue, string> compareCue)
        where TProgram : struct, Enum
        where TFrame : struct
        where TCue : notnull =>
        CompareAll(cartridge, bank, entries, programs, spritemap, new NativeEnemyCueRules<TCue>(operandWords, compareCue));

    /// <summary>Compares every program with its native list under <paramref name="rules"/> and returns the total step count.</summary>
    /// <param name="cartridge">The pinned cartridge.</param>
    /// <param name="bank">The lists' bank.</param>
    /// <param name="entries">Each program's native entry, in authored order.</param>
    /// <param name="programs">The typed programs.</param>
    /// <param name="spritemap">The spritemap a frame shows.</param>
    /// <param name="rules">How the owner's instructions are read and compared.</param>
    internal static int CompareAll<TProgram, TFrame, TCue>(
        IImportCartridgeSource cartridge,
        byte bank,
        (TProgram Program, ushort Entry)[] entries,
        AnimationProgramSet<TProgram, TFrame, TCue> programs,
        Func<TFrame, ushort> spritemap,
        NativeEnemyCueRules<TCue> rules)
        where TProgram : struct, Enum
        where TFrame : struct
        where TCue : notnull =>
        StepAddresses(cartridge, bank, entries, programs, spritemap, rules).Values.Sum(addresses => addresses.Length);

    /// <summary>
    /// Compares every program with its native list, as <see cref="CompareAll{TProgram, TFrame, TCue}(IImportCartridgeSource, byte, ValueTuple{TProgram, ushort}[], AnimationProgramSet{TProgram, TFrame, TCue}, Func{TFrame, ushort}, Func{ushort, int?}, Action{ushort, ushort[], TCue, string})"/> does, and returns the native address of
    /// each program's steps. A typed cursor at a step and the native pointer at that address stand for the same state.
    /// </summary>
    internal static Dictionary<TProgram, ushort[]> StepAddresses<TProgram, TFrame, TCue>(
        IImportCartridgeSource cartridge,
        byte bank,
        (TProgram Program, ushort Entry)[] entries,
        AnimationProgramSet<TProgram, TFrame, TCue> programs,
        Func<TFrame, ushort> spritemap,
        Func<ushort, int?> operandWords,
        Action<ushort, ushort[], TCue, string> compareCue)
        where TProgram : struct, Enum
        where TFrame : struct
        where TCue : notnull =>
        StepAddresses(cartridge, bank, entries, programs, spritemap, new NativeEnemyCueRules<TCue>(operandWords, compareCue));

    /// <summary>Compares every program with its native list under <paramref name="rules"/> and returns each program's step addresses.</summary>
    internal static Dictionary<TProgram, ushort[]> StepAddresses<TProgram, TFrame, TCue>(
        IImportCartridgeSource cartridge,
        byte bank,
        (TProgram Program, ushort Entry)[] entries,
        AnimationProgramSet<TProgram, TFrame, TCue> programs,
        Func<TFrame, ushort> spritemap,
        NativeEnemyCueRules<TCue> rules)
        where TProgram : struct, Enum
        where TFrame : struct
        where TCue : notnull
    {
        AssertEqual(Enum.GetValues<TProgram>().Length, entries.Length, $"every typed {typeof(TProgram).Name} has a native entry");
        for (int index = 0; index < entries.Length; index++)
            AssertEqual(Enum.GetValues<TProgram>()[index], entries[index].Program, $"the {typeof(TProgram).Name} values are declared in authored order");
        AssertTrue(entries.Zip(entries.Skip(1)).All(pair => pair.First.Entry < pair.Second.Entry), $"native {typeof(TProgram).Name} entries ascend");
        var family = new Family<TProgram, TFrame, TCue>(cartridge, bank, entries, programs, spritemap, rules);
        return entries.ToDictionary(item => item.Program, item => Compare(family, item.Program, item.Entry));
    }

    /// <summary>One owner's programs and how its native lists are read and compared.</summary>
    /// <param name="Cartridge">The pinned cartridge.</param>
    /// <param name="Bank">The lists' bank.</param>
    /// <param name="Entries">Each program's native entry, in authored order.</param>
    /// <param name="Programs">The typed programs.</param>
    /// <param name="Spritemap">The spritemap a frame shows.</param>
    /// <param name="Rules">How the owner's instructions are read and compared.</param>
    private sealed record Family<TProgram, TFrame, TCue>(
        IImportCartridgeSource Cartridge,
        byte Bank,
        (TProgram Program, ushort Entry)[] Entries,
        AnimationProgramSet<TProgram, TFrame, TCue> Programs,
        Func<TFrame, ushort> Spritemap,
        NativeEnemyCueRules<TCue> Rules)
        where TProgram : struct, Enum
        where TFrame : struct
        where TCue : notnull;

    /// <summary>The native entry of <paramref name="program"/>.</summary>
    internal static ushort EntryOf<TProgram>((TProgram Program, ushort Entry)[] entries, TProgram program)
        where TProgram : struct, Enum =>
        entries.Single(item => EqualityComparer<TProgram>.Default.Equals(item.Program, program)).Entry;

    /// <summary>The program whose native list starts at <paramref name="address"/>, if one does.</summary>
    private static TProgram? ProgramAt<TProgram>((TProgram Program, ushort Entry)[] entries, ushort address)
        where TProgram : struct, Enum =>
        entries.Any(item => item.Entry == address) ? entries.Single(item => item.Entry == address).Program : null;

    /// <summary>Compares one program with its native list and returns each step's native address.</summary>
    private static ushort[] Compare<TProgram, TFrame, TCue>(Family<TProgram, TFrame, TCue> family, TProgram program, ushort entry)
        where TProgram : struct, Enum
        where TFrame : struct
        where TCue : notnull
    {
        (IImportCartridgeSource cartridge, byte bank, (TProgram Program, ushort Entry)[] entries, AnimationProgramSet<TProgram, TFrame, TCue> programs,
            Func<TFrame, ushort> spritemap, NativeEnemyCueRules<TCue> rules) = family;
        (Func<ushort, int?> operandWords, Action<ushort, ushort[], TCue, string> compareCue, Func<TCue, bool>? endsList) = rules;
        var steps = programs.Steps(program);
        var addresses = new ushort[steps.Count];
        ushort cursor = entry;
        int index = 0;
        AnimationProgramStep<TProgram, TFrame, TCue> Next(string what)
        {
            AssertTrue(index < steps.Count, $"{program} has a step for native {what} at ${bank:X2}:{cursor:X4}");
            addresses[index] = cursor;
            return steps[index++];
        }
        while (true)
        {
            string at = $"{program} at ${bank:X2}:{cursor:X4}";
            if (cursor != entry && ProgramAt(entries, cursor) is { } following)
            {
                var fall = Next("fall-through");
                AssertEqual(AnimationProgramStepKind.FallInto, fall.Kind, $"{at}: runs on into the next authored program");
                AssertEqual(following, fall.Target, $"{at}: fall-through target");
                break;
            }
            int address = (int)new SnesAddress(bank, cursor);
            ushort word = cartridge.ReadCartridgeWord(address);
            ushort operand = cartridge.ReadCartridgeWord(address + 2);
            NativeEnemyListDefinitions.WordKind kind = NativeEnemyListDefinitions.Classify(word);
            if (kind == NativeEnemyListDefinitions.WordKind.Frame)
            {
                var show = Next("frame");
                AssertEqual(AnimationProgramStepKind.Show, show.Kind, $"{at}: frame");
                AssertEqual(word, show.Duration, $"{at}: duration");
                AssertEqual(operand, spritemap(show.Frame), $"{at}: spritemap");
                cursor = unchecked((ushort)(cursor + 4));
                continue;
            }
            switch (kind)
            {
                case NativeEnemyListDefinitions.WordKind.Sleep:
                    AssertEqual(AnimationProgramStepKind.Hold, Next("sleep").Kind, $"{at}: sleep");
                    break;
                case NativeEnemyListDefinitions.WordKind.Stop:
                    AssertEqual(AnimationProgramStepKind.Stop, Next("stop").Kind, $"{at}: stop");
                    break;
                case NativeEnemyListDefinitions.WordKind.GoTo:
                {
                    var jump = Next("goto");
                    AssertEqual(AnimationProgramStepKind.GoTo, jump.Kind, $"{at}: goto");
                    AssertEqual(operand, EntryOf(entries, jump.Target), $"{at}: goto target");
                    break;
                }
                case NativeEnemyListDefinitions.WordKind.SetCounter:
                {
                    var load = Next("counter load");
                    AssertEqual(AnimationProgramStepKind.SetCounter, load.Kind, $"{at}: counter load");
                    AssertEqual(operand, load.Duration, $"{at}: counter");
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                }
                case NativeEnemyListDefinitions.WordKind.RepeatWhileCounting:
                {
                    var repeat = Next("counted repeat");
                    AssertEqual(AnimationProgramStepKind.RepeatWhileCounting, repeat.Kind, $"{at}: counted repeat");
                    AssertEqual(operand, EntryOf(entries, repeat.Target), $"{at}: repeat target");
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                }
                case NativeEnemyListDefinitions.WordKind.Wait:
                {
                    var wait = Next("wait");
                    AssertEqual(AnimationProgramStepKind.Wait, wait.Kind, $"{at}: wait");
                    AssertEqual(operand, wait.Duration, $"{at}: wait length");
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                }
                case NativeEnemyListDefinitions.WordKind.Callback:
                {
                    AssertTrue(operandWords(word) is not null, $"{at}: native instruction ${word:X4} is known");
                    int count = operandWords(word) ?? 0;
                    var step = Next("instruction");
                    AssertEqual(AnimationProgramStepKind.Cue, step.Kind, $"{at}: instruction");
                    ushort[] operands = [.. Enumerable.Range(0, count).Select(item => cartridge.ReadCartridgeWord(address + 2 + 2 * item))];
                    compareCue(word, operands, step.PublishedCue, at);
                    cursor = unchecked((ushort)(cursor + 2 + 2 * count));
                    if (endsList?.Invoke(step.PublishedCue) != true)
                        continue;
                    AssertEqual(AnimationProgramStepKind.Hold, Next("end after a cue that always continues elsewhere").Kind,
                        $"{at}: the program ends at its cue");
                    break;
                }
                case NativeEnemyListDefinitions.WordKind.Frame:
                default:
                    throw new InvalidOperationException($"Undefined {nameof(NativeEnemyListDefinitions.WordKind)} {kind}.");
            }
            break;
        }
        AssertEqual(steps.Count, index, $"{program} ends where its native list does");
        return addresses;
    }
}
