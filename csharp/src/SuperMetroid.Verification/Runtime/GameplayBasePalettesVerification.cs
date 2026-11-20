using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Runtime;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Runtime;

using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Assets;
using SuperMetroid.AssetExtraction.Installation;

/// <summary>Checks installed gameplay CGRAM base colors and room-entry sprite restoration.</summary>
internal static class GameplayBasePalettesVerification
{
    /// <summary>Compares cartridge colors and exercises ROM-free runtime construction.</summary>
    internal static void VerifyGameplayBasePalettes(string sourceRom)
    {
        // Only the import reads the base palette (#1544), from its import data (#1637); the check reads it where the import does.
        string importData = CartridgeImportContent.ImportData(GameInstallationLayout.GameplayBasePaletteDirectoryName);
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom);
        GameplayBasePalette baseline = GameplayBasePaletteFiles.LoadForImport(importData);
        AssertTrue(!Directory.Exists(Path.Combine(RepositoryInstallation.Installation.ContentDirectory, GameInstallationLayout.GameplayBasePaletteDirectoryName)),
            "the installation holds no gameplay base palette");
        // The starting image holds the background colours, which checks decode, then the sprite colours, in the cartridge's order.
        RgbColor[] background = GameplayStartingColoursAccess.BackgroundColours(importData);
        for (int color = 0; color < SnesCgram.ColorCount; color++)
            AssertEqual(ReadWord(bus, GameplayBasePaletteSourceDefinitions.InitialPalette + color * 2),
                color < background.Length ? background[color] : baseline.Starting.SpriteColours[color - background.Length],
                $"installed starting colour {color}");
        for (int color = 0; color < GameplayBasePaletteFormat.SpriteColorCount; color++)
            AssertEqual(ReadWord(bus, GameplayBasePaletteSourceDefinitions.CommonSpritePalette + color * 2),
                baseline.CommonSprites[color], $"installed shared OBJ color {color}");

        // Runtime construction reads no cartridge and binds no base palette: only the import reads it (#1512).
        _ = SuperMetroid.Core.Runtime.SuperMetroidRuntime.Create(GameplayAssists.None, InstalledFixtures.GameArtwork.Gameplay);

        Console.WriteLine("Gameplay base palettes: 256+16 cartridge colors and guarded runtime construction pass.");

        static ushort ReadWord(ISnesAddressSpace addressSpace, int address) =>
            LittleEndianBytes.Word(GameplayBasePalettesRomByte(addressSpace, address), GameplayBasePalettesRomByte(addressSpace, address + 1));
    }

    /// <summary>Reads native palette bytes for the installed-color comparison.</summary>
    private static byte GameplayBasePalettesRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Runs the gameplay base palette installation check.</summary>
    [VerificationCommand("--gameplay-base-palettes")]
    internal static int GameplayBasePalettesCommand()
    {
        string gameplayPaletteRom = RepositoryRomPath;
        Suite(nameof(VerifyGameplayBasePalettes), () => VerifyGameplayBasePalettes(gameplayPaletteRom));
        return 0;
    }
}
