using SuperMetroid.Core.Bosses.Ridley;

namespace SuperMetroid.Verification.Bosses.Ridley;

/// <summary>The x, facing and turn of one ridley full movie case.</summary>
/// <param name="X">The x.</param>
/// <param name="Facing">The facing.</param>
/// <param name="Turn">The turn.</param>
internal readonly record struct RidleyFullMovieCase(ushort X, RidleyFacing Facing, bool Turn);
