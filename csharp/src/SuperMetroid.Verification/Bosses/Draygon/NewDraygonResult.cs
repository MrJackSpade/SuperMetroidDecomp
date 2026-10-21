using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Bosses.Draygon;

/// <summary>The enemies and state of one new draygon result.</summary>
/// <param name="Enemies">The enemies.</param>
/// <param name="State">The state.</param>
internal readonly record struct NewDraygonResult(RoomEnemySystem Enemies, DraygonEnemyState State);
