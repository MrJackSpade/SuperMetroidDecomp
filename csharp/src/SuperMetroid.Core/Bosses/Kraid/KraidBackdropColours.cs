using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Kraid.Artwork;

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

    /// <summary>What a fade to black began from, which names the colours each of its steps shows.</summary>
    private KraidBackdropShows fadingOutFrom = KraidBackdropShows.KraidColours;

    /// <summary>
    /// The counter value that fade began at, from the room's colours, or the growth fade's last step it began after; the
    /// fade from Kraid's colours always begins at zero.
    /// </summary>
    private int fadingOutOrigin;

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
        KraidBackdropShows fading = fade switch
        {
            KraidBackdropFade.ToRoomColors => Shows switch
            {
                KraidBackdropShows.RoomColours => KraidBackdropShows.FadingInFromRoomColours,
                KraidBackdropShows.Black => KraidBackdropShows.FadingInFromBlack,
                KraidBackdropShows.FadingInFromRoomColours or KraidBackdropShows.FadingInFromBlack => Shows,
                KraidBackdropShows.KraidColours or KraidBackdropShows.FadingOut => throw CannotFade(fade),
                _ => throw CannotFade(fade),
            },
            // The death fade takes the backdrop toward black from whatever it shows: Kraid's colours after his growth, the
            // room's when he dies before his growth fades the backdrop in, or the growth fade's colours when he dies during it.
            KraidBackdropFade.ToBlack => Shows switch
            {
                KraidBackdropShows.KraidColours or KraidBackdropShows.RoomColours or KraidBackdropShows.FadingInFromRoomColours =>
                    BeginFadingOut(step),
                KraidBackdropShows.FadingOut => Shows,
                KraidBackdropShows.Black or KraidBackdropShows.FadingInFromBlack => throw CannotFade(fade),
                _ => throw CannotFade(fade),
            },
            _ => throw new ArgumentOutOfRangeException(nameof(fade), fade, "Kraid's backdrop fades only to black or to the room's colours."),
        };
        Steps = step;
        Shows = fading;
    }

    /// <summary>Starts a fade to black at counter value <paramref name="step"/> from what the backdrop shows.</summary>
    private KraidBackdropShows BeginFadingOut(int step)
    {
        fadingOutFrom = Shows;
        fadingOutOrigin = Shows switch
        {
            KraidBackdropShows.RoomColours => step,
            KraidBackdropShows.FadingInFromRoomColours => Steps,
            KraidBackdropShows.KraidColours => 0,
            KraidBackdropShows.FadingOut or KraidBackdropShows.Black or KraidBackdropShows.FadingInFromBlack =>
                throw new InvalidOperationException($"Kraid's backdrop cannot begin a fade to black while it shows {Shows}."),
            _ => throw new InvalidOperationException($"Undefined Kraid backdrop {Shows}."),
        };
        return KraidBackdropShows.FadingOut;
    }

    /// <summary>The shade of the fade to black's step at counter value <paramref name="value"/>.</summary>
    private PictureShade FadingOutShade(int value) => fadingOutFrom switch
    {
        KraidBackdropShows.RoomColours => KraidShades.BackdropFadingOutFromRoomColours(fadingOutOrigin, value),
        KraidBackdropShows.FadingInFromRoomColours => KraidShades.BackdropFadingOutFromFadingIn(fadingOutOrigin, value),
        KraidBackdropShows.KraidColours => KraidShades.BackdropFadingOut(value),
        KraidBackdropShows.FadingOut or KraidBackdropShows.Black or KraidBackdropShows.FadingInFromBlack =>
            throw new InvalidOperationException($"Kraid's backdrop fades to black from {fadingOutFrom}, which no fade to black starts from."),
        _ => throw new InvalidOperationException($"Undefined Kraid backdrop {fadingOutFrom}."),
    };

    /// <summary>Completes the running fade: the backdrop shows the colours it faded to.</summary>
    /// <exception cref="InvalidOperationException">No fade is running.</exception>
    internal void Complete()
    {
        Shows = Shows switch
        {
            KraidBackdropShows.FadingInFromRoomColours or KraidBackdropShows.FadingInFromBlack => KraidBackdropShows.KraidColours,
            KraidBackdropShows.FadingOut => KraidBackdropShows.Black,
            KraidBackdropShows.RoomColours or KraidBackdropShows.KraidColours or KraidBackdropShows.Black =>
                throw new InvalidOperationException($"Kraid's backdrop completes a fade while it shows {Shows}."),
            _ => throw new InvalidOperationException($"Kraid's backdrop completes a fade while it shows {Shows}."),
        };
        Steps = 0;
    }

    /// <summary>The error for a fade toward <paramref name="fade"/>'s colours the backdrop cannot start from what it shows.</summary>
    private InvalidOperationException CannotFade(KraidBackdropFade fade) =>
        new($"Kraid's backdrop cannot fade {fade} while it shows {Shows}.");

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
                frame.Shade(GameplayPictures.Room, FadingOutShade(Steps));
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
