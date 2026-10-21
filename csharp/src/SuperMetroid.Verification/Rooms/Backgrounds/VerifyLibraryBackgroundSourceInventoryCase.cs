namespace SuperMetroid.Verification.Rooms.Backgrounds;

/// <summary>The source address, transfer byte count and vram destination of one verify library background source inventory case.</summary>
/// <param name="SourceAddress">The source address.</param>
/// <param name="TransferByteCount">The transfer byte count.</param>
/// <param name="VramDestination">The vram destination.</param>
internal readonly record struct VerifyLibraryBackgroundSourceInventoryCase(int SourceAddress, ushort? TransferByteCount, ushort? VramDestination);
