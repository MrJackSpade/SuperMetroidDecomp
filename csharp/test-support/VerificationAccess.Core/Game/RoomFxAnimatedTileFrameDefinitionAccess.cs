using SuperMetroid.Core.Game;

/// <summary>Verification access to <see cref="RoomFxAnimatedTileFrameDefinition"/> members production does not use.</summary>
internal static class RoomFxAnimatedTileFrameDefinitionAccess
{
    /// <summary>Locates the art source operand immediately after a room-FX animated-tile control word.</summary>
    extension(RoomFxAnimatedTileFrameDefinition self)
    {
        /// <summary>Native artwork-operand identity immediately after this compiled control word.</summary>
        internal ushort SourceOperandPointer => unchecked((ushort)(self.InstructionPointer + 2));
    }
}
