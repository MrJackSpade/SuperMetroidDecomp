using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The sheet a room graphics set draws the objects spawned at one tile base from (#1392), pre-coloured in the colours
/// its spawners draw in and in every shade they can show (#1512).
/// </summary>
/// <param name="Sheet">The pre-coloured sheet.</param>
public sealed record SpawnedObjectSheet(SpriteSheet Sheet);
