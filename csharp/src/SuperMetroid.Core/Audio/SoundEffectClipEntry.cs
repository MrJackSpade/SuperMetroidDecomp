namespace SuperMetroid.Core.Audio;

/// <summary>
/// One sound effect rendered dry (#1305), as a music data set plays it (#2057, #2645).
/// Frame zero is the request check that accepted the request.
/// </summary>
/// <param name="Sound">The sound effect, whose library plays it.</param>
/// <param name="Policy">How the library accepts and starts the request.</param>
/// <param name="Shape">Whether the effect is a one-shot that ends or a never-ending loop, with the clips each plays (#2560).</param>
public sealed partial record SoundEffectClipEntry(
    SoundEffectId Sound,
    SoundEffectClipPolicy Policy,
    SoundEffectClipShape Shape);
