using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Hud;

namespace SuperMetroid.Core.World;

/// <summary>
/// Gameplay HUD initialization: the HUD's presentation and the standard sprite characters.
/// </summary>
internal sealed class GameplayHudSetup
{
    /// <summary>Selects installed HUD artwork.</summary>
    private readonly RuntimeArtwork _artwork;

    /// <summary>Owns the live HUD tilemap and its palette or map presentation binding.</summary>
    private readonly HudState _hud;
    /// <summary>Supplies Samus state for HUD item and resource display.</summary>
    private readonly SamusFrameState _player;
    /// <summary>The session whose OBJ character table enemy pictures are selected over.</summary>
    private readonly EnemySession _enemySession;
    /// <summary>The room whose loaded enemy characters show again over the standard sprite characters.</summary>
    private readonly ActiveRoomState _room;

    /// <summary>Binds the runtime owners HUD initialization selects artwork for.</summary>
    internal GameplayHudSetup(GameplayHudSetupDependencies dependencies)
    {
        _artwork = dependencies.Artwork;

        _hud = dependencies.Hud;
        _player = dependencies.Player;
        _enemySession = dependencies.EnemySession;
        _room = dependencies.Room;
    }


    /// <summary>
    /// Selects the standard sprite characters over the start of the OBJ character table, as <c>$82:8318</c>'s setup does:
    /// enemy frames drawn over characters an earlier room's encounter replaced show the room's own pictures again from
    /// here (#1406). Setup selects them while the screen is blank, before any room loads its enemies' characters.
    /// </summary>
    internal void ShowStandardObjectCharacters() =>
        _enemySession.SpriteLooks.Show(new InstalledArtworkCharacters(InstalledArtworkId.StandardObjectTiles));

    /// <summary>
    /// Selects the standard sprite characters and initializes the HUD, as <c>$82:82E2</c> and <c>$80:9A79</c> do. The
    /// HUD's rows and static top row are its own, and the accepted frame shows them in the BG3 word sheet's pictures, which
    /// show the standard BG3 characters (#1466).
    /// </summary>
    public void InitializeHud(HudSnapshot snapshot)
    {
        _hud.BindPresentation(_artwork.MapPresentation?.GameplayHud, _player.Samus);
        ShowStandardObjectCharacters();
        // A loaded room's enemy characters show over the standard ones again from the next accepted frame.
        // Before the gameplay runtime's first room there is no enemy population.
        if (_room.Scope?.Enemies is { IsLoaded: true } enemies)
            enemies.GraphicsSet.ShowGraphics();
        _hud.Initialize(snapshot);
    }
}
