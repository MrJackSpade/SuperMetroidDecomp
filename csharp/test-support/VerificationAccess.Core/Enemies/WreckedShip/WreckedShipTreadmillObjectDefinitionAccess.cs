using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>Verification access to the bank-$87 program a Wrecked Ship treadmill's loop replaces.</summary>
internal static class WreckedShipTreadmillObjectDefinitionAccess
{
    /// <summary>Reads the boss wait and the four timed frame-control pointers of one Wrecked Ship treadmill's program.</summary>
    extension(BlockAnimation self)
    {
        /// <summary>The boss-wait command installed by the object header.</summary>
        /// <exception cref="InvalidDataException">The loop is not a treadmill's.</exception>
        public ushort WaitInstructionPointer =>
            self == BlockAnimation.WreckedShipTreadmillRightwards ? AnimatedTileAnimationScriptPointers.WreckedShipTreadmillRightwardsWait
            : self == BlockAnimation.WreckedShipTreadmillLeftwards ? AnimatedTileAnimationScriptPointers.WreckedShipTreadmillLeftwardsWait
            : throw new InvalidDataException($"{self} is not a Wrecked Ship treadmill.");
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
                    $"Treadmill {self} has no frame at $87:{instructionPointer:X4}.");
            return AnimatedTileImageSources.Of(self.TreadmillImage(listIndex));
        }
    }
}
