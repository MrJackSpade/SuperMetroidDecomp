using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyMetroidBombPlacement()
    {
        VerifyMetroidBombRuntime();
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var room = CartridgeRoomHeader.Load(bus, 0xdae1);
        var assets = CartridgeRoomAssets.Load(bus, room);
        var enemies = new RoomEnemySystem();
        var random = new Bank80SystemState();
        enemies.Load(bus, room.State.EnemyPopulationPointer, room.State.EnemyTilesetPointer,
            new SnesVram(), new SnesCgram(), random.NextRandom, random.SetRandomNumber);
        var samus = new SamusState { Health = 999, MaxHealth = 999, XPosition = 128, YPosition = 128,
            Pose = (byte)SamusPoseId.MorphBallGroundRightPose,
            EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs) };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        var target = enemies.Slots[0];
        target.XPosition = samus.XPosition;
        target.YPosition = (ushort)(samus.YPosition - 8);
        enemies.StepFrame(0, 0, false, samus, level: assets.LevelData);
        enemies.ResolveOrdinarySamusContact(samus, 0);
        var state = enemies.MetroidStates[0]!;
        AssertEqual(MetroidAiFunction.AttachedToSamus, state.Function, "real touch attaches Metroid to morphed Samus");
        var bombs = new SamusBombProjectileSystem();
        var shots = new SamusProjectileSystem();
        int firstExplosion = -1, detached = -1;
        for (int frame = 0; frame < 120; frame++)
        {
            ushort input = frame == 0 ? (ushort)SnesButton.X : (ushort)0;
            bombs.StepFrame(bus, assets.LevelData, samus, input, input);
            if (firstExplosion < 0 && bombs.Slots.Any(b => b.Type != 0 && b.BombTimer == 0)) firstExplosion = frame;
            enemies.StepFrame(0, 0, false, samus, level: assets.LevelData);
            enemies.ResolveOrdinaryBombHits(bombs, shots, samus);
            if (state.Function == MetroidAiFunction.PowerBombEscape && detached < 0) detached = frame;
            if (detached >= 0) break;
        }
        AssertTrue(firstExplosion >= 0, "placed normal bomb reaches its natural explosion");
        AssertTrue(detached >= firstExplosion, "centered placed bomb detaches attached Metroid");
        Console.WriteLine($"Centered normal bomb: explosion frame {firstExplosion}, detached frame {detached}, escape timer {state.EscapeTimer}.");
        AssertEqual((ushort)4, state.EscapeTimer, "native bomb-detach escape interval");
        // Isolate the post-hit AI from further bomb/contact hits and check the exact
        // native jitter trajectory rather than treating a temporary detach as immunity.
        for (int frame = 0; frame < 4; frame++)
        {
            int index = state.EscapeTimer & 3;
            ushort expectedX = unchecked((ushort)(target.XPosition + SuperMetroid.Core.Rom.RomDataReader.ReadWordFixedBank(bus, 0xa3ea3f + index * 2)));
            ushort expectedY = unchecked((ushort)(target.YPosition + SuperMetroid.Core.Rom.RomDataReader.ReadWordFixedBank(bus, 0xa3ea3f + (index + 4) * 2)));
            // Move Samus away to prevent touch from immediately changing the returned
            // Homing state. The escape routine itself ignores her new position.
            samus.XPosition = 32;
            enemies.StepFrame(0, 0, false, samus, level: assets.LevelData);
            AssertEqual(expectedX, target.XPosition, "native post-bomb escape X");
            AssertEqual(expectedY, target.YPosition, "native post-bomb escape Y");
            AssertEqual(frame == 3 ? MetroidAiFunction.Homing : MetroidAiFunction.PowerBombEscape,
                state.Function, "native escape phase ends after four updates");
        }
    }

    private static void VerifyMetroidBombRuntime()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroidRuntime(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0xdae1);
        var level = runtime.LevelData!;
        // Controlled platform inside the loaded retail room: retain enemy definitions,
        // collision dispatch and the entire gameplay loop, removing only terrain noise.
        for (int y = 3; y <= 12; y++)
        for (int x = 3; x <= 12; x++)
            level.SetForegroundEntry(level.GetBlockIndex(x, y), y == 12 ? (ushort)0x8000 : (ushort)0);
        var samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.PoseId = SamusPoseId.MorphBallGroundRightPose;
        samus.EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs);
        samus.XPosition = 128;
        samus.YPosition = 184;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        foreach (var other in runtime.Enemies.Slots.Take(runtime.Enemies.EnemyCount).Skip(1))
            other.Properties = other.Properties.With(EnemyProperties.Deleted);
        for (int frame = 0; frame < 8; frame++) runtime.StepFrame(0);
        var target = runtime.Enemies.Slots[0];
        target.XPosition = samus.XPosition;
        target.YPosition = unchecked((ushort)(samus.YPosition - 8));
        runtime.StepFrame(0);
        runtime.Enemies.ResolveOrdinarySamusContact(samus, 0);
        var state = runtime.Enemies.MetroidStates[0]!;
        AssertEqual(MetroidAiFunction.AttachedToSamus, state.Function, "runtime contact attaches Metroid");
        int detached = -1, reattached = -1;
        ushort lowestY = samus.YPosition;
        for (int frame = 0; frame < 150; frame++)
        {
            runtime.StepFrame(frame == 0 ? runtime.ControllerBindings.Shoot : (ushort)0);
            if (frame == 0 || frame is >= 57 and <= 65)
                Console.WriteLine($"runtime {frame}: Samus={samus.XPosition}/{samus.YPosition} pose={samus.Pose:X2}, Metroid={target.XPosition}/{target.YPosition} {state.Function}, bombs={string.Join(';', runtime.BombProjectiles.Slots.Where(b => b.Type != 0).Select(b => $"{b.Type:X4}@{b.XPosition}/{b.YPosition} timer={b.BombTimer} radius={b.XRadius}/{b.YRadius}"))}");
            lowestY = Math.Min(lowestY, samus.YPosition);
            if (state.Function == MetroidAiFunction.PowerBombEscape && detached < 0) detached = frame;
            if (detached >= 0 && frame > detached && state.Function == MetroidAiFunction.AttachedToSamus && reattached < 0)
                reattached = frame;
        }
        AssertTrue(detached >= 0, "full runtime centered bomb detaches Metroid");
        Console.WriteLine($"Runtime normal bomb: detach={detached}, reattach={reattached}, Samus apex Y={lowestY}.");
    }
}
