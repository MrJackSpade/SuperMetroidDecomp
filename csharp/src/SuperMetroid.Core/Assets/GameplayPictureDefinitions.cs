namespace SuperMetroid.Core.Assets;

/// <summary>What the groups of gameplay pictures are (<see cref="GameplayPictures"/>).</summary>
internal static class GameplayPictureDefinitions
{
    /// <summary>The groups a fade that spares the HUD spares: the HUD and the escape timer.</summary>
    internal static readonly GameplayPictures[] SparedByHudSparingFade = [GameplayPictures.Hud, GameplayPictures.EscapeTimer];
}
