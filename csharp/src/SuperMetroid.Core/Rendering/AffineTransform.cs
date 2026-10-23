namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A Mode 7 floor's affine transform (#1403): the signed 8.8 matrix stepping through the floor per screen pixel and per
/// scanline, the floor point it turns about, the floor's scroll, and what shows beyond the floor's 1024 by 1024 pixels.
/// Kept as the integers the scene computes, so both renderers project exactly as the cartridge's frames did.
/// </summary>
/// <param name="MatrixA">Signed 8.8 step through floor X per screen pixel.</param>
/// <param name="MatrixB">Signed 8.8 step through floor X per scanline.</param>
/// <param name="MatrixC">Signed 8.8 step through floor Y per screen pixel.</param>
/// <param name="MatrixD">Signed 8.8 step through floor Y per scanline.</param>
/// <param name="Center">The floor point the transform turns about; the renderers sign-extend its low 13 bits.</param>
/// <param name="Scroll">The floor's scroll, taken relative to <paramref name="Center"/> through the same signed 13-bit calculation.</param>
/// <param name="Outside">What shows beyond the floor's edges.</param>
public readonly record struct AffineTransform(
    short MatrixA, short MatrixB, short MatrixC, short MatrixD,
    Offset16<Mode7Pixels> Center, Offset16<Mode7Pixels> Scroll,
    Mode7OverflowMode Outside = Mode7OverflowMode.Transparent);
