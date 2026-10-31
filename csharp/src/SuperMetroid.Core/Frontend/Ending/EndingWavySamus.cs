using SuperMetroid.Core.Rendering;
using static SuperMetroid.Core.Frontend.Ending.EndingWavySamusDefinitions;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// Ripples Samus in the ending (<see cref="EndingWavySamusDefinitions"/>): the sine wave her plane shows at each age of
/// the backdrop's wait, and the plane's vertical scroll (#1468: the wave is described, and the backend draws each line).
/// </summary>
internal static class EndingWavySamus
{
    /// <summary>
    /// The wave the plane shows at cinematic age <paramref name="age"/>, or null before it first shows. The age counts
    /// the wait's frames from one, derived from the cinematic countdown, so a restored state keeps the wave's phase.
    /// </summary>
    /// <param name="age">The frames the wait has run, from one.</param>
    internal static ScrollWave? WaveAtAge(int age) => age < FirstWaveAge ? null
        : new ScrollWave(0, Amplitude, new ByteAngle(WordWrap.LowByte(InitialAngle + AngleStepPerFrame * (age - 1))),
            Period, 0, ScreenGeometry.ScreenHeightPixels);

    /// <summary>The plane's vertical scroll at cinematic age <paramref name="age"/>: it climbs from zero as the wait runs.</summary>
    /// <param name="age">The frames the wait has run.</param>
    internal static int VerticalScrollAtAge(int age) => age * VerticalStep;
}
