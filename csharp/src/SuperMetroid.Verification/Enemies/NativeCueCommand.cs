namespace SuperMetroid.Verification.Enemies;

/// <summary>One native command a family's lists use: its operand byte count and how the cue it became is checked.</summary>
/// <typeparam name="TCue">The family's cue type.</typeparam>
/// <param name="OperandBytes">The command's operand byte count.</param>
/// <param name="Check">Compares the command's operand bytes with the cue it became.</param>
internal sealed record NativeCueCommand<TCue>(int OperandBytes, Action<byte[], TCue, string> Check)
    where TCue : notnull
{
    /// <summary>A command that becomes exactly <paramref name="expected"/>.</summary>
    internal static NativeCueCommand<TCue> Exactly(TCue expected, int operandBytes = 0) =>
        new(operandBytes, (_, cue, at) => AssertEqual(expected, cue, $"{at}: cue"));

    /// <summary>A command that becomes a <typeparamref name="TVariant"/> whose fields <paramref name="check"/> compares.</summary>
    internal static NativeCueCommand<TCue> Of<TVariant>(int operandBytes, Action<byte[], TVariant, string> check)
        where TVariant : TCue =>
        new(operandBytes, (operands, cue, at) => check(operands, AssertIs<TVariant>(cue, $"{at}: cue"), at));

    /// <summary>The oracle rules for a family whose commands are <paramref name="commands"/>, keyed by native word.</summary>
    internal static NativeEnemyProjectileCueRules<TCue> Rules<TWord>(IReadOnlyDictionary<TWord, NativeCueCommand<TCue>> commands,
        Func<ushort, TWord> word, Func<TCue, bool>? endsList = null)
        where TWord : notnull =>
        new(native => commands.TryGetValue(word(native), out var command) ? command.OperandBytes : null,
            (native, operands, cue, at) => commands[word(native)].Check(operands, cue, at),
            endsList);
}
