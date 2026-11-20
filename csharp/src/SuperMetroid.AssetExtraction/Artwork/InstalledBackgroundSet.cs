using System.Text.Json.Serialization;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// One of gameplay's background colour sets, named by what it holds (#1464). A room load fills every set from its
/// tileset's colours; the HUD, the minimap and the message box show the two HUD sets, and the room's tiles, its
/// background and the bosses drawn on the background show the six tile sets. Which colour group of an installed image
/// was installed from which set is decided by the import pipeline when it installs the image and recorded in the sheet's
/// manifest (#1512); only the import pipeline knows where each set lies among the cartridge's colours, and Core's
/// installed images never name a set.
/// </summary>
[JsonConverter(typeof(InstalledBackgroundSetJsonConverter))]
public enum InstalledBackgroundSet
{
    /// <summary>
    /// The HUD's colours: its counters and icons and the minimap's explored and unexplored rooms. Its first colour is the
    /// backdrop, shown where nothing is drawn.
    /// </summary>
    Hud,

    /// <summary>
    /// The HUD's item and effect colours: the selected and highlighted HUD items, the minimap's background and room
    /// highlight, the message box's text, and the colours a room effect blends into layer 3.
    /// </summary>
    HudItemsAndEffects,

    /// <summary>The tile colours the Ceres doors and Maridia's sand animate.</summary>
    DoorAndSandTiles,

    /// <summary>The tile colours Mother Brain's room lights and Tourian's background animate.</summary>
    AnimatedLightTiles,

    /// <summary>The tile colours Mother Brain's body and the Wrecked Ship's lights show.</summary>
    MachineryTiles,

    /// <summary>The tile colours Draygon's room background and Ceres's floor show.</summary>
    BackgroundAndFloorTiles,

    /// <summary>The tile colours Kraid's room backdrop and the Tourian statues show.</summary>
    BackdropAndStatueTiles,

    /// <summary>
    /// The tile colours the background-drawn boss bodies (Kraid, Phantoon, Crocomire, Ridley) and the escape typewriter's
    /// text source show.
    /// </summary>
    BackgroundBossTiles,
}
