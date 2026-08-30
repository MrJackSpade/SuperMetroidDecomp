using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>
/// End-to-end ROM audit for the six walking Space Pirate definitions and the three real
/// green Pirates preceding Fake Kraid in Mini-Kraid Hallway. Every actor, list, map, radius,
/// damage value, and projectile definition is read from the user's cartridge image.
/// </summary>
internal static class WalkingSpacePirateAudit
{
    private const ushort RoomPointer = 0xa521;
    private const ushort CameraX = 0;
    private const ushort CameraY = 0;

    private static readonly ushort[] Definitions =
        [0xf653, 0xf693, 0xf6d3, 0xf713, 0xf753, 0xf793];
    private static readonly ushort[] Health = [20, 90, 200, 900, 300, 500];
    private static readonly ushort[] Damage = [15, 20, 80, 200, 160, 15];

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, RoomPointer);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);

        VerifyDefinitionsAndRetailPopulation(bus, room);
        VerifyUntouchedInitialization(bus, room, assets);
        PatrolResult patrol = VerifyPatrolMovementAndAnimation(bus, room, assets);
        AttackResult rightAttack = VerifyAttackAnimationAndLaserLifecycle(bus, room, assets);
        AttackResult leftAttack = VerifyLeftAttackAndFastLaserLifecycle(bus, room, assets);
        VerifyProjectileFlinch(bus, room, assets);
        VerifyContactProjectilePowerBombAndGrapple(bus, room, assets);

        Console.WriteLine(
            "Walking Space Pirate audit passed: all six retail headers share the exact " +
            "$B2:FD02/$FD32 family; Mini-Kraid Hallway loaded its untouched four-enemy " +
            $"population; Pirates patrolled ${patrol.MinimumX:X4}-${patrol.MaximumX:X4} " +
            "with both walking and both look-around list families; firing covered all " +
            $"left/right maps and {rightAttack.LaserMaps + leftAttack.LaserMaps} directional " +
            $"laser-map samples, emitted {rightAttack.SpawnedLasers + leftAttack.SpawnedLasers} " +
            "room-graphics lasers through both 2- and 4-px/frame branches with the native " +
            "immediate A050 movement and sound; projectile proximity selected both ROM " +
            "flinch lists; body/laser contact damaged Samus; beam, normal-bomb, and " +
            "power-bomb damage " +
            "killed the actor; frozen touch was inert; grapple cancelled; and extended " +
            "spritemaps emitted OBJ pieces.");
        return 0;
    }

    private static void VerifyDefinitionsAndRetailPopulation(
        ISnesAddressSpace bus,
        CartridgeRoomHeader room)
    {
        if (room.State.Pointer != 0xa533 ||
            room.State.EnemyPopulationPointer != 0xa0ba ||
            room.State.EnemyTilesetPointer != 0x8651 ||
            room.WidthInScreens != 6 || room.HeightInScreens != 1)
        {
            throw new InvalidDataException(
                $"Mini-Kraid Hallway mismatch: state=${room.State.Pointer:X4}, " +
                $"population=${room.State.EnemyPopulationPointer:X4}, tileset=" +
                $"${room.State.EnemyTilesetPointer:X4}, size={room.WidthInScreens}x" +
                $"{room.HeightInScreens}.");
        }

        for (int index = 0; index < Definitions.Length; index++)
        {
            RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(bus, Definitions[index]);
            if (definition.TileDataSize != 0x0c00 ||
                definition.Health != Health[index] || definition.Damage != Damage[index] ||
                definition.XRadius != 0x0010 || definition.YRadius != 0x0020 ||
                definition.Bank != 0xb2 || definition.HurtAiTime != 0 ||
                definition.InitializationAiPointer != 0xfd02 || definition.PartCount != 1 ||
                definition.MainAiPointer != 0xfd32 || definition.GrappleAiPointer != 0x800f ||
                definition.HurtAiPointer != 0x804c || definition.FrozenAiPointer != 0x8041 ||
                definition.DeathAnimation != 4 ||
                definition.PowerBombReactionPointer != 0x8767 ||
                definition.TouchAiPointer != 0x876c || definition.ShotAiPointer != 0x8779 ||
                definition.TileDataAddress == 0 || definition.Layer != 5 ||
                definition.ItemDropChancesPointer == 0 ||
                definition.VulnerabilityPointer == 0 || definition.NamePointer == 0)
            {
                throw new InvalidDataException(
                    $"Walking Space Pirate header $A0:{Definitions[index]:X4} did not " +
                    "match the shared retail family contract.");
            }
        }

        int population = 0xa10000 | room.State.EnemyPopulationPointer;
        ushort[] expectedX = [0x00d9, 0x0120, 0x01f4];
        for (int record = 0; record < expectedX.Length; record++)
        {
            int address = population + record * 16;
            ushort[] expected =
                [0xf693, expectedX[record], 0x00a0, 0, 0x2000, 0x0004, 0x8000, 0x0050];
            for (int word = 0; word < expected.Length; word++)
            {
                if (ReadWord(bus, address + word * 2) != expected[word])
                {
                    throw new InvalidDataException(
                        $"Walking Pirate population record {record}, word {word} changed.");
                }
            }
        }
        if (ReadWord(bus, population + 3 * 16) != 0xe0ff ||
            ReadWord(bus, population + 4 * 16) != 0xffff)
        {
            throw new InvalidDataException(
                "Walking Pirate population did not lead into Fake Kraid and its terminator.");
        }

        // These live ROM sentinels cover the four custom actor opcodes, both walking lists,
        // the laser's unusual A050 opcode, and all seven projectile-definition words.
        VerifyWords(bus, 0xb2fb64, [0xfcb8, 0xfd44, 0x000a]);
        VerifyWords(bus, 0xb2fbe6, [0xfcb8, 0xfdce, 0x000a]);
        VerifyWords(bus, 0xb2fba0, [0xfc68, 0x0008]);
        VerifyWords(bus, 0xb2fc22, [0xfc90, 0x0008]);
        VerifyWords(bus, 0x869f4d, [0xa050, 0xa05c, 0x0001]);
        VerifyWords(bus, 0x869f89, [0xa050, 0xa07a, 0x0001]);
        VerifyWords(bus, 0x86a17b,
            [0xa009, 0xa05c, 0x9f41, 0x0410, 0x100a, 0x0000, 0x84fc]);
    }

    private static void VerifyUntouchedInitialization(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedPirates loaded = Load(bus, room, assets, samusX: 0x0180, samusY: 0x0040);
        if (loaded.Enemies.EnemyCount != 4 || loaded.Pirates.Count != 3)
            throw new InvalidDataException("Untouched Mini-Kraid population did not load four actors.");

        ushort[] expectedX = [0x00d9, 0x0120, 0x01f4];
        for (int index = 0; index < loaded.Pirates.Count; index++)
        {
            RoomEnemySlot actor = loaded.Pirates[index];
            WalkingSpacePirateEnemyState state = State(loaded.Enemies, actor);
            if (actor.XPosition != expectedX[index] || actor.YPosition != 0x00a0 ||
                actor.Parameter1 != 0x8000 || actor.Parameter2 != 0x0050 ||
                actor.CurrentInstruction != 0xfb64 || actor.InstructionTimer != 1 ||
                actor.SpritemapPointer != 0x804f || actor.Health != 90 ||
                actor.Properties != 0x2000 || actor.ExtraProperties != 0x0004 ||
                state.Function != WalkingSpacePirateFunction.NoOperation ||
                state.LeftPostXPosition != unchecked((ushort)(expectedX[index] - 0x50)) ||
                state.RightPostXPosition != unchecked((ushort)(expectedX[index] + 0x50)) ||
                state.SpawnedLaserCount != 0)
            {
                throw new InvalidDataException(
                    $"Walking Pirate slot {index} initialization mismatch at " +
                    $"(${actor.XPosition:X4},${actor.YPosition:X4}), list=" +
                    $"${actor.CurrentInstruction:X4}, function=${(ushort)state.Function:X4}.");
            }
        }
    }

    private static PatrolResult VerifyPatrolMovementAndAnimation(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedPirates loaded = Load(bus, room, assets, samusX: 0x0080, samusY: 0x0040);
        RoomEnemySlot actor = loaded.Pirates[0];
        var seenMaps = new HashSet<ushort>();
        var seenFunctions = new HashSet<WalkingSpacePirateFunction>();
        ushort minimumX = actor.XPosition;
        ushort maximumX = actor.XPosition;

        // Eighteen hundred frames is long enough to cross both patrol directions and finish
        // both look-around animations, while remaining deterministic because this AI has
        // no random branch. Samus is vertically outside the strict 16-pixel firing band.
        ushort trackingCameraX = 0;
        for (int frame = 0; frame < 1800; frame++)
        {
            // EnemyMain only processes actors inside its 16-pixel-expanded screen window.
            // Follow this audit actor so reaching the right patrol post is not accidentally
            // turned into an off-screen freeze at world X=$0111.
            trackingCameraX = actor.XPosition > 0x0080
                ? unchecked((ushort)(actor.XPosition - 0x0080))
                : (ushort)0;
            loaded.Enemies.StepFrame(
                trackingCameraX,
                CameraY,
                timeIsFrozen: false,
                loaded.Samus,
                level: assets.LevelData);
            seenMaps.Add(actor.SpritemapPointer);
            seenFunctions.Add(State(loaded.Enemies, actor).Function);
            minimumX = Math.Min(minimumX, actor.XPosition);
            maximumX = Math.Max(maximumX, actor.XPosition);
        }

        HashSet<ushort> expectedMaps = ReadMapPointers(
            bus,
            [
                0xb2fb68, 0xb2fb6c, 0xb2fb70, 0xb2fb74,
                0xb2fb78, 0xb2fb7c, 0xb2fb80, 0xb2fb84,
                0xb2fbea, 0xb2fbee, 0xb2fbf2, 0xb2fbf6,
                0xb2fbfa, 0xb2fbfe, 0xb2fc02, 0xb2fc06,
                0xb2fbca, 0xb2fbce, 0xb2fbd2, 0xb2fbd6, 0xb2fbda, 0xb2fbde,
                0xb2fc4c, 0xb2fc50, 0xb2fc54, 0xb2fc58, 0xb2fc5c, 0xb2fc60,
            ]);
        if (!expectedMaps.IsSubsetOf(seenMaps) || minimumX == maximumX ||
            !seenFunctions.Contains(WalkingSpacePirateFunction.WalkingLeft) ||
            !seenFunctions.Contains(WalkingSpacePirateFunction.WalkingRight))
        {
            throw new InvalidDataException(
                $"Walking Pirate patrol failed: x=${minimumX:X4}-${maximumX:X4}, " +
                $"functions={string.Join(',', seenFunctions)}, maps=" +
                $"{string.Join(',', seenMaps.Order())}, missing=" +
                $"{string.Join(',', expectedMaps.Except(seenMaps).Order())}, final=" +
                $"(${actor.XPosition:X4},${actor.YPosition:X4})/" +
                $"${actor.CurrentInstruction:X4}/${actor.InstructionTimer:X4}/" +
                $"${(ushort)State(loaded.Enemies, actor).Function:X4}.");
        }

        var oam = new OamBuffer();
        oam.BeginFrame();
        loaded.Enemies.DrawLayers(oam, trackingCameraX, CameraY, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException("Walking Pirate extended map emitted no OBJ pieces.");

        return new PatrolResult(minimumX, maximumX);
    }

    private static AttackResult VerifyAttackAnimationAndLaserLifecycle(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        const ushort attackCameraX = 0x0080;
        LoadedPirates loaded = Load(bus, room, assets, samusX: 0x0180, samusY: 0x00a0);
        RoomEnemySlot actor = loaded.Pirates[0];
        var seenActorMaps = new HashSet<ushort>();
        var seenLaserMaps = new HashSet<ushort>();
        var seenSpawnY = new HashSet<ushort>();
        bool sawSound = false;
        bool sawImmediateMovement = false;
        bool sawSteadyMovement = false;

        for (int frame = 0; frame < 600; frame++)
        {
            loaded.Enemies.StepFrame(
                attackCameraX,
                CameraY,
                timeIsFrozen: false,
                loaded.Samus,
                level: assets.LevelData,
                samusProjectiles: loaded.Projectiles);
            seenActorMaps.Add(actor.SpritemapPointer);
            sawSound |= loaded.Enemies.LastSpacePirateSoundEffect == 0x0067;

            // Snapshot physical slots because StepEnemyProjectiles mutates the same exposed
            // objects. This lets the audit distinguish A050's immediate move from the normal
            // next-frame pre-instruction without adding test-only hooks to production code.
            var before = loaded.Enemies.EnemyProjectiles.ToDictionary(
                projectile => projectile.SlotIndex,
                projectile => (projectile.Kind, projectile.XPosition, projectile.PreInstruction));
            foreach (RoomEnemyProjectileSlot projectile in loaded.Enemies.EnemyProjectiles)
            {
                if (projectile.Kind == RoomEnemyProjectileKind.PirateMotherBrainLaser &&
                    projectile.PreInstruction == 0xa05b)
                {
                    seenSpawnY.Add(projectile.YPosition);
                }
            }

            loaded.Enemies.StepEnemyProjectiles(
                assets.LevelData,
                samus: null,
                cameraX: attackCameraX,
                cameraY: CameraY);

            foreach (RoomEnemyProjectileSlot projectile in loaded.Enemies.EnemyProjectiles)
            {
                if (!projectile.IsActive ||
                    projectile.Kind != RoomEnemyProjectileKind.PirateMotherBrainLaser)
                {
                    continue;
                }
                seenLaserMaps.Add(projectile.SpritemapPointer);
                (RoomEnemyProjectileKind oldKind, ushort oldX, ushort oldPre) =
                    before[projectile.SlotIndex];
                if (oldKind != RoomEnemyProjectileKind.PirateMotherBrainLaser)
                    continue;
                ushort delta = unchecked((ushort)(projectile.XPosition - oldX));
                if (oldPre == 0xa05b && projectile.PreInstruction == 0xa07a && delta == 2)
                    sawImmediateMovement = true;
                if (oldPre == 0xa07a && projectile.PreInstruction == 0xa07a && delta == 2)
                    sawSteadyMovement = true;
            }
        }

        HashSet<ushort> expectedActorMaps = ReadMapPointers(
            bus,
            [
                0xb2fc12, 0xb2fc16, 0xb2fc1a, 0xb2fc1e,
                0xb2fc26, 0xb2fc2e, 0xb2fc36, 0xb2fc3a, 0xb2fc3e, 0xb2fc42,
            ]);
        HashSet<ushort> expectedLaserMaps = ReadMapPointers(
            bus,
            [
                0x869f7d, 0x869f81, 0x869f85,
                0x869f8d, 0x869f91, 0x869f95, 0x869f99, 0x869f9d,
                0x869fa1, 0x869fa5, 0x869fa9, 0x869fad, 0x869fb1,
            ]);
        WalkingSpacePirateEnemyState state = State(loaded.Enemies, actor);
        if (!expectedActorMaps.IsSubsetOf(seenActorMaps) ||
            !expectedLaserMaps.IsSubsetOf(seenLaserMaps) ||
            !seenSpawnY.IsSupersetOf([(ushort)0x0098, (ushort)0x009e, (ushort)0x00a8]) ||
            state.SpawnedLaserCount < 3 || !sawSound ||
            !sawImmediateMovement || !sawSteadyMovement)
        {
            throw new InvalidDataException(
                $"Walking Pirate attack failed: actor maps=" +
                $"{string.Join(',', seenActorMaps.Order())}, laser maps=" +
                $"{string.Join(',', seenLaserMaps.Order())}, y=" +
                $"{string.Join(',', seenSpawnY.Order())}, count={state.SpawnedLaserCount}, " +
                $"missing actor={string.Join(',', expectedActorMaps.Except(seenActorMaps))}, " +
                $"missing laser={string.Join(',', expectedLaserMaps.Except(seenLaserMaps))}, " +
                $"sound={sawSound}, movement={sawImmediateMovement}/{sawSteadyMovement}.");
        }

        return new AttackResult(state.SpawnedLaserCount, seenLaserMaps.Count);
    }

    private static AttackResult VerifyLeftAttackAndFastLaserLifecycle(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedPirates loaded = Load(bus, room, assets, samusX: 0x0080, samusY: 0x00a0);
        RoomEnemySlot actor = loaded.Pirates[0];

        // Parameter bit fifteen is the laser-speed/flinch selector, not a family variant.
        // The room's real records all set it (2 px/frame), so clear it only after the exact
        // population load to exercise the equally real 4 px/frame branch used by other data.
        actor.Parameter1 = 0;
        foreach (RoomEnemySlot other in loaded.Pirates.Skip(1))
            other.Properties = other.Properties.With(EnemyProperties.Deleted);

        var seenActorMaps = new HashSet<ushort>();
        var seenLaserMaps = new HashSet<ushort>();
        var seenSpawnY = new HashSet<ushort>();
        bool sawSound = false;
        bool sawImmediateMovement = false;
        bool sawSteadyMovement = false;

        for (int frame = 0; frame < 600; frame++)
        {
            loaded.Enemies.StepFrame(
                CameraX,
                CameraY,
                false,
                loaded.Samus,
                level: assets.LevelData,
                samusProjectiles: loaded.Projectiles);
            seenActorMaps.Add(actor.SpritemapPointer);
            sawSound |= loaded.Enemies.LastSpacePirateSoundEffect == 0x0067;

            var before = loaded.Enemies.EnemyProjectiles.ToDictionary(
                projectile => projectile.SlotIndex,
                projectile => (projectile.Kind, projectile.XPosition, projectile.PreInstruction));
            foreach (RoomEnemyProjectileSlot projectile in loaded.Enemies.EnemyProjectiles)
            {
                if (projectile.Kind == RoomEnemyProjectileKind.PirateMotherBrainLaser &&
                    projectile.PreInstruction == 0xa05b)
                {
                    seenSpawnY.Add(projectile.YPosition);
                }
            }

            loaded.Enemies.StepEnemyProjectiles(
                assets.LevelData,
                samus: null,
                cameraX: CameraX,
                cameraY: CameraY);
            foreach (RoomEnemyProjectileSlot projectile in loaded.Enemies.EnemyProjectiles)
            {
                if (!projectile.IsActive ||
                    projectile.Kind != RoomEnemyProjectileKind.PirateMotherBrainLaser)
                {
                    continue;
                }
                seenLaserMaps.Add(projectile.SpritemapPointer);
                (RoomEnemyProjectileKind oldKind, ushort oldX, ushort oldPre) =
                    before[projectile.SlotIndex];
                if (oldKind != RoomEnemyProjectileKind.PirateMotherBrainLaser)
                    continue;
                ushort leftDelta = unchecked((ushort)(oldX - projectile.XPosition));
                if (oldPre == 0xa05b && projectile.PreInstruction == 0xa05c && leftDelta == 4)
                    sawImmediateMovement = true;
                if (oldPre == 0xa05c && projectile.PreInstruction == 0xa05c && leftDelta == 4)
                    sawSteadyMovement = true;
            }
        }

        HashSet<ushort> expectedActorMaps = ReadMapPointers(
            bus,
            [
                0xb2fb90, 0xb2fb94, 0xb2fb98, 0xb2fb9c,
                0xb2fba4, 0xb2fbac, 0xb2fbb4, 0xb2fbb8, 0xb2fbbc, 0xb2fbc0,
            ]);
        HashSet<ushort> expectedLaserMaps = ReadMapPointers(
            bus,
            [
                0x869f41, 0x869f45, 0x869f49,
                0x869f51, 0x869f55, 0x869f59, 0x869f5d, 0x869f61,
                0x869f65, 0x869f69, 0x869f6d, 0x869f71, 0x869f75,
            ]);
        WalkingSpacePirateEnemyState state = State(loaded.Enemies, actor);
        if (!expectedActorMaps.IsSubsetOf(seenActorMaps) ||
            !expectedLaserMaps.IsSubsetOf(seenLaserMaps) ||
            !seenSpawnY.IsSupersetOf([(ushort)0x0098, (ushort)0x009e, (ushort)0x00a8]) ||
            state.SpawnedLaserCount < 3 || !sawSound ||
            !sawImmediateMovement || !sawSteadyMovement)
        {
            throw new InvalidDataException(
                $"Walking Pirate left/fast attack failed: actor maps=" +
                $"{string.Join(',', seenActorMaps.Order())}, laser maps=" +
                $"{string.Join(',', seenLaserMaps.Order())}, y=" +
                $"{string.Join(',', seenSpawnY.Order())}, count={state.SpawnedLaserCount}, " +
                $"missing actor={string.Join(',', expectedActorMaps.Except(seenActorMaps))}, " +
                $"missing laser={string.Join(',', expectedLaserMaps.Except(seenLaserMaps))}, " +
                $"sound={sawSound}, movement={sawImmediateMovement}/{sawSteadyMovement}.");
        }

        return new AttackResult(state.SpawnedLaserCount, seenLaserMaps.Count);
    }

    private static void VerifyProjectileFlinch(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        VerifyFacing(samusX: 0x0119, expectedMapAddress: 0xb2fb5e,
            expectedCursor: 0xfb60, facingName: "right");
        VerifyFacing(samusX: 0x0080, expectedMapAddress: 0xb2fb52,
            expectedCursor: 0xfb54, facingName: "left");

        void VerifyFacing(
            ushort samusX,
            int expectedMapAddress,
            ushort expectedCursor,
            string facingName)
        {
            LoadedPirates loaded = Load(bus, room, assets, samusX, samusY: 0x0040);
            RoomEnemySlot actor = loaded.Pirates[0];
            loaded.Enemies.StepFrame(
                CameraX,
                CameraY,
                false,
                loaded.Samus,
                level: assets.LevelData,
                samusProjectiles: loaded.Projectiles);
            ArmProjectile(
                loaded.Projectiles.Slots[4],
                actor.XPosition,
                actor.YPosition,
                type: 0x0001,
                damage: 20);
            loaded.Enemies.StepFrame(
                CameraX,
                CameraY,
                false,
                loaded.Samus,
                level: assets.LevelData,
                samusProjectiles: loaded.Projectiles);

            WalkingSpacePirateEnemyState state = State(loaded.Enemies, actor);
            ushort expectedFlinchMap = ReadWord(bus, expectedMapAddress);
            if (state.Function != WalkingSpacePirateFunction.NoOperation ||
                actor.SpritemapPointer != expectedFlinchMap ||
                actor.CurrentInstruction != expectedCursor || actor.InstructionTimer != 0x0010)
            {
                throw new InvalidDataException(
                    $"Walking Pirate {facingName} flinch mismatch: " +
                    $"function=${(ushort)state.Function:X4}, " +
                    $"map=${actor.SpritemapPointer:X4}, " +
                    $"cursor=${actor.CurrentInstruction:X4}, " +
                    $"timer=${actor.InstructionTimer:X4}.");
            }
        }
    }

    private static void VerifyContactProjectilePowerBombAndGrapple(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedPirates body = Load(bus, room, assets, samusX: 0x0080, samusY: 0x0040);
        Prime(body, assets);
        body.Samus.XPosition = body.Pirates[0].XPosition;
        body.Samus.YPosition = body.Pirates[0].YPosition;
        body.Samus.InvincibilityTimer = 0;
        if (!body.Enemies.ResolveOrdinarySamusContact(body.Samus, 0) ||
            body.Samus.Health != 979 || !body.Samus.KnockbackActive)
        {
            throw new InvalidDataException("Green walking Pirate body contact did not deal 20 damage.");
        }

        LoadedPirates frozen = Load(bus, room, assets, samusX: 0x0080, samusY: 0x0040);
        // Let the actor install its first real extended map before freezing it. A freshly
        // allocated `$804F` is the common-bank "nothing" sentinel, not a bank-$B2 Pirate
        // map, and production never asks the family hitbox walker to collide that transient
        // initialization frame.
        Prime(frozen, assets);
        frozen.Pirates[0].FrozenTimer = 2;
        frozen.Samus.XPosition = frozen.Pirates[0].XPosition;
        frozen.Samus.YPosition = frozen.Pirates[0].YPosition;
        frozen.Samus.InvincibilityTimer = 0;
        if (!frozen.Enemies.ResolveOrdinarySamusContact(frozen.Samus, 0) ||
            frozen.Samus.Health != 999)
        {
            throw new InvalidDataException("Frozen walking Pirate incorrectly damaged Samus.");
        }

        LoadedPirates laser = Load(bus, room, assets, samusX: 0x0180, samusY: 0x00a0);
        RoomEnemyProjectileSlot? laserActor = null;
        for (int frame = 0; frame < 256 && laserActor is null; frame++)
        {
            laser.Enemies.StepFrame(
                CameraX,
                CameraY,
                false,
                laser.Samus,
                level: assets.LevelData,
                samusProjectiles: laser.Projectiles);
            laserActor = laser.Enemies.EnemyProjectiles.FirstOrDefault(
                projectile => projectile.Kind == RoomEnemyProjectileKind.PirateMotherBrainLaser);
        }
        if (laserActor is null)
            throw new InvalidDataException("Walking Pirate never spawned a contact-test laser.");
        foreach (RoomEnemyProjectileSlot other in laser.Enemies.EnemyProjectiles)
        {
            if (!ReferenceEquals(other, laserActor))
                other.Clear();
        }
        laser.Samus.XPosition = laserActor.XPosition;
        laser.Samus.YPosition = laserActor.YPosition;
        laser.Samus.InvincibilityTimer = 0;
        laser.Enemies.StepEnemyProjectiles(
            assets.LevelData,
            laser.Samus,
            cameraX: CameraX,
            cameraY: CameraY);
        if (laser.Samus.Health != 979 || laserActor.IsActive)
            throw new InvalidDataException("Walking Pirate laser did not deal header damage and delete.");

        LoadedPirates shot = Load(bus, room, assets, samusX: 0x0080, samusY: 0x0040);
        Prime(shot, assets);
        ArmProjectile(
            shot.Projectiles.Slots[0],
            shot.Pirates[0].XPosition,
            shot.Pirates[0].YPosition,
            type: 0x0200,
            damage: 300);
        if (shot.Enemies.ResolveOrdinaryProjectileHits(
                bus,
                shot.Projectiles,
                shot.SharedProjectiles,
                shot.Samus) != 1 ||
            shot.Pirates[0].Health != 0 ||
            !shot.Pirates[0].Properties.HasAny(EnemyProperties.Deleted))
        {
            throw new InvalidDataException("Walking Pirate lethal projectile damage failed.");
        }

        LoadedPirates bomb = Load(bus, room, assets, samusX: 0x0080, samusY: 0x0040);
        Prime(bomb, assets);
        RoomEnemySlot bombActor = bomb.Pirates[0];
        SamusBombProjectileSlot normalBomb =
            EnemyProjectileAuditAssertions.ArmExplodingNormalBomb(
                bomb.SharedProjectiles,
                bombActor.XPosition,
                bombActor.YPosition,
                damage: 1000);
        if (bomb.Enemies.ResolveOrdinaryBombHits(
                bomb.SharedProjectiles, bomb.Projectiles, bomb.Samus) != 1 ||
            (normalBomb.Direction & 0x0010) == 0 || bombActor.Health != 0 ||
            !bombActor.Properties.HasAny(EnemyProperties.Deleted))
        {
            throw new InvalidDataException(
                "Walking Pirate normal bomb did not run its selected bank-$B2 hitbox " +
                "callback through common damage and death.");
        }

        LoadedPirates powerBomb = Load(bus, room, assets, samusX: 0x0080, samusY: 0x0040);
        if (powerBomb.Enemies.ResolveOrdinaryPowerBombHits(
                bus,
                powerBomb.Pirates[0].XPosition,
                powerBomb.Pirates[0].YPosition,
                explosionRadius: 64) != 1 ||
            powerBomb.Pirates[0].Health != 0 ||
            !powerBomb.Pirates[0].Properties.HasAny(EnemyProperties.Deleted))
        {
            throw new InvalidDataException("Walking Pirate power-bomb reaction failed.");
        }

        LoadedPirates grapple = Load(bus, room, assets, samusX: 0x0080, samusY: 0x0040);
        Prime(grapple, assets);
        GrappleEnemyCollision result = grapple.Enemies.ResolveGrappleEndpoint(
            grapple.Pirates[0].XPosition,
            grapple.Pirates[0].YPosition);
        if (!result.Collided || result.Reaction != GrappleEnemyReaction.Cancel ||
            result.EnemyNativeIndex != grapple.Pirates[0].NativeIndex)
        {
            throw new InvalidDataException("Walking Pirate grapple-cancel reaction failed.");
        }
    }

    private static LoadedPirates Load(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        ushort samusX,
        ushort samusY)
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var samus = new SamusState
        {
            XPosition = samusX,
            YPosition = samusY,
            Health = 999,
            MaxHealth = 999,
            Pose = SamusState.FacingRightNormalPose,
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
        List<RoomEnemySlot> pirates = enemies.Slots
            .Where(slot => slot.EnemyDefinitionPointer == 0xf693)
            .ToList();
        return new LoadedPirates(
            enemies,
            samus,
            pirates,
            new SamusProjectileSystem(),
            new SamusBombProjectileSystem());
    }

    private static void Prime(LoadedPirates loaded, CartridgeRoomAssets assets) =>
        loaded.Enemies.StepFrame(
            CameraX,
            CameraY,
            false,
            loaded.Samus,
            level: assets.LevelData,
            samusProjectiles: loaded.Projectiles);

    private static WalkingSpacePirateEnemyState State(
        RoomEnemySystem enemies,
        RoomEnemySlot actor) =>
        enemies.WalkingSpacePirateStates[actor.SlotIndex] ??
        throw new InvalidDataException(
            $"Walking Pirate slot {actor.SlotIndex} did not initialize typed state.");

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

    private static HashSet<ushort> ReadMapPointers(
        ISnesAddressSpace bus,
        IEnumerable<int> frameAddresses) =>
        frameAddresses.Select(address => ReadWord(bus, address + 2)).ToHashSet();

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

    private readonly record struct LoadedPirates(
        RoomEnemySystem Enemies,
        SamusState Samus,
        IReadOnlyList<RoomEnemySlot> Pirates,
        SamusProjectileSystem Projectiles,
        SamusBombProjectileSystem SharedProjectiles);

    private readonly record struct PatrolResult(ushort MinimumX, ushort MaximumX);
    private readonly record struct AttackResult(int SpawnedLasers, int LaserMaps);
}
