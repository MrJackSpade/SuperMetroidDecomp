using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>What one update of the self-destruct warning's current phase runs on (#2571), handed to the phase as it runs.</summary>
/// <param name="Escape">The station's escape, whose warning the phase runs.</param>
/// <param name="RidleyFrameCounter">Ridley's body's update count, which paces the alarm.</param>
[RoomScopeBoundary("A per-call input, built for one call and never kept.")]
internal readonly record struct CeresSelfDestructTurn(CeresEscape Escape, int RidleyFrameCounter);
