using SuperMetroid.Core.Enemies.Beetom;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.Beetom;

/// <summary>The enemies, state and choose of one create beetom distant selection fixture result.</summary>
/// <param name="Enemies">The enemies.</param>
/// <param name="State">The state.</param>
/// <param name="Choose">The choose.</param>
internal readonly record struct CreateBeetomDistantSelectionFixtureResult(RoomEnemySystem Enemies, BeetomEnemyState State, Action<BeetomEnemyState> Choose);
