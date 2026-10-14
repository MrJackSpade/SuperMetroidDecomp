namespace SuperMetroid.Core.Game;

/// <summary>
/// Origin and edge description of Mother Brain's rainbow beam, resolved by the simulation from
/// <c>$AD:DE00-$DE5D</c> and rasterized only on the render side.
/// </summary>
/// <param name="MouthScreenX">Native <c>(head X + 14) &amp; $FF</c> low byte used as the window apex; not a camera-relative coordinate.</param>
/// <param name="MouthY">Head Y plus five, validated by the simulation to lie strictly inside the native scanline work area.</param>
/// <param name="EdgeA">Wrapped lower edge angle: aim table index minus half the angular width, in 256-unit-turn table units.</param>
/// <param name="EdgeB">Wrapped upper edge angle: aim table index plus half the angular width, in 256-unit-turn table units.</param>
/// <param name="Direction">Dispatcher selected from the edge quadrants: right, up or down, never retain or unsupported.</param>
public readonly record struct MotherBrainRainbowBeamGeometry(
    byte MouthScreenX,
    int MouthY,
    byte EdgeA,
    byte EdgeB,
    MotherBrainBeamRomData.Direction Direction);
