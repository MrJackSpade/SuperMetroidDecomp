namespace SuperMetroid.Desktop;

public sealed partial class PlayableGameControl
{
    // Kept by the host across state loads. Restart reloads disk overrides explicitly.
    /// <summary>Installed area-map and pause-screen presentation assets retained for state rebinding.</summary>
    private SuperMetroid.Core.Assets.AreaMapPresentationCatalog? mapPresentation;
    /// <summary>Installed base gameplay palette assets retained while the game state is reloaded.</summary>
    private SuperMetroid.Core.Assets.GameplayBasePaletteCatalog? gameplayBasePalettes;
    /// <summary>Installed standard object graphics retained for rebinding after a state load.</summary>
    private SuperMetroid.Core.Assets.RoomCharacterAtlas? standardObjectArt;
}
