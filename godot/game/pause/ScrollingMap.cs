using Engine.Content;
using Engine.Core;
using Engine.Input;
using Engine.Ui;
using Engine.World;
using Godot;

namespace Game;

/// <summary>
/// An area's map as a plane behind a screen's frame, which the directions move: the pause screen's map page and the
/// map of a saved game are both this. The map's cells show only through the opening of the frame; its markers and
/// labels lie over the frame too. It is first laid so that the middle of what shows of it lies about the middle of
/// the screen and a given place of it (where Samus is, or where the game was saved) lies within the page. Its bounds
/// are those of what the area's map shows once it is had, or of what has been explored while it is not, and it can
/// be moved a way only while what shows of it still lies past that edge of the page.
/// </summary>
/// <remarks>
/// Its data gives the size of a "cell" and of the screen ("screen_width", "screen_height"); the "opening" of the
/// frame it shows through ("x", "y", "width", "height", in the frame's own picture); the edges of the part of the
/// screen its bounds are kept to ("left_edge", "right_edge", "top_edge", "bottom_edge"); how it first lies
/// ("centre_column", across which the middle of what shows is put; "centre_line" and "row_middle_above_centre",
/// down; "here_leftmost", "here_rightmost" and "here_top", which the given place must not lie beyond;
/// "lowest_map_top"); the cells its bounds are taken to be while none of it shows ("unrevealed"); and how far a
/// press moves it ("scroll_step"), how many updates that takes ("scroll_updates") and on which of them it moves
/// ("scroll_moves_at").
/// </remarks>
public partial class ScrollingMap : Node2D
{
    /// <summary>The ways the map can be moved, in the order they are tried when several are held.</summary>
    public static readonly (string Name, GameAction Action, Vector2I Step)[] Ways =
    [
        ("left", GameAction.Left, new Vector2I(-1, 0)), ("right", GameAction.Right, new Vector2I(1, 0)),
        ("up", GameAction.Up, new Vector2I(0, -1)), ("down", GameAction.Down, new Vector2I(0, 1)),
    ];

    private DataNode? _data;
    private MapView? _cells;
    private MapView? _signs;
    private Vector2I _plane;
    private int _cell;
    private Rect2I _bounds;
    private string? _moving;
    private int _movingFor;

    /// <summary>How far the screen's corner lies from the map's, in pixels: what the map has been moved by.</summary>
    public Vector2I Scroll { get; private set; }

    /// <summary>
    /// A map of <paramref name="area"/> as <paramref name="progress"/> has it, laid out as <paramref name="data"/>
    /// says, first laid about <paramref name="here"/>, a place of the map in its pixels.
    /// </summary>
    /// <param name="data">How it is laid out and moved.</param>
    /// <param name="area">The area whose map it is.</param>
    /// <param name="progress">What is explored and revealed of it.</param>
    /// <param name="content">Where its pictures are loaded from.</param>
    /// <param name="frameAt">Where the corner of the frame's picture lies on the screen, which its opening is a part of.</param>
    /// <param name="plane">Where the corner of the map's own plane lies on the screen before it is moved.</param>
    /// <param name="here">The place of the map it is first laid about.</param>
    public static ScrollingMap Of(DataNode data, Area area, Progress progress, ContentLibrary content, Vector2I frameAt, Vector2I plane, Vector2I here)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(area);
        int cell = data.Int("cell");
        if (area.TileSize != cell)
            throw new InvalidOperationException($"The map is laid out for cells of {cell} pixels, and the map of {area.Name} has cells of {area.TileSize}.");
        var map = new ScrollingMap { Name = "Map", _data = data, _cell = cell, _plane = plane };
        DataNode opening = data.Get("opening");
        var window = new Control
        {
            Name = "Opening", ClipContents = true, MouseFilter = Control.MouseFilterEnum.Ignore,
            Position = new Vector2(frameAt.X + opening.Int("x"), frameAt.Y + opening.Int("y")), Size = new Vector2(opening.Int("width"), opening.Int("height")),
        };
        map.AddChild(window);

        // Enough cells to cover the screen wherever the map lies under it, and one more each way for a part cell.
        var cells = new Vector2I(data.Int("screen_width") / cell + 1, data.Int("screen_height") / cell + 1);
        map._cells = new MapView { Name = "Cells", Window = cells, DrawsSigns = false };
        window.AddChild(map._cells);
        map._cells.Present(area, progress, content);
        // The signs lie within the opening too: nothing of the map is drawn over its frame.
        map._signs = new MapView { Name = "Signs", Window = cells, DrawsCells = false };
        window.AddChild(map._signs);
        map._signs.Present(area, progress, content);

