namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>Which character page a room's BG1 and BG2 draw from (#1357).</summary>
public enum RoomBackgroundCharacters
{
    /// <summary>The room tileset's characters at the start of VRAM.</summary>
    Tileset,

    /// <summary>
    /// The object character page: the Ceres Ridley arena's setup (<c>$8F:C97B</c>) points BG1 and BG2 there, because
    /// its Mode 7 tileset fills the ordinary character area.
    /// </summary>
    ObjectCharacterPage,
}
