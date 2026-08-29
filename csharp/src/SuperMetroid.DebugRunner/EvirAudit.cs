using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>
/// ROM-backed audit for Evir / Mini-Draygon. Retail never authors these as independent
/// actors: each animal is exactly three adjacent population records (body, arms, spit).
/// The audit deliberately preserves that physical layout because every translated native
/// relative-slot read depends on it.
/// </summary>
internal static class EvirAudit
{
    private const ushort BodyDefinition = 0xe63f;
    private const ushort ProjectileDefinition = 0xe67f;
    private const ushort WestRoomHeader = 0xd461;
    private const ushort WestRoomState = 0xd46e;
    private const ushort WestPopulation = 0xda3d;
    private const ushort EastRoomHeader = 0xd4c2;
    private const ushort EastRoomState = 0xd4cf;
    private const ushort EastPopulation = 0xdad3;

    private static readonly EvirRecord[] WestRecords =
    [
        new(BodyDefinition, 0x01f8, 0x00a0, 0x0000, 0x2000, 0, 0, 0xf808),
        new(BodyDefinition, 0x01f8, 0x00a0, 0x0000, 0x2400, 0, 1, 0x0000),
        new(ProjectileDefinition, 0x01f8, 0x00a0, 0x0000, 0x2800, 0, 2, 0x0000),
        new(BodyDefinition, 0x02e0, 0x0078, 0x0000, 0x2000, 0, 0, 0xd00c),
        new(BodyDefinition, 0x02e0, 0x0078, 0x0000, 0x2400, 0, 1, 0x0000),
        new(ProjectileDefinition, 0x02e0, 0x0078, 0x0000, 0x2800, 0, 2, 0x0000),
        new(BodyDefinition, 0x0340, 0x00a0, 0x0000, 0x2000, 0, 0, 0xf808),
        new(BodyDefinition, 0x0340, 0x00a0, 0x0000, 0x2400, 0, 1, 0x0000),
        new(ProjectileDefinition, 0x0340, 0x00a0, 0x0000, 0x2800, 0, 2, 0x0000),
    ];

    private static readonly EvirRecord[] EastRecords =
    [
        new(BodyDefinition, 0x00a8, 0x00a0, 0x0000, 0x2000, 0, 0, 0xf808),
        new(BodyDefinition, 0x00a8, 0x00a0, 0x0000, 0x2400, 0, 1, 0x0000),
        new(ProjectileDefinition, 0x00a8, 0x00a0, 0x0000, 0x2800, 0, 2, 0x0000),
        new(BodyDefinition, 0x0100, 0x0078, 0x0000, 0x2000, 0, 0, 0xd00c),
        new(BodyDefinition, 0x0100, 0x0078, 0x0000, 0x2400, 0, 1, 0x0000),
        new(ProjectileDefinition, 0x0100, 0x0078, 0x0000, 0x2800, 0, 2, 0x0000),
        new(BodyDefinition, 0x0220, 0x0078, 0x0000, 0x2000, 0, 0, 0xd00c),
        new(BodyDefinition, 0x0220, 0x0078, 0x0000, 0x2400, 0, 1, 0x0000),
        new(ProjectileDefinition, 0x0220, 0x0078, 0x0000, 0x2800, 0, 2, 0x0000),
    ];

