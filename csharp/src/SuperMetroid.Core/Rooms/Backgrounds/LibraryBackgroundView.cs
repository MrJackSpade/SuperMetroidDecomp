namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// What the player sees on the background of a room state that loads a library background (#1649): the background's own
/// picture, or something its picture would not show. Only a background the player sees as its own picture is installed
/// as one; every other room draws its background from what the view names.
/// </summary>
public enum LibraryBackgroundView
{
    /// <summary>The background's own picture, which the room draws from its installed image.</summary>
    Picture,

    /// <summary>
    /// The boss's body: the background shows the body the room's boss draws there, which its body sheet pictures
    /// (#1466). The background starts empty and shows nothing until the boss draws its body.
    /// </summary>
    BossBody,

    /// <summary>The scrolling sky's rows, which the sky draws from its own image (#1392, #1403).</summary>
    SkyRows,

    /// <summary>Nothing: the background is cleared to an empty picture, or the room does not show it at all.</summary>
    Nothing,
}
