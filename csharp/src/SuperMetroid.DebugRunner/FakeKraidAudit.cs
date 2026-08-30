using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>
/// ROM-backed audit for Fake Kraid/Mini-Kraid definition $E0FF. The loader starts at the
/// room's untouched population pointer, proving that the three preceding walking Space
/// Pirates and Fake Kraid can coexist through the real room initialization path.
/// </summary>
internal static class FakeKraidAudit
{
    private const ushort RoomPointer = 0xa521;
    private const ushort Definition = 0xe0ff;
    private const int FakeKraidPopulationRecordIndex = 3;
    private const ushort CameraX = 0x0500;

    private static readonly ushort[] ExpectedLeftBodyMaps =
        [0x9c64, 0x9cb6, 0x9d08, 0x9d5a, 0x9dac, 0x9dfe, 0x9e50];

    private static readonly ushort[] ExpectedRightBodyMaps =
        [0x9ea2, 0x9ef4, 0x9f46, 0x9f98, 0x9fea, 0xa03c, 0xa08e];

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, RoomPointer);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);

        VerifyRetailRecords(bus, room);
        VerifyInitialization(bus, room, assets);
        SweepResult right = VerifyAnimationMovementProjectilesAndDrawing(
            bus,
            room,
            assets,
            samusX: 0x0560,
            expectedFacing: 4,
            ExpectedRightBodyMaps,
            RoomEnemyProjectileKind.FakeKraidSpikeRight,
            expectedSpikeMap: 0x842e);
        SweepResult left = VerifyAnimationMovementProjectilesAndDrawing(
            bus,
            room,
            assets,
            samusX: 0x0500,
            expectedFacing: -4,
            ExpectedLeftBodyMaps,
            RoomEnemyProjectileKind.FakeKraidSpikeLeft,
            expectedSpikeMap: 0x8427);
        VerifyCombat(bus, room, assets);

        Console.WriteLine(
            "Fake Kraid audit passed: exact retail record/header initialized; both " +
            $"facings covered all 14 body maps, walked ${right.MinimumX:X4}-${right.MaximumX:X4} " +
            $"and ${left.MinimumX:X4}-${left.MaximumX:X4}, spawned " +
            $"{right.Spikes + left.Spikes} spikes and {right.Spit + left.Spit} spit actors " +
            "from all three ROM projectile definitions, emitted OBJ pieces and both native " +
            "sounds, damaged Samus through body/spit/spike contact, accepted beam, normal-" +
            "bomb, and power-bomb damage, published the Mini-Kraid death-drop request, and cancelled " +
            "grapple exactly as its header specifies.");
        return 0;
    }

    private static void VerifyRetailRecords(
        ISnesAddressSpace bus,
        CartridgeRoomHeader room)
    {
        if (room.State.Pointer != 0xa533 ||
            room.State.EnemyPopulationPointer != 0xa0ba ||
            room.State.EnemyTilesetPointer != 0x8651 ||
            room.WidthInScreens != 6 || room.HeightInScreens != 1 || room.AreaIndex != 1)
        {
            throw new InvalidDataException(
                $"Fake Kraid room mismatch: state=${room.State.Pointer:X4}, population=" +
                $"${room.State.EnemyPopulationPointer:X4}, tileset=" +
                $"${room.State.EnemyTilesetPointer:X4}, size={room.WidthInScreens}x" +
                $"{room.HeightInScreens}, area={room.AreaIndex}.");
        }

        int population = 0xa10000 | room.State.EnemyPopulationPointer;
        for (int record = 0; record < FakeKraidPopulationRecordIndex; record++)
        {
            int address = population + record * 16;
            if (ReadWord(bus, address) != 0xf693 ||
                ReadWord(bus, address + 4) != 0x00a0 ||
                ReadWord(bus, address + 8) != 0x2000)
            {
                throw new InvalidDataException(
                    $"Expected walking Space Pirate in population record {record}.");
            }
        }

        int fakeKraidRecord = population + FakeKraidPopulationRecordIndex * 16;
        ushort[] expectedPopulation = [0xe0ff, 0x0530, 0x00a0, 0, 0x2800, 0, 0, 0];
        if (Enumerable.Range(0, expectedPopulation.Length).Any(
                word => ReadWord(bus, fakeKraidRecord + word * 2) != expectedPopulation[word]) ||
            ReadWord(bus, fakeKraidRecord + 16) != 0xffff)
        {
            throw new InvalidDataException("Fake Kraid's retail population record changed.");
        }

        RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(bus, Definition);
        if (definition.TileDataSize != 0x1000 || definition.PalettePointer != 0x998c ||
            definition.Health != 400 || definition.Damage != 100 ||
            definition.XRadius != 32 || definition.YRadius != 24 ||
            definition.Bank != 0xa6 || definition.InitializationAiPointer != 0x9a58 ||
            definition.PartCount != 1 || definition.MainAiPointer != 0x9ac2 ||
            definition.GrappleAiPointer != 0x800f || definition.HurtAiPointer != 0x804c ||
            definition.FrozenAiPointer != 0x8041 || definition.DeathAnimation != 3 ||
            definition.PowerBombReactionPointer != 0x9c39 ||
            definition.TouchAiPointer != 0x9c22 || definition.ShotAiPointer != 0x9c39 ||
            definition.TileDataAddress != 0xab8000 || definition.Layer != 5 ||
            definition.ItemDropChancesPointer != 0xf2c0 ||
            definition.VulnerabilityPointer != 0xeffa || definition.NamePointer != 0xe0df)
        {
            throw new InvalidDataException("Fake Kraid enemy header did not match $A0:E0FF.");
        }

        // These sentinels cover every custom instruction opcode, list family, launch
        // velocity, and projectile definition consumed by the translation.
        VerifyWords(bus, 0xa699ac,
            [0x9b74, 0x0010, 0x9c64, 0x000c, 0x9cb6, 0x0008, 0x9d08, 0x000c]);
        VerifyWords(bus, 0xa699dc,
            [0x0010, 0x9dac, 0x9bb2, 0x0008, 0x9dfe, 0x9bc4, 0x0010, 0x9e50]);
        VerifyWords(bus, 0xa69a2a,
            [0x0010, 0x9fea, 0x9bb2, 0x0008, 0xa03c, 0x9c02, 0x0010, 0xa08e]);
        VerifyWords(bus, 0xa69a48,
            [0xfe00, 0xfb00, 0xfc00, 0xfb00, 0x0200, 0xfb00, 0x0400, 0xfb00]);
        VerifyWords(bus, 0x869db0,
            [0x9dec, 0x9e1e, 0x9dda, 0x0404, 0x0014, 0x0000, 0x84fc]);
        VerifyWords(bus, 0x869dbe,
            [0x9e46, 0x9e83, 0x9de0, 0x0204, 0x0006, 0x0000, 0x84fc]);
        VerifyWords(bus, 0x869dcc,
            [0x9e4b, 0x9e83, 0x9de6, 0x0204, 0x0006, 0x0000, 0x84fc]);
        VerifyWords(bus, 0x869dda, [0x7fff, 0x8420, 0x8159]);
        VerifyWords(bus, 0x869de0, [0x7fff, 0x8427, 0x8159]);
        VerifyWords(bus, 0x869de6, [0x7fff, 0x842e, 0x8159]);
    }

    private static void VerifyInitialization(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedFakeKraid loaded = Load(bus, room, assets, samusX: 0x0560);
        RoomEnemySlot actor = loaded.Actor;
        FakeKraidEnemyState state = loaded.State;
        if (loaded.Enemies.EnemyCount != 4 || actor.EnemyDefinitionPointer != Definition ||
            actor.XPosition != 0x0530 || actor.YPosition != 0x00a0 ||
            actor.Properties != 0x2800 || actor.Health != 400 ||
            actor.CurrentInstruction != 0x99fc || actor.InstructionTimer != 1 ||
            state.WalkDelta != 4 || state.FacingDelta != 4 ||
            state.WalkStepTimer != 3 || state.SpitDecisionTimer != 3 ||
            !state.SpikeTimers.SequenceEqual([(ushort)67, (ushort)99, (ushort)51]) ||
            state.SpikeTimerByteOffset != 0 || state.SpawnedSpikeCount != 0 ||
            state.SpawnedSpitCount != 0)
        {
            throw new InvalidDataException(
                $"Fake Kraid initialization failed: actor=(${actor.XPosition:X4}," +
                $"${actor.YPosition:X4}) list=${actor.CurrentInstruction:X4}, " +
                $"walk/facing={state.WalkDelta}/{state.FacingDelta}, clocks=" +
                $"{state.WalkStepTimer}/{state.SpitDecisionTimer}/" +
                $"{string.Join('/', state.SpikeTimers)}.");
        }
    }

    private static SweepResult VerifyAnimationMovementProjectilesAndDrawing(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        ushort samusX,
        short expectedFacing,
        IReadOnlyCollection<ushort> expectedBodyMaps,
        RoomEnemyProjectileKind expectedSpikeKind,
        ushort expectedSpikeMap)
    {
        LoadedFakeKraid loaded = Load(bus, room, assets, samusX);
        loaded.Samus.InvincibilityTimer = ushort.MaxValue;
        var bodyMaps = new HashSet<ushort>();
        var projectileMaps = new HashSet<ushort>();
        var projectileKinds = new HashSet<RoomEnemyProjectileKind>();
        ushort minimumX = loaded.Actor.XPosition;
        ushort maximumX = loaded.Actor.XPosition;
        bool sawSpitSound = false;
        bool sawSpikeSound = false;

        for (int frame = 0; frame < 1000; frame++)
        {
            loaded.Enemies.StepFrame(
                CameraX,
                cameraY: 0,
                timeIsFrozen: false,
                loaded.Samus,
                level: assets.LevelData);
            loaded.Enemies.StepEnemyProjectiles(
                assets.LevelData,
                loaded.Samus,
                cameraX: CameraX,
                cameraY: 0);

            bodyMaps.Add(loaded.Actor.SpritemapPointer);
            minimumX = Math.Min(minimumX, loaded.Actor.XPosition);
            maximumX = Math.Max(maximumX, loaded.Actor.XPosition);
            sawSpitSound |= loaded.Enemies.LastFakeKraidSoundEffect == 0x0016;
            sawSpikeSound |= loaded.Enemies.LastFakeKraidSoundEffect == 0x003f;
            foreach (RoomEnemyProjectileSlot projectile in loaded.Enemies.EnemyProjectiles
                         .Where(projectile => projectile.IsActive))
            {
                projectileKinds.Add(projectile.Kind);
                projectileMaps.Add(projectile.SpritemapPointer);
            }
        }

        if (loaded.State.FacingDelta != expectedFacing ||
            !bodyMaps.SetEquals(expectedBodyMaps) || minimumX == maximumX ||
            !projectileKinds.Contains(RoomEnemyProjectileKind.FakeKraidSpit) ||
            !projectileKinds.Contains(expectedSpikeKind) ||
            !projectileMaps.Contains(0x8420) || !projectileMaps.Contains(expectedSpikeMap) ||
            loaded.State.SpawnedSpikeCount == 0 || loaded.State.SpawnedSpitCount == 0 ||
            !sawSpitSound || !sawSpikeSound)
        {
            throw new InvalidDataException(
                $"Fake Kraid facing {expectedFacing} sweep failed: x=${minimumX:X4}-" +
                $"${maximumX:X4}, maps={string.Join(',', bodyMaps.Order())}, projectile " +
                $"kinds={string.Join(',', projectileKinds)}, projectile maps=" +
                $"{string.Join(',', projectileMaps.Order())}, spawned=" +
                $"{loaded.State.SpawnedSpikeCount}/{loaded.State.SpawnedSpitCount}, sounds=" +
                $"{sawSpikeSound}/{sawSpitSound}.");
        }

        var oam = new OamBuffer();
        oam.BeginFrame();
        loaded.Enemies.DrawEnemyProjectiles(oam, CameraX, 0);
        loaded.Enemies.DrawLayers(oam, CameraX, 0, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException("Fake Kraid sweep emitted no OBJ pieces.");

        return new SweepResult(
            minimumX,
            maximumX,
            loaded.State.SpawnedSpikeCount,
            loaded.State.SpawnedSpitCount);
    }

    private static void VerifyCombat(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedFakeKraid bodyLoad = Load(bus, room, assets, samusX: 0x0560);
        PrimeGameplayFrame(bodyLoad, assets);
        bodyLoad.Samus.XPosition = bodyLoad.Actor.XPosition;
        bodyLoad.Samus.YPosition = bodyLoad.Actor.YPosition;
        bodyLoad.Samus.InvincibilityTimer = 0;
        bool bodyContact = bodyLoad.Enemies.ResolveOrdinarySamusContact(bodyLoad.Samus, 0);
        if (!bodyContact ||
            bodyLoad.Samus.Health != 899 || !bodyLoad.Samus.KnockbackActive)
        {
            throw new InvalidDataException(
                $"Fake Kraid body contact failed: collided={bodyContact}, health=" +
                $"{bodyLoad.Samus.Health}, knockback={bodyLoad.Samus.KnockbackActive}.");
        }

        VerifyProjectileContact(
            bus,
            room,
            assets,
            RoomEnemyProjectileKind.FakeKraidSpit,
            expectedDamage: 20);
        VerifyProjectileContact(
            bus,
            room,
            assets,
            RoomEnemyProjectileKind.FakeKraidSpikeRight,
            expectedDamage: 6);

        LoadedFakeKraid shotLoad = Load(bus, room, assets, samusX: 0x0560);
        PrimeGameplayFrame(shotLoad, assets);
        var shots = new SamusProjectileSystem();
        var sharedProjectiles = new SamusBombProjectileSystem();
        ArmProjectile(
            shots.Slots[0],
            shotLoad.Actor.XPosition,
            shotLoad.Actor.YPosition,
            type: 0x0200,
            damage: 300);
        int shotHits = shotLoad.Enemies.ResolveOrdinaryProjectileHits(
            bus,
            shots,
            sharedProjectiles,
            shotLoad.Samus);
        FakeKraidDropRequest? shotDrop = shotLoad.Enemies.LastFakeKraidDropRequest;
        if (shotHits != 1 || shotLoad.Actor.Health != 0 ||
            !shotLoad.Actor.Properties.HasAny(EnemyProperties.Deleted) ||
            shotDrop is not { X: 0x0530, Y: 0x00a0, ItemDropChancesPointer: 0xf2c0,
                DeathExplosionVariant: 3 })
        {
            throw new InvalidDataException(
                $"Fake Kraid lethal super-missile path failed: hits={shotHits}, health=" +
                $"{shotLoad.Actor.Health}, drop={shotDrop}.");
        }

        LoadedFakeKraid bombLoad = Load(bus, room, assets, samusX: 0x0560);
        PrimeGameplayFrame(bombLoad, assets);
        var normalBombs = new SamusBombProjectileSystem();
        ArmNormalBomb(normalBombs.Slots[0], bombLoad.Actor, damage: 1000);
        int normalBombHits = bombLoad.Enemies.ResolveOrdinaryBombHits(
            normalBombs,
            new SamusProjectileSystem(),
            bombLoad.Samus);
        FakeKraidDropRequest? bombDrop = bombLoad.Enemies.LastFakeKraidDropRequest;
        if (normalBombHits != 1 || (normalBombs.Slots[0].Direction & 0x0010) == 0 ||
            bombLoad.Actor.Health != 0 ||
            !bombLoad.Actor.Properties.HasAny(EnemyProperties.Deleted) ||
            bombDrop is not { X: 0x0530, Y: 0x00a0, ItemDropChancesPointer: 0xf2c0,
                DeathExplosionVariant: 3 })
        {
            throw new InvalidDataException(
                $"Fake Kraid lethal normal-bomb path failed: hits={normalBombHits}, " +
                $"direction=${normalBombs.Slots[0].Direction:X4}, health=" +
                $"{bombLoad.Actor.Health}, drop={bombDrop}.");
        }

        LoadedFakeKraid powerBombLoad = Load(bus, room, assets, samusX: 0x0560);
        int powerBombHits = powerBombLoad.Enemies.ResolveOrdinaryPowerBombHits(
            bus,
            powerBombLoad.Actor.XPosition,
            powerBombLoad.Actor.YPosition,
            explosionRadius: 64);
        if (powerBombHits != 1 || powerBombLoad.Actor.Health != 200 ||
            powerBombLoad.Enemies.LastFakeKraidDropRequest is not null)
        {
            throw new InvalidDataException("Fake Kraid first power bomb did not deal 200 damage.");
        }
        powerBombLoad.Actor.InvincibilityTimer = 0;
        powerBombHits = powerBombLoad.Enemies.ResolveOrdinaryPowerBombHits(
            bus,
            powerBombLoad.Actor.XPosition,
            powerBombLoad.Actor.YPosition,
            explosionRadius: 64);
        if (powerBombHits != 1 || powerBombLoad.Actor.Health != 0 ||
            powerBombLoad.Enemies.LastFakeKraidDropRequest is not { DeathExplosionVariant: 3 })
        {
            throw new InvalidDataException("Fake Kraid lethal power-bomb path lost its drop request.");
        }

        LoadedFakeKraid grappleLoad = Load(bus, room, assets, samusX: 0x0560);
        PrimeGameplayFrame(grappleLoad, assets);
        GrappleEnemyCollision grapple = grappleLoad.Enemies.ResolveGrappleEndpoint(
            grappleLoad.Actor.XPosition,
            grappleLoad.Actor.YPosition);
        if (!grapple.Collided || grapple.Reaction != GrappleEnemyReaction.Cancel ||
            grapple.EnemyNativeIndex != grappleLoad.Actor.NativeIndex)
        {
            throw new InvalidDataException(
                $"Fake Kraid grapple mismatch: collided={grapple.Collided}, reaction=" +
                $"{grapple.Reaction}, slot=${grapple.EnemyNativeIndex:X4}.");
        }
    }

    private static void VerifyProjectileContact(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        RoomEnemyProjectileKind kind,
        ushort expectedDamage)
    {
        LoadedFakeKraid loaded = Load(bus, room, assets, samusX: 0x0560);
        loaded.Samus.InvincibilityTimer = ushort.MaxValue;
        RoomEnemyProjectileSlot? target = null;
        for (int frame = 0; frame < 512 && target is null; frame++)
        {
            loaded.Enemies.StepFrame(
                CameraX,
                0,
                timeIsFrozen: false,
                loaded.Samus,
                level: assets.LevelData);
            loaded.Enemies.StepEnemyProjectiles(
                assets.LevelData,
                loaded.Samus,
                cameraX: CameraX,
                cameraY: 0);
            target = loaded.Enemies.EnemyProjectiles.FirstOrDefault(
                projectile => projectile.IsActive && projectile.Kind == kind);
        }
        if (target is null)
            throw new InvalidDataException($"Fake Kraid never spawned projectile {kind}.");

        // Spit is deliberately emitted as a co-located pair. Isolate one physical slot so
        // this check proves the ROM definition's per-projectile damage rather than merely
        // asserting the pair's combined result.
        foreach (RoomEnemyProjectileSlot other in loaded.Enemies.EnemyProjectiles)
        {
            if (!ReferenceEquals(other, target))
                other.Clear();
        }

        loaded.Samus.XPosition = target.XPosition;
        loaded.Samus.YPosition = target.YPosition;
        loaded.Samus.InvincibilityTimer = 0;
        ushort health = loaded.Samus.Health;
        loaded.Enemies.StepEnemyProjectiles(
            assets.LevelData,
            loaded.Samus,
            cameraX: CameraX,
            cameraY: 0);
        if (loaded.Samus.Health != health - expectedDamage || target.IsActive)
        {
            throw new InvalidDataException(
                $"Fake Kraid {kind} contact failed: health={health}->{loaded.Samus.Health}, " +
                $"active={target.IsActive}.");
        }
    }

    private static LoadedFakeKraid Load(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        ushort samusX)
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var samus = new SamusState
        {
            XPosition = samusX,
            YPosition = 0x00a0,
            Health = 999,
            MaxHealth = 999,
            Pose = samusX > 0x0530
                ? SamusState.FacingLeftNormalPose
                : SamusState.FacingRightNormalPose,
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
        RoomEnemySlot actor = enemies.Slots.Single(
            slot => slot.EnemyDefinitionPointer == Definition);
        FakeKraidEnemyState state = enemies.FakeKraidStates[actor.SlotIndex]
            ?? throw new InvalidDataException("Fake Kraid did not initialize typed state.");
        return new LoadedFakeKraid(enemies, samus, actor, state);
    }

    private static void PrimeGameplayFrame(
        LoadedFakeKraid loaded,
        CartridgeRoomAssets assets) =>
        loaded.Enemies.StepFrame(
            CameraX,
            cameraY: 0,
            timeIsFrozen: false,
            loaded.Samus,
            level: assets.LevelData);

    private static void ArmProjectile(
        SamusProjectileSlot projectile,
        ushort x,
        ushort y,
        ushort type,
        ushort damage)
    {
        projectile.ClearFields();
        projectile.Type = type;
        projectile.Damage = damage;
        projectile.Direction = 2;
        projectile.XPosition = x;
        projectile.YPosition = y;
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    private static void ArmNormalBomb(
        SamusBombProjectileSlot bomb,
        RoomEnemySlot target,
        ushort damage)
    {
        bomb.ClearFields();
        bomb.Type = SamusBombProjectileSystem.NormalBombType;
        bomb.Damage = damage;
        bomb.Direction = (ushort)SamusProjectileDirection.Right;
        bomb.XPosition = target.XPosition;
        bomb.YPosition = target.YPosition;
        bomb.XRadius = 32;
        bomb.YRadius = 24;
        bomb.BombTimer = 0;
        bomb.InstructionPointer = 0xa06b;
        bomb.InstructionTimer = 1;
    }

    private static void VerifyWords(
        ISnesAddressSpace bus,
        int address,
        IReadOnlyList<ushort> expected)
    {
        for (int index = 0; index < expected.Count; index++)
        {
            ushort actual = ReadWord(bus, address + index * 2);
            if (actual != expected[index])
            {
                throw new InvalidDataException(
                    $"ROM word ${address + index * 2:X6} was ${actual:X4}; expected " +
                    $"${expected[index]:X4}.");
            }
        }
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private readonly record struct LoadedFakeKraid(
        RoomEnemySystem Enemies,
        SamusState Samus,
        RoomEnemySlot Actor,
        FakeKraidEnemyState State);

    private readonly record struct SweepResult(
        ushort MinimumX,
        ushort MaximumX,
        int Spikes,
        int Spit);
}
