using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Verification.Harness;

/// <summary>Constructs installed retail states used by focused frontend and instruction verifications.</summary>
internal static class RetailGameFixtureVerification
{
    /// <summary>Creates a game host from a single retail import, with optional game settings and raster rendering.</summary>
    internal static SuperMetroidGame CreateRetailGameFixture(ISnesAddressSpace bus,
        SuperMetroidGameOptions? gameOptions = null, bool renderGameplayFrames = true) =>
        InstalledFixtures.CreateGame(bus, gameOptions, renderGameplayFrames);

    /// <summary>Creates a Ceres destruction state with optional supplied audio and cinematic artwork.</summary>
    internal static CeresDestructionCinematicState CreateRetailDestructionFixture(ISnesAddressSpace bus,
        CartridgeAudioState? audio = null, IntroCinematicArtworkCatalog? artwork = null) =>
        InstalledFixtures.CreateDestruction(bus, audio, artwork);

    /// <summary>
    /// Independent reference for ending-actor instruction streams: reads each word from the
    /// cartridge's bank $8B, where production reads the compiled instruction definitions.
    /// </summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for ending instruction-program words")]
    internal static Func<ushort, ushort> EndingCartridgeInstructionWord(ISnesAddressSpace bus) =>
        pointer => CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank(0x8b0000 | pointer);

    /// <summary>Creates an ending state with supplied game time, inventory, audio, and language selection.</summary>
    internal static EndingCreditsState CreateRetailEndingFixture(ISnesAddressSpace bus,
        CartridgeAudioState audio, ushort gameTimeHours, ushort gameTimeMinutes,
        EndingInventorySnapshot inventory = default, bool japaneseText = false) =>
        InstalledFixtures.CreateEnding(bus, audio, gameTimeHours, gameTimeMinutes, inventory, japaneseText);
    /// <summary>Creates an intro state with optionally supplied audio and installed visual catalogs.</summary>
    internal static IntroCinematicState CreateRetailIntroFixture(ISnesAddressSpace bus, CartridgeAudioState? audio = null,
        IntroFontAtlas? introFont = null, IntroCinematicArtworkCatalog? characterArtwork = null,
        BeamTileCatalog? beamArtwork = null, SamusBodyArtworkCatalog? samusBodyArtwork = null) =>
        InstalledFixtures.CreateIntro(bus, audio, introFont, characterArtwork, beamArtwork, samusBodyArtwork);
}
