using Engine.Content;
using Engine.Core;
using Engine.World;
using Godot;

namespace Engine.Ui;

/// <summary>
/// Draws an area's map from what progress says is explored and revealed. A minimap, a pause map and a file-select map
/// are all this, with different windows onto the map. It decides nothing about what is explored.
/// </summary>
public partial class MapView : Node2D
{
    /// <summary>How many columns and rows of the map are drawn, around <see cref="Centre"/>; zero in either for the whole map.</summary>
    [Export]
    public Vector2I Window { get; set; }

    /// <summary>The cell the window is centred on.</summary>
    [Export]
    public Vector2I Centre { get; set; }

    /// <summary>The tint of a cell that is revealed but not explored, for an area with no pictures of its own for such cells.</summary>
    [Export]
    public Color RevealedTint { get; set; } = new(0.5f, 0.5f, 0.5f);

    /// <summary>The cell the player is in, shown with the area's picture for that where it has one; null to show none so.</summary>
    public Vector2I? Here { get; set; }

    /// <summary>Whether the map's cells are drawn. A view may draw only what lies over them, as where the cells show through an opening and the rest does not.</summary>
    [Export]
    public bool DrawsCells { get; set; } = true;

    /// <summary>Whether the map's labels are drawn: its pictures laid at places of their own, over its cells and under its markers.</summary>
    [Export]
    public bool DrawsLabels { get; set; } = true;

    /// <summary>Whether the map's markers are drawn, which lie over everything else of it.</summary>
    [Export]
    public bool DrawsMarkers { get; set; } = true;

    private Area? _area;
    private Progress? _progress;
    private Texture2D? _sheet;
    private Texture2D? _unexploredSheet;
    private Texture2D? _hereSheet;
    private readonly Dictionary<AreaLabel, Texture2D> _labels = [];
    private readonly Dictionary<AreaMarker, Texture2D> _markers = [];

    /// <summary>Shows <paramref name="area"/>'s map as <paramref name="progress"/> has it, with pictures from <paramref name="content"/>.</summary>
    public void Present(Area area, Progress progress, ContentLibrary content)
    {
        ArgumentNullException.ThrowIfNull(area);
        ArgumentNullException.ThrowIfNull(progress);
        ArgumentNullException.ThrowIfNull(content);
        _area = area;
        _progress = progress;
        _sheet = content.LoadTexture(area.Sheet);
        _unexploredSheet = area.UnexploredSheet.Length > 0 ? content.LoadTexture(area.UnexploredSheet) : null;
        _hereSheet = area.HereSheet.Length > 0 ? content.LoadTexture(area.HereSheet) : null;
        _labels.Clear();
        foreach (AreaLabel label in area.Labels)
            _labels[label] = content.LoadTexture(label.Picture);
        _markers.Clear();
        foreach (AreaMarker marker in area.Markers.Where(marker => marker.Picture.Length > 0))
            _markers[marker] = content.LoadTexture(marker.Picture);
        QueueRedraw();
    }

    /// <summary>Whether a cell of the map is inside the window being drawn.</summary>
    public bool IsInWindow(Vector2I cell)
    {
        if (Window.X <= 0 || Window.Y <= 0)
            return true;
        Vector2I first = Centre - Window / 2;
        return cell.X >= first.X && cell.X < first.X + Window.X && cell.Y >= first.Y && cell.Y < first.Y + Window.Y;
    }

    /// <summary>Whether the player has been in a cell.</summary>
    public bool IsExplored(AreaCell cell) => Shown().Progress.IsExplored(Shown().Area.Name, cell.Place.X, cell.Place.Y);

    /// <summary>Whether a cell shows: it has been explored, or the map has been revealed and the cell is not secret.</summary>
    public bool Shows(AreaCell cell)
    {
        (Area area, Progress progress) = Shown();
        return IsExplored(cell) || (!cell.Secret && area.RevealedBy.Length > 0 && progress.Has(area.RevealedBy));
    }

