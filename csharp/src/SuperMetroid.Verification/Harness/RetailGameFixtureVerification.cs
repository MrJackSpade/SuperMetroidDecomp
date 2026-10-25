using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Verification.Harness;

/// <summary>Constructs installed retail states used by focused frontend and instruction verifications.</summary>
internal static class RetailGameFixtureVerification
{
    /// <summary>Creates a game host from a single retail import over <paramref name="saves"/>, with optional game settings and raster rendering.</summary>
    internal static SuperMetroidGame CreateRetailGameFixture(SuperMetroid.Core.Saves.SaveSlotStore saves,
        SuperMetroidGameOptions? gameOptions = null, bool renderGameplayFrames = true,
        SuperMetroidGameHostServices? hostServices = null, SharedRandom? random = null) =>
        InstalledFixtures.CreateGame(saves, gameOptions, renderGameplayFrames, hostServices, random);

    /// <summary>Creates a Ceres destruction state with optional supplied audio and cinematic artwork.</summary>
    internal static CeresDestructionCinematicState CreateRetailDestructionFixture(AudioQueues? audio = null, IntroCinematicArtworkCatalog? artwork = null) =>
        InstalledFixtures.CreateDestruction(audio, artwork);

    /// <summary>Creates an ending state with supplied game time, inventory, audio, and language selection.</summary>
    internal static EndingCreditsState CreateRetailEndingFixture(AudioQueues audio, ushort gameTimeHours, ushort gameTimeMinutes,
        EndingInventorySnapshot inventory = default, bool japaneseText = false) =>
        InstalledFixtures.CreateEnding(audio, gameTimeHours, gameTimeMinutes, inventory, japaneseText);
    /// <summary>Creates an intro state with optionally supplied audio and installed visual catalogs.</summary>
    internal static IntroCinematicState CreateRetailIntroFixture(AudioQueues? audio = null,
        IntroFontAtlas? introFont = null, IntroCinematicArtworkCatalog? characterArtwork = null,
        BeamPaletteCatalog? beamPalettes = null, SamusBodyArtworkCatalog? samusBodyArtwork = null) =>
        InstalledFixtures.CreateIntro(audio, introFont, characterArtwork, beamPalettes, samusBodyArtwork);
}
