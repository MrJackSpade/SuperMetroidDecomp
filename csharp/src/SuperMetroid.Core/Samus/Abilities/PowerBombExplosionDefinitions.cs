using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Power Bomb/Crystal Flash blast radii, the ovals their shape streams walk, and phase boundaries.</summary>
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
    /// The horizontal radius, in pixels, of each yellow oval the pre-explosion's last frames walk, one per frame. Each
    /// is the radius a blast starting at four pixels and forty-eight pixels a frame, slowing by half a pixel a frame,
    /// reaches after three to six frames, held at 255.
    /// </summary>
    public static IReadOnlyList<int> YellowShapeRadii { get; } = [146, 193, 239, 255];
    /// <summary>
    /// The horizontal radius, in pixels, of each white oval the explosion walks while it finishes expanding, one per
    /// frame. Each is the radius a blast starting at four pixels and accelerating by three sixteenths of a pixel a
    /// frame reaches after thirty-seven to fifty-three frames, held at 255.
    /// </summary>
    public static IReadOnlyList<int> WhiteShapeRadii { get; } =
        [128, 135, 142, 150, 157, 165, 173, 181, 189, 198, 206, 215, 224, 233, 243, 252, 255];
    /// <summary>The blast's oval is three quarters as tall as it is wide: the parts of its width its height takes.</summary>
    public const int OvalHeightParts = 3;
    /// <summary>The parts the blast's oval width is divided into for <see cref="OvalHeightParts"/>.</summary>
    public const int OvalWidthParts = 4;
    /// <summary>Radius at which white pre-explosion becomes yellow.</summary>
    public static readonly PowerBombRadius PreExplosionWhiteLimit = PowerBombRadius.FromWholePixels(146);
    /// <summary>Radius at which yellow explosion switches to white shapes.</summary>
    public static readonly PowerBombRadius ExplosionYellowLimit = PowerBombRadius.FromWholePixels(134);
    /// <summary>Crystal Flash's shorter explosion radius limit: thirty-two pixels.</summary>
    public static readonly PowerBombRadius CrystalFlashRadiusLimit = PowerBombRadius.FromWholePixels(32);
    /// <summary>
    /// Maximum component of the first Crystal Flash color:
    /// levels (14,14,10). This is a lifetime operand, not replacement artwork.
    /// </summary>
    internal const int CrystalFlashInitialFadeSteps = 14;
    /// <summary>
    /// The explosion selects a colour for each band of this many whole pixels of its consumed radius. Crystal Flash
    /// reaches only the first four yellow colour steps.
    /// </summary>
    internal const int CrystalFlashColorBandPixels = 8;
}
