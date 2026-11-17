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
/// <param name="GoesTo">
/// The native entry an owner routine only goes on to (#2167): a routine that loads the instruction pointer with another
/// list and returns, which the oracle reads as that goto; null for any other word, or when the owner has none.
/// </param>
/// <param name="ShowsPoseAgain">
/// Whether an owner instruction may show the pose before it again (#2180), as the Yard's closed shell holds: the pose and
/// the instruction are a run of their own, which the owner plays again; null when the owner has none.
/// </param>
/// <param name="PicturedAt">
/// The native frame word an installed frame is read from, for a frame word the cartridge stores a picture at again (#2249):
/// a frame repeating another record's picture shows that record's frame; null when the import's repeated records
/// (<c>EnemyFrameCartridgeLocationDefinitions.AlsoStoredAt</c>) name every such word (#2272).
/// </param>
internal sealed record NativeEnemyCueRules<TCue>(
    Func<ushort, int?> OperandWords,
    Action<ushort, ushort[], TCue, string> CompareCue,
    Func<TCue, bool>? EndsList = null,
    Func<ushort, bool>? Omitted = null,
    Func<ushort, ushort?>? GoesTo = null,
    Func<ushort, bool>? ShowsPoseAgain = null,
    Func<ushort, ushort>? PicturedAt = null)
    where TCue : notnull;
