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
    /// <summary>The colouring his body last showed whole, or null while it shows his room's own colours or a fade's start.</summary>
    private PhantoonBodyLook? settled;

    /// <summary>What his body showed when the running fade started, or null before he shows any colouring.</summary>
    private PictureShading? from;

    /// <summary>The running fade, or null while none runs.</summary>
    private RunningBodyFade? fade;

    /// <summary>The place of the power coming on's latest step, or null before it starts.</summary>
    private int? powerOnStep;

    /// <summary>Whether the power has finished coming on.</summary>
    private bool powered;

    /// <summary>Shows <paramref name="look"/> on his body at once; a running fade continues from it on its next step.</summary>
    internal void Show(PhantoonBodyLook look)
    {
        settled = look;
        from = PictureShading.Shaded(PhantoonShades.Body(look));
        fade = null;
    }

    /// <summary>
    /// Takes the step at place <paramref name="step"/> of his body's fade toward <paramref name="to"/> over
    /// <paramref name="length"/> steps: the running fade's next when it fades that way, otherwise the first of a fade
    /// from what his body shows.
    /// </summary>
    /// <param name="to">The colouring faded toward.</param>
    /// <param name="step">The step's place in the fade (<see cref="PhantoonFadeProgress.TakeStep"/>).</param>
    /// <param name="length">The fade's length in steps.</param>
    /// <exception cref="InvalidOperationException">His body has shown no colouring to fade from.</exception>
    internal void Fade(PhantoonBodyLook to, int step, int length)
    {
        if (fade is { } running && running.To == to)
        {
            fade = running with { Step = step };
            return;
        }
        if (fade is { } other)
        {
            from = PictureShading.Standard.With(other.Shown);
            settled = null;
        }
        PictureShading start = from ?? throw new InvalidOperationException("Phantoon's body fades once it has shown a colouring.");
        fade = new RunningBodyFade(start, to, StartedAt: step, Step: step, Length: length);
    }

    /// <summary>Completes his body's running fade: his body shows the colouring it faded to from now on.</summary>
    internal void CompleteFade()
    {
        if (fade is { } running)
            Show(running.To);
    }

    /// <summary>Takes the step at place <paramref name="step"/> of the Wrecked Ship's power coming on over his room.</summary>
    internal void PowerOn(int step) => powerOnStep = Ensure.AtLeastZero(step);

    /// <summary>Completes the power coming on: the room shows its powered colours.</summary>
    internal void CompletePowerOn() => powered = true;

    /// <inheritdoc />
    public void ShadeGameplay(GameplayShadingBuilder frame)
    {
        Ensure.NotNull(frame);
        if (fade is { } running)
            frame.Shade(GameplayPictures.BossBody, running.Shown);
        else if (settled is { } look)
            frame.Shade(GameplayPictures.BossBody, PhantoonShades.Body(look));
        foreach (GameplayPictures pictures in PoweredPictures)
        {
            if (powered)
                frame.Shade(pictures, PhantoonShades.Powered);
            else if (powerOnStep is { } step)
                frame.Shade(pictures, PowerComingOn(step));
        }
    }

    /// <summary>
    /// The room's fade from its own colours to its powered ones at the power coming on's step at place
    /// <paramref name="step"/>: its first step moves nothing, and each later one an equal part of the way.
    /// </summary>
    private static PictureShadeFade PowerComingOn(int step) =>
        new(PictureShading.Standard, PictureShading.Shaded(PhantoonShades.Powered),
            Math.Min(step, PhantoonColorDefinitions.PowerOnSteps), PhantoonColorDefinitions.PowerOnSteps);

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
}
