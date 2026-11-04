using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The sheet a room graphics set draws one owner's spawned objects from (#1392, #1513), pre-coloured in the colours
/// its spawners draw in and in every shade they can show (#1512).
/// </summary>
/// <param name="Sheet">The pre-coloured sheet.</param>
public sealed record SpawnedObjectSheet(SpriteSheet Sheet);
