using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>
/// ROM-backed end-to-end regression for the complete Crateria Power Bombs population. It
/// covers Alcoon's initialization floor simulation, all movement phases, animation-owned
/// walking, three-shot volley, bank-$86 physics/rendering, and shared damage handlers.
/// </summary>
internal static class AlcoonAudit
{
    private const ushort AlcoonDefinition = 0xe9bf;
    private const ushort CrateriaPowerBombsRoom = 0x93aa;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, CrateriaPowerBombsRoom);
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
            random.SetRandomNumber,
            readRandomNumber: () => random.RandomNumber,
            level: assets.LevelData);

        RoomEnemySlot[] population = enemies.Slots.Take(enemies.EnemyCount).ToArray();
        if (room.State.Pointer != 0x93b7 ||
            room.State.EnemyPopulationPointer != 0x80d5 ||
            room.State.EnemyTilesetPointer != 0x801f ||
            enemies.EnemyCount != 3 || enemies.DeathQuota != 3 ||
            population.Any(slot => slot.EnemyDefinitionPointer != AlcoonDefinition) ||
            population.Any(slot => enemies.AlcoonStates[slot.SlotIndex] is null))
        {
            throw new InvalidDataException(
                $"Crateria Power Bombs load failed: state=${room.State.Pointer:X4}, " +
                $"population=${room.State.EnemyPopulationPointer:X4}, set=" +
                $"${room.State.EnemyTilesetPointer:X4}, count={enemies.EnemyCount}, " +
                $"quota={enemies.DeathQuota}, Alcoon=" +
                $"{population.Count(slot => slot.EnemyDefinitionPointer == AlcoonDefinition)}.");
        }

        ushort[] expectedX = [0x0088, 0x0108, 0x0178];
        for (int index = 0; index < population.Length; index++)
        {
            RoomEnemySlot slot = population[index];
            AlcoonEnemyState state = RequireState(enemies, slot);
            if (slot.XPosition != expectedX[index] || slot.YPosition != 0x00d8 ||
                slot.XSubposition != 0 || slot.Properties != 0x2800 ||
                slot.Parameter1 != 0 || slot.Parameter2 != 0 ||
                slot.Health != 200 || slot.Definition.Damage != 50 ||
                slot.XRadius != 8 || slot.YRadius != 24 || slot.Layer != 5 ||
                slot.CurrentInstruction != 0xdbe7 ||
                state.Function != AlcoonEnemyFunction.WaitingForSamus ||
                slot.YSubposition != 0xffff ||
                state.SpawnXPosition != expectedX[index] || state.SpawnYPosition != 0x00d8 ||
                state.LandingYPosition != 0x0088 ||
                state.YAcceleration != 0 || state.YSubacceleration != 0x8000 ||
                state.ClearedVariable != 0)
            {
                throw new InvalidDataException(
                    $"Alcoon {index} initialization failed: position=({slot.XPosition:X4}," +
                    $"{slot.YPosition:X4}:{slot.YSubposition:X4}), landing=" +
                    $"${state.LandingYPosition:X4}, properties=${slot.Properties:X4}, " +
                    $"health/damage={slot.Health}/{slot.Definition.Damage}, radii=" +
                    $"{slot.XRadius}/{slot.YRadius}, layer={slot.Layer}, list=" +
                    $"${slot.CurrentInstruction:X4}, function={state.Function}, acceleration=" +
                    $"{state.YAcceleration:X4}:{state.YSubacceleration:X4}.");
            }
        }

        RoomEnemySlot actor = population[0];
        AlcoonEnemyState actorState = RequireState(enemies, actor);
        SamusState samus = CreateSamus(bus, actor.XPosition + 1, actorState.LandingYPosition);
        var actorMaps = new HashSet<ushort>();
        var projectileMaps = new HashSet<ushort>();
        var projectileYVelocities = new HashSet<ushort>();
        bool sawEmergingRise = false;
        bool sawEmergingFall = false;
        bool sawWalking = false;
        bool sawFireAnimation = false;
        bool sawEmergeSound = false;
        bool sawLeftMovingFireball = false;
        bool sawRightMovingFireball = false;
        int fireSoundCount = 0;
        ushort minimumY = actor.YPosition;
        ushort maximumX = actor.XPosition;

        for (int frame = 0; frame < 2_048 &&
             (fireSoundCount < 3 || actorMaps.Count < 13 || projectileMaps.Count < 4 ||
              !sawLeftMovingFireball || !sawRightMovingFireball); frame++)
        {
            StepCentered(enemies, assets, room, samus, actor);
            // Walk Samus around the caged actor after its own frame so each natural wall
            // reversal eventually gets a facing-matched volley. No enemy or projectile
            // state is authored here; this is ordinary player positioning between frames.
            samus.XPosition = unchecked((ushort)(actor.XPosition +
                (unchecked((short)actorState.XVelocity) < 0 ? -32 : 32)));
            samus.YPosition = actorState.LandingYPosition;
            actorMaps.Add(actor.SpritemapPointer);
            minimumY = Math.Min(minimumY, actor.YPosition);
            maximumX = Math.Max(maximumX, actor.XPosition);
            sawEmergingRise |= actorState.Function == AlcoonEnemyFunction.EmergingRising;
            sawEmergingFall |= actorState.Function == AlcoonEnemyFunction.EmergingFalling;
            sawWalking |= actorState.Function == AlcoonEnemyFunction.WalkingAndFiring;
            sawFireAnimation |= actorState.Function == AlcoonEnemyFunction.WaitingForFireAnimation;
            sawEmergeSound |= enemies.LastAlcoonSoundEffect == 0x005e;
            if (enemies.LastAlcoonSoundEffect == 0x003f)
                fireSoundCount++;

            foreach (RoomEnemyProjectileSlot projectile in enemies.EnemyProjectiles.Where(
                         projectile => projectile.Kind == RoomEnemyProjectileKind.AlcoonFireball))
            {
                projectileYVelocities.Add(projectile.YVelocity);
                sawLeftMovingFireball |= unchecked((short)projectile.XVelocity) < 0;
                sawRightMovingFireball |= unchecked((short)projectile.XVelocity) > 0;
                if (projectile.SpritemapPointer != 0)
                    projectileMaps.Add(projectile.SpritemapPointer);
            }
            enemies.StepEnemyProjectiles(
                assets.LevelData,
                samus: null,
                cameraX: 0,
                cameraY: 0);
            foreach (RoomEnemyProjectileSlot projectile in enemies.EnemyProjectiles.Where(
                         projectile => projectile.Kind == RoomEnemyProjectileKind.AlcoonFireball))
            {
                if (projectile.SpritemapPointer != 0)
                    projectileMaps.Add(projectile.SpritemapPointer);
            }
        }

        ushort[] expectedProjectileYVelocities = [0xff00, 0x0000, 0x0100];
        if (!sawEmergingRise || !sawEmergingFall || !sawWalking || !sawFireAnimation ||
            !sawEmergeSound || fireSoundCount < 3 || minimumY != 0x0042 ||
            maximumX != 0x00b8 || actorMaps.Count != 13 ||
            projectileMaps.Count != 4 ||
            !sawLeftMovingFireball || !sawRightMovingFireball ||
            !expectedProjectileYVelocities.All(projectileYVelocities.Contains) ||
            random.RandomNumber != 0x0061)
        {
            throw new InvalidDataException(
                $"Alcoon natural cycle failed: functions=" +
                $"{sawEmergingRise}/{sawEmergingFall}/{sawWalking}/{sawFireAnimation}, sounds=" +
                $"{sawEmergeSound}/{fireSoundCount}, Y=${minimumY:X4}, Xmax=${maximumX:X4}, " +
                $"position=({actor.XPosition:X4},{actor.YPosition:X4}), maps=" +
                $"{actorMaps.Count}/{projectileMaps.Count}, projectileY=" +
                $"{string.Join(',', projectileYVelocities.Select(value => $"${value:X4}"))}, " +
                $"projectileDirections={sawLeftMovingFireball}/{sawRightMovingFireball}, " +
                $"random=${random.RandomNumber:X4}.");
        }

        VerifyNaturalHideCycle(bus);
        VerifyProjectileDamageAndDrag(bus, room, assets);
        VerifyOrdinaryCombat(bus, room, assets, enemies, population);
        VerifyProductionRuntime(bus);

        var oam = new OamBuffer();
        oam.BeginFrame();
        enemies.DrawLayers(oam, 0, 0, 0, 7);
        enemies.DrawEnemyProjectiles(oam, 0, 0);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException("Crateria Power Bombs emitted no ROM-authored enemy OBJ.");

        Console.WriteLine(
            $"Alcoon audit passed: unchanged Crateria Power Bombs loaded three actors; " +
            $"the initializer found floors at {string.Join(',', population.Select(slot => $"${RequireState(enemies, slot).LandingYPosition:X4}"))}; " +
            $"all seven AI states, thirteen actor maps, four fireball maps, three velocity " +
            $"variants in both directions, horizontal drag, wall deletion, production " +
            $"runtime scheduling, 20-damage fireball contact, " +
            $"50-damage body contact, nonlethal/lethal shot behavior, and " +
            $"{oam.LastFinalizedSpriteCount} OBJ pieces were verified.");
        return 0;
    }

    private static void VerifyProductionRuntime(SuperMetroidAddressSpace bus)
    {
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(CrateriaPowerBombsRoom);

        SamusState samus = runtime.Samus ?? throw new InvalidDataException(
            "Production Crateria Power Bombs load omitted Samus.");
        RoomEnemySlot actor = runtime.Enemies.Slots[0];
        AlcoonEnemyState state = RequireState(runtime.Enemies, actor);
        samus.Pose = SamusState.FacingRightNormalPose;
        samus.XPosition = unchecked((ushort)(actor.XPosition + 1));
        samus.YPosition = state.LandingYPosition;
        samus.InputLocked = true;
        samus.Health = 999;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);

        bool sawAirborneActor = false;
        bool sawFireball = false;
        for (int frame = 0; frame < 512 && !sawFireball; frame++)
        {
            runtime.StepFrame(0);
            sawAirborneActor |= state.Function is
                AlcoonEnemyFunction.EmergingRising or AlcoonEnemyFunction.EmergingFalling;
            sawFireball |= runtime.Enemies.EnemyProjectiles.Any(projectile =>
                projectile.Kind == RoomEnemyProjectileKind.AlcoonFireball);
        }
        if (!sawAirborneActor || !sawFireball)
        {
            throw new InvalidDataException(
                $"Production Alcoon scheduling failed: airborne={sawAirborneActor}, " +
                $"fireball={sawFireball}, function={state.Function}.");
        }
    }

    private static void VerifyNaturalHideCycle(SuperMetroidAddressSpace bus)
    {
        // Crateria Power Bombs deliberately pens each Alcoon between close walls, so its
        // shipped actors exercise both walking facings but never reach the 112-pixel hide
        // threshold. Lower Norfair Spring Ball Maze is the all-Alcoon retail population
        // whose longer ledges exercise that remaining state transition without fixtures.
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, 0xb510);
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
            random.SetRandomNumber,
            readRandomNumber: () => random.RandomNumber,
            level: assets.LevelData);
        RoomEnemySlot[] population = enemies.Slots.Take(enemies.EnemyCount).ToArray();
        if (room.State.Pointer != 0xb51d || enemies.EnemyCount != 5 ||
            population.Any(slot => slot.EnemyDefinitionPointer != AlcoonDefinition))
        {
            throw new InvalidDataException(
                $"LN Spring Ball Maze did not load its five-Alcoon population: " +
                $"state=${room.State.Pointer:X4}, count={enemies.EnemyCount}.");
        }

        bool sawHidingRise = false;
        bool sawHidingFall = false;
        bool returnedToWaiting = false;
        foreach (RoomEnemySlot candidate in population)
        {
            AlcoonEnemyState candidateState = RequireState(enemies, candidate);
            SamusState samus = CreateSamus(
                bus,
                candidate.XPosition + 1,
                candidateState.LandingYPosition);
            bool candidateHid = false;
            for (int frame = 0; frame < 4_096 && !returnedToWaiting; frame++)
            {
                StepCentered(enemies, assets, room, samus, candidate);
                sawHidingRise |= candidateState.Function == AlcoonEnemyFunction.HidingRising;
                sawHidingFall |= candidateState.Function == AlcoonEnemyFunction.HidingFalling;
                candidateHid |= sawHidingRise || sawHidingFall;
                returnedToWaiting = candidateHid &&
                    candidateState.Function == AlcoonEnemyFunction.WaitingForSamus &&
                    candidate.XPosition == candidateState.SpawnXPosition &&
                    candidate.YPosition == candidateState.SpawnYPosition;
                enemies.StepEnemyProjectiles(
                    assets.LevelData,
                    samus: null,
                    cameraX: 0,
                    cameraY: 0);
            }
            if (returnedToWaiting)
                break;
        }

        if (!sawHidingRise || !sawHidingFall || !returnedToWaiting)
        {
            throw new InvalidDataException(
                $"Alcoon retail hide cycle failed: rise={sawHidingRise}, " +
                $"fall={sawHidingFall}, reset={returnedToWaiting}.");
        }
    }

    private static void VerifyProjectileDamageAndDrag(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        // Use a fresh room system so a projectile surviving the two-facing animation pass
        // cannot be mistaken for this deliberately rightward shot.
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
            random.SetRandomNumber,
            readRandomNumber: () => random.RandomNumber,
            level: assets.LevelData);
        RoomEnemySlot actor = enemies.Slots[0];
        AlcoonEnemyState state = RequireState(enemies, actor);
        SamusState producerSamus = CreateSamus(bus, actor.XPosition + 1, state.LandingYPosition);

        // Wait only until the first horizontal shot exists. This is still a natural producer
        // path: player positioning follows the actor, but no instruction pointer, function,
        // velocity, or projectile field is written by the audit.
        RoomEnemyProjectileSlot? shot = null;
        for (int frame = 0; frame < 512 && shot is null; frame++)
        {
            StepCentered(enemies, assets, room, producerSamus, actor);
            producerSamus.XPosition = unchecked((ushort)(actor.XPosition + 32));
            producerSamus.YPosition = state.LandingYPosition;
            shot = enemies.EnemyProjectiles.FirstOrDefault(projectile =>
                projectile.Kind == RoomEnemyProjectileKind.AlcoonFireball);
        }
        if (shot is null)
            throw new InvalidDataException("A second natural Alcoon cycle spawned no fireball.");

        ushort initialSpeed = shot.XVelocity;
        ushort initialX = shot.XPosition;
        enemies.StepEnemyProjectiles(assets.LevelData, samus: null, cameraX: 0, cameraY: 0);
        if (!shot.IsActive || shot.XPosition <= initialX || shot.XVelocity >= initialSpeed ||
            shot.XVelocity < 0x0200)
        {
            throw new InvalidDataException(
                $"Alcoon fireball rightward drag failed: X=${initialX:X4}->${shot.XPosition:X4}, " +
                $"speed=${initialSpeed:X4}->${shot.XVelocity:X4}.");
        }

        SamusState target = CreateSamus(bus, shot.XPosition, shot.YPosition);
        target.Health = 999;
        enemies.StepEnemyProjectiles(assets.LevelData, target, controllerInput: 0);
        if (target.Health != 979 || !target.KnockbackActive || shot.IsActive)
        {
            throw new InvalidDataException(
                $"Alcoon fireball contact failed: health={target.Health}, " +
                $"knockback={target.KnockbackActive}, live={shot.IsActive}.");
        }

        // Let the remaining naturally spawned volley actors run into collision geometry.
        // Definition $9E90 deletes immediately on either vertical or horizontal carry.
        for (int frame = 0; frame < 512 && enemies.EnemyProjectiles.Any(
                 projectile => projectile.Kind == RoomEnemyProjectileKind.AlcoonFireball); frame++)
        {
            enemies.StepEnemyProjectiles(assets.LevelData, samus: null, cameraX: 0, cameraY: 0);
        }
        if (enemies.EnemyProjectiles.Any(
                projectile => projectile.Kind == RoomEnemyProjectileKind.AlcoonFireball))
        {
            throw new InvalidDataException("Alcoon fireball collision did not delete the volley.");
        }
    }

    private static void VerifyOrdinaryCombat(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        RoomEnemySystem enemies,
        RoomEnemySlot[] population)
    {
        RoomEnemySlot touchTarget = population[0];
        SamusState samus = CreateSamus(bus, touchTarget.XPosition, touchTarget.YPosition);
        samus.Health = 999;
        StepCentered(enemies, assets, room, samus, touchTarget);
        samus.XPosition = touchTarget.XPosition;
        samus.YPosition = touchTarget.YPosition;
        samus.InvincibilityTimer = 0;
        if (!enemies.ResolveOrdinarySamusContact(samus, 0) || samus.Health != 949 ||
            !samus.KnockbackActive)
        {
            throw new InvalidDataException(
                $"Alcoon common touch failed: health={samus.Health}, " +
                $"knockback={samus.KnockbackActive}.");
        }

        RoomEnemySlot beamTarget = population[1];
        StepCentered(enemies, assets, room, samus, beamTarget);
        var projectiles = new SamusProjectileSystem();
        var shared = new SamusBombProjectileSystem();
        ArmProjectile(projectiles.Slots[0], beamTarget, projectileType: 0x0002, damage: 20);
        if (enemies.ResolveOrdinaryProjectileHits(bus, projectiles, shared, samus) != 1 ||
            beamTarget.FrozenTimer != 0 || beamTarget.Health != 180 ||
            beamTarget.FlashTimer == 0)
        {
            throw new InvalidDataException(
                $"Alcoon ice-beam multiplier failed: frozen={beamTarget.FrozenTimer}, " +
                $"health={beamTarget.Health}, flash={beamTarget.FlashTimer}.");
        }

        RoomEnemySlot lethalTarget = population[2];
        StepCentered(enemies, assets, room, samus, lethalTarget);
        projectiles = new SamusProjectileSystem();
        shared = new SamusBombProjectileSystem();
        ArmProjectile(projectiles.Slots[0], lethalTarget, projectileType: 0x0200, damage: 1000);
        if (enemies.ResolveOrdinaryProjectileHits(bus, projectiles, shared, samus) != 1 ||
            lethalTarget.Health != 0 ||
            !lethalTarget.Properties.HasAny(EnemyProperties.Deleted))
        {
            throw new InvalidDataException(
                $"Alcoon lethal shot failed: health={lethalTarget.Health}, " +
                $"properties=${lethalTarget.Properties:X4}.");
        }
    }

    private static void StepCentered(
        RoomEnemySystem enemies,
        CartridgeRoomAssets assets,
        CartridgeRoomHeader room,
        SamusState samus,
        RoomEnemySlot target)
    {
        int maximumX = Math.Max(0, room.WidthInScreens * 256 - 256);
        ushort cameraX = unchecked((ushort)Math.Clamp(target.XPosition - 128, 0, maximumX));
        enemies.StepFrame(cameraX, 0, false, samus, level: assets.LevelData);
    }

    private static SamusState CreateSamus(
        SuperMetroidAddressSpace bus,
        int xPosition,
        int yPosition)
    {
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusState.FacingRightNormalPose,
            XPosition = unchecked((ushort)xPosition),
            YPosition = unchecked((ushort)yPosition),
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        return samus;
    }

    private static void ArmProjectile(
        SamusProjectileSlot projectile,
        RoomEnemySlot target,
        ushort projectileType,
        ushort damage)
    {
        projectile.ClearFields();
        projectile.Type = projectileType;
        projectile.Damage = damage;
        projectile.Direction = 2;
        projectile.XPosition = target.XPosition;
        projectile.YPosition = target.YPosition;
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    private static AlcoonEnemyState RequireState(
        RoomEnemySystem enemies,
        RoomEnemySlot slot) =>
        enemies.AlcoonStates[slot.SlotIndex] ?? throw new InvalidDataException(
            $"Alcoon slot {slot.SlotIndex} has no typed state.");
}
