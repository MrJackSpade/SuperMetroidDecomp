namespace SuperMetroid.Verification.Enemies;

/// <summary>How the <see cref="NativeEnemyProjectileProgramOracle"/> reads and compares one family's bank-$86 commands.</summary>
/// <param name="OperandBytes">A family command's operand byte count, or null for a word the family does not use.</param>
/// <param name="CompareCue">Compares a family command's operand bytes with the cue it became.</param>
/// <param name="EndsList">
/// Whether a cue always ends the list (a deletion, or a command that always continues elsewhere), so the port ends its
/// program there with an unreachable hold; null when no cue does.
/// </param>
/// <param name="ShowsFrame">
/// Whether a family command is a flicker: it shows a frame for one update itself, its operand words being that frame's
/// pose and any reflected pose, so the port follows its cue with a one-update show; null when no command does.
/// </param>
internal sealed record NativeEnemyProjectileCueRules<TCue>(
    Func<ushort, int?> OperandBytes,
    Action<ushort, byte[], TCue, string> CompareCue,
    Func<TCue, bool>? EndsList = null,
    Func<ushort, bool>? ShowsFrame = null)
    where TCue : notnull;
