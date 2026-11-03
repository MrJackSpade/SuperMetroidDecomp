using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Verification.Enemies.SpacePirates;

/// <summary>The slot and program of one initialize result.</summary>
/// <param name="Slot">The slot.</param>
/// <param name="Program">The program.</param>
internal readonly record struct InitializeResult(RoomEnemySlot Slot, AnimationProgramCursor<NinjaSpacePirateProgram> Program);
