namespace SuperMetroid.Core.Fx;

/// <summary>How lava's and acid's heat haze moves and how often lava bubbles (#1415).</summary>
public static class LavaAcidWaveDefinitions
{
    /// <summary>Frames between the vertical heat-haze wave's steps.</summary>
    public const ushort VerticalWavePhaseDuration = 4;

    /// <summary>Frames between the horizontal heat-haze wave's steps.</summary>
    public const ushort HorizontalWavePhaseDuration = 6;

    /// <summary>The steps in one cycle of either heat-haze wave.</summary>
    public const int WaveDisplacementCount = 16;

    /// <summary>Frames between lava's ambient bubbling sounds.</summary>
    public const ushort AmbientSoundPeriod = 112;

    /// <summary>The request priority of lava's ambient sound.</summary>
    public static readonly SoundRequestPriority AmbientSoundPriority = SoundRequestPriority.Normal;
}
