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

    /// <summary>Where the layer's picture lies in the room, in pixels.</summary>
    [Export]
    public Vector2I LiesAt { get; set; }

    /// <summary>The width and height it fills from there, in pixels; zero for as far as the room goes.</summary>
    [Export]
    public Vector2I Fills { get; set; }

    /// <summary>How far the picture drifts each update, in pixels, as clouds do: a picture that repeats comes round again.</summary>
    [Export]
    public Vector2 Drift { get; set; }

    /// <summary>How far the picture has drifted.</summary>
    private Vector2 _drifted;

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
    internal virtual void Follow(Vector2I viewTopLeft)
    {
        _drifted += Drift;
        if (Repeats && _texture is not null)
        {
            Vector2 size = _texture.GetSize();
            _drifted = new Vector2(Mathf.PosMod(_drifted.X, size.X), Mathf.PosMod(_drifted.Y, size.Y));
        }

        Position = new Vector2(
            LiesAt.X + Mathf.Floor(_drifted.X) + viewTopLeft.X * (1 - ScrollShare.X),
            LiesAt.Y + Mathf.Floor(_drifted.Y) + viewTopLeft.Y * (1 - ScrollShare.Y));
    }

    /// <inheritdoc/>
    public override void _Draw()
    {
        if (_texture is null)
            return;
        if (!Repeats)
        {
            DrawTexture(_texture, Vector2.Zero);
            return;
        }

        // One that drifts is laid a picture's width and height further back, so its drift uncovers nothing.
        Vector2 fills = new(Fills.X > 0 ? Fills.X : _roomSize.X, Fills.Y > 0 ? Fills.Y : _roomSize.Y);
        Vector2 back = new(Drift.X != 0 ? _texture.GetSize().X : 0, Drift.Y != 0 ? _texture.GetSize().Y : 0);
        DrawTextureRect(_texture, new Rect2(-back, fills + back), tile: true);
    }
}
