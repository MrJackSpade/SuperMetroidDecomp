using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Rooms;

internal static class ImmunePlasmaHitAudit
{
    internal static int Run(string installationRoot)
    {
        var installation = new GameInstallation(installationRoot);
        var bus = installation.OpenRuntimeAddressSpace();
        var game = new SuperMetroidGame(bus, gameOptions: null, renderGameplayFrames: false);
        InstalledInputReplay.Bind(game, installation);
        typeof(SuperMetroidGame).GetMethod("CreateGameplayRuntime", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(game, [false]);
        var runtime = game.RuntimeForVerification!;
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0xa253);
        var enemy = runtime.Enemies.Slots.First(e => e.EnemyDefinitionPointer == 0xd47f);
        runtime.Samus!.XPosition = enemy.XPosition;
        runtime.Samus.YPosition = (ushort)(enemy.YPosition + 80);
        runtime.Camera!.SetPosition(0, (ushort)Math.Max(0, enemy.YPosition - 100));
        runtime.StepFrame(0);
        var shot = runtime.Projectiles.Slots[0];
        shot.ClearFields();
        shot.Type = (ushort)(SamusBeamFlags.Wave | SamusBeamFlags.Plasma);
        shot.Damage = 250;
        shot.Direction = (ushort)SamusProjectileDirection.Right;
        shot.XPosition = enemy.XPosition;
        shot.YPosition = enemy.YPosition;
        shot.XRadius = shot.YRadius = 4;
        shot.InstructionPointer = 0x9000;
        shot.InstructionTimer = 1;
        runtime.Projectiles.ReflectFromEnemy(bus, 0);
        ushort health = enemy.Health;
        int hits = runtime.Enemies.ResolveOrdinaryProjectileHits(bus, runtime.Projectiles, runtime.BombProjectiles, runtime.Samus);
        Console.WriteLine($"hits={hits}, health={health}->{enemy.Health}, direction={shot.Direction:X4}, dud={runtime.Enemies.LastEnemyProjectileDudSoundEffect:X4}");
        if (hits != 1 || enemy.Health != health || (shot.Direction & 0x10) == 0 ||
            runtime.Enemies.LastEnemyProjectileDudSoundEffect != 0x3d ||
            !runtime.Enemies.RoomSpriteObjects.Any(s => s.Kind == RoomSpriteObjectKind.EnemyProjectileDud && s.XPosition == shot.XPosition && s.YPosition == shot.YPosition))
            throw new InvalidDataException("Immune Ripper must stop Wave/Plasma and emit the cartridge dud sprite/sound.");
        runtime.Projectiles.StepFrame(bus, runtime.LevelData!, runtime.Samus, 0, 0, runtime.Camera.XPosition, runtime.Camera.YPosition, runtime.BombProjectiles, projectileProducerEnabled: false);
        if (shot.HasEnemyCollisionPayload) throw new InvalidDataException("Rejected Wave/Plasma retained its gameplay collision payload.");
        Console.WriteLine("PASS: immune hit leaves enemy health unchanged, publishes dud visual/sound, and removes the beam payload on its next update.");
        // The follow-up reports the same weapon making Mother Brain easier. Her
        // private no-death callback must retain common Plasma hit invincibility.
        runtime.LoadCartridgeRoomForDebug(0xdd58);
        var brain = runtime.Enemies.MotherBrain!;
        brain.Form = 2;
        var head = brain.Head!;
        head.Health = 18000;
        shot.ClearFields();
        shot.Type = (ushort)((ushort)SamusBeamFlags.Wave | (ushort)SamusBeamFlags.Plasma | 0x10);
        shot.Damage = 900;
        shot.Direction = (ushort)SamusProjectileDirection.Right;
        shot.XPosition = head.XPosition;
        shot.YPosition = head.YPosition;
        shot.XRadius = shot.YRadius = 4;
        shot.InstructionPointer = 0x9000;
        shot.InstructionTimer = 1;
        runtime.Projectiles.ReflectFromEnemy(bus, 0);
        ushort headHealth = head.Health;
        ushort chargedDamage = shot.Damage;
        var callback = typeof(RoomEnemySystem).GetMethod("ResolveMotherBrainHeadShot", BindingFlags.Instance | BindingFlags.NonPublic)!;
        callback.Invoke(runtime.Enemies, [bus, head, shot, runtime.Projectiles, runtime.BombProjectiles, shot.PackedType, shot.Damage]);
        Console.WriteLine($"Mother Brain charged Wave/Plasma: health={headHealth}->{head.Health}, invincibility={head.InvincibilityTimer}");
        if (head.Health != headHealth - chargedDamage || head.InvincibilityTimer != 16)
            throw new InvalidDataException("Mother Brain common Plasma damage tail must grant 16 invincibility frames.");
        Console.WriteLine("PASS: Mother Brain receives one native charged hit and 16-frame Plasma immunity.");
        return 0;
    }
}
