using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// The pixels of a boss body frame a room animation paints (#1515), as Crocomire's melt bands paint his body and his melt
/// images: a pre-coloured picture of them for each frame of the animation, shown over the frame's own picture once the
/// room's animation has selected one. Before it has, the frame's own picture shows them as the room loads them.
/// </summary>
public sealed class BossBodyOverlay
{
    /// <summary>The pictures by animation frame name.</summary>
    private readonly IReadOnlyDictionary<string, SpriteSheetFrame> pictures;

    /// <summary>Holds the animation, where its pictures lie, what they cover and a picture for each of its frames.</summary>
    /// <exception cref="ArgumentException">The overlay covers nothing or pictures no frame.</exception>
    public BossBodyOverlay(ArtworkAnimation animation, Position16<BackgroundPixels> corner, ImmutableArray<BossBodyRectangle> covers,
        IReadOnlyDictionary<string, SpriteSheetFrame> pictures)
    {
        Animation = Ensure.NotNull(animation);
        if (covers.IsDefaultOrEmpty)
            throw new ArgumentException($"The overlay of {animation.Name} covers nothing.", nameof(covers));
        if (Ensure.NotNull(pictures).Count == 0)
            throw new ArgumentException($"The overlay of {animation.Name} pictures none of its frames.", nameof(pictures));
        Corner = corner;
        Covers = covers;
        this.pictures = pictures;
    }

    /// <summary>The animation whose selected frame the overlay shows.</summary>
    public ArtworkAnimation Animation { get; }

    /// <summary>The body pixel each picture's top-left corner lies on.</summary>
    public Position16<BackgroundPixels> Corner { get; }

    /// <summary>The rectangles of the body the animation paints, which its picture covers.</summary>
    public ImmutableArray<BossBodyRectangle> Covers { get; }

    /// <summary>
    /// The picture of the frame <paramref name="selections"/> hold for the animation, or null before the room's animation
    /// has selected one.
    /// </summary>
    /// <exception cref="InvalidDataException">The selected frame is not pictured.</exception>
    public SpriteSheetFrame? Shown(ArtworkFrameSelections selections)
    {
        if (!Ensure.NotNull(selections).TryShown(Animation, out ArtworkFrame frame))
            return null;
        return pictures.TryGetValue(frame.Name, out SpriteSheetFrame? shown) ? shown
            : throw new InvalidDataException($"A boss body overlay of {Animation.Name} pictures no frame {frame.Name}.");
    }
}
