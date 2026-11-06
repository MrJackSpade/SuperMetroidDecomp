namespace SuperMetroid.Core.Assets;

/// <summary>
/// The groups of gameplay's background pictures that each show one shading at a time (#1512): a colour effect of the
/// room recolours the room's pictures, the HUD's or the boss body's, never another group's.
/// </summary>
public enum GameplayPictures
{
    /// <summary>
    /// The room the frame shows: its tileset's block pictures on layers 1 and 2, its library background or sky, the
    /// pictures written over its layers, its X-ray reveal and its turning floor.
    /// </summary>
    Room,

    /// <summary>The room a door leaves, shown beside the room it enters while the door scrolls.</summary>
    DoorSourceRoom,

    /// <summary>The HUD's pictures.</summary>
    Hud,

    /// <summary>The room effect's pages on the effect overlay: its liquid, fog or haze.</summary>
    FxOverlay,

    /// <summary>The message box's pictures.</summary>
    MessageBox,

    /// <summary>The boss body drawn on the background: Kraid's, Phantoon's, Crocomire's, Draygon's, Mother Brain's or Ridley's.</summary>
    BossBody,

    /// <summary>
    /// The escape timer's sprite pictures, which a door's fade keeps partly lit as it keeps the HUD's: a display fade that
    /// spares the HUD spares them too, and the door shades them instead.
    /// </summary>
    EscapeTimer,
}
