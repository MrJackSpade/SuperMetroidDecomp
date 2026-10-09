using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Audio;

internal static partial class Program
{
    /// <summary>Creates a production game instance over the supplied address space using repository-installed services.</summary>
    /// <param name="bus">Cartridge address space provided to the game.</param>
    /// <param name="gameOptions">Optional runtime configuration; repository defaults apply when omitted.</param>
    /// <param name="renderGameplayFrames">Whether the game should create gameplay frame rendering services.</param>
    private static SuperMetroidGame CreateRetailGameFixture(ISnesAddressSpace bus,
        SuperMetroidGameOptions? gameOptions = null, bool renderGameplayFrames = true) =>
        RepositoryInstallation.CreateGame(bus, gameOptions, renderGameplayFrames);

    /// <summary>Creates the production Ceres destruction state with optional audio and cinematic artwork dependencies.</summary>
    /// <param name="bus">Cartridge address space supplying the cinematic's native data.</param>
    /// <param name="audio">Optional audio state shared with the cinematic.</param>
    /// <param name="artwork">Optional imported artwork catalog used to resolve cinematic visuals.</param>
    private static CeresDestructionCinematicState CreateRetailDestructionFixture(ISnesAddressSpace bus,
        CartridgeAudioState? audio = null, IntroCinematicArtworkCatalog? artwork = null) =>
        RepositoryInstallation.CreateDestruction(bus, audio, artwork);

    /// <summary>
    /// Independent reference for ending-actor instruction streams: reads each word from the
    /// cartridge's bank $8B, where production reads the compiled instruction definitions.
    /// </summary>
    private static Func<ushort, ushort> EndingCartridgeInstructionWord(ISnesAddressSpace bus) =>
        pointer => (ushort)(bus.ReadByte(0x8b0000 | pointer) |
            bus.ReadByte(0x8b0000 | unchecked((ushort)(pointer + 1))) << 8);

    /// <summary>Creates the production ending-credits state initialized with the supplied time and inventory snapshot.</summary>
    /// <param name="bus">Cartridge address space supplying ending sequence data.</param>
    /// <param name="audio">Audio state whose queue and playback status are observed by the ending.</param>
    /// <param name="gameTimeHours">Recorded play-time hours presented by the ending.</param>
    /// <param name="gameTimeMinutes">Recorded play-time minutes presented by the ending.</param>
    /// <param name="inventory">Inventory snapshot used to select ending text and completion presentation.</param>
    /// <param name="japaneseText">Whether Japanese ending text should be selected.</param>
    private static EndingCreditsState CreateRetailEndingFixture(ISnesAddressSpace bus,
        CartridgeAudioState audio, ushort gameTimeHours, ushort gameTimeMinutes,
        EndingInventorySnapshot inventory = default, bool japaneseText = false) =>
        RepositoryInstallation.CreateEnding(bus, audio, gameTimeHours, gameTimeMinutes, inventory, japaneseText);

    /// <summary>Creates the production intro-cinematic state with optional audio, font, character, beam, and body artwork assets.</summary>
    /// <param name="bus">Cartridge address space supplying intro sequence data.</param>
    /// <param name="audio">Optional audio state shared with the intro.</param>
    /// <param name="introFont">Optional imported font atlas for intro text.</param>
    /// <param name="characterArtwork">Optional artwork catalog for intro characters.</param>
    /// <param name="beamArtwork">Optional tile catalog for Samus's beam effects.</param>
    /// <param name="samusBodyArtwork">Optional body artwork catalog for Samus.</param>
    private static IntroCinematicState CreateRetailIntroFixture(ISnesAddressSpace bus, CartridgeAudioState? audio = null,
        IntroFontAtlas? introFont = null, IntroCinematicArtworkCatalog? characterArtwork = null,
        BeamTileCatalog? beamArtwork = null, SamusBodyArtworkCatalog? samusBodyArtwork = null) =>
        RepositoryInstallation.CreateIntro(bus, audio, introFont, characterArtwork, beamArtwork, samusBodyArtwork);
}
