namespace SuperMetroid.Core.Hardware;

/// <summary>The installed artwork a room's native enemy tile upload selects, shown in the OBJ character table.</summary>
/// <param name="SourceAddress">The native upload's source address.</param>
/// <param name="ByteCount">The native upload's byte count.</param>
public sealed record TransferObjCharacters(int SourceAddress, int ByteCount) : ObjCharacterSource
{
    /// <inheritdoc/>
    public override ReadOnlyMemory<byte> Resolve(IObjCharacterSourceResolver resolver) =>
        Ensure.NotNull(resolver).ResolveCartridgeSource(SourceAddress, ByteCount);

    /// <summary>Names the source for diagnostics.</summary>
    public override string ToString() => $"transfer ${SourceAddress:X6}+{ByteCount}";
}
