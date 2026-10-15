namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>One native bank-$AA common-enemy $814B tile transfer descriptor.</summary>
internal readonly record struct TorizoInstructionVramTransferDefinition(
    ushort Instruction, ushort ByteCount, int SourceAddress, ushort DestinationWord);
