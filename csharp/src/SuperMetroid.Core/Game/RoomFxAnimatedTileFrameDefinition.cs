namespace SuperMetroid.Core.Game;

/// <summary>One timed control word preceding a live presentation-source operand.</summary>
public readonly record struct RoomFxAnimatedTileFrameDefinition(
    ushort InstructionPointer,
    ushort Duration)
{
}
