using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>What one update of the self-destruct warning's current phase runs on (#2571), handed to the phase as it runs.</summary>
/// <param name="Escape">The station's escape, whose warning the phase runs.</param>
/// <param name="RidleyFrameCounter">Ridley's body's update count, which paces the alarm.</param>
/// <param name="Warning">The room's foreground warning, which the warning's phases write (#2554).</param>
[RoomScopeBoundary("A per-call input, built for one call and never kept.")]
internal readonly partial record struct CeresSelfDestructTurn(CeresEscape Escape, int RidleyFrameCounter, RoomWarning Warning);
