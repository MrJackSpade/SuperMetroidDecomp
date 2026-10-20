using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Cartridge values and helpers of <see cref="IntroCinematicState"/> that only verification reads.</summary>
internal static class IntroCinematicStateConstants
{
    /// <summary>Full SNES viewport width used by the cinematic's software planes.</summary>
    internal const int ScreenWidth = SnesPpuLayout.ScreenWidthPixels;
    /// <summary>Visible SNES viewport height used by the cinematic's software planes.</summary>
    internal const int ScreenHeight = SnesPpuLayout.ScreenHeightPixels;
}
