using SuperMetroid.Verification.Harness;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Hardware.ImportedSpritemapsVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Yard;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Verification.Enemies.Yard;

/// <summary>Checks installed Yard animation selectors and editable OAM against retail data.</summary>
internal static class YardVisualsVerification
{
    /// <summary>Checks every pose the Yard's programs show against editable stock OAM equal to the cartridge's.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static void VerifyInstalledYardVisuals(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog stock)
    {
        HashSet<ushort> shown = YardAnimations.Spritemaps(Enum.GetValues<YardProgram>()).ToHashSet();
        foreach (ushort pointer in shown)
        {
            AssertTrue((stock.Spritemaps ?? throw new InvalidOperationException("Verification requires stock.Spritemaps.")).TryGetDisplay(
                    YardVisualDefinitions.Bank, pointer, out EnemySpritemapParts installedParts),
                $"Yard pose $A3:{pointer:X4} has editable OAM");
            var nativeOam = new OamBuffer();
            var installedOam = new OamBuffer();
            DrawImportedEnemySpritemap(rom, nativeOam, new EnemySpritemapDraw { Bank = YardVisualDefinitions.Bank, Pointer = pointer, Origin = new(128, 128), PaletteBits = 0, BaseTileIndex = 0 });
            installedOam.AddEnemySpritemap(installedParts, new Position16<OamPixels>(128, 128), 0, 0);
            AssertTrue(nativeOam.ImageEquals(installedOam) &&
                       nativeOam.NextByteOffset == installedOam.NextByteOffset,
                $"Yard pose $A3:{pointer:X4} installed OAM matches cartridge");
        }
        AssertTrue(YardVisualDefinitions.Frames().Select(frame => frame.Pointer).ToHashSet().SetEquals(shown),
            "the Yard's programs show exactly its 104 installed frames");
        Console.WriteLine("  Yard visuals: 104 editable OAM frames shown by its programs match the cartridge.");
    }

    /// <summary>Runs the Yard visual check against the installed stock artwork, without re-extracting every enemy's art.</summary>
    [VerificationCommand("--yard-visuals")]
    internal static int YardVisualsCommand()
    {
        Suite(nameof(VerifyInstalledYardVisuals), () => VerifyInstalledYardVisuals(
            CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")), InstalledFixtures.EnemyTiles));
        return 0;
    }
}
