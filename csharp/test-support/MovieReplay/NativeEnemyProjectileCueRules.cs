using SuperMetroid.AssetExtraction.Enemies;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>How the <see cref="NativeEnemyProjectileProgramOracle"/> reads and compares one family's bank-$86 commands.</summary>
/// <param name="OperandBytes">A family command's operand byte count, or null for a word the family does not use.</param>
/// <param name="CompareCue">Compares a family command's operand bytes with the cue it became.</param>
/// <param name="EndsList">
/// Whether a cue always ends the list (a deletion, or a command that always continues elsewhere), so the port ends its
/// program there with an unreachable hold; null when no cue does.
/// </param>
/// <param name="EndsAt">
/// Whether a native list address is where the list ends because routine code follows it, which the program never
/// reaches, so the port ends its program there with an unreachable hold; null when no list does.
/// </param>
/// <param name="ShowsFrame">
/// Whether a family command is a flicker: it shows a frame for one update itself, its operand words being that frame's
/// pose and any reflected pose, so the port follows its cue with a one-update show; null when no command does.
/// </param>
/// <param name="LoopMarks">
/// Whether a cue marks the end of a run its animation loops on while the owner waits for its condition to change (#2411),
/// as the native conditional goto to its own list replays the run while the condition holds: its operand word is its own
/// list's entry, its run begins that list, the animation loops from that run's phase, and the animation the owner then
/// starts by name starts just past the goto; null when no cue does.
/// </param>
/// <param name="Omitted">
/// Whether a family command has no event in the typed animations because it has no effect (#1870): an operand-less
/// routine that returned at once, or a switch to the room's graphics before frames that are room pictures, which carry their
/// sheet and colours themselves (#2260). The oracle reads past it without a beat; null when the family omits none.
/// </param>
/// <param name="PicturedAt">
/// The operand an installed frame is read from, for an operand the cartridge stores a frame's picture at again (#2234): a
/// list showing a pose again from an operand of its own shows that pose's one frame; null when the import's repeated
/// operands (<c>EnemyProjectileProgramFrameDefinitions.AlsoStoredAt</c>) name every such operand (#2273).
/// </param>
internal sealed record NativeEnemyProjectileCueRules<TCue>(
    Func<ushort, int?> OperandBytes,
    Action<ushort, byte[], TCue, string> CompareCue,
    Func<TCue, bool>? EndsList = null,
    Func<ushort, bool>? ShowsFrame = null,
    Func<ushort, bool>? EndsAt = null,
    Func<TCue, bool>? LoopMarks = null,
    Func<ushort, bool>? Omitted = null,
    Func<ushort, ushort>? PicturedAt = null)
    where TCue : notnull
{
    /// <summary>
    /// The operand the installed frame shown from <paramref name="operand"/> is read from: the family's own rule where it has
    /// one, otherwise the frame's own operand for one the import's repeated operands name (#2273).
    /// </summary>
    internal ushort FrameOperand(ushort operand) => PicturedAt?.Invoke(operand) ?? EnemyProjectileProgramFrameOperands.OwnOperand(operand);
}
