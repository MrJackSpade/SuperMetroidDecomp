using System.Reflection;
using SuperMetroid.Core.Game;

/// <summary>Checks native immediate operands against the real projectile initializer and motion callbacks.</summary>
internal static class PhantoonFlameRegionAudit
{
    public static int Run(string rom)
    {
        int cases = 0;
        foreach (ushort parameter in new ushort[] { 0x0200, 0x020f, 0x0600, 0x0607 })
        {
            var runtime = PhantoonMaterializationAudit.CreateEncounter(rom);
            var enemies = runtime.Enemies;
            var body = enemies.Phantoon!.Body;
            body.XPosition = body.YPosition = 128;
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            bool spawned = (bool)typeof(RoomEnemySystem).GetMethod("SpawnPhantoonDestroyableFlame", flags)!
                .Invoke(enemies, [body, parameter])!;
            if (!spawned) throw new InvalidDataException("No flame slot allocated.");
            var flame = enemies.EnemyProjectiles.Single(p => p.IsActive && p.Kind == RoomEnemyProjectileKind.PhantoonDestroyableFlame);
            bool rage = parameter < 0x0600;
            ushort ReadImmediate(int opcodeAddress, byte opcode)
            {
                if (runtime.AddressSpace.ReadByte(opcodeAddress) != opcode)
                    throw new InvalidDataException("Unexpected regional operand opcode.");
                return (ushort)(runtime.AddressSpace.ReadByte(opcodeAddress + 1) |
                    runtime.AddressSpace.ReadByte(opcodeAddress + 2) << 8);
            }
            int angleDelta = rage ? (short)ReadImmediate((parameter & 8) == 0 ? 0x869885 : 0x86988d, 0xa9)
                : ReadImmediate(0x869ae8, 0x69);
            int radiusDelta = ReadImmediate(rage ? 0x869a49 : 0x869ade, 0x69);
            if (rage && flame.XVelocity != unchecked((ushort)angleDelta))
                throw new InvalidDataException($"Flame {parameter:X4}: initial angle increment {(short)flame.XVelocity} differs from ROM {angleDelta}.");
            int startAngle = flame.Variable0;
            var step = typeof(RoomEnemySystem).GetMethod(rage ? "RunPhantoonEnragedFlame" : "RunPhantoonSpiralFlame", flags)!;
            for (int frame = 1; frame <= 8; frame++)
            {
                step.Invoke(enemies, [flame]);
                int expectedAngle = (startAngle + frame * angleDelta) & 255;
                if (flame.Variable0 != expectedAngle || flame.YVelocity != frame * radiusDelta)
                    throw new InvalidDataException($"Flame {parameter:X4} frame {frame}: angle/radius {flame.Variable0}/{flame.YVelocity}, ROM {expectedAngle}/{frame * radiusDelta}.");
                cases++;
            }
        }
        Console.WriteLine($"Phantoon flame region: {cases} real motion steps match the cartridge's NTSC immediate operands.");
        return 0;
    }
}
