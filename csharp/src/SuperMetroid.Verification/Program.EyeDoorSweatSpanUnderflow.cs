using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>
    /// Move_EnemyProjectile_Horizontally ($86:88B6) counts the rows to test as
    /// ((Y + r - 1) - ((Y - r) &amp; $FFF0)) &gt;&gt; 4 in 16 bits. The eye-door sweat has radius zero,
    /// so at a block-aligned Y the count underflows to $0FFF and the scan runs down its column
    /// to the floor: the drop "collides", keeps its X and loses its X fraction. In the 13%
    /// movie a fresh sweat drop spawns at Y $80 and holds its X for one frame.
    /// </summary>
    private static void VerifyEyeDoorSweatSpanUnderflow()
    {
        const int width = 8, height = 8;
        var blocks = new ushort[width * height];
        for (int x = 0; x < width; x++)
            blocks[6 * width + x] = 0x8000; // floor at row 6; rows 0-5 are air
        var level = new RoomLevelData(width, height, blocks, new byte[blocks.Length],
            new ushort[blocks.Length], new byte[8]);

        var drop = new RoomEnemyProjectileSlot(0)
        {
            Kind = RoomEnemyProjectileKind.EyeDoorSweat,
            XPosition = 0x0058, YPosition = 0x0020, XVelocity = 0xffc0, YVelocity = 0x0200,
        };
        const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
        MethodInfo run = typeof(RoomEnemySystem).GetMethod("RunEyeDoorSweatPreInstruction", flags)!;
        run.Invoke(null, [drop, level]);
        AssertEqual((ushort)0x0058, drop.XPosition, "the block-aligned drop keeps its X");
        AssertEqual((ushort)0x0000, drop.XSubposition, "and loses its X fraction");
        AssertEqual((ushort)0x0022, drop.YPosition, "while it still falls");

        // Off the block boundary the count is zero and the single row is air.
        run.Invoke(null, [drop, level]);
        AssertEqual((ushort)0x0057, drop.XPosition, "the next frame moves the drop left");
        AssertEqual((ushort)0xc000, drop.XSubposition, "by a quarter pixel");
        Console.WriteLine("  Eye-door sweat span: a radius-zero drop on a block boundary scans down to the floor.");
    }
}
