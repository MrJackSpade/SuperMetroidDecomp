using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.World;

/// <summary>
/// The shades the import pre-colours the HUD's pictures in for a door's fades (#1512), one per step taken. A door keeps
/// the minimap's and the highlighted item's colours lit as the room fades, and between two common rooms the HUD's item
/// and minimap backgrounds too; which colours those are is the import's.
/// </summary>
public static class DoorFadeShades
{
    /// <summary>The HUD after <paramref name="steps"/> steps of a door's fade out of a room.</summary>
    public static PictureShade HudFadingOut(int steps) => new("door-hud-fading-out", steps);

    /// <summary>The HUD after <paramref name="steps"/> steps of a door's fade out of one common room into another.</summary>
    public static PictureShade HudFadingOutBetweenCommonRooms(int steps) => new("door-hud-fading-out-common-rooms", steps);

    /// <summary>The HUD after <paramref name="steps"/> steps of a door's fade into a room.</summary>
    public static PictureShade HudFadingIn(int steps) => new("door-hud-fading-in", steps);

    /// <summary>The HUD after <paramref name="steps"/> steps of a door's fade into one common room from another.</summary>
    public static PictureShade HudFadingInBetweenCommonRooms(int steps) => new("door-hud-fading-in-common-rooms", steps);
}
