using SuperMetroid.Core.Hud;

namespace SuperMetroid.Core.World;

/// <summary>
/// Gameplay HUD initialization: the HUD's presentation.
/// </summary>
internal sealed class GameplayHudSetup
{
    /// <summary>Selects installed HUD artwork.</summary>
    private readonly RuntimeArtwork _artwork;

    /// <summary>Owns the live HUD picture and its palette or map presentation binding.</summary>
    private readonly HudState _hud;

    /// <summary>Binds the runtime owners HUD initialization selects artwork for.</summary>
    internal GameplayHudSetup(GameplayHudSetupDependencies dependencies)
    {
        _artwork = dependencies.Artwork;

        _hud = dependencies.Hud;
    }

    /// <summary>
    /// Initializes the HUD, as gameplay setup does. The HUD's rows and static top row are its own, and the accepted frame
    /// shows them in the HUD sheet's pictures (#1466). Enemy frames show their own
    /// pictures throughout (#1513), so setup selects no enemy artwork.
    /// </summary>
    public void InitializeHud(HudSnapshot snapshot)
    {
        _hud.BindPresentation(_artwork.MapPresentation?.GameplayHud);
        _hud.Initialize(snapshot);
    }
}
