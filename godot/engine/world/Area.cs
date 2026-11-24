using Engine.Content;
using Godot;

namespace Engine.World;

/// <summary>
/// An area of the world as its map shows it: its name, the cells of its map and what each shows, its markers, and the
/// sheet of small pictures the map is drawn from. It is read from a data file and never changes; what the player has
/// explored is in progress, not here.
/// </summary>
/// <remarks>
/// <code>
/// { "name": "caves", "revealed_by": "caves map seen",
///   "sheet": "maps/tiles.png", "tile_size": 8,
///   "tiles": { "corridor": { "column": 0, "row": 0 }, "save": { "column": 1, "row": 0 } },
///   "cells": [ { "column": 3, "row": 2, "shows": "corridor" }, { "column": 4, "row": 2, "shows": "corridor", "secret": true } ],
///   "markers": [ { "column": 3, "row": 2, "shows": "save" } ] }
/// </code>
/// A secret cell shows only once explored, never because the map was revealed. An area may give two more sheets
/// laid out as "sheet" is: "unexplored_sheet", the pictures of cells that show but have not been explored, and
/// "here_sheet", the pictures of the cell the player is in. It may give "labels": pictures of their own laid over
/// the map at a place in its pixels, each an "x", a "y", a "picture" file, and optionally "when", a fact that must
/// be held for it to show, and "unless", a fact that hides it.
/// </remarks>
public sealed class Area
{
    private readonly Dictionary<Vector2I, AreaCell> _cells = [];
    private readonly Dictionary<string, Vector2I> _tiles = new(StringComparer.Ordinal);

    private Area()
    {
    }

    /// <summary>The area's name, as rooms and progress give it.</summary>
    public string Name { get; private init; } = "";

    /// <summary>The fact that, once recorded, reveals every cell that is not secret; empty when nothing reveals the map.</summary>
    public string RevealedBy { get; private init; } = "";

    /// <summary>The picture, in the assets tree, holding the map's small pictures in a grid.</summary>
    public string Sheet { get; private init; } = "";

    /// <summary>The picture holding the small pictures of cells not yet explored; empty when those are the explored ones, dimmed.</summary>
    public string UnexploredSheet { get; private init; } = "";

    /// <summary>The picture holding the small pictures of the cell the player is in; empty when that cell is not shown differently.</summary>
    public string HereSheet { get; private init; } = "";

    /// <summary>The width and height of one small picture, in pixels.</summary>
    public int TileSize { get; private init; }

    /// <summary>The map's cells, in the order the file gives them.</summary>
    public IReadOnlyList<AreaCell> Cells { get; private init; } = [];

    /// <summary>The map's markers, in the order the file gives them.</summary>
    public IReadOnlyList<AreaCell> Markers { get; private init; } = [];

    /// <summary>The pictures laid over the map at places of their own, in the order the file gives them.</summary>
    public IReadOnlyList<AreaLabel> Labels { get; private init; } = [];

    /// <summary>Reads an area from its data file.</summary>
    public static Area Read(DataNode data)
    {
        var area = new Area
        {
            Name = data.Text("name"),
            RevealedBy = data.Has("revealed_by") ? data.Text("revealed_by") : "",
            Sheet = data.Text("sheet"),
            UnexploredSheet = data.Has("unexplored_sheet") ? data.Text("unexplored_sheet") : "",
            HereSheet = data.Has("here_sheet") ? data.Text("here_sheet") : "",
            TileSize = data.Int("tile_size"),
            Cells = data.List("cells").Select(ReadCell).ToList(),
            Markers = data.Has("markers") ? data.List("markers").Select(ReadCell).ToList() : [],
            Labels = data.Has("labels") ? data.List("labels").Select(ReadLabel).ToList() : [],
        };
        if (area.TileSize <= 0)
            throw data.Get("tile_size").Problem("must be a positive number of pixels");
        foreach ((string name, DataNode tile) in data.Get("tiles").Entries())
            area._tiles[name] = new Vector2I(tile.Int("column"), tile.Int("row"));
        foreach (AreaCell cell in area.Cells)
        {
            if (!area._cells.TryAdd(cell.Place, cell))
                throw data.Get("cells").Problem($"gives the cell at column {cell.Place.X}, row {cell.Place.Y} twice");
        }

        foreach (AreaCell cell in area.Cells.Concat(area.Markers))
        {
            if (!area._tiles.ContainsKey(cell.Shows))
                throw data.Get("tiles").Problem($"has no picture named \"{cell.Shows}\"");
        }

        return area;
    }

    /// <summary>The cell at a column and row of the map, when the map has one there.</summary>
    public AreaCell? CellAt(int column, int row) => _cells.TryGetValue(new Vector2I(column, row), out AreaCell? cell) ? cell : null;

    /// <summary>Where on the sheet the small picture named <paramref name="shows"/> is, in pixels.</summary>
    public Rect2I PictureOf(string shows)
    {
        Vector2I tile = _tiles[shows];
        return new Rect2I(tile.X * TileSize, tile.Y * TileSize, TileSize, TileSize);
    }

    /// <summary>Reads one cell or marker.</summary>
    /// <summary>Reads one label.</summary>
    private static AreaLabel ReadLabel(DataNode data) => new(
        new Vector2I(data.Int("x"), data.Int("y")), data.Text("picture"), data.Has("when") ? data.Text("when") : "",
        data.Has("unless") ? data.Text("unless") : "");

    private static AreaCell ReadCell(DataNode data) =>
        new(new Vector2I(data.Int("column"), data.Int("row")), data.Text("shows"), data.Has("secret") && data.Flag("secret"));
}