        // Its bounds are those of what the area's map shows once had, or of what has been explored while it is not.
        bool had = area.RevealedBy.Length > 0 && progress.Has(area.RevealedBy);
        // What has been explored counts wherever it is, a place whose cell shows nothing too, as the cartridge's
        // map counts every explored place.
        List<Vector2I> counted = had
            ? [.. area.Cells.Where(found => !found.Secret).Select(found => found.Place)]
            : [.. progress.Explored.Where(found => found.Area == area.Name).Select(found => new Vector2I(found.Column, found.Row))];
        DataNode none = data.Get("unrevealed");
        int left = counted.Count > 0 ? counted.Min(found => found.X) : none.Int("left");
        int right = counted.Count > 0 ? counted.Max(found => found.X) : none.Int("right");
        int top = counted.Count > 0 ? counted.Min(found => found.Y) : none.Int("top");
        int bottom = counted.Count > 0 ? counted.Max(found => found.Y) : none.Int("bottom");
        map._bounds = new Rect2I(left * cell, top * cell, (right - left) * cell, (bottom - top) * cell);

        int across = map._bounds.Position.X + map._bounds.Size.X / 2 - data.Int("centre_column");
        int hereAcross = here.X - across;
        if (hereAcross > data.Int("here_rightmost"))
            across += hereAcross - data.Int("here_rightmost");
        else if (hereAcross <= data.Int("here_leftmost"))
            across -= data.Int("here_leftmost") - hereAcross;
        int middle = map._bounds.Position.Y + map._bounds.Size.Y / 2 + data.Int("row_middle_above_centre");
        int lift = Mathf.FloorToInt((float)(data.Int("centre_line") - middle) / cell) * cell;
        int down = -lift;
        int hereDown = here.Y + lift;
        if (hereDown <= data.Int("here_top"))
            down = Math.Max(down - (data.Int("here_top") - hereDown), -data.Int("lowest_map_top"));
        map.Scroll = new Vector2I(across, down);
        map.Place();
        return map;
    }

    /// <summary>Whether the map's cells show; without them only the frame's opening is left.</summary>
    public bool ShowsCells
    {
        get => Cells().Visible;
        set => Cells().Visible = value;
    }

    /// <summary>Whether the map's markers and labels show.</summary>
    public bool ShowsSigns
    {
        get => Signs().Visible;
        set => Signs().Visible = value;
    }

    /// <summary>Where a place of the map, in its pixels, lies on the screen as the map now lies.</summary>
    public Vector2I OnScreen(Vector2I place) => place - Scroll;

    /// <summary>Whether the map can be moved a way: what shows of it still lies past that edge of the page.</summary>
    public bool CanScroll(string way)
    {
        DataNode at = Data();
        return way switch
        {
            "left" => _bounds.Position.X - Scroll.X < at.Int("left_edge"),
            "right" => _bounds.End.X - Scroll.X >= at.Int("right_edge"),
            "up" => _bounds.Position.Y - Scroll.Y < at.Int("top_edge"),
            "down" => _bounds.End.Y - Scroll.Y >= at.Int("bottom_edge"),
            _ => throw new InvalidOperationException($"The map cannot be moved \"{way}\"."),
        };
    }

    /// <summary>
    /// Plays one update of the map's moving, and says whether a move finished on it. A direction held starts a move,
    /// which takes its while and moves the map part way through; once started it runs on though the direction is let
    /// go, and only a move down is stopped short, when the map can go no further down.
    /// </summary>
    public bool StepScroll(GameInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        DataNode at = Data();
        if (_moving is null)
        {
            foreach ((string way, GameAction action, _) in Ways)
            {
                if (!CanScroll(way) || !input.IsHeld(action))
                    continue;
                (_moving, _movingFor) = (way, 0);
                break;
            }
        }

        if (_moving == "down" && !CanScroll("down"))
            _moving = null;
        if (_moving is not { } moving)
            return false;
        _movingFor++;
        if (_movingFor == at.Int("scroll_moves_at"))
        {
            Scroll += Ways.First(found => found.Name == moving).Step * at.Int("scroll_step");
            Place();
        }

        if (_movingFor < at.Int("scroll_updates"))
            return false;
        _moving = null;
        return true;
    }

    /// <summary>Puts the map where it has been moved to.</summary>
    private void Place()
    {
        MapView cells = Cells(), signs = Signs();
        var first = new Vector2I(Mathf.FloorToInt((float)Scroll.X / _cell), Mathf.FloorToInt((float)Scroll.Y / _cell));
        Vector2I at = first * _cell - Scroll + _plane;

        // Both are drawn within the opening, whose corner their own place is measured from.
        cells.Position = at - (Vector2I)cells.GetParent<Control>().Position;
        signs.Position = cells.Position;
        foreach (MapView view in new[] { cells, signs })
        {
            view.Centre = first + view.Window / 2;
            view.QueueRedraw();
        }
    }

    private MapView Cells() => _cells ?? throw NotMade();

    private MapView Signs() => _signs ?? throw NotMade();

    private DataNode Data() => _data ?? throw NotMade();

    private static InvalidOperationException NotMade() => new("The map has not been made.");
}
