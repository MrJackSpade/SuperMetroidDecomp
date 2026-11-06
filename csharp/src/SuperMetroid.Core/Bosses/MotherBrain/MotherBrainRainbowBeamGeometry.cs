namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Origin and edge description of Mother Brain's rainbow beam, resolved by the simulation
/// and rasterized only on the render side.
/// </summary>
/// <param name="MouthScreenX">The window apex: the mouth's screen X, the head's room X plus fourteen less the camera's.</param>
/// <param name="MouthY">The mouth's screen Y, the head's room Y plus five less the camera's, validated by the simulation to lie strictly inside the scanline work area.</param>
/// <param name="EdgeA">Lower edge angle, 0 through 255: aim table index minus half the angular width, wrapped around the 256-unit turn.</param>
/// <param name="EdgeB">Upper edge angle, 0 through 255: aim table index plus half the angular width, wrapped around the 256-unit turn.</param>
public readonly record struct MotherBrainRainbowBeamGeometry(
    int MouthScreenX,
    int MouthY,
    int EdgeA,
    int EdgeB);
