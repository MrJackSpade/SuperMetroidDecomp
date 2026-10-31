using SuperMetroid.Core;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Saves;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Frontend.FileSelect;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;

/// <summary>Checks the update timing of loading a save continued from the file-select map.</summary>
internal static class FileSelectMapLoadTimingVerification
{
    /// <summary>
    /// The map's final dispatch forces blank and waits for an NMI ($81:AF8C) before it raises state six.
    /// State six's ordinary branch ($82:801D) then loads the game in one dispatch and waits for the same
    /// 67 NMIs as state $1F, after which the fade-in (state seven) runs. The port loaded the game inside
    /// the map's final update and went straight to the fade-in; the contributed lowc movie continues a
    /// save this way after its console reset (update 85,977).
    /// </summary>
    internal static void VerifyFileSelectMapLoadTiming()
    {
        byte[] rom = File.ReadAllBytes(Path.GetFullPath("Super Metroid.smc"));
        var bus = new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(rom);
        var save = new SuperMetroidSaveSnapshot { Area = 4, SaveStation = 0, Health = 99, MaxHealth = 99 };
        save.Progress.SetAreaMap(SuperMetroid.Core.Progression.AreaId.Maridia, acquired: true);
        save.Progress.UsedStations(SuperMetroid.Core.Progression.AreaId.Maridia).Add(0);
        var saves = new SuperMetroid.Core.Saves.SaveSlotStore(RetailPresentationFixture());
        saves.SaveSlot(0, save);
        var game = CreateRetailGameFixture(saves);
        game.BindArtwork(game.Artwork with { MapPresentation = RetailPresentationFixture() });

        var states = new List<SuperMetroidGameState>();
        for (int tick = 0; tick < 2000 && game.GameState != SuperMetroidGameState.MainGameplay; tick++)
        {
            game.Step(tick % 47 == 0 ? ControllerButton.Start : ControllerButton.None);
            states.Add(game.GameState);
        }
        AssertEqual(SuperMetroidGameState.MainGameplay, game.GameState, "the continued save reaches gameplay");
        int firstLoading = states.IndexOf(SuperMetroidGameState.LoadingGameData);
        AssertTrue(firstLoading > 0, "the continued save passes through state six");
        AssertEqual(SuperMetroidGameState.FileSelectMap, states[firstLoading - 1], "state six follows the map directly");
        int loadingUpdates = states.Skip(firstLoading).TakeWhile(state => state == SuperMetroidGameState.LoadingGameData).Count();
        AssertEqual(1 + GameLoadingDefinitions.OrdinaryLoadWaits, loadingUpdates,
            "state six's raising NMI, its loading dispatch and its NMI waits");
        AssertEqual(SuperMetroidGameState.MainGameplayFadeIn, states[firstLoading + loadingUpdates], "the fade-in follows the load");
        Console.WriteLine($"  File-select map load: state six lasts {loadingUpdates} updates, then the fade-in.");
    }

    /// <summary>Runs the file-select map load timing check from the verification command line.</summary>
    [VerificationCommand("--file-select-map-load-timing")]
    internal static int FileSelectMapLoadTimingCommand()
    {
        Suite(nameof(VerifyFileSelectMapLoadTiming), () => VerifyFileSelectMapLoadTiming());
        return 0;
    }
}
