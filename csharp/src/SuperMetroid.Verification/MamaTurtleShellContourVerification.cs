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
using static SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Checks the asymmetric Mama Turtle shell contour and its effect
/// on sleeping-parent collision geometry.</summary>
internal static class MamaTurtleShellContourVerification
{
    /// <summary>
    /// Compares the complete asymmetric shell contour with the pinned cartridge and drives
    /// the real sleeping-parent collision path with the old source range forbidden.
    /// </summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Mama Turtle shell contour definitions")]
    internal static void VerifyMamaTurtleShellContourDefinitions()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        for (int index = 0; index < MamaTurtleShellContourDefinitions.EntryCount; index++)
        {
            int address = MamaTurtleShellContourDefinitions.SourceAddress + index * 2;
            short expected = unchecked((short)rom.ReadCartridgeWord(address));
            AssertEqual(expected, MamaTurtleShellContourDefinitions.GetRawOffset(index),
                $"Mama Turtle shell contour word {index}");
        }

        for (short difference = -23; difference <= 23; difference++)
        {
            int distance = Math.Abs((int)difference);
            int index = difference < 0
                ? distance + MamaTurtleShellContourDefinitions.HalfWidth
                : distance;
            AssertEqual(MamaTurtleShellContourDefinitions.GetRawOffset(index),
                MamaTurtleShellContourDefinitions.GetOffset(difference),
                $"Mama Turtle shell contour direction/index for difference {difference}");
        }
        AssertThrows<ArgumentOutOfRangeException>(
            () => MamaTurtleShellContourDefinitions.GetOffset(-24),
            "Mama Turtle shell contour rejects left distance twenty-four");
        AssertThrows<ArgumentOutOfRangeException>(
            () => MamaTurtleShellContourDefinitions.GetOffset(24),
            "Mama Turtle shell contour rejects right distance twenty-four");

        var mama = new RoomEnemySlot(0) { Position = FixedPosition<RoomPixels>.FromWhole(new(0x0200, 0x0200))};
        var state = new MamaTurtleEnemyState(mama) { AsleepFlag = NativeCountdown.Start(1) };
        var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(0, 0)) };
        Action<RoomEnemySlot, MamaTurtleEnemyState, SamusState> runAsleep = MamaTurtleBehavior.RunMamaTurtleAsleep;

        int productionCases = 0;
        for (short difference = -30; difference <= 30; difference++)
        {
            mama.Radius = mama.Radius.WithY(0xffff);
            mama.Properties = 0;
            samus.Position = samus.Position.WithWholeX(unchecked((ushort)(mama.Position.X.Whole - difference)));
            runAsleep(mama, state, samus);

            if (Math.Abs((int)difference) >= MamaTurtleShellContourDefinitions.HalfWidth)
            {
                AssertEqual((ushort)0, mama.Radius.Y,
                    $"sleeping Mama ignores out-of-contour difference {difference}");
                AssertEqual((ushort)0, mama.Properties,
                    $"sleeping Mama remains nonsolid outside contour at {difference}");
            }
            else
            {
                short offset = MamaTurtleShellContourDefinitions.GetOffset(difference);
                AssertEqual(unchecked((ushort)-offset), mama.Radius.Y,
                    $"sleeping Mama production radius at difference {difference}");
                AssertTrue((mama.Properties & 0x8000) != 0,
                    $"sleeping Mama production solidity at difference {difference}");
            }
            productionCases++;
        }

        Console.WriteLine(
            $"  Mama Turtle shell contour: 48 native words and {productionCases} " +
            "production distance cases pass without a ROM bus.");
    }

    /// <summary>Runs the native contour and production collision checks from the CLI.</summary>
    [VerificationCommand("--mama-turtle-shell-contour")]
    internal static int MamaTurtleShellContourCommand()
    {
        Suite(nameof(VerifyMamaTurtleShellContourDefinitions), () => VerifyMamaTurtleShellContourDefinitions());
        return 0;
    }
}
