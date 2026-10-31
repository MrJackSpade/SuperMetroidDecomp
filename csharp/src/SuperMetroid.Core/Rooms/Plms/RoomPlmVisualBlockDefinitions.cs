using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Block appearances PLM setups and block reactions write themselves rather than read from the room: unmirrored blocks
/// of the room's tileset.
/// </summary>
internal static class RoomPlmVisualBlockDefinitions
{
    /// <summary>
    /// Bomb-block parent $058, shown on successful Samus contact. The live block immediately becomes this air block;
    /// the restore block keeps the original collision kind with the same replacement appearance.
    /// </summary>
    public static readonly RoomBlockAppearance CollisionBombParent = RoomBlockAppearance.Of(88);

    /// <summary>Speed-block parent $0B6, shown by the Speed Booster block setup before its animation replaces it.</summary>
    public static readonly RoomBlockAppearance SpeedBoosterParent = RoomBlockAppearance.Of(182);

    /// <summary>Contact-crumble parent $0BC, shown by the crumble block setup before its animation replaces it.</summary>
    public static readonly RoomBlockAppearance ContactCrumbleParent = RoomBlockAppearance.Of(188);

    /// <summary>Block $052: the shot block a beam break restores after its respawn delay.</summary>
    public static readonly RoomBlockAppearance ShotBlockParent = RoomBlockAppearance.Of(82);
    /// <summary>Block $057: the power-bomb block a power bomb restores after its respawn delay.</summary>
    public static readonly RoomBlockAppearance PowerBombBlockParent = RoomBlockAppearance.Of(87);
    /// <summary>Block $09F: the Super Missile block a Super Missile restores after its respawn delay.</summary>
    public static readonly RoomBlockAppearance SuperMissileBlockParent = RoomBlockAppearance.Of(159);
}
