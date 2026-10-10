using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>Creates a message-box state bound to the retail titles, panels, and notice artwork.</summary>
    /// <returns>A gameplay-message state ready to begin messages using installed presentation data.</returns>
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

    /// <summary>Creates a pause-menu state through the repository installation factory for a prepared retail gameplay fixture.</summary>
    /// <param name="bus">Address space used by the installation to read gameplay data.</param>
    /// <param name="samus">Player state whose inventory and equipment are shown by the pause menu.</param>
    /// <param name="system">Bank-$80 system state used by the pause menu.</param>
    /// <param name="areaIndex">Area whose map and pause-page data are presented.</param>
    /// <param name="roomMapX">Current room's horizontal map coordinate.</param>
    /// <param name="roomMapY">Current room's vertical map coordinate.</param>
    /// <param name="audio">Optional audio state shared with the pause-menu fixture.</param>
    /// <param name="gameplayVram">Optional gameplay VRAM supplied to the pause-menu factory.</param>
    /// <param name="mapRevealMode">Map-reveal policy used when constructing the pause menu.</param>
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
