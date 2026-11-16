using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.FuneNamihe;

/// <summary>The system and slot of one run fune namihe program result.</summary>
/// <param name="System">The system.</param>
/// <param name="Slot">The slot.</param>
internal readonly record struct RunFuneNamiheProgramResult(RoomEnemySystem System, RoomEnemySlot Slot);
