using SuperMetroid.Core.Assets;
using static SuperMetroid.Core.Frontend.Ending.EndingShootingStarDefinitions;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// Names and draws the ending shooting star's sheet frames (<see cref="EndingShootingStarDefinitions"/>).
/// </summary>
internal static class EndingShootingStarFrames
{
    /// <summary>The sheet frame showing a visible animation frame (#1392).</summary>
    internal static string SheetFrameName(int frame) => frame is > 0 and < FrameCount
        ? $"ShootingStar.{Math.Min(frame, FullStarFrame)}"
        : throw new ArgumentOutOfRangeException(nameof(frame));

    /// <summary>
    /// $8B:E9A7 / Handle_ShootingStars.tilemapValues: frame 0 is the blank piece (character 0, palette 0), frames 1 to 3
    /// grow through characters $1F0 to $1F2, and every later frame holds the full star at $1F3, all in sprite palette 4.
    /// </summary>
    internal static SpritePart Appearance(int frame) => frame switch
    {
        0 => SpritePart.Single(0, 0, 0),
        > 0 and < FrameCount => SpritePart.Single(CharacterSheetGrid.Cell(31, Math.Min(frame - 1, 3)), 4, 0),
        _ => throw new ArgumentOutOfRangeException(nameof(frame)),
    };
}
