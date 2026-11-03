using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.World;

/// <summary>
/// The shades the import pre-colours the HUD's pictures in for a door's fades (#1512), one per value of the shared counter
/// a step can take, each showing the colours after the steps from zero through that value. A door keeps
/// the minimap's and the highlighted item's colours lit as the room fades, and between two common rooms the HUD's item
/// and minimap backgrounds too; which colours those are is the import's.
/// </summary>
public static class DoorFadeShades
{
    /// <summary>The HUD after the step of counter value <paramref name="value"/> of a door's fade out of a room.</summary>
    public static PictureShade HudFadingOut(int value) => new("door-hud-fading-out", value);

    /// <summary>The HUD after the step of counter value <paramref name="value"/> of a door's fade out of one common room into another.</summary>
    public static PictureShade HudFadingOutBetweenCommonRooms(int value) => new("door-hud-fading-out-common-rooms", value);

    /// <summary>The HUD after the step of counter value <paramref name="value"/> of a door's fade into a room.</summary>
    public static PictureShade HudFadingIn(int value) => new("door-hud-fading-in", value);

    /// <summary>The HUD after the step of counter value <paramref name="value"/> of a door's fade into one common room from another.</summary>
    public static PictureShade HudFadingInBetweenCommonRooms(int value) => new("door-hud-fading-in-common-rooms", value);

    /// <summary>
    /// The escape timer's colours after a door's fade out to black from the shared counter's zero through
    /// <paramref name="value"/>, its lit colours kept.
    /// </summary>
    public static PictureShade EscapeTimerFadingOut(int value) => new("door-escape-timer-fading-out", value);

    /// <summary>
    /// The escape timer's colours after a door's fade in from black from the shared counter's zero through
    /// <paramref name="value"/>, its lit colours kept.
    /// </summary>
    public static PictureShade EscapeTimerFadingIn(int value) => new("door-escape-timer-fading-in", value);
}
