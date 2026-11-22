using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>What one update of Ceres Ridley's current phase runs on (#2571), handed to the phase as it runs.</summary>
/// <param name="Ridley">His encounter's behaviour, whose flight and escape the phases run.</param>
/// <param name="Place">His body.</param>
/// <param name="State">His encounter.</param>
/// <param name="Samus">Samus, whom his attacks aim at.</param>
internal readonly partial record struct CeresRidleyPhaseTurn(CeresRidleyBehavior Ridley, RoomEnemyPlace Place, CeresRidleyState State, SamusState Samus);
