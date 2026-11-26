using Godot;

namespace Engine.Shared;

/// <summary>An exact place in a room: how far right of and how far below the room's top-left corner.</summary>
/// <param name="X">The distance right of the room's left edge.</param>
/// <param name="Y">The distance below the room's top edge.</param>
public readonly record struct PixelPosition(Pixels X, Pixels Y)
{
    /// <summary>The room's top-left corner.</summary>
    public static PixelPosition Origin => default;

    /// <summary>A place on a whole pixel.</summary>
    public static PixelPosition FromWhole(int x, int y) => new(Pixels.FromWhole(x), Pixels.FromWhole(y));

    /// <summary>The place <paramref name="offset"/> away from <paramref name="position"/>.</summary>
    public static PixelPosition operator +(PixelPosition position, PixelOffset offset) =>
        new(position.X + offset.X, position.Y + offset.Y);

    /// <summary>The place <paramref name="offset"/> back from <paramref name="position"/>.</summary>
    public static PixelPosition operator -(PixelPosition position, PixelOffset offset) =>
        new(position.X - offset.X, position.Y - offset.Y);

    /// <summary>How far, and which way, <paramref name="to"/> lies from <paramref name="from"/>.</summary>
    public static PixelOffset operator -(PixelPosition to, PixelPosition from) => new(to.X - from.X, to.Y - from.Y);

    /// <summary>
    /// The place as Godot coordinates, for drawing only: the pixel it lies in. A thing is shown at its whole pixel, as
    /// the cartridge shows it. The fraction of a pixel is for moving: shown, it would put a picture a pixel further
    /// along whenever it is a half or more. Nothing in gameplay reads this back.
    /// </summary>
    public Vector2 ToDisplay() => new(X.Whole, Y.Whole);
}
