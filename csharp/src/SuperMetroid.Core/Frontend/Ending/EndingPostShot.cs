using SuperMetroid.Core.Frontend.Ending.Artwork;
namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>After the shot: rotate the shot graphic, fade Samus, and hold before the white flash.</summary>
internal sealed class EndingPostShot
{
    /// <summary>The shot picture's fade out (#2541): a step on each of the shot's updates from its first.</summary>
    private readonly PacedFadeSteps shotFade = new(EndingPostShotDefinitions.FadeSteps, EndingPostShotDefinitions.FadeUpdatesPerStep,
        EndingPostShotDefinitions.ShotFadeFirstStepUpdate);
    /// <summary>Samus's fade out (#2541): a step on each of the shot's updates once its delay has passed.</summary>
    private readonly PacedFadeSteps samusFade = new(EndingPostShotDefinitions.FadeSteps, EndingPostShotDefinitions.FadeUpdatesPerStep,
        firstStepUpdate: EndingPostShotDefinitions.SamusFadeDelay + 1);
    /// <summary>The hold before the white flash (#2541), which counts the updates after the rotation reaches its minimum zoom.</summary>
    private readonly UpdateCountdown hold = UpdateCountdown.Lasting(EndingPostShotDefinitions.HoldFrames);
    /// <summary>Current zoom of the rotating shot graphic, narrowed each step and stopped at its minimum.</summary>
    public FloorZoom Zoom { get; private set; } = EndingPostShotDefinitions.InitialZoom;
    /// <summary>Rotation of the shot graphic, turned backward on each active rotation step and wrapping around the full turn.</summary>
    public RotationAngle Angle { get; private set; } = RotationAngle.Zero;
    /// <summary>Indicates that the shot graphic has reached its minimum zoom.</summary>
    private bool RotationFinished { get; set; }
    /// <summary>Signals that the ending hands over to the white-flash phase now that the hold has expired.</summary>
    public bool ReadyForWhiteFlash { get; private set; }

    /// <summary>Advances the rotation, both colour fades and the white-flash handoff by one update.</summary>
    public void Step(EndingScreen screen)
    {
        Ensure.NotNull(screen);
        if (ReadyForWhiteFlash) throw new InvalidOperationException("Post-shot white-flash handoff must be consumed.");
        shotFade.Advance();
        samusFade.Advance();
        ApplyFade(screen.Colours, EndingFade.Shot, shotFade);
        ApplyFade(screen.Colours, EndingFade.ShootingSamus, samusFade);
        if (!RotationFinished)
        {
            Angle = Angle.TurnedBySteps(-EndingPostShotDefinitions.AngleStep);
            Zoom = Zoom.Narrowed(EndingPostShotDefinitions.ZoomStep);
            if (Zoom < EndingPostShotDefinitions.MinimumZoom)
            {
                Zoom = EndingPostShotDefinitions.MinimumZoom;
                RotationFinished = true;
            }
        }
        else
        {
            ReadyForWhiteFlash = hold.Count();
        }
    }

    /// <summary>
    /// Shows <paramref name="group"/> at the step <paramref name="fade"/> has reached, over the fade's steps from the colours it showed
    /// when the shot began (#1512): those are the post-credits colours, which nothing recolours between their restoration
    /// and the shot, so the import pre-colours each level from them.
    /// </summary>
    private static void ApplyFade(EndingColours colours, EndingFade group, PacedFadeSteps fade)
    {
        // Each step dims every colour by an equal share of its full brightness: the level left is its brightness times the
        // share of steps remaining, rounded down.
        colours.Fade(group, fade.Left);
    }

}
