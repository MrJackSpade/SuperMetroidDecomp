namespace SuperMetroid.Core.Rendering;

/// <summary>Signed Mode 7 registers, retained without floating-point conversion.</summary>
/// <param name="MatrixA">Signed 8.8 coefficient advancing source X per screen pixel ($211B).</param>
/// <param name="MatrixB">Signed 8.8 coefficient advancing source X per physical scanline ($211C).</param>
/// <param name="MatrixC">Signed 8.8 coefficient advancing source Y per screen pixel ($211D).</param>
/// <param name="MatrixD">Signed 8.8 coefficient advancing source Y per physical scanline ($211E).</param>
/// <param name="Center">Source-map center in pixels ($211F/$2120) as signed register words; the renderer sign-extends their low 13 bits.</param>
/// <param name="Scroll">BG1 scroll in pixels ($210D/$210E) as signed register words, interpreted through the native signed 13-bit center-relative calculation.</param>
/// <param name="FillOutsideWithCharacterZero">Uses character zero outside the 1024-by-1024-pixel map when wrapping is disabled, preserving low three pixel-coordinate bits; otherwise outside samples are transparent.</param>
/// <param name="WrapOutsideMap">Wraps source coordinates to ten bits; takes precedence over character-zero fill.</param>
public readonly record struct Mode7RenderRegisters(
    short MatrixA, short MatrixB, short MatrixC, short MatrixD,
    Offset16<Mode7Pixels> Center, Offset16<Mode7Pixels> Scroll,
    bool FillOutsideWithCharacterZero = false, bool WrapOutsideMap = false);
