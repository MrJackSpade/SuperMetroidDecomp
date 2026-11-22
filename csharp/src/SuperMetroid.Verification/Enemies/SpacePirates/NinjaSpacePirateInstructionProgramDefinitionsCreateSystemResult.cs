using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.SpacePirates;

/// <summary>The enemies, slot and samus of one ninja space pirate instruction program definitions create system result.</summary>
/// <param name="Enemies">The enemies.</param>
/// <param name="Place">The slot.</param>
/// <param name="Samus">The samus.</param>
/// <param name="State">The state the Pirate's setup built.</param>
internal readonly record struct NinjaSpacePirateInstructionProgramDefinitionsCreateSystemResult(RoomEnemySystem Enemies, RoomEnemyPlace Place, SamusState Samus,
    NinjaSpacePirateEnemyState State);
