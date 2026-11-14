namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Artwork a room's enemy graphics set loads from an enemy header whose enemy no room places and no code spawns (#1686).
/// The game has no such enemy, but the record's tiles and colours land among the characters and palette rows the set's
/// other members and spawned objects draw from, so the import uploads them where the cartridge does and composes the
/// pictures over them.
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
}
