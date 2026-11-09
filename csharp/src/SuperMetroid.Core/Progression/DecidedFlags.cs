namespace SuperMetroid.Core.Progression;

/// <summary>
/// What an overwrite decides for a <see cref="ProgressSet{T}"/>: every member <paramref name="Decided"/> lists takes the
/// state the overwrite gives it, happened when <paramref name="Set"/> lists it and not otherwise; every other member
/// keeps its state.
/// </summary>
/// <typeparam name="T">The kind of progress.</typeparam>
/// <param name="Decided">The members whose state the overwrite decides.</param>
/// <param name="Set">The decided members the overwrite leaves happened.</param>
public sealed record DecidedFlags<T>(IReadOnlySet<T> Decided, IReadOnlySet<T> Set)
    where T : notnull;
