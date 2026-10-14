namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Bounded transitions into <see cref="InWorldCollectibleKind"/>.</summary>
internal static class InWorldCollectibleKinds
{
    /// <summary>Selects the kind at one bank-$84 header-table entry; entries beyond the table throw.</summary>
    internal static InWorldCollectibleKind AtHeaderEntry(int entry)
    {
        if ((uint)entry > byte.MaxValue || !Enum.IsDefined((InWorldCollectibleKind)entry))
            throw new InvalidOperationException($"Collectible header-table entry {entry} is not an in-world collectible kind.");
        return (InWorldCollectibleKind)entry;
    }
}
