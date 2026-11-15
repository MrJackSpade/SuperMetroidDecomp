using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Power Bomb/Crystal Flash blast radii, the ovals its last stages show, and phase boundaries and durations.</summary>
public static class PowerBombExplosionDefinitions
{
    /// <summary>Initial radius: four pixels.</summary>
    public static readonly PowerBombRadius InitialRadius = PowerBombRadius.FromWholePixels(4);
    /// <summary>Initial pre-explosion radial speed: forty-eight pixels per update.</summary>
    public static readonly PowerBombRadius InitialPreExplosionSpeed = PowerBombRadius.FromWholePixels(48);
    /// <summary>The pre-explosion flash's slowing: its speed drops by half a pixel an update each update it grows.</summary>
    public static readonly PowerBombRadius PreExplosionSlowing = PowerBombRadius.FromFraction(128);
    /// <summary>The explosion's acceleration: its speed rises by three sixteenths of a pixel an update each update it grows.</summary>
    public static readonly PowerBombRadius ExplosionAcceleration = PowerBombRadius.FromFraction(48);
    /// <summary>
    /// The updates the yellow pre-explosion lasts (#1927): it shows one of <see cref="YellowOvalRadii"/> each update, and
    /// the damaging explosion starts after the last.
    /// </summary>
    public const int PreExplosionYellowUpdates = 4;
    /// <summary>
    /// The updates the white explosion lasts (#1927): it shows one of <see cref="WhiteOvalRadii"/> each update, and the
    /// afterglow starts after the last.
    /// </summary>
    public const int ExplosionWhiteUpdates = 17;
    /// <summary>
    /// The horizontal radius, in pixels, of the oval the yellow pre-explosion shows on each of its updates. The ovals are
    /// authored: they run close to the flash's own radius but are not it (the flash reaches 240 pixels and holds there).
    /// </summary>
    public static IReadOnlyList<int> YellowOvalRadii { get; } = [146, 193, 239, 255];
    /// <summary>
    /// The horizontal radius, in pixels, of the oval the white explosion shows on each of its updates. The ovals are
    /// authored: they run close to the explosion's own radius but are not it (the explosion starts them at 135 pixels
    /// and holds at 250).
    /// </summary>
    public static IReadOnlyList<int> WhiteOvalRadii { get; } =
        [128, 135, 142, 150, 157, 165, 173, 181, 189, 198, 206, 215, 224, 233, 243, 252, 255];
    /// <summary>The blast's oval is three quarters as tall as it is wide: the parts of its width its height takes.</summary>
    public const int OvalHeightParts = 3;
    /// <summary>The parts the blast's oval width is divided into for <see cref="OvalHeightParts"/>.</summary>
    public const int OvalWidthParts = 4;
    /// <summary>Radius at which white pre-explosion becomes yellow.</summary>
    public static readonly PowerBombRadius PreExplosionWhiteLimit = PowerBombRadius.FromWholePixels(146);
    /// <summary>Radius at which the yellow explosion switches to the white explosion's ovals.</summary>
    public static readonly PowerBombRadius ExplosionYellowLimit = PowerBombRadius.FromWholePixels(134);
    /// <summary>Crystal Flash's shorter explosion radius limit: thirty-two pixels.</summary>
    public static readonly PowerBombRadius CrystalFlashRadiusLimit = PowerBombRadius.FromWholePixels(32);
    /// <summary>
    /// Maximum component of the first Crystal Flash color:
    /// levels (14,14,10). This is a lifetime count, not replacement artwork.
    /// </summary>
    internal const int CrystalFlashInitialFadeSteps = 14;
}
