namespace SuperMetroid.Core.Game;

/// <summary>One owner's private enemy-instruction opcodes; returns false for a word it does not own.</summary>
internal delegate bool EnemyOwnedInstructionHandler(
    RoomEnemySlot slot, EnemyInstructionContext context, ushort word, ref ushort cursor, out bool yieldInterpreter);
