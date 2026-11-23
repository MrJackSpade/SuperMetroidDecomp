using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>What one update of Phantoon's current phase runs on (#2571), handed to the phase as it runs.</summary>
/// <param name="Phantoon">His species, whose opening, patterns and death the phases run.</param>
/// <param name="State">His encounter.</param>
/// <param name="Samus">Samus, whom his eye and swoops follow.</param>
internal readonly partial record struct PhantoonPhaseTurn(PhantoonBehavior Phantoon, PhantoonEnemyState State, SamusState Samus);
