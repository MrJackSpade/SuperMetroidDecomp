using SuperMetroid.Core.Samus.Abilities.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Backgrounds;
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
    /// <summary>Supplies the collected items, which uncollected-item overlays leave out.</summary>
    private readonly SharedGameState _system;

    /// <summary>Binds the gameplay owners the reveal is built from and the installation's reveal pictures.</summary>
    internal XrayReveal(BackgroundScrollState backgroundScroll, SharedGameState system,
        InstalledXrayReveals visuals)
    {
        _backgroundScroll = backgroundScroll;
        _system = system;
        XrayRevealVisuals = Ensure.NotNull(visuals);
    }

    /// <summary>The installed reveal pictures (#2290), which a host binding another installation's artwork replaces.</summary>
    private InstalledXrayReveals XrayRevealVisuals { get; set; }

    /// <summary>Replaces the reveal pictures with <paramref name="visuals"/>, as a host binding another installation's artwork does.</summary>
    internal void Rebind(InstalledXrayReveals visuals) => XrayRevealVisuals = Ensure.NotNull(visuals);

    /// <summary>
    /// The blocks the X-ray reveals now: the foreground's blocks the room shows from level data (#1351, #1431), the
    /// terrain reveals, then the item and special-room overlays, placed over the room's own blocks. Building it reads the
    /// room and changes nothing.
    /// </summary>
    internal XrayRevealedBlocks Reveal(RoomScope room)
    {
        Ensure.NotNull(room);
        InstalledXrayReveals visuals = XrayRevealVisuals;
        Position16<RoomPixels> foregroundPosition = _backgroundScroll.ForegroundPosition;
        XrayRevealGrid blocks = XrayRevealMap.Build(room.LevelData, foregroundPosition, room.Room.Area, visuals);
        XrayRevealOverlays.Apply(blocks, room.RoomObjects.CollectibleObjects.Collectibles, _system,
            room.Room.State.XrayOverlay, foregroundPosition, visuals);
        return new(XrayRevealMap.OriginOf(foregroundPosition), blocks);
    }
}
