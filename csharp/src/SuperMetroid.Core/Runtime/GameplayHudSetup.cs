using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// Gameplay HUD initialization and the gameplay HUD character-base selection.
/// </summary>
internal sealed class GameplayHudSetup
{
    private readonly RuntimeArtwork _artwork;
    private readonly GameplayDisplayState _display;
    private readonly RoomEnemySystem _enemies;
    private readonly HudState _hud;
    private readonly SamusFrameState _player;
    private readonly SnesVram _vram;
    private readonly VramWriteQueue _vramWrites;
    private readonly ISnesAddressSpace _addressSpace;

    internal GameplayHudSetup(GameplayHudSetupDependencies dependencies)
    {
        _artwork = dependencies.Artwork;
        _display = dependencies.Display;
        _enemies = dependencies.Enemies;
        _hud = dependencies.Hud;
        _player = dependencies.Player;
        _vram = dependencies.Vram;
        _vramWrites = dependencies.VramWrites;
        _addressSpace = dependencies.AddressSpace;
    }

    /// <summary>Selects the ordinary BG3 character base (BG34NBA=$04), as gameplay PPU initialization does.</summary>
    [PpuLayoutBoundary("rendering view: the BG34NBA BG3 character-base register shadow")]
    internal void SelectGameplayHudCharacterBase() => _display.GameplayHudCharacterBaseWord = GameplayVramLayout.HudCharacterWord;

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
            _vramWrites.Enqueue(HudTileAtlasFormat.TransferByteCount, HudTileAtlasFormat.SourceAddress, Assets.AssetVram.HudCharacterUploadDma);
        else
            _vramWrites.EnqueueAsset(VramAssetId.StandardHudTiles, HudTileAtlasFormat.TransferByteCount, Assets.AssetVram.HudCharacterUploadDma);

        // $82:8318 follows the BG3 transfer with $2E00 bytes of standard sprite tiles at
        // VRAM $6000. The dynamic Samus DMA refreshes its four reserved regions each NMI;
        // fixed projectile tiles such as bomb $14C-$14F remain in the untouched portion.
        _vramWrites.Enqueue(sizeInBytes: StandardObjectArtworkFormat.TransferByteCount,
            sourceAddress: StandardObjectArtworkAddresses.Source,
            destination: GameplayVram.ObjectCharactersDma());
        _artwork.TrailArtwork?.Tiles?.QueueTo(_vramWrites);

        // LoadEnemyTileData's transfers follow the overlapping standard OBJ upload. The
        // room enemy loader has already populated the modeled VRAM for immediate watches;
        // queueing the source records here makes enemy art win again at the accepted NMI.
        if (_enemies.IsLoaded)
            _enemies.PopulationLoader.QueueGraphicsUploads(_vramWrites);

        // The immutable first row bypasses WRAM and is DMAed straight from $80:988B.
        _vramWrites.Enqueue(sizeInBytes: GameplayHudDefinitions.TopRowByteCount,
            sourceAddress: GameplayHudDefinitions.TopRowAddress,
            destination: GameplayVram.HudTilemapDma());

        _hud.Initialize(_addressSpace, snapshot);
        _hud.QueueUpload(_addressSpace, _vramWrites);
    }
}
