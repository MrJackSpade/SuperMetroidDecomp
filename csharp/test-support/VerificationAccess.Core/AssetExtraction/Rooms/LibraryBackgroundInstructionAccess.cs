namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Verification access to <see cref="LibraryBackgroundInstruction"/> queries only verification asks (#1466).</summary>
internal static class LibraryBackgroundInstructionAccess
{
    /// <summary>A library background command's kind.</summary>
    extension(LibraryBackgroundInstruction self)
    {
        /// <summary>Whether the command copies its source to its destination: a transfer of any kind.</summary>
        internal bool Transfers => self.Command is LibraryBackgroundCommand.Transfer or LibraryBackgroundCommand.TransferForKraid
            or LibraryBackgroundCommand.TransferForDoor;
    }
}
