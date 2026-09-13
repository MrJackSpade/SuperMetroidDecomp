using SuperMetroid.Core.Game;
using SuperMetroid.Core.Runtime;

/// <summary>Private EPJ1 snapshot of the bank-$86 pool at the controller replay boundary.</summary>
internal static class ZebetiteProjectileSeed
{
    public static void Write(SuperMetroidRuntime runtime, string path)
    {
        var pool = runtime.Enemies.EnemyProjectiles;
        if (pool.Count != 18 || pool.Count(p => p.Kind == RoomEnemyProjectileKind.MotherBrainRoomTurret) != 12 ||
            pool.Any(p => p.IsActive && p.Kind != RoomEnemyProjectileKind.MotherBrainRoomTurret))
            throw new InvalidDataException("EPJ1 seed requires the twelve initial turrets and six empty slots.");
        using var output = new BinaryWriter(File.Create(path));
        output.Write("EPJ1"u8);
        output.Write(runtime.System.RandomNumber);
        output.Write((ushort)pool.Count);
        foreach (var p in pool)
        {
            // Native array order $1997..$1BFB. Only the collision properties are
            // unpacked in C#; reconstruct those bits without changing their meaning.
            ushort properties = (ushort)(p.Damage |
                (p.DrawPriority == EnemyProjectileDrawPriority.High ? 0x1000 : 0) |
                (!p.CanDamageSamus ? 0x2000 : 0) |
                (p.PersistsOnSamusContact ? 0x4000 : 0) |
                (p.BlocksSamusProjectiles ? 0x8000 : 0));
            ushort[] words = [(ushort)p.Kind, p.GraphicsIndex, p.GeneralTimer, p.PreInstruction,
                p.XSubposition, p.XPosition, p.YSubposition, p.YPosition, p.XVelocity, p.YVelocity,
                p.Variable0, p.Variable1, p.InstructionPointer, p.SpritemapPointer, p.InstructionTimer,
                (ushort)(p.XRadius | p.YRadius << 8), properties, p.CollidedProjectileType];
            foreach (ushort word in words) output.Write(word);
        }
    }
}
