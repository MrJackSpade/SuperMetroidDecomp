namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>The two melt-pass header offsets from $A4:9BC5, Crocomire's serialized melt cursor.</summary>
internal enum CrocomireMeltingHeader : ushort
{
    /// <summary>First header's native offset from $A4:9BC5.</summary>
    First = 0x0000,
    /// <summary>Second header's native offset from $A4:9BC5.</summary>
    Second = 0x0054,
}
