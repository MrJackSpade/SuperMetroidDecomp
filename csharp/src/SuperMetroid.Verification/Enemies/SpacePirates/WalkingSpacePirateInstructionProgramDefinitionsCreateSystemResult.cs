using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.SpacePirates;

/// <summary>The enemies, slot, state and samus of one walking space pirate instruction program definitions create system result.</summary>
/// <param name="Enemies">The enemies.</param>
/// <param name="Slot">The slot.</param>
/// <param name="State">The state.</param>
/// <param name="Samus">The samus.</param>
internal readonly record struct WalkingSpacePirateInstructionProgramDefinitionsCreateSystemResult(RoomEnemySystem Enemies, RoomEnemySlot Slot, WalkingSpacePirateEnemyState State, SamusState Samus);
