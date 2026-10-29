using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>The ending shooting star's single-character appearances, which its sheet frames picture (#1463).</summary>
internal static class EndingShootingStarParts
{
    /// <summary>
    /// $8B:E9A7 / Handle_ShootingStars.tilemapValues: frame 0 is the blank piece (character 0, palette 0), frames 1 to 3
    /// grow through characters $1F0 to $1F2, and every later frame holds the full star at $1F3, all in sprite palette 4.
    /// </summary>
    internal static SpritePart Appearance(int frame) => frame switch
    {
        0 => SpritePart.Single(0, 0, 0),
        > 0 and < EndingShootingStarDefinitions.FrameCount => SpritePart.Single(CharacterSheetGrid.Cell(31, Math.Min(frame - 1, 3)), 4, 0),
        _ => throw new ArgumentOutOfRangeException(nameof(frame)),
    };
}
