using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Drawing;

/// <summary>
/// The colours an encounter loads into a sprite colour set for itself rather than through the room's graphics set
/// (#1464), such as a boss's body colours, named by the colour run its definitions give them.
/// </summary>
/// <param name="Colours">The encounter's colour run whose set the actor draws in.</param>
internal sealed record EncounterSpriteColouring(SpriteColourRun Colours) : SpriteColouring;
