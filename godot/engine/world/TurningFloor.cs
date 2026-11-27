using Engine.Content;
using Engine.Actors.Components;
using Godot;

namespace Engine.World;

/// <summary>
/// A backdrop that can be turned and brought nearer or farther about a point: a floor seen from above, or a whole
/// room's picture that tilts. Whatever is placed under it as a child turns with it, so a part of the floor that
/// changes its picture is a visual under it. It is level and at its own size until it is told otherwise.
/// </summary>
[Tool]
public partial class TurningFloor : Backdrop
{
    /// <summary>The size of the floor's picture, which is how far apart its repeats lie.</summary>
    private Vector2 _extent;

    /// <summary>Loads the floor's picture and lays it level.</summary>
    internal override void Setup(ContentLibrary content, Vector2I roomSize)
    {
        base.Setup(content, roomSize);
        _extent = Picture.Length > 0 ? content.LoadTexture(Picture).GetSize() : roomSize;
        foreach (ActorVisual part in GetChildren().OfType<ActorVisual>())
            part.Bind(content);
        LayLevel();
    }

    /// <summary>Its turn places it: its share of the room's scrolling says nothing of where a background would stand.</summary>
    protected override bool FollowsAsItsShareSays => false;

    /// <summary>The floor keeps the place its turn gave it, whatever the view does and whatever shakes the room.</summary>
    internal override void Follow(Vector2I viewTopLeft, Vector2I thrown)
    {
    }

    /// <summary>Lays the floor level, at its own size, where it lies in the room (<see cref="Backdrop.LiesAt"/>).</summary>
    public void LayLevel()
    {
        Rotation = 0;
        Scale = Vector2.One;
        Position = LiesAt;
    }

    /// <summary>
    /// Shows the floor turned by <paramref name="radians"/> clockwise about <paramref name="centre"/>, a point of the
    /// floor, with <paramref name="floorPixelsPerScreenPixel"/> of the floor to each pixel shown, and moved back by
    /// <paramref name="scroll"/>.
    /// </summary>
    public void Turn(double radians, double floorPixelsPerScreenPixel, Vector2 centre, Vector2 scroll)
    {
        if (!double.IsFinite(floorPixelsPerScreenPixel) || floorPixelsPerScreenPixel <= 0)
            throw new ArgumentOutOfRangeException(nameof(floorPixelsPerScreenPixel), floorPixelsPerScreenPixel, "A floor is shown at a size above nothing.");
        float size = (float)(1 / floorPixelsPerScreenPixel);
        Rotation = (float)radians;
        Scale = new Vector2(size, size);
        // The centre stays where it is but for the scroll: the floor's corner goes wherever that leaves it, from
        // where the floor lies in the room.
        Position = LiesAt + centre - scroll - centre.Rotated((float)radians) * size;
    }

    /// <summary>Shows a part of the floor, by the name of its visual, with a picture; an empty picture shows nothing.</summary>
    public void ShowPart(string part, string picture)
    {
        ActorVisual visual = GetNodeOrNull<ActorVisual>(part)
            ?? throw new ArgumentException($"The floor \"{Name}\" has no part named \"{part}\".", nameof(part));
        visual.Show(picture);
    }

    /// <inheritdoc/>
    public override void _Draw()
    {
        if (!Repeats)
        {
            base._Draw();
            return;
        }

        // One that repeats is laid out a whole picture beyond itself each way, so a turn uncovers nothing.
        if (Texture is { } texture)
            DrawTextureRect(texture, new Rect2(-_extent, _extent * 3), tile: true);
    }
}
