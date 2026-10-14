namespace SuperMetroid.Core.Game;

/// <summary>One presentation-only spritemap operand in a compiled bank-$86 program.</summary>
internal readonly record struct EnemyProjectilePresentationFrameDefinition(
    ushort OperandAddress,
    string Name);
