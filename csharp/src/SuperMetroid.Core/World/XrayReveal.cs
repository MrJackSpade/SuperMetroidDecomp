using SuperMetroid.Core.Samus.Abilities.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Core.World;

/// <summary>
/// What the X-ray reveals (#1431, #1514): the revealed blocks over the screen, built from the room's level data, its
/// collectibles and the camera as they are when a frame shows them. Nothing is kept between frames: the reveal shows the
/// room as it is.
/// </summary>
internal sealed class XrayReveal
{
    /// <summary>Supplies the camera position, which places the reveal.</summary>
    private readonly BackgroundScrollState _backgroundScroll;
    /// <summary>Supplies the active room, its level data and its area.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Supplies the collected items, which uncollected-item overlays leave out.</summary>
    private readonly SharedGameState _system;

    /// <summary>Binds the gameplay owners the reveal is built from.</summary>
    internal XrayReveal(BackgroundScrollState backgroundScroll, ActiveRoomState room, SharedGameState system)
    {
        _backgroundScroll = backgroundScroll;
        _room = room;
        _system = system;
    }
    /// <summary>Installed X-ray art, rebound when a saved game state is restored.</summary>
    public InstalledXrayReveals? XrayRevealVisuals { get; set; }

    /// <summary>
    /// The blocks the X-ray reveals now: the foreground's blocks the room shows from level data (#1351, #1431), the
    /// terrain reveals, then the item and special-room overlays, placed over the room's own blocks. Building it reads the
    /// room and changes nothing.
    /// </summary>
    /// <exception cref="InvalidOperationException">No room or level data is loaded.</exception>
    internal XrayRevealedBlocks Reveal(IRoomObjects roomObjects)
    {
        Ensure.NotNull(roomObjects);
        var room = _room.ActiveRoom ?? throw new InvalidOperationException("The X-ray reveals no room.");
        var level = _room.LevelData ?? throw new InvalidOperationException("The X-ray reveals a room with no level data.");
        Position16<RoomPixels> foregroundPosition = _backgroundScroll.ForegroundPosition;
        RevealedBlock?[] blocks = XrayRevealMap.Build(level, foregroundPosition, room.AreaIndex, XrayRevealVisuals);
        XrayRevealOverlays.Apply(level, blocks, roomObjects.CollectibleObjects.Collectibles, _system,
            room.State.XrayOverlay, foregroundPosition, XrayRevealVisuals);
        return new(XrayRevealMap.OriginOf(foregroundPosition), [.. blocks]);
    }
}
