using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Func139–142: rotate the shot graphic, fade Samus, and stage the final logo.</summary>
internal sealed class EndingPostShot
{
    /// <summary>Palette captured before the shot and Samus fades begin.</summary>
    private readonly Bgr555[] sourcePalette;
    /// <summary>Post-shot update count driving the shooting and Samus fade schedules.</summary>
    private int calls;
    /// <summary>Frames remaining after rotation reaches its minimum scale.</summary>
    private int hold;
    /// <summary>Current size of the rotating shot graphic, clamped at the native minimum.</summary>
    public int Scale { get; private set; } = EndingPostShotDefinitions.InitialScale;
    /// <summary>Rotation of the shot graphic, turned backward on each active rotation step and wrapping around the full turn.</summary>
    public SnesAngle Angle { get; private set; } = SnesAngle.Zero;
    /// <summary>Number of subtitle and logo transfers already sent to VRAM.</summary>
    public int Uploads { get; private set; }
    /// <summary>Indicates that the shot graphic has reached its minimum scale.</summary>
    public bool RotationFinished { get; private set; }
    /// <summary>Signals the caller to hand control to the white-flash phase after the hold expires.</summary>
    public bool ReadyForWhiteFlash { get; private set; }

    /// <summary>Captures the starting palette for the post-shot rotation, fades and logo transfers.</summary>
    public EndingPostShot(ScreenPalette palette) => sourcePalette = Ensure.NotNull(palette).Colors.ToArray();

    /// <summary>Advances the native rotation, both palette fades, timed VRAM uploads, and white-flash handoff by one update.</summary>
    public void Step(EndingScreen screen)
    {
        Ensure.NotNull(screen);
        ScreenPalette colours = screen.Palette;
        if (ReadyForWhiteFlash) throw new InvalidOperationException("Post-shot white-flash handoff must be consumed.");
        calls++;
        ApplyFade(colours.SpriteRow(EndingColourRowDefinitions.ShootingSpriteRow),
            Math.Min(calls, EndingPostShotDefinitions.FadeFrames));
        int samusFade = Math.Clamp(calls - EndingPostShotDefinitions.SamusFadeDelay, 0,
            EndingPostShotDefinitions.FadeFrames);
        ApplyFade(colours.SpriteRow(EndingColourRowDefinitions.SamusSpriteRow), samusFade);
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
            // Func141 calls the fade before the upload helper: its final fade call
            // permits the first sheet replacement on that very same frame.
            if (samusFade == EndingPostShotDefinitions.FadeFrames && Uploads < EndingPostShotUploadDefinitions.Count)
                screen.Record(EndingUploadKind.PostShotTransfer, Uploads++);
            ReadyForWhiteFlash = --hold == 0;
        }
    }

    /// <summary>Fades one row from its captured colours using the 32-step component progression.</summary>
    private void ApplyFade(ColourRun row, int elapsed)
    {
        int remaining = EndingPostShotDefinitions.FadeFrames - elapsed;
        ReadOnlySpan<Bgr555> source = row.SamePositionsIn(sourcePalette);
        for (int colour = 0; colour < row.Count; colour++)
        {
            // The 8.8 accumulator subtracts component<<3 on every call;
            // taking the high byte is exactly floor(component * remaining / 32).
            row.Write(colour, source[colour].Map((_, channel) => channel * remaining / EndingPostShotDefinitions.FadeFrames));
        }
    }

}
