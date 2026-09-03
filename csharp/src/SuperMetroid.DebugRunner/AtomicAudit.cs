using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>
/// ROM-backed regression for Atomic. Wrecked Ship East Super supplies all four shipped
/// animation-direction parameters, while post-Phantoon Basement supplies the nonzero speed
/// used for exact four-quadrant chase motion. Population-prefix decorators only place a native
/// terminator after those unchanged records; headers, parameters, code, speed table, graphics,
/// palettes, spritemaps, vulnerabilities, and room assets all remain cartridge sourced.
/// </summary>
internal static class AtomicAudit
{
    private const ushort BasementRoom = 0xcc6f;
    private const ushort EastSuperRoom = 0xcdf1;
    private const ushort AtomicDefinition = 0xe9ff;

    private static readonly ushort[] InstructionLists =
    [
        0xe310,
        0xe32c,
        0xe348,
        0xe364,
    ];

    private static readonly HashSet<ushort> RightSpinMaps =
    [
        0xe489,
        0xe49f,
        0xe4b5,
        0xe4cb,
        0xe4e1,
        0xe4f2,
    ];

    private static readonly HashSet<ushort> LeftSpinMaps =
    [
        0xe508,
        0xe51e,
        0xe534,
        0xe54a,
        0xe560,
        0xe571,
    ];

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);

        // The ordinary East Super state contains four consecutive zero-speed Atomics with
        // parameter-one values 0,1,2,3. Starting the audit population at the first of those
        // records avoids depending on the unrelated Coven family that precedes them.
        CartridgeRoomHeader eastSuperRoom = CartridgeRoomHeader.Load(bus, EastSuperRoom);
        CartridgeRoomAssets eastSuperAssets = CartridgeRoomAssets.Load(bus, eastSuperRoom);
        ushort eastAtomicPopulation = FindFirstPopulationRecord(
            bus,
            eastSuperRoom.State.EnemyPopulationPointer,
            AtomicDefinition);
        LoadedAtomics directions = LoadPrefix(
            bus,
            eastSuperRoom,
            eastSuperAssets,
            eastAtomicPopulation,
            retainedRecordCount: 4);
        VerifyDirectionPopulation(eastSuperRoom, directions);
        int animationMaps = VerifyAllAnimationLists(directions, eastSuperAssets);
        int objectPieces = VerifyDrawing(directions);

        // Boss bit one selects Basement's post-Phantoon state. Its three Atomics use speed
        // index eight, a literal half pixel per axis per frame in the NTSC cartridge table.
        CartridgeRoomHeader basementRoom = CartridgeRoomHeader.Load(
            bus,
            BasementRoom,
            new RoomStateSelectionContext(default, BossBits: 1, false, false));
        CartridgeRoomAssets basementAssets = CartridgeRoomAssets.Load(bus, basementRoom);
        ushort basementAtomicPopulation = FindFirstPopulationRecord(
            bus,
            basementRoom.State.EnemyPopulationPointer,
            AtomicDefinition);
        VerifyMovementHeaderAndInitialization(
            bus,
            basementRoom,
            basementAssets,
            basementAtomicPopulation);
        VerifyAllMovementQuadrants(
            bus,
            basementRoom,
            basementAssets,
            basementAtomicPopulation);
        VerifyContactAttack(bus, basementRoom, basementAssets, basementAtomicPopulation);
        VerifyOrdinaryShotDamageAndDeath(
            bus,
            basementRoom,
            basementAssets,
            basementAtomicPopulation);
        VerifyPowerBombDamageAndDeath(
            bus,
            basementRoom,
            basementAssets,
            basementAtomicPopulation);
        VerifyGrappleCancel(bus, basementRoom, basementAssets, basementAtomicPopulation);

        Console.WriteLine(
            "Atomic audit passed: all four retail animation parameters produced " +
            $"{animationMaps} ROM maps and {objectPieces} OBJ pieces; exact signed 16.16 " +
            "movement chased in every quadrant, contact dealt 40 damage, common shots and " +
            "power bombs damaged/killed the 250-health actor, and grapple selected cancel.");
        return 0;
    }

    private static void VerifyDirectionPopulation(
        CartridgeRoomHeader room,
        LoadedAtomics loaded)
    {
        if (room.State.Pointer != 0xce03 || loaded.Enemies.EnemyCount != 4)
        {
            throw new InvalidDataException(
                $"East Super Atomic prefix selected state ${room.State.Pointer:X4} with " +
                $"{loaded.Enemies.EnemyCount} actors.");
        }

        ushort[] expectedX = [0x00a0, 0x0120, 0x01d0, 0x0250];
        for (int index = 0; index < 4; index++)
        {
            RoomEnemySlot actor = loaded.Enemies.Slots[index];
            AtomicEnemyState state = RequireState(loaded.Enemies, actor);
            if (actor.EnemyDefinitionPointer != AtomicDefinition ||
                actor.XPosition != expectedX[index] || actor.YPosition != 0x0050 ||
                actor.Parameter1 != index || actor.Parameter2 != 0 ||
                actor.CurrentInstruction != InstructionLists[index] ||
                actor.InstructionTimer != 1 || state.SpeedWhole != 0 ||
                state.SpeedFraction != 0 || state.NegativeSpeedWhole != 0 ||
                state.NegativeSpeedFraction != 0 || actor.VariableA != 0 ||
                actor.VariableB != 0)
            {
                throw new InvalidDataException(
                    $"East Super Atomic {index} initialization failed: definition=" +
                    $"${actor.EnemyDefinitionPointer:X4}, position=" +
                    $"(${actor.XPosition:X4},${actor.YPosition:X4}), params=" +
                    $"${actor.Parameter1:X4}/${actor.Parameter2:X4}, list=" +
                    $"${actor.CurrentInstruction:X4}, speed=" +
                    $"{state.SpeedWhole:X4}:{state.SpeedFraction:X4}/" +
                    $"{state.NegativeSpeedWhole:X4}:{state.NegativeSpeedFraction:X4}, " +
                    $"functions=${actor.VariableA:X4}/${actor.VariableB:X4}.");
            }
        }
    }

    private static int VerifyAllAnimationLists(
        LoadedAtomics loaded,
        CartridgeRoomAssets assets)
    {
        var maps = Enumerable.Range(0, 4)
            .Select(_ => new HashSet<ushort>())
            .ToArray();

        // Six records at eight frames each form one complete cycle. Fifty-six frames also
        // crosses each list's common goto instruction and proves it loops back cleanly. The
        // four population records span more than one screen, so center the scheduler on each
        // actor in turn instead of mistaking an intentionally inactive off-screen actor for a
        // stalled animation.
        for (int actorIndex = 0; actorIndex < 4; actorIndex++)
        {
            RoomEnemySlot actor = loaded.Enemies.Slots[actorIndex];
            ushort cameraX = actor.XPosition > 0x0080
                ? unchecked((ushort)(actor.XPosition - 0x0080))
                : (ushort)0;
            for (int frame = 0; frame < 56; frame++)
            {
                loaded.Enemies.StepFrame(
                    cameraX,
                    0,
                    false,
                    loaded.Samus,
                    level: assets.LevelData);
                maps[actorIndex].Add(loaded.Enemies.Slots[actorIndex].SpritemapPointer);
            }
        }

        for (int index = 0; index < maps.Length; index++)
        {
            HashSet<ushort> expected = index is 0 or 2 ? RightSpinMaps : LeftSpinMaps;
            if (!maps[index].SetEquals(expected))
            {
                throw new InvalidDataException(
                    $"Atomic animation parameter {index} emitted " +
                    $"[{string.Join(',', maps[index].Select(map => $"${map:X4}"))}] " +
                    $"instead of [{string.Join(',', expected.Select(map => $"${map:X4}"))}].");
            }
        }

        return maps.SelectMany(set => set).Distinct().Count();
    }

    private static int VerifyDrawing(LoadedAtomics loaded)
    {
        var oam = new OamBuffer();
        oam.BeginFrame();
        loaded.Enemies.DrawLayers(oam, 0, 0, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException("East Super Atomics emitted no ROM-authored OBJ.");
        return oam.LastFinalizedSpriteCount;
    }

    private static void VerifyMovementHeaderAndInitialization(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        ushort populationPointer)
    {
        LoadedAtomics loaded = LoadPrefix(bus, room, assets, populationPointer, 1);
        RoomEnemySlot actor = loaded.Enemies.Slots[0];
        AtomicEnemyState state = RequireState(loaded.Enemies, actor);
        RoomEnemyDefinition definition = actor.Definition;
        if (room.State.Pointer != 0xcc9b || loaded.Enemies.EnemyCount != 1 ||
            actor.EnemyDefinitionPointer != AtomicDefinition || actor.XPosition != 0x0098 ||
            actor.YPosition != 0x004d || actor.Parameter1 != 0 || actor.Parameter2 != 8 ||
            actor.Properties != 0x2000 || definition.TileDataSize != 0x0400 ||
            actor.Health != 250 || definition.Damage != 40 || actor.XRadius != 8 ||
            actor.YRadius != 8 || definition.Bank != 0xa8 ||
            definition.InitializationAiPointer != 0xe388 ||
            definition.MainAiPointer != 0xe3c3 || definition.GrappleAiPointer != 0x800f ||
            definition.TouchAiPointer != 0x8023 || definition.ShotAiPointer != 0x802d ||
            definition.PowerBombReactionPointer != 0 || definition.DeathAnimation != 2 ||
            definition.VulnerabilityPointer != 0xec1c || actor.Layer != 5 ||
            actor.CurrentInstruction != 0xe310 || actor.InstructionTimer != 1 ||
            state.SpeedWhole != 0 || state.SpeedFraction != 0x8000 ||
            state.NegativeSpeedWhole != 0xffff ||
            state.NegativeSpeedFraction != 0x8000)
        {
            throw new InvalidDataException(
                $"Basement Atomic initialization failed: state=${room.State.Pointer:X4}, " +
                $"count={loaded.Enemies.EnemyCount}, definition=${actor.EnemyDefinitionPointer:X4}, " +
                $"position=(${actor.XPosition:X4},${actor.YPosition:X4}), params=" +
                $"${actor.Parameter1:X4}/${actor.Parameter2:X4}, health/damage=" +
                $"{actor.Health}/{definition.Damage}, list=${actor.CurrentInstruction:X4}, " +
                $"speed={state.SpeedWhole:X4}:{state.SpeedFraction:X4}/" +
                $"{state.NegativeSpeedWhole:X4}:{state.NegativeSpeedFraction:X4}.");
        }
    }

    private static void VerifyAllMovementQuadrants(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        ushort populationPointer)
    {
        MovementCase[] cases =
        [
            new(64, 64, AtomicHorizontalMovement.MoveRight,
                AtomicVerticalMovement.MoveDown, 0, 0x8000, 0, 0x8000, 1, 0, 1, 0),
            new(-64, 64, AtomicHorizontalMovement.MoveLeft,
                AtomicVerticalMovement.MoveDown, -1, 0x8000, 0, 0x8000, -1, 0, 1, 0),
            new(-64, -64, AtomicHorizontalMovement.MoveLeft,
                AtomicVerticalMovement.MoveUp, -1, 0x8000, -1, 0x8000, -1, 0, -1, 0),
            new(64, -64, AtomicHorizontalMovement.MoveRight,
                AtomicVerticalMovement.MoveUp, 0, 0x8000, -1, 0x8000, 1, 0, -1, 0),
            // Equality follows the nonnegative/BPL path, so an exactly overlapping target
            // selects right/down rather than leaving either old function word unchanged.
            new(0, 0, AtomicHorizontalMovement.MoveRight,
                AtomicVerticalMovement.MoveDown, 0, 0x8000, 0, 0x8000, 1, 0, 1, 0),
        ];

        foreach (MovementCase movement in cases)
        {
            LoadedAtomics loaded = LoadPrefix(bus, room, assets, populationPointer, 1);
            RoomEnemySlot actor = loaded.Enemies.Slots[0];
            AtomicEnemyState state = RequireState(loaded.Enemies, actor);
            ushort startX = actor.XPosition;
            ushort startY = actor.YPosition;
            loaded.Samus.XPosition = unchecked((ushort)(startX + movement.SamusXOffset));
            loaded.Samus.YPosition = unchecked((ushort)(startY + movement.SamusYOffset));

            Step(loaded, assets);
            AssertMovement(
                actor,
                state,
                startX,
                startY,
                movement.FirstXWhole,
                movement.FirstXFraction,
                movement.FirstYWhole,
                movement.FirstYFraction,
                movement.Horizontal,
                movement.Vertical,
                "first");

            Step(loaded, assets);
            AssertMovement(
                actor,
                state,
                startX,
                startY,
                movement.SecondXWhole,
                movement.SecondXFraction,
                movement.SecondYWhole,
                movement.SecondYFraction,
                movement.Horizontal,
                movement.Vertical,
                "second");
        }
    }

    private static void AssertMovement(
        RoomEnemySlot actor,
        AtomicEnemyState state,
        ushort startX,
        ushort startY,
        int expectedXWholeOffset,
        ushort expectedXFraction,
        int expectedYWholeOffset,
        ushort expectedYFraction,
        AtomicHorizontalMovement expectedHorizontal,
        AtomicVerticalMovement expectedVertical,
        string frameName)
    {
        if (actor.XPosition != unchecked((ushort)(startX + expectedXWholeOffset)) ||
            actor.XSubposition != expectedXFraction ||
            actor.YPosition != unchecked((ushort)(startY + expectedYWholeOffset)) ||
            actor.YSubposition != expectedYFraction || state.HorizontalMovement != expectedHorizontal ||
            state.VerticalMovement != expectedVertical)
        {
            throw new InvalidDataException(
                $"Atomic {frameName}-frame {expectedHorizontal}/{expectedVertical} movement failed: " +
                $"position=({startX:X4},{startY:X4})->" +
                $"({actor.XPosition:X4}.{actor.XSubposition:X4}," +
                $"{actor.YPosition:X4}.{actor.YSubposition:X4}), functions=" +
                $"$A8:{(ushort)state.HorizontalMovement:X4}/" +
                $"$A8:{(ushort)state.VerticalMovement:X4}.");
        }
    }

    private static void VerifyContactAttack(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        ushort populationPointer)
    {
        LoadedAtomics loaded = LoadPrefix(bus, room, assets, populationPointer, 1);
        RoomEnemySlot actor = loaded.Enemies.Slots[0];
        Step(loaded, assets);
        loaded.Samus.XPosition = actor.XPosition;
        loaded.Samus.YPosition = actor.YPosition;
        ushort health = loaded.Samus.Health;
        if (!loaded.Enemies.ResolveOrdinarySamusContact(loaded.Samus, 0) ||
            loaded.Samus.Health != health - 40 || !loaded.Samus.KnockbackActive ||
            loaded.Samus.InvincibilityTimer != 0x0060)
        {
            throw new InvalidDataException(
                $"Atomic contact failed: health={health}->{loaded.Samus.Health}, " +
                $"knockback={loaded.Samus.KnockbackActive}, " +
                $"invincibility={loaded.Samus.InvincibilityTimer}.");
        }
    }

    private static void VerifyOrdinaryShotDamageAndDeath(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        ushort populationPointer)
    {
        LoadedAtomics loaded = LoadPrefix(bus, room, assets, populationPointer, 1);
        RoomEnemySlot actor = loaded.Enemies.Slots[0];
        Step(loaded, assets);
        var projectiles = new SamusProjectileSystem();
        var shared = new SamusBombProjectileSystem();

        // Default vulnerability multiplier two turns a real 20-damage missile into 20 enemy
        // damage: the common routine halves the projectile value, then multiplies by two.
        ArmProjectile(projectiles.Slots[0], actor, type: 0x0100, damage: 20);
        int hits = loaded.Enemies.ResolveOrdinaryProjectileHits(
            bus,
            projectiles,
            shared,
            loaded.Samus);
        if (hits != 1 || actor.Health != 230 || actor.FlashTimer != 12)
        {
            throw new InvalidDataException(
                $"Atomic first missile failed: hits={hits}, health={actor.Health}, " +
                $"flash={actor.FlashTimer}.");
        }

        for (int hit = 1; hit < 13; hit++)
        {
            ArmProjectile(projectiles.Slots[0], actor, type: 0x0100, damage: 20);
            loaded.Enemies.ResolveOrdinaryProjectileHits(bus, projectiles, shared, loaded.Samus);
        }
        if (actor.Health != 0 || !actor.Properties.HasAny(EnemyProperties.Deleted) ||
            loaded.Enemies.EnemiesKilled != 1)
        {
            throw new InvalidDataException(
                $"Atomic missile death failed: health={actor.Health}, deleted=" +
                $"{actor.Properties.HasAny(EnemyProperties.Deleted)}, " +
                $"kills={loaded.Enemies.EnemiesKilled}.");
        }
    }

    private static void VerifyPowerBombDamageAndDeath(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        ushort populationPointer)
    {
        LoadedAtomics loaded = LoadPrefix(bus, room, assets, populationPointer, 1);
        RoomEnemySlot actor = loaded.Enemies.Slots[0];
        int first = loaded.Enemies.ResolveOrdinaryPowerBombHits(
            bus,
            actor.XPosition,
            actor.YPosition,
            explosionRadius: 32);
        if (first != 1 || actor.Health != 50 || actor.InvincibilityTimer != 48 ||
            actor.Properties.HasAny(EnemyProperties.Deleted))
        {
            throw new InvalidDataException(
                $"Atomic first power bomb failed: reactions={first}, health={actor.Health}, " +
                $"invincibility={actor.InvincibilityTimer}, deleted=" +
                $"{actor.Properties.HasAny(EnemyProperties.Deleted)}.");
        }

        actor.InvincibilityTimer = 0;
        int second = loaded.Enemies.ResolveOrdinaryPowerBombHits(
            bus,
            actor.XPosition,
            actor.YPosition,
            explosionRadius: 32);
        if (second != 1 || actor.Health != 0 ||
            !actor.Properties.HasAny(EnemyProperties.Deleted) ||
            loaded.Enemies.EnemiesKilled != 1)
        {
            throw new InvalidDataException(
                $"Atomic second power bomb failed: reactions={second}, health={actor.Health}, " +
                $"deleted={actor.Properties.HasAny(EnemyProperties.Deleted)}, " +
                $"kills={loaded.Enemies.EnemiesKilled}.");
        }
    }

    private static void VerifyGrappleCancel(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        ushort populationPointer)
    {
        LoadedAtomics loaded = LoadPrefix(bus, room, assets, populationPointer, 1);
        RoomEnemySlot actor = loaded.Enemies.Slots[0];
        Step(loaded, assets);
        GrappleEnemyCollision collision = loaded.Enemies.ResolveGrappleEndpoint(
            actor.XPosition,
            actor.YPosition);
        if (!collision.Collided || collision.Reaction != GrappleEnemyReaction.Cancel ||
            actor.AiHandlerBits != 1)
        {
            throw new InvalidDataException(
                $"Atomic grapple collision selected {collision.Reaction} with handler " +
                $"${actor.AiHandlerBits:X4}.");
        }

        // Cancel dispatches to common frozen AI for one follow-up frame, then clears its
        // handler bit without deleting or damaging the actor.
        Step(loaded, assets);
        if (actor.AiHandlerBits != 4 || actor.Health != 250 ||
            actor.Properties.HasAny(EnemyProperties.Deleted))
        {
            throw new InvalidDataException(
                $"Atomic grapple-cancel dispatch failed: handler=${actor.AiHandlerBits:X4}, " +
                $"health={actor.Health}, deleted=" +
                $"{actor.Properties.HasAny(EnemyProperties.Deleted)}.");
        }
        Step(loaded, assets);
        if (actor.AiHandlerBits != 0)
            throw new InvalidDataException("Atomic grapple-cancel frozen handler did not clear.");
    }

    private static LoadedAtomics LoadPrefix(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        ushort populationPointer,
        int retainedRecordCount)
    {
        var prefixBus = new PopulationPrefixAddressSpace(
            bus,
            populationPointer,
            retainedRecordCount,
            deathQuota: 0);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusPoseIds.FacingRightNormalPose,
            XPosition = 0,
            YPosition = 0,
        };
        samus.RefreshCollisionRadii(prefixBus);
        samus.InitializeAnimation(prefixBus);
        var random = new Bank80SystemState();
        var enemies = new RoomEnemySystem();
        enemies.Load(
            prefixBus,
            populationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            random.NextRandom,
            random.SetRandomNumber,
            readRandomNumber: () => random.RandomNumber,
            level: assets.LevelData,
            samus: samus);
        return new LoadedAtomics(enemies, samus);
    }

    private static void Step(LoadedAtomics loaded, CartridgeRoomAssets assets) =>
        loaded.Enemies.StepFrame(0, 0, false, loaded.Samus, level: assets.LevelData);

    private static ushort FindFirstPopulationRecord(
        ISnesAddressSpace bus,
        ushort populationPointer,
        ushort definitionPointer)
    {
        ushort cursor = populationPointer;
        for (int record = 0; record < RoomEnemySystem.MaximumEnemyCount; record++)
        {
            ushort candidate = ReadWord(bus, 0xa10000 | cursor);
            if (candidate == definitionPointer)
                return cursor;
            if (candidate == 0xffff)
                break;
            cursor = unchecked((ushort)(cursor + 16));
        }

        throw new InvalidDataException(
            $"Population ${populationPointer:X4} contains no enemy ${definitionPointer:X4}.");
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private static AtomicEnemyState RequireState(
        RoomEnemySystem enemies,
        RoomEnemySlot actor) =>
        enemies.AtomicStates[actor.SlotIndex] ?? throw new InvalidDataException(
            $"Atomic slot {actor.SlotIndex} has no typed state.");

    private static void ArmProjectile(
        SamusProjectileSlot projectile,
        RoomEnemySlot target,
        ushort type,
        ushort damage)
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

    private readonly record struct LoadedAtomics(RoomEnemySystem Enemies, SamusState Samus);

    private readonly record struct MovementCase(
        int SamusXOffset,
        int SamusYOffset,
        AtomicHorizontalMovement Horizontal,
        AtomicVerticalMovement Vertical,
        int FirstXWhole,
        ushort FirstXFraction,
        int FirstYWhole,
        ushort FirstYFraction,
        int SecondXWhole,
        ushort SecondXFraction,
        int SecondYWhole,
        ushort SecondYFraction);
}
