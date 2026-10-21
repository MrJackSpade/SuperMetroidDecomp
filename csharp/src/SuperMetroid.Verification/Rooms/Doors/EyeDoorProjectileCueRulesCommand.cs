using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>The kind and operand bytes of one eye door projectile cue rules command.</summary>
/// <param name="Kind">The kind.</param>
/// <param name="OperandBytes">The operand bytes.</param>
internal readonly record struct EyeDoorProjectileCueRulesCommand(EyeDoorProjectileCueKind Kind, int OperandBytes);
