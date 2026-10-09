namespace SuperMetroid.Core.Frontend;

internal sealed partial class EndingCreditsState
{
    /// <summary>Palette snapshot used as the source for each interpolated explosion-fade color.</summary>
    private ushort[] explosionFadeSource = [];

    /// <summary>Current interpolation step shared by the background and two object palette regions.</summary>
    private int explosionFadeStep;

    /// <summary>Indicates whether the ending is in either phase that performs the explosion palette crossfade.</summary>
    private bool ExplosionCrossfadeActive => Phase is EndingCreditsPhase.FadeInZebesExplosion
        or EndingCreditsPhase.ZebesExplosionPaletteCrossfade;

    /// <summary>Captures the palette source and initializes the explosion crossfade at its first step.</summary>
    private void BeginExplosionCrossfade()
    {
        // Func115 clears the previous cloud palette programs before taking its source
        // snapshot; otherwise their next instruction can overwrite the new OBJ fade.
        ResetPaletteFx();
        explosionFadeSource = cgram.Colors.ToArray();
        explosionFadeStep = 0;
        ApplyExplosionFade(EndingExplosionFadeDefinitions.FirstObjectStart, 0);
        ApplyExplosionFade(EndingExplosionFadeDefinitions.SecondObjectStart, 0);
        phaseTimer = EndingExplosionFadeDefinitions.InitialCountdown;
    }

    /// <summary>Advances the explosion crossfade and starts flyaway uploads when its countdown expires.</summary>
    private void StepExplosionCrossfade()
    {
        mode7Zoom = unchecked((ushort)(mode7Zoom + EndingExplosionFadeDefinitions.ZoomStep));
        if ((phaseTimer & 1) == 0)
        {
            explosionFadeStep++;
            ApplyExplosionFade(EndingExplosionFadeDefinitions.BackgroundStart,
                EndingExplosionFadeDefinitions.Steps - explosionFadeStep);
            ApplyExplosionFade(EndingExplosionFadeDefinitions.FirstObjectStart, explosionFadeStep);
            ApplyExplosionFade(EndingExplosionFadeDefinitions.SecondObjectStart, explosionFadeStep);
        }
        if (--phaseTimer < 0)
        {
            phaseTimer = 16;
            PrepareFlyawayUploads();
            Phase = EndingCreditsPhase.ZebesExplosionTileUpload;
        }
    }

    /// <summary>Writes one palette region scaled from the captured source by the requested fade factor.</summary>
    /// <param name="start">First CGRAM color index in the region.</param>
    /// <param name="factor">Numerator of the fade intensity, measured against the configured step count.</param>
    private void ApplyExplosionFade(int start, int factor)
    {
        for (int i = start; i < start + EndingExplosionFadeDefinitions.Colors; i++)
        {
            ushort source = explosionFadeSource[i];
            int red = (source & 31) * factor / EndingExplosionFadeDefinitions.Steps;
            int green = (source >> 5 & 31) * factor / EndingExplosionFadeDefinitions.Steps;
            int blue = (source >> 10 & 31) * factor / EndingExplosionFadeDefinitions.Steps;
            cgram.SetColor(i, (ushort)(red | green << 5 | blue << 10));
        }
    }
}
