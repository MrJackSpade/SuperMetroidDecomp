using SuperMetroid.Core.Saves;
using static SuperMetroid.Core.Frontend.FileSelect.Artwork.FileSelectPresentationDefinitions;

namespace SuperMetroid.Core.Frontend.FileSelect.Artwork;

/// <summary>
/// Names the file-select document's parts (<see cref="FileSelectPresentationDefinitions"/>): its pages, patches,
/// each bordered heading's sprite, and the menus' pictures of its pages and fields (#1467).
/// </summary>
public static class FileSelectParts
{
    /// <summary>The JSON name of a foreground page.</summary>
    public static string PageName(FileSelectPage page) =>
        Ensure.Listed(FileSelectPresentationDefinitions.PageNames, page);

    /// <summary>The JSON name of a slot-field patch.</summary>
    public static string PatchName(FileSelectPatch patch) =>
        Ensure.Listed(FileSelectPresentationDefinitions.PatchNames, patch);

    /// <summary>The JSON name of a bordered heading's sprite.</summary>
    public static string BorderSpriteName(FileSelectBorder border) =>
        Ensure.Listed(FileSelectPresentationDefinitions.BorderSpriteNames, border);

    /// <summary>The menus' picture of the file-select backdrop page (#1467).</summary>
    public const string BackgroundFrame = "FileSelect.Background";

    /// <summary>The menus' picture of <paramref name="page"/>, without its slots' fields (#1467).</summary>
    public static string PageFrame(FileSelectPage page) => $"FileSelect.Page.{PageName(page)}";

    /// <summary>The menus' picture of decimal digit <paramref name="digit"/> (#1467).</summary>
    public static string DigitFrame(int digit) =>
        $"FileSelect.Digit.{Ensure.BetweenInclusive(0, FileSelectPresentationDefinitions.DigitCount - 1, digit)}";

    /// <summary>The menus' picture of save slot <paramref name="slot"/>'s letter (#1467, #1976).</summary>
    public static string LetterFrame(SaveSlot slot) =>
        $"FileSelect.Letter.{Ensure.Listed(FileSelectPresentationDefinitions.SlotLetterNames, slot)}";

    /// <summary>The menus' picture of a slot field's <paramref name="patch"/>, placed at the patch's anchor (#1467).</summary>
    public static string PatchFrame(FileSelectPatch patch) => $"FileSelect.Patch.{PatchName(patch)}";

    /// <summary>
    /// The menus' picture of an energy tank, filled or empty, in the colours of the health tens digit
    /// <paramref name="tensDigit"/> the field shows beside it (#1467).
    /// </summary>
    public static string TankFrame(bool filled, int tensDigit) =>
        $"FileSelect.Tank.{(filled ? "Filled" : "Empty")}.{Ensure.BetweenInclusive(0, FileSelectPresentationDefinitions.DigitCount - 1, tensDigit)}";

    /// <summary>The name of the selected slot's helmet picture at step <paramref name="frame"/> of its animation.</summary>
    public static string HelmetSpriteName(int frame) =>
        Ensure.Element(HelmetSpriteNames, frame);
}
