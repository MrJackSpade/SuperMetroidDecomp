using SuperMetroid.Core.Areas.Tourian;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>The cue and operand bytes of one tourian statue projectile animations command.</summary>
/// <param name="Cue">The cue.</param>
/// <param name="OperandBytes">The operand bytes.</param>
internal readonly record struct TourianStatueProjectileAnimationsCommand(NativeCueShape<TourianStatueProjectileCue> Cue, int OperandBytes);
