namespace SuperMetroid.Core.Rendering;

/// <summary>Cartridge values and helpers of <see cref="ScreenLineBand"/> that only verification reads.</summary>
internal static class ScreenLineBandConstants
{
    /// <summary>Every line below the HUD: the gameplay area.</summary>
    public static ScreenLineBand Gameplay { get; } = new(ScreenGeometry.GameplayHudHeightPixels, ScreenGeometry.ScreenHeightPixels);
}
