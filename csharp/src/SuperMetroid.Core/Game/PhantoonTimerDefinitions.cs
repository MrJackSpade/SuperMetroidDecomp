namespace SuperMetroid.Core.Game;

/// <summary>Phantoon's authored timer choices; callers retain their native RNG/frame masks.</summary>
public static class PhantoonTimerDefinitions
{
    /// <summary>$A7:CD41, Phantoon_Figure8_VulnerableWindowTimers: eight eye-open duration choices.</summary>
    public static ReadOnlySpan<ushort> VulnerableWindow => [60, 30, 15, 30, 60, 30, 15, 60];
    /// <summary>$A7:CD53, Phantoon_EyeClosedTimers: eight hidden-eye duration choices; first-round NMI selection uses only the first four.</summary>
    public static ReadOnlySpan<ushort> EyeClosed => [720, 60, 360, 720, 360, 60, 360, 720];
    /// <summary>$A7:CD63, Phantoon_FlameRain_HidingTimers: eight hidden delays before rain placement.</summary>
    public static ReadOnlySpan<ushort> RainHiding => [60, 120, 30, 60, 30, 60, 30, 30];
}
