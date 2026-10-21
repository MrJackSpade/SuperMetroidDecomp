using SuperMetroid.Core.Enemies.Metroids;

namespace SuperMetroid.Verification.Enemies.Metroids;

/// <summary>The x, y, state, timer and health of one metroid bomb placement expected.</summary>
/// <param name="X">The x.</param>
/// <param name="Y">The y.</param>
/// <param name="State">The state.</param>
/// <param name="Timer">The timer.</param>
/// <param name="Health">The health.</param>
internal readonly record struct MetroidBombPlacementExpected(ushort X, ushort Y, MetroidAiFunction State, ushort Timer, ushort Health);
