namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// What each actor of the Zebes reveal shows (#1768): its definition and installed placement key. The Japanese subtitle
/// is not spawned with the others: the title spawns it when Japanese text is selected.
/// </summary>
public enum ZebesRevealActorRole
{
    /// <summary>Planet Zebes.</summary>
    Planet,

    /// <summary>The upper-left quarter of the star field around the planet.</summary>
    StarsUpperLeft,

    /// <summary>The upper-right quarter of the star field around the planet.</summary>
    StarsUpperRight,

    /// <summary>The lower-left quarter of the star field around the planet.</summary>
    StarsLowerLeft,

    /// <summary>The lower-right quarter of the star field around the planet, whose leaving the screen ends the scene.</summary>
    StarsLowerRight,

    /// <summary>The PLANET ZEBES caption.</summary>
    Title,

    /// <summary>The Japanese subtitle under the caption.</summary>
    JapaneseSubtitle,
}
