using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.World;

/// <summary>
/// The shades the import pre-colours the background's pictures and the escape timer's in for a door's fades (#1512, #2289,
/// #3036): each picture with the colours the door fades black and its lit colours kept. A door's fade shows a typed fade
/// between a picture's standard colours and its dark shade, as far as the fade has come. A door keeps the background
/// colours the minimap and the highlighted item show lit as the room fades, and between two common rooms the HUD's item and
/// minimap backgrounds too; the room's own pictures that show those colours, its doors among them, stay lit with them, as
/// the cartridge's fade target holds the colours and not the HUD. Which colours those are is the import's.
/// </summary>
public static partial class DoorFadeShades
{
    /// <summary>A background picture once a door's room has faded out: every colour the door fades black.</summary>
    public static PictureShade BackgroundDark { get; } = new("door-background-dark", 0);

    /// <summary>A background picture once a door between two common rooms has faded its room out: every colour that door fades black.</summary>
    public static PictureShade BackgroundDarkBetweenCommonRooms { get; } = new("door-background-dark-common-rooms", 0);

    /// <summary>The escape timer once a door's room has faded out: every colour but its lit ones black.</summary>
    public static PictureShade EscapeTimerDark { get; } = new("door-escape-timer-dark", 0);
}
