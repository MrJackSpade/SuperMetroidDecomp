using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static Program;
using static SuiteTimingVerification;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Represents mother brain health palette verification.</summary>
internal static class MotherBrainHealthPaletteVerification
{
    /// <summary>verify mother brain health palette.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for Mother Brain health palette")]
    internal static void VerifyMotherBrainHealthPalette(string romPath)
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        var colors = new HashSet<string>();
        foreach (var (health, index) in new (ushort, int)[]
            { (36000, 0), (9000, 0), (8999, 1), (5400, 1), (5399, 2), (1800, 2), (1799, 3), (0, 3) })
        {
            var cgram = new SnesCgram();
            MotherBrainHealthPalette.Apply(cgram, health);
            VerifyCopy(0xade6a2, 65);
            VerifyCopy(0xade6a2, 145);
            VerifyCopy(0xade742, 177);
            colors.Add(string.Join(',', cgram.Colors.Slice(145, 15).ToArray()));
            AssertEqual((ushort)0, cgram.Colors[144], "health palette leaves transparent color untouched");
            AssertEqual((ushort)0, cgram.Colors[192], "health palette cannot overwrite Samus palette");

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

    /// <summary>mother brain health command.</summary>
    [VerificationCommand("--mother-brain-health", "argument1")]
    internal static int MotherBrainHealthCommand(string[] args)
    {
        Suite(nameof(VerifyMotherBrainHealthPalette), () => VerifyMotherBrainHealthPalette(args[1]));
        return 0;
    }
}
