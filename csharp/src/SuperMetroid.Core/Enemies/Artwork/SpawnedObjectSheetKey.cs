namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>A spawned-object sheet's identity (#1392, #1513): the room graphics set and whose objects the sheet holds.</summary>
/// <param name="Set">The graphics set's name.</param>
/// <param name="Owner">Whose spawned objects the sheet pictures.</param>
public readonly record struct SpawnedObjectSheetKey(string Set, SpawnedObjectOwner Owner);
