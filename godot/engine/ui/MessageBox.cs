using Godot;

namespace Engine.Ui;

/// <summary>
/// A box showing a message: a picture shown across the middle of the screen through a window of so many lines about
/// its middle. It shows; the mode that presents it decides what is shown and when it is dismissed, and it grants
/// nothing and saves nothing itself.
/// </summary>
public partial class MessageBox : CanvasLayer
{
    private Sprite2D? _picture;

    /// <summary>
    /// Shows a message's picture, the middle of it <paramref name="centreY"/> pixels down the screen, as much
    /// of it as lies within <paramref name="radius"/> lines above that line and below it: none of it while that is
    /// nothing.
    /// </summary>
    public void Present(Texture2D picture, int centreY, int radius)
    {
        ArgumentNullException.ThrowIfNull(picture);
        ArgumentOutOfRangeException.ThrowIfNegative(radius);
        if (_picture is null)
        {
            _picture = new Sprite2D { Name = "Picture", Centered = false, RegionEnabled = true, TextureFilter = CanvasItem.TextureFilterEnum.Nearest };
            AddChild(_picture);
        }

        int top = centreY - picture.GetHeight() / 2;
        int first = Math.Max(top, centreY - radius), end = Math.Min(top + picture.GetHeight(), centreY + radius);
        _picture.Visible = end > first;
        if (end <= first)
            return;
        _picture.Texture = picture;
        _picture.RegionRect = new Rect2(0, first - top, picture.GetWidth(), end - first);
        Vector2 screen = GetViewport()?.GetVisibleRect().Size ?? new Vector2(picture.GetWidth(), 0);
        _picture.Position = new Vector2(Mathf.Floor((screen.X - picture.GetWidth()) / 2), first);
    }
}
