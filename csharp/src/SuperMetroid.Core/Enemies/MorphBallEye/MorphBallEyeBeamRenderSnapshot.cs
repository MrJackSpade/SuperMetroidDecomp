using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>
/// Immutable eye-beam inputs published with the OAM image by an accepted NMI.
/// </summary>
/// <remarks>
/// The eye body, its bank-$88 HDMA object, and layer-one scroll all advance during the
/// main loop. The PPU does not see any of those shadow values until NMI. Keeping this
/// compact presentation record prevents the software compositor from pairing yesterday's
/// OAM with today's cone angle or camera position.
/// </remarks>
/// <param name="Phase">Beam lifetime phase latched with the accepted NMI's actor image.</param>
/// <param name="WorldPosition">Eye body's origin in whole room pixels, before layer-one scroll is subtracted.</param>
/// <param name="Angle">Latched beam aim, measured clockwise from up in 256 table units per turn.</param>
/// <param name="AngularWidth">Whole angular half-width in table units, zero through four; fractional widening state is not included in this render snapshot.</param>
/// <param name="Color">The colour the beam adds inside its cone, latched with the accepted NMI's actor image.</param>
public readonly record struct MorphBallEyeBeamRenderSnapshot(
    MorphBallEyeBeamPhase Phase,
    Position16<RoomPixels> WorldPosition,
    RotationAngle Angle,
    int AngularWidth,
    RgbColor Color);
