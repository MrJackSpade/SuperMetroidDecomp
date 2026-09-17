using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Compares the complete asymmetric shell contour with the pinned cartridge and drives
    /// the real sleeping-parent collision path with the old source range forbidden.
    /// </summary>
    private static void VerifyMamaTurtleShellContourDefinitions()
    {
        var rom = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        for (int index = 0; index < MamaTurtleShellContourDefinitions.EntryCount; index++)
        {
            int address = MamaTurtleShellContourDefinitions.SourceAddress + index * 2;
            short expected = unchecked((short)(
                rom.ReadByte(address) | rom.ReadByte(address + 1) << 8));
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

        var mama = new RoomEnemySlot(0) { XPosition = 0x0200, YPosition = 0x0200 };
        var state = new MamaTurtleEnemyState(mama) { AsleepFlag = 1 };
        var samus = new SamusState { YPosition = 0 };
        var runAsleep = typeof(RoomEnemySystem).GetMethod(
                "RunMamaTurtleAsleep", BindingFlags.Static | BindingFlags.NonPublic)!
            .CreateDelegate<Action<RoomEnemySlot, MamaTurtleEnemyState, SamusState>>();

        int productionCases = 0;
        for (short difference = -30; difference <= 30; difference++)
        {
            mama.YRadius = 0xffff;
            mama.Properties = 0;
            samus.XPosition = unchecked((ushort)(mama.XPosition - difference));
            runAsleep(mama, state, samus);

            if (Math.Abs((int)difference) >= MamaTurtleShellContourDefinitions.HalfWidth)
            {
                AssertEqual((ushort)0, mama.YRadius,
                    $"sleeping Mama ignores out-of-contour difference {difference}");
                AssertEqual((ushort)0, mama.Properties,
                    $"sleeping Mama remains nonsolid outside contour at {difference}");
            }
            else
            {
                short offset = MamaTurtleShellContourDefinitions.GetOffset(difference);
                AssertEqual(unchecked((ushort)-offset), mama.YRadius,
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
}
