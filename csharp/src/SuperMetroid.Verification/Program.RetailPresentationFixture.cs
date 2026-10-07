using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static GameplayMessageBoxState CreateGameplayMessageFixture()
    {
        var maps = RetailPresentationFixture();
        var state = new GameplayMessageBoxState();
        state.BindPresentation(maps.GameplayMessageTitles, maps.GameplayMessagePanels, maps.GameplayMessageNotices);
        return state;
    }

    private static PauseMenuState CreateRetailPauseFixture(ISnesAddressSpace bus,
        SamusState samus, Bank80SystemState system, AreaId areaIndex,
        byte roomMapX, byte roomMapY, CartridgeAudioState? audio = null,
        SnesVram? gameplayVram = null, MapRevealMode mapRevealMode = MapRevealMode.None) =>
        new(bus, samus, system, areaIndex, roomMapX, roomMapY, audio,
            gameplayVram, mapRevealMode, RetailPresentationFixture());

    private static AreaMapPresentationCatalog? retailPresentationFixture;

    /// <summary>
    /// The shared installation's map presentation, the same extracted content contract the
    /// playable host uses. Independent game instances still own their mutable state.
    /// </summary>
    private static AreaMapPresentationCatalog RetailPresentationFixture() =>
        retailPresentationFixture ??= runtimeFixtureInstallation.Value.LoadMaps();
}
