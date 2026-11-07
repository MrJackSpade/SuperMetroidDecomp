using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Verification.Samus.Abilities;

/// <summary>The bts, area and respawns of one speed booster roomObject programs case.</summary>
/// <param name="Bts">The bts.</param>
/// <param name="Area">The area.</param>
/// <param name="Respawns">The respawns.</param>
internal readonly record struct SpeedBoosterObjectProgramsCase(RoomBlockBehavior Bts, AreaId Area, bool Respawns);
