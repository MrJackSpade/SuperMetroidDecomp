using SuperMetroid.Core.Frontend.Ending.Artwork;
namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>After the shot: rotate the shot graphic, fade Samus, and hold before the white flash.</summary>
internal sealed class EndingPostShot
{
    /// <summary>Post-shot update count driving the shooting and Samus fade schedules.</summary>
    private int calls;
    /// <summary>Frames remaining after rotation reaches its minimum zoom.</summary>
    private int hold;
    /// <summary>Current zoom of the rotating shot graphic, narrowed each step and stopped at its minimum.</summary>
    public FloorZoom Zoom { get; private set; } = EndingPostShotDefinitions.InitialZoom;
    /// <summary>Rotation of the shot graphic, turned backward on each active rotation step and wrapping around the full turn.</summary>
    public RotationAngle Angle { get; private set; } = RotationAngle.Zero;
    /// <summary>Indicates that the shot graphic has reached its minimum zoom.</summary>
    public bool RotationFinished { get; private set; }
    /// <summary>Signals the caller to hand control to the white-flash phase after the hold expires.</summary>
    public bool ReadyForWhiteFlash { get; private set; }

    /// <summary>Advances the rotation, both palette fades and the white-flash handoff by one update.</summary>
    public void Step(EndingScreen screen)
    {
        Ensure.NotNull(screen);
        if (ReadyForWhiteFlash) throw new InvalidOperationException("Post-shot white-flash handoff must be consumed.");
        calls++;
        ApplyFade(screen.Colours, EndingFade.Shot, Math.Min(calls, EndingPostShotDefinitions.FadeFrames));
        int samusFade = Math.Clamp(calls - EndingPostShotDefinitions.SamusFadeDelay, 0,
            EndingPostShotDefinitions.FadeFrames);
        ApplyFade(screen.Colours, EndingFade.ShootingSamus, samusFade);
        if (!RotationFinished)
        {
            Angle = Angle.TurnedBySteps(-EndingPostShotDefinitions.AngleStep);
            Zoom = Zoom.Narrowed(EndingPostShotDefinitions.ZoomStep);
            if (Zoom < EndingPostShotDefinitions.MinimumZoom)
            {
                Zoom = EndingPostShotDefinitions.MinimumZoom;
                RotationFinished = true;
                hold = EndingPostShotDefinitions.HoldFrames;
            }
        }
        else
        {
            ReadyForWhiteFlash = --hold == 0;
        }
    }

    /// <summary>
    /// Fades <paramref name="group"/> from the colours it showed when the shot began using the 32-step component
    /// progression (#1512): those are the post-credits colours, which nothing recolours between their restoration and
    /// the shot, so the import pre-colours each level from them.
    /// </summary>
    private static void ApplyFade(EndingColours colours, EndingFade group, int elapsed)
    {
        // The game's fade lowers each colour component by a 32nd of its full value on every call;
        // the whole part left is exactly floor(component * remaining / 32).
        colours.Fade(group, EndingPostShotDefinitions.FadeFrames - elapsed);
    }

}
