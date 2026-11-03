using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The colours Kraid's room backdrop shows (#1512): the room's own until he grows, then a fade into his, a fade out to
/// black as he dies and back in once his body has sunk, each taking one step of the shared gradual colour change per
/// update. The room's pictures show the import's shade of each step; the backdrop's colours never reach gameplay.
/// </summary>
internal sealed class KraidBackdropColours : IGameplayShadingSource
{
    /// <summary>What the backdrop shows.</summary>
    internal KraidBackdropShows Shows { get; private set; } = KraidBackdropShows.RoomColours;

    /// <summary>
    /// The shared counter's value at the step the running fade took last, which names its shade: step thirteen shows the
    /// colours it fades to exactly however the counter reached it.
    /// </summary>
    internal int Steps { get; private set; }

    /// <summary>Shows Kraid's backdrop colours at once, as his defeated room loads them.</summary>
    internal void ShowKraidColours()
    {
        Shows = KraidBackdropShows.KraidColours;
        Steps = 0;
    }

    /// <summary>
    /// Takes step <paramref name="step"/> of a fade toward <paramref name="fade"/>'s colours, starting the fade from what
    /// the backdrop shows when it is not already fading that way.
    /// </summary>
    /// <param name="fade">The colours faded toward.</param>
    /// <param name="step">The shared counter's value at this step, zero through thirteen.</param>
    /// <exception cref="InvalidOperationException">The backdrop shows colours the fade cannot start from.</exception>
    internal void Step(KraidBackdropFade fade, int step)
    {
        Ensure.BetweenInclusive(0, KraidShades.BackdropFadeShades - 1, step);
        KraidBackdropShows fading = (fade, Shows) switch
        {
            (KraidBackdropFade.ToRoomColors, KraidBackdropShows.RoomColours) => KraidBackdropShows.FadingInFromRoomColours,
            (KraidBackdropFade.ToRoomColors, KraidBackdropShows.Black) => KraidBackdropShows.FadingInFromBlack,
            (KraidBackdropFade.ToRoomColors, KraidBackdropShows.FadingInFromRoomColours or KraidBackdropShows.FadingInFromBlack) => Shows,
            (KraidBackdropFade.ToBlack, KraidBackdropShows.KraidColours) => KraidBackdropShows.FadingOut,
            (KraidBackdropFade.ToBlack, KraidBackdropShows.FadingOut) => Shows,
            _ => throw new InvalidOperationException($"Kraid's backdrop cannot fade {fade} while it shows {Shows}."),
        };
        Steps = step;
        Shows = fading;
    }

    /// <summary>Completes the running fade: the backdrop shows the colours it faded to.</summary>
    /// <exception cref="InvalidOperationException">No fade is running.</exception>
    internal void Complete()
    {
        Shows = Shows switch
        {
            KraidBackdropShows.FadingInFromRoomColours or KraidBackdropShows.FadingInFromBlack => KraidBackdropShows.KraidColours,
            KraidBackdropShows.FadingOut => KraidBackdropShows.Black,
            _ => throw new InvalidOperationException($"Kraid's backdrop completes a fade while it shows {Shows}."),
        };
        Steps = 0;
    }

    /// <inheritdoc />
    public void ShadeGameplay(GameplayShadingBuilder frame)
    {
        Ensure.NotNull(frame);
        switch (Shows)
        {
            case KraidBackdropShows.RoomColours:
                return;
            case KraidBackdropShows.FadingInFromRoomColours:
                frame.Shade(GameplayPictures.Room, KraidShades.BackdropFadingInFromRoomColours(Steps));
                return;
            case KraidBackdropShows.KraidColours:
                frame.Shade(GameplayPictures.Room, KraidShades.Backdrop);
                return;
            case KraidBackdropShows.FadingOut:
                frame.Shade(GameplayPictures.Room, KraidShades.BackdropFadingOut(Steps));
                return;
            case KraidBackdropShows.Black:
                frame.Shade(GameplayPictures.Room, KraidShades.BackdropFadingOut(KraidShades.BackdropFadeShades - 1));
                return;
            case KraidBackdropShows.FadingInFromBlack:
                frame.Shade(GameplayPictures.Room, KraidShades.BackdropFadingInFromBlack(Steps));
                return;
            default:
                throw new InvalidOperationException($"Undefined Kraid backdrop {Shows}.");
        }
    }
}
