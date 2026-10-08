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

    /// <summary>
    /// Steps a requested L/R pause-page switch to completion. Native page switches take
    /// a fade-out, a separate load dispatch and a delay-one fade-in (46 updates).
    /// </summary>
    private static void CompletePausePageTransition(PauseMenuState pause)
    {
        for (int update = 0; update < 64 && pause.PageTransitionActive; update++)
            pause.Step(0, 0);
        AssertTrue(!pause.PageTransitionActive, "pause page switch completes");
    }

    private static PauseMenuState CreateRetailPauseFixture(ISnesAddressSpace bus,
        SamusState samus, Bank80SystemState system, AreaId areaIndex,
        byte roomMapX, byte roomMapY, CartridgeAudioState? audio = null,
        SnesVram? gameplayVram = null, MapRevealMode mapRevealMode = MapRevealMode.None) =>
        RepositoryInstallation.CreatePause(bus, samus, system, areaIndex, roomMapX, roomMapY, audio,
            gameplayVram, mapRevealMode);

    /// <summary>
    /// The shared installation's map presentation, the same extracted content contract the
    /// playable host uses. Independent game instances still own their mutable state.
    /// </summary>
    private static AreaMapPresentationCatalog RetailPresentationFixture() =>
        RepositoryInstallation.Maps;
}
