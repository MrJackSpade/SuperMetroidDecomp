using System.Collections.Immutable;

namespace SuperMetroid.Core.Fx;

/// <summary>How lava's and acid's heat haze moves and how often lava bubbles (#1415).</summary>
public static class LavaAcidWaveDefinitions
{
    /// <summary>Frames between the vertical heat-haze wave's steps.</summary>
    public const int VerticalWavePhaseDuration = 4;

    /// <summary>Frames between the horizontal heat-haze wave's steps.</summary>
    public const int HorizontalWavePhaseDuration = 6;

    /// <summary>
    /// One period of the horizontal heat haze, one sideways background displacement in pixels per step (#1468, #2054): a
    /// four-step push one way from the third step, then a four-step push the other way from the ninth. The vertical heat
    /// haze repeats the water wave (<see cref="WaterWaveDefinitions.Waveform"/>).
    /// </summary>
    public static ImmutableArray<int> HorizontalHazeWaveform { get; } = [0, 0, 1, 1, 1, 1, 0, 0, -1, -1, -1, -1, 0, 0, 0, 0];

    /// <summary>The steps in one cycle of either heat-haze wave.</summary>
    public static int WaveDisplacementCount => HorizontalHazeWaveform.Length;

    /// <summary>Frames between lava's ambient bubbling sounds.</summary>
    public const int AmbientSoundPeriod = 112;

    /// <summary>The random selection that chooses which of the three bubbling sounds lava plays: eight choices.</summary>
    internal static readonly RandomSampleField AmbientSoundChoice = new(0, 3);

    /// <summary>The request priority of lava's ambient sound.</summary>
    public static readonly SoundRequestPriority AmbientSoundPriority = SoundRequestPriority.Normal;
}
