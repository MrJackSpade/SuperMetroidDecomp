using Engine.Content;
using Godot;

namespace Engine.World;

/// <summary>
/// One layer drawn behind or in front of a room: a picture that may scroll more slowly than the room, as distant
/// scenery does. It affects nothing in play. A picture may have beside it the part of it that lies in front of the
/// things standing in the room, as a tile's may: the picture stays whole where it is, and that part is drawn again
/// at its own depth (<see cref="RoomDepth.BackdropInFront"/>), over the room's things and the tiles behind them.
/// </summary>
[Tool]
public partial class Backdrop : Node2D, IPlaced, Ui.IRecoloured
{
    /// <summary>The colours of its picture shown as others, made the first time any is asked for.</summary>
    private Ui.Recolouring? _recolouring;

    /// <inheritdoc/>
    public void Recolour(string source, IReadOnlyList<Shared.ColourSwap> swaps) => (_recolouring ??= new Ui.Recolouring(this)).Recolour(source, swaps);

    /// <summary>The picture it shows as its room's content names it, with the room's set of graphics put in; empty before it is set up or with no picture.</summary>
    private string _pictureNamed = "";

    /// <inheritdoc/>
    public bool ShowsPicturesIn(string place) => _pictureNamed.Length > 0 && Ui.Recolouring.LiesIn(_pictureNamed, place);

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

    /// <inheritdoc/>
    [Export]
    public Condition? PresentWhen { get; set; }

    /// <summary>How far the picture has drifted.</summary>
    private Vector2 _drifted;

    /// <summary>The loaded picture.</summary>
    private Texture2D? _texture;

    /// <summary>The loaded picture, for a kind of backdrop that draws it its own way.</summary>
    protected Texture2D? Texture => _texture;

    /// <summary>The part of the picture that lies in front of the room's things; null for a picture with none.</summary>
    private Texture2D? _textureInFront;

    /// <summary>What draws that part, made the first time a picture has one.</summary>
    private BackdropFront? _front;

    /// <summary>The size of the room the layer fills, in pixels.</summary>
    private Vector2I _roomSize;

    /// <summary>The size of the room the layer fills, for a kind of backdrop that draws its picture its own way.</summary>
    protected Vector2I RoomSize => _roomSize;

    /// <summary>Loads the layer's picture.</summary>
    internal virtual void Setup(ContentLibrary content, Vector2I roomSize)
    {
        _roomSize = roomSize;
        _texture = Picture.Length > 0 ? content.LoadTexture(Picture) : null;
        _textureInFront = Picture.Length > 0 ? content.LoadFrontOf(Picture) : null;
        _pictureNamed = Picture.Length > 0 ? content.Named(Picture) : "";
        if (_textureInFront is not null && _front is null)
        {
            _front = new BackdropFront { Name = "Front", Of = this, ZAsRelative = false, ZIndex = RoomDepth.BackdropInFront, UseParentMaterial = true };
            AddChild(_front, forceReadableName: false, InternalMode.Back);
        }

        QueueRedraw();
        _front?.QueueRedraw();
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
        if (_texture is not null)
            Lay(this, _texture);
    }

    /// <summary>Draws, on <paramref name="front"/>, the part of the picture that lies in front of the room's things, laid as the picture is.</summary>
    internal void DrawFront(CanvasItem front)
    {
        if (_textureInFront is not null)
            Lay(front, _textureInFront);
    }

    /// <summary>Lays a picture where the layer lies: once, or over and over as far as the layer fills.</summary>
    private void Lay(CanvasItem on, Texture2D texture)
    {
        if (!Repeats)
        {
            on.DrawTexture(texture, Vector2.Zero);
            return;
        }

        // One that drifts is laid a picture's width and height further back, so its drift uncovers nothing.
        Vector2 fills = new(Fills.X > 0 ? Fills.X : _roomSize.X, Fills.Y > 0 ? Fills.Y : _roomSize.Y);
        Vector2 back = new(Drift.X != 0 ? texture.GetSize().X : 0, Drift.Y != 0 ? texture.GetSize().Y : 0);
        on.DrawTextureRect(texture, new Rect2(-back, fills + back), tile: true);
    }
}
