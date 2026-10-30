namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// One room block as <see cref="StraddledBlockRead"/> reads it: the block it shows, its mirroring and its collision
/// family's position among the game's sixteen collision families.
/// </summary>
/// <param name="Block">The block shown, 0 through 1023.</param>
/// <param name="Mirroring">How the block is mirrored.</param>
/// <param name="Family">The collision family's position in the game's order, 0 through 15.</param>
[CompatibilityService("a block read straddling two neighbouring blocks, as a Wave shot's scan past the room's ceiling finds it")]
public readonly record struct StraddledBlock(int Block, StraddledBlock.Reflection Mirroring, int Family)
{
    /// <summary>How a straddled block is mirrored.</summary>
    public enum Reflection
    {
        /// <summary>Shown as drawn.</summary>
        None,
        /// <summary>Reflected left to right.</summary>
        Horizontal,
        /// <summary>Reflected top to bottom.</summary>
        Vertical,
        /// <summary>Reflected both ways.</summary>
        Both,
    }

    /// <summary>The highest block number.</summary>
    public const int LastBlock = 1023;

    /// <summary>The highest family position.</summary>
    public const int LastFamily = 15;

    /// <summary>The block shown, 0 through <see cref="LastBlock"/>.</summary>
    public int Block { get; init; } = Ensure.BetweenInclusive(0, LastBlock, Block);

    /// <summary>The collision family's position, 0 through <see cref="LastFamily"/>.</summary>
    public int Family { get; init; } = Ensure.BetweenInclusive(0, LastFamily, Family);
}
