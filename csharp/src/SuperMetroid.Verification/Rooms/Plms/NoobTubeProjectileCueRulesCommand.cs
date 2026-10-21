using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>The kind and operand bytes of one noob tube projectile cue rules command.</summary>
/// <param name="Kind">The kind.</param>
/// <param name="OperandBytes">The operand bytes.</param>
internal readonly record struct NoobTubeProjectileCueRulesCommand(NoobTubeProjectileCueKind Kind, int OperandBytes);