    // These are every map referenced by the four looping body/arms lists plus both spit
    // lists at $A8:86A7-$878D. Keeping literal ROM addresses makes omissions visible.
    private static readonly HashSet<ushort> ReferencedSpritemaps =
    [
        0x8b59, 0x8b88, 0x8bb7, 0x8be6, 0x8c15, 0x8c44,
        0x8ca2, 0x8cbd, 0x8cd8, 0x8cf3, 0x8d13,
        0x8d64, 0x8d7a,
        0x8d81, 0x8db0, 0x8ddf, 0x8e0e, 0x8e3d, 0x8e6c,
        0x8eca, 0x8ee5, 0x8f00, 0x8f1b, 0x8f3b,
    ];

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        VerifyDefinitionHeaders(bus);
        VerifyRoomAndPopulation(bus, WestRoomHeader, WestRoomState, WestPopulation, WestRecords);
        VerifyRoomAndPopulation(bus, EastRoomHeader, EastRoomState, EastPopulation, EastRecords);

        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, WestRoomHeader);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        VerifyLifecycleAndDrawing(bus, room, assets);
        VerifyCombatPropagation(bus, room, assets);

        Console.WriteLine(
            "Evir audit passed: both retail nine-record populations, body/arms following, " +
            "ROM speed-table bobbing, both facings, all 24 referenced maps, aimed spit, " +
            "far-offscreen regeneration bytecode/SFX, projectile RTL collision prelude, " +
            "OBJ drawing, contact damage, and " +
            "composite freeze/death propagation were verified from cartridge data.");
        return 0;
    }

    private static void VerifyDefinitionHeaders(ISnesAddressSpace bus)
    {
        RoomEnemyDefinition body = RoomEnemySystem.ReadDefinition(bus, BodyDefinition);
        RoomEnemyDefinition projectile = RoomEnemySystem.ReadDefinition(bus, ProjectileDefinition);
        if (body.TileDataSize != 0x0600 || body.PalettePointer != 0x8687 ||
            body.Health != 300 || body.Damage != 100 || body.XRadius != 0x10 ||
            body.YRadius != 0x14 || body.Bank != 0xa8 ||
            body.InitializationAiPointer != 0x87e0 || body.PartCount != 3 ||
            body.MainAiPointer != 0x891b || body.GrappleAiPointer != 0x800f ||
            body.HurtAiPointer != 0x804c || body.FrozenAiPointer != 0x8041 ||
            body.PowerBombReactionPointer != 0x8b0c || body.TouchAiPointer != 0x8b06 ||
            body.ShotAiPointer != 0x8b12 || body.Layer != 5 || body.DeathAnimation != 4)
        {
            throw new InvalidDataException(
                "Evir body $E63F header disagrees with bank-$A8 translation: " +
                $"size/palette=${body.TileDataSize:X4}/${body.PalettePointer:X4}, " +
                $"hp/damage={body.Health}/{body.Damage}, radius={body.XRadius}/{body.YRadius}, " +
                $"bank/init/main=${body.Bank:X2}:${body.InitializationAiPointer:X4}/" +
                $"${body.MainAiPointer:X4}, parts={body.PartCount}, grapple/hurt/frozen=" +
                $"${body.GrappleAiPointer:X4}/${body.HurtAiPointer:X4}/" +
                $"${body.FrozenAiPointer:X4}, PB/touch/shot=${body.PowerBombReactionPointer:X4}/" +
                $"${body.TouchAiPointer:X4}/${body.ShotAiPointer:X4}, " +
                $"layer/death={body.Layer}/{body.DeathAnimation}.");
        }

        if (projectile.TileDataSize != 0x0600 || projectile.PalettePointer != 0x8687 ||
            projectile.Health != 300 || projectile.Damage != 100 ||
            projectile.XRadius != 8 || projectile.YRadius != 8 || projectile.Bank != 0xa8 ||
            projectile.InitializationAiPointer != 0x88b0 || projectile.PartCount != 1 ||
            projectile.MainAiPointer != 0x899e || projectile.GrappleAiPointer != 0x800f ||
            projectile.HurtAiPointer != 0x804c || projectile.FrozenAiPointer != 0x8041 ||
            projectile.PowerBombReactionPointer != 0x804c ||
            projectile.TouchAiPointer != 0x8023 || projectile.ShotAiPointer != 0x804c ||
            projectile.Layer != 5 || projectile.DeathAnimation != 0)
        {
            throw new InvalidDataException(
                "Evir projectile $E67F header disagrees with bank-$A8 translation.");
        }
    }

    private static void VerifyRoomAndPopulation(
        ISnesAddressSpace bus,
        ushort roomPointer,
        ushort statePointer,
        ushort populationPointer,
        IReadOnlyList<EvirRecord> expected)
    {
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, roomPointer);
        if (room.State.Pointer != statePointer ||
            room.State.EnemyPopulationPointer != populationPointer)
        {
            throw new InvalidDataException(
                $"Room ${roomPointer:X4} selected state/population " +
                $"${room.State.Pointer:X4}/${room.State.EnemyPopulationPointer:X4}.");
        }

        var actual = new List<EvirRecord>();
        int cursor = 0xa10000 | populationPointer;
        for (int index = 0; index < expected.Count; index++, cursor += 16)
        {
            actual.Add(new EvirRecord(
                ReadWord(bus, cursor), ReadWord(bus, cursor + 2),
                ReadWord(bus, cursor + 4), ReadWord(bus, cursor + 6),
                ReadWord(bus, cursor + 8), ReadWord(bus, cursor + 10),
                ReadWord(bus, cursor + 12), ReadWord(bus, cursor + 14)));
        }
        if (!actual.SequenceEqual(expected) || ReadWord(bus, cursor) != 0xffff)
        {
            throw new InvalidDataException(
                $"Population ${populationPointer:X4} is not the exact retail three-triplet layout.");
        }
    }

    private static void VerifyLifecycleAndDrawing(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedEvir loaded = Load(bus, room, assets);
        RoomEnemySlot body = loaded.Enemies.Slots[0];
        RoomEnemySlot arms = loaded.Enemies.Slots[1];
        RoomEnemySlot projectile = loaded.Enemies.Slots[2];
        EvirEnemyState bodyState = RequireState(loaded.Enemies, body);
        EvirEnemyState armsState = RequireState(loaded.Enemies, arms);
        EvirEnemyState projectileState = RequireState(loaded.Enemies, projectile);

        if (loaded.Enemies.EnemyCount != 3 || body.CurrentInstruction != 0x86a7 ||
            arms.CurrentInstruction != 0x86c3 || projectile.CurrentInstruction != 0x876f ||
            bodyState.Function != EvirAiFunction.HandleBodyOrArms ||
            armsState.Function != EvirAiFunction.HandleBodyOrArms ||
            projectileState.Function != EvirAiFunction.ProjectileIdle ||
            bodyState.MovementTimer != 0 || arms.Layer != 4 ||
            arms.XPosition != body.XPosition - 4 || arms.YPosition != body.YPosition + 10 ||
            projectile.XPosition != body.XPosition - 4 ||
            projectile.YPosition != body.YPosition + 18 ||
            projectile.PaletteIndex != body.PaletteIndex ||
            projectile.VramTilesIndex != body.VramTilesIndex)
        {
            throw new InvalidDataException("Evir three-slot initialization contract failed.");
        }

        // P2=$F808 selects table entries +$40 and +$44 at $A0:8187. Comparing both words
        // proves the port reads the cartridge's signed 16.16 values instead of tuned doubles.
        if (bodyState.DownVelocity != unchecked((short)ReadWord(bus, 0xa081c7)) ||
            bodyState.DownSubvelocity != ReadWord(bus, 0xa081c9) ||
            bodyState.UpVelocity != unchecked((short)ReadWord(bus, 0xa081cb)) ||
            bodyState.UpSubvelocity != ReadWord(bus, 0xa081cd))
        {
            throw new InvalidDataException("Evir bobbing speed does not match ROM table entries.");
        }

        var maps = new HashSet<ushort>();
        ushort highestY = body.YPosition;
        ushort lowestY = body.YPosition;
        int frame = 0;

        // The buggy zero initial timer immediately selects the 248-frame downward half-cycle.
        // It eventually carries this actor outside the one-screen room's processing window,
        // so observe each facing from a fresh retail triplet rather than mutating properties.
        // The arms' distinct fifth map begins at frame 160; 170 frames reaches every map.
        loaded.Samus.XPosition = unchecked((ushort)(body.XPosition - 0x0100));
        for (; frame < 170; frame++)
        {
            Step(loaded, room, assets, frame);
            CollectMaps(maps, body, arms, projectile);
            highestY = Math.Min(highestY, body.YPosition);
            lowestY = Math.Max(lowestY, body.YPosition);
        }
        if (bodyState.FacingDirection != 0 || arms.XPosition != body.XPosition - 4 ||
            arms.YPosition != body.YPosition + 10)
        {
            throw new InvalidDataException("Left-facing Evir follower offsets diverged.");
        }

        loaded = Load(bus, room, assets);
        body = loaded.Enemies.Slots[0];
        arms = loaded.Enemies.Slots[1];
        projectile = loaded.Enemies.Slots[2];
        bodyState = RequireState(loaded.Enemies, body);
        armsState = RequireState(loaded.Enemies, arms);
        projectileState = RequireState(loaded.Enemies, projectile);
        loaded.Samus.XPosition = unchecked((ushort)(body.XPosition + 0x0100));
        frame = 0;
        for (; frame < 170; frame++)
        {
            Step(loaded, room, assets, frame);
            CollectMaps(maps, body, arms, projectile);
            highestY = Math.Min(highestY, body.YPosition);
            lowestY = Math.Max(lowestY, body.YPosition);
        }
        if (bodyState.FacingDirection != 1 || armsState.FacingDirection != 1 ||
            arms.XPosition != body.XPosition + 4 || arms.YPosition != body.YPosition + 10 ||
            highestY == lowestY)
        {
            throw new InvalidDataException(
                "Evir facing/follower/bobbing lifecycle did not advance: " +
                $"lists=${body.CurrentInstruction:X4}/${arms.CurrentInstruction:X4}, " +
                $"body=(${body.XPosition:X4},${body.YPosition:X4}), " +
                $"arms=(${arms.XPosition:X4},${arms.YPosition:X4}), " +
                $"Y=${highestY:X4}..${lowestY:X4}, speeds=" +
                $"{bodyState.DownVelocity}:{bodyState.DownSubvelocity:X4}/" +
                $"{bodyState.UpVelocity}:{bodyState.UpSubvelocity:X4}, " +
                $"timer/direction={bodyState.MovementTimer}/{bodyState.MovementDirection}.");
        }

        // Aim horizontally right so the exact result is easy to independently inspect:
        // the native angle routine and buggy multiplication tables still supply velocity.
        loaded = Load(bus, room, assets);
        body = loaded.Enemies.Slots[0];
        arms = loaded.Enemies.Slots[1];
        projectile = loaded.Enemies.Slots[2];
        bodyState = RequireState(loaded.Enemies, body);
        projectileState = RequireState(loaded.Enemies, projectile);
        frame = 0;
        loaded.Samus.XPosition = unchecked((ushort)(body.XPosition + 0x0040));
        loaded.Samus.YPosition = projectile.YPosition;
        Step(loaded, room, assets, frame++);
        if (projectileState.Function != EvirAiFunction.ProjectileMoving ||
            projectileState.MovingFlag == 0 || projectileState.XVelocity <= 0 ||
            projectileState.YVelocity != 0)
        {
            throw new InvalidDataException(
                $"Evir aimed spit failed: function={projectileState.Function}, velocity=" +
                $"{projectileState.XVelocity}:{projectileState.XSubvelocity:X4}," +
                $"{projectileState.YVelocity}:{projectileState.YSubvelocity:X4}.");
        }

        bool sawRegeneration = false;
        bool sawRegenerationMap = false;
        bool sawSpitSound = false;
        bool returnedIdle = false;
        for (; frame < 900; frame++)
        {
            Step(loaded, room, assets, frame);
            CollectMaps(maps, body, arms, projectile);
            sawRegeneration |= projectileState.Function == EvirAiFunction.ProjectileRegenerating;
            sawRegenerationMap |= projectile.SpritemapPointer == 0x8d64;
            sawSpitSound |= loaded.Enemies.LastEvirSoundEffect == 0x005e;
            returnedIdle |= sawRegeneration &&
                projectileState.Function == EvirAiFunction.ProjectileIdle;
            if (returnedIdle)
                break;
        }
        if (!sawRegeneration || !sawRegenerationMap || !sawSpitSound || !returnedIdle ||
            !maps.SetEquals(ReferencedSpritemaps))
        {
            throw new InvalidDataException(
                "Evir spit/regeneration coverage failed: " +
                $"regen/map/sound/idle={sawRegeneration}/{sawRegenerationMap}/" +
                $"{sawSpitSound}/{returnedIdle}, missing=[{FormatWords(ReferencedSpritemaps.Except(maps))}].");
        }

        var oam = new OamBuffer();
        oam.BeginFrame();
        loaded.Enemies.DrawLayers(oam, CameraX(room, body), CameraY(room, body), 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException("Evir's live ROM spritemaps emitted no OBJ pieces.");
    }

    private static void VerifyCombatPropagation(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedEvir noOpShot = Load(bus, room, assets);
        Step(noOpShot, room, assets, frame: 0);
        RoomEnemySlot noOpProjectile = noOpShot.Enemies.Slots[2];
        // While attached, the spit overlaps the larger body hitbox and bank-$A0 visits the
        // body first. Isolate the same live projectile record as it would be in flight so
        // this assertion reaches the projectile header's own literal-RTL callback.
        noOpProjectile.XPosition = unchecked((ushort)(noOpProjectile.XPosition + 0x0060));
        var passThroughShots = new SamusProjectileSystem();
        ArmProjectile(passThroughShots.Slots[0], noOpProjectile, damage: 1000);
        ushort noOpHealthBefore = noOpProjectile.Health;
        ushort instructionBefore = passThroughShots.Slots[0].InstructionPointer;
        int noOpHits = noOpShot.Enemies.ResolveOrdinaryProjectileHits(
            bus,
            passThroughShots,
            new SamusBombProjectileSystem(),
            noOpShot.Samus);
        if (noOpHits != 1 || noOpProjectile.Health != noOpHealthBefore ||
            passThroughShots.Slots[0].InstructionPointer != instructionBefore ||
            (passThroughShots.Slots[0].Direction & 0x0010) == 0)
        {
            throw new InvalidDataException(
                "Evir projectile's literal-RTL shot callback did not preserve the native " +
                "collision prelude without impact/damage.");
        }

        LoadedEvir contact = Load(bus, room, assets);
        RoomEnemySlot contactBody = contact.Enemies.Slots[0];
        Step(contact, room, assets, frame: 0);
        contact.Samus.XPosition = contactBody.XPosition;
        contact.Samus.YPosition = contactBody.YPosition;
        ushort healthBefore = contact.Samus.Health;
        if (!contact.Enemies.ResolveOrdinarySamusContact(contact.Samus, controllerInput: 0) ||
            contact.Samus.Health != healthBefore - 100)
        {
            throw new InvalidDataException("Evir body contact did not apply header damage 100.");
        }

        LoadedEvir death = Load(bus, room, assets);
        RoomEnemySlot body = death.Enemies.Slots[0];
        Step(death, room, assets, frame: 0);
        var projectiles = new SamusProjectileSystem();
        var sharedProjectiles = new SamusBombProjectileSystem();
        ArmProjectile(projectiles.Slots[0], body, damage: 1000);
        int hits = death.Enemies.ResolveOrdinaryProjectileHits(
            bus, projectiles, sharedProjectiles, death.Samus);
        if (hits != 1 || body.Health != 0 ||
            !death.Enemies.Slots[1].Properties.HasAny(EnemyProperties.Deleted) ||
            !death.Enemies.Slots[2].Properties.HasAny(EnemyProperties.Deleted))
        {
            throw new InvalidDataException(
                "Lethal Evir body shot did not remove its dependent arms and projectile.");
        }

        LoadedEvir frozen = Load(bus, room, assets);
        Step(frozen, room, assets, frame: 0);
        RoomEnemySlot frozenBody = frozen.Enemies.Slots[0];
        var ice = new SamusProjectileSystem();
        ArmProjectile(ice.Slots[0], frozenBody, damage: 20, type: 0x0002);
        int freezeHits = frozen.Enemies.ResolveOrdinaryProjectileHits(
            bus, ice, new SamusBombProjectileSystem(), frozen.Samus);
        if (freezeHits != 1 || frozenBody.FrozenTimer != 400 ||
            frozen.Enemies.Slots[1].FrozenTimer != 400 ||
            frozen.Enemies.Slots[2].FrozenTimer != 400)
        {
            throw new InvalidDataException(
                "Idle Evir freeze did not propagate to arms and the attached projectile: " +
                $"hits={freezeHits}, timers={frozenBody.FrozenTimer}/" +
                $"{frozen.Enemies.Slots[1].FrozenTimer}/" +
                $"{frozen.Enemies.Slots[2].FrozenTimer}, health={frozenBody.Health}.");
        }

        LoadedEvir flying = Load(bus, room, assets);
        RoomEnemySlot flyingBody = flying.Enemies.Slots[0];
        RoomEnemySlot flyingProjectile = flying.Enemies.Slots[2];
        flying.Samus.XPosition = unchecked((ushort)(flyingBody.XPosition + 0x0040));
        flying.Samus.YPosition = unchecked((ushort)(flyingBody.YPosition + 18));
        Step(flying, room, assets, frame: 0);
        if (RequireState(flying.Enemies, flyingProjectile).Function !=
            EvirAiFunction.ProjectileMoving)
        {
            throw new InvalidDataException("Moving-projectile freeze fixture did not launch.");
        }
        var movingIce = new SamusProjectileSystem();
        ArmProjectile(movingIce.Slots[0], flyingBody, damage: 20, type: 0x0002);
        int movingFreezeHits = flying.Enemies.ResolveOrdinaryProjectileHits(
            bus, movingIce, new SamusBombProjectileSystem(), flying.Samus);
        if (movingFreezeHits != 1 || flyingBody.FrozenTimer != 400 ||
            flying.Enemies.Slots[1].FrozenTimer != 400 || flyingProjectile.FrozenTimer != 0)
        {
            throw new InvalidDataException(
                "Evir incorrectly propagated body freeze to an already-flying projectile.");
        }
    }

    private static LoadedEvir Load(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        var prefixBus = new PopulationPrefixAddressSpace(
            bus, WestPopulation, retainedRecordCount: 3, deathQuota: 0);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState();
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusState.FacingRightNormalPose,
            XPosition = 0,
            YPosition = 0,
        };
        samus.RefreshCollisionRadii(prefixBus);
        samus.InitializeAnimation(prefixBus);

        var enemies = new RoomEnemySystem();
        enemies.Load(
            prefixBus,
            WestPopulation,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            random.NextRandom,
            random.SetRandomNumber,
            readRandomNumber: () => random.RandomNumber,
            level: assets.LevelData,
            samus: samus);
        return new LoadedEvir(enemies, samus);
    }

    private static void Step(
        LoadedEvir loaded,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        int frame)
    {
        RoomEnemySlot body = loaded.Enemies.Slots[0];
        loaded.Enemies.StepFrame(
            CameraX(room, body),
            CameraY(room, body),
            false,
            loaded.Samus,
            level: assets.LevelData,
            nmiFrameCounter8: unchecked((byte)frame));
    }

    private static void CollectMaps(
        ISet<ushort> maps,
        RoomEnemySlot body,
        RoomEnemySlot arms,
        RoomEnemySlot projectile)
    {
        if (ReferencedSpritemaps.Contains(body.SpritemapPointer))
            maps.Add(body.SpritemapPointer);
        if (ReferencedSpritemaps.Contains(arms.SpritemapPointer))
            maps.Add(arms.SpritemapPointer);
        if (ReferencedSpritemaps.Contains(projectile.SpritemapPointer))
            maps.Add(projectile.SpritemapPointer);
    }

    private static void ArmProjectile(
        SamusProjectileSlot projectile,
        RoomEnemySlot target,
        ushort damage,
        ushort type = 0x0100)
    {
        projectile.ClearFields();
        projectile.Type = type;
        projectile.Damage = damage;
        projectile.Direction = 2;
        projectile.XPosition = target.XPosition;
        projectile.YPosition = target.YPosition;
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    private static EvirEnemyState RequireState(RoomEnemySystem enemies, RoomEnemySlot slot) =>
        enemies.EvirStates[slot.SlotIndex] ?? throw new InvalidDataException(
            $"Evir slot {slot.SlotIndex} has no typed state.");

    private static ushort CameraX(CartridgeRoomHeader room, RoomEnemySlot actor) =>
        unchecked((ushort)Math.Clamp(
            actor.XPosition - 128,
            0,
            Math.Max(0, room.WidthInScreens * 256 - 256)));

    private static ushort CameraY(CartridgeRoomHeader room, RoomEnemySlot actor) =>
        unchecked((ushort)Math.Clamp(
            actor.YPosition - 112,
            0,
            Math.Max(0, room.HeightInScreens * 256 - 224)));

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private static string FormatWords(IEnumerable<ushort> words) =>
        string.Join(',', words.Order().Select(word => $"${word:X4}"));

    private readonly record struct EvirRecord(
        ushort Definition,
        ushort X,
        ushort Y,
        ushort InitializationParameter,
        ushort Properties,
        ushort ExtraProperties,
        ushort Parameter1,
        ushort Parameter2);

    private sealed record LoadedEvir(RoomEnemySystem Enemies, SamusState Samus);
}
