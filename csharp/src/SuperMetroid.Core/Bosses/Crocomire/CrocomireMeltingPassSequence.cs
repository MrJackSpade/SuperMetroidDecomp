namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Enumerates the two native melt passes without stored records.</summary>
internal readonly record struct CrocomireMeltingPassSequence()
{
    /// <summary>Decodes the first or second native melt header by pass ordinal.</summary>
    internal CrocomireMeltingPass this[int index] => index switch
    {
        0 => CrocomireMeltingTransferDefinitions.Header((ushort)CrocomireMeltingHeader.First),
        1 => CrocomireMeltingTransferDefinitions.Header((ushort)CrocomireMeltingHeader.Second),
        _ => throw new IndexOutOfRangeException(),
    };
}
