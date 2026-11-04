using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Kraid.Artwork;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The colours Kraid's room backdrop shows (#1512). Each fade takes the backdrop toward its goal, one step of the shared
/// gradual colour change per update, from whatever the backdrop shows when it starts, a fade still running among them:
/// his growth fades the room's colours into his, his death fades the backdrop to black, and once his body has sunk the
/// backdrop fades back into his colours. The room's pictures show the import's shade of what the backdrop shows; the
/// backdrop's colours never reach gameplay.
/// </summary>
internal sealed class KraidBackdropColours : IGameplayShadingSource
{
    /// <summary>What the backdrop showed when no fade was running, or when the running fade started.</summary>
    private KraidBackdropShown StartedFrom { get; set; } = new KraidBackdropShown.Settled(KraidBackdropSettledColours.RoomColours);

    /// <summary>The fade that took the latest step, until a fade completes or the backdrop is shown outright.</summary>
    private RunningKraidBackdropFade? running;

    /// <summary>What the backdrop shows.</summary>
    internal KraidBackdropShown Shows => running is null
        ? StartedFrom
        : KraidBackdropShown.Fade(StartedFrom, running.Goal, running.First, running.Reached);

    /// <summary>Shows Kraid's backdrop colours at once, as his defeated room loads them.</summary>
    internal void ShowKraidColours()
    {
        StartedFrom = new KraidBackdropShown.Settled(KraidBackdropSettledColours.KraidColours);
        running = null;
    }

    /// <summary>
    /// Takes the step at counter value <paramref name="step"/> of a fade toward <paramref name="fade"/>'s colours: the
    /// running fade's next step when it fades that way, otherwise the first step of a fade from what the backdrop shows.
    /// </summary>
    /// <param name="fade">The colours faded toward.</param>
    /// <param name="step">The shared counter's value at this step, zero through the last moving value.</param>
    /// <exception cref="InvalidOperationException">The running fade's counter skipped a value.</exception>
    internal void Step(KraidBackdropFade fade, int step)
    {
        Ensure.BetweenInclusive(0, KraidBackgroundDefinitions.BackdropFadeLastStep, step);
        KraidBackdropSettledColours goal = GoalOf(fade);
        if (running is not null && running.Goal == goal)
        {
            if (step != running.Reached + 1)
                throw new InvalidOperationException(
                    $"Kraid's backdrop fade toward {goal} took its step at counter {running.Reached}, then one at {step}.");
            running = running with { Reached = step };
            return;
        }
        StartedFrom = Shows;
        running = new RunningKraidBackdropFade(goal, step, step);
    }

    /// <summary>
    /// Completes the fade toward <paramref name="fade"/>'s colours: a fade that stepped through the last moving value
    /// shows its goal, and one that took no step (the shared counter had passed its last value when it started) leaves
    /// the backdrop showing what it showed.
    /// </summary>
    /// <param name="fade">The colours faded toward.</param>
    /// <exception cref="InvalidOperationException">The fade stepped but stopped short of its goal.</exception>
    internal void Complete(KraidBackdropFade fade)
    {
        KraidBackdropSettledColours goal = GoalOf(fade);
        KraidBackdropShown shown = Shows;
        if (running is not null && running.Goal == goal && shown != new KraidBackdropShown.Settled(goal))
            throw new InvalidOperationException($"Kraid's backdrop fade toward {goal} completes showing {shown}.");
        StartedFrom = shown;
        running = null;
    }

    /// <summary>The colours a fade toward <paramref name="fade"/> ends on.</summary>
    private static KraidBackdropSettledColours GoalOf(KraidBackdropFade fade) => fade switch
    {
        KraidBackdropFade.ToBlack => KraidBackdropSettledColours.Black,
        KraidBackdropFade.ToRoomColors => KraidBackdropSettledColours.KraidColours,
        _ => throw new ArgumentOutOfRangeException(nameof(fade), fade, "Kraid's backdrop fades only to black or to the room's colours."),
    };

    /// <inheritdoc />
    public void ShadeGameplay(GameplayShadingBuilder frame)
    {
        Ensure.NotNull(frame);
        KraidBackdropShown shown = Shows;
        if (shown == new KraidBackdropShown.Settled(KraidBackdropSettledColours.RoomColours))
            return;
        frame.Shade(GameplayPictures.Room, KraidShades.BackdropShowing(shown));
    }

    /// <summary>The fade that took the backdrop's latest step.</summary>
    /// <param name="Goal">The colours it ends on.</param>
    /// <param name="First">The shared counter's value at its first step.</param>
    /// <param name="Reached">The counter's value at its latest step.</param>
    private sealed record RunningKraidBackdropFade(KraidBackdropSettledColours Goal, int First, int Reached);
}
