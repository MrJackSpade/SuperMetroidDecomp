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
    private static AreaMapPresentationCatalog? retailPresentationFixture;

    private static PauseMenuState CreateRetailPauseFixture(ISnesAddressSpace bus,
        SamusState samus, Bank80SystemState system, AreaId areaIndex,
        byte roomMapX, byte roomMapY, CartridgeAudioState? audio = null,
        SnesVram? gameplayVram = null, MapRevealMode mapRevealMode = MapRevealMode.None) =>
        new(bus, samus, system, areaIndex, roomMapX, roomMapY, audio,
            gameplayVram, mapRevealMode, RetailPresentationFixture());

    /// <summary>
    /// Installs the same extracted content contract used by the playable host in
    /// frontend verifier fixtures. Reusing one catalog avoids repeated full-map
    /// extraction while independent game instances still own their mutable state.
    /// </summary>
    private static AreaMapPresentationCatalog RetailPresentationFixture()
    {
        if (retailPresentationFixture is not null)
            return retailPresentationFixture;

        string root = Path.GetFullPath(Path.Combine("csharp", "test-temp",
            "retail-presentation-fixture-" + Guid.NewGuid().ToString("N")));
        MapPresentationExtractor.Extract(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")),
            Path.Combine(root, "game", "maps"), "test-provenance");
        return retailPresentationFixture = new GameInstallation(root).LoadMaps();
    }
}
