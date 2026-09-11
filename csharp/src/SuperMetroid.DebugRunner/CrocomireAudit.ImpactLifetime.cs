using SuperMetroid.Core.Game;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;

internal static partial class CrocomireAudit
{
    /// <summary>Actual room artwork, shot-list lifetime, and post-deletion contact for #573.</summary>
    public static int RunImpactLifetime(string rom)
    {
        foreach (ushort weapon in new ushort[] { 0, 0x0100, 0x0200 })
        foreach (int delay in new[] { 0, 1, 4, 7 })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = new SuperMetroidRuntime(bus);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.VramWrites.DrainTo(runtime.Vram, bus);
            runtime.LoadCartridgeRoomForDebug(RoomHeader, 1024, 0);
            var samus = runtime.Samus!;
            samus.XPosition = 1040; samus.YPosition = 139;
            samus.Health = 1;
            var state = runtime.Enemies.Crocomire!;
            state.FightFunction = CrocomireFightFunction.ProjectileAttack;
            state.ProjectileCounter = 0;
            state.Body.CurrentInstruction = 0xbb94;
            state.Body.InstructionTimer = 1;
            runtime.Enemies.StepFrame(1024, 0, false, samus, level: runtime.LevelData);
            var target = runtime.Enemies.EnemyProjectiles.Single(p => p.Kind == RoomEnemyProjectileKind.CrocomireProjectile);
            for (int frame = 0; frame < delay; frame++)
                runtime.Enemies.StepEnemyProjectiles(runtime.LevelData!, samus, cameraX: 1024, cameraY: 0);
            ushort x = target.XPosition, y = target.YPosition;
            var shots = new SamusProjectileSystem();
            var shot = shots.Slots[0];
            shot.Type = weapon; shot.InstructionPointer = 0x9000;
            shot.XPosition = x; shot.YPosition = y;
            if (runtime.Enemies.ResolveEnemyProjectileSamusProjectileHits(bus, shots, new SamusBombProjectileSystem()) != 1)
                throw new InvalidDataException("Impact lifetime fixture failed to shoot down the projectile.");
            int visible = 0;
            for (int frame = 0; frame < 30; frame++)
            {
                runtime.Enemies.StepEnemyProjectiles(runtime.LevelData!, samus, cameraX: 1024, cameraY: 0);
                var actual = Draw();
                var kind = target.Kind;
                target.Kind = RoomEnemyProjectileKind.None;
                var withoutImpact = Draw();
                target.Kind = kind;
                bool contributesPixels = !actual.SequenceEqual(withoutImpact);
                if (frame < 20)
                {
                    if (!contributesPixels || !target.IsActive || target.XPosition != x || target.YPosition != y)
                        throw new InvalidDataException($"Impact art/lifetime changed at frame {frame}, weapon={weapon:X4}, delay={delay}.");
                    visible++;
                }
                else
                {
                    if (contributesPixels || target.IsActive)
                        throw new InvalidDataException("Destroyed Crocomire projectile still contributes ring pixels.");
                    samus.XPosition = x; samus.YPosition = y;
                    samus.InvincibilityTimer = 0;
                    ushort health = samus.Health;
                    runtime.Enemies.ResolveEnemyProjectileSamusHits(samus);
                    if (samus.Health < health)
                        throw new InvalidDataException("Former impact location still damages Samus after cleanup.");
                }
            }
            Console.WriteLine($"weapon={weapon:X4}, delay={delay}: {visible} rendered impact frames, then ten harmless/absent frames.");

            Rgba32[] Draw()
            {
                var oam = new OamBuffer();
                oam.BeginFrame();
                runtime.Enemies.DrawEnemyProjectiles(oam, 1024, 0);
                oam.FinalizeFrame();
                return SnesObjRenderer.Render(oam, runtime.Vram, runtime.Cgram, 0x03);
            }
        }
        return 0;
    }
}
