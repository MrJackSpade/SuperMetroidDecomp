namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Artwork a room's enemy graphics set loads from an enemy header that names no one enemy of the game (#1686): one whose
/// enemy no room places and no code spawns, or one the game's several actors share, none of which owns the record's
/// artwork (#1989). The record's tiles and colours land among the characters and palette rows the set's other members,
/// actors and spawned objects draw from, so the import uploads them where the cartridge does and composes the pictures
/// over them.
/// </summary>
internal enum UnplacedEnemyArtwork
{
    /// <summary>
    /// The mini-Crocomire's tiles and colours, which both Final Missile Bombway sets load into the palette row the face
    /// blocks and the room's explosions and drops draw in, so those show its colours.
    /// </summary>
    Stoke,

    /// <summary>
    /// The large Sidehopper's tiles, which Big Boy's room's set loads after the dead Sidehopper's own, and from which the
    /// dead Sidehopper's living frames draw.
    /// </summary>
    DeadSidehopperLivingTiles,

    /// <summary>
    /// The Tourian statues' ghost tiles and the Ridley statue's colours, which the statue room's set loads at a fixed
    /// block, and from which the statues' eye glows, ghosts and rising bosses draw.
    /// </summary>
    TourianStatueGhosts,

    /// <summary>
    /// The Ceres station's door tiles and colours, which every Ceres set loads at a fixed block (#1989). The station's
    /// doors and walls are each their own actor, all placed by this one header; each draws from the characters the Ceres
    /// tileset loads, in the row its initialization selects, so none draws from the record.
    /// </summary>
    CeresDoors,
}
