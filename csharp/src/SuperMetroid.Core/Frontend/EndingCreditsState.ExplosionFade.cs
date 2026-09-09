namespace SuperMetroid.Core.Frontend;

internal sealed partial class EndingCreditsState
{
    private ushort[] explosionFadeSource = [];
    private int explosionFadeStep;
    private bool ExplosionCrossfadeActive => Phase is EndingCreditsPhase.FadeInZebesExplosion
        or EndingCreditsPhase.ZebesExplosionPaletteCrossfade;

    private void BeginExplosionCrossfade()
    {
        // Func115 clears the previous cloud palette programs before taking its source
        // snapshot; otherwise their next instruction can overwrite the new OBJ fade.
        paletteFx = new Game.RoomPaletteFxSystem();
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
            ushort source = explosionFadeSource[i];
            int red = (source & 31) * factor / EndingExplosionFadeDefinitions.Steps;
            int green = (source >> 5 & 31) * factor / EndingExplosionFadeDefinitions.Steps;
            int blue = (source >> 10 & 31) * factor / EndingExplosionFadeDefinitions.Steps;
            cgram.SetColor(i, (ushort)(red | green << 5 | blue << 10));
        }
    }
}
