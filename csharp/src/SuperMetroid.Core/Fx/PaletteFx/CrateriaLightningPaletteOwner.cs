namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>The mutually exclusive Crateria lightning palette program.</summary>
public enum CrateriaLightningPaletteOwner
{
    /// <summary>$8D:F765, Crateria 1 lightning: program $8D:EB3B flashes BG palette 5 colors 4..11 at the Landing Site before power bombs are acquired.</summary>
    SurfaceLightning,
    /// <summary>$8D:F769, unused dark-lightning counterpart: program $8D:EC6E darkens seven colors starting at CGRAM color 65.</summary>
    UnusedDarkLightning,
}
