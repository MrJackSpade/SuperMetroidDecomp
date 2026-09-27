namespace SuperMetroid.Core.Assets;

/// <summary>Compiled visual selectors for the bank-$B4 room sprite-object programs.</summary>
internal static class RoomSpriteObjectVisualDefinitions
{
    internal const byte Bank = 0xb4;

    internal static ushort FrameAt(ushort operandAddress)
    {
        if (CompiledEnemyVisualSelectors.TryGet(Bank, operandAddress,
                out ushort frame))
            return frame;
        throw new InvalidDataException(
            $"Room sprite-object visual operand $B4:{operandAddress:X4} is not compiled.");
    }
}
