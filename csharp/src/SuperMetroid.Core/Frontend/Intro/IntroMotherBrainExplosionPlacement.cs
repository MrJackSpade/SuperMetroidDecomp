namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>One signed origin-offset and start-delay record for an intro explosion actor.</summary>
/// <param name="Offset">Where the explosion appears relative to Mother Brain's origin.</param>
/// <param name="StartDelayFrames">Cinematic updates before the explosion shows its first frame, from one upward.</param>
internal readonly record struct IntroMotherBrainExplosionPlacement(
    Offset16<ScreenPixels> Offset,
    int StartDelayFrames);
