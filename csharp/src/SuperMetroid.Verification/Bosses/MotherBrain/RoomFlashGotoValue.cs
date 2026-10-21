namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>The opcode and target of one room flash goto value.</summary>
/// <param name="Opcode">The opcode.</param>
/// <param name="Target">The target.</param>
internal readonly record struct RoomFlashGotoValue(ushort Opcode, ushort Target);
