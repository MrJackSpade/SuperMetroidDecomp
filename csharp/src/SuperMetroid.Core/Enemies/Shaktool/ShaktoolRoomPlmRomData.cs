namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>Native Shaktool-room persistence and scroll controller definitions.</summary>
internal static class ShaktoolRoomPlmRomData
{
    /// <summary>$84:B8C3 compares $0348 with Samus X; carry clear marks event $0D.</summary>
    public const ushort ClearedPathXBoundary = 0x348;
    /// <summary>$84:B8B8/B8BF and B8DF/B8E6 write the first four scroll bytes.</summary>
    public const int ScrollCellCount = 4;
}
