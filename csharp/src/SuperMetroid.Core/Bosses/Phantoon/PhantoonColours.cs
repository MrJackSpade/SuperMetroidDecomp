using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Phantoon's colours (#1512, #2251): the colouring his body last showed whole on the background, the fade running from
/// it, and the Wrecked Ship's power coming on over his room once he dies. A fade is published as a typed fade between his
/// body's shadings (<see cref="PictureShadeFade"/>) and the power coming on as one from the room's own colours to its
/// powered ones; a completed fade shows its goal. The colours never reach gameplay; only when a fade completes does,
/// through its own progress (<see cref="PhantoonFadeProgress"/>).
/// </summary>
/// <remarks>
/// A fade starts from what his body shows when it takes its first step. His hurt flash writes white and his health
/// colours in the middle of the fade that follows the hit, which restarts that fade from the colouring written, over the
/// steps it has left. A fade toward another goal while one runs starts from the running fade as it shows.
/// </remarks>
internal sealed class PhantoonColours : IGameplayShadingSource
{
    /// <summary>
    /// What his body shows (#2560): his room's own colours before he shows any colouring, a colouring settled whole, or a
    /// fade running from what his body showed as it started.
    /// </summary>
    private BodyColouring body = new UnshownBody();

    /// <summary>The Wrecked Ship's power over his room (#2560): off, coming on at its latest step, or on; null while off.</summary>
    private PowerColouring? power;

    /// <summary>Shows <paramref name="look"/> on his body at once; a running fade continues from it on its next step.</summary>
    internal void Show(PhantoonBodyLook look)
    {
        body = new SettledBody(look);
    }

    /// <summary>
    /// Takes the step at place <paramref name="step"/> of his body's fade toward <paramref name="to"/> over
    /// <paramref name="length"/> steps: the running fade's next when it fades that way, otherwise the first of a fade
    /// from what his body shows.
    /// </summary>
    /// <param name="to">The colouring faded toward.</param>
    /// <param name="step">The step's place in the fade (<see cref="PhantoonFadeProgress.TakeStep"/>).</param>
    /// <param name="length">The fade's length in steps.</param>
    internal void Fade(PhantoonBodyLook to, int step, int length)
    {
        if (body is FadingBody { Fade: var running } && running.To == to)
        {
            body = new FadingBody(running with { Step = step });
            return;
        }
        PictureShading start = body switch
        {
            FadingBody other => PictureShading.Standard.With(other.Fade.Shown),
            SettledBody settled => PictureShading.Shaded(PhantoonShades.Body(settled.Look)),
            _ => throw new InvalidOperationException("Phantoon's body fades once it has shown a colouring."),
        };
        body = new FadingBody(new RunningBodyFade(start, to, StartedAt: step, Step: step, Length: length));
    }

    /// <summary>Completes his body's running fade: his body shows the colouring it faded to from now on.</summary>
    internal void CompleteFade()
    {
        if (body is FadingBody fading)
            Show(fading.Fade.To);
    }

    /// <summary>Takes the step at place <paramref name="step"/> of the Wrecked Ship's power coming on over his room.</summary>
    internal void PowerOn(int step) => power = new PowerComingOnColouring(Ensure.AtLeastZero(step));

    /// <summary>Completes the power coming on: the room shows its powered colours.</summary>
    internal void CompletePowerOn() => power = new PoweredColouring();

    /// <inheritdoc />
    public void ShadeGameplay(GameplayShadingBuilder frame)
    {
        Ensure.NotNull(frame);
        if (body is FadingBody fading)
            frame.Shade(GameplayPictures.BossBody, fading.Fade.Shown);
        else if (body is SettledBody settled)
            frame.Shade(GameplayPictures.BossBody, PhantoonShades.Body(settled.Look));
        foreach (GameplayPictures pictures in PoweredPictures)
        {
            if (power is PoweredColouring)
                frame.Shade(pictures, PhantoonShades.Powered);
            else if (power is PowerComingOnColouring { Step: var step })
                frame.Shade(pictures, PowerComingOn(step));
        }
    }

    /// <summary>
    /// The room's fade from its own colours to its powered ones at the power coming on's step at place
    /// <paramref name="step"/>: its first step moves nothing, and each later one an equal part of the way.
    /// </summary>
    private static PictureShadeFade PowerComingOn(int step) =>
        new(PictureShading.Standard, PictureShading.Shaded(PhantoonShades.Powered),
            new FadeLevel(Math.Min(step, PhantoonColorDefinitions.PowerOnSteps), PhantoonColorDefinitions.PowerOnSteps));

    /// <summary>
    /// The pictures the power coming on recolours: the room's and its effect overlay's. The HUD keeps its own colours,
    /// which the power coming on does not change (#1544).
    /// </summary>
    private static readonly GameplayPictures[] PoweredPictures = [GameplayPictures.Room, GameplayPictures.FxOverlay];

    /// <summary>
    /// The fade running on his body: from <paramref name="From"/> toward <paramref name="To"/>'s colouring over
    /// <paramref name="Length"/> steps, started at the step at place <paramref name="StartedAt"/> and now at
    /// <paramref name="Step"/>. The first step moves nothing, so a fade's moving steps run from its first past zero through
    /// its length, each an equal part of the way, and the step past its length reaches its goal.
    /// </summary>
    /// <param name="From">What his body showed when the fade started.</param>
    /// <param name="To">The colouring the fade runs toward.</param>
    /// <param name="StartedAt">The place of the fade's first step.</param>
    /// <param name="Step">The place of its latest step.</param>
    /// <param name="Length">The fade's length in steps.</param>
    private sealed record RunningBodyFade(PictureShading From, PhantoonBodyLook To, int StartedAt, int Step, int Length)
    {
        /// <summary>The typed fade his body shows.</summary>
        internal PictureShadeFade Shown
        {
            get
            {
                PictureShading goal = PictureShading.Shaded(PhantoonShades.Body(To));
                int firstMoving = Math.Max(StartedAt, 1);
                int parts = Length - firstMoving + 1;
                // A fade restarted past its last moving step has only the step that reaches its goal left.
                if (parts < 1)
                    return new PictureShadeFade(From, goal, new FadeLevel(Step > Length ? 1 : 0, 1));
                return new PictureShadeFade(From, goal, new FadeLevel(Math.Clamp(Step - firstMoving + 1, 0, parts), parts));
            }
        }
    }

    /// <summary>What his body shows (#2560).</summary>
    private abstract record BodyColouring;

    /// <summary>His room's own colours, before he shows any colouring.</summary>
    private sealed record UnshownBody : BodyColouring;

    /// <summary>A colouring shown whole, from which a fade starts.</summary>
    /// <param name="Look">The colouring.</param>
    private sealed record SettledBody(PhantoonBodyLook Look) : BodyColouring;

    /// <summary>A fade running from what his body showed as it started.</summary>
    /// <param name="Fade">The running fade.</param>
    private sealed record FadingBody(RunningBodyFade Fade) : BodyColouring;

    /// <summary>The Wrecked Ship's power once it starts coming on (#2560).</summary>
    private abstract record PowerColouring;

    /// <summary>The power coming on, at the place of its latest step.</summary>
    /// <param name="Step">The latest step's place.</param>
    private sealed record PowerComingOnColouring(int Step) : PowerColouring;

    /// <summary>The power on: the room shows its powered colours.</summary>
    private sealed record PoweredColouring : PowerColouring;
}
