using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>
/// ROM-backed audit for the first Dragon body/wing pair in Norfair room $8F:AA0E. A
/// read-only address decorator terminates the retail population after those two unchanged
/// records; code, animation lists, spritemaps, projectiles, graphics, and room data remain
/// bytes from the user's cartridge image.
/// </summary>
internal static class DragonAudit
{
    private const ushort DefinitionPointer = 0xd4bf;
    private const ushort RoomPointer = 0xaa0e;
    private const ushort ExpectedStatePointer = 0xaa1b;
    private const ushort ExpectedPopulationPointer = 0xad8f;
    private const ushort CameraX = 0x0300;
    private const ushort CameraY = 0x0100;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace retailBus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        VerifyHeader(retailBus);
        VerifyAllRetailPopulationPairs(retailBus);

        CartridgeRoomHeader room = CartridgeRoomHeader.Load(
            retailBus,
            RoomPointer,
            new RoomStateSelectionContext(
                Array.Empty<byte>(),
                BossBits: 0,
                HasMorphBallAndMissiles: false,
                HasPowerBombs: false));
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(retailBus, room);
        if (room.State.Pointer != ExpectedStatePointer ||
            room.State.EnemyPopulationPointer != ExpectedPopulationPointer)
        {
            throw new InvalidDataException(
                $"Dragon room selection mismatch: state=${room.State.Pointer:X4}, " +
                $"population=${room.State.EnemyPopulationPointer:X4}.");
        }

