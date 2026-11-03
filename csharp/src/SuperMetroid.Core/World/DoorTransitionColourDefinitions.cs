namespace SuperMetroid.Core.World;

/// <summary>The timing of a door's fades (#1405, #1512), as <c>GameState_A_LoadingNextRoom</c> runs them.</summary>
public static class DoorTransitionColourDefinitions
{
    /// <summary>The length of a door's fades: the source room fades out, and the room it enters fades in, over this many steps.</summary>
    public const int FadeDenominator = 12;

    /// <summary>
    /// The shared counter values a door's fade can show: zero through the fade's length plus one, the last of which shows
    /// the colours it fades to exactly. The HUD's door fade shades are indexed by the value.
    /// </summary>
    public const int HudFadeShades = FadeDenominator + 2;
}
