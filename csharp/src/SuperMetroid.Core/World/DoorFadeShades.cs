using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.World;

/// <summary>
/// The shades the import pre-colours the HUD's and the escape timer's pictures in for a door's fades (#1512, #2289): each
/// picture with the colours the door fades black and its lit colours kept. A door's fade shows a typed fade between a
/// picture's standard colours and its dark shade, as far as the fade has come. A door keeps the minimap's and the
/// highlighted item's colours lit as the room fades, and between two common rooms the HUD's item and minimap backgrounds
/// too; which colours those are is the import's.
/// </summary>
public static class DoorFadeShades
{
    /// <summary>The HUD once a door's room has faded out: every colour the door fades black.</summary>
    public static PictureShade HudDark { get; } = new("door-hud-dark", 0);

    /// <summary>The HUD once a door between two common rooms has faded its room out: every colour that door fades black.</summary>
    public static PictureShade HudDarkBetweenCommonRooms { get; } = new("door-hud-dark-common-rooms", 0);

    /// <summary>The escape timer once a door's room has faded out: every colour but its lit ones black.</summary>
    public static PictureShade EscapeTimerDark { get; } = new("door-escape-timer-dark", 0);
}
