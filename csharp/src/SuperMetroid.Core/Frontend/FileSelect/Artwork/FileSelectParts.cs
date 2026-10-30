using static SuperMetroid.Core.Frontend.FileSelect.Artwork.FileSelectPresentationDefinitions;

namespace SuperMetroid.Core.Frontend.FileSelect.Artwork;

/// <summary>
/// Names the file-select document's parts (<see cref="FileSelectPresentationDefinitions"/>): its pages, patches,
/// borders and anchors, each bordered heading, and the menus' pictures of its pages and fields (#1467).
/// </summary>
public static class FileSelectParts
{
    /// <summary>The JSON name of a foreground page.</summary>
    public static string PageName(FileSelectPage page) =>
        Ensure.Listed(FileSelectPresentationDefinitions.PageNames, page);

    /// <summary>The JSON name of a slot-field patch.</summary>
    public static string PatchName(FileSelectPatch patch) =>
        Ensure.Listed(FileSelectPresentationDefinitions.PatchNames, patch);

    /// <summary>The JSON name of a bordered heading, which keys its anchor.</summary>
    public static string BorderName(FileSelectBorder border) =>
        Ensure.Listed(FileSelectPresentationDefinitions.BorderNames, border);

    /// <summary>The JSON name of a bordered heading's sprite.</summary>
    public static string BorderSpriteName(FileSelectBorder border) =>
        Ensure.Listed(FileSelectPresentationDefinitions.BorderSpriteNames, border);

    /// <summary>The JSON name of a copy or clear page's slot-letter anchor.</summary>
    public static string AnchorName(FileSelectAnchor anchor) =>
        Ensure.Listed(FileSelectPresentationDefinitions.AnchorNames, anchor);


    /// <summary>The menus' picture of the file-select backdrop page (#1467).</summary>
    public const string BackgroundFrame = "FileSelect.Background";

    /// <summary>The menus' picture of <paramref name="page"/>, without its slots' fields (#1467).</summary>
    public static string PageFrame(FileSelectPage page) => $"FileSelect.Page.{PageName(page)}";

    /// <summary>The menus' picture of decimal digit <paramref name="digit"/> (#1467).</summary>
    public static string DigitFrame(int digit) =>
        $"FileSelect.Digit.{Ensure.BetweenInclusive(0, FileSelectPresentationDefinitions.DigitCount - 1, digit)}";

    /// <summary>The menus' picture of save slot <paramref name="slot"/>'s letter (#1467).</summary>
    public static string LetterFrame(int slot) =>
        $"FileSelect.Letter.{Ensure.BetweenInclusive(0, FileSelectPresentationDefinitions.SlotLetterCount - 1, slot)}";

    /// <summary>The menus' picture of a slot field's <paramref name="patch"/>, placed at the patch's anchor (#1467).</summary>
    public static string PatchFrame(FileSelectPatch patch) => $"FileSelect.Patch.{PatchName(patch)}";

    /// <summary>
    /// The menus' picture of an energy tank, filled or empty, in the colours of the health tens digit
    /// <paramref name="tensDigit"/> the field shows beside it (#1467).
    /// </summary>
    public static string TankFrame(bool filled, int tensDigit) =>
        $"FileSelect.Tank.{(filled ? "Filled" : "Empty")}.{Ensure.BetweenInclusive(0, FileSelectPresentationDefinitions.DigitCount - 1, tensDigit)}";

    /// <summary>The JSON name of a looping missile-cursor frame's sprite, for frames 0-3.</summary>
    public static string CursorSpriteName(int frame) =>
        Ensure.Element(CursorSpriteNames, frame);

    /// <summary>The JSON name of a selected-slot helmet frame's sprite, for frames 0-7.</summary>
    public static string HelmetSpriteName(int frame) =>
        Ensure.Element(HelmetSpriteNames, frame);

    /// <summary>Every sprite the document names: the three borders, the four cursor frames and the eight helmet frames.</summary>
    public static IEnumerable<string> SpriteNames =>
        [.. Borders.Select(FileSelectParts.BorderSpriteName), .. CursorSpriteNames, .. HelmetSpriteNames];

    /// <summary>Every frame of the file-select sheet: each sprite the document names, then each file-copy arrow.</summary>
    public static IEnumerable<string> SheetFrames =>
        SpriteNames.Concat(Enumerable.Range(0, FileCopyArrowDefinitions.ShapeCount).Select(FileCopyArrowDefinitions.FrameName));
}
