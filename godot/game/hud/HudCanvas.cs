using Godot;

namespace Game;

/// <summary>What the HUD draws on: it shows the pictures it is given, each at its place, and nothing else.</summary>
public partial class HudCanvas : Node2D
{
    private IReadOnlyList<(Texture2D Picture, Vector2I At, Color Tint)> _pictures = [];
    private IReadOnlyList<(Rect2I Area, Color Colour)> _marks = [];

    /// <summary>Shows these pictures, in order, and these marks over them.</summary>
    public void Show(IReadOnlyList<(Texture2D Picture, Vector2I At, Color Tint)> pictures, IReadOnlyList<(Rect2I Area, Color Colour)> marks)
    {
        ArgumentNullException.ThrowIfNull(pictures);
        ArgumentNullException.ThrowIfNull(marks);
        (_pictures, _marks) = (pictures, marks);
        QueueRedraw();
    }

    /// <inheritdoc/>
    public override void _Draw()
    {
        foreach ((Texture2D picture, Vector2I at, Color tint) in _pictures)
            DrawTexture(picture, at, tint);
        foreach ((Rect2I area, Color colour) in _marks)
            DrawRect(area, colour);
    }
}
