using Engine.Content;
using Engine.Shared;
using Godot;

namespace Engine.World;

/// <summary>
/// A room-wide condition that shows as a picture laid over the whole view, tiled and drifting: fog, rain, falling
/// spores, the shimmer of heat. Like any room effect it may also do ongoing damage. It affects play in no other way.
/// </summary>
public partial class OverlayEffect : RoomEffect
{
    /// <summary>The picture, as a path within the assets tree; it is tiled across the view.</summary>
    [Export(PropertyHint.File)]
    public string Picture { get; set; } = "";

    /// <summary>How far the picture drifts each update, in pixels across and down.</summary>
    [Export]
    public Vector2 Drift { get; set; }

    /// <summary>How much of the picture's own drift the camera's movement adds, from none (it stays with the screen) to all (it stays with the room).</summary>
    [Export]
    public float FollowsRoom { get; set; } = 1f;

    /// <summary>How solid it shows, from nothing to wholly.</summary>
    [Export]
    public float Opacity { get; set; } = 0.5f;

    private Texture2D? _texture;
    private Vector2 _drifted;
    private PixelRect _view;

    /// <inheritdoc/>
    protected override void OnSetup(Room room)
    {
        _texture = Picture.Length > 0 ? room.Content.LoadTexture(Picture) : null;
        _drifted = Vector2.Zero;
        ZIndex = 100;
    }

    /// <inheritdoc/>
    protected override void OnStep(Room room)
    {
        _drifted += Drift;
        _view = room.Session.Camera.View;
        QueueRedraw();
    }

    /// <inheritdoc/>
    public override void _Draw()
    {
        if (_texture is null || !IsActive)
            return;
        Vector2 size = _texture.GetSize();
        if (size.X <= 0 || size.Y <= 0)
            return;

        // Tiles cover the view from the first one that reaches into it.
        float startX = _view.Left - Wrap(_view.Left * FollowsRoom - _drifted.X, size.X);
        float startY = _view.Top - Wrap(_view.Top * FollowsRoom - _drifted.Y, size.Y);
        var tint = new Color(1f, 1f, 1f, Opacity);
        for (float y = startY; y < _view.Top + _view.Height; y += size.Y)
        {
            for (float x = startX; x < _view.Left + _view.Width; x += size.X)
                DrawTexture(_texture, ToLocal(new Vector2(x, y)), tint);
        }

        static float Wrap(float value, float period) => ((value % period) + period) % period;
    }
}
