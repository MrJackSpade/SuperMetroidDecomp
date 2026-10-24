using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Hud.Artwork;

namespace SuperMetroid.Core.World;

/// <summary>
/// Gameplay HUD initialization and the gameplay HUD character-base selection.
/// </summary>
internal sealed class GameplayHudSetup
{
    /// <summary>Selects installed HUD artwork.</summary>
    private readonly RuntimeArtwork _artwork;
    /// <summary>Receives the gameplay BG3 character-base selection.</summary>
    private readonly GameplayDisplayState _display;
    /// <summary>Owns the live HUD tilemap and its palette or map presentation binding.</summary>
    private readonly HudState _hud;
    /// <summary>Supplies Samus state for HUD item and resource display.</summary>
    private readonly SamusFrameState _player;
    /// <summary>The session whose OBJ character table enemy pictures are selected over.</summary>
    private readonly EnemySession _enemySession;
    /// <summary>The room whose loaded enemy characters show again over the standard sprite characters.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Carries the HUD characters' and the loaded enemies' character uploads, which still go through the queue.</summary>
    private readonly VramWriteQueue _vramWrites;

    /// <summary>Binds the runtime owners HUD initialization selects artwork for.</summary>
    internal GameplayHudSetup(GameplayHudSetupDependencies dependencies)
    {
        _artwork = dependencies.Artwork;
        _display = dependencies.Display;
        _hud = dependencies.Hud;
        _player = dependencies.Player;
        _enemySession = dependencies.EnemySession;
        _room = dependencies.Room;
        _vramWrites = dependencies.VramWrites;
    }

    /// <summary>Selects the ordinary BG3 character base (BG34NBA=$04), as gameplay PPU initialization does.</summary>
    [PpuLayoutBoundary("rendering view: the BG34NBA BG3 character-base register shadow")]
    internal void SelectGameplayHudCharacterBase() => _display.GameplayHudCharacterBaseWord = GameplayVramLayout.HudCharacterWord;

    /// <summary>
    /// Queues the standard BG3 characters' upload at the ordinary HUD base, followed by its clearing half: a game has them
    /// there whenever it enters a room through a door, as only Kraid's room writes over them and his death restores them
    /// before any of its doors opens.
    /// </summary>
    internal void QueueHudCharacters() =>
        _vramWrites.EnqueueBackgroundAsset(VramAssetId.StandardHudTiles, HudTileAtlasFormat.TransferByteCount, GameplayVram.HudCharacterUploadDma);

    /// <summary>
    /// Selects the standard sprite characters over the start of the OBJ character table, as <c>$82:8318</c>'s setup does:
    /// enemy frames drawn over characters an earlier room's encounter replaced show the room's own pictures again from
    /// here (#1406). Setup selects them while the screen is blank, before any room loads its enemies' characters.
    /// </summary>
    internal void ShowStandardObjectCharacters() =>
        _enemySession.ObjCharacters.Record(0, StandardObjectArtworkFormat.TransferByteCount,
            new AssetObjCharacters(VramAssetId.StandardObjectTiles));

    /// <summary>
    /// Selects the standard BG3 and sprite characters and initializes the HUD tilemap, as <c>$82:82E2</c> and
    /// <c>$80:9A79</c> do. The HUD's rows and static top row are its own, and the accepted frame shows them as it latches
    /// them.
    /// </summary>
    public void InitializeHud(HudSnapshot snapshot)
    {
        _hud.BindPresentation(_artwork.MapPresentation?.GameplayHud, _player.Samus);
        // The HUD's characters are installed artwork bound at the NMI (#1350).
        if (_artwork.MapPresentation is null)
            throw new InvalidOperationException("Initializing the HUD requires installed HUD artwork.");
        QueueHudCharacters();
        ShowStandardObjectCharacters();
        // A loaded room's enemy characters show over the standard ones again from the next accepted frame.
        // Before the gameplay runtime's first room there is no enemy population.
        if (_room.Scope?.Enemies is { IsLoaded: true } enemies)
            enemies.GraphicsSet.QueueGraphicsUploads(_vramWrites);
        _hud.Initialize(snapshot);
    }
}
