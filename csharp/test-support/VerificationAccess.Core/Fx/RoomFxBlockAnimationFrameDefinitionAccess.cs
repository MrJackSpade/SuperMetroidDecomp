namespace SuperMetroid.Core.Fx;

/// <summary>Verification access to the native cursors of a <see cref="RoomFxBlockAnimationLoop"/>'s timed frames.</summary>
internal static class RoomFxBlockAnimationFrameDefinitionAccess
{
    /// <summary>Locates the art source operand immediately after a room-FX animated-tile control word.</summary>
    extension(RoomFxBlockAnimationLoop self)
    {
        /// <summary>The bank-$87 cursor of the timed control word of the loop's frame <paramref name="index"/>.</summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not one of the loop's frames.</exception>
        internal ushort FrameInstructionPointer(int index) =>
            (uint)index < (uint)self.Frames.Count
                ? unchecked((ushort)(RoomFxBlockAnimationDefinitionAccess.FirstCursor(self.AnimatedObject) + index * 4))
                : throw new ArgumentOutOfRangeException(nameof(index), index, $"{self.AnimatedObject} has no frame {index}.");

        /// <summary>Native artwork-operand identity immediately after the compiled control word of frame <paramref name="index"/>.</summary>
        internal ushort FrameSourceOperandPointer(int index) => unchecked((ushort)(self.FrameInstructionPointer(index) + 2));
    }
}
