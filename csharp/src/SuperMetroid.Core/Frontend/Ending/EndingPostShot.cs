using SuperMetroid.Core.Frontend.Ending.Artwork;
namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Func139–142: rotate the shot graphic, fade Samus, and stage the final logo.</summary>
internal sealed class EndingPostShot
{
    /// <summary>Post-shot update count driving the shooting and Samus fade schedules.</summary>
    private int calls;
    /// <summary>Frames remaining after rotation reaches its minimum scale.</summary>
    private int hold;
    /// <summary>Current size of the rotating shot graphic, clamped at its minimum.</summary>
    public int Scale { get; private set; } = EndingPostShotDefinitions.InitialScale;
    /// <summary>Rotation of the shot graphic, turned backward on each active rotation step and wrapping around the full turn.</summary>
    public RotationAngle Angle { get; private set; } = RotationAngle.Zero;
    /// <summary>Number of the subtitle and logo parts shown so far.</summary>
    public int PartsShown { get; private set; }
    /// <summary>Indicates that the shot graphic has reached its minimum scale.</summary>
    public bool RotationFinished { get; private set; }
    /// <summary>Signals the caller to hand control to the white-flash phase after the hold expires.</summary>
    public bool ReadyForWhiteFlash { get; private set; }

    /// <summary>Advances the rotation, both palette fades, the subtitle and logo parts, and white-flash handoff by one update.</summary>
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
            Angle = Angle.AddTableUnits(-EndingPostShotDefinitions.AngleStep);
            Scale -= EndingPostShotDefinitions.ScaleStep;
            if (Scale < EndingPostShotDefinitions.MinimumScale)
            {
                Scale = EndingPostShotDefinitions.MinimumScale;
                RotationFinished = true;
                hold = EndingPostShotDefinitions.HoldFrames;
            }
        }
        else
        {
            // Func141 calls the fade before the part helper: its final fade call
            // permits the first part on that very same frame.
            if (samusFade == EndingPostShotDefinitions.FadeFrames && PartsShown < EndingPostShotParts.Count)
            {
                screen.ShowPostShotPart();
                PartsShown++;
            }
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
        // The 8.8 accumulator subtracts component<<3 on every call;
        // taking the high byte is exactly floor(component * remaining / 32).
        colours.Fade(group, EndingPostShotDefinitions.FadeFrames - elapsed);
    }

}
