using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>Verification access to <see cref="WreckedShipTreadmillObjectDefinition"/> members production does not use.</summary>
internal static class WreckedShipTreadmillObjectDefinitionAccess
{
    /// <summary>Reads the four timed frame-control pointers for one Wrecked Ship treadmill direction.</summary>
    extension(WreckedShipTreadmillObjectDefinition self)
    {
        /// <summary>The boss-wait command installed by the object header.</summary>
        public ushort WaitInstructionPointer => self.Direction == WreckedShipTreadmillDirection.Rightwards
            ? AnimatedTileInstructionListPointers.WreckedShipTreadmillRightwardsWait
            : AnimatedTileInstructionListPointers.WreckedShipTreadmillLeftwardsWait;
        /// <summary>The first timed frame reached after Phantoon is defeated.</summary>
        public ushort LoopInstructionPointer => unchecked((ushort)(self.WaitInstructionPointer + 2));
        /// <summary>The four timed frame-control addresses in execution order.</summary>
        internal IReadOnlyList<ushort> FrameInstructionPointers =>
            [.. Enumerable.Range(0, 4).Select(index => unchecked((ushort)(self.LoopInstructionPointer + 4 * index)))];
        /// <summary>Maps a four-byte-aligned timed instruction cursor to its frame ordinal, returning -1 outside the loop.</summary>
        internal int FrameIndex(ushort pointer)
        {
            int delta = unchecked((ushort)(pointer - self.LoopInstructionPointer));
            return delta < 16 && (delta & 3) == 0 ? delta / 4 : -1;
        }
        /// <summary>
        /// Returns the native artwork identity for a frame-control record without reading
        /// its bank-$87 presentation operand during installed play.
        /// </summary>
        public int FrameSourceAddress(ushort instructionPointer)
        {
            int listIndex = self.FrameIndex(instructionPointer);
            if (listIndex < 0)
                throw new InvalidDataException(
                    $"Treadmill $87:{(int)self.AnimatedObject:X4} has no frame at $87:{instructionPointer:X4}.");
            return self.FrameSource(listIndex);
        }
        /// <summary>The terminal loop command after the fourth frame.</summary>
        public ushort GotoInstructionPointer =>
            unchecked((ushort)(self.LoopInstructionPointer + 16));
        /// <summary>Reads one immutable mechanics word, excluding presentation source operands.</summary>
        public bool TryReadMechanicsWord(ushort pointer, out ushort value)
        {
            if (pointer == (ushort)self.AnimatedObject)
                value = self.WaitInstructionPointer;
            else if (pointer == unchecked((ushort)(self.AnimatedObject + 2)))
                value = WreckedShipTreadmillRomData.TransferByteCount;
            else if (pointer == unchecked((ushort)(self.AnimatedObject + 4)))
                value = WreckedShipTreadmillRomData.EncodedVramDestination;
            else if (pointer == self.WaitInstructionPointer)
                value = (ushort)AnimatedTileInstruction.WaitUntilAreaBossIsDead;
            else if (self.FrameIndex(pointer) >= 0)
                value = WreckedShipTreadmillMechanicsDefinitions.FrameDuration;
            else if (pointer == self.GotoInstructionPointer)
                value = (ushort)AnimatedTileInstruction.Goto;
            else if (pointer == unchecked((ushort)(self.GotoInstructionPointer + 2)))
                value = self.LoopInstructionPointer;
            else
            {
                value = 0;
                return false;
            }

            return true;
        }
    }
}
