namespace SuperMetroid.Core.Rendering;

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

    /// <summary>The room effect's pages on layer 3: its liquid, fog or haze.</summary>
    Layer3,

    /// <summary>The message box's pictures.</summary>
    MessageBox,

    /// <summary>The boss body drawn on layer 2: Kraid's, Phantoon's, Crocomire's, Draygon's, Mother Brain's or Ridley's.</summary>
    BossBody,
}
