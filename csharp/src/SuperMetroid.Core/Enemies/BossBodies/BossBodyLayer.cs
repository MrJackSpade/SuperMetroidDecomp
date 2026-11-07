using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

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
    /// The body as the frame shows it on an area of <paramref name="area"/>: each frame's picture, in draw order, cut where
    /// the area ends and at <see cref="ShownAbove"/>. A frame showing nothing draws no part.
    /// </summary>
    public ShownBossBody Show(Extent16<BackgroundPixels> area)
    {
        int bottom = ShownAbove is { } shownAbove ? Math.Clamp(shownAbove, 0, area.Y) : area.Y;
        var parts = ImmutableArray.CreateBuilder<ShownBossBodyPart>(Frames.Length);
        foreach (BossBodyFrame frame in Frames)
        {
            if (frame.Picture is not { } picture)
                continue;
            int width = Math.Min(picture.Size.X, area.X - frame.Corner.X);
            int height = Math.Min(picture.Size.Y, bottom - frame.Corner.Y);
            if (width > 0 && height > 0)
                parts.Add(new ShownBossBodyPart(picture, frame.Corner, new Extent16<BackgroundPixels>(width, height)));
        }
        return new(parts.ToImmutable(), area);
    }
}
