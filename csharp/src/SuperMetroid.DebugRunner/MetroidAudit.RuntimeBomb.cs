using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;

internal static partial class MetroidAudit
{
    public static int VerifyRuntimePlacedBomb(string romPath)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var runtime = new SuperMetroidRuntime(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(0, true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(MetroidRoomHeader, 0, 0);
        var level = runtime.LevelData!;
        // Replace terrain only: a flat test floor removes slopes/room triggers while
        // retaining retail Metroid definitions and the complete runtime update order.
        // Keep the floor above the retained $00D0 acid surface. The original row-14
        // fixture accidentally tested submerged bomb jumps, whose retail launch speed
        // is only $0000:1000, and could not establish ordinary dry-floor behavior.
        for (int y = 0; y < level.HeightInBlocks; y++)
        for (int x = 0; x < level.WidthInBlocks; x++)
        {
            int index = y * level.WidthInBlocks + x;
            level.SetForegroundEntry(index, y >= 10 ? (ushort)0x8000 : (ushort)0);
            level.SetBehavior(index, 0);
        }
        runtime.InitializeDebugGroundedSamus(128, 100, 10);
        var samus = runtime.Samus!;
        samus.Pose = SamusPoseIds.MorphBallGroundRightPose;
        samus.EquippedItems |= (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs);
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.YPosition = (ushort)(160 - samus.Kinematics.YRadius);
        samus.InputLocked = false;
        var actor = runtime.Enemies.Slots[0];
        foreach (var other in runtime.Enemies.Slots.Skip(1))
            other.Properties |= (ushort)EnemyProperties.Deleted;
        actor.XPosition = samus.XPosition;
        actor.YPosition = (ushort)(samus.YPosition - 8);
        var state = RequireState(runtime.Enemies, actor);
        int attached = -1, placed = -1, detached = -1, reattached = -1;
        ushort groundY = samus.YPosition;
        ushort minimumY = groundY;
        for (int frame = 0; frame < 140; frame++)
        {
            bool fire = attached >= 0 && placed < 0;
            runtime.StepFrame(fire ? (ushort)SnesButton.X : (ushort)0);
            minimumY = Math.Min(minimumY, samus.YPosition);
            if (attached < 0 && state.Function == MetroidAiFunction.AttachedToSamus) attached = frame;
            if (fire)
            {
                if (runtime.BombProjectiles.BombCounter != 1)
                    throw new InvalidDataException("Runtime Shoot failed to place the regular bomb.");
                placed = frame;
            }
            if (placed >= 0 && detached < 0 && state.Function == MetroidAiFunction.PowerBombEscape)
                detached = frame;
            if (detached >= 0 && state.Function == MetroidAiFunction.AttachedToSamus && reattached < 0)
                reattached = frame;
        }
        if (attached < 0 || placed < 0 || detached < 0)
            throw new InvalidDataException($"Runtime bomb detachment failed: attached={attached}, placed={placed}, detached={detached}, pose={samus.Pose:X2}, Y={samus.YPosition}, minimumY={minimumY}.");
        if (detached != placed + 59 || reattached >= 0)
            throw new InvalidDataException($"Runtime detach timing changed: placed={placed}, detached={detached}, reattached={reattached}.");
        Console.WriteLine($"Runtime regular bomb: attached={attached}, placed={placed}, detached={detached}, reattached={reattached}, Samus Y={groundY}->{minimumY} (full movement enabled; no forced trajectory).");
        return 0;
    }
}
