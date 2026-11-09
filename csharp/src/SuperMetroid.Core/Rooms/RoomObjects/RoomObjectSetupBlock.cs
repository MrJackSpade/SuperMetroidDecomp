namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A block a room-object setup or a block reaction writes of its own accord rather than drawing a frame (#1580): what it
/// shows is an installed picture (<see cref="InstalledRoomObjectSetupBlocks"/>), and how it collides stays the game's.
/// </summary>
public enum RoomObjectSetupBlock
{
    /// <summary>
    /// The bomb block's parent, which a bomb block shows from the moment it reacts: the live block becomes it as air, and
    /// a respawning one-block bomb block grows back as it.
    /// </summary>
    BombBlockParent,

    /// <summary>
    /// The crumble block's parent, which a crumble block Samus stands on shows while it waits to crumble and grows back
    /// as. It keeps the struck block's own mirroring.
    /// </summary>
    CrumbleBlockParent,

    /// <summary>
    /// The Speed Booster block's parent, which a Speed Booster block shows as it starts to crumble and grows back as. It
    /// keeps the struck block's own mirroring.
    /// </summary>
    SpeedBoosterBlockParent,

    /// <summary>The shot block's parent, which a respawning one-block shot block grows back as.</summary>
    ShotBlockParent,

    /// <summary>The Power Bomb block's parent, which a respawning Power Bomb block grows back as.</summary>
    PowerBombBlockParent,

    /// <summary>The Super Missile block's parent, which a respawning Super Missile block grows back as.</summary>
    SuperMissileBlockParent,

    /// <summary>The block the escape animals' rescue wall shows the moment a shot strikes it, before it breaks open.</summary>
    CritterWallStruck,

    /// <summary>The block the Wrecked Ship entrance treadmill's row shows, as air, while Phantoon lives.</summary>
    TreadmillIdle,

    /// <summary>The block the Wrecked Ship entrance treadmill's row shows, as a running belt, once Phantoon is dead.</summary>
    TreadmillRunning,
}
