using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>
/// End-to-end regression for Sponge Bath's single retail Bull. A one-actor population makes
/// every observed movement, animation, collision, grapple, and projectile reaction attributable
/// to Bull while still retaining the room header, enemy header, parameters, graphics, palette,
/// instruction lists, vulnerabilities, and level data from the user's unchanged cartridge.
/// </summary>
internal static class BullAudit
{
    private const ushort SpongeBathRoom = 0xcd5c;
    private const ushort BullDefinition = 0xe97f;

    private static readonly ushort[] ShotAngles =
    [
        0x00c0, 0x00e0, 0x0000, 0x0020, 0x0040,
        0x0040, 0x0060, 0x0080, 0x00a0, 0x00c0,
    ];

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        // Bull exists only in Sponge Bath's post-Phantoon state. The selector input is still
        // the ordinary area-boss bit consumed by the retail room header—not a patched state.
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(
            bus,
            SpongeBathRoom,
            new RoomStateSelectionContext(default, BossBits: 1, false, false));
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);

        LoadedBull natural = Load(bus, room, assets);
        VerifyPopulation(room, natural);
        LoadedBull drawing = Load(bus, room, assets);
        Step(drawing, assets);
        int pieces = VerifyDrawing(drawing.Enemies);
        NaturalResult cycle = VerifyNaturalCycle(natural, assets);
        VerifyContact(bus, room, assets);
        VerifyDamagingShot(bus, room, assets);
        VerifyAllImmuneShotDirections(bus, room, assets);
        VerifyShotReactionGuardAndAnimation(bus, room, assets);
        VerifyPowerBomb(bus, room, assets);
        VerifyGrappleKill(bus, room, assets);

        Console.WriteLine(
            "Bull audit passed: unchanged Sponge Bath loaded its single retail actor; " +
            $"seek/acceleration/deceleration crossed {cycle.Functions} functions and " +
            $"{cycle.AnimationMaps} maps, {pieces} OBJ pieces rendered, contact dealt ten " +
            "damage, ordinary damage/death resolved, all ten immune-shot knockback angles " +
            "and the 48-frame reaction guard animated, and power-bomb/grapple kills resolved.");
        return 0;
    }

    private static void VerifyPopulation(CartridgeRoomHeader room, LoadedBull loaded)
    {
        RoomEnemySlot bull = loaded.Actor;
        BullEnemyState state = loaded.State;
        if (room.State.Pointer != 0xcd88 || loaded.Enemies.EnemyCount != 1 ||
            bull.EnemyDefinitionPointer != BullDefinition || bull.XPosition != 0x00f0 ||
            bull.YPosition != 0x0088 || bull.Parameter1 != 3 || bull.Parameter2 != 3 ||
            bull.Properties != 0x2800 || bull.Health != 100 || bull.Definition.Damage != 10 ||
            bull.XRadius != 8 || bull.YRadius != 8 || bull.Definition.Bank != 0xa8 ||
            bull.Definition.InitializationAiPointer != 0xd8c9 ||
            bull.Definition.MainAiPointer != 0xd90b ||
            bull.Definition.GrappleAiPointer != 0x800a ||
            bull.Definition.TouchAiPointer != 0x8023 ||
            bull.Definition.ShotAiPointer != 0xdb14 || bull.Layer != 5 ||
            bull.CurrentInstruction != 0xd841 || bull.InstructionTimer != 1 ||
            state.Function != BullEnemyFunction.MovementDelay || state.ActivationTimer != 16 ||
            state.AccelerationIntervalTimerReset != 6 ||
            state.DecelerationIntervalTimerReset != 2 ||
            state.AccelerationIntervalTimer != 6 || state.MaxSpeed != 0x06ff)
        {
            throw new InvalidDataException(
                $"Sponge Bath Bull initialization failed: room state=${room.State.Pointer:X4}, " +
                $"count={loaded.Enemies.EnemyCount}, definition=${bull.EnemyDefinitionPointer:X4}, " +
                $"position=(${bull.XPosition:X4},${bull.YPosition:X4}), params=" +
                $"${bull.Parameter1:X4}/${bull.Parameter2:X4}, health/damage=" +
                $"{bull.Health}/{bull.Definition.Damage}, list=${bull.CurrentInstruction:X4}, " +
                $"function=$A8:{(ushort)state.Function:X4}, intervals=" +
                $"{state.AccelerationIntervalTimerReset}/{state.DecelerationIntervalTimerReset}, " +
                $"max=${state.MaxSpeed:X4}.");
        }
    }

    private static NaturalResult VerifyNaturalCycle(LoadedBull loaded, CartridgeRoomAssets assets)
    {
        var functions = new HashSet<BullEnemyFunction>();
        var maps = new HashSet<ushort>();
        ushort startX = loaded.Actor.XPosition;
        ushort startY = loaded.Actor.YPosition;

        // Keep the target within the native angle routine's documented one-byte coordinate
        // range. Bull should wait sixteen frames, acquire angle zero (right), then build its
        // 8.8 acceleration and speed at the parameter-selected six-frame interval.
        loaded.Samus.XPosition = unchecked((ushort)(startX + 96));
        loaded.Samus.YPosition = startY;
        for (int frame = 0; frame < 96; frame++)
        {
            Step(loaded, assets);
            functions.Add(loaded.State.Function);
            maps.Add(loaded.Actor.SpritemapPointer);
        }

        ushort acceleratedX = loaded.Actor.XPosition;
        ushort peakSpeed = loaded.State.Speed;
        if (acceleratedX == startX || loaded.Actor.YPosition != startY || peakSpeed == 0 ||
            loaded.State.Angle != 0 || loaded.State.Acceleration == 0)
        {
            throw new InvalidDataException(
                $"Bull acceleration failed: position=({startX},{startY})->" +
                $"({loaded.Actor.XPosition},{loaded.Actor.YPosition}), angle=" +
                $"${loaded.State.Angle:X2}, speed/acceleration=" +
                $"${loaded.State.Speed:X4}/${loaded.State.Acceleration:X4}.");
        }

        // Move Samus behind the actor. The wrapped signed-byte angle difference is $80, so
        // the same frame must select deceleration while preserving the original travel angle.
        loaded.Samus.XPosition = unchecked((ushort)(loaded.Actor.XPosition - 96));
        loaded.Samus.YPosition = loaded.Actor.YPosition;
        bool sawDeceleration = false;
        bool returnedToDelay = false;
        for (int frame = 0; frame < 512 && !returnedToDelay; frame++)
        {
            Step(loaded, assets);
            functions.Add(loaded.State.Function);
            maps.Add(loaded.Actor.SpritemapPointer);
            sawDeceleration |= loaded.State.Function == BullEnemyFunction.Decelerating;
            returnedToDelay = sawDeceleration &&
                loaded.State.Function == BullEnemyFunction.MovementDelay;
        }

        if (!sawDeceleration || !returnedToDelay || loaded.State.Speed != 0 ||
            loaded.State.Acceleration != 0 || maps.Count != 3 ||
            !functions.Contains(BullEnemyFunction.TargetSamus) ||
            !functions.Contains(BullEnemyFunction.Accelerating))
        {
            throw new InvalidDataException(
                $"Bull natural cycle failed: decelerated/returned=" +
                $"{sawDeceleration}/{returnedToDelay}, speed/acceleration=" +
                $"${loaded.State.Speed:X4}/${loaded.State.Acceleration:X4}, maps={maps.Count}, " +
                $"functions={string.Join(',', functions)}.");
        }

        return new NaturalResult(functions.Count, maps.Count);
    }

    private static int VerifyDrawing(RoomEnemySystem enemies)
    {
        var oam = new OamBuffer();
        oam.BeginFrame();
        enemies.DrawLayers(oam, 0, 0, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException("Sponge Bath Bull emitted no ROM-authored OBJ.");
        return oam.LastFinalizedSpriteCount;
    }

    private static void VerifyContact(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedBull loaded = Load(bus, room, assets);
        Step(loaded, assets);
        loaded.Samus.XPosition = loaded.Actor.XPosition;
        loaded.Samus.YPosition = loaded.Actor.YPosition;
        ushort health = loaded.Samus.Health;
        if (!loaded.Enemies.ResolveOrdinarySamusContact(loaded.Samus, 0) ||
            loaded.Samus.Health != health - 10 || !loaded.Samus.KnockbackActive ||
            loaded.Samus.InvincibilityTimer != 0x0060)
        {
            throw new InvalidDataException(
                $"Bull touch failed: health={health}->{loaded.Samus.Health}, " +
                $"knockback={loaded.Samus.KnockbackActive}, " +
                $"invincibility={loaded.Samus.InvincibilityTimer}.");
        }
    }

    private static void VerifyDamagingShot(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedBull loaded = Load(bus, room, assets);
        Step(loaded, assets);
        var projectiles = new SamusProjectileSystem();
        ArmProjectile(projectiles.Slots[0], loaded.Actor, type: 0x0100, direction: 2, damage: 20);
        if (loaded.Enemies.ResolveOrdinaryProjectileHits(
                bus,
                projectiles,
                new SamusBombProjectileSystem(),
                loaded.Samus) != 1 || loaded.Actor.Health != 80 ||
            loaded.Actor.CurrentInstruction != 0xd845 || loaded.State.ShotReactionDisabled)
        {
            throw new InvalidDataException(
                $"Bull damaging missile failed: health={loaded.Actor.Health}, list=" +
                $"${loaded.Actor.CurrentInstruction:X4}, guarded={loaded.State.ShotReactionDisabled}.");
        }

        // Four further 20-damage missiles prove normal vulnerability death and quota updates.
        for (int hit = 0; hit < 4; hit++)
        {
            loaded.Actor.InvincibilityTimer = 0;
            Step(loaded, assets);
            ArmProjectile(projectiles.Slots[0], loaded.Actor, 0x0100, 2, 20);
            loaded.Enemies.ResolveOrdinaryProjectileHits(
                bus,
                projectiles,
                new SamusBombProjectileSystem(),
                loaded.Samus);
        }
        if (!loaded.Actor.Properties.HasAny(EnemyProperties.Deleted) ||
            loaded.Enemies.EnemiesKilled != 1)
        {
            throw new InvalidDataException(
                $"Bull projectile death failed: health={loaded.Actor.Health}, deleted=" +
                $"{loaded.Actor.Properties.HasAny(EnemyProperties.Deleted)}, " +
                $"kills={loaded.Enemies.EnemiesKilled}.");
        }
    }

    private static void VerifyAllImmuneShotDirections(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        for (ushort direction = 0; direction < ShotAngles.Length; direction++)
        {
            LoadedBull loaded = Load(bus, room, assets);
            Step(loaded, assets);
            var projectiles = new SamusProjectileSystem();
            ArmProjectile(projectiles.Slots[0], loaded.Actor, type: 0, direction, damage: 20);
            int hits = loaded.Enemies.ResolveOrdinaryProjectileHits(
                bus,
                projectiles,
                new SamusBombProjectileSystem(),
                loaded.Samus);
            if (hits != 1 || loaded.Actor.Health != 100 ||
                loaded.Actor.CurrentInstruction != 0xd855 ||
                loaded.State.Angle != ShotAngles[direction] ||
                loaded.State.Speed != 0x0600 || loaded.State.Acceleration != 0x0100 ||
                loaded.State.Function != BullEnemyFunction.Decelerating ||
                loaded.State.ShotReactionDisableTimer != 0x0030 ||
                !loaded.State.ShotReactionDisabled)
            {
                throw new InvalidDataException(
                    $"Bull immune-shot direction {direction} failed: hits={hits}, " +
                    $"health={loaded.Actor.Health}, list=${loaded.Actor.CurrentInstruction:X4}, " +
                    $"angle=${loaded.State.Angle:X2}, speed/acceleration=" +
                    $"${loaded.State.Speed:X4}/${loaded.State.Acceleration:X4}, " +
                    $"guard={loaded.State.ShotReactionDisableTimer}/" +
                    $"{loaded.State.ShotReactionDisabled}.");
            }

            // Direction two is horizontal right. The 16-bit sine table caps at $7FFF; Bull
            // discards its low byte, so $7F * $0600 yields +2.$FA00 on the first frame.
            if (direction == 2)
            {
                ushort x = loaded.Actor.XPosition;
                ushort y = loaded.Actor.YPosition;
                Step(loaded, assets);
                if (loaded.Actor.XPosition != x + 2 ||
                    loaded.Actor.XSubposition != 0xfa00 || loaded.Actor.YPosition != y ||
                    loaded.Actor.YSubposition != 0)
                {
                    throw new InvalidDataException(
                        $"Bull direction-two fixed movement failed: X/Y=" +
                        $"({x},{y})->({loaded.Actor.XPosition},{loaded.Actor.YPosition}), " +
                        $"sub=({loaded.Actor.XSubposition:X4},{loaded.Actor.YSubposition:X4}).");
                }
            }

            // The opposite cardinal direction also proves $A8:DAF6's asymmetric negative
            // conversion. Negating +2.$FA00 produces whole $FFFD/fraction $0600, so the
            // wrapped whole coordinate falls by three while its subposition becomes $0600.
            if (direction == 7)
            {
                ushort x = loaded.Actor.XPosition;
                ushort y = loaded.Actor.YPosition;
                Step(loaded, assets);
                if (loaded.Actor.XPosition != x - 3 ||
                    loaded.Actor.XSubposition != 0x0600 || loaded.Actor.YPosition != y ||
                    loaded.Actor.YSubposition != 0)
                {
                    throw new InvalidDataException(
                        $"Bull direction-seven negative fixed movement failed: X/Y=" +
                        $"({x},{y})->({loaded.Actor.XPosition},{loaded.Actor.YPosition}), " +
                        $"sub=({loaded.Actor.XSubposition:X4},{loaded.Actor.YSubposition:X4}).");
                }
            }
        }
    }

    private static void VerifyShotReactionGuardAndAnimation(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedBull loaded = Load(bus, room, assets);
        Step(loaded, assets);
        var projectiles = new SamusProjectileSystem();
        var shared = new SamusBombProjectileSystem();
        ArmProjectile(projectiles.Slots[0], loaded.Actor, 0, 2, 20);
        loaded.Enemies.ResolveOrdinaryProjectileHits(bus, projectiles, shared, loaded.Samus);

        // One enemy frame lowers the guard to 47. A second immune impact is still consumed by
        // common shot AI but must not replace the angle, list, or cooldown with direction seven.
        Step(loaded, assets);
        ushort guardedAngle = loaded.State.Angle;
        ushort guardedTimer = loaded.State.ShotReactionDisableTimer;
        ArmProjectile(projectiles.Slots[0], loaded.Actor, 0, 7, 20);
        loaded.Enemies.ResolveOrdinaryProjectileHits(bus, projectiles, shared, loaded.Samus);
        if (loaded.State.Angle != guardedAngle ||
            loaded.State.ShotReactionDisableTimer != guardedTimer)
        {
            throw new InvalidDataException(
                $"Bull repeated-shot guard failed: angle=${guardedAngle:X2}->" +
                $"${loaded.State.Angle:X2}, timer={guardedTimer}->" +
                $"{loaded.State.ShotReactionDisableTimer}.");
        }

        var shotMaps = new HashSet<ushort>();
        bool guardExpired = false;
        bool returnedToNormalAnimation = false;
        for (int frame = 0; frame < 80; frame++)
        {
            Step(loaded, assets);
            shotMaps.Add(loaded.Actor.SpritemapPointer);
            guardExpired |= !loaded.State.ShotReactionDisabled;
            returnedToNormalAnimation |= loaded.Actor.CurrentInstruction is >= 0xd841 and <= 0xd853;
        }
        if (shotMaps.Count != 3 || !guardExpired || !returnedToNormalAnimation)
        {
            throw new InvalidDataException(
                $"Bull shot animation/guard expiry failed: maps={shotMaps.Count}, " +
                $"expired={guardExpired}, normal={returnedToNormalAnimation}, list=" +
                $"${loaded.Actor.CurrentInstruction:X4}.");
        }
    }

    private static void VerifyPowerBomb(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedBull loaded = Load(bus, room, assets);
        int reactions = loaded.Enemies.ResolveOrdinaryPowerBombHits(
            bus,
            loaded.Actor.XPosition,
            loaded.Actor.YPosition,
            explosionRadius: 32);
        if (reactions != 1 || !loaded.Actor.Properties.HasAny(EnemyProperties.Deleted) ||
            loaded.Enemies.EnemiesKilled != 1)
        {
            throw new InvalidDataException(
                $"Bull power-bomb death failed: reactions={reactions}, deleted=" +
                $"{loaded.Actor.Properties.HasAny(EnemyProperties.Deleted)}, " +
                $"kills={loaded.Enemies.EnemiesKilled}.");
        }
    }

    private static void VerifyGrappleKill(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedBull loaded = Load(bus, room, assets);
        Step(loaded, assets);
        GrappleEnemyCollision collision = loaded.Enemies.ResolveGrappleEndpoint(
            loaded.Actor.XPosition,
            loaded.Actor.YPosition);
        if (!collision.Collided || collision.Reaction != GrappleEnemyReaction.Kill)
            throw new InvalidDataException($"Bull grapple selected {collision.Reaction}.");
        Step(loaded, assets);
        if (!loaded.Actor.Properties.HasAny(EnemyProperties.Deleted) ||
            loaded.Enemies.EnemiesKilled != 1)
        {
            throw new InvalidDataException(
                $"Bull grapple kill failed: deleted=" +
                $"{loaded.Actor.Properties.HasAny(EnemyProperties.Deleted)}, " +
                $"kills={loaded.Enemies.EnemiesKilled}.");
        }
    }

    private static LoadedBull Load(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusState.FacingRightNormalPose,
            XPosition = 0x0080,
            YPosition = 0x0088,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
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
            level: assets.LevelData,
            samus: samus);
        RoomEnemySlot actor = enemies.Slots[0];
        BullEnemyState state = enemies.BullStates[0] ??
            throw new InvalidDataException("Sponge Bath slot zero has no Bull state.");
        return new LoadedBull(enemies, actor, state, samus);
    }

    private static void Step(LoadedBull loaded, CartridgeRoomAssets assets) =>
        loaded.Enemies.StepFrame(0, 0, false, loaded.Samus, level: assets.LevelData);

    private static void ArmProjectile(
        SamusProjectileSlot projectile,
        RoomEnemySlot target,
        ushort type,
        ushort direction,
        ushort damage)
    {
        projectile.ClearFields();
        projectile.Type = type;
        projectile.Damage = damage;
        projectile.Direction = direction;
        projectile.XPosition = target.XPosition;
        projectile.YPosition = target.YPosition;
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    private readonly record struct LoadedBull(
        RoomEnemySystem Enemies,
        RoomEnemySlot Actor,
        BullEnemyState State,
        SamusState Samus);

    private readonly record struct NaturalResult(int Functions, int AnimationMaps);
}
