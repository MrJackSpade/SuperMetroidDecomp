using Godot;

namespace Engine.World;

/// <summary>One cell of an area's map, or one marker on it.</summary>
/// <param name="Place">Its column and row on the map.</param>
/// <param name="Shows">The small picture it shows, by name.</param>
/// <param name="Secret">Whether it shows only once explored, never because the map was revealed.</param>
public sealed record AreaCell(Vector2I Place, string Shows, bool Secret);
