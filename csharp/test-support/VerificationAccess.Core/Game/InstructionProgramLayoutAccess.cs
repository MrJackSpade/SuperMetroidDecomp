using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Verification access to <see cref="InstructionProgramLayout"/> members production does not use.</summary>
internal static class InstructionProgramLayoutAccess
{
    /// <summary>Walks a compiled instruction layout from an entry to count timed frames and locate its terminator.</summary>
    extension(InstructionProgramLayout self)
    {
        /// <summary>
        /// Counts the timed frames that run from <paramref name="entry"/> and returns the opcode of
        /// the first control instruction after them.
        /// </summary>
        internal (int Frames, ushort Terminator) FramesFrom(ushort entry)
        {
            int index = Array.FindIndex(self.items, item => item.Kind == InstructionItemKind.Entry && item.Value == entry);
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(entry), entry, "Entry is not part of this layout.");
            int frames = 0;
            for (index++; index < self.items.Length; index++)
            {
                InstructionItem item = self.items[index];
                if (item.Kind != InstructionItemKind.Words) continue;
                if (item.Words is [{ IsPresentation: false }, { IsPresentation: true }]) { frames++; continue; }
                return (frames, item.Words[0].Value);
            }
            throw new InvalidDataException($"Instruction entry ${self.Bank:X2}:{entry:X4} has no terminating instruction.");
        }

        /// <summary>Finds the presentation slot containing either byte at <paramref name="longAddress"/>.</summary>
        internal bool TryGetPresentationWord(int longAddress, out ushort wordAddress)
        {
            wordAddress = 0;
            if (SnesAddressMath.BankNumber(longAddress) != self.Bank) return false;
            int bankAddress = SnesAddressMath.OffsetInBank(longAddress);
            // Words never overlap, so the byte belongs to the word starting one byte before it or at it.
            if (self.TryWordContaining(bankAddress, out ushort start, out InstructionWord word) && word.IsPresentation)
            {
                wordAddress = start;
                return true;
            }
            return false;
        }
    }
}
