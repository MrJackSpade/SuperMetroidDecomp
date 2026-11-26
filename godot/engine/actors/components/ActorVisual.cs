using Engine.Content;
using Engine.Shared;
using Godot;

namespace Engine.Actors.Components;

/// <summary>
/// Shows an actor: the picture gameplay selects, which way it faces, whether it is flashing or tinted. It only shows.
/// Nothing here advances gameplay, and gameplay never reads anything back from it.
/// </summary>
[Tool]
public partial class ActorVisual : Node2D, Ui.IRecoloured
{
    /// <summary>The colours of its pictures shown as others, made the first time any is asked for.</summary>
    private Ui.Recolouring? _recolouring;

    /// <inheritdoc/>
    public void Recolour(string source, IReadOnlyList<Shared.ColourSwap> swaps) => (_recolouring ??= new Ui.Recolouring(this)).Recolour(source, swaps);

    /// <summary>Whether the picture it is showing is at <paramref name="place"/> or in the folder that names.</summary>
    public bool ShowsPicturesIn(string place) =>
        Picture.Length > 0 && _content is { } content && Ui.Recolouring.LiesIn(content.Named(Picture), place);

    /// <summary>The picture shown before gameplay selects one, as a path within the assets tree; empty for none.</summary>
    [Export(PropertyHint.File)]
    public string Picture { get; set; } = "";

    /// <summary>Where the picture's top-left corner lies from the actor's origin, in whole pixels.</summary>
    [Export]
    public Vector2I PictureOffset { get; set; }

    /// <summary>Where the step of a timeline being shown puts its picture, beyond <see cref="PictureOffset"/>.</summary>
    private Vector2I _stepOffset;

    /// <summary>Where pictures are loaded from.</summary>
    private ContentLibrary? _content;

    /// <summary>The loaded picture being shown.</summary>
    private Texture2D? _texture;

    /// <summary>Whether the picture is mirrored left to right.</summary>
    private bool _mirrored;

    /// <summary>Which row of the picture shows at each row down from its top, when it is shown row by row.</summary>
    private IReadOnlyList<int?>? _rows;

    /// <summary>
    /// Gives the visual where to load pictures from, and loads from there the picture it shows. What it shows and
    /// where is left as it is: a visual that goes on from one room to the next, as the player's does through a door,
    /// is bound again to the next room's library with a timeline's step still showing.
    /// </summary>
    public void Bind(ContentLibrary content)
    {
        _content = content;
        Load();
    }

    /// <summary>Shows the picture at <paramref name="path"/>; empty shows nothing.</summary>
    public void Show(string path)
    {
        _stepOffset = Vector2I.Zero;
        Picture = path;
        Load();
    }

    /// <summary>Loads the picture it shows from its library, and has it drawn again.</summary>
    private void Load()
    {
        _texture = Picture.Length > 0 && _content is not null ? _content.LoadTexture(Picture) : null;
        QueueRedraw();
    }

    /// <summary>Shows the picture of a timeline's step, where the step puts it.</summary>
    public void Show(TimelineStep step)
    {
        ArgumentNullException.ThrowIfNull(step);
        Show(step.Picture);
        _stepOffset = new Vector2I(step.Offset.X, step.Offset.Y);
    }

    /// <summary>Mirrors the picture left to right, or not.</summary>
    public void Mirror(bool mirrored)
    {
        _mirrored = mirrored;
        QueueRedraw();
    }

    /// <summary>
    /// Shows the picture row by row: at each row down from its top, the row of the picture named there, or nothing.
    /// For something that crumbles or falls apart. Null shows the picture whole again.
    /// </summary>
    public void ShowRows(IReadOnlyList<int?>? rows)
    {
        _rows = rows;
        QueueRedraw();
    }

    /// <summary>Tints the picture; white leaves it as it is.</summary>
    public void Tint(Color colour) => Modulate = colour;

    /// <summary>Shows or hides the whole visual, as a flash does on alternate updates.</summary>
    public void SetShown(bool shown) => Visible = shown;

    /// <inheritdoc/>
    public override void _Ready()
    {
        // In the editor a scene shows its real art through the same path the game uses.
        if (Godot.Engine.IsEditorHint() && Picture.Length > 0 && _content is null)
            Bind(ContentLibrary.ForProject());
    }

    /// <inheritdoc/>
    public override void _Draw()
    {
        if (_texture is null)
            return;
        var size = _texture.GetSize();
        Vector2I offset = PictureOffset + _stepOffset;
        if (_rows is { } rows)
        {
            for (int row = 0; row < rows.Count && row < (int)size.Y; row++)
            {
                if (rows[row] is not { } shown)
                    continue;
                var from = new Rect2(0, shown, size.X, 1);
                Rect2 at = _mirrored
                    ? new Rect2(-offset.X, offset.Y + row, -size.X, 1)
                    : new Rect2(offset.X, offset.Y + row, size.X, 1);
                DrawTextureRectRegion(_texture, at, from);
            }

            return;
        }

        var area = new Rect2(offset, size);
        if (_mirrored)
            area = new Rect2(new Vector2(-offset.X, offset.Y), new Vector2(-size.X, size.Y));
        DrawTextureRect(_texture, area, tile: false);
    }
}
