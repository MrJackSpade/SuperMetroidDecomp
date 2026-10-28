using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>
/// The eye beam's effect as a frame shows it: the cone of added colour cast from the eye toward Samus. The frame-end
/// latch takes it from the beam and its eye together, so the cone shown always pairs the eye's position and aim with
/// the beam's width and colour from the same frame.
/// </summary>
/// <param name="Phase">The beam's lifetime phase.</param>
/// <param name="Origin">The eye's position in whole room pixels, where the cone starts.</param>
/// <param name="Angle">The eye's aim, measured clockwise from up in 256 units per turn.</param>
/// <param name="AngularWidth">The cone's whole angular half-width in the same units, zero through four.</param>
/// <param name="Color">The colour the beam adds inside its cone.</param>
public readonly record struct MorphBallEyeBeamCone(
    MorphBallEyeBeamPhase Phase,
    Position16<RoomPixels> Origin,
    RotationAngle Angle,
    int AngularWidth,
    RgbColor Color);
