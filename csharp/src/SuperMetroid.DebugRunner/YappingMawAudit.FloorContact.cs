using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;

internal static partial class YappingMawAudit
{
    /// <summary>Real Beta Power Bomb room, floor-mounted Maw, normal jump/contact dispatch.</summary>
    public static int RunFloorContact(string rom)
    {
        foreach (bool jump in new[] { false, true })
        foreach (bool openCeiling in new[] { false, true })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = new SuperMetroidRuntime(bus);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.VramWrites.DrainTo(runtime.Vram, bus);
            runtime.LoadCartridgeRoomForDebug(MawFloorContactData.Room, 256, 0);
            if (openCeiling)
            {
                // Separate the floor-mounted actor from this room's low roof.
                // Preserve the floor; this control is explicitly synthetic terrain.
                var level = runtime.LevelData!;
                for (int y = 0; y < 14; y++)
                for (int x = 0; x < level.WidthInBlocks; x++)
                    level.SetForegroundEntry(level.GetBlockIndex(x, y), 0);
            }
            var actor = runtime.Enemies.Slots.First(slot => slot.EnemyDefinitionPointer == DefinitionPointer);
            var maw = runtime.Enemies.YappingMawStates[actor.SlotIndex]!;
            var samus = runtime.Samus!;
            samus.Pose = SamusPoseIds.FacingRightNormalPose;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            samus.CommitPoseHistory(bus);
            samus.XPosition = maw.OriginX;
            samus.YPosition = (ushort)(maw.OriginY - samus.Kinematics.YRadius);
            samus.EquippedItems = samus.EquippedBeams = samus.InvincibilityTimer = 0;
            samus.InputLocked = false;
            bool captured = false, released = false;
            for (int frame = 0; frame < 300; frame++)
            {
                runtime.StepFrame(jump && frame >= 60 && frame < 80 ? runtime.ControllerBindings.Jump : (ushort)0);
                if (!openCeiling && (maw.DistanceToSamus >= 32 || maw.GrabCooldown != 48 ||
                    maw.Function != YappingMawAiFunction.WaitingForSamus || maw.HasGrabbedSamus))
                    throw new InvalidDataException("Low-roof fixture no longer stays inside the native safety radius.");
                if (frame % 60 == 0 || (!captured && maw.HasGrabbedSamus))
                    Console.WriteLine($"floor jump={jump} openCeiling={openCeiling} frame={frame} samus=({samus.XPosition},{samus.YPosition}) mouth=({actor.XPosition},{actor.YPosition}) state={maw.Function} cooldown={maw.GrabCooldown:X4} grabbed={maw.HasGrabbedSamus}");
                if (maw.HasGrabbedSamus)
                {
                    captured = true;
                    if (!samus.InputLocked || samus.XPosition != unchecked((ushort)(actor.XPosition + maw.HeldSamusXOffset)) ||
                        samus.YPosition != unchecked((ushort)(actor.YPosition + maw.HeldSamusYOffset)))
                        throw new InvalidDataException("Floor Maw lost held Samus position or input ownership.");
                }
                if (captured && !maw.HasGrabbedSamus)
                {
                    if (samus.InputLocked) throw new InvalidDataException("Floor Maw release left input locked.");
                    Console.WriteLine($"Floor Maw released at frame {frame}.");
                    released = true;
                    break;
                }
            }
            Console.WriteLine($"Floor Maw result jump={jump}, openCeiling={openCeiling}: captured={captured}, released={released}.");
            if (jump && openCeiling && (!captured || !released))
                throw new InvalidDataException("Floor Maw did not capture/release a natural jump from its root.");
            if (!jump && captured)
                throw new InvalidDataException("Floor Maw captured a stationary target inside the native safety radius.");
        }
        return 0;
    }
}

/// <summary>Retail floor-Maw fixture identity.</summary>
internal static class MawFloorContactData
{
    /// <summary>$8F:A37C Beta Power Bomb room, population $A1:90C7, floor-mounted Maws.</summary>
    public const ushort Room = 0xa37c;
}
