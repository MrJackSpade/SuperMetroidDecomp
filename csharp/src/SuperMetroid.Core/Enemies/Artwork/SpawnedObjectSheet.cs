using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The sheet a room graphics set draws the objects spawned at one tile base from (#1392), and the image colour group its
/// frames were coloured in, which a draw in colours held elsewhere overrides.
/// </summary>
/// <param name="Sheet">The pre-coloured sheet.</param>
/// <param name="ColourGroup">The image colour group its frames were coloured in.</param>
public sealed record SpawnedObjectSheet(SpriteSheet Sheet, ImageColourGroup ColourGroup);
