namespace SuperMetroid.Core.Fx;

/// <summary>Verification access to <see cref="RoomFxAnimatedTileObjectDefinition"/> members production does not use.</summary>
internal static class RoomFxAnimatedTileObjectDefinitionAccess
{
    /// <summary>Exposes <see cref="RoomFxAnimatedTileObjectDefinition"/> state that only verification reads.</summary>
    extension(RoomFxAnimatedTileObjectDefinition self)
    {
        /// <summary>The address of the terminal <c>goto</c> command after the final frame.</summary>
        public ushort GotoInstructionPointer =>
            unchecked((ushort)(self.InstructionPointer + self.frames.Count * 4));
        /// <summary>
        /// Reads one immutable mechanics word. Frame source operands are intentionally absent.
        /// </summary>
        public bool TryReadMechanicsWord(ushort pointer, out ushort value)
        {
            if (pointer == (ushort)self.ObjectPointer)
                value = self.InstructionPointer;
            else if (pointer == unchecked((ushort)((ushort)self.ObjectPointer + 2)))
                value = self.TransferByteCount;
            else if (pointer == unchecked((ushort)((ushort)self.ObjectPointer + 4)))
                value = self.EncodedVramDestination;
            else if (pointer == self.GotoInstructionPointer)
                value = (ushort)AnimatedTileInstruction.Goto;
            else if (pointer == unchecked((ushort)(self.GotoInstructionPointer + 2)))
                value = self.InstructionPointer;
            else
            {
                foreach (RoomFxAnimatedTileFrameDefinition frame in self.frames)
                {
                    if (pointer != frame.InstructionPointer)
                        continue;

                    value = frame.Duration;
                    return true;
                }

                value = 0;
                return false;
            }

            return true;
        }
    }
}
