namespace SuperMetroid.Core.Programs;

/// <summary>Verification access to <see cref="InstructionProgramLayout.WordIndex"/> members production does not use.</summary>
internal static class InstructionProgramLayoutWordIndexAccess
{
    /// <summary>Exposes <see cref="InstructionProgramLayout.WordIndex"/> state that only verification reads.</summary>
    extension(InstructionProgramLayout.WordIndex self)
    {
            /// <summary>Finds an owned word without scanning the program's item sequence.</summary>
            internal bool TryGet(ushort address, out InstructionWord word)
            {
                bool found = self.byAddress.TryGetValue(address, out int position);
                word = found ? self.Words[position].Word : default;
                return found;
            }
    }
}
