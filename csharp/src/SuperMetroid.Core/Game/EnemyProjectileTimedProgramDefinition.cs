namespace SuperMetroid.Core.Game;

/// <summary>
/// One bank-$86 timed program whose frame durations and control flow affect simulation;
/// <paramref name="Duration"/> gives each frame's hold.
/// Spritemap operands remain outside this definition and select installed presentation content.
/// </summary>
internal readonly record struct EnemyProjectileTimedProgramDefinition(
    ushort InitialPointer,
    int FrameCount,
    ushort? PrefixInstruction,
    ushort TerminalInstruction,
    ushort? TerminalOperand,
    Func<int, ushort> Duration);
