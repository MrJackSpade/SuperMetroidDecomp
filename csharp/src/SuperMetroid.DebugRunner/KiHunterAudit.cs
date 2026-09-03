using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;

internal static class KiHunterAudit
{
    private const ushort NormalRoomPointer = 0x948c;
    private const ushort NormalStatePointer = 0x9499;
    private const ushort NormalPopulationPointer = 0x8f19;
    private const ushort RedRoomPointer = 0xca52;
    private const ushort RedStatePointer = 0xca7e;
    private const ushort RedPopulationPointer = 0xbfe6;
    private const ushort GoldRoomPointer = 0xb585;
    private const ushort GoldStatePointer = 0xb592;
    private const ushort GoldPopulationPointer = 0xa428;

    private static readonly ushort[] RetailPopulations =
        [0x8f19, 0x8fc5, 0x98f7, 0xbfe6, 0xa428, 0xba4b];

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        VerifyAllHeaders(bus);
        VerifyAllRetailPairs(bus);

        CartridgeRoomHeader normalRoom = LoadExpectedRoom(
            bus, NormalRoomPointer, NormalStatePointer, NormalPopulationPointer);
        // $CA52's red population is the boss-dead branch. All boss bits are supplied here
        // only to select that authored state directly; the enemy audit does not mutate save data.
        CartridgeRoomHeader redRoom = CartridgeRoomHeader.Load(
            bus,
            RedRoomPointer,
            new RoomStateSelectionContext(Array.Empty<byte>(), ushort.MaxValue, false, false));
        AssertExpectedState(redRoom, RedStatePointer, RedPopulationPointer);
        CartridgeRoomHeader goldRoom = LoadExpectedRoom(
            bus, GoldRoomPointer, GoldStatePointer, GoldPopulationPointer);
        CartridgeRoomAssets normalAssets = CartridgeRoomAssets.Load(bus, normalRoom);
        CartridgeRoomAssets redAssets = CartridgeRoomAssets.Load(bus, redRoom);
        CartridgeRoomAssets goldAssets = CartridgeRoomAssets.Load(bus, goldRoom);

        VerifyFlyingPair(bus, normalRoom, normalAssets);
        VerifyGroundedAttackAndAcid(bus, redRoom, redAssets);
        VerifyGroundedShotAfterWingDefinitionClear(bus, redRoom, redAssets);
        VerifyDamageFreezeDetachAndDeath(bus, normalRoom, normalAssets);
        VerifyGoldVariantLoad(bus, goldRoom, goldAssets);

