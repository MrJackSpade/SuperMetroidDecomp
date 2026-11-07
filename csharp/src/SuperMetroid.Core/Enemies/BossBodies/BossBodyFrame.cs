using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// One whole frame a part of a boss shows on its body (#1466, #1515, #1540): a body pose, Kraid's head at a mouth stage, an
/// eye, a mouth or a melt image, as one pre-coloured picture placed on the body's area. The part showing it draws the
/// picture over the parts drawn before it, and a pixel the picture leaves clear shows them. Each state a room animation
/// gives a frame is a frame of its own, pictured whole (#1513).
/// </summary>
public sealed class BossBodyFrame
{
    /// <summary>Holds the frame's picture and where it lies.</summary>
    /// <exception cref="ArgumentException">The frame has no name.</exception>
    public BossBodyFrame(string name, SpriteSheetFrame? picture, Position16<BackgroundPixels> corner)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        Name = name;
        Picture = picture;
        Corner = corner;
    }

    /// <summary>The frame's name in its sheet.</summary>
    public string Name { get; }

    /// <summary>
    /// The frame's whole picture; null when the frame shows nothing, as a melt image the dissolve has erased whole does: the
    /// part showing it is not drawn (#1513, #1540).
    /// </summary>
    public SpriteSheetFrame? Picture { get; }

    /// <summary>The body pixel the picture's top-left corner lies on.</summary>
    public Position16<BackgroundPixels> Corner { get; }
}
