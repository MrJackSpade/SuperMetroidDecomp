namespace SuperMetroid.Verification.Enemies;

/// <summary>How the <see cref="NativeEnemyProgramOracle"/> reads and compares one owner's private instructions.</summary>
/// <param name="OperandWords">An owner instruction's operand word count, or null for a word the owner does not use.</param>
/// <param name="CompareCue">Compares an owner instruction's operands with the cue it became.</param>
/// <param name="EndsList">
/// Whether a cue always continues elsewhere, so the port ends its program there with an unreachable hold and the native
/// list's following bytes, never executed, are not compared; null when no cue does.
/// </param>
internal sealed record NativeEnemyCueRules<TCue>(
    Func<ushort, int?> OperandWords,
    Action<ushort, ushort[], TCue, string> CompareCue,
    Func<TCue, bool>? EndsList = null)
    where TCue : notnull;