        Console.WriteLine(
            "Ki-Hunter audit passed: all 38 body/wing records across six populations pair " +
            "correctly; normal/red/gold headers, patrol/swoop, ground fall/jump/wait, ROM " +
            "animation callbacks, acid-spit projectile motion/damage, hurt/freeze mirroring, " +
            "deleted-wing physical aliasing, health-threshold wing detachment, detached-wing " +
            "orbit, death cleanup, and OBJ " +
            "drawing agree with the cartridge path.");
        return 0;
    }

    private static void VerifyFlyingPair(
        ISnesAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedPair loaded = LoadPair(bus, room, assets, room.State.EnemyPopulationPointer);
        RoomEnemySlot body = loaded.Body;
        RoomEnemySlot wings = loaded.Wings;
        KiHunterEnemyState bodyState = State(loaded, body);
        KiHunterEnemyState wingState = State(loaded, wings);
        if (body.EnemyDefinitionPointer != 0xeabf || wings.EnemyDefinitionPointer != 0xeaff ||
            body.Health != 60 || wings.Health != 60 ||
            body.CurrentInstruction != 0xe9fa || wings.CurrentInstruction != 0xea4e ||
            bodyState.Function != KiHunterEnemyFunction.FlyingPatrol ||
            wingState.Function != KiHunterEnemyFunction.FollowBody ||
            bodyState.UpperPatrolY != unchecked((ushort)(body.Spawn.Population.YPosition - 16)) ||
            bodyState.LowerPatrolY != unchecked((ushort)(body.Spawn.Population.YPosition + 16)) ||
            body.XPosition != wings.XPosition || body.YPosition != wings.YPosition ||
            body.PaletteIndex != wings.PaletteIndex || body.VramTilesIndex != wings.VramTilesIndex)
        {
            throw new InvalidDataException(
                $"Normal Ki-Hunter initialization mismatch: body/wing=" +
                $"${body.EnemyDefinitionPointer:X4}/${wings.EnemyDefinitionPointer:X4}, " +
                $"functions=${(ushort)bodyState.Function:X4}/${(ushort)wingState.Function:X4}, " +
                $"lists=${body.CurrentInstruction:X4}/${wings.CurrentInstruction:X4}.");
        }

        ushort startX = body.XPosition;
        ushort startY = body.YPosition;
        loaded.Samus.XPosition = body.XPosition;
        loaded.Samus.YPosition = unchecked((ushort)(body.YPosition + 64));
        bool sawSwoop = false;
        bool sawSwoopAnimation = false;
        var maps = new HashSet<ushort>();
        for (int frame = 0; frame < 160; frame++)
        {
            Step(loaded, assets);
            maps.Add(body.SpritemapPointer);
            sawSwoop |= bodyState.Function == KiHunterEnemyFunction.Swooping;
            sawSwoopAnimation |= bodyState.SwoopAnimationChanged;
            if (wingState.Function == KiHunterEnemyFunction.FollowBody &&
                (body.XPosition != wings.XPosition || body.YPosition != wings.YPosition))
            {
                throw new InvalidDataException(
                    $"Attached Ki-Hunter wings lagged body on frame {frame}: " +
                    $"({body.XPosition},{body.YPosition})/({wings.XPosition},{wings.YPosition}).");
            }
        }
        if (!sawSwoop || !sawSwoopAnimation || maps.Count < 3 ||
            body.XPosition == startX && body.YPosition == startY)
        {
            throw new InvalidDataException(
                $"Ki-Hunter flight did not exercise its ROM arc: swoop={sawSwoop}, " +
                $"animationChange={sawSwoopAnimation}, maps={maps.Count}, " +
                $"position=({startX},{startY})->({body.XPosition},{body.YPosition}).");
        }
        VerifyDrawing(loaded.Enemies, body);
    }

    private static void VerifyGroundedAttackAndAcid(
        ISnesAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedPair loaded = LoadPair(bus, room, assets, room.State.EnemyPopulationPointer);
        KiHunterEnemyState state = State(loaded, loaded.Body);
        if (loaded.Body.EnemyDefinitionPointer != 0xeb3f ||
            loaded.Wings.EnemyDefinitionPointer != 0xeb7f ||
            (loaded.Body.Parameter1 & 0x8000) == 0 ||
            state.Function != KiHunterEnemyFunction.FallingAfterWingLoss ||
            !state.HasLostWings ||
            !loaded.Wings.Properties.HasAny(EnemyProperties.Deleted))
        {
            throw new InvalidDataException("Red ground Ki-Hunter did not start wingless and falling.");
        }

        loaded.Samus.XPosition = unchecked((ushort)(loaded.Body.XPosition + 32));
        loaded.Samus.YPosition = loaded.Body.YPosition;
        bool sawJump = false;
        bool sawWait = false;
        RoomEnemyProjectileSlot? acid = null;
        for (int frame = 0; frame < 900 && acid is null; frame++)
        {
            Step(loaded, assets);
            sawJump |= state.Function == KiHunterEnemyFunction.GroundJump;
            sawWait |= state.Function is
                KiHunterEnemyFunction.GroundWait or KiHunterEnemyFunction.SelectAcidSpit;
            acid = loaded.Enemies.EnemyProjectiles.FirstOrDefault(projectile =>
                projectile.Kind is RoomEnemyProjectileKind.KiHunterAcidSpitLeft or
                    RoomEnemyProjectileKind.KiHunterAcidSpitRight);
        }
        if (!sawJump || !sawWait || acid is null ||
            loaded.Enemies.LastKiHunterSoundEffect != 0x004c ||
            acid.Damage == 0 || !acid.CanDamageSamus)
        {
            throw new InvalidDataException(
                $"Ground Ki-Hunter attack cycle failed: jump={sawJump}, wait={sawWait}, " +
                $"acid={acid is not null}, sound={loaded.Enemies.LastKiHunterSoundEffect}.");
        }

        ushort spawnX = acid.XPosition;
        ushort spawnY = acid.YPosition;
        ushort initialPreInstruction = acid.PreInstruction;
        bool movingRight = acid.Kind == RoomEnemyProjectileKind.KiHunterAcidSpitRight;
        loaded.Enemies.StepEnemyProjectiles(
            assets.LevelData,
            samus: null,
            cameraX: CameraX(loaded.Body),
            cameraY: CameraY(loaded.Body));
        if (initialPreInstruction != 0xcff7 ||
            acid.PreInstruction != 0xcff7 || acid.SpritemapPointer is 0 or 0x8000 ||
            acid.XPosition != unchecked((ushort)(spawnX + (movingRight ? 3 : -3))) ||
            acid.YPosition != spawnY || acid.XSubposition != 0 || acid.YSubposition != 0 ||
            acid.YVelocity != 0x0010)
        {
            throw new InvalidDataException(
                $"Acid first frame mismatch: pre=${initialPreInstruction:X4}->" +
                $"${acid.PreInstruction:X4}, position=({spawnX},{spawnY})->" +
                $"({acid.XPosition},{acid.YPosition}), map=${acid.SpritemapPointer:X4}.");
        }

        ushort afterShiftX = acid.XPosition;
        loaded.Enemies.StepEnemyProjectiles(
            assets.LevelData,
            samus: null,
            cameraX: CameraX(loaded.Body),
            cameraY: CameraY(loaded.Body));
        if (acid.IsActive &&
            (acid.XPosition != unchecked((ushort)(afterShiftX + (movingRight ? 3 : -3))) ||
             acid.YPosition != spawnY || acid.YSubposition != 0x1000 ||
             acid.YVelocity != 0x0020))
        {
            throw new InvalidDataException(
                $"Acid movement/gravity failed: X={afterShiftX}->{acid.XPosition}, " +
                $"Y={acid.YPosition:X4}.{acid.YSubposition:X4}, velocity=${acid.YVelocity:X4}.");
        }

        // $CFD5/$CFE6 is embedded after the opening map, so it does not run on the spawn
        // frame. Continue until one frame performs the standalone 19-pixel offset instead
        // of ordinary velocity; that proves the one-shot callback executes at the ROM boundary.
        bool sawMuzzleShiftInstruction = false;
        ushort previousX = acid.XPosition;
        for (int frame = 0; frame < 32 && acid.IsActive && !sawMuzzleShiftInstruction; frame++)
        {
            loaded.Enemies.StepEnemyProjectiles(
                assets.LevelData,
                samus: null,
                cameraX: CameraX(loaded.Body),
                cameraY: CameraY(loaded.Body));
            short deltaX = unchecked((short)(acid.XPosition - previousX));
            sawMuzzleShiftInstruction = Math.Abs(deltaX) == 19;
            previousX = acid.XPosition;
        }
        if (!sawMuzzleShiftInstruction)
            throw new InvalidDataException("Ki-Hunter acid never executed its $CFD5/$CFE6 muzzle shift.");

        // A fresh projectile placed on Samus proves shared bank-$A0 projectile damage and
        // deletion, independently of where the room geometry eventually splashes this one.
        LoadedPair damaging = LoadPair(bus, room, assets, room.State.EnemyPopulationPointer);
        RoomEnemyProjectileSlot damagingAcid = AdvanceToAcid(damaging, assets);
        damaging.Samus.Health = 999;
        damaging.Samus.InvincibilityTimer = 0;
        damaging.Samus.XPosition = damagingAcid.XPosition;
        damaging.Samus.YPosition = damagingAcid.YPosition;
        ushort acidDamage = damagingAcid.Damage;
        damaging.Enemies.StepEnemyProjectiles(
            assets.LevelData,
            damaging.Samus,
            cameraX: CameraX(damaging.Body),
            cameraY: CameraY(damaging.Body));
        if (damaging.Samus.Health != 999 - acidDamage || damagingAcid.IsActive)
            throw new InvalidDataException("Ki-Hunter acid did not apply header damage and delete on contact.");
    }

    private static void VerifyDamageFreezeDetachAndDeath(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedPair frozen = LoadPair(bus, room, assets, room.State.EnemyPopulationPointer);
        Step(frozen, assets);
        int vulnerabilityAddress = 0xb40000 | frozen.Body.Definition.VulnerabilityPointer;
        int freezeBeamType = Enumerable.Range(0, 12)
            .FirstOrDefault(index => bus.ReadByte(vulnerabilityAddress + index) == 0xff, -1);
        FireProjectile(
            bus,
            frozen,
            projectileType: freezeBeamType >= 0 ? (ushort)freezeBeamType : (ushort)0,
            damage: 20);
        bool synchronized = frozen.Body.FrozenTimer == frozen.Wings.FrozenTimer &&
            frozen.Body.InvincibilityTimer == frozen.Wings.InvincibilityTimer &&
            frozen.Body.FlashTimer == frozen.Wings.FlashTimer &&
            frozen.Body.AiHandlerBits == frozen.Wings.AiHandlerBits;
        bool expectedReaction = freezeBeamType >= 0
            ? frozen.Body.FrozenTimer == 400
            : frozen.Body.Health < frozen.Body.Definition.Health && frozen.Body.FlashTimer != 0;
        if (!synchronized || !expectedReaction)
        {
            throw new InvalidDataException(
                $"Ki-Hunter body hurt/freeze state was not mirrored: type={freezeBeamType}, frozen=" +
                $"{frozen.Body.FrozenTimer}/{frozen.Wings.FrozenTimer}, invincibility=" +
                $"{frozen.Body.InvincibilityTimer}/{frozen.Wings.InvincibilityTimer}, AI=" +
                $"${frozen.Body.AiHandlerBits:X4}/${frozen.Wings.AiHandlerBits:X4}, health=" +
                $"{frozen.Body.Health}/{frozen.Wings.Health}.");
        }

        LoadedPair detached = LoadPair(bus, room, assets, room.State.EnemyPopulationPointer);
        Step(detached, assets);
        for (int hit = 0; hit < 4; hit++)
        {
            detached.Body.InvincibilityTimer = 0;
            FireProjectile(bus, detached, projectileType: 0, damage: 20);
        }
        KiHunterEnemyState bodyState = State(detached, detached.Body);
        KiHunterEnemyState wingState = State(detached, detached.Wings);
        if (detached.Body.Health > detached.Wings.Parameter1 || !bodyState.HasLostWings ||
            bodyState.Function != KiHunterEnemyFunction.FallingAfterWingLoss ||
            wingState.Function != KiHunterEnemyFunction.DetachedWing ||
            wingState.WingFunction != KiHunterWingFunction.Orbit ||
            detached.Wings.SpritemapPointer != 0x804d ||
            !detached.Wings.Properties.HasAny(EnemyProperties.ProcessOffScreen))
        {
            throw new InvalidDataException(
                $"Ki-Hunter wing detachment mismatch: health={detached.Body.Health}/" +
                $"threshold={detached.Wings.Parameter1}, body=${(ushort)bodyState.Function:X4}, " +
                $"wing=${(ushort)wingState.Function:X4}/${(ushort)wingState.WingFunction:X4}.");
        }
        ushort wingX = detached.Wings.XPosition;
        ushort wingY = detached.Wings.YPosition;
        Step(detached, assets);
        if (detached.Wings.XPosition == wingX && detached.Wings.YPosition == wingY)
            throw new InvalidDataException("Detached Ki-Hunter wing orbit did not advance.");

        LoadedPair killed = LoadPair(bus, room, assets, room.State.EnemyPopulationPointer);
        Step(killed, assets);
        FireProjectile(
            bus,
            killed,
            projectileType: (ushort)SamusProjectileFamily.SuperMissile,
            damage: 300);
        if (killed.Body.Health != 0 ||
            !killed.Body.Properties.HasAny(EnemyProperties.Deleted) ||
            killed.Wings.Properties != (ushort)EnemyProperties.Deleted ||
            killed.Enemies.EnemiesKilled != 1)
        {
            throw new InvalidDataException(
                $"Fatal Ki-Hunter cleanup mismatch: health={killed.Body.Health}, " +
                $"properties=${killed.Body.Properties:X4}/${killed.Wings.Properties:X4}, " +
                $"kills={killed.Enemies.EnemiesKilled}.");
        }
    }

    /// <summary>
    /// Proves the retail ground-bound red variant remains shootable after the common enemy
    /// scheduler consumes its initializer-set deleted property. Native code clears only the
    /// following wing record's definition word, then $A8:F701 continues to use that record as
    /// a raw <c>body + $40</c> alias. This exact lifecycle exposed the former typed-adjacency
    /// exception in the exhaustive projectile audit.
    /// </summary>
    private static void VerifyGroundedShotAfterWingDefinitionClear(
        ISnesAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedPair loaded = LoadPair(bus, room, assets, room.State.EnemyPopulationPointer);

        // The wing initializer sets property $0200. The first frame's selection pass converts
        // that tombstone to a free definition word while deliberately preserving parameter 1
        // and every other native slot field used by KiHunter_Shot.
        Step(loaded, assets);
        if (loaded.Wings.EnemyDefinitionPointer != 0)
        {
            throw new InvalidDataException(
                $"Ground Ki-Hunter wing definition remained ${loaded.Wings.EnemyDefinitionPointer:X4} " +
                "after its deletion-selection frame.");
        }

        ushort healthBefore = loaded.Body.Health;
        FireProjectile(bus, loaded, projectileType: 0, damage: 20);
        if (loaded.Body.Health >= healthBefore || loaded.Body.Health == 0 ||
            !State(loaded, loaded.Body).HasLostWings)
        {
            throw new InvalidDataException(
                $"Ground Ki-Hunter shot after wing deletion produced health " +
                $"{healthBefore}->{loaded.Body.Health} or restored its wing-loss state.");
        }
    }

    private static void VerifyGoldVariantLoad(
        ISnesAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedPair loaded = LoadPair(bus, room, assets, room.State.EnemyPopulationPointer);
        if (loaded.Body.EnemyDefinitionPointer != 0xebbf ||
            loaded.Wings.EnemyDefinitionPointer != 0xebff ||
            loaded.Body.Health != 1800 || loaded.Wings.Health != 1800 ||
            loaded.Body.Definition.Damage != 200 || loaded.Wings.Definition.Damage != 200)
        {
            throw new InvalidDataException("Gold Ki-Hunter variant did not retain its own header stats.");
        }
        Step(loaded, assets);
        VerifyDrawing(loaded.Enemies, loaded.Body);
    }

    private static RoomEnemyProjectileSlot AdvanceToAcid(
        LoadedPair loaded,
        CartridgeRoomAssets assets)
    {
        loaded.Samus.XPosition = unchecked((ushort)(loaded.Body.XPosition + 32));
        loaded.Samus.YPosition = loaded.Body.YPosition;
        for (int frame = 0; frame < 900; frame++)
        {
            Step(loaded, assets);
            RoomEnemyProjectileSlot? acid = loaded.Enemies.EnemyProjectiles.FirstOrDefault(projectile =>
                projectile.Kind is RoomEnemyProjectileKind.KiHunterAcidSpitLeft or
                    RoomEnemyProjectileKind.KiHunterAcidSpitRight);
            if (acid is not null)
                return acid;
        }
        throw new InvalidDataException("Ground Ki-Hunter did not spawn acid within 900 frames.");
    }

    private static void FireProjectile(
        ISnesAddressSpace bus,
        LoadedPair loaded,
        ushort projectileType,
        ushort damage)
    {
        var projectiles = new SamusProjectileSystem();
        SamusProjectileSlot projectile = projectiles.Slots[0];
        projectile.ClearFields();
        projectile.Type = projectileType;
        projectile.Damage = damage;
        projectile.Direction = (ushort)SamusProjectileDirection.Right;
        projectile.XPosition = loaded.Body.XPosition;
        projectile.YPosition = loaded.Body.YPosition;
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
        int hits = loaded.Enemies.ResolveOrdinaryProjectileHits(
            bus,
            projectiles,
            new SamusBombProjectileSystem(),
            loaded.Samus);
        if (hits != 1)
            throw new InvalidDataException($"Ki-Hunter projectile hit count was {hits}, expected one.");
    }

    private static LoadedPair LoadPair(
        ISnesAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        ushort populationPointer)
    {
        var pairBus = new PopulationPrefixAddressSpace(
            bus,
            populationPointer,
            retainedRecordCount: 2,
            deathQuota: 1);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState(0x4567);
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusState.FacingRightNormalPose,
        };
        samus.RefreshCollisionRadii(pairBus);
        samus.InitializeAnimation(pairBus);
        var enemies = new RoomEnemySystem();
        enemies.Load(
            pairBus,
            populationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            random.NextRandom,
            random.SetRandomNumber,
            readRandomNumber: () => random.RandomNumber,
            level: assets.LevelData,
            samus: samus,
            cameraX: 0,
            cameraY: 0);
        return new LoadedPair(enemies, enemies.Slots[0], enemies.Slots[1], samus);
    }

    private static void Step(LoadedPair loaded, CartridgeRoomAssets assets) =>
        loaded.Enemies.StepFrame(
            CameraX(loaded.Body),
            CameraY(loaded.Body),
            timeIsFrozen: false,
            loaded.Samus,
            level: assets.LevelData);

    private static KiHunterEnemyState State(LoadedPair loaded, RoomEnemySlot actor) =>
        loaded.Enemies.KiHunterStates[actor.SlotIndex] ?? throw new InvalidDataException(
            $"Ki-Hunter slot {actor.SlotIndex} has no typed state.");

    private static void VerifyDrawing(RoomEnemySystem enemies, RoomEnemySlot body)
    {
        var oam = new OamBuffer();
        oam.BeginFrame();
        enemies.DrawLayers(oam, CameraX(body), CameraY(body), 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount < 2)
            throw new InvalidDataException("Ki-Hunter body/wing pair emitted fewer than two OBJ pieces.");
    }

    private static ushort CameraX(RoomEnemySlot actor) =>
        unchecked((ushort)Math.Max(0, actor.XPosition - 128));

    private static ushort CameraY(RoomEnemySlot actor) =>
        unchecked((ushort)Math.Max(0, actor.YPosition - 96));

    private static CartridgeRoomHeader LoadExpectedRoom(
        ISnesAddressSpace bus,
        ushort roomPointer,
        ushort statePointer,
        ushort populationPointer)
    {
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, roomPointer);
        AssertExpectedState(room, statePointer, populationPointer);
        return room;
    }

    private static void AssertExpectedState(
        CartridgeRoomHeader room,
        ushort statePointer,
        ushort populationPointer)
    {
        if (room.State.Pointer == statePointer && room.State.EnemyPopulationPointer == populationPointer)
            return;
        throw new InvalidDataException(
            $"Ki-Hunter room ${room.Pointer:X4} selected state/population " +
            $"${room.State.Pointer:X4}/${room.State.EnemyPopulationPointer:X4}, expected " +
            $"${statePointer:X4}/${populationPointer:X4}.");
    }

    private static void VerifyAllHeaders(ISnesAddressSpace bus)
    {
        VerifyHeader(bus, 0xeabf, 0xf188, 0xf25c, 0x8023, 0xf701, 60, 20);
        VerifyHeader(bus, 0xeaff, 0xf214, 0xf262, 0x804c, 0x804c, 60, 20);
        VerifyHeader(bus, 0xeb3f, 0xf188, 0xf25c, 0x8023, 0xf701, 360, 60);
        VerifyHeader(bus, 0xeb7f, 0xf214, 0xf262, 0x804c, 0x804c, 360, 60);
        VerifyHeader(bus, 0xebbf, 0xf188, 0xf25c, 0x8023, 0xf701, 1800, 200);
        VerifyHeader(bus, 0xebff, 0xf214, 0xf262, 0x804c, 0x804c, 1800, 200);
    }

    private static void VerifyHeader(
        ISnesAddressSpace bus,
        ushort pointer,
        ushort init,
        ushort main,
        ushort touch,
        ushort shot,
        ushort health,
        ushort damage)
    {
        RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(bus, pointer);
        if (definition.Bank != 0xa8 || definition.InitializationAiPointer != init ||
            definition.MainAiPointer != main || definition.TouchAiPointer != touch ||
            definition.ShotAiPointer != shot || definition.Health != health ||
            definition.Damage != damage)
        {
            throw new InvalidDataException(
                $"Ki-Hunter header ${pointer:X4} mismatch: bank/init/main=" +
                $"${definition.Bank:X2}:${definition.InitializationAiPointer:X4}/" +
                $"${definition.MainAiPointer:X4}, touch/shot=${definition.TouchAiPointer:X4}/" +
                $"${definition.ShotAiPointer:X4}, health/damage={definition.Health}/{definition.Damage}.");
        }
    }

    private static void VerifyAllRetailPairs(ISnesAddressSpace bus)
    {
        var counts = new Dictionary<ushort, int>();
        foreach (ushort population in RetailPopulations)
        {
            var definitions = new List<ushort>();
            for (int index = 0; index < RoomEnemySystem.MaximumEnemyCount; index++)
            {
                ushort definition = ReadWord(bus, 0xa10000 | (population + index * 16));
                if (definition == 0xffff)
                    break;
                definitions.Add(definition);
                if (definition is 0xeabf or 0xeaff or 0xeb3f or 0xeb7f or 0xebbf or 0xebff)
                    counts[definition] = counts.GetValueOrDefault(definition) + 1;
            }
            for (int index = 0; index < definitions.Count; index++)
            {
                ushort definition = definitions[index];
                bool validBody = definition is 0xeabf or 0xeb3f or 0xebbf &&
                    index + 1 < definitions.Count &&
                    IsExpectedPair(definition, definitions[index + 1]);
                bool validWing = definition is 0xeaff or 0xeb7f or 0xebff &&
                    index > 0 && IsExpectedPair(definitions[index - 1], definition);
                bool belongsToFamily = definition is
                    0xeabf or 0xeaff or 0xeb3f or 0xeb7f or 0xebbf or 0xebff;
                if (belongsToFamily && !validBody && !validWing)
                {
                    throw new InvalidDataException(
                        $"Population $A1:{population:X4} Ki-Hunter record {index} is not in its matching pair.");
                }
            }
        }

        var expected = new Dictionary<ushort, int>
        {
            [0xeabf] = 9, [0xeaff] = 9,
            [0xeb3f] = 4, [0xeb7f] = 4,
            [0xebbf] = 6, [0xebff] = 6,
        };
        if (expected.Any(pair => counts.GetValueOrDefault(pair.Key) != pair.Value) ||
            counts.Values.Sum() != 38)
        {
            throw new InvalidDataException(
                "Retail Ki-Hunter inventory did not contain the expected 19 body/wing pairs.");
        }
    }

    private static bool IsExpectedPair(ushort body, ushort wings) =>
        (body, wings) is
            (0xeabf, 0xeaff) or
            (0xeb3f, 0xeb7f) or
            (0xebbf, 0xebff);

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private readonly record struct LoadedPair(
        RoomEnemySystem Enemies,
        RoomEnemySlot Body,
        RoomEnemySlot Wings,
        SamusState Samus);
}
