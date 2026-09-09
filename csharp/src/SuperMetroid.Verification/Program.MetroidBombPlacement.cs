using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyMetroidBombPlacement()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var room = CartridgeRoomHeader.Load(bus, 0xdae1);
        VerifyMetroidBombRuntime(bus, room);
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

    private static void VerifyMetroidBombRuntime(SuperMetroidAddressSpace bus, CartridgeRoomHeader room)
    {
        // Use the dry-floor setup from the full-alpha native capture, not the
        // former submerged fixture or a hand-positioned already-exploding bomb.
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(0, true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(room.Pointer, 0, 0);
        var level = runtime.LevelData!;
        for (int y = 0; y < level.HeightInBlocks; y++)
        for (int x = 0; x < level.WidthInBlocks; x++)
        {
            int index = level.GetBlockIndex(x, y);
            level.SetForegroundEntry(index, y >= 10 ? (ushort)0x8000 : (ushort)0);
            level.SetBehavior(index, 0);
        }
        runtime.InitializeDebugGroundedSamus(128, 100, 10);
        var samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.PoseId = SamusPoseId.MorphBallGroundRightPose;
        samus.EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs);
        samus.Health = samus.MaxHealth = 999;
        samus.XPosition = 128; samus.YPosition = 153;
        samus.Kinematics.XSubposition = samus.Kinematics.YSubposition = 0;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.SetAnimationFrameFromSpecialHandler(0, 1);
        samus.PoseHistory.PreviousPose = samus.Pose;
        samus.PoseHistory.PreviousDirectionAndMovement =
            (ushort)(((byte)SamusMovementType.MorphBallGround << 8) | (byte)SamusFacingDirection.Right);
        samus.PoseHistory.LastDifferentPose = samus.PoseHistory.LastDifferentDirectionAndMovement = 0;
        var target = runtime.Enemies.Slots[0];
        foreach (var other in runtime.Enemies.Slots.Skip(1)) other.Clear();
        target.XPosition = 128; target.YPosition = 145;
        target.XSubposition = target.YSubposition = 0;
        runtime.Controller1.Latch(0);
        var state = runtime.Enemies.MetroidStates[0]!;

        // Exact native rows 61..66, case right/travel2/single. The complete 40-case
        // trace is enforced by DebugRunner; these boundaries run in the core suite.
        (ushort X, ushort Y, MetroidAiFunction State, ushort Timer, ushort Health)[] expected =
        [
            (149, 145, MetroidAiFunction.AttachedToSamus, 0, 952),
            (151, 145, MetroidAiFunction.PowerBombEscape, 3, 952),
            (151, 147, MetroidAiFunction.PowerBombEscape, 2, 952),
            (149, 147, MetroidAiFunction.PowerBombEscape, 1, 952),
            (149, 145, MetroidAiFunction.Homing, 0, 952),
            (149, 145, MetroidAiFunction.AttachedToSamus, 0, 951),
        ];
        for (int frame = 0; frame <= 66; frame++)
        {
            ushort input = frame == 1 ? (ushort)SnesButton.X : (ushort)0;
            if (frame is >= 46 and < 54) input |= (ushort)SnesButton.Right;
            runtime.StepFrame(input);
            if (frame < 61) continue;
            AssertEqual(expected[frame - 61],
                (target.XPosition, target.YPosition, state.Function, state.EscapeTimer, samus.Health),
                $"native bomb/EnemyMain detach trajectory at frame {frame}");
            AssertEqual(0x00950000u, samus.Kinematics.XFixed, "Samus remains at native release X");
            AssertEqual(0x0099ffffu, samus.Kinematics.YFixed, "Samus stays on the floor during detach");
        }
    }
}
