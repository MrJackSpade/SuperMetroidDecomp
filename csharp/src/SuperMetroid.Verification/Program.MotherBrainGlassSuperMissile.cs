using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    // #1269: In Mother Brain's first form the bank-$A0 walker marks the colliding projectile
    // and EnemyShot_MotherBrainHead ($A9:B507) bumps the glass, queues library-two $6E and
    // runs the normal damage. The next projectile pass removes the marked Super Missile and
    // its pair. The port converted the missile on the spot, leaving the pair live to hit the
    // glass again: in the 100% movie Mother Brain dropped to 2400 instead of 2700.
    private static void VerifyMotherBrainGlassSuperMissile()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var bus = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        int glassHits = 0;
        typeof(RoomEnemySystem).GetField("_incrementMotherBrainGlassRoomArgument", flags)!
            .SetValue(enemies, (Action)(() => glassHits++));
        var head = enemies.Slots[1];
        head.EnemyDefinitionPointer = 0xec3f;
        head.Definition = RoomEnemyDefinitionCatalog.Get(head.EnemyDefinitionPointer);
        head.XPosition = head.YPosition = 128;
        head.XRadius = head.YRadius = 16;
        head.SpritemapPointer = 1;
        head.Health = 3000;
        var state = new MotherBrainEnemyState(enemies.Slots[0]) { Head = head, Form = 0 };
        typeof(RoomEnemySystem).GetField("_motherBrain", flags)!.SetValue(enemies, state);
        ((List<ushort>)typeof(RoomEnemySystem).GetField("_interactiveEnemyIndexes", flags)!.GetValue(enemies)!).Add(head.NativeIndex);
        var samus = new SamusState();
        string root = Path.GetFullPath("out/workbook-investigation/crocomire-install");
        var installation = GameAssetInstaller.EnsureInstalled(root) ??
            GameAssetInstaller.Install(Path.GetFullPath("Super Metroid.smc"), root);
        var shots = new SamusProjectileSystem { FrameBindings = installation.LoadProjectiles().FrameBindings };
        var bombs = new SamusBombProjectileSystem();
        // A fired Super Missile occupies two slots at one position, as on the movie's frame:
        // the missile and its zero-radius link, with the instruction lists the port fires.
        (ushort List, ushort Radius, SamusProjectilePreInstruction Pre)[] pair =
            [(0x9f6b, 8, SamusProjectilePreInstruction.SuperMissile), (0x9f7b, 0, SamusProjectilePreInstruction.SuperMissileLink)];
        for (int slot = 0; slot < pair.Length; slot++)
        {
            var shot = shots.Slots[slot];
            shot.Type = 0x8200; shot.Damage = 300; shot.Direction = 7;
            shot.XPosition = shot.YPosition = 128; shot.XRadius = shot.YRadius = pair[slot].Radius;
            shot.InstructionPointer = pair[slot].List; shot.InstructionTimer = 1;
            shot.PreInstruction = pair[slot].Pre;
        }

        AssertEqual(1, enemies.ResolveOrdinaryProjectileHits(bus, shots, bombs, samus), "the glass accepts one projectile per pass");
        AssertEqual((ushort)2700, head.Health, "one Super Missile deals 300 to the glass");
        AssertEqual(1, glassHits, "the hit advances the glass PLM argument once");
        AssertTrue(enemies.SoundRequests.Any(request => request.SoundEffect == SoundEffectLibrary2Sounds.MotherBrainGlassHit),
            "the hit queues library-two $6E");
        AssertTrue(shots.Slots[0].PackedDirection.HasLowByteLifecycleState, "the walker marks the first slot");
        AssertTrue(!shots.Slots[1].PackedDirection.HasLowByteLifecycleState, "the pair's second slot is not marked");

        var level = CreateRoom(16, 16, new ushort[256], new byte[256]);
        shots.StepFrame(bus, level, samus, 0, 0, 0, 0, bombs, projectileProducerEnabled: false);
        head.InvincibilityTimer = head.FlashTimer = head.AiHandlerBits = 0;
        AssertEqual(0, enemies.ResolveOrdinaryProjectileHits(bus, shots, bombs, samus), "nothing of the pair is left to hit the glass");
        AssertEqual((ushort)2700, head.Health, "the glass takes no second hit");
        Console.WriteLine("Mother Brain glass Super Missile: one marked hit, $6E, and the pair removed on the next pass.");
    }
}
