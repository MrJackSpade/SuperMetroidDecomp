using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>
/// End-to-end retail-ROM audit for Golden Torizo. The room, population, definition,
/// instruction lists, extended hitboxes, projectiles, collision layer, palettes, and
/// vulnerability table all come from the user's cartridge image.
/// </summary>
internal static partial class GoldenTorizoAudit
{
    private const ushort RoomPointer = 0xb283;
    private const ushort Definition = 0xef7f;
    private const ushort PopulationPointer = 0xb720;
    private const ushort CameraX = 0x0100;
    private const ushort CameraY = 0x0100;
    private const ushort StandUpSitDownShotCallback = 0xc9c2;
    private const ushort NormalBodyShotCallback = 0xc97c;
    private const ushort DefaultEnemyVulnerability = 0xec1c;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, RoomPointer);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        VerifyRetailStructures(bus, room);
        VerifyEncounter(bus, room, assets);
        VerifyNaturalProjectileInteractions(bus, room, assets);
        VerifyNormalBombReaction(bus, room, assets);
        VerifyAlreadyDefeatedLoad(bus, room, assets);

        Console.WriteLine(
            "Golden Torizo audit passed: retail room/header/population, activation, " +
            "extended animation, walking/jumping, five projectile attacks, missile and " +
            "Super Missile reactions, every damage-enabled projectile phase, orb/egg/" +
            "Super shot responses, contact/projectile damage, Power Bomb immunity, " +
            "death, palette, item " +
            "drop, music, and area boss bit all used cartridge data.");
        return 0;
    }

    private static void VerifyRetailStructures(
        ISnesAddressSpace bus,
        CartridgeRoomHeader room)
    {
        if (room.Pointer != RoomPointer || room.AreaIndex != 2 ||
            room.State.Pointer != 0xb295 ||
            room.State.EnemyPopulationPointer != PopulationPointer)
        {
            throw new InvalidDataException(
                $"Golden Torizo room mismatch: room/state=${room.Pointer:X4}/${room.State.Pointer:X4}, " +
                $"area={room.AreaIndex}, size={room.WidthInScreens}x{room.HeightInScreens}, " +
                $"population/tiles=${room.State.EnemyPopulationPointer:X4}/" +
                $"${room.State.EnemyTilesetPointer:X4}.");
        }

        ushort[] expectedPopulation = [Definition, 0x0080, 0x0180];
        for (int word = 0; word < expectedPopulation.Length; word++)
        {
            ushort actual = ReadWord(bus, 0xa10000 | (PopulationPointer + word * 2));
            if (actual != expectedPopulation[word])
            {
                throw new InvalidDataException(
                    $"Golden Torizo population word {word} is ${actual:X4}, expected " +
                    $"${expectedPopulation[word]:X4}.");
            }
        }

        RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(bus, Definition);
        if (definition.Bank != 0xaa || definition.Health != 13500 ||
            definition.Damage != 160 || definition.BossId != 2 ||
            definition.InitializationAiPointer != 0xc87f ||
            definition.MainAiPointer != 0xd369 || definition.HurtAiPointer != 0xd3ba ||
            definition.TouchAiPointer != 0xc977 || definition.ShotAiPointer != 0xd667 ||
            definition.PowerBombReactionPointer != 0)
        {
            throw new InvalidDataException(
                $"Golden Torizo header mismatch: bank=${definition.Bank:X2}, " +
                $"health/damage={definition.Health}/{definition.Damage}, boss={definition.BossId}, " +
                $"init/main/hurt=${definition.InitializationAiPointer:X4}/" +
                $"${definition.MainAiPointer:X4}/${definition.HurtAiPointer:X4}, " +
                $"touch/shot/power-bomb=${definition.TouchAiPointer:X4}/" +
                $"${definition.ShotAiPointer:X4}/${definition.PowerBombReactionPointer:X4}.");
        }
    }

    private static void VerifyEncounter(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        bool bossBitSet = false;
        LoadedGoldenTorizo loaded = Load(
            bus,
            room,
            assets,
            alreadyDefeated: false,
            () => bossBitSet = true);

        VerifyStandUpSitDownShotCallback(bus, room, assets);

        if (loaded.Enemies.EnemyCount != 1 || loaded.Head.Health != 13500 ||
            loaded.Head.XPosition != 0x01a8 || loaded.Head.YPosition != 0x0090 ||
            loaded.Head.XRadius != 18 || loaded.Head.YRadius != 41 ||
            loaded.State.Function != 0xc6bf)
        {
            throw new InvalidDataException(
                $"Golden Torizo initialization mismatch: count={loaded.Enemies.EnemyCount}, " +
                $"health={loaded.Head.Health}, pos=(${loaded.Head.XPosition:X4}," +
                $"${loaded.Head.YPosition:X4}), function=${loaded.State.Function:X4}, " +
                $"radius={loaded.Head.XRadius}x{loaded.Head.YRadius}.");
        }

        var maps = new HashSet<ushort>();
        var attacks = new HashSet<RoomEnemyProjectileKind>();
        ushort startX = loaded.Head.XPosition;
        ushort startY = loaded.Head.YPosition;
        for (int frame = 0; frame < 30000; frame++)
        {
            Step(loaded, assets.LevelData);
            maps.Add(loaded.Head.SpritemapPointer);
            foreach (RoomEnemyProjectileSlot projectile in loaded.Enemies.EnemyProjectiles)
            {
                if (projectile.IsActive)
                    attacks.Add(projectile.Kind);
            }

            if (maps.Count >= 8 && loaded.Head.XPosition != startX &&
                loaded.Head.YPosition != startY &&
                attacks.Contains(RoomEnemyProjectileKind.GoldenTorizoChozoOrb) &&
                attacks.Contains(RoomEnemyProjectileKind.GoldenTorizoSonicBoom) &&
                attacks.Contains(RoomEnemyProjectileKind.GoldenTorizoEyeBeam))
            {
                break;
            }
        }

        if (maps.Count < 8 || loaded.Head.XPosition == startX || loaded.Head.YPosition == startY ||
            !attacks.Contains(RoomEnemyProjectileKind.GoldenTorizoChozoOrb) ||
            !attacks.Contains(RoomEnemyProjectileKind.GoldenTorizoSonicBoom) ||
            !attacks.Contains(RoomEnemyProjectileKind.GoldenTorizoEyeBeam))
        {
            throw new InvalidDataException(
                $"Golden Torizo live cycle incomplete: maps={maps.Count}, position=" +
                $"({startX},{startY})->({loaded.Head.XPosition},{loaded.Head.YPosition}), " +
                $"attacks=[{string.Join(',', attacks)}].");
        }

        // The egg is a low-health authored branch and the held Super Missile is reached
        // only after Samus supplies one. Drive those real list entries explicitly instead
        // of pretending a full-health idle sample can prove conditional attacks.
        loaded.Head.CurrentInstruction = 0xd031;
        loaded.Head.InstructionTimer = 1;
        bool sawEgg = false;
        for (int frame = 0; frame < 1200 && !sawEgg; frame++)
        {
            Step(loaded, assets.LevelData);
            sawEgg = loaded.Enemies.EnemyProjectiles.Any(
                projectile => projectile.Kind == RoomEnemyProjectileKind.GoldenTorizoEgg);
        }
        if (!sawEgg)
            throw new InvalidDataException("Golden Torizo low-health egg list spawned no egg.");

        var oam = new OamBuffer();
        oam.BeginFrame();
        loaded.Enemies.DrawEnemyProjectiles(oam, CameraX, CameraY);
        loaded.Enemies.DrawLayers(oam, CameraX, CameraY, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException("Golden Torizo encounter emitted no OBJ pieces.");

        loaded.Samus.XPosition = loaded.Head.XPosition;
        loaded.Samus.YPosition = loaded.Head.YPosition;
        loaded.Samus.InvincibilityTimer = 0;
        loaded.Samus.KnockbackTimer = 0;
        loaded.Samus.KnockbackDirection = 0;
        loaded.Samus.KnockbackActive = false;
        loaded.Samus.Pose = SamusState.FacingRightNormalPose;
        loaded.Samus.RefreshCollisionRadii(bus);
        loaded.Samus.InitializeAnimation(bus);
        ushort healthBeforeTouch = loaded.Samus.Health;
        if (!loaded.Enemies.ResolveOrdinarySamusContact(loaded.Samus, controllerInput: 0) ||
            loaded.Samus.Health >= healthBeforeTouch || !loaded.Samus.KnockbackActive)
        {
            throw new InvalidDataException(
                $"Golden Torizo contact failed: health {healthBeforeTouch}->{loaded.Samus.Health}, " +
                $"knockback={loaded.Samus.KnockbackActive}.");
        }

        VerifyPowerBombImmunity(bus, loaded);
        VerifyMissileAndSuperReactions(bus, loaded, assets.LevelData);

        var fatalShots = new SamusProjectileSystem();
        var bombs = new SamusBombProjectileSystem();
        loaded.Head.FlashTimer = 0;
        loaded.State.ShotGuard = 1; // Golden's nonzero guard explicitly takes common damage.
        ArmProjectile(fatalShots.Slots[0], loaded.Head, type: 0x0200, damage: 30000);
        int hits = loaded.Enemies.ResolveOrdinaryProjectileHits(
            bus,
            fatalShots,
            bombs,
            loaded.Samus);
        if (hits != 1 || loaded.Head.Health != 0 || !loaded.State.DeathStarted)
        {
            throw new InvalidDataException(
                $"Golden Torizo fatal shot mismatch: hits={hits}, health={loaded.Head.Health}, " +
                $"death={loaded.State.DeathStarted}.");
        }

        bool sawDeathMusic = false;
        for (int frame = 0; frame < 8000 && !loaded.State.BossBitSet; frame++)
        {
            Step(loaded, assets.LevelData);
            sawDeathMusic |= loaded.Enemies.LastBombTorizoMusicRequest is
                { Track: 3, DelayFrames: 8 };
        }
        if (!loaded.State.BossBitSet || !bossBitSet || !loaded.State.ItemDropRequested ||
            !sawDeathMusic)
        {
            throw new InvalidDataException(
                $"Golden Torizo death incomplete: boss={loaded.State.BossBitSet}/{bossBitSet}, " +
                $"drop={loaded.State.ItemDropRequested}, music={sawDeathMusic}.");
        }
    }

    private static void VerifyMissileAndSuperReactions(
        SuperMetroidAddressSpace bus,
        LoadedGoldenTorizo loaded,
        RoomLevelData level)
    {
        var shots = new SamusProjectileSystem();
        var bombs = new SamusBombProjectileSystem();
        loaded.Head.FlashTimer = 0;
        loaded.State.ShotGuard = 0;
        loaded.Head.Parameter2 &= 0xcfff;
        ushort healthBefore = loaded.Head.Health;

        ArmProjectile(shots.Slots[0], loaded.Head, type: 0x0100, damage: 100);
        int missileHits = loaded.Enemies.ResolveOrdinaryProjectileHits(
            bus,
            shots,
            bombs,
            loaded.Samus);
        if (missileHits != 1 || loaded.Head.Health != healthBefore ||
            loaded.State.CapturedProjectileFamily != 0x0100 ||
            (shots.Slots[0].Direction & 0x0010) != 0)
        {
            throw new InvalidDataException(
                $"Golden Torizo missile catch mismatch: hits={missileHits}, " +
                $"health={healthBefore}->{loaded.Head.Health}, family=" +
                $"${loaded.State.CapturedProjectileFamily:X4}, direction=" +
                $"${shots.Slots[0].Direction:X4}.");
        }

        loaded.Head.FlashTimer = 0;
        loaded.State.ShotGuard = 0;
        loaded.Head.Parameter2 &= 0xefff;
        loaded.Samus.XPosition = (loaded.Head.Parameter1 & 0x8000) != 0
            ? unchecked((ushort)(loaded.Head.XPosition + 32))
            : unchecked((ushort)(loaded.Head.XPosition - 32));
        shots = new SamusProjectileSystem();
        ArmProjectile(shots.Slots[0], loaded.Head, type: 0x0200, damage: 300);
        int superHits = loaded.Enemies.ResolveOrdinaryProjectileHits(
            bus,
            shots,
            bombs,
            loaded.Samus);
        if (superHits != 1 || loaded.Head.Health != healthBefore ||
            loaded.State.CapturedProjectileFamily != 0x0200 ||
            (loaded.Head.Parameter2 & 0x1000) == 0 ||
            (shots.Slots[0].Direction & 0x0010) == 0)
        {
            throw new InvalidDataException(
                $"Golden Torizo Super Missile catch mismatch: hits={superHits}, " +
                $"health={healthBefore}->{loaded.Head.Health}, family=" +
                $"${loaded.State.CapturedProjectileFamily:X4}, parameter2=" +
                $"${loaded.Head.Parameter2:X4}, direction=${shots.Slots[0].Direction:X4}.");
        }


        bool sawThrownSuper = false;
        for (int frame = 0; frame < 1200 && !sawThrownSuper; frame++)
        {
            Step(loaded, level);
            sawThrownSuper = loaded.Enemies.EnemyProjectiles.Any(
                projectile => projectile.Kind ==
                    RoomEnemyProjectileKind.GoldenTorizoSuperMissile);
        }
        if (!sawThrownSuper)
        {
            throw new InvalidDataException(
                "Golden Torizo caught a Super Missile but its ROM list never spawned the held actor.");
        }
    }

    /// <summary>
    /// Proves the private `$AA:C9C2` callback selected by transitional stand/sit hitboxes.
    /// Unlike ordinary `$C97C`, it reaches guarded common damage directly and therefore
    /// cannot catch missiles, reflect Supers, or arm Golden Torizo's counterattack flags.
    /// </summary>
    private static void VerifyStandUpSitDownShotCallback(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedGoldenTorizo loaded = Load(
            bus,
            room,
            assets,
            alreadyDefeated: false,
            () => { });
        if (!AdvanceToShotCallback(
                bus,
                loaded,
                assets.LevelData,
                StandUpSitDownShotCallback,
                out ushort shotX,
                out ushort shotY))
        {
            throw new InvalidDataException(
                "Golden Torizo never displayed an authored $AA:C9C2 shot hitbox.");
        }

        RoomEnemyDefinition definition = loaded.Head.Definition;
        ushort vulnerabilityPointer = definition.VulnerabilityPointer != 0
            ? definition.VulnerabilityPointer
            : DefaultEnemyVulnerability;
        byte superVulnerability = bus.ReadByte(
            0xb40000 | unchecked((ushort)(vulnerabilityPointer + 13)));
        int expectedDamage = (300 >> 1) * (superVulnerability & 0x7f);
        if (superVulnerability == 0xff || expectedDamage == 0)
        {
            throw new InvalidDataException(
                $"Golden Torizo's retail Super vulnerability ${superVulnerability:X2} " +
                "cannot prove $AA:C9C2 direct damage.");
        }

        loaded.Head.FlashTimer = 0;
        loaded.State.ShotGuard = 0;
        loaded.State.CapturedProjectileFamily = 0x5555;
        loaded.Head.Parameter2 &= 0xcfff;
        ushort healthBefore = loaded.Head.Health;
        var shots = new SamusProjectileSystem();
        var bombs = new SamusBombProjectileSystem();
        ArmProjectile(shots.Slots[0], loaded.Head, type: 0x8200, damage: 300);
        shots.Slots[0].XPosition = shotX;
        shots.Slots[0].YPosition = shotY;
        shots.Slots[0].XRadius = 1;
        shots.Slots[0].YRadius = 1;
        int hits = loaded.Enemies.ResolveOrdinaryProjectileHits(
            bus,
            shots,
            bombs,
            loaded.Samus);
        ushort expectedHealth = expectedDamage >= healthBefore
            ? (ushort)0
            : unchecked((ushort)(healthBefore - expectedDamage));
        if (hits != 1 || loaded.Head.Health != expectedHealth ||
            loaded.State.CapturedProjectileFamily != 0x5555 ||
            (loaded.Head.Parameter2 & 0x3000) != 0)
        {
            throw new InvalidDataException(
                $"Golden Torizo $C9C2 direct damage mismatch: hits={hits}, " +
                $"health={healthBefore}->{loaded.Head.Health} expected {expectedHealth}, " +
                $"captured=${loaded.State.CapturedProjectileFamily:X4}, " +
                $"parameter2=${loaded.Head.Parameter2:X4}.");
        }

        // `$D658` rejects the same hit while `toriz_var_04` is nonzero. The extended
        // collision walker still marks the Super as collided, but no impact or damage AI
        // may run and the projectile family must remain intact.
        loaded.Head.FlashTimer = 0;
        loaded.State.ShotGuard = 1;
        loaded.Head.Health = healthBefore;
        shots = new SamusProjectileSystem();
        ArmProjectile(shots.Slots[0], loaded.Head, type: 0x8200, damage: 300);
        shots.Slots[0].XPosition = shotX;
        shots.Slots[0].YPosition = shotY;
        shots.Slots[0].XRadius = 1;
        shots.Slots[0].YRadius = 1;
        hits = loaded.Enemies.ResolveOrdinaryProjectileHits(
            bus,
            shots,
            bombs,
            loaded.Samus);
        if (hits != 1 || loaded.Head.Health != healthBefore ||
            shots.Slots[0].Type != 0x8200 || (shots.Slots[0].Direction & 0x0010) == 0)
        {
            throw new InvalidDataException(
                $"Golden Torizo $C9C2 guard mismatch: hits={hits}, " +
                $"health={healthBefore}->{loaded.Head.Health}, projectile=" +
                $"${shots.Slots[0].Type:X4}/${shots.Slots[0].Direction:X4}.");
        }
    }

    private static bool AdvanceToShotCallback(
        ISnesAddressSpace bus,
        LoadedGoldenTorizo loaded,
        RoomLevelData level,
        ushort callback,
        out ushort shotX,
        out ushort shotY)
    {
        for (int frame = 0; frame < 30000; frame++)
        {
            if (RetailExtendedHitboxProbe.TryFindShotPoint(
                    bus,
                    loaded.Head,
                    callback,
                    out shotX,
                    out shotY))
            {
                return true;
            }
            Step(loaded, level);
        }

        shotX = 0;
        shotY = 0;
        return false;
    }

    private static void VerifyPowerBombImmunity(
        SuperMetroidAddressSpace bus,
        LoadedGoldenTorizo loaded)
    {
        loaded.Head.FlashTimer = 0;
        loaded.Head.InvincibilityTimer = 0;
        loaded.State.ShotGuard = 0;
        ushort healthBefore = loaded.Head.Health;
        int reactions = loaded.Enemies.ResolveOrdinaryPowerBombHits(
            bus,
            loaded.Head.XPosition,
            loaded.Head.YPosition,
            explosionRadius: 64,
            loaded.Samus);
        if (reactions != 0 || loaded.Head.Health != healthBefore ||
            loaded.Head.FlashTimer != 0 || loaded.Head.InvincibilityTimer != 0)
        {
            throw new InvalidDataException(
                $"Golden Torizo Power Bomb immunity mismatch: reactions={reactions}, " +
                $"health={healthBefore}->{loaded.Head.Health}, flash={loaded.Head.FlashTimer}, " +
                $"invincibility={loaded.Head.InvincibilityTimer}.");
        }
    }

    /// <summary>
    /// Proves that family <c>$0500</c> reaches shared hitbox callback <c>$AA:C97C</c>, which
    /// dispatches by area to Golden's header callback <c>$AA:D667</c>. Normal bombs are
    /// neither caught Missiles nor reflected Supers: they arm counterattack bit <c>$2000</c>
    /// and then enter common no-death vulnerability damage.
    /// </summary>
    private static void VerifyNormalBombReaction(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedGoldenTorizo loaded = Load(bus, room, assets, false, () => { });
        if (!AdvanceToShotCallback(
                bus,
                loaded,
                assets.LevelData,
                NormalBodyShotCallback,
                out ushort bombX,
                out ushort bombY))
        {
            throw new InvalidDataException(
                "Golden Torizo never displayed an authored $AA:C97C shot hitbox.");
        }

        loaded.Head.FlashTimer = 0;
        loaded.State.ShotGuard = 0;
        loaded.State.CapturedProjectileFamily = 0x5555;
        loaded.Head.Parameter2 &= 0xcfff;
        var bombs = new SamusBombProjectileSystem();
        var shots = new SamusProjectileSystem();
        SamusBombProjectileSlot bomb =
            EnemyProjectileAuditAssertions.ArmExplodingNormalBomb(
                bombs,
                bombX,
                bombY,
                damage: 100);
        bomb.XRadius = 1;
        bomb.YRadius = 1;
        ushort healthBefore = loaded.Head.Health;
        ushort vulnerabilityPointer = loaded.Head.Definition.VulnerabilityPointer != 0
            ? loaded.Head.Definition.VulnerabilityPointer
            : DefaultEnemyVulnerability;
        byte vulnerability = bus.ReadByte(
            0xb40000 | unchecked((ushort)(vulnerabilityPointer + 14)));
        int damage = (bomb.Damage >> 1) * (vulnerability & 0x7f);
        ushort expectedHealth = damage >= healthBefore
            ? (ushort)0
            : unchecked((ushort)(healthBefore - damage));
        int hits = loaded.Enemies.ResolveOrdinaryBombHits(bombs, shots, loaded.Samus);
        if (hits != 1 || (bomb.Direction & 0x0010) == 0 ||
            loaded.Head.Health != expectedHealth ||
            loaded.State.CapturedProjectileFamily != SamusBombProjectileSystem.NormalBombType ||
            (loaded.Head.Parameter2 & 0x2000) == 0 ||
            loaded.Head.Properties.HasAny(EnemyProperties.Deleted))
        {
            throw new InvalidDataException(
                $"Golden Torizo normal-bomb reaction mismatch: hits={hits}, " +
                $"direction=${bomb.Direction:X4}, health={loaded.Head.Health}/{expectedHealth}, " +
                $"vulnerability=${vulnerability:X2}, family=" +
                $"${loaded.State.CapturedProjectileFamily:X4}, " +
                $"parameter2=${loaded.Head.Parameter2:X4}, " +
                $"deleted={loaded.Head.Properties.HasAny(EnemyProperties.Deleted)}.");
        }
    }

    private static void VerifyAlreadyDefeatedLoad(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedGoldenTorizo loaded = Load(bus, room, assets, true, () => { });
        if (!loaded.Head.Properties.HasAny(EnemyProperties.Deleted) ||
            !loaded.State.BossBitSet)
        {
            throw new InvalidDataException("Defeated Golden Torizo did not delete during init.");
        }
    }

    private static LoadedGoldenTorizo Load(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        bool alreadyDefeated,
        Action setBossBit)
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState(0x1234);
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Missiles = 100,
            MaxMissiles = 100,
            SuperMissiles = 20,
            MaxSuperMissiles = 20,
            XPosition = 0x0180,
            YPosition = 0x0160,
            Pose = SamusState.FacingRightNormalPose,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);

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
            samus: samus,
            isAreaTorizoDefeated: () => alreadyDefeated,
            setAreaTorizoDefeated: setBossBit,
            isRoomPlmPresent: _ => false);

        RoomEnemySlot head = enemies.Slots[0];
        TorizoEnemyState state = enemies.GoldenTorizo ?? throw new InvalidDataException(
            "Golden Torizo load produced no typed state.");
        return new LoadedGoldenTorizo(enemies, samus, head, state);
    }

    private static void Step(LoadedGoldenTorizo loaded, RoomLevelData level)
    {
        loaded.Enemies.StepFrame(
            CameraX,
            CameraY,
            timeIsFrozen: false,
            loaded.Samus,
            level: level);
        loaded.Enemies.StepEnemyProjectiles(
            level,
            loaded.Samus,
            cameraX: CameraX,
            cameraY: CameraY);
    }

    private static void ArmProjectile(
        SamusProjectileSlot projectile,
        RoomEnemySlot target,
        ushort type,
        ushort damage)
    {
        projectile.ClearFields();
        projectile.Type = type;
        projectile.Damage = damage;
        projectile.Direction = (ushort)SamusProjectileDirection.Right;
        projectile.XPosition = target.XPosition;
        projectile.YPosition = target.YPosition;
        projectile.XRadius = 16;
        projectile.YRadius = 24;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private readonly record struct LoadedGoldenTorizo(
        RoomEnemySystem Enemies,
        SamusState Samus,
        RoomEnemySlot Head,
        TorizoEnemyState State);
}
