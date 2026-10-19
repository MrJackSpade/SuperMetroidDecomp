namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Origin and edge description of Mother Brain's rainbow beam, resolved by the simulation from
/// <c>$AD:DE00-$DE5D</c> and rasterized only on the render side.
/// </summary>
/// <param name="MouthScreenX">The window apex, 0 through 255: head X plus fourteen, wrapped to the 256-pixel window line; not a camera-relative coordinate.</param>
/// <param name="MouthY">Head Y plus five, validated by the simulation to lie strictly inside the native scanline work area.</param>
/// <param name="EdgeA">Lower edge angle, 0 through 255: aim table index minus half the angular width, wrapped around the 256-unit turn.</param>
/// <param name="EdgeB">Upper edge angle, 0 through 255: aim table index plus half the angular width, wrapped around the 256-unit turn.</param>
/// <param name="Direction">Dispatcher selected from the edge quadrants: right, up or down, never retain or unsupported.</param>
public readonly record struct MotherBrainRainbowBeamGeometry(
    int MouthScreenX,
    int MouthY,
    int EdgeA,
    int EdgeB,
    MotherBrainBeamRomData.Direction Direction);
