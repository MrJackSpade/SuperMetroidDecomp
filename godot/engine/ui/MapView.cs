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

    private Area? _area;
    private Progress? _progress;
    private Texture2D? _sheet;
    private Texture2D? _unexploredSheet;
    private Texture2D? _hereSheet;

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

    /// <summary>The markers drawn at the moment: those on a cell that shows.</summary>
    public IReadOnlyList<AreaCell> MarkersDrawn()
    {
        Area area = Shown().Area;
        return area.Markers
            .Where(marker => IsInWindow(marker.Place) && area.CellAt(marker.Place.X, marker.Place.Y) is { } cell && Shows(cell))
            .ToList();
    }

    /// <inheritdoc/>
    public override void _Draw()
    {
        if (_area is null || _sheet is null)
            return;
        foreach (AreaCell cell in CellsDrawn())
        {
            if (Here == cell.Place && _hereSheet is not null)
                DrawCell(cell, _hereSheet, Colors.White);
            else if (IsExplored(cell))
                DrawCell(cell, _sheet, Colors.White);
            else if (_unexploredSheet is not null)
                DrawCell(cell, _unexploredSheet, Colors.White);
            else
                DrawCell(cell, _sheet, RevealedTint);
        }

        foreach (AreaCell marker in MarkersDrawn())
            DrawCell(marker, _sheet, Colors.White);
    }

    /// <summary>Draws one cell's or marker's small picture, from one of the area's sheets, at its place in the window.</summary>
    private void DrawCell(AreaCell cell, Texture2D sheet, Color tint)
    {
        Area area = Shown().Area;
        Vector2I first = Window.X <= 0 || Window.Y <= 0 ? Vector2I.Zero : Centre - Window / 2;
        Vector2I at = (cell.Place - first) * area.TileSize;
        DrawTextureRectRegion(sheet, new Rect2(at, new Vector2(area.TileSize, area.TileSize)), area.PictureOf(cell.Shows), tint);
    }

    /// <summary>The area and progress being shown.</summary>
    private (Area Area, Progress Progress) Shown() =>
        _area is not null && _progress is not null ? (_area, _progress) : throw new InvalidOperationException("The map view has been given no map to show.");
}
