using Godot;

namespace Engine.World;

/// <summary>One cell of an area's map.</summary>
/// <param name="Place">Its column and row on the map.</param>
/// <param name="Shows">The small picture it shows, by name.</param>
/// <param name="Secret">Whether it shows only once explored, never because the map was revealed.</param>
/// <param name="ExploresAbove">
/// Whether the cell above it is explored with it: a cell whose own upper part is no place to be in, as the low piece
/// of a sloped hallway's floor, whose map has its other half in the cell above.
/// </param>
public sealed record AreaCell(Vector2I Place, string Shows, bool Secret, bool ExploresAbove = false);
