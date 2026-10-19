using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Hud;

/// <summary>
/// Gameplay HUD initialization and the gameplay HUD character-base selection.
/// </summary>
internal sealed class GameplayHudSetup
{
    /// <summary>Selects installed HUD and trail artwork when available.</summary>
    private readonly RuntimeArtwork _artwork;
    /// <summary>Receives the gameplay BG3 character-base selection.</summary>
    private readonly GameplayDisplayState _display;
    /// <summary>The room whose loaded enemy graphics a HUD initialization requeues.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Owns the live HUD tilemap and its palette or map presentation binding.</summary>
    private readonly HudState _hud;
    /// <summary>Supplies Samus state for HUD item and resource display.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Holds room-FX BG3 padding immediately before queued HUD uploads.</summary>
    private readonly SnesVram _vram;
    /// <summary>Preserves native DMA ordering for HUD, OBJ, trail, and enemy graphics.</summary>
    private readonly VramWriteQueue _vramWrites;

    /// <summary>Binds the runtime owners needed for initial BG3, OBJ, and HUD transfers.</summary>
    internal GameplayHudSetup(GameplayHudSetupDependencies dependencies)
    {
        _artwork = dependencies.Artwork;
        _display = dependencies.Display;
        _room = dependencies.Room;
        _hud = dependencies.Hud;
        _player = dependencies.Player;
        _vram = dependencies.Vram;
        _vramWrites = dependencies.VramWrites;
    }

    /// <summary>Selects the ordinary BG3 character base (BG34NBA=$04), as gameplay PPU initialization does.</summary>
    [PpuLayoutBoundary("rendering view: the BG34NBA BG3 character-base register shadow")]
    internal void SelectGameplayHudCharacterBase() => _display.GameplayHudCharacterBaseWord = GameplayVramLayout.HudCharacterWord;

    /// <summary>
    /// $82:8318 follows the BG3 transfer with $2E00 bytes of standard sprite tiles at VRAM $6000: the installed sheet
    /// when one is bound, otherwise the cartridge source. The dynamic Samus DMA refreshes its four reserved regions each
    /// NMI; fixed projectile tiles such as bomb $14C-$14F remain in the untouched portion.
    /// </summary>
    internal void QueueStandardObjectTiles()
    {
        if (_artwork.StandardObjectArt is null)
            _vramWrites.Enqueue(sizeInBytes: StandardObjectArtworkFormat.TransferByteCount,
                sourceAddress: StandardObjectArtworkAddresses.Source,
                destination: GameplayVram.ObjectCharactersDma());
        else
            _vramWrites.EnqueueAsset(VramAssetId.StandardObjectTiles, StandardObjectArtworkFormat.TransferByteCount,
                GameplayVram.ObjectCharactersDma());
    }

    /// <summary>
    /// Loads standard 2-bpp BG3 graphics and initializes the HUD tilemap using the exact
    /// ROM/WRAM/VRAM destinations from <c>$82:82E2</c> and <c>$80:9A79</c>.
    /// </summary>
    public void InitializeHud(HudSnapshot snapshot)
    {
        _hud.BindPresentation(_artwork.MapPresentation?.GameplayHud, _player.Samus);
        // Load_StandardBG3Tiles_SpriteTiles_ClearTilemaps at `$82:82E2` directly DMAs a
        // prefilled WRAM page to VRAM $5800. Rows zero through three are replaced by the
        // live HUD below; rows four through 31 must remain character $6F padding. A fresh
        // host VRAM previously left those words at zero, selecting the visible orange `1`
        // glyph whenever liquid HDMA exposed the above-surface portion of the BG3 page.
        var roomFxPadding = new ushort[RoomFxRomData.Layer3.PaddingWordCount];
        Array.Fill(roomFxPadding, RoomFxRomData.Layer3.PaddingTilemapWord);
        new RoomFxVram(_vram).Layer3Area.Write(roomFxPadding);

        // The original copies $2000 bytes even though the named standard BG3 graphics are
        // $1000 bytes; the following $1000-byte clear table intentionally fills the rest of
        // VRAM $4000-$4FFF with zeroes.
        if (_artwork.MapPresentation is null)
            _vramWrites.Enqueue(HudTileAtlasFormat.TransferByteCount, HudTileAtlasFormat.SourceAddress, GameplayVram.HudCharacterUploadDma);
        else
            _vramWrites.EnqueueAsset(VramAssetId.StandardHudTiles, HudTileAtlasFormat.TransferByteCount, GameplayVram.HudCharacterUploadDma);

        QueueStandardObjectTiles();

        // LoadEnemyTileData's transfers follow the overlapping standard OBJ upload. The
        // room enemy loader has already populated the modeled VRAM for immediate watches;
        // queueing the source records here makes enemy art win again at the accepted NMI.
        // Before the gameplay runtime's first room there is no enemy population to upload.
        if (_room.Scope?.Enemies is { IsLoaded: true } enemies)
            enemies.PopulationLoader.QueueGraphicsUploads(_vramWrites);

        // The immutable first row bypasses WRAM and is DMAed straight from $80:988B.
        if (_artwork.MapPresentation is null)
            _vramWrites.Enqueue(sizeInBytes: GameplayHudDefinitions.TopRowByteCount,
                sourceAddress: GameplayHudDefinitions.TopRowAddress,
                destination: GameplayVram.HudTilemapDma());
        else
            _vramWrites.EnqueueAsset(VramAssetId.HudTopRowTilemap, GameplayHudDefinitions.TopRowByteCount,
                GameplayVram.HudTilemapDma());

        _hud.Initialize(snapshot);
        _hud.QueueUpload(_vramWrites);
    }
}
