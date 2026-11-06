using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// A boss's body as an accepted frame latched it (#1466, #1515): the frames it shows, each over the frames before it, and
/// the body row from which it shows nothing, as the acid hides Crocomire's sinking body. The frame shows it as pictures
/// placed where they show (<see cref="Show"/>, #1514).
/// </summary>
/// <param name="Frames">The frames shown, the first beneath the others.</param>
/// <param name="HiddenFrom">The body row from which nothing shows, or null when the whole body shows.</param>
public sealed record BossBodyLayer(ImmutableArray<BossBodyFrame> Frames, int? HiddenFrom = null)
{
    /// <summary>A body showing nothing.</summary>
    public static BossBodyLayer Empty { get; } = new([]);

    /// <summary>
    /// The body as the frame shows it on an area of <paramref name="area"/>: each frame's picture over the rectangles it
    /// covers and no later frame does. Nothing shows beyond the area or from <see cref="HiddenFrom"/> down.
    /// </summary>
    public ShownBossBody Show(Extent16<BackgroundPixels> area)
    {
        var shows = BossBodyRectangle.Between(0, 0, area.X, HiddenFrom is { } hidden ? Math.Clamp(hidden, 0, area.Y) : area.Y);
        var covered = new List<BossBodyRectangle>();
        var pieces = ImmutableArray.CreateBuilder<ShownBossBodyPiece>();
        // The frontmost frame is placed first, so each rectangle shows the frontmost picture covering it.
        for (int index = Frames.Length - 1; index >= 0; index--)
        {
            BossBodyFrame frame = Frames[index];
            Place(frame.Picture, frame.Corner, frame.Covers);
        }
        return new(pieces.ToImmutable(), area);

        // Places `picture`, its top-left corner on `corner`, over what of `covers` the area shows and nothing in front covers.
        void Place(SpriteSheetFrame? picture, Position16<BackgroundPixels> corner, ImmutableArray<BossBodyRectangle> covers)
        {
            foreach (BossBodyRectangle cover in covers)
            {
                BossBodyRectangle shown = cover.Within(shows);
                if (shown.IsEmpty)
                    continue;
                if (picture is not null)
                {
                    foreach (BossBodyRectangle piece in shown.Without(covered))
                    {
                        pieces.Add(new ShownBossBodyPiece(picture, piece.Corner, piece.Size,
                            new Position16<ImagePixels>(piece.Left - corner.X, piece.Top - corner.Y)));
                    }
                }
                covered.Add(shown);
            }
        }
    }
}
