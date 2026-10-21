namespace SuperMetroid.Verification.Rooms;

/// <summary>The source address and byte count of one verify installed background transfer failures case.</summary>
/// <param name="SourceAddress">The source address.</param>
/// <param name="ByteCount">The byte count.</param>
internal readonly record struct VerifyInstalledBackgroundTransferFailuresCase(int SourceAddress, ushort ByteCount);
