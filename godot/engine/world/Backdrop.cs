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

    /// <summary>
    /// Adds a colour to, or takes one from, every colour of its picture, in place of what <paramref name="source"/>
    /// last asked; <see cref="Shared.ColourShift.None"/> takes it back.
    /// </summary>
    public void Shift(string source, Shared.ColourShift shift) => (_recolouring ??= new Ui.Recolouring(this)).Shift(source, shift);

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

    /// <inheritdoc/>
    [Export]
    public Condition? PresentWhen { get; set; }

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

    /// <summary>
    /// Whether its share of the room's scrolling is how its room's background follows the view. A kind that places
    /// its picture some other way says no: it neither keeps the last room's place nor gives the next room one.
    /// </summary>
    protected virtual bool FollowsAsItsShareSays => true;

    /// <summary>
    /// How far back its picture is laid along each axis, in pixels, less than the picture's size: along an axis it
    /// follows the view on not at all, the place the backdrop of the room last left had there.
    /// </summary>
    private Vector2I _keptBack;

    /// <summary>
    /// Has the layer keep, along each axis it follows the view on not at all, the place the backdrop of the room last
    /// left had: <paramref name="kept"/> is the pixel of that backdrop that lay at the view's top-left corner, and
    /// the same pixel of this one's picture lies there. Only a picture that repeats keeps a place, round its own size.
    /// Told by the room once the picture is loaded.
    /// </summary>
    internal void KeepPlace(Vector2I kept)
    {
        if (!FollowsAsItsShareSays || !Repeats || _texture is null)
            return;
        Vector2 size = _texture.GetSize();
        _keptBack = new Vector2I(
            ScrollShare.X == 0 ? Mathf.PosMod(kept.X, (int)size.X) : 0,
            ScrollShare.Y == 0 ? Mathf.PosMod(kept.Y, (int)size.Y) : 0);
        QueueRedraw();
        _front?.QueueRedraw();
    }

    /// <summary>
    /// The pixel of the layer's picture at the top-left corner of a view at <paramref name="viewTopLeft"/>, along each
    /// axis: its share of the view's place, what is less than a pixel dropped, or <paramref name="kept"/>, the place
    /// the room before left, along an axis it follows the view on not at all. What the room gives the next room as
    /// it is left. A kind that places its picture some other way hands on what it was given.
    /// </summary>
    internal Vector2I PlaceAt(Vector2I viewTopLeft, Vector2I kept) => !FollowsAsItsShareSays
        ? kept
        : new Vector2I(
            ScrollShare.X == 0 ? kept.X : Mathf.FloorToInt(viewTopLeft.X * ScrollShare.X),
            ScrollShare.Y == 0 ? kept.Y : Mathf.FloorToInt(viewTopLeft.Y * ScrollShare.Y));

    /// <summary>
    /// Waves the layer's rows as <paramref name="wave"/> says, or not at all for null: what a room's
    /// <see cref="BackdropWaveEffect"/> asks of what lies behind the room. Only a layer that follows the view as its
    /// share says is waved.
    /// </summary>
    internal void ShowWave(Ui.RowWave? wave)
    {
        if (!FollowsAsItsShareSays || (wave is null && _recolouring is null))
            return;
        (_recolouring ??= new Ui.Recolouring(this)).Wave(wave);
    }

    /// <summary>Places the layer for a view whose top-left corner is at <paramref name="viewTopLeft"/> in the room.</summary>
    internal virtual void Follow(Vector2I viewTopLeft, Vector2I thrown)
    {
        Position = new Vector2(
            LiesAt.X + viewTopLeft.X * (1 - ScrollShare.X) + thrown.X - _keptBack.X,
            LiesAt.Y + viewTopLeft.Y * (1 - ScrollShare.Y) + thrown.Y - _keptBack.Y);
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

        // Laid back to keep the last room's place, it is laid that much further on, to fill the room all the same.
        on.DrawTextureRect(texture, new Rect2(Vector2.Zero, _roomSize + _keptBack), tile: true);
    }
}
