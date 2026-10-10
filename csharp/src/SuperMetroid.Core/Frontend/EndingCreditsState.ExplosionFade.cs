using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

internal sealed partial class EndingCreditsState
{
    private Bgr555[] explosionFadeSource = [];
    private int explosionFadeStep;
    private bool ExplosionCrossfadeActive => Phase is EndingCreditsPhase.FadeInZebesExplosion
        or EndingCreditsPhase.ZebesExplosionPaletteCrossfade;

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

    private void ApplyExplosionFade(int start, int factor)
    {
        for (int i = start; i < start + EndingExplosionFadeDefinitions.Colors; i++)
        {
            cgram.SetColor(i, explosionFadeSource[i].Map((_, channel) =>
                channel * factor / EndingExplosionFadeDefinitions.Steps));
        }
    }
}
