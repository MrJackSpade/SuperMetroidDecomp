using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// One whole frame of a boss's body (#1466, #1515): a body pose, Kraid's head at a mouth stage, an eye, a mouth or a melt
/// image, as one pre-coloured picture placed on the body's area. Where it is shown it covers the rectangles it lists, so
/// whatever lies beneath it there is hidden, and a pixel of them its picture leaves clear shows nothing. Each state a room
/// animation gives a frame is a frame of its own, pictured whole (#1513).
/// </summary>
public sealed class BossBodyFrame
{
    /// <summary>Holds the frame's picture, where it lies and what it covers.</summary>
    /// <exception cref="ArgumentException">The frame has no name or covers nothing.</exception>
    public BossBodyFrame(string name, SpriteSheetFrame? picture, Position16<BackgroundPixels> corner, ImmutableArray<BossBodyRectangle> covers)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        if (covers.IsDefaultOrEmpty)
            throw new ArgumentException($"Boss body frame {name} covers nothing.", nameof(covers));
        Name = name;
        Picture = picture;
        Corner = corner;
        Covers = covers;
    }

    /// <summary>The frame's name in its sheet.</summary>
    public string Name { get; }

    /// <summary>
    /// The frame's whole picture; null when nothing it covers shows anything, so it hides what lies beneath it and shows
    /// nothing there (#1513).
    /// </summary>
    public SpriteSheetFrame? Picture { get; }

    /// <summary>The body pixel the picture's top-left corner lies on.</summary>
    public Position16<BackgroundPixels> Corner { get; }

    /// <summary>The rectangles of the body the frame covers.</summary>
    public ImmutableArray<BossBodyRectangle> Covers { get; }
}
