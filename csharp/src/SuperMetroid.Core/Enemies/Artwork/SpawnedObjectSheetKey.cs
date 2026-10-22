namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>A spawned-object sheet's identity (#1392): the room graphics set and the tile base its objects draw at.</summary>
/// <param name="Set">The graphics set's name.</param>
/// <param name="TileBase">The packed tile base the objects' parts are drawn at.</param>
public readonly record struct SpawnedObjectSheetKey(string Set, int TileBase);
