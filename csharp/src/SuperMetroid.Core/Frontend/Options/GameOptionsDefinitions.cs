namespace SuperMetroid.Core.Frontend.Options;

/// <summary>
/// The options screen's fixed timing: the controller page's scrolling and the menu's sound priority. The pages' rows are
/// <see cref="GameOptionsPageRows"/>'s; the cursor's and headings' anchors are installed (#1615).
/// </summary>
public static partial class GameOptionsDefinitions
{
    /// <summary>
    /// Updates the menu holds the black screen the previous menu leaves before fading in; the selection missile animates
    /// from the second on.
    /// </summary>
    public const int BlackHoldUpdates = 2;
    /// <summary>How far, in pixels, the controller page scrolls down to show its Exit and Reset rows.</summary>
    public const int ControllerScrollLimit = 32;
    /// <summary>Pixels the controller page scrolls per update.</summary>
    public const int ControllerScrollPixelsPerFrame = 2;
    /// <summary>
    /// How crowded the sound queue may be for the options menu's cursor and confirmation sounds to join it: the usual
    /// priority.
    /// </summary>
    public static readonly SoundRequestPriority MenuSoundPriority = SoundRequestPriority.Normal;
}
