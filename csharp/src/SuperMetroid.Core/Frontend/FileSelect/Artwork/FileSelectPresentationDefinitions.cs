using SuperMetroid.Core.Saves;

namespace SuperMetroid.Core.Frontend.FileSelect.Artwork;

/// <summary>The file-select menu's page dimensions, glyph counts and the names its pieces are pictured under.</summary>
public static class FileSelectPresentationDefinitions
{
    /// <summary>The width in pixels of each page file select shows.</summary>
    public const int PageWidthPixels = 256;
    /// <summary>The height in pixels of each page file select shows.</summary>
    public const int PageHeightPixels = 256;
    /// <summary>The decimal digits, zero through nine, file select writes its counts with.</summary>
    public const int DigitCount = 10;
    /// <summary>The name each save slot's letter is pictured under.</summary>
    internal static readonly IReadOnlyDictionary<SaveSlot, string> SlotLetterNames =
        new Dictionary<SaveSlot, string>
        {
            [SaveSlot.A] = "A",
            [SaveSlot.B] = "B",
            [SaveSlot.C] = "C",
        };

    /// <summary>
    /// The name each picture of the selected slot's helmet is installed under, in the order it turns to face the player
    /// and shines (#2202): turned away, turning, the shine on its visor from the left edge across the middle to the right
    /// edge, a tall sparkle as the shine leaves, and the wide sparkle it settles on.
    /// </summary>
    internal static readonly string[] HelmetSpriteNames =
    [
        "Helmet.TurnedAway", "Helmet.Turning", "Helmet.ShineLeft", "Helmet.ShineLeftOfMiddle", "Helmet.ShineRightOfMiddle",
        "Helmet.ShineRight", "Helmet.SparkleTall", "Helmet.SparkleWide",
    ];

    /// <summary>The JSON name of a foreground page.</summary>
    internal static readonly IReadOnlyDictionary<FileSelectPage, string> PageNames =
        new Dictionary<FileSelectPage, string>
        {
            [FileSelectPage.MainWithData] = "Main.WithData",
            [FileSelectPage.MainEmpty] = "Main.Empty",
            [FileSelectPage.CopySource] = "Copy.Source",
            [FileSelectPage.CopyDestination] = "Copy.Destination",
            [FileSelectPage.CopyConfirm] = "Copy.Confirm",
            [FileSelectPage.CopyCompleted] = "Copy.Completed",
            [FileSelectPage.ClearSelection] = "Clear.Selection",
            [FileSelectPage.ClearConfirm] = "Clear.Confirm",
            [FileSelectPage.ClearCompleted] = "Clear.Completed",
        };

    /// <summary>The JSON name of a slot-field patch.</summary>
    internal static readonly IReadOnlyDictionary<FileSelectPatch, string> PatchNames =
        new Dictionary<FileSelectPatch, string>
        {
            [FileSelectPatch.Energy] = "Energy",
            [FileSelectPatch.NoData] = "NoData",
            [FileSelectPatch.TimeColon] = "TimeColon",
        };

    /// <summary>The JSON name of a bordered heading's sprite.</summary>
    internal static readonly IReadOnlyDictionary<FileSelectBorder, string> BorderSpriteNames =
        new Dictionary<FileSelectBorder, string>
        {
            [FileSelectBorder.Main] = "Border.Main",
            [FileSelectBorder.Copy] = "Border.Copy",
            [FileSelectBorder.Clear] = "Border.Clear",
        };
}