        VerifyCompleteRoomPopulation(retailBus, room, assets);
        VerifyLoadAnimationMovementAndVolley(retailBus, room, assets);
        VerifyFireballArcDrawingAndDamage(retailBus, room, assets);
        VerifyBodyAndWeaponDamage(retailBus, room, assets);
        Console.WriteLine(
            "Dragon audit passed: all 38 retail records form body/wing pairs; right/left " +
            "facing, 49-pixel rise/sink movement, ROM attack loops, three-shot volleys, " +
            "signed 8.8 fireball arcs, projectile/contact damage, freeze mirroring, missile/" +
            "super damage, native wing power-bomb aliasing, death cleanup, and OBJ drawing agree.");
        return 0;
    }

    /// <summary>
    /// Loads $A1:AD8F without the focused prefix decorator. Its five Dragon pairs surround
    /// one Norfair-Rio body/follower pair, making it a useful proof that Dragon's +$40 aliases
    /// stay within each authored pair when several translated multipart actors share a room.
    /// </summary>
    private static void VerifyCompleteRoomPopulation(
        SuperMetroidAddressSpace retailBus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState();
        var samus = CreateSamus(retailBus, xPosition: 0x03c0);
        var enemies = new RoomEnemySystem();
        enemies.Load(
            retailBus,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            random.NextRandom,
            random.SetRandomNumber,
            readRandomNumber: () => random.RandomNumber,
            level: assets.LevelData,
            samus: samus,
            cameraX: CameraX,
            cameraY: CameraY);

        int[] expectedDragonSlots = [0, 1, 4, 5, 6, 7, 8, 9, 10, 11];
        if (enemies.EnemyCount != 12 ||
            expectedDragonSlots.Any(index =>
                enemies.Slots[index].EnemyDefinitionPointer != DefinitionPointer ||
                enemies.DragonStates[index] is null) ||
            enemies.Slots[2].EnemyDefinitionPointer != 0xd2ff ||
            enemies.Slots[3].EnemyDefinitionPointer != 0xd2ff)
        {
            throw new InvalidDataException(
                "Complete $A1:AD8F population did not load five Dragon pairs and its Norfair-Rio pair.");
        }

        enemies.StepFrame(
            CameraX,
            CameraY,
            timeIsFrozen: false,
            samus,
            level: assets.LevelData);
        if (GetState(enemies, enemies.Slots[0]).Function != DragonEnemyFunction.Rising)
        {
            throw new InvalidDataException(
                "Complete Dragon room failed to advance its visible lead pair into rising AI.");
        }
    }

    private static void VerifyLoadAnimationMovementAndVolley(
        SuperMetroidAddressSpace retailBus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedPair loaded = LoadPair(retailBus, room, assets, samusX: 0x03c0);
        RoomEnemySlot body = loaded.Body;
        RoomEnemySlot wing = loaded.Wing;
        DragonEnemyState bodyState = GetState(loaded.Enemies, body);
        DragonEnemyState wingState = GetState(loaded.Enemies, wing);

        if (loaded.Enemies.EnemyCount != 2 ||
            body.XPosition != 0x0380 || body.YPosition != 0x01e8 ||
            wing.XPosition != 0x0380 || wing.YPosition != 0x01e8 ||
            body.Health != 300 || wing.Health != 300 ||
            body.CurrentInstruction != 0xe59b || wing.CurrentInstruction != 0xe5a1 ||
            bodyState.Function != DragonEnemyFunction.WaitToRise ||
            wingState.Function != DragonEnemyFunction.WingNoOp ||
            bodyState.RequestedInstructionListIndex != 0 ||
            bodyState.InstalledInstructionListIndex != 0 ||
            wingState.RequestedInstructionListIndex != 2 ||
            wingState.InstalledInstructionListIndex != 2 ||
            body.Properties.HasAny(EnemyProperties.IgnoreSamusCollision) ||
            !wing.Properties.HasAny(EnemyProperties.IgnoreSamusCollision))
        {
            throw new InvalidDataException(
                $"Dragon initialization mismatch: count={loaded.Enemies.EnemyCount}, " +
                $"body=({body.XPosition:X4},{body.YPosition:X4})/" +
                $"${body.CurrentInstruction:X4}/${(ushort)bodyState.Function:X4}, " +
                $"wing=({wing.XPosition:X4},{wing.YPosition:X4})/" +
                $"${wing.CurrentInstruction:X4}/${(ushort)wingState.Function:X4}.");
        }

        // Variable D powers up as zero. DEC therefore underflows on the first call and the
        // signed BMI branch immediately begins the rise, exactly like the 65C816 routine.
        StepEnemies(loaded, assets);
        if (bodyState.Function != DragonEnemyFunction.Rising ||
            bodyState.FunctionTimer != 0x0030 || bodyState.DirectionWord != 0 ||
            bodyState.RequestedInstructionListIndex != 1 ||
            bodyState.InstalledInstructionListIndex != 1 ||
            wingState.RequestedInstructionListIndex != 3 ||
            wingState.InstalledInstructionListIndex != 3 ||
            body.SpritemapPointer != 0xe8c2 || wing.SpritemapPointer != 0xe96a ||
            body.YPosition != 0x01e8 || wing.YPosition != 0x01e8)
        {
            throw new InvalidDataException(
                $"Dragon right-facing setup mismatch: function=${(ushort)bodyState.Function:X4}, " +
                $"timer={bodyState.FunctionTimer}, lists=" +
                $"{bodyState.RequestedInstructionListIndex}/" +
                $"{wingState.RequestedInstructionListIndex}, maps=" +
                $"${body.SpritemapPointer:X4}/${wing.SpritemapPointer:X4}.");
        }

        // DEC/BPL applies displacement for timer values 47 through -1: 49 moves, not the
        // superficially tempting 48. The final mouth position makes that off-by-one visible.
        for (int frame = 0; frame < 49; frame++)
            StepEnemies(loaded, assets);
        if (bodyState.Function != DragonEnemyFunction.Attacking ||
            bodyState.AttackCounter != 3 ||
            bodyState.RequestedInstructionListIndex != 5 ||
            body.YPosition != 0x01b7 || wing.YPosition != 0x01b7)
        {
            throw new InvalidDataException(
                $"Dragon rise mismatch: function=${(ushort)bodyState.Function:X4}, " +
                $"counter={bodyState.AttackCounter}, list=" +
                $"{bodyState.RequestedInstructionListIndex}, Y=" +
                $"${body.YPosition:X4}/${wing.YPosition:X4}.");
        }

        StepEnemies(loaded, assets);
        if (bodyState.InstalledInstructionListIndex != 5 ||
            body.SpritemapPointer != 0xe8ec)
        {
            throw new InvalidDataException(
                $"Dragon attack-list mismatch: index=" +
                $"{bodyState.InstalledInstructionListIndex}, map=${body.SpritemapPointer:X4}.");
        }

        int volleyCount = 0;
        for (int frame = 0; frame < 200 && volleyCount < 3; frame++)
        {
            StepEnemies(loaded, assets);
            if (loaded.Enemies.LastDragonSoundEffect == 0x0061)
            {
                volleyCount++;
                if (loaded.Enemies.ActiveEnemyProjectileCount != volleyCount)
                {
                    throw new InvalidDataException(
                        $"Dragon volley {volleyCount} allocated " +
                        $"{loaded.Enemies.ActiveEnemyProjectileCount} projectile slots.");
                }
            }
        }

        if (volleyCount != 3 ||
            bodyState.Function != DragonEnemyFunction.WaitToSink ||
            bodyState.FunctionTimer != 0x0060 ||
            bodyState.RequestedInstructionListIndex != 1 ||
            bodyState.InstalledInstructionListIndex != 0xffff)
        {
            throw new InvalidDataException(
                $"Dragon volley mismatch: shots={volleyCount}, function=" +
                $"${(ushort)bodyState.Function:X4}, timer={bodyState.FunctionTimer}, " +
                $"requested/installed={bodyState.RequestedInstructionListIndex:X4}/" +
                $"{bodyState.InstalledInstructionListIndex:X4}.");
        }

        for (int frame = 0; frame < 96; frame++)
            StepEnemies(loaded, assets);
        if (bodyState.Function != DragonEnemyFunction.Sinking ||
            bodyState.FunctionTimer != 0x0030 ||
            bodyState.InstalledInstructionListIndex != 1)
        {
            throw new InvalidDataException(
                $"Dragon sink-delay mismatch: function=${(ushort)bodyState.Function:X4}, " +
                $"timer={bodyState.FunctionTimer}, list={bodyState.InstalledInstructionListIndex}.");
        }

        for (int frame = 0; frame < 49; frame++)
            StepEnemies(loaded, assets);
        if (bodyState.Function != DragonEnemyFunction.WaitToRise ||
            bodyState.FunctionTimer != 0x0080 ||
            body.YPosition != 0x01e8 || wing.YPosition != 0x01e8)
        {
            throw new InvalidDataException(
                $"Dragon sink mismatch: function=${(ushort)bodyState.Function:X4}, " +
                $"timer={bodyState.FunctionTimer}, Y=" +
                $"${body.YPosition:X4}/${wing.YPosition:X4}.");
        }
        VerifyDrawing(loaded.Enemies);

        // A separate left-side load proves the signed subtraction and even table entries.
        LoadedPair left = LoadPair(retailBus, room, assets, samusX: 0x0300);
        StepEnemies(left, assets);
        DragonEnemyState leftBody = GetState(left.Enemies, left.Body);
        DragonEnemyState leftWing = GetState(left.Enemies, left.Wing);
        if (leftBody.DirectionWord != 0x8000 ||
            leftBody.RequestedInstructionListIndex != 0 ||
            leftWing.RequestedInstructionListIndex != 2 ||
            left.Body.SpritemapPointer != 0xe80c || left.Wing.SpritemapPointer != 0xe8b4)
        {
            throw new InvalidDataException(
                $"Dragon left-facing mismatch: direction=${leftBody.DirectionWord:X4}, " +
                $"lists={leftBody.RequestedInstructionListIndex}/" +
                $"{leftWing.RequestedInstructionListIndex}, maps=" +
                $"${left.Body.SpritemapPointer:X4}/${left.Wing.SpritemapPointer:X4}.");
        }
    }

    private static void VerifyFireballArcDrawingAndDamage(
        SuperMetroidAddressSpace retailBus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedPair arc = LoadPair(retailBus, room, assets, samusX: 0x03c0);
        RoomEnemyProjectileSlot fireball = AdvanceToFirstFireball(arc, assets);
        ushort initialX = fireball.XPosition;
        ushort initialY = fireball.YPosition;
        if (fireball.Kind != RoomEnemyProjectileKind.DragonFireball ||
            fireball.XVelocity != 0x02c0 || fireball.YVelocity != 0xfc3f ||
            fireball.InstructionPointer != 0xb4cb || fireball.PreInstruction != 0xb535 ||
            fireball.XRadius != 2 || fireball.YRadius != 2 || fireball.Damage != 10 ||
            fireball.GraphicsIndex != (ushort)(arc.Body.PaletteIndex | arc.Body.VramTilesIndex))
        {
            throw new InvalidDataException(
                $"Dragon fireball initialization mismatch: kind=${(ushort)fireball.Kind:X4}, " +
                $"velocity=${fireball.XVelocity:X4}/${fireball.YVelocity:X4}, " +
                $"list/pre=${fireball.InstructionPointer:X4}/${fireball.PreInstruction:X4}, " +
                $"radius={fireball.XRadius}/{fireball.YRadius}, damage={fireball.Damage}.");
        }

        arc.Enemies.StepEnemyProjectiles(
            assets.LevelData,
            samus: null,
            cameraX: CameraX,
            cameraY: CameraY);
        if (fireball.XPosition != unchecked((ushort)(initialX + 2)) ||
            fireball.XSubposition != 0xc000 ||
            fireball.YPosition != unchecked((ushort)(initialY - 4)) ||
            fireball.YSubposition != 0x3f00 || fireball.YVelocity != 0xfc5f ||
            fireball.SpritemapPointer != ReadWord(retailBus, 0x86b4cd))
        {
            throw new InvalidDataException(
                $"Dragon fireball first motion mismatch: position=" +
                $"({fireball.XPosition:X4}.{fireball.XSubposition:X4}," +
                $"{fireball.YPosition:X4}.{fireball.YSubposition:X4}), velocity=" +
                $"${fireball.YVelocity:X4}, map=${fireball.SpritemapPointer:X4}.");
        }

        for (int frame = 1; frame < 31; frame++)
        {
            arc.Enemies.StepEnemyProjectiles(
                assets.LevelData,
                samus: null,
                cameraX: CameraX,
                cameraY: CameraY);
        }
        if (fireball.YVelocity != 0x001f ||
            fireball.InstructionPointer != 0xb4e7 ||
            fireball.SpritemapPointer != ReadWord(retailBus, 0x86b4e5))
        {
            throw new InvalidDataException(
                $"Dragon fireball apex mismatch: velocity=${fireball.YVelocity:X4}, " +
                $"list=${fireball.InstructionPointer:X4}, map=${fireball.SpritemapPointer:X4}.");
        }

        var oam = new OamBuffer();
        oam.BeginFrame();
        arc.Enemies.DrawEnemyProjectiles(oam, CameraX, CameraY);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException("Dragon fireball emitted no ROM OBJ pieces.");

        for (int frame = 0; frame < 500 && fireball.IsActive; frame++)
        {
            arc.Enemies.StepEnemyProjectiles(
                assets.LevelData,
                samus: null,
                cameraX: CameraX,
                cameraY: CameraY);
        }
        if (fireball.IsActive)
            throw new InvalidDataException("Dragon fireball never reached its bottom-only camera cull.");

        LoadedPair hit = LoadPair(retailBus, room, assets, samusX: 0x03c0);
        fireball = AdvanceToFirstFireball(hit, assets);
        hit.Samus.Health = 999;
        hit.Samus.InvincibilityTimer = 0;
        hit.Samus.KnockbackActive = false;
        hit.Samus.XPosition = unchecked((ushort)(fireball.XPosition + 2));
        hit.Samus.YPosition = unchecked((ushort)(fireball.YPosition - 4));
        hit.Enemies.StepEnemyProjectiles(
            assets.LevelData,
            hit.Samus,
            controllerInput: 0,
            cameraX: CameraX,
            cameraY: CameraY);
        if (hit.Samus.Health != 989 || !hit.Samus.KnockbackActive || fireball.IsActive)
        {
            throw new InvalidDataException(
                $"Dragon fireball contact mismatch: health={hit.Samus.Health}, " +
                $"knockback={hit.Samus.KnockbackActive}, active={fireball.IsActive}.");
        }
    }

    private static void VerifyBodyAndWeaponDamage(
        SuperMetroidAddressSpace retailBus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedPair contact = LoadPair(retailBus, room, assets, samusX: 0x03c0);
        StepEnemies(contact, assets);
        contact.Samus.Health = 999;
        contact.Samus.InvincibilityTimer = 0;
        contact.Samus.KnockbackActive = false;
        contact.Samus.XPosition = contact.Body.XPosition;
        contact.Samus.YPosition = contact.Body.YPosition;
        if (!contact.Enemies.ResolveOrdinarySamusContact(contact.Samus, controllerInput: 0) ||
            contact.Samus.Health != 975 || !contact.Samus.KnockbackActive ||
            contact.Body.Health != 300)
        {
            throw new InvalidDataException(
                $"Dragon body contact mismatch: Samus health={contact.Samus.Health}, " +
                $"knockback={contact.Samus.KnockbackActive}, body health={contact.Body.Health}.");
        }

        LoadedPair frozen = LoadPair(retailBus, room, assets, samusX: 0x03c0);
        StepEnemies(frozen, assets);
        int freezeHits = FireProjectile(retailBus, frozen, projectileType: 0x0002, damage: 20);
        if (freezeHits != 1 || frozen.Body.FrozenTimer != 400 ||
            frozen.Wing.FrozenTimer != 400 || frozen.Body.InvincibilityTimer != 10 ||
            frozen.Wing.InvincibilityTimer != 10 ||
            frozen.Body.AiHandlerBits != frozen.Wing.AiHandlerBits)
        {
            throw new InvalidDataException(
                $"Dragon freeze mirror mismatch: hits={freezeHits}, frozen=" +
                $"{frozen.Body.FrozenTimer}/{frozen.Wing.FrozenTimer}, invincibility=" +
                $"{frozen.Body.InvincibilityTimer}/{frozen.Wing.InvincibilityTimer}, AI=" +
                $"${frozen.Body.AiHandlerBits:X4}/${frozen.Wing.AiHandlerBits:X4}.");
        }

        LoadedPair missile = LoadPair(retailBus, room, assets, samusX: 0x03c0);
        StepEnemies(missile, assets);
        int missileHits = FireProjectile(
            retailBus,
            missile,
            projectileType: (ushort)SamusProjectileFamily.Missile,
            damage: 100);
        if (missileHits != 1 || missile.Body.Health != 200 ||
            missile.Wing.Health != 300 || missile.Body.FlashTimer != missile.Wing.FlashTimer ||
            missile.Body.InvincibilityTimer != missile.Wing.InvincibilityTimer)
        {
            throw new InvalidDataException(
                $"Dragon missile mismatch: hits={missileHits}, health=" +
                $"{missile.Body.Health}/{missile.Wing.Health}, flash=" +
                $"{missile.Body.FlashTimer}/{missile.Wing.FlashTimer}, invincibility=" +
                $"{missile.Body.InvincibilityTimer}/{missile.Wing.InvincibilityTimer}.");
        }

        LoadedPair killed = LoadPair(retailBus, room, assets, samusX: 0x03c0);
        StepEnemies(killed, assets);
        int superHits = FireProjectile(
            retailBus,
            killed,
            projectileType: (ushort)SamusProjectileFamily.SuperMissile,
            damage: 300);
        if (superHits != 1 || killed.Body.Health != 0 ||
            !killed.Body.Properties.HasAny(EnemyProperties.Deleted) ||
            !killed.Wing.Properties.HasAny(EnemyProperties.Deleted) ||
            killed.Enemies.EnemiesKilled != 1)
        {
            throw new InvalidDataException(
                $"Dragon super-missile death mismatch: hits={superHits}, health=" +
                $"{killed.Body.Health}, deleted=" +
                $"{killed.Body.Properties.HasAny(EnemyProperties.Deleted)}/" +
                $"{killed.Wing.Properties.HasAny(EnemyProperties.Deleted)}, " +
                $"kills={killed.Enemies.EnemiesKilled}.");
        }

        LoadedPair bombed = LoadPair(retailBus, room, assets, samusX: 0x03c0);
        int reactions = bombed.Enemies.ResolveOrdinaryPowerBombHits(
            retailBus,
            bombed.Body.XPosition,
            bombed.Body.YPosition,
            explosionRadius: 64);
        RoomEnemySlot followingClearSlot = bombed.Enemies.Slots[2];
        if (reactions != 2 || bombed.Body.Health != 100 || bombed.Wing.Health != 100 ||
            bombed.Body.InvincibilityTimer != 48 || bombed.Wing.InvincibilityTimer != 48 ||
            followingClearSlot.InvincibilityTimer != 48 ||
            !bombed.Body.Properties.HasAny(EnemyProperties.ProcessOffScreen) ||
            !bombed.Wing.Properties.HasAny(EnemyProperties.ProcessOffScreen))
        {
            throw new InvalidDataException(
                $"Dragon power-bomb alias mismatch: reactions={reactions}, health=" +
                $"{bombed.Body.Health}/{bombed.Wing.Health}, invincibility=" +
                $"{bombed.Body.InvincibilityTimer}/{bombed.Wing.InvincibilityTimer}/" +
                $"{followingClearSlot.InvincibilityTimer}, offscreen=" +
                $"{bombed.Body.Properties.HasAny(EnemyProperties.ProcessOffScreen)}/" +
                $"{bombed.Wing.Properties.HasAny(EnemyProperties.ProcessOffScreen)}.");
        }
    }

    private static RoomEnemyProjectileSlot AdvanceToFirstFireball(
        LoadedPair loaded,
        CartridgeRoomAssets assets)
    {
        for (int frame = 0; frame < 160; frame++)
        {
            StepEnemies(loaded, assets);
            if (loaded.Enemies.LastDragonSoundEffect == 0x0061)
            {
                return loaded.Enemies.EnemyProjectiles.Single(projectile =>
                    projectile.Kind == RoomEnemyProjectileKind.DragonFireball);
            }
        }
        throw new InvalidDataException("Dragon did not spawn its first fireball within 160 frames.");
    }

    private static int FireProjectile(
        ISnesAddressSpace bus,
        LoadedPair loaded,
        ushort projectileType,
        ushort damage)
    {
        var projectiles = new SamusProjectileSystem();
        var sharedProjectiles = new SamusBombProjectileSystem();
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
        return loaded.Enemies.ResolveOrdinaryProjectileHits(
            bus,
            projectiles,
            sharedProjectiles,
            loaded.Samus);
    }

    private static LoadedPair LoadPair(
        SuperMetroidAddressSpace retailBus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        ushort samusX)
    {
        var pairBus = new PopulationPrefixAddressSpace(
            retailBus,
            room.State.EnemyPopulationPointer,
            retainedRecordCount: 2,
            deathQuota: 1);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState();
        SamusState samus = CreateSamus(retailBus, samusX);

        var enemies = new RoomEnemySystem();
        enemies.Load(
            pairBus,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            random.NextRandom,
            random.SetRandomNumber,
            readRandomNumber: () => random.RandomNumber,
            level: assets.LevelData,
            samus: samus,
            cameraX: CameraX,
            cameraY: CameraY);
        return new LoadedPair(enemies, enemies.Slots[0], enemies.Slots[1], samus);
    }

    private static SamusState CreateSamus(ISnesAddressSpace bus, ushort xPosition)
    {
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusPoseIds.FacingRightNormalPose,
            XPosition = xPosition,
            YPosition = 0x01c0,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        return samus;
    }

    private static void StepEnemies(LoadedPair loaded, CartridgeRoomAssets assets) =>
        loaded.Enemies.StepFrame(
            CameraX,
            CameraY,
            timeIsFrozen: false,
            loaded.Samus,
            level: assets.LevelData);

    private static DragonEnemyState GetState(RoomEnemySystem enemies, RoomEnemySlot slot) =>
        enemies.DragonStates[slot.SlotIndex] ?? throw new InvalidDataException(
            $"Retail Dragon slot {slot.SlotIndex} did not receive typed state.");

    private static void VerifyDrawing(RoomEnemySystem enemies)
    {
        var oam = new OamBuffer();
        oam.BeginFrame();
        enemies.DrawLayers(oam, CameraX, CameraY, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount < 2)
            throw new InvalidDataException("Dragon body/wing pair emitted fewer than two OBJ pieces.");
    }

    private static void VerifyAllRetailPopulationPairs(ISnesAddressSpace bus)
    {
        ushort[] populations = [0xa6a8, 0xad8f, 0xafea, 0xb32c, 0xb5e7];
        int dragonRecords = 0;
        foreach (ushort population in populations)
        {
            var records = new List<(ushort Definition, ushort Parameter1)>();
            int cursor = population;
            for (int guard = 0; guard < RoomEnemySystem.MaximumEnemyCount; guard++)
            {
                ushort definition = ReadWord(bus, 0xa10000 | cursor);
                if (definition == 0xffff)
                    break;
                records.Add((definition, ReadWord(bus, 0xa10000 | (cursor + 12))));
                cursor = unchecked((ushort)(cursor + 16));
            }

            for (int index = 0; index < records.Count; index++)
            {
                if (records[index].Definition != DefinitionPointer)
                    continue;
                dragonRecords++;
                bool validBody = records[index].Parameter1 == 0 &&
                    index + 1 < records.Count && records[index + 1].Definition == DefinitionPointer &&
                    records[index + 1].Parameter1 != 0;
                bool validWing = records[index].Parameter1 != 0 && index > 0 &&
                    records[index - 1].Definition == DefinitionPointer &&
                    records[index - 1].Parameter1 == 0;
                if (!validBody && !validWing)
                {
                    throw new InvalidDataException(
                        $"Retail Dragon record {index} in $A1:{population:X4} is not a body/wing pair.");
                }
            }
        }

        if (dragonRecords != 38)
        {
            throw new InvalidDataException(
                $"Retail Dragon inventory contains {dragonRecords} records instead of 38.");
        }
    }

    private static void VerifyHeader(ISnesAddressSpace bus)
    {
        RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(bus, DefinitionPointer);
        if (definition.TileDataSize != 0x0600 || definition.PalettePointer != 0xe57b ||
            definition.Health != 300 || definition.Damage != 24 ||
            definition.XRadius != 8 || definition.YRadius != 0x001c ||
            definition.Bank != 0xa2 || definition.HurtAiTime != 0 ||
            definition.HurtSoundEffect != 0x0036 || definition.BossId != 0 ||
            definition.InitializationAiPointer != 0xe606 || definition.PartCount != 2 ||
            definition.MainAiPointer != 0xe64e || definition.GrappleAiPointer != 0x800f ||
            definition.HurtAiPointer != 0x804c || definition.FrozenAiPointer != 0x8041 ||
            definition.TimeFrozenAiPointer != 0 || definition.DeathAnimation != 4 ||
            definition.PowerBombReactionPointer != 0xe7d4 || definition.VariantIndex != 0 ||
            definition.TouchAiPointer != 0xe7c8 || definition.ShotAiPointer != 0xe7ce ||
            definition.InitialSpritemapPointer != 0 ||
            definition.TileDataAddress != 0xaea000 || definition.Layer != 5 ||
            definition.ItemDropChancesPointer != 0xf368 ||
            definition.VulnerabilityPointer != 0xee16 || definition.NamePointer != 0xde15)
        {
            throw new InvalidDataException(
                $"Retail Dragon header mismatch: size=${definition.TileDataSize:X4}, " +
                $"palette=${definition.PalettePointer:X4}, health/damage=" +
                $"{definition.Health}/{definition.Damage}, radius={definition.XRadius}/" +
                $"{definition.YRadius}, bank=${definition.Bank:X2}, hurt=" +
                $"{definition.HurtAiTime}/${definition.HurtSoundEffect:X4}, init/main=" +
                $"${definition.InitializationAiPointer:X4}/${definition.MainAiPointer:X4}, " +
                $"parts={definition.PartCount}, grapple/hurt/frozen=" +
                $"${definition.GrappleAiPointer:X4}/${definition.HurtAiPointer:X4}/" +
                $"${definition.FrozenAiPointer:X4}, death={definition.DeathAnimation}, " +
                $"PB/touch/shot=${definition.PowerBombReactionPointer:X4}/" +
                $"${definition.TouchAiPointer:X4}/${definition.ShotAiPointer:X4}, " +
                $"tiles=${definition.TileDataAddress:X6}, layer={definition.Layer}, " +
                $"drops/vulnerability/name=${definition.ItemDropChancesPointer:X4}/" +
                $"${definition.VulnerabilityPointer:X4}/${definition.NamePointer:X4}.");
        }
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private readonly record struct LoadedPair(
        RoomEnemySystem Enemies,
        RoomEnemySlot Body,
        RoomEnemySlot Wing,
        SamusState Samus);
}
