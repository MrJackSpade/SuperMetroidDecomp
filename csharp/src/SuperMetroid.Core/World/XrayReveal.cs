using SuperMetroid.Core.Samus.Abilities.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Core.World;

/// <summary>
/// X-ray reveal presentation: the revealed blocks, built once from the room's level data and BG1 plane.
/// </summary>
internal sealed class XrayReveal
{
    /// <summary>Supplies layer-one scroll and BG1 offsets for reveal placement.</summary>
    private readonly BackgroundScrollState _backgroundScroll;
    /// <summary>Supplies the active room, level, area, and X-ray pointer.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Supplies system flags that affect revealed overlays.</summary>
    private readonly SharedGameState _system;
    /// <summary>
    /// The frozen reveal map, owned here (#1406). The cartridge built it in the tilemap staging its library backgrounds
    /// decompress into; nothing reads one through the other, so the map is its own state.
    /// </summary>
    private readonly RevealedBlock?[] _revealedBlocks = new RevealedBlock?[XrayRevealLayout.Cells];

    /// <summary>The frozen reveal map's shown blocks (#1431); it is never rebuilt from later room changes.</summary>
    internal ReadOnlySpan<RevealedBlock?> RevealedBlocks => _revealedBlocks;

    /// <summary>Binds the gameplay owners used to construct the X-ray reveal.</summary>
    internal XrayReveal(BackgroundScrollState backgroundScroll, ActiveRoomState room, SharedGameState system)
    {
        _backgroundScroll = backgroundScroll;
        _room = room;
        _system = system;
    }
    /// <summary>Installed X-ray art, rebound by the host after debugger-state restore.</summary>
    [field: NonSerialized]
    public InstalledXrayReveals? XrayRevealVisuals { get; set; }

    /// <summary>
    /// Setup call four's reveal construction (<c>$91:CB8E</c>), run before the setup stage advances. The reveal map is
    /// owned state, so state saves retain it and rendering never reruns game logic to synthesize a replacement. Its base
    /// copy reads the BG1 plane's blocks the room shows from level data (#1351, #1431), where the cartridge read two BG1
    /// screens back out of VRAM on the two setup calls before; time is frozen across them.
    /// </summary>
    internal void BuildXrayRevealMap(IRoomPlms plms)
    {
        var room = _room.ActiveRoom ?? throw new InvalidOperationException("X-ray setup has no room.");
        var level = _room.LevelData ?? throw new InvalidOperationException("X-ray setup has no level data.");
        Position16<RoomPixels> layer1 = _backgroundScroll.Layer1Position;
        RevealedBlock?[] map = XrayRevealMap.Build(level, layer1, _backgroundScroll.Bg1Offset,
            room.AreaIndex, XrayRevealVisuals);
        XrayRevealOverlays.Apply(level, map, plms.CollectiblePlms.Collectibles, _system,
            room.State.XrayOverlay, layer1, XrayRevealVisuals);
        map.CopyTo(_revealedBlocks.AsSpan());
    }
}
