namespace SuperMetroid.Core.Assets;

/// <summary>What the groups of gameplay pictures are (<see cref="GameplayPictures"/>).</summary>
internal static partial class GameplayPictureDefinitions
{
    /// <summary>
    /// The groups of background pictures a door's fade shades instead of the display's fade (#3036): every group the door
    /// may show drawn in the background's colours, whose fade keeps some of those colours lit wherever they show.
    /// </summary>
    internal static readonly GameplayPictures[] Backgrounds =
    [
        GameplayPictures.Room, GameplayPictures.WrittenOverRoom, GameplayPictures.DoorSourceRoom, GameplayPictures.Hud,
        GameplayPictures.FxOverlay, GameplayPictures.BossBody,
    ];

    /// <summary>The groups a fade that spares the backgrounds spares: the background pictures and the escape timer.</summary>
    internal static readonly GameplayPictures[] SparedByBackgroundSparingFade = [.. Backgrounds, GameplayPictures.EscapeTimer];
}
