namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>One packed VRAM write-table record emitted by bank-$84 PLM bytecode.</summary>
public readonly record struct PlmVramWriteRequest(
    int SizeInBytes,
    int SourceAddress,
    VramDmaDestination Destination)
{
    /// <summary>The bytes the write moves, 0 through 65535.</summary>
    public int SizeInBytes { get; } = Ensure.BetweenInclusive(0, ushort.MaxValue, SizeInBytes);

}
