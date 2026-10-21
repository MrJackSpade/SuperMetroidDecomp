
namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>A part of Ridley's explosion and the direction he faces.</summary>
/// <param name="Part">The part.</param>
/// <param name="FacingRight">The facing right.</param>
internal readonly record struct RidleyExplosionPartFacing(RidleyExplosionPart Part, bool FacingRight);
