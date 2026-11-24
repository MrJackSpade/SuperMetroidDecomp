using Godot;

namespace Engine.World;

/// <summary>
/// A picture laid over an area's map at a place of its own, not a cell's: a sign, or words. It shows while one fact
/// is held and another is not.
/// </summary>
/// <param name="Place">Where its top-left corner lies on the map, in pixels from the map's own corner.</param>
/// <param name="Picture">Its picture, by the file it is in.</param>
/// <param name="When">The fact that must be held for it to show; empty when it needs none.</param>
/// <param name="Unless">The fact that hides it once held; empty when none does.</param>
public sealed record AreaLabel(Vector2I Place, string Picture, string When, string Unless);
