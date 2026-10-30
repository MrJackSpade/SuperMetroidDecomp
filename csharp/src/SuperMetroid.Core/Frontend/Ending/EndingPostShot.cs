using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Func139–142: rotate the shot graphic, fade Samus, and stage the final logo.</summary>
internal sealed class EndingPostShot
{
    /// <summary>The screen's colours, by owner, captured before the shot and Samus fades begin.</summary>
    private readonly ColourImage sourceColours;
    /// <summary>Post-shot update count driving the shooting and Samus fade schedules.</summary>
    private int calls;
    /// <summary>Frames remaining after rotation reaches its minimum scale.</summary>
    private int hold;
    /// <summary>Current size of the rotating shot graphic, clamped at the native minimum.</summary>
    public int Scale { get; private set; } = EndingPostShotDefinitions.InitialScale;
    /// <summary>Rotation of the shot graphic, turned backward on each active rotation step and wrapping around the full turn.</summary>
    public RotationAngle Angle { get; private set; } = RotationAngle.Zero;
    /// <summary>Number of the subtitle and logo parts shown so far.</summary>
    public int PartsShown { get; private set; }
    /// <summary>Indicates that the shot graphic has reached its minimum scale.</summary>
    public bool RotationFinished { get; private set; }
    /// <summary>Signals the caller to hand control to the white-flash phase after the hold expires.</summary>
    public bool ReadyForWhiteFlash { get; private set; }

    /// <summary>Captures the starting palette for the post-shot rotation, fades and logo parts.</summary>
    public EndingPostShot(ScreenPalette palette) => sourceColours = Ensure.NotNull(palette).Capture();

    /// <summary>Advances the native rotation, both palette fades, the subtitle and logo parts, and white-flash handoff by one update.</summary>
    public void Step(EndingScreen screen)
    {
        Ensure.NotNull(screen);
        ScreenPalette colours = screen.Palette;
        if (ReadyForWhiteFlash) throw new InvalidOperationException("Post-shot white-flash handoff must be consumed.");
        calls++;
        ApplyFade(colours.Colours(ScreenColourGroup.EndingShot),
            Math.Min(calls, EndingPostShotDefinitions.FadeFrames));
        int samusFade = Math.Clamp(calls - EndingPostShotDefinitions.SamusFadeDelay, 0,
            EndingPostShotDefinitions.FadeFrames);
        ApplyFade(colours.Colours(ScreenColourGroup.EndingShootingSamus), samusFade);
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

    /// <summary>Fades one colour group from its captured colours using the 32-step component progression.</summary>
    private void ApplyFade(ColourRun row, int elapsed)
    {
        int remaining = EndingPostShotDefinitions.FadeFrames - elapsed;
        ReadOnlySpan<RgbColor> source = sourceColours.Of(row);
        for (int colour = 0; colour < row.Count; colour++)
        {
            // The 8.8 accumulator subtracts component<<3 on every call;
            // taking the high byte is exactly floor(component * remaining / 32).
            row.Write(colour, source[colour].Map((_, channel) => channel * remaining / EndingPostShotDefinitions.FadeFrames));
        }
    }

}
