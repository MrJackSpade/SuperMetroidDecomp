using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.Xray;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// X-ray reveal presentation: the revealed BG1 tilemap and its transfer.
/// </summary>
internal sealed class XrayReveal
{
    /// <summary>Supplies layer-one scroll and BG1 offsets for reveal placement.</summary>
    private readonly BackgroundScrollState _backgroundScroll;
    /// <summary>Supplies the active Samus X-ray setup stage during NMI reads.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Supplies the active room, level, area, and X-ray pointer.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Supplies system flags that affect revealed overlays.</summary>
    private readonly Bank80SystemState _system;
    /// <summary>The session's BG tilemap staging; the frozen reveal map is built in its bytes (<c>$7E:4000</c>).</summary>
    private readonly Bg2TilemapStaging _tilemapStaging;
    /// <summary>
    /// The two BG1 tilemap screens captured during setup (cartridge <c>$7E:6000-$6FFF</c>): the first screen is read by
    /// <c>$91:CB57</c>, the second by <c>$91:CB1C</c>.
    /// </summary>
    internal readonly ushort[] SavedBg1 = new ushort[XrayTilemapLayout.BufferWords];

    /// <summary>The frozen reveal map; it is never rebuilt from later room or VRAM changes.</summary>
    internal StagingWordView RevealTilemap => _tilemapStaging.TilemapWords;

    /// <summary>Binds the gameplay owners used to capture BG1 and construct the X-ray reveal.</summary>
    internal XrayReveal(BackgroundScrollState backgroundScroll, SamusFrameState player, ActiveRoomState room, Bank80SystemState system, Bg2TilemapStaging tilemapStaging)
    {
        _backgroundScroll = backgroundScroll;
        _player = player;
        _room = room;
        _system = system;
        _tilemapStaging = tilemapStaging;
    }
    /// <summary>Installed X-ray art, rebound by the host after debugger-state restore.</summary>
    [field: NonSerialized]
    public XrayRevealVisualCatalog? XrayRevealVisuals { get; set; }

    /// <summary>
    /// Setup call four's reveal construction (<c>$91:CB8E</c>), run before the setup stage
    /// advances. The captured pages and the reveal map are owned state, so state saves retain the
    /// exact captured map and rendering never reruns game logic to synthesize a replacement.
    /// </summary>
    internal void BuildXrayRevealTilemap(RoomPlmSystem plms)
    {
        var room = _room.ActiveRoom ?? throw new InvalidOperationException("X-ray setup has no room.");
        var level = _room.LevelData ?? throw new InvalidOperationException("X-ray setup has no level data.");
        // The builder reads only BG1 tilemap words. Feed the two captured pages,
        // not current VRAM: each page was read on a different setup call.
        var captured = new SnesVram();
        new GameplayVram(captured).Bg1Tilemap.Write(SavedBg1);
        Position16<RoomPixels> layer1 = _backgroundScroll.Layer1Position;
        Position16<RoomPixels> bg1Scroll = layer1 + _backgroundScroll.Bg1Offset;
        var map = XrayRevealTilemap.Build(level, captured, bg1Scroll, layer1,
            (byte)room.AreaIndex, XrayRevealVisuals);
        XrayRevealOverlays.Apply(level, map, plms.CollectiblePlms.Collectibles, _system,
            room.State.XrayOverlay, layer1, XrayRevealVisuals);
        StagingWordView reveal = RevealTilemap;
        for (int i = 0; i < map.Length; i++) reveal[i] = map[i];
    }

    /// <summary>
    /// Completes the previous setup call's read of one BG1 tilemap screen after NMI's video writes. The room shows BG1
    /// from level data (#1346), so each tilemap cell reads the level tile at the scrolled position nearest the screen.
    /// </summary>
    internal void TransferXrayBg1Read()
    {
        if (_player.Samus?.Xray is not { IsActive: true } xray) return;
        // SetupStage already names the NEXT main-thread call. Calls two and three enqueue
        // reads; the following NMI performs them before call four consumes the captured
        // pages. In particular, queued writes must win first.
        (int SourceScreen, int SavedScreen)? read = xray.SetupStage switch
        {
            XraySetupStage.ReadBg1FirstScreen => (1, 1),
            XraySetupStage.BuildRevealReadBg2FirstScreen => (0, 0),
            XraySetupStage.Complete or XraySetupStage.FreezeTimeBackupBg2Registers or
                XraySetupStage.ReadBg1SecondScreen or XraySetupStage.ReadBg2SecondScreen or
                XraySetupStage.TransferRevealFirstScreen or XraySetupStage.InitializeTransferRevealSecondScreen or
                XraySetupStage.BackdropColor => null,
            _ => throw new InvalidOperationException($"Undefined X-ray setup stage {xray.SetupStage}."),
        };
        if (read is not (int sourceScreen, int savedScreen)) return;
        ushort[] saved = SavedBg1;
        var room = new RoomBackgroundLayer(_room.LoadedLevelData.ForegroundPlane, _backgroundScroll.Bg1Offset.AsBgPlaneOffset());
        Position16<BgPlanePixels> scroll = _backgroundScroll.Bg1Scroll.AsBgPlaneScroll();
        int firstWord = savedScreen * XrayTilemapLayout.ScreenWords;
        for (int i = 0; i < XrayTilemapLayout.ScreenWords; i++)
        {
            int column = sourceScreen * XrayTilemapLayout.ScreenColumns + i % XrayTilemapLayout.ScreenColumns;
            int row = i / XrayTilemapLayout.ScreenColumns;
            var cell = new Position16<BgPlanePixels>(
                unchecked((ushort)(scroll.X + ((column * SnesPpuLayout.BackgroundTileSizePixels - scroll.X) & (XrayTilemapLayout.TilemapPixelWidth - 1)))),
                unchecked((ushort)(scroll.Y + ((row * SnesPpuLayout.BackgroundTileSizePixels - scroll.Y) & (XrayTilemapLayout.TilemapPixelHeight - 1)))));
            saved[firstWord + i] = room.TrySample(cell, out BgTileSample sample) ? sample.Tile.Raw : (ushort)0;
        }
    }
}
