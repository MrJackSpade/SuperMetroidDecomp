using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>
/// Isolates a retail door boundary from a long controller recording. Positions and room
/// pointers are supplied explicitly; this is a constructed scenario, not a player replay
/// or a claim of cartridge parity. Output separates IRQ travel from unlocked movement.
/// </summary>
internal static class DoorExitMomentumAudit
{
    public static int Run(string romPath, ushort source, ushort destination, ushort x, ushort y,
        ushort xSubposition = 0, ushort ySubposition = 0, ushort equippedItems = 0,
        string? seedPrefix = null)
    {
        foreach (bool tubeBroken in new[] { false, true })
        foreach (bool carriedSpeed in new[] { false, true })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
            var runtime = new SuperMetroidRuntime(bus, playerInvincibilityEnabled: true);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            // The tube room selects different FX/level data after this event. Omitting
            // it would silently turn the reported underwater case into a dry-room test.
            if (tubeBroken) runtime.System.SetEvent(EventNumber.MaridiaNoobTubeBroken);
            runtime.LoadCartridgeRoomForDebug(source, (ushort)(x & 0xff00), (ushort)(y & 0xff00));
            var samus = runtime.Samus!;
            var level = runtime.LevelData!;
            CartridgeDoorHeader? door = null;
            foreach (var index in Enumerable.Range(0, level.ForegroundEntries.Length))
            {
                var block = level.GetCollisionBlockByIndex(index);
                if (block.CollisionType != RoomCollisionType.DoorBlock) continue;
                var candidate = level.ResolveDoorCollision(bus, block.Behavior, samus.Pose, false);
                if (candidate.DestinationRoomPointer != destination) continue;
                door = level.ResolveDoorCollision(bus, block.Behavior, samus.Pose, true);
                break;
            }
            if (door is null || (door.Orientation & 2) != 0)
                throw new InvalidDataException("The requested source needs a horizontal door to the destination.");
            bool left = (door.Orientation & 1) != 0;
            samus.Pose = left ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose;
            if (left) samus.ApplyStandingLeftToRunningLeft(bus);
            else samus.ApplyStandingRightToRunningRight(bus);
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            samus.Kinematics.SetXFixed(((uint)x << 16) | xSubposition);
            samus.Kinematics.SetYFixed(((uint)y << 16) | ySubposition);
            samus.EquippedItems = equippedItems;
            samus.HorizontalSpeed.BaseSpeed = carriedSpeed ? (ushort)2 : (ushort)0;
            samus.HorizontalSpeed.BaseSubspeed = carriedSpeed ? (ushort)0xc000 : (ushort)0;
            var transition = new DoorTransitionState();
            var audio = new CartridgeAudioState();
            transition.Begin(runtime);
            Console.WriteLine($"CASE source={source:X4} destination={destination:X4} door={door.Pointer:X4} tube-broken={tubeBroken} carried-speed={carriedSpeed}");
            int frame = 0;
            for (; transition.IsActive && frame < 300; frame++)
            {
                uint previousX = samus.Kinematics.XFixed;
                var before = transition.Phase;
                transition.Step(runtime, audio, 0);
                Trace(before.ToString(), previousX);
            }
            if (transition.IsActive) throw new InvalidDataException("Door coroutine did not finish within 300 calls.");
            if (seedPrefix is not null)
                RoomMovementSeedExporter.Write(runtime,
                    $"{seedPrefix}-{tubeBroken}-{carriedSpeed}.movement-seed");
            for (int tick = 0; tick < 20; tick++, frame++)
            {
                uint previousX = samus.Kinematics.XFixed;
                runtime.StepFrame(0);
                runtime.RunNmi(0, true);
                Trace("Gameplay", previousX);
            }
            void Trace(string phase, uint previousX) => Console.WriteLine(
                $"frame={frame} phase={phase} x={samus.Kinematics.XFixed:X8} dx={unchecked((int)(samus.Kinematics.XFixed - previousX)) / 65536.0:F4} " +
                $"y={samus.Kinematics.YFixed:X8} base={samus.HorizontalSpeed.BaseFixed:X8} " +
                $"extra={samus.HorizontalSpeed.ExtraRunSpeed:X4}.{samus.HorizontalSpeed.ExtraRunSubspeed:X4} mode={samus.HorizontalSpeed.AccelerationMode} " +
                $"medium={samus.LiquidPhysics.DetermineMovementMedium(samus)} fx={samus.LiquidPhysics.FxType} surface={samus.LiquidPhysics.FxYPosition:X4} pose={samus.Pose:X2} locked={samus.InputLocked}");
        }
        return 0;
    }
}
