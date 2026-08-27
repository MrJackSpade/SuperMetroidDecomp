namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Independent visual transforms stored in level-word bits ten and eleven. Collision type
/// and block index remain separate packed fields and therefore do not belong in this enum.
/// </summary>
[Flags]
public enum LevelBlockFlipFlags : ushort
{
    None = 0,
    Horizontal = 0x0400,
    Vertical = 0x0800,
}
