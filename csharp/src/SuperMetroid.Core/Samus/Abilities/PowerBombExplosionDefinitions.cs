using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Power Bomb/Crystal Flash HDMA radii, shape streams, and phase boundaries.</summary>
public static class PowerBombExplosionDefinitions
{
    /// <summary>Initial unsigned 8.8 radius.</summary>
    public const ushort InitialRadius = 0x0400;
    /// <summary>Initial pre-explosion unsigned 8.8 radial velocity.</summary>
    public const ushort InitialPreExplosionSpeed = 0x3000;
    /// <summary>Pre-explosion unsigned 8.8 acceleration.</summary>
    public const ushort PreExplosionAcceleration = 0x0080;
    /// <summary>Explosion unsigned 8.8 acceleration.</summary>
    public const ushort ExplosionAcceleration = 0x0030;
    /// <summary>The authored yellow ellipse frames.</summary>
    /// <remarks>
    /// #625 exact pre-scaled profile generator: yellow frame f=0..3 uses n=f+3 and
    /// R=min(255, floor(4+48*n-n*(n-1)/4)). This is the initial radius 4, velocity 48,
    /// and per-frame deceleration 1/2, integrated before discarding the fractional radius.
    /// Feed R into the quantized ellipse/scaling recipe on WhiteShapeFrames. All 768
    /// bytes match the ROM and pinned PowerBomb_PreExplosion_ShapeDefinitionTables_PreScaled.
    /// The last frame clamps R to 255; it is not a per-row correction. Use the authored
    /// frame number, not the live radius: stage boundaries affect runtime accumulator timing.
    /// </remarks>
    public const int YellowShapeFrames = 4;
    /// <summary>The authored white ellipse frames.</summary>
    /// <remarks>
    /// #625 exact, correction-free generator for all 17*192 white-profile bytes:
    /// frame f=0..16 uses n=f+37 and R=min(255, 4+floor(3*n*(n-1)/32)). This integrates
    /// radius 4, initial speed zero, acceleration 3/16; round only after the quadratic sum.
    /// Generate a 192-row unit profile B: for j=0..191, k=floor(512*asin((j+1)/192)/pi),
    /// then B(j)=min(255, floor(256*cos(k*pi/512))). The endpoint j=191 is exactly zero.
    /// Thus the ellipse uses a 1,024-step full circle BEFORE rasterization, not a smooth square root.
    /// For output row y=0..191 let j=max(0, ceil(256*y/R)-1), equivalently
    /// max(0, (256*y-1)/R) with integer division. Return zero if j&gt;=192, else floor(B(j)*R/256).
    /// Preserve both quantization stages and the lower-side inverse-scale boundary.
    /// The #1165 research verified all 4,032 white/yellow bytes against NTSC J/U v1.0 ROM
    /// and pinned bank_88.asm. Its deterministic decimal implementation selects k by bounded
    /// sine comparisons rather than platform asin, and certified every resulting integer.
    /// Both true pi and 3.14159 reproduce the bytes, so their historical choice is undetermined.
    /// A continuous ellipse basis misses 955 bytes; ordinary floor(256*y/R) misses 59.
    /// Full-circle resolutions 256, 512, 2048, and 4096 also fail. No stored shape/radius table
    /// or per-entry exceptions are required. These are authored frame profiles: do not substitute
    /// the live ExplosionRadius, whose white-phase threshold is offset from n=37.
    /// Runtime replacement, caching strategy, and performance verification remain deferred.
    /// </remarks>
    public const int WhiteShapeFrames = 17;
    /// <summary>Bytes occupied by one ellipse definition: 192 one-byte pixel-row widths.</summary>
    public const ushort ShapeStride = 192;
    /// <summary>Radius at which white pre-explosion becomes yellow.</summary>
    public static readonly PowerBombRadius PreExplosionWhiteLimit = PowerBombRadius.FromWholePixels(146);
    /// <summary>Radius at which yellow explosion switches to white shapes.</summary>
    public static readonly PowerBombRadius ExplosionYellowLimit = PowerBombRadius.FromWholePixels(134);
    /// <summary>Crystal Flash's shorter explosion radius limit.</summary>
    public const ushort CrystalFlashRadiusLimit = 0x2000;
    /// <summary>
    /// Maximum component of the first native Crystal Flash color at $88:8D85:
    /// RGB5 (14,14,10). This is a lifetime operand, not replacement artwork.
    /// </summary>
    internal const byte CrystalFlashInitialFadeSteps = 14;
    /// <summary>
    /// $88:A5BE-$A5CC divides the consumed unsigned 8.8 radius by $0800 to
    /// select a color. Crystal Flash reaches only the first four yellow rows.
    /// </summary>
    internal const int CrystalFlashColorRadiusShift = 11;
    /// <summary>$88:8B96, byte reloaded by ordinary and Crystal Flash afterglow handlers.</summary>
    public const byte AfterglowTimerReload = 3;
}