    /// <summary>The cells drawn at the moment, in the map's order.</summary>
    public IReadOnlyList<AreaCell> CellsDrawn() => Shown().Area.Cells.Where(cell => IsInWindow(cell.Place) && Shows(cell)).ToList();

    /// <summary>
    /// The markers drawn at the moment: each by its own rule, on a cell that shows, on a cell that has been explored,
    /// or always.
    /// </summary>
    public IReadOnlyList<AreaMarker> MarkersDrawn()
    {
        (Area area, Progress progress) = Shown();
        return area.Markers.Where(marker => IsInWindow(marker.Place) && marker.When switch
        {
            AreaMarkerShows.Always => true,
            AreaMarkerShows.Explored => progress.IsExplored(area.Name, marker.Place.X, marker.Place.Y),
            AreaMarkerShows.WithCell => area.CellAt(marker.Place.X, marker.Place.Y) is { } cell && Shows(cell),
            _ => throw new InvalidOperationException($"A marker shows {marker.When}, which is no rule a marker has."),
        }).ToList();
    }

    /// <summary>The labels drawn at the moment: those whose fact is held, whose hiding fact is not, and whose corner is in the window.</summary>
    public IReadOnlyList<AreaLabel> LabelsDrawn()
    {
        (Area area, Progress progress) = Shown();
        return area.Labels
            .Where(label => (label.When.Length == 0 || progress.Has(label.When)) && (label.Unless.Length == 0 || !progress.Has(label.Unless))
                && IsInWindow(new Vector2I(Mathf.FloorToInt((float)label.Place.X / area.TileSize), Mathf.FloorToInt((float)label.Place.Y / area.TileSize))))
            .ToList();
    }

    /// <inheritdoc/>
    public override void _Draw()
    {
        if (_area is null || _sheet is null)
            return;
        if (DrawsCells)
        {
            foreach (AreaCell cell in CellsDrawn())
            {
                if (Here == cell.Place && _hereSheet is not null)
                    DrawSmallPicture(cell.Place, cell.Shows, _hereSheet, Colors.White);
                else if (IsExplored(cell))
                    DrawSmallPicture(cell.Place, cell.Shows, _sheet, Colors.White);
                else if (_unexploredSheet is not null)
                    DrawSmallPicture(cell.Place, cell.Shows, _unexploredSheet, Colors.White);
                else
                    DrawSmallPicture(cell.Place, cell.Shows, _sheet, RevealedTint);
            }
        }

        // By kind: the labels lie over the cells, and the markers over the labels.
        Vector2I first = Window.X <= 0 || Window.Y <= 0 ? Vector2I.Zero : Centre - Window / 2;
        if (DrawsLabels)
        {
            foreach (AreaLabel label in LabelsDrawn())
                DrawTexture(_labels[label], label.Place - first * _area.TileSize);
        }

        if (!DrawsMarkers)
            return;
        foreach (AreaMarker marker in MarkersDrawn())
        {
            if (marker.Picture.Length > 0)
                DrawTexture(_markers[marker], marker.At - first * _area.TileSize);
            else
                DrawSmallPicture(marker.Place, marker.Shows, _sheet, Colors.White);
        }
    }

    /// <summary>Draws one of the map's small pictures, from one of the area's sheets, over a cell's place in the window.</summary>
    private void DrawSmallPicture(Vector2I place, string shows, Texture2D sheet, Color tint)
    {
        Area area = Shown().Area;
        Vector2I first = Window.X <= 0 || Window.Y <= 0 ? Vector2I.Zero : Centre - Window / 2;
        Vector2I at = (place - first) * area.TileSize;
        DrawTextureRectRegion(sheet, new Rect2(at, new Vector2(area.TileSize, area.TileSize)), area.PictureOf(shows), tint);
    }

    /// <summary>The area and progress being shown.</summary>
    private (Area Area, Progress Progress) Shown() =>
        _area is not null && _progress is not null ? (_area, _progress) : throw new InvalidOperationException("The map view has been given no map to show.");
}
