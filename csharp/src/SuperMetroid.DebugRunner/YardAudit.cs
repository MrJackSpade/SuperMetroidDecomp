using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>Retail Aqueduct regression for Yard's crawl/hide/launch state machine.</summary>
internal static class YardAudit
{
    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, 0xd5a7);
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

        const ushort yardDefinition = 0xdbbf;
        if (room.State.Pointer != 0xd5b4 || enemies.EnemyCount != 5 ||
            enemies.Slots.Take(5).Any(slot => slot.EnemyDefinitionPointer != yardDefinition) ||
            enemies.YardStates.Take(5).Any(state => state is null))
        {
            throw new InvalidDataException(
                $"Aqueduct selected state ${room.State.Pointer:X4} with " +
                $"{enemies.EnemyCount} non-uniform or uninitialized Yards.");
        }

        RoomEnemySlot yard = enemies.Slots[0];
        YardEnemyState state = enemies.YardStates[0]
            ?? throw new InvalidDataException("Aqueduct slot zero has no Yard state.");
        if (yard.XPosition != 0x0544 || yard.YPosition != 0x01d8 ||
            yard.Parameter1 != 4 || state.Direction != 0 ||
            state.IdleCrawlingSpeedIndex != 4 ||
            state.MovementFunction != YardMovementFunction.InstructionPending)
        {
            throw new InvalidDataException(
                $"First Aqueduct Yard init failed: position=({yard.XPosition},{yard.YPosition}), " +
                $"speed={yard.Parameter1}/{state.IdleCrawlingSpeedIndex}, direction={state.Direction}, " +
                $"function=$A3:{(ushort)state.MovementFunction:X4}.");
        }

        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusPoseIds.FacingRightNormalPose,
            XPosition = 0x0500,
            YPosition = 0x01d8,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);

        // Samus is left of the actor and facing right, satisfying the exact look-at test.
        // The main routine selects the hidden list before that frame's instruction pass.
        enemies.StepFrame(0x04c0, 0x0140, false, samus, level: assets.LevelData);
        if (state.Behavior != 2 ||
            state.MovementFunction != YardMovementFunction.Hiding)
        {
            throw new InvalidDataException(
                $"Yard did not hide when observed: behavior={state.Behavior}, " +
                $"function=$A3:{(ushort)state.MovementFunction:X4}.");
        }

        // Turn away. CC78 now uses real RNG to decide how many hidden animation frames to
        // retain before the list reinstalls the appropriate surface-crawling function.
        samus.Pose = SamusPoseIds.FacingLeftNormalPose;
        var maps = new HashSet<ushort>();
        for (int frame = 0; frame < 180; frame++)
        {
            enemies.StepFrame(0x04c0, 0x0140, false, samus, level: assets.LevelData);
            maps.Add(yard.SpritemapPointer);
        }
        if (state.Behavior != 0 ||
            state.MovementFunction is YardMovementFunction.InstructionPending or
                YardMovementFunction.Hiding ||
            maps.Count < 3)
        {
            throw new InvalidDataException(
                $"Yard did not leave hiding and resume animated crawling: " +
                $"behavior={state.Behavior}, function=$A3:{(ushort)state.MovementFunction:X4}, " +
                $"maps={maps.Count}.");
        }

        ushort yardCameraX = yard.XPosition > 0x0080
            ? unchecked((ushort)(yard.XPosition - 0x0080))
            : (ushort)0;
        ushort yardCameraY = yard.YPosition > 0x0070
            ? unchecked((ushort)(yard.YPosition - 0x0070))
            : (ushort)0;
        enemies.StepFrame(yardCameraX, yardCameraY, false, samus, level: assets.LevelData);
        var oam = new OamBuffer();
        oam.BeginFrame();
        enemies.DrawLayers(oam, yardCameraX, yardCameraY, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException("Aqueduct Yards emitted no live ROM OBJ.");

        // Ordinary contact while neither party has running momentum takes Yard's normal
        // 100-damage branch, rather than the kick path.
        enemies.StepFrame(0x04c0, 0x0140, false, samus, level: assets.LevelData);
        samus.XPosition = yard.XPosition;
        samus.YPosition = yard.YPosition;
        samus.Health = 999;
        samus.InvincibilityTimer = 0;
        if (!enemies.ResolveOrdinarySamusContact(samus, 0) || samus.Health != 899)
        {
            throw new InvalidDataException(
                $"Yard normal touch left Samus at {samus.Health}, expected 899.");
        }

        // A power-beam collision consumes the projectile impact but cannot damage Yard's
        // ten-health shell. Its custom shot AI instead publishes behavior five and ±1 px/f.
        samus.InvincibilityTimer = 0;
        var projectiles = new SamusProjectileSystem();
        var sharedProjectiles = new SamusBombProjectileSystem();
        SamusProjectileSlot beam = projectiles.Slots[0];
        beam.ClearFields();
        beam.Type = 0;
        beam.Damage = 20;
        beam.Direction = 2;
        beam.XPosition = yard.XPosition;
        beam.YPosition = yard.YPosition;
        beam.XRadius = 4;
        beam.YRadius = 4;
        beam.InstructionPointer = 0x9000;
        beam.InstructionTimer = 1;
        ushort healthBeforeBeam = yard.Health;
        if (enemies.ResolveOrdinaryProjectileHits(
                bus,
                projectiles,
                sharedProjectiles,
                samus) != 1 ||
            yard.Health != healthBeforeBeam || state.Behavior != 5 ||
            state.MovementFunction != YardMovementFunction.Airborne ||
            enemies.LastYardSoundEffect != 0x0070)
        {
            throw new InvalidDataException(
                $"Yard beam launch failed: health={healthBeforeBeam}->{yard.Health}, " +
                $"behavior={state.Behavior}, function=$A3:{(ushort)state.MovementFunction:X4}, " +
                $"sound={enemies.LastYardSoundEffect?.ToString("X4") ?? "none"}.");
        }

        ushort launchedX = yard.XPosition;
        for (int frame = 0; frame < 16; frame++)
            enemies.StepFrame(0x04c0, 0x0140, false, samus, level: assets.LevelData);
        if (yard.XPosition == launchedX || state.AirborneYVelocity == 0xffff)
        {
            throw new InvalidDataException(
                $"Shot Yard did not advance its horizontal/gravity arc: " +
                $"X={launchedX}->{yard.XPosition}, Y velocity=${state.AirborneYVelocity:X4}.");
        }

        // Earthquake reaction is independent of the beam launch. Select another untouched
        // retail actor and prove type $14/timer $1E enters behavior-three free fall.
        YardEnemyState quakeState = enemies.YardStates[1]
            ?? throw new InvalidDataException("Aqueduct slot one has no Yard state.");
        enemies.EarthquakeType = 0x0014;
        enemies.EarthquakeTimer = 0x001e;
        enemies.StepFrame(0x04c0, 0x0140, false, samus, level: assets.LevelData);
        if (quakeState.Behavior != 3 ||
            quakeState.MovementFunction != YardMovementFunction.Airborne)
        {
            throw new InvalidDataException(
                $"Yard earthquake drop failed: behavior={quakeState.Behavior}, " +
                $"function=$A3:{(ushort)quakeState.MovementFunction:X4}.");
        }

        Console.WriteLine(
            "Aqueduct Yard audit passed: five retail actors loaded, look-at hiding and " +
            $"random release ran, {maps.Count} maps animated, 100 contact damage resolved, " +
            "beam launch/gravity and earthquake drop advanced, and " +
            $"{oam.LastFinalizedSpriteCount} OBJ pieces rendered.");
        return 0;
    }
}
