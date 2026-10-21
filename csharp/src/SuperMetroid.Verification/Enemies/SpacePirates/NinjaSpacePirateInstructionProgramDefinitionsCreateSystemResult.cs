using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.SpacePirates;

/// <summary>The enemies, slot and samus of one ninja space pirate instruction program definitions create system result.</summary>
/// <param name="Enemies">The enemies.</param>
/// <param name="Slot">The slot.</param>
/// <param name="Samus">The samus.</param>
internal readonly record struct NinjaSpacePirateInstructionProgramDefinitionsCreateSystemResult(RoomEnemySystem Enemies, RoomEnemySlot Slot, SamusState Samus);
