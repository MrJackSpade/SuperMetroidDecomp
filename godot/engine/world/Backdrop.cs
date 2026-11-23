using Engine.Content;
using Godot;

namespace Engine.World;

/// <summary>
/// One layer drawn behind or in front of a room: a picture that may scroll more slowly than the room, as distant
/// scenery does. It affects nothing in play.
/// </summary>
[Tool]
public partial class Backdrop : Node2D
{
    /// <summary>The layer's picture, as a path within the assets tree.</summary>
    [Export(PropertyHint.File)]
    public string Picture { get; set; } = "";

    /// <summary>How much of the view's scrolling the layer follows: 1 scrolls with the room, 0 stays still on screen.</summary>
    [Export]
    public Vector2 ScrollShare { get; set; } = Vector2.One;

    /// <summary>Whether the picture repeats to fill the room.</summary>
    [Export]
    public bool Repeats { get; set; } = true;

    /// <summary>The loaded picture.</summary>
    private Texture2D? _texture;

    /// <summary>The loaded picture, for a kind of backdrop that draws it its own way.</summary>
    protected Texture2D? Texture => _texture;

    /// <summary>The size of the room the layer fills, in pixels.</summary>
    private Vector2I _roomSize;

    /// <summary>Loads the layer's picture.</summary>
    internal virtual void Setup(ContentLibrary content, Vector2I roomSize)
    {
        _roomSize = roomSize;
        _texture = Picture.Length > 0 ? content.LoadTexture(Picture) : null;
        QueueRedraw();
    }

    /// <summary>Places the layer for a view whose top-left corner is at <paramref name="viewTopLeft"/> in the room.</summary>
    internal virtual void Follow(Vector2I viewTopLeft) =>
        Position = new Vector2(viewTopLeft.X * (1 - ScrollShare.X), viewTopLeft.Y * (1 - ScrollShare.Y));

    /// <inheritdoc/>
    public override void _Draw()
    {
        if (_texture is null)
            return;
        if (Repeats)
            DrawTextureRect(_texture, new Rect2(Vector2.Zero, _roomSize), tile: true);
        else
            DrawTexture(_texture, Vector2.Zero);
    }
}
