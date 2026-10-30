namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>How a room block's picture is mirrored where the block is placed. The two reflections are independent.</summary>
[Flags]
public enum BlockMirroring
{
    /// <summary>The block is shown as drawn.</summary>
    None = 0,
    /// <summary>The block is reflected left to right.</summary>
    Horizontal = 1,
    /// <summary>The block is reflected top to bottom.</summary>
    Vertical = 2,
    /// <summary>The block is reflected both ways.</summary>
    Both = Horizontal | Vertical,
}
