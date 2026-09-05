using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>Retail identities used by the room-$01/$2E terrain-hazard regression.</summary>
internal static class SpikeHazardAuditRomData
{
    /// <summary>Room $01/$2E header at <c>$8F:A56B</c>.</summary>
    public const ushort RoomHeader = 0xa56b;

    /// <summary>
    /// Authored level index 907 is the first BTS-one floor spike in room $01/$2E with
    /// open space above and beside it, allowing the hurt trajectory itself to be observed.
    /// </summary>
    public const int OpenFloorSpikeBlockIndex = 907;
}

/// <summary>
/// Reproduces issue #279 through the production runtime using room $01/$2E's authored
/// spike blocks. This intentionally verifies the hurt pose and subsequent displacement,
/// not merely the damage-request timers written by bank $94.
/// </summary>
internal static class SpikeHazardAudit
{
    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(
            bus,
            SpikeHazardAuditRomData.RoomHeader);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        RoomLevelData level = assets.LevelData;

        if (room.Identity != new RoomIdentity(AreaId.Brinstar, 0x2e))
        {
            throw new InvalidDataException(
                $"Spike audit selected {room.Identity}, expected room $01/$2E.");
        }

        RoomCollisionBlock spike = level.GetCollisionBlockByIndexOrPrefilledSolid(
            SpikeHazardAuditRomData.OpenFloorSpikeBlockIndex);
        if (spike.CollisionType != RoomCollisionType.SpikeBlock ||
            spike.Behavior != SamusTerrainHazardRomData.LightSpikeBlockBehavior)
        {
            throw new InvalidDataException(
                "Room $01/$2E contains no authored light spike block for the regression.");
        }

        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(SpikeHazardAuditRomData.RoomHeader);

        SamusState samus = runtime.Samus ?? throw new InvalidDataException(
            "Spike audit room load did not create Samus.");
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus, initialFrame: 0);
        int blockX = spike.Index % level.WidthInBlocks;
        int blockY = spike.Index / level.WidthInBlocks;
        samus.XPosition = unchecked((ushort)(blockX * 16 + 8));
        samus.YPosition = unchecked((ushort)(blockY * 16 - samus.Kinematics.YRadius));
        samus.Kinematics.XSubposition = 0;
        samus.Kinematics.YSubposition = 0;
        samus.InputLocked = false;
        samus.Health = 99;
        samus.InvincibilityTimer = 0;
        samus.KnockbackTimer = 0;
        samus.KnockbackDirection = 0;
        samus.KnockbackActive = false;

        ushort contactX = samus.XPosition;
        ushort contactY = samus.YPosition;
        runtime.StepFrame(controller1Input: 0);

        if (samus.Health != 99 - SamusTerrainHazardRomData.LightSpikeDamage ||
            samus.InvincibilityTimer == 0 ||
            !samus.KnockbackActive ||
            samus.Pose != SamusPoseIds.KnockbackRightPose ||
            samus.KnockbackXDirection != 0 ||
            samus.KnockbackDirection != 1)
        {
            throw new InvalidDataException(
                $"Room $01/$2E spike contact failed: block={spike.Index} " +
                $"({blockX},{blockY}) type/BTS={spike.CollisionType}/${spike.Behavior:X2}, " +
                $"health={samus.Health}, inv={samus.InvincibilityTimer}, " +
                $"timer={samus.KnockbackTimer}, active={samus.KnockbackActive}, " +
                $"pose=${samus.Pose:X2}, xdir={samus.KnockbackXDirection}, " +
                $"direction={samus.KnockbackDirection}.");
        }

        runtime.StepFrame(controller1Input: 0);
        if (samus.XPosition >= contactX || samus.YPosition >= contactY)
        {
            KnockbackMovementResult? movement = runtime.LastKnockbackMovement;
            throw new InvalidDataException(
                $"Room $01/$2E spike knockback did not move up-left: " +
                $"({contactX:X4},{contactY:X4}) -> " +
                $"({samus.XPosition:X4},{samus.YPosition:X4}); active={samus.KnockbackActive}, " +
                $"timer={samus.KnockbackTimer}, direction={samus.KnockbackDirection}, " +
                $"movement={movement}.");
        }

        ushort onceDamagedHealth = samus.Health;
        runtime.StepFrame(controller1Input: 0);
        if (samus.Health != onceDamagedHealth)
        {
            throw new InvalidDataException(
                $"Spike invincibility failed: health {onceDamagedHealth}->{samus.Health}.");
        }

        Console.WriteLine(
            $"Spike hazard audit passed: room $01/$2E block {spike.Index} " +
            $"({blockX},{blockY}) BTS ${spike.Behavior:X2} dealt 16 damage, installed " +
            "the cartridge hurt pose, moved Samus up-left, and respected invincibility.");
        return 0;
    }
}
