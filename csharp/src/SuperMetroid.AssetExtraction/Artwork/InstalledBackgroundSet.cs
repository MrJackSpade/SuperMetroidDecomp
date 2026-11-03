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
[JsonConverter(typeof(JsonStringEnumConverter<InstalledBackgroundSet>))]
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

    /// <summary>The tileset's first tile colours, which the Ceres doors and Maridia's sand animate.</summary>
    TilesFirst,

    /// <summary>The tileset's second tile colours, which Mother Brain's room lights and Tourian's background animate.</summary>
    TilesSecond,

    /// <summary>The tileset's third tile colours, which Mother Brain's body and the Wrecked Ship's lights show.</summary>
    TilesThird,

    /// <summary>The tileset's fourth tile colours, which Draygon's room background and Ceres's floor show.</summary>
    TilesFourth,

    /// <summary>The tileset's fifth tile colours, which Kraid's room backdrop and the Tourian statues show.</summary>
    TilesFifth,

    /// <summary>
    /// The tileset's sixth tile colours, which the background-drawn boss bodies (Kraid, Phantoon, Crocomire, Ridley) and
    /// the escape typewriter's text source show.
    /// </summary>
    TilesSixth,
}
