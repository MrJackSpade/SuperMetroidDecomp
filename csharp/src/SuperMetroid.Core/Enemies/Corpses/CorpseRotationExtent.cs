namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>The number of rotation rows of a corpse and the tiles in each.</summary>
/// <param name="Count">The count.</param>
/// <param name="Tiles">The tiles.</param>
internal readonly record struct CorpseRotationExtent(int Count, int Tiles);
