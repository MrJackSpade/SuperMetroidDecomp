using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The colours Kraid's room backdrop shows (#1512, #2215). Each fade takes the backdrop toward its goal from whatever the
/// backdrop shows when it starts, a fade still running among them: his growth fades the room's colours into his, his
/// death fades the backdrop to black, and once his body has sunk the backdrop fades back into his colours. A fade is
/// published as a typed fade between the backdrop's shadings, its progress the fade's own steps (#2302); a completed
/// fade shows its goal. The backdrop's colours never reach gameplay; only when a fade completes does, through the shared
/// counter.
/// </summary>
internal sealed class KraidBackdropColours : IGameplayShadingSource
{
    /// <summary>What the backdrop showed when no fade was running, or when the running fade started.</summary>
    private KraidBackdropShown StartedFrom { get; set; } = new KraidBackdropShown.Settled(KraidBackdropSettledColours.RoomColours);

    /// <summary>The fade that took the latest step, until a fade completes or the backdrop is shown outright.</summary>
    private RunningKraidBackdropFade? running;

    /// <summary>What the backdrop shows.</summary>
    private KraidBackdropShown Shows => running is null
        ? StartedFrom
        : KraidBackdropShown.Fade(StartedFrom, running.Goal, running.Progress);

    /// <summary>Shows Kraid's backdrop colours at once, as his defeated room loads them.</summary>
    internal void ShowKraidColours()
    {
        StartedFrom = new KraidBackdropShown.Settled(KraidBackdropSettledColours.KraidColours);
        running = null;
    }

    /// <summary>
    /// Takes a step of a fade toward <paramref name="fade"/>'s colours: the running fade's next step when it fades that
    /// way, one more of its parts, otherwise the first step of a fade from what the backdrop shows, which shows it unmoved.
    /// A fade moves in one part for each moving value a fade alone on the shared counter takes
    /// (<see cref="KraidBackgroundDefinitions.BackdropFadeLastStep"/>), whatever values the counter takes (#2302).
    /// </summary>
    /// <param name="fade">The colours faded toward.</param>
    internal void Step(KraidBackdropSettledColours fade)
    {
        if (running is not null && running.Goal == fade)
        {
            running = running with { Progress = running.Progress.Then() };
            return;
        }
        StartedFrom = Shows;
        running = new RunningKraidBackdropFade(fade, StepCountedFadeProgress.Started(KraidBackgroundDefinitions.BackdropFadeLastStep));
    }

    /// <summary>Completes the fade toward <paramref name="fade"/>'s colours: the backdrop shows them from now on.</summary>
    /// <param name="fade">The colours faded toward.</param>
    internal void Complete(KraidBackdropSettledColours fade)
    {
        StartedFrom = new KraidBackdropShown.Settled(fade);
        running = null;
    }

    /// <inheritdoc />
    public void ShadeGameplay(GameplayShadingBuilder frame)
    {
        Ensure.NotNull(frame);
        // The room's own colours are its pictures' standard colours, which no shade changes.
        Shows.React(
            static (settled, shaded) =>
            {
                if (ShadeOf(settled.Colours) is { } shade)
                    shaded.Shade(GameplayPictures.Room, shade);
            },
            static (fading, shaded) => shaded.Shade(GameplayPictures.Room, FadeOf(fading)),
            frame);
    }

    /// <summary>The fade between the backdrop's shadings that <paramref name="fading"/> shows.</summary>
    internal static PictureShadeFade FadeOf(KraidBackdropShown.Fading fading)
    {
        Ensure.NotNull(fading);
        return new PictureShadeFade(ShadingOf(fading.From), ShadingOf(new KraidBackdropShown.Settled(fading.Goal)),
            fading.Progress.Moved, fading.Progress.Length);
    }

    /// <summary>The room pictures' shading that shows <paramref name="shown"/>.</summary>
    private static PictureShading ShadingOf(KraidBackdropShown shown) => shown.Match(
        static settled => ShadeOf(settled.Colours) is { } shade ? PictureShading.Shaded(shade) : PictureShading.Standard,
        static fading => PictureShading.Standard.With(FadeOf(fading)));

    /// <summary>The shade of each colours.</summary>
    private static IdentityTable<KraidBackdropSettledColours, PictureShade?> ShadeByColours => field ??= new(new Dictionary<KraidBackdropSettledColours, PictureShade?>
    {
        [KraidBackdropSettledColours.RoomColours] = null,
        [KraidBackdropSettledColours.KraidColours] = KraidShades.Backdrop,
        [KraidBackdropSettledColours.Black] = KraidShades.BackdropBlack,
    });

    /// <summary>The installed shade of settled backdrop colours; none for the room's own, its pictures' standard colours.</summary>
    private static PictureShade? ShadeOf(KraidBackdropSettledColours colours) => ShadeByColours[colours];

    /// <summary>The fade that took the backdrop's latest step.</summary>
    /// <param name="Goal">The colours it ends on.</param>
    /// <param name="Progress">How far it has come.</param>
    private sealed record RunningKraidBackdropFade(KraidBackdropSettledColours Goal, StepCountedFadeProgress Progress);
}
