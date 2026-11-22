namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Where one of the intro's Mother Brain explosions appears and how long it waits before showing.</summary>
/// <param name="Offset">Where the explosion appears relative to Mother Brain's origin.</param>
/// <param name="StartDelayFrames">Cinematic updates before the explosion shows its first frame, from one upward.</param>
internal readonly partial record struct IntroMotherBrainExplosionPlacement(
    Offset16<ScreenPixels> Offset,
    int StartDelayFrames);
