namespace SuperMetroid.Core.Progression;

/// <summary>
/// What an overwrite decides for a range of <see cref="NumberedFlags"/>: the flags numbered from zero up to
/// <paramref name="Decided"/>, each turned on when <paramref name="Set"/> lists it and off otherwise. Flags from
/// <paramref name="Decided"/> on keep their state.
/// </summary>
/// <param name="Decided">How many flags, from number zero, the overwrite decides.</param>
/// <param name="Set">The decided flags the overwrite turns on, ascending.</param>
public sealed record FlagOverwrite(int Decided, IReadOnlyList<int> Set);
