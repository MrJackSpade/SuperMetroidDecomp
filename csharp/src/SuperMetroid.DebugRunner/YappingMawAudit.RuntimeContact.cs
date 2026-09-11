using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class YappingMawAudit
{
    /// <summary>
    /// Exercises EnemyMain contact through the complete runtime, without invoking
    /// touch directly or moving either actor onto the other after initialization.
    /// The retail ceiling Maw targets Samus above a constructed flat floor.
    /// </summary>
    public static int RunRuntimeContact(string rom)
    {
        VerifyNativeExtensionRange(rom);
        foreach (var (jump, insideRoot) in new[] { (false, false), (true, false), (false, true) })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = new SuperMetroidRuntime(bus);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.VramWrites.DrainTo(runtime.Vram, bus);
            runtime.LoadCartridgeRoomForDebug(AuditRoomPointer, 0, 0);
            var level = runtime.LevelData!;
            int floorRow = insideRoot ? 5 : 9;
            for (int i = 0; i < level.WidthInBlocks * level.HeightInBlocks; i++)
                level.SetForegroundEntry(i, (ushort)(i / level.WidthInBlocks == floorRow
                    ? (int)RoomCollisionType.SolidBlock << 12 : 0));
            var actor = runtime.Enemies.Slots.First(slot => slot.EnemyDefinitionPointer == DefinitionPointer);
            foreach (var other in runtime.Enemies.Slots)
                if (other != actor) other.Properties |= (ushort)EnemyProperties.Deleted;
            var maw = runtime.Enemies.YappingMawStates[actor.SlotIndex]!;
            var samus = runtime.Samus!;
            samus.Pose = SamusPoseIds.FacingRightNormalPose;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            samus.CommitPoseHistory(bus);
            samus.XPosition = maw.OriginX;
            samus.YPosition = (ushort)(floorRow * 16 - samus.Kinematics.YRadius);
            samus.InputLocked = false;
            samus.EquippedItems = 0;
            samus.EquippedBeams = 0;
            samus.InvincibilityTimer = 0;
            runtime.Camera!.SetPosition(0, 0);
            bool captured = false;
            bool released = false;
            ushort initialHealth = samus.Health;
            for (int frame = 0; frame < 240; frame++)
            {
                var previousFunction = maw.Function;
                ushort previousX = samus.XPosition;
                ushort previousY = samus.YPosition;
                runtime.StepFrame(jump && frame < 20 ? runtime.ControllerBindings.Jump : (ushort)0);
                if (frame % 10 == 0 || (!captured && maw.HasGrabbedSamus))
                    Console.WriteLine($"jump={jump} insideRoot={insideRoot} frame={frame} samus=({samus.XPosition},{samus.YPosition}) mouth=({actor.XPosition},{actor.YPosition}) function={maw.Function} cooldown={maw.GrabCooldown:X4} grabbed={maw.HasGrabbedSamus} locked={samus.InputLocked}");
                if (samus.Health != initialHealth)
                    throw new InvalidDataException("Maw capture unexpectedly applied damage.");
                if (insideRoot && (maw.HasGrabbedSamus || samus.InputLocked ||
                    maw.Function != YappingMawAiFunction.WaitingForSamus || maw.GrabCooldown != 48))
                    throw new InvalidDataException("Resting inside the root bypassed the native 32-pixel safety gate.");
                if (maw.HasGrabbedSamus)
                {
                    if (!captured && frame != (jump ? 12 : 49))
                        throw new InvalidDataException("Runtime capture timing changed for the deterministic trajectory.");
                    captured = true;
                    // Parameter-zero's shared retracted list returns before the
                    // held-position writer on this transition call.
                    bool skipsPlacement = actor.Parameter2 == 0 &&
                        maw.DirectionTableByteOffset is not (4 or 12) &&
                        previousFunction == YappingMawAiFunction.ExtendingOrRetracting &&
                        maw.Function == YappingMawAiFunction.RetractedDelay;
                    ushort expectedX = skipsPlacement ? previousX : unchecked((ushort)(actor.XPosition + maw.HeldSamusXOffset));
                    ushort expectedY = skipsPlacement ? previousY : unchecked((ushort)(actor.YPosition + maw.HeldSamusYOffset));
                    if (!samus.InputLocked ||
                        samus.XPosition != expectedX || samus.YPosition != expectedY)
                        throw new InvalidDataException($"Captured Samus lost mouth ownership at {frame}: actual=({samus.XPosition},{samus.YPosition}) mouth=({actor.XPosition},{actor.YPosition}) offset=({maw.HeldSamusXOffset},{maw.HeldSamusYOffset}).");
                }
                if (captured && !maw.HasGrabbedSamus)
                {
                    if (frame != 140 || samus.InputLocked)
                        throw new InvalidDataException("Runtime Maw release timing or input restoration changed.");
                    released = true;
                    break;
                }
            }
            if (!insideRoot && (!captured || !released))
                throw new InvalidDataException($"Runtime Maw contact failed: jump={jump}, captured={captured}, released={released}.");
        }
        return 0;
    }

    private static void VerifyNativeExtensionRange(string rom)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var room = CartridgeRoomHeader.Load(bus, AuditRoomPointer);
        var assets = CartridgeRoomAssets.Load(bus, room);
        foreach (ushort distance in new ushort[] { 33, 40, 48, 63, 64, 65, 96 })
        {
            var loaded = Load(bus, room, assets);
            var maw = State(loaded);
            loaded.Samus.YPosition = (ushort)(maw.OriginY + distance);
            Step(loaded, assets, 0);
            // Native CMP #64 / BMI skips the assignment for SHORTER targets.
            // This is a maximum length, not a minimum extension distance.
            // The vertical-axis eight-bit cosine sample is 255, not 256.
            ushort expected = (ushort)Math.Min(distance * 255 >> 8, 64);
            if (maw.DistanceToSamus != expected)
                throw new InvalidDataException($"Native Maw range clamp: distance={distance}, expected={expected}, actual={maw.DistanceToSamus}.");
            Step(loaded, assets, 1);
            if (maw.SegmentRadius != expected / 2)
                throw new InvalidDataException("Maw curve radius did not consume the native clamped distance.");
        }
    }
}
