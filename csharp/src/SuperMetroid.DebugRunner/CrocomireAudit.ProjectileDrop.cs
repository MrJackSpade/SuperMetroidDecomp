using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class CrocomireAudit
{
    public static int RunProjectileDrop(string rom)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var room = CartridgeRoomHeader.Load(bus, RoomHeader);
        var loaded = Load(bus, room, CartridgeRoomAssets.Load(bus, room));
        loaded.Samus.Health = 1;
        var state = RequireState(loaded);
        state.FightFunction = CrocomireFightFunction.ProjectileAttack;
        state.ProjectileCounter = 0;
        state.Body.CurrentInstruction = 0xbb94;
        state.Body.InstructionTimer = 1;
        Step(loaded);
        var target = loaded.Enemies.EnemyProjectiles.Single(p => p.Kind == RoomEnemyProjectileKind.CrocomireProjectile);
        var shots = new SamusProjectileSystem();
        var shot = shots.Slots[0];
        shot.Type = SamusProjectileTypeWord.CreateBeam(0, false);
        shot.InstructionPointer = 0x9000;
        shot.InstructionTimer = 100;
        shot.XPosition = target.XPosition;
        shot.YPosition = target.YPosition;
        if (loaded.Enemies.ResolveEnemyProjectileSamusProjectileHits(bus, shots, new SamusBombProjectileSystem()) != 1)
            throw new InvalidDataException("Crocomire projectile did not enter its shot list.");
        for (int frame = 0; frame < 21; frame++)
        {
            loaded.Enemies.StepEnemyProjectiles(loaded.Level, loaded.Samus, cameraX: 1024, cameraY: 0);
            if (frame < 20 && (!target.IsActive ||
                target.InstructionPointer != 0x900b + frame / 4 * 4))
                throw new InvalidDataException($"Crocomire destruction timing diverged at frame {frame}.");
        }
        if (target.IsActive)
            throw new InvalidDataException("Crocomire's shot projectile did not complete its destruction list.");
        var drop = loaded.Enemies.EnemyProjectiles.Single(p => p.IsActive && p.Kind == RoomEnemyProjectileKind.EnemyDeathPickup);
        if (drop.EnemyHeaderPointer != BodyDefinition || drop.XPosition != shot.XPosition || drop.YPosition != shot.YPosition)
            throw new InvalidDataException("Crocomire shot drop used the wrong header or origin.");
        Console.WriteLine("Crocomire shot-list drop opcode and deletion pass.");
        return 0;
    }
}
