using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// The pre-coloured sheet of the single sprites drawn over Samus's body (#1392): the arm cannon's cover in each
/// direction and opening frame, and the Power Suit chest cover the forward-facing pose shows.
/// </summary>
public static class SamusOverlaySheetDefinitions
{
    /// <summary>The cover frames an open or opening cannon shows; the closed frame 0 draws nothing.</summary>
    public const int FirstCoverFrame = 1, LastCoverFrame = 3;

    /// <summary>The chest cover's frame.</summary>
    public const string ChestCoverFrame = "ChestCover";

    /// <summary>
    /// <c>$90:868D</c>: the chest cover is standard sprite character $21 in Samus's palette row, in front of every
    /// background.
    /// </summary>
    public static SpritePart ChestCoverAppearance { get; } = SpritePart.Single(character: 33, palette: 4, priority: 3);

    /// <summary>The arm cannon's cover in native direction <paramref name="direction"/> at cover frame <paramref name="frame"/>.</summary>
    public static string ArmCannonFrame(int direction, int frame) => $"ArmCannon.{direction}.{frame}";

    /// <summary>Every frame the sheet holds.</summary>
    public static IEnumerable<string> Frames =>
        Enumerable.Range(0, SamusDrawingDefinitions.ArmCannon.DirectionCount)
            .SelectMany(direction => Enumerable.Range(FirstCoverFrame, LastCoverFrame - FirstCoverFrame + 1)
                .Select(frame => ArmCannonFrame(direction, frame)))
            .Append(ChestCoverFrame);
}
