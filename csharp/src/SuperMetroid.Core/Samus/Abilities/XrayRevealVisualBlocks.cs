namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// Replaceable blocks one X-ray reveal shows: block numbers in the room's tileset block set, shown unmirrored, as every
/// reveal copy lays its blocks out as they are defined.
/// </summary>
/// <param name="TopLeft">The first block, used by every drawable reveal.</param>
/// <param name="TopRight">The block to its right, for wide and square copies.</param>
/// <param name="BottomLeft">The block below it, for tall and square copies.</param>
/// <param name="BottomRight">The diagonal block, for square copies; other copy shapes leave it unused.</param>
public readonly record struct XrayRevealVisualBlocks(
    int TopLeft,
    int TopRight,
    int BottomLeft,
    int BottomRight);
