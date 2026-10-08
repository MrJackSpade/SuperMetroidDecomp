namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Independent visual transforms stored in level-word bits ten and eleven. Collision type
/// and block index remain separate packed fields and therefore do not belong in this enum.
/// </summary>
[Flags]
public enum LevelBlockFlipFlags : ushort
{
    /// <summary>No visual reflection bits are set in the level word.</summary>
    None = 0,
    /// <summary>Level-word bit ten ($0400), reflecting the block's graphics horizontally.</summary>
    Horizontal = 0x0400,
    /// <summary>Level-word bit eleven ($0800), reflecting the block's graphics vertically.</summary>
    Vertical = 0x0800,
}
