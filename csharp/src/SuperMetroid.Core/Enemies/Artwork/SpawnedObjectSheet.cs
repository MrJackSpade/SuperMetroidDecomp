using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The sheet a room graphics set draws the objects spawned at one tile base from (#1392), and the palette row its
/// frames were coloured in, which a draw in another row overrides.
/// </summary>
/// <param name="Sheet">The pre-coloured sheet.</param>
/// <param name="Palette">The palette row its frames were coloured in.</param>
public sealed record SpawnedObjectSheet(SpriteSheet Sheet, int Palette);
