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
    /// <summary>The background artwork bindings the HUD's characters are shown through.</summary>
    private readonly BgArtworkBindings _backgrounds;

    /// <summary>Binds the runtime owners HUD initialization selects artwork for.</summary>
    internal GameplayHudSetup(GameplayHudSetupDependencies dependencies)
    {
        _artwork = dependencies.Artwork;
        _display = dependencies.Display;
        _hud = dependencies.Hud;
        _player = dependencies.Player;
        _enemySession = dependencies.EnemySession;
        _room = dependencies.Room;
        _backgrounds = dependencies.Backgrounds;
    }

    /// <summary>Selects the standard layer-3 characters, as gameplay PPU initialization does.</summary>
    internal void SelectGameplayHudCharacterBase() => _display.Layer3Characters = BgCharacterSet.Layer3;

    /// <summary>
    /// Shows the standard BG3 characters over the standard layer-3 characters (#1406): a game has them there whenever it
    /// enters a room through a door, as only Kraid's room covers them and his death restores them before any of its doors
    /// opens. Every caller publishes the forced-blank uploads before a room loads, so the room's own backgrounds still bind
    /// over them in the cartridge's order.
    /// </summary>
    /// <exception cref="InvalidOperationException">The HUD artwork is not installed.</exception>
    internal void ShowHudCharacters()
    {
        if (_artwork.MapPresentation is null)
            throw new InvalidOperationException("Showing the HUD's characters requires installed HUD artwork.");
        _backgrounds.Bind(BgArtworkIdentity.Asset(InstalledArtworkId.StandardHudTiles), BgCharacterSet.Layer3, 0,
            BgCharacterSet.Layer3.CharactersIn(HudTileAtlasFormat.CharacterByteCount));
    }

    /// <summary>
    /// Selects the standard sprite characters over the start of the OBJ character table, as <c>$82:8318</c>'s setup does:
    /// enemy frames drawn over characters an earlier room's encounter replaced show the room's own pictures again from
    /// here (#1406). Setup selects them while the screen is blank, before any room loads its enemies' characters.
    /// </summary>
    internal void ShowStandardObjectCharacters() =>
        _enemySession.SpriteCharacters.Show(new SpriteArtworkPlacement(new InstalledArtworkCharacters(InstalledArtworkId.StandardObjectTiles), 0,
            StandardObjectArtworkFormat.ByteCount / SpriteArtworkPlacements.CharacterByteCount));

    /// <summary>
    /// Selects the standard BG3 and sprite characters and initializes the HUD tilemap, as <c>$82:82E2</c> and
    /// <c>$80:9A79</c> do. The HUD's rows and static top row are its own, and the accepted frame shows them as it latches
    /// them.
    /// </summary>
    public void InitializeHud(HudSnapshot snapshot)
    {
        _hud.BindPresentation(_artwork.MapPresentation?.GameplayHud, _player.Samus);
        ShowHudCharacters();
        ShowStandardObjectCharacters();
        // A loaded room's enemy characters show over the standard ones again from the next accepted frame.
        // Before the gameplay runtime's first room there is no enemy population.
        if (_room.Scope?.Enemies is { IsLoaded: true } enemies)
            enemies.GraphicsSet.ShowGraphics();
        _hud.Initialize(snapshot);
    }
}
