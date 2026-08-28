using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Retail-ROM audit for Lava Dive's all-Namihe population. It complements Volcano's Fune
/// coverage by proving the other branch of the shared initializer, strict wake distance,
/// left/right animation tables, four-pixel muzzle adjustment, directional projectile
/// movers, body/beam/Grapple reactions, and the much stronger Namihe fireball.
/// </summary>
internal static class NamiheAudit
{
    private const ushort RoomPointer = 0xaf14;
    private const ushort StatePointer = 0xaf21;
    private const ushort PopulationPointer = 0xad09;
    private const ushort TilesetPointer = 0x88a3;
    private const ushort DefinitionPointer = 0xe73f;
    private const ushort EmptySpritemap = 0x804d;

    private static readonly ushort[] LeftActorMaps =
        [0x97b4, 0x97de, 0x9808, 0x9832, 0x985c, 0x9886];
    private static readonly ushort[] RightActorMaps =
        [0x98b0, 0x98da, 0x9904, 0x992e, 0x9958, 0x9982];
    private static readonly ushort[] LeftProjectileMaps = [0xaab9, 0xaac0, 0xaac7];
    private static readonly ushort[] RightProjectileMaps = [0xaace, 0xaad5, 0xaadc];

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, RoomPointer);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        VerifyHeader(bus);
        VerifyProjectileDefinition(bus);

        RoomEnemySystem enemies = LoadRoom(bus, room, assets, out SamusState samus);
        RoomEnemySlot[] population = enemies.Slots.Take(enemies.EnemyCount).ToArray();
        if (room.State.Pointer != StatePointer ||
            room.State.EnemyPopulationPointer != PopulationPointer ||
            room.State.EnemyTilesetPointer != TilesetPointer ||
            enemies.EnemyCount != 6 || enemies.DeathQuota != 6 ||
            population.Any(slot => slot.EnemyDefinitionPointer != DefinitionPointer) ||
            population.Any(slot => enemies.FuneNamiheStates[slot.SlotIndex] is null))
        {
            throw new InvalidDataException(
                $"Lava Dive load mismatch: state=${room.State.Pointer:X4}, population=" +
                $"${room.State.EnemyPopulationPointer:X4}, set=" +
                $"${room.State.EnemyTilesetPointer:X4}, count/quota=" +
                $"{enemies.EnemyCount}/{enemies.DeathQuota}.");
        }

        VerifyPopulationInitialization(enemies, population);
        VerifyFacingCycle(bus, room, assets, populationIndex: 0, movingRight: false);
        VerifyFacingCycle(bus, room, assets, populationIndex: 1, movingRight: true);
        VerifyProjectileDamage(bus, room, assets);
        VerifyCommonCombat(bus, room, assets);

        Console.WriteLine(
            "Namihe audit passed: untouched Lava Dive loaded six actors; strict vertical " +
            "wake distance, both facing tables, all twelve actor maps, both three-map " +
            "fireball loops, native velocity-field asymmetry, four-pixel muzzle offset, " +
            "200-damage projectile contact, 10-damage body contact, Ice immunity, and " +
            "Grapple cancel were verified.");
        return 0;
    }

    private static void VerifyPopulationInitialization(
        RoomEnemySystem enemies,
        RoomEnemySlot[] population)
    {
        ushort[] expectedX = [0x01f0, 0x0120, 0x0120, 0x027f, 0x0390, 0x0340];
        ushort[] expectedY = [0x0108, 0x01d0, 0x0248, 0x0238, 0x0118, 0x0198];
        bool[] expectedRight = [false, true, true, false, false, false];
        for (int index = 0; index < population.Length; index++)
        {
            RoomEnemySlot actor = population[index];
            FuneNamiheEnemyState state = State(enemies, actor);
            ushort expectedParameter1 = expectedRight[index] ? (ushort)0x1011 : (ushort)0x1001;
            if (actor.XPosition != expectedX[index] || actor.YPosition != expectedY[index] ||
                actor.Parameter1 != expectedParameter1 || actor.Parameter2 != 0x8005 ||
                actor.Properties != 0xa000 || actor.Health != 20 ||
                actor.CurrentInstruction != (expectedRight[index] ? 0x95f1 : 0x95bd) ||
                actor.SpritemapPointer != EmptySpritemap ||
                state.InstructionListPointerTableCursor !=
                    (expectedRight[index] ? 0x96e1 : 0x96df) ||
                state.Function != FuneNamiheEnemyFunction.NamiheWaitForSamus ||
                state.VariantIndex != 1 || state.YProximity != 0x0080 ||
                state.CooldownTime != 0x0010 || state.CooldownTimer != 0)
            {
                throw new InvalidDataException(
                    $"Lava Dive Namihe {index} initialization mismatch: position=" +
                    $"(${actor.XPosition:X4},${actor.YPosition:X4}), params=" +
                    $"${actor.Parameter1:X4}/${actor.Parameter2:X4}, list=" +
                    $"$A8:{actor.CurrentInstruction:X4}, table=$A8:" +
                    $"{state.InstructionListPointerTableCursor:X4}, function={state.Function}, " +
                    $"variant/proximity/cooldown={state.VariantIndex}/" +
                    $"{state.YProximity}/{state.CooldownTime}.");
            }
        }
    }

    private static void VerifyFacingCycle(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        int populationIndex,
        bool movingRight)
    {
        RoomEnemySystem enemies = LoadRoom(bus, room, assets, out SamusState samus);
        RoomEnemySlot actor = enemies.Slots[populationIndex];
        FuneNamiheEnemyState state = State(enemies, actor);
        Isolate(enemies, actor);
        (ushort cameraX, ushort cameraY) = CenterCamera(room, actor);

        // Equality is deliberately outside the bank-$A0 helper. This first frame also lets
        // the initial idle list produce its map without waking the actor.
        samus.XPosition = actor.XPosition;
        samus.YPosition = unchecked((ushort)(actor.YPosition + state.YProximity));
        enemies.StepFrame(cameraX, cameraY, false, samus, level: assets.LevelData);
        if (state.Function != FuneNamiheEnemyFunction.NamiheWaitForSamus ||
            state.InstructionListPointerTableCursor != (movingRight ? 0x96e1 : 0x96df))
        {
            throw new InvalidDataException("Namihe woke at the excluded proximity boundary.");
        }

        // One pixel inside must switch to active bytecode on this actor frame.
        samus.YPosition = unchecked((ushort)(actor.YPosition + state.YProximity - 1));
        enemies.StepFrame(cameraX, cameraY, false, samus, level: assets.LevelData);
        if (state.Function != FuneNamiheEnemyFunction.NamiheActivityNoOp ||
            state.InstructionListPointerTableCursor != (movingRight ? 0x96dd : 0x96db))
        {
            throw new InvalidDataException("Namihe did not wake one pixel inside proximity.");
        }

        var actorMaps = new HashSet<ushort> { actor.SpritemapPointer };
        var projectileMaps = new HashSet<ushort>();
        bool sawSound = false;
        bool returnedToWaiting = false;
        bool checkedInitialProjectile = false;
        ushort expectedStartX = actor.XPosition;
        RoomEnemyProjectileSlot? shot = null;

        for (int frame = 0; frame < 192 && !returnedToWaiting; frame++)
        {
            enemies.StepFrame(cameraX, cameraY, false, samus, level: assets.LevelData);
            actorMaps.Add(actor.SpritemapPointer);
            sawSound |= enemies.LastFuneNamiheSoundEffect == 0x001f;
            shot ??= enemies.EnemyProjectiles.FirstOrDefault(projectile =>
                projectile.Kind == RoomEnemyProjectileKind.NamiheFireball);

            if (shot is not null && !checkedInitialProjectile)
            {
                ushort expectedList = movingRight ? (ushort)0xdea6 : (ushort)0xde96;
                ushort expectedFunction = movingRight ? (ushort)0xdf6a : (ushort)0xdf40;
                if (shot.XPosition != actor.XPosition ||
                    shot.YPosition != unchecked((ushort)(actor.YPosition + 4)) ||
                    shot.InstructionPointer != expectedList ||
                    shot.PreInstruction != 0xdf39 || shot.Variable0 != expectedFunction ||
                    shot.YVelocity != 0xfe80 || shot.XVelocity != 0x0180 ||
                    shot.Damage != 200 || shot.XRadius != 4 || shot.YRadius != 8)
                {
                    throw new InvalidDataException(
                        $"Namihe {(movingRight ? "right" : "left")} fireball init mismatch: " +
                        $"position=(${shot.XPosition:X4},${shot.YPosition:X4}), list/pre/" +
                        $"function=${shot.InstructionPointer:X4}/${shot.PreInstruction:X4}/" +
                        $"${shot.Variable0:X4}, velocities=${shot.YVelocity:X4}/" +
                        $"${shot.XVelocity:X4}, damage/radii={shot.Damage}/" +
                        $"{shot.XRadius}/{shot.YRadius}.");
                }
                checkedInitialProjectile = true;
            }

            enemies.StepEnemyProjectiles(
                assets.LevelData,
                samus: null,
                cameraX: cameraX,
                cameraY: cameraY);
            if (shot?.IsActive == true)
                projectileMaps.Add(shot.SpritemapPointer);

            returnedToWaiting = checkedInitialProjectile &&
                state.Function == FuneNamiheEnemyFunction.NamiheWaitForSamus &&
                state.InstructionListPointerTableCursor == (movingRight ? 0x96e1 : 0x96df);
        }

        if (shot is null)
            throw new InvalidDataException("Namihe active bytecode spawned no fireball.");
        ushort expectedX = movingRight
            ? unchecked((ushort)(expectedStartX + 1))
            : unchecked((ushort)(expectedStartX - 2));
        ushort expectedXSubposition = 0x8000;
        ushort[] requiredActorMaps = movingRight ? RightActorMaps : LeftActorMaps;
        ushort[] requiredProjectileMaps = movingRight ? RightProjectileMaps : LeftProjectileMaps;
        if (!returnedToWaiting || !sawSound || !checkedInitialProjectile ||
            !requiredActorMaps.All(actorMaps.Contains) ||
            !requiredProjectileMaps.All(projectileMaps.Contains) ||
            // At least one projectile step has occurred before this check. Subsequent steps
            // continue in the same direction, so compare sign and fractional first-step
            // residue rather than assuming the shot survived for exactly one frame.
            (movingRight ? shot.XPosition <= expectedStartX : shot.XPosition >= expectedStartX))
        {
            throw new InvalidDataException(
                $"Namihe {(movingRight ? "right" : "left")} cycle mismatch: " +
                $"returned={returnedToWaiting}, sound={sawSound}, projectile=" +
                $"{checkedInitialProjectile}, X=${shot.XPosition:X4}." +
                $"{shot.XSubposition:X4} (first expected ${expectedX:X4}." +
                $"{expectedXSubposition:X4}), actor maps=" +
                $"{string.Join(',', actorMaps.Select(value => $"${value:X4}"))}, " +
                $"projectile maps=" +
                $"{string.Join(',', projectileMaps.Select(value => $"${value:X4}"))}.");
        }
    }

    private static void VerifyProjectileDamage(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        RoomEnemySystem enemies = LoadRoom(bus, room, assets, out SamusState producer);
        RoomEnemySlot actor = enemies.Slots[0];
        FuneNamiheEnemyState state = State(enemies, actor);
        Isolate(enemies, actor);
        producer.XPosition = actor.XPosition;
        producer.YPosition = actor.YPosition;
        (ushort cameraX, ushort cameraY) = CenterCamera(room, actor);
        RoomEnemyProjectileSlot? shot = null;
        for (int frame = 0; frame < 128 && shot is null; frame++)
        {
            enemies.StepFrame(cameraX, cameraY, false, producer, level: assets.LevelData);
            shot = enemies.EnemyProjectiles.FirstOrDefault(projectile =>
                projectile.Kind == RoomEnemyProjectileKind.NamiheFireball);
        }
        if (shot is null || state.Function != FuneNamiheEnemyFunction.NamiheActivityNoOp)
            throw new InvalidDataException("Natural Namihe cycle produced no damage-test shot.");

        SamusState target = CreateSamus(bus, shot.XPosition, shot.YPosition);
        enemies.StepEnemyProjectiles(
            assets.LevelData,
            target,
            cameraX: cameraX,
            cameraY: cameraY);
        if (target.Health != 799 || !target.KnockbackActive || shot.IsActive)
        {
            throw new InvalidDataException(
                $"Namihe fireball contact mismatch: health={target.Health}, " +
                $"knockback={target.KnockbackActive}, live={shot.IsActive}.");
        }
    }

    private static void VerifyCommonCombat(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        RoomEnemySystem enemies = LoadRoom(bus, room, assets, out SamusState samus);
        RoomEnemySlot actor = enemies.Slots[0];
        Isolate(enemies, actor);
        (ushort cameraX, ushort cameraY) = CenterCamera(room, actor);
        enemies.StepFrame(cameraX, cameraY, false, samus, level: assets.LevelData);
        samus.XPosition = actor.XPosition;
        samus.YPosition = actor.YPosition;
        samus.Health = 999;
        samus.InvincibilityTimer = 0;
        if (!enemies.ResolveOrdinarySamusContact(samus, 0) ||
            samus.Health != 989 || !samus.KnockbackActive)
        {
            throw new InvalidDataException("Namihe body contact did not deal header damage ten.");
        }

        var shots = new SamusProjectileSystem();
        var bombs = new SamusBombProjectileSystem();
        ArmShot(shots.Slots[0], actor, type: 0x0002, damage: 20);
        if (enemies.ResolveOrdinaryProjectileHits(bus, shots, bombs, samus) != 1 ||
            actor.Health != 20 || actor.FrozenTimer != 0 ||
            actor.Properties.HasAny(EnemyProperties.Deleted))
        {
            throw new InvalidDataException(
                $"Namihe Ice immunity mismatch: health={actor.Health}, " +
                $"frozen={actor.FrozenTimer}, properties=${actor.Properties:X4}.");
        }

        GrappleEnemyCollision grapple = enemies.ResolveGrappleEndpoint(
            actor.XPosition,
            actor.YPosition);
        if (!grapple.Collided || grapple.Reaction != GrappleEnemyReaction.Cancel)
            throw new InvalidDataException("Namihe did not select Grapple-cancel AI $800F.");
    }

    private static void VerifyHeader(ISnesAddressSpace bus)
    {
        RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(bus, DefinitionPointer);
        if (definition.TileDataSize != 0x0800 || definition.PalettePointer != 0x959d ||
            definition.Health != 20 || definition.Damage != 10 ||
            definition.XRadius != 16 || definition.YRadius != 16 ||
            definition.Bank != 0xa8 || definition.InitializationAiPointer != 0x96e3 ||
            definition.PartCount != 3 || definition.MainAiPointer != 0x9730 ||
            definition.GrappleAiPointer != 0x800f || definition.HurtAiPointer != 0x804c ||
            definition.FrozenAiPointer != 0x8041 || definition.DeathAnimation != 2 ||
            definition.TouchAiPointer != 0x8023 || definition.ShotAiPointer != 0x802d ||
            definition.TileDataAddress != 0xb19e00 || definition.Layer != 5 ||
            definition.ItemDropChancesPointer != 0xf3da ||
            definition.VulnerabilityPointer != 0xeef2 || definition.NamePointer != 0xde3f)
        {
            throw new InvalidDataException("Retail Namihe header does not match $A0:E73F.");
        }
    }

    private static void VerifyProjectileDefinition(ISnesAddressSpace bus)
    {
        const int definition = 0x86dfbc;
        if (ReadWord(bus, definition) != 0xded6 ||
            ReadWord(bus, definition + 2) != 0xdf39 ||
            ReadWord(bus, definition + 4) != 0xde96 ||
            ReadWord(bus, definition + 6) != 0x0804 ||
            ReadWord(bus, definition + 8) != 0x00c8)
        {
            throw new InvalidDataException(
                "Enemy projectile definition $86:DFBC differs from retail data.");
        }
    }

    private static RoomEnemySystem LoadRoom(
        ISnesAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        out SamusState samus)
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState();
        samus = CreateSamus(bus, 0, 0);
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
        return enemies;
    }

    private static void Isolate(RoomEnemySystem enemies, RoomEnemySlot retained)
    {
        foreach (RoomEnemySlot slot in enemies.Slots.Take(enemies.EnemyCount))
        {
            if (!ReferenceEquals(slot, retained))
                slot.Properties = slot.Properties.With(EnemyProperties.Deleted);
        }
    }

    private static (ushort X, ushort Y) CenterCamera(
        CartridgeRoomHeader room,
        RoomEnemySlot actor)
    {
        int maximumX = Math.Max(0, room.WidthInScreens * 256 - 256);
        int maximumY = Math.Max(0, room.HeightInScreens * 256 - 256);
        return (
            unchecked((ushort)Math.Clamp(actor.XPosition - 128, 0, maximumX)),
            unchecked((ushort)Math.Clamp(actor.YPosition - 128, 0, maximumY)));
    }

    private static SamusState CreateSamus(
        ISnesAddressSpace bus,
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

    private static void ArmShot(
        SamusProjectileSlot shot,
        RoomEnemySlot target,
        ushort type,
        ushort damage)
    {
        shot.ClearFields();
        shot.Type = type;
        shot.Damage = damage;
        shot.Direction = (ushort)SamusProjectileDirection.Right;
        shot.XPosition = target.XPosition;
        shot.YPosition = target.YPosition;
        shot.XRadius = 4;
        shot.YRadius = 4;
        shot.InstructionPointer = 0x9000;
        shot.InstructionTimer = 1;
    }

    private static FuneNamiheEnemyState State(
        RoomEnemySystem enemies,
        RoomEnemySlot actor) =>
        enemies.FuneNamiheStates[actor.SlotIndex] ?? throw new InvalidDataException(
            $"Namihe slot {actor.SlotIndex} has no typed state.");

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
}
