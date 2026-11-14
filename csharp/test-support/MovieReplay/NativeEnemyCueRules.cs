namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>How the <see cref="NativeEnemyAnimationOracle"/> reads and compares one owner's private instructions.</summary>
/// <param name="OperandWords">An owner instruction's operand word count, or null for a word the owner does not use.</param>
/// <param name="CompareCue">Compares an owner instruction's operands with the cue it became.</param>
/// <param name="EndsList">
/// Whether a cue always continues elsewhere, so the port ends its program there with an unreachable hold and the native
/// list's following bytes, never executed, are not compared; null when no cue does.
/// </param>
/// <param name="Omitted">
/// Whether a native instruction has no event in the typed animations because its only effect is presentation the port
/// does not keep (#1602) or because it has none (#1803); the oracle reads past it without a beat. Null when the owner
/// omits none.
/// </param>
internal sealed record NativeEnemyCueRules<TCue>(
    Func<ushort, int?> OperandWords,
    Action<ushort, ushort[], TCue, string> CompareCue,
    Func<TCue, bool>? EndsList = null,
    Func<ushort, bool>? Omitted = null)
    where TCue : notnull;
