using Godot;

namespace Engine.World;

/// <summary>When a marker on an area's map shows.</summary>
public enum AreaMarkerShows
{
    /// <summary>While the cell it marks shows: explored, or revealed with the map.</summary>
    WithCell,

    /// <summary>Only once the cell it marks has been explored.</summary>
    Explored,

    /// <summary>Always.</summary>
    Always,
}

/// <summary>
/// A marker on an area's map: a sign of something at a cell. It is one of the map's small pictures laid over its
/// cell, or a picture of its own with its corner at a place in the map's pixels.
/// </summary>
/// <param name="Place">The column and row of the cell it marks.</param>
/// <param name="Shows">The small picture of the map's sheet it shows, by name; empty for one with a picture of its own.</param>
/// <param name="Picture">The file of its own picture; empty for one that shows a small picture of the sheet.</param>
/// <param name="At">For one with a picture of its own, where that picture's corner lies on the map, in pixels.</param>
/// <param name="When">When it shows.</param>
public sealed record AreaMarker(Vector2I Place, string Shows, string Picture, Vector2I At, AreaMarkerShows When);
