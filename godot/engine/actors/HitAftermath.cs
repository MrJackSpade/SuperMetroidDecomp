namespace Engine.Actors;

/// <summary>
/// What a hit has left of an enemy, or of a part an enemy keeps to itself, that another enemy may be given
/// (<see cref="Enemy.TakeAftermath"/>): each count with the updates it has left.
/// </summary>
/// <param name="ShakeShows">How many more times it is shown shaken.</param>
/// <param name="UnhittableUpdates">For how many more updates it cannot be hit.</param>
/// <param name="FlashUpdates">For how many more updates its hurt flash runs, which holds its behaviour meanwhile.</param>
/// <param name="Status">The status it is in; empty for none.</param>
/// <param name="StatusUpdates">For how many more updates the status lasts.</param>
public readonly record struct HitAftermath(int ShakeShows, int UnhittableUpdates, int FlashUpdates, string Status, int StatusUpdates);
