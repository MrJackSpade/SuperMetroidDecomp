using Engine.Content;
using Godot;

namespace Engine.Actors.Components;

/// <summary>
/// Shows an actor: the picture gameplay selects, which way it faces, whether it is flashing or tinted. It only shows.
/// Nothing here advances gameplay, and gameplay never reads anything back from it.
/// </summary>
[Tool]
public partial class ActorVisual : Node2D
{
    /// <summary>The picture shown before gameplay selects one, as a path within the assets tree; empty for none.</summary>
    [Export(PropertyHint.File)]
    public string Picture { get; set; } = "";

    /// <summary>Where the picture's top-left corner lies from the actor's origin, in whole pixels.</summary>
    [Export]
    public Vector2I PictureOffset { get; set; }

    /// <summary>Where pictures are loaded from.</summary>
    private ContentLibrary? _content;

    /// <summary>The loaded picture being shown.</summary>
    private Texture2D? _texture;

    /// <summary>Whether the picture is mirrored left to right.</summary>
    private bool _mirrored;

    /// <summary>Gives the visual where to load pictures from and shows its starting picture.</summary>
    public void Bind(ContentLibrary content)
    {
        _content = content;
        Show(Picture);
    }

    /// <summary>Shows the picture at <paramref name="path"/>; empty shows nothing.</summary>
    public void Show(string path)
    {
        Picture = path;
        _texture = path.Length > 0 && _content is not null ? _content.LoadTexture(path) : null;
        QueueRedraw();
    }

    /// <summary>Mirrors the picture left to right, or not.</summary>
    public void Mirror(bool mirrored)
    {
        _mirrored = mirrored;
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
        var area = new Rect2(PictureOffset, size);
        if (_mirrored)
            area = new Rect2(new Vector2(-PictureOffset.X, PictureOffset.Y), new Vector2(-size.X, size.Y));
        DrawTextureRect(_texture, area, tile: false);
    }
}
