using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Retail Terminator-room regression for Waver flight, spin bytecode, collision, combat,
/// and OBJ rendering. The room contains only Wavers and already-translated Zoomers, so an
/// unrelated placeholder actor cannot make this family appear healthy.
/// </summary>
internal static class WaverAudit
{
    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, 0x990d);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);

        var random = new Bank80SystemState();
        var enemies = new RoomEnemySystem();
        enemies.Load(
            bus,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            random.NextRandom,
            random.SetRandomNumber);

        const ushort waverDefinition = 0xd63f;
        RoomEnemySlot[] wavers = enemies.Slots
            .Take(enemies.EnemyCount)
            .Where(slot => slot.EnemyDefinitionPointer == waverDefinition)
            .ToArray();
        if (room.State.Pointer != 0x991a || enemies.EnemyCount != 9 || wavers.Length != 3 ||
            wavers.Any(slot => enemies.WaverStates[slot.SlotIndex] is null))
        {
            throw new InvalidDataException(
                $"Terminator selected state ${room.State.Pointer:X4} with " +
                $"{enemies.EnemyCount} enemies and {wavers.Length} initialized Wavers.");
        }

        RoomEnemySlot audited = wavers[0];
        WaverEnemyState state = enemies.WaverStates[audited.SlotIndex]
            ?? throw new InvalidDataException("Terminator Waver has no typed state.");
        if (audited.XPosition != 0x0316 || audited.YPosition != 0x015c ||
            audited.Parameter1 != 1 || state.XVelocity != 1 ||
            state.XSubvelocity != 0x8000 || state.CurrentInstructionListIndex != 1 ||
            audited.CurrentInstruction != 0x86ad)
        {
            throw new InvalidDataException(
                $"First Terminator Waver init failed: position=({audited.XPosition:X4}," +
                $"{audited.YPosition:X4}), parameter={audited.Parameter1}, " +
                $"velocity={state.XVelocity}:{state.XSubvelocity:X4}, " +
                $"list={state.CurrentInstructionListIndex}/${audited.CurrentInstruction:X4}.");
        }

        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusState.FacingRightNormalPose,
            XPosition = 0x0300,
            YPosition = 0x0160,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);

        ushort startX = audited.XPosition;
        ushort startY = audited.YPosition;
        short initialVelocity = state.XVelocity;
        ushort minimumY = startY;
        ushort maximumY = startY;
        bool sawSpinList = false;
        bool sawSteadyAfterSpin = false;
        bool sawWallReversal = false;
        var maps = new HashSet<ushort>();

        // Following the actor with the camera keeps the same retail slot active while it
        // traverses real Terminator collision. Nine hundred frames covers multiple sine
        // periods and reaches a wall from this population coordinate at 1.5 px/frame.
        for (int frame = 0; frame < 900; frame++)
        {
            ushort cameraX = audited.XPosition > 0x0080
                ? unchecked((ushort)(audited.XPosition - 0x0080))
                : (ushort)0;
            ushort cameraY = audited.YPosition > 0x0070
                ? unchecked((ushort)(audited.YPosition - 0x0070))
                : (ushort)0;
            enemies.StepFrame(cameraX, cameraY, false, samus, level: assets.LevelData);
            maps.Add(audited.SpritemapPointer);
            minimumY = Math.Min(minimumY, audited.YPosition);
            maximumY = Math.Max(maximumY, audited.YPosition);
            sawWallReversal |= Math.Sign(state.XVelocity) != Math.Sign(initialVelocity);
            if (state.CurrentInstructionListIndex >= 2)
                sawSpinList = true;
            else if (sawSpinList)
                sawSteadyAfterSpin = true;
        }

        if (audited.XPosition == startX || minimumY == maximumY || maps.Count < 5 ||
            !sawSpinList || !sawSteadyAfterSpin || !sawWallReversal)
        {
            throw new InvalidDataException(
                $"Waver flight/animation failed: X={startX:X4}->{audited.XPosition:X4}, " +
                $"Y range={minimumY:X4}-{maximumY:X4}, maps={maps.Count}, " +
                $"spin={sawSpinList}/{sawSteadyAfterSpin}, reversal={sawWallReversal}.");
        }

        ushort cameraAtWaverX = audited.XPosition > 0x0080
            ? unchecked((ushort)(audited.XPosition - 0x0080))
            : (ushort)0;
        ushort cameraAtWaverY = audited.YPosition > 0x0070
            ? unchecked((ushort)(audited.YPosition - 0x0070))
            : (ushort)0;
        enemies.StepFrame(
            cameraAtWaverX,
            cameraAtWaverY,
            false,
            samus,
            level: assets.LevelData);

        var oam = new OamBuffer();
        oam.BeginFrame();
        enemies.DrawLayers(oam, cameraAtWaverX, cameraAtWaverY, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException("Terminator Wavers emitted no live ROM OBJ.");

        // A normal overlap uses the header's ten-point contact damage and the common Samus
        // knockback path; Waver itself has no bespoke touch handler.
        samus.XPosition = audited.XPosition;
        samus.YPosition = audited.YPosition;
        samus.Health = 999;
        samus.InvincibilityTimer = 0;
        if (!enemies.ResolveOrdinarySamusContact(samus, 0) || samus.Health != 989)
        {
            throw new InvalidDataException(
                $"Waver contact left Samus at {samus.Health}, expected 989.");
        }

        // Preserve the incoming projectile's type and damage through its explosion
        // conversion: a 20-damage power beam must leave this 30-health Waver at ten.
        var beamProjectiles = new SamusProjectileSystem();
        var sharedProjectiles = new SamusBombProjectileSystem();
        ArmProjectile(beamProjectiles.Slots[0], audited, damage: 20);
        if (enemies.ResolveOrdinaryProjectileHits(
                bus,
                beamProjectiles,
                sharedProjectiles,
                samus) != 1 || audited.Health != 10)
        {
            throw new InvalidDataException(
                $"Waver power-beam damage left health {audited.Health}, expected 10.");
        }

        // A second, independently armed high-damage beam proves the shared death path
        // deletes the actor and increments the room kill counter exactly once.
        var lethalProjectiles = new SamusProjectileSystem();
        ArmProjectile(lethalProjectiles.Slots[0], audited, damage: 1000);
        if (enemies.ResolveOrdinaryProjectileHits(
                bus,
                lethalProjectiles,
                sharedProjectiles,
                samus) != 1 || audited.Health != 0 ||
            !audited.Properties.HasAny(EnemyProperties.Deleted) || enemies.EnemiesKilled != 1)
        {
            throw new InvalidDataException(
                $"Waver death failed: health={audited.Health}, " +
                $"properties=${(ushort)audited.Properties:X4}, killed={enemies.EnemiesKilled}.");
        }

        Console.WriteLine(
            "Terminator Waver audit passed: three retail actors loaded, signed 16.16 " +
            $"flight crossed Y ${minimumY:X4}-${maximumY:X4}, wall reversal and " +
            $"spin/steady bytecode produced {maps.Count} maps, contact dealt ten damage, " +
            $"beam damage/death resolved, and {oam.LastFinalizedSpriteCount} OBJ pieces rendered.");
        return 0;
    }

    private static void ArmProjectile(
        SamusProjectileSlot projectile,
        RoomEnemySlot target,
        ushort damage)
    {
        projectile.ClearFields();
        projectile.Type = 0;
        projectile.Damage = damage;
        projectile.Direction = 2;
        projectile.XPosition = target.XPosition;
        projectile.YPosition = target.YPosition;
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }
}
