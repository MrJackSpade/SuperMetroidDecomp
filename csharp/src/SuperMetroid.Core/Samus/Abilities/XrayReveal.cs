using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.Xray;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// X-ray reveal presentation: the revealed tilemap, built once from the room's level data and BG1 plane.
/// </summary>
internal sealed class XrayReveal
{
    /// <summary>Supplies layer-one scroll and BG1 offsets for reveal placement.</summary>
    private readonly BackgroundScrollState _backgroundScroll;
    /// <summary>Supplies the active room, level, area, and X-ray pointer.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Supplies system flags that affect revealed overlays.</summary>
    private readonly Bank80SystemState _system;
    /// <summary>The session's BG tilemap staging; the frozen reveal map is built in its bytes (<c>$7E:4000</c>).</summary>
    private readonly Bg2TilemapStaging _tilemapStaging;
    /// <summary>The frozen reveal map; it is never rebuilt from later room or VRAM changes.</summary>
    internal StagingWordView RevealTilemap => _tilemapStaging.TilemapWords;

    /// <summary>Binds the gameplay owners used to construct the X-ray reveal.</summary>
    internal XrayReveal(BackgroundScrollState backgroundScroll, ActiveRoomState room, Bank80SystemState system, Bg2TilemapStaging tilemapStaging)
    {
        _backgroundScroll = backgroundScroll;
        _room = room;
        _system = system;
        _tilemapStaging = tilemapStaging;
    }
    /// <summary>Installed X-ray art, rebound by the host after debugger-state restore.</summary>
    [field: NonSerialized]
    public XrayRevealVisualCatalog? XrayRevealVisuals { get; set; }

    /// <summary>
    /// Setup call four's reveal construction (<c>$91:CB8E</c>), run before the setup stage advances. The reveal map is
    /// owned state, so state saves retain it and rendering never reruns game logic to synthesize a replacement. Its base
    /// copy reads the BG1 plane the room shows from level data (#1351), where the cartridge read two BG1 screens back out
    /// of VRAM on the two setup calls before; time is frozen across them.
    /// </summary>
    internal void BuildXrayRevealTilemap(RoomPlmSystem plms)
    {
        var room = _room.ActiveRoom ?? throw new InvalidOperationException("X-ray setup has no room.");
        var level = _room.LevelData ?? throw new InvalidOperationException("X-ray setup has no level data.");
        Position16<RoomPixels> layer1 = _backgroundScroll.Layer1Position;
        Position16<RoomPixels> bg1Scroll = layer1 + _backgroundScroll.Bg1Offset;
        var bg1 = new RoomBackgroundLayer(level.ForegroundPlane, _backgroundScroll.Bg1Offset.AsBgPlaneOffset());
        var map = XrayRevealTilemap.Build(level, bg1, bg1Scroll, layer1,
            (byte)room.AreaIndex, XrayRevealVisuals);
        XrayRevealOverlays.Apply(level, map, plms.CollectiblePlms.Collectibles, _system,
            room.State.XrayOverlay, layer1, XrayRevealVisuals);
        StagingWordView reveal = RevealTilemap;
        for (int i = 0; i < map.Length; i++) reveal[i] = map[i];
    }
}
