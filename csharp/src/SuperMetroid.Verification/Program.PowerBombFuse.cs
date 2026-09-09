using SuperMetroid.Core.Game;

internal static partial class Program
{
    private static void VerifyPowerBombFuse()
    {
        // Expected boundary cases follow the literal C157 branches, including a
        // positive nonzero flag (native tests zero, not merely the sign bit).
        foreach (ushort flag in new ushort[] { 0, 1, 0x8000, 0xffff })
        {
            AssertEqual(new PowerBombFuseStep(0, 0x9000, false, flag == 0),
                SamusPowerBombFuse.Step(0, 0x9000, flag), "zero fuse follows native flag equality");
            AssertEqual(new PowerBombFuseStep(0xffff, 0x9000, true, false),
                SamusPowerBombFuse.Step(1, 0x9000, flag), "expiration spawns before collision consumes sentinel");
            AssertEqual(new PowerBombFuseStep(15, 0x0010, false, false),
                SamusPowerBombFuse.Step(16, 0xfff4, flag), "fast-list addition wraps within bank");
            AssertEqual(new PowerBombFuseStep(14, 0x9000, false, false),
                SamusPowerBombFuse.Step(15, 0x9000, flag), "fast threshold is tested after decrement");
            AssertEqual(new PowerBombFuseStep(0xfffe, 0x9000, false, false),
                SamusPowerBombFuse.Step(0xffff, 0x9000, flag), "fuse routine does not consume collision sentinel");
        }
        ushort timer = 60, pointer = 0x9000;
        int spawns = 0;
        for (int frame = 0; frame < 60; frame++)
        {
            PowerBombFuseStep result = SamusPowerBombFuse.Step(timer, pointer, 0x8000);
            timer = result.Timer;
            pointer = result.InstructionPointer;
            spawns += result.SpawnExplosion ? 1 : 0;
            AssertEqual(false, result.DeleteProjectile, "live fuse never requests deletion");
            AssertEqual(frame >= 44 ? 0x901c : 0x9000, pointer, "native fast-list transition frame");
            AssertEqual(frame == 59, result.SpawnExplosion, "native fuse expiration frame");
        }
        AssertEqual(1, spawns, "one expiration across complete fuse");
        Console.WriteLine("Power Bomb fuse: native boundary/flag/wrap cases and complete 60-frame fuse agree.");
    }
}
