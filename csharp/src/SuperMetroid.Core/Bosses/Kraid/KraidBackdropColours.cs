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
    internal void Step(KraidBackdropFade fade)
    {
        KraidBackdropSettledColours goal = GoalOf(fade);
        if (running is not null && running.Goal == goal)
        {
            running = running with { Progress = running.Progress.Then() };
            return;
        }
        StartedFrom = Shows;
        running = new RunningKraidBackdropFade(goal, StepCountedFadeProgress.Started(KraidBackgroundDefinitions.BackdropFadeLastStep));
    }

    /// <summary>Completes the fade toward <paramref name="fade"/>'s colours: the backdrop shows them from now on.</summary>
    /// <param name="fade">The colours faded toward.</param>
    internal void Complete(KraidBackdropFade fade)
    {
        StartedFrom = new KraidBackdropShown.Settled(GoalOf(fade));
        running = null;
    }

    /// <summary>The colours a fade toward <paramref name="fade"/> ends on.</summary>
    private static KraidBackdropSettledColours GoalOf(KraidBackdropFade fade) => fade switch
    {
        KraidBackdropFade.ToBlack => KraidBackdropSettledColours.Black,
        KraidBackdropFade.ToKraidColours => KraidBackdropSettledColours.KraidColours,
        _ => throw new ArgumentOutOfRangeException(nameof(fade), fade, "Kraid's backdrop fades only to black or to his colours."),
    };

    /// <inheritdoc />
    public void ShadeGameplay(GameplayShadingBuilder frame)
    {
        Ensure.NotNull(frame);
        switch (Shows)
        {
            case KraidBackdropShown.Settled { Colours: KraidBackdropSettledColours.RoomColours }:
                return;
            case KraidBackdropShown.Settled settled:
                frame.Shade(GameplayPictures.Room, ShadeOf(settled.Colours)
                    ?? throw new InvalidOperationException($"Kraid's backdrop colours {settled.Colours} have no shade."));
                return;
            case KraidBackdropShown.Fading fading:
                frame.Shade(GameplayPictures.Room, FadeOf(fading));
                return;
            default:
                throw new InvalidOperationException($"Undefined Kraid backdrop colours {Shows}.");
        }
    }

    /// <summary>The fade between the backdrop's shadings that <paramref name="fading"/> shows.</summary>
    internal static PictureShadeFade FadeOf(KraidBackdropShown.Fading fading)
    {
        Ensure.NotNull(fading);
        return new PictureShadeFade(ShadingOf(fading.From), ShadingOf(new KraidBackdropShown.Settled(fading.Goal)),
            fading.Progress.Moved, fading.Progress.Length);
    }

    /// <summary>The room pictures' shading that shows <paramref name="shown"/>.</summary>
    private static PictureShading ShadingOf(KraidBackdropShown shown) => shown switch
    {
        KraidBackdropShown.Settled settled => ShadeOf(settled.Colours) is { } shade ? PictureShading.Shaded(shade) : PictureShading.Standard,
        KraidBackdropShown.Fading fading => PictureShading.Standard.With(FadeOf(fading)),
        _ => throw new ArgumentOutOfRangeException(nameof(shown), shown, "Undefined Kraid backdrop colours."),
    };

    /// <summary>The installed shade of settled backdrop colours; none for the room's own, its pictures' standard colours.</summary>
    private static PictureShade? ShadeOf(KraidBackdropSettledColours colours) => colours switch
    {
        KraidBackdropSettledColours.RoomColours => null,
        KraidBackdropSettledColours.KraidColours => KraidShades.Backdrop,
        KraidBackdropSettledColours.Black => KraidShades.BackdropBlack,
        _ => throw new ArgumentOutOfRangeException(nameof(colours), colours, "Undefined Kraid backdrop colours."),
    };

    /// <summary>The fade that took the backdrop's latest step.</summary>
    /// <param name="Goal">The colours it ends on.</param>
    /// <param name="Progress">How far it has come.</param>
    private sealed record RunningKraidBackdropFade(KraidBackdropSettledColours Goal, StepCountedFadeProgress Progress);
}
