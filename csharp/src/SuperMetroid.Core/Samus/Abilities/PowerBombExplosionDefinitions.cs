using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Power Bomb/Crystal Flash blast radii, the ovals their shape streams walk, and phase boundaries.</summary>
public static class PowerBombExplosionDefinitions
{
    /// <summary>Initial unsigned 8.8 radius.</summary>
    public const int InitialRadius = 0x0400;
    /// <summary>Initial pre-explosion unsigned 8.8 radial velocity.</summary>
    public const int InitialPreExplosionSpeed = 0x3000;
    /// <summary>Pre-explosion unsigned 8.8 acceleration.</summary>
    public const int PreExplosionAcceleration = 0x0080;
    /// <summary>Explosion unsigned 8.8 acceleration.</summary>
    public const int ExplosionAcceleration = 0x0030;
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
    /// <summary>Crystal Flash's shorter explosion radius limit.</summary>
    public const int CrystalFlashRadiusLimit = 0x2000;
    /// <summary>
    /// Maximum component of the first native Crystal Flash color at $88:8D85:
    /// RGB5 (14,14,10). This is a lifetime operand, not replacement artwork.
    /// </summary>
    internal const int CrystalFlashInitialFadeSteps = 14;
    /// <summary>
    /// $88:A5BE-$A5CC divides the consumed unsigned 8.8 radius by $0800 to
    /// select a color. Crystal Flash reaches only the first four yellow rows.
    /// </summary>
    internal const int CrystalFlashColorRadiusShift = 11;
    /// <summary>$88:8B96, byte reloaded by ordinary and Crystal Flash afterglow handlers.</summary>
    public const int AfterglowTimerReload = 3;
}
