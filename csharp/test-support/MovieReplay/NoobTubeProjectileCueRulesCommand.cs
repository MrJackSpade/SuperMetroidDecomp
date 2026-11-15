using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>The kind and operand bytes of one noob tube projectile cue rules command.</summary>
/// <param name="Kind">The kind, or null for setting a routine, whose cue starts the motion that routine stands for.</param>
/// <param name="OperandBytes">The operand bytes.</param>
internal readonly record struct NoobTubeProjectileCueRulesCommand(NoobTubeProjectileCueKind? Kind, int OperandBytes);
