using System.Collections.Frozen;
using System.Collections.Immutable;

namespace SuperMetroid.Core.Fx;

/// <summary>How lava's and acid's heat haze moves and how lava bubbles (#1415).</summary>
public static class LavaAcidWaveDefinitions
{
    /// <summary>Frames between the vertical heat-haze wave's steps.</summary>
    public const int VerticalWavePhaseDuration = 4;

    /// <summary>Frames between the horizontal heat-haze wave's steps.</summary>
    public const int HorizontalWavePhaseDuration = 6;

    /// <summary>
    /// One period of each heat haze as its first phase shows it, one background displacement in pixels per line (#1468,
    /// #2054, #2367); each later phase draws it one line further down. The horizontal haze sways the lines sideways: a
    /// four-line push one way from the third line, then a four-line push the other way from the ninth. The vertical haze
    /// sways them up and down: two identical eight-line triangular half-periods, rising then falling.
    /// </summary>
    public static FrozenDictionary<HeatHazeDirection, ImmutableArray<int>> HazeWaveforms { get; } =
        new Dictionary<HeatHazeDirection, ImmutableArray<int>>
    {
        [HeatHazeDirection.Horizontal] = [0, 0, 1, 1, 1, 1, 0, 0, -1, -1, -1, -1, 0, 0, 0, 0],
        [HeatHazeDirection.Vertical] = [0, 1, 1, 0, 0, -1, -1, 0, 0, 1, 1, 0, 0, -1, -1, 0],
    }.ToFrozenDictionary();

    /// <summary>The phases in one cycle of either heat haze: one for each line of its waveform.</summary>
    public const int HazePhaseCount = 16;

    /// <summary>Frames between lava's ambient bubbling sounds.</summary>
    public const int AmbientSoundPeriod = 112;

    /// <summary>
    /// The bubbling sound lava plays each time it bubbles (#2367): the eight values of its random choice take the three
    /// bubbling sounds in turn, so the first two play three times in eight and the third twice.
    /// </summary>
    internal static readonly WeightedRandomChoice<SoundEffectId> AmbientSound = new(new RandomSampleField(0, 3),
        new(1, EnemySound.LavaBubbling1), new(1, EnemySound.LavaBubbling2), new(1, EnemySound.LavaBubbling3),
        new(1, EnemySound.LavaBubbling1), new(1, EnemySound.LavaBubbling2), new(1, EnemySound.LavaBubbling3),
        new(1, EnemySound.LavaBubbling1), new(1, EnemySound.LavaBubbling2));

    /// <summary>The request priority of lava's ambient sound.</summary>
    public static readonly SoundRequestPriority AmbientSoundPriority = SoundRequestPriority.Normal;
}
