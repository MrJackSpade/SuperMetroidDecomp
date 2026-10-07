using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Audio;

internal static partial class Program
{
    private static SuperMetroidGame CreateRetailGameFixture(ISnesAddressSpace bus,
        SuperMetroidGameOptions? gameOptions = null, bool renderGameplayFrames = true) =>
        RepositoryInstallation.CreateGame(bus, gameOptions, renderGameplayFrames);

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

    private static EndingCreditsState CreateRetailEndingFixture(ISnesAddressSpace bus,
        CartridgeAudioState audio, ushort gameTimeHours, ushort gameTimeMinutes,
        EndingInventorySnapshot inventory = default, bool japaneseText = false) =>
        RepositoryInstallation.CreateEnding(bus, audio, gameTimeHours, gameTimeMinutes, inventory, japaneseText);
    private static IntroCinematicState CreateRetailIntroFixture(ISnesAddressSpace bus, CartridgeAudioState? audio = null,
        IntroFontAtlas? introFont = null, IntroCinematicArtworkCatalog? characterArtwork = null,
        BeamTileCatalog? beamArtwork = null, SamusBodyArtworkCatalog? samusBodyArtwork = null) =>
        RepositoryInstallation.CreateIntro(bus, audio, introFont, characterArtwork, beamArtwork, samusBodyArtwork);
}
