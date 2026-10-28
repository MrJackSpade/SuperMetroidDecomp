using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Mother Brain's four health palettes and strict threshold boundaries against native CGRAM data.</summary>
internal static class MotherBrainHealthPaletteVerification
{
    /// <summary>Compares each selected health palette with cartridge colors while guarding neighboring CGRAM slots.</summary>
    internal static void VerifyMotherBrainHealthPalette(string romPath)
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        var colors = new HashSet<string>();
        foreach (var (health, index) in new VerifyMotherBrainHealthPaletteCase[]
            { new VerifyMotherBrainHealthPaletteCase(36000, 0), new VerifyMotherBrainHealthPaletteCase(9000, 0), new VerifyMotherBrainHealthPaletteCase(8999, 1), new VerifyMotherBrainHealthPaletteCase(5400, 1), new VerifyMotherBrainHealthPaletteCase(5399, 2), new VerifyMotherBrainHealthPaletteCase(1800, 2), new VerifyMotherBrainHealthPaletteCase(1799, 3), new VerifyMotherBrainHealthPaletteCase(0, 3) })
        {
            var cgram = new GameplayColours(new SpriteColourSets());
            MotherBrainHealthPalette.Apply(cgram, health, InstalledFixtures.Maps.MotherBrainHealthPalette);
            VerifyCopy(0xade6a2, 65);
            VerifyCopy(0xade6a2, 145);
            VerifyCopy(0xade742, 177);
            colors.Add(string.Join(',', cgram.Colors.Slice(145, 15).ToArray()));
            AssertEqual(0, cgram.Colors[144], "health palette leaves transparent color untouched");
            AssertEqual(0, cgram.Colors[192], "health palette cannot overwrite Samus palette");

            void VerifyCopy(int table, int destination)
            {
                int pointer = table + index * 2;
                int source = (0xad0000|bus.ReadCartridgeWord(pointer));
                for (int color = 0; color < 15; color++)
                    AssertEqual(bus.ReadCartridgeWord(source + color * 2),
                        cgram.Colors[destination + color], $"health {health} palette {index} color {color}");
            }
        }
        AssertEqual(4, colors.Count, "all four native damage palettes are distinct");
        Console.WriteLine("Mother Brain health palettes: all strict thresholds, ROM copies and neighboring colors verified.");
    }

    /// <summary>Runs the focused Mother Brain health palette verification command.</summary>
    [VerificationCommand("--mother-brain-health", "argument1")]
    internal static int MotherBrainHealthCommand(string[] args)
    {
        Suite(nameof(VerifyMotherBrainHealthPalette), () => VerifyMotherBrainHealthPalette(args[1]));
        return 0;
    }
}
