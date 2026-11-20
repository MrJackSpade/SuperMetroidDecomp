using Engine.Shared;
using Godot;

namespace Engine.World;

/// <summary>How a region node's rectangle is worked out and drawn: its size, from where the node stands in the room.</summary>
internal static class Region
{
    /// <summary>The rectangle of the room a region node of <paramref name="size"/> covers, its top-left where the node stands.</summary>
    public static PixelRect Of(Node2D node, Vector2I size) =>
        new((int)MathF.Round(node.Position.X), (int)MathF.Round(node.Position.Y), size.X, size.Y);

    /// <summary>Draws a region's outline in the editor.</summary>
    public static void DrawInEditor(Node2D node, Vector2I size, Color colour)
    {
        if (Godot.Engine.IsEditorHint())
            node.DrawRect(new Rect2(Vector2.Zero, size), colour, filled: false);
    }
}
