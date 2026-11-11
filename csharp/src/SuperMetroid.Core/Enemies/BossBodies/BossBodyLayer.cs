using System.Collections.Immutable;

namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// A boss's body as an accepted frame latched it (#1466, #1515, #1540): the frame each of its parts shows, in the order the
/// parts are drawn, and the body row above which alone it shows, as the acid hides Crocomire's sinking body. The frame
/// shows it as its parts' pictures, layered where they lie (<see cref="Show"/>, #1514).
/// </summary>
/// <param name="Frames">The frames shown, the first drawn beneath the others.</param>
/// <param name="ShownAbove">The body row above which alone the body shows, or null when the whole body shows.</param>
public sealed record BossBodyLayer(ImmutableArray<BossBodyFrame> Frames, int? ShownAbove = null)
{
    /// <summary>A body showing nothing.</summary>
    public static BossBodyLayer Empty { get; } = new([]);

    /// <summary>
    /// The body as the frame shows it: each frame's picture, in draw order, cut at <see cref="ShownAbove"/>. A frame showing
    /// nothing draws no part.
    /// </summary>
    public ShownBossBody Show()
    {
        var parts = ImmutableArray.CreateBuilder<ShownBossBodyPart>(Frames.Length);
        foreach (BossBodyFrame frame in Frames)
        {
            if (frame.Picture is not { } picture)
                continue;
            int height = ShownAbove is { } shownAbove
                ? Math.Min(picture.Size.Y, Math.Max(shownAbove, 0) - frame.Corner.Y)
                : picture.Size.Y;
            if (height > 0)
                parts.Add(new ShownBossBodyPart(picture, frame.Corner, new Extent16<BackgroundPixels>(picture.Size.X, height)));
        }
        return new(parts.ToImmutable());
    }
}
