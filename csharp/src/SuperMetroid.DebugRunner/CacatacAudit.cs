using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>
/// ROM-backed audit for Cacatac $CFFF and its spike projectile $86:DAFE. Noob Bridge owns
/// both patrol directions in untouched retail data; the audit then selects the second
/// orientation through the same public instruction engine to cover all twenty actor maps
/// and all ten projectile table entries.
/// </summary>
internal static class CacatacAudit
{
    private const ushort RoomPointer = 0x9fba;
    private const ushort DefinitionPointer = 0xcfff;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, RoomPointer);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        VerifyHeader(bus);

        RoomEnemySlot? randomActor = null;
        bool suppressAttack = true;
        ushort NextRandom()
        {
            // MaybeMakeCacatacAttack adds Enemy.frameCounter then keeps the low byte. This
            // callback cancels that add when suppression is requested, or returns its
            // two's-complement inverse to produce decision zero deterministically.
            ushort frame = randomActor?.FrameCounter ?? 0;
            return suppressAttack
                ? unchecked((ushort)(0x0080 - frame))
                : unchecked((ushort)(0 - frame));
        }

        var samus = CreateSamus(bus);
        var enemies = new RoomEnemySystem();
        enemies.Load(
            bus,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            NextRandom,
            level: assets.LevelData,
            samus: samus);

        RoomEnemySlot[] cacatacs = enemies.Slots
            .Take(enemies.EnemyCount)
            .Where(slot => slot.EnemyDefinitionPointer == DefinitionPointer)
            .ToArray();
        if (room.State.Pointer != 0x9fc7 ||
            room.State.EnemyPopulationPointer != 0x92a3 ||
            enemies.EnemyCount != 8 || cacatacs.Length != 4 ||
            cacatacs.Any(actor => enemies.CacatacStates[actor.SlotIndex] is null))
        {
            throw new InvalidDataException(
                $"Noob Bridge state=${room.State.Pointer:X4}, population=" +
                $"${room.State.EnemyPopulationPointer:X4}, count={enemies.EnemyCount}, " +
                $"Cacatacs={cacatacs.Length}.");
        }

        ushort[] expectedX = [0x00c0, 0x01b0, 0x0570, 0x03d0];
        CacatacDirection[] expectedDirection =
        [
            CacatacDirection.Left,
            CacatacDirection.Left,
            CacatacDirection.Right,
            CacatacDirection.Left,
        ];
        for (int index = 0; index < cacatacs.Length; index++)
        {
            RoomEnemySlot actor = cacatacs[index];
            CacatacEnemyState state = State(enemies, actor);
            CacatacEnemyFunction expectedFunction = expectedDirection[index] == CacatacDirection.Left
                ? CacatacEnemyFunction.MovingLeft
                : CacatacEnemyFunction.MovingRight;
            if (actor.XPosition != expectedX[index] || actor.YPosition != 0x00b3 ||
                actor.Parameter1 != (index == 2 ? 0x0101 : 0x0100) ||
                actor.Parameter2 != 0x0301 || actor.CurrentInstruction != 0x9e8a ||
                actor.SpritemapPointer != 0x804d || actor.Health != 60 ||
                state.Direction != expectedDirection[index] || state.Function != expectedFunction ||
                !state.UpsideUp || state.MinimumXPosition != expectedX[index] - 0x40 ||
                state.MaximumXPosition != expectedX[index] + 0x40 ||
                state.RightVelocity != 0 || state.RightSubvelocity != 0x3000 ||
                state.LeftVelocity != 0xffff || state.LeftSubvelocity != 0xd000)
            {
                throw new InvalidDataException(
                    $"Cacatac {index} initialization mismatch: position=" +
                    $"(${actor.XPosition:X4},${actor.YPosition:X4}), params=" +
                    $"${actor.Parameter1:X4}/${actor.Parameter2:X4}, bounds=" +
                    $"${state.MinimumXPosition:X4}-${state.MaximumXPosition:X4}, " +
                    $"speed=${state.LeftVelocity:X4}.{state.LeftSubvelocity:X4}/" +
                    $"${state.RightVelocity:X4}.{state.RightSubvelocity:X4}.");
            }
        }

        RoomEnemySlot audited = cacatacs[0];
        CacatacEnemyState auditedState = State(enemies, audited);
        randomActor = audited;
        foreach (RoomEnemySlot actor in enemies.Slots.Take(enemies.EnemyCount))
        {
            if (!ReferenceEquals(actor, audited))
                actor.Properties = actor.Properties.With(EnemyProperties.Deleted);
        }

        // Exercise the exact carry-sensitive split fixed-point addition on both bounds. The
        // patrol overshoots by its fractional step before changing direction; it never
        // silently clamps to the configured endpoint.
        audited.XPosition = auditedState.MinimumXPosition;
        audited.XSubposition = 0;
        auditedState.Direction = CacatacDirection.Left;
        auditedState.Function = CacatacEnemyFunction.MovingLeft;
        enemies.StepFrame(0, 0, false, samus, level: assets.LevelData);
        if (audited.XPosition != auditedState.MinimumXPosition - 1 ||
            audited.XSubposition != 0xd000 ||
            auditedState.Direction != CacatacDirection.Right ||
            auditedState.Function != CacatacEnemyFunction.MovingRight)
        {
            throw new InvalidDataException(
                $"Cacatac left bound failed: X=${audited.XPosition:X4}." +
                $"{audited.XSubposition:X4}, direction={auditedState.Direction}.");
        }

        audited.XPosition = auditedState.MaximumXPosition;
        audited.XSubposition = 0xf000;
        auditedState.Direction = CacatacDirection.Right;
        auditedState.Function = CacatacEnemyFunction.MovingRight;
        enemies.StepFrame(0, 0, false, samus, level: assets.LevelData);
        if (audited.XPosition != auditedState.MaximumXPosition + 1 ||
            audited.XSubposition != 0x2000 ||
            auditedState.Direction != CacatacDirection.Left ||
            auditedState.Function != CacatacEnemyFunction.MovingLeft)
        {
            throw new InvalidDataException(
                $"Cacatac right bound failed: X=${audited.XPosition:X4}." +
                $"{audited.XSubposition:X4}, direction={auditedState.Direction}.");
        }

        var actorMaps = new HashSet<ushort>();
        CollectIdleMaps(
            enemies,
            audited,
            auditedState,
            samus,
            assets.LevelData,
            instructionList: 0x9e8a,
            upsideUp: true,
            actorMaps);
        CollectIdleMaps(
            enemies,
            audited,
            auditedState,
            samus,
            assets.LevelData,
            instructionList: 0x9eda,
            upsideUp: false,
            actorMaps);

        suppressAttack = false;
        CacatacSpikeDirection[] upward = RunAttack(
            enemies,
            audited,
            auditedState,
            samus,
            assets.LevelData,
            upsideUp: true,
            actorMaps);
        suppressAttack = true;
        VerifyDirections(
            upward,
            CacatacSpikeDirection.LeftFacingUp,
            CacatacSpikeDirection.UpLeft,
            CacatacSpikeDirection.Up,
            CacatacSpikeDirection.UpRight,
            CacatacSpikeDirection.RightFacingUp);
        VerifySpikeInitializationAndMotion(enemies, audited, assets.LevelData, upwardFacing: true);

        // Clear the first volley, then enter the upside-down attack list directly. This is
        // the same instruction engine path used by retail ceiling-mounted variants; only
        // the population-derived orientation discriminator differs from Noob Bridge.
        foreach (RoomEnemyProjectileSlot projectile in enemies.EnemyProjectiles)
            projectile.Clear();
        CacatacSpikeDirection[] downward = RunAttack(
            enemies,
            audited,
            auditedState,
            samus,
            assets.LevelData,
            upsideUp: false,
            actorMaps,
            enterThroughRandomGate: false);
        VerifyDirections(
            downward,
            CacatacSpikeDirection.LeftFacingDown,
            CacatacSpikeDirection.DownLeft,
            CacatacSpikeDirection.Down,
            CacatacSpikeDirection.DownRight,
            CacatacSpikeDirection.RightFacingDown);
        VerifySpikeInitializationAndMotion(enemies, audited, assets.LevelData, upwardFacing: false);

        ushort[] expectedActorMaps =
        [
            0xa0bb, 0xa0db, 0xa0fb, 0xa11b, 0xa13b, 0xa15b, 0xa17b, 0xa19b,
            0xa1bb, 0xa1ef,
            0xa223, 0xa243, 0xa263, 0xa283, 0xa2a3, 0xa2c3, 0xa2e3, 0xa303,
            0xa323, 0xa357,
        ];
        if (!expectedActorMaps.All(actorMaps.Contains))
        {
            throw new InvalidDataException(
                "Cacatac actor animation did not cover all twenty ROM maps: " +
                string.Join(',', actorMaps.Order()));
        }

        var oam = new OamBuffer();
        oam.BeginFrame();
        enemies.DrawLayers(oam, 0, 0, 0, 7);
        enemies.DrawEnemyProjectiles(oam, 0, 0);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException("Cacatac actor/spike ROM maps emitted no OBJ pieces.");

        VerifySpikeContact(enemies, samus, assets.LevelData);
        VerifyActorCombat(bus, room, assets, samus);

        Console.WriteLine(
            "Cacatac audit passed: four untouched Noob Bridge actors loaded; split 16.16 " +
            "patrol/reversal, both orientations, all twenty actor maps, random attack pause/" +
            "resume, sound $34, ten spike directions/maps/velocities, inclusive viewport " +
            "culling, five-damage spike contact, 20-damage body contact, beam death, and " +
            "Grapple kill were verified.");
        return 0;
    }

    private static void CollectIdleMaps(
        RoomEnemySystem enemies,
        RoomEnemySlot actor,
        CacatacEnemyState state,
        SamusState samus,
        RoomLevelData level,
        ushort instructionList,
        bool upsideUp,
        HashSet<ushort> maps)
    {
        state.UpsideUp = upsideUp;
        state.Direction = CacatacDirection.Left;
        state.Function = CacatacEnemyFunction.Stopped;
        actor.CurrentInstruction = instructionList;
        actor.InstructionTimer = 1;
        actor.Timer = 0;
        for (int frame = 0; frame < 72; frame++)
        {
            enemies.StepFrame(0, 0, false, samus, level: level);
            maps.Add(actor.SpritemapPointer);
        }
    }

    private static CacatacSpikeDirection[] RunAttack(
        RoomEnemySystem enemies,
        RoomEnemySlot actor,
        CacatacEnemyState state,
        SamusState samus,
        RoomLevelData level,
        bool upsideUp,
        HashSet<ushort> maps,
        bool enterThroughRandomGate = true)
    {
        state.UpsideUp = upsideUp;
        state.Direction = CacatacDirection.Left;
        state.Function = enterThroughRandomGate
            ? CacatacEnemyFunction.MovingLeft
            : CacatacEnemyFunction.Stopped;
        actor.FrameCounter = 0;
        actor.CurrentInstruction = enterThroughRandomGate
            ? (ushort)(upsideUp ? 0x9e8a : 0x9eda)
            : (ushort)(upsideUp ? 0x9eb0 : 0x9f00);
        actor.InstructionTimer = 1;
        actor.Timer = 0;

        for (int frame = 0; frame < 80; frame++)
        {
            enemies.StepFrame(0, 0, false, samus, level: level);
            maps.Add(actor.SpritemapPointer);
            RoomEnemyProjectileSlot[] spikes = enemies.EnemyProjectiles
                .Where(projectile => projectile.Kind == RoomEnemyProjectileKind.CacatacSpike)
                .ToArray();
            if (spikes.Length == 5)
            {
                if (enemies.LastCacatacSoundEffect != 0x0034 ||
                    state.Function == CacatacEnemyFunction.Stopped)
                {
                    throw new InvalidDataException(
                        $"Cacatac attack did not publish sound/resume patrol: sound=" +
                        $"${enemies.LastCacatacSoundEffect:X4}, function={state.Function}.");
                }
                return spikes
                    .Select(projectile => (CacatacSpikeDirection)projectile.DirectionParameter)
                    .Order()
                    .ToArray();
            }
        }
        throw new InvalidDataException("Cacatac attack did not spawn five spikes within 80 frames.");
    }

    private static void VerifySpikeInitializationAndMotion(
        RoomEnemySystem enemies,
        RoomEnemySlot source,
        RoomLevelData level,
        bool upwardFacing)
    {
        RoomEnemyProjectileSlot[] spikes = enemies.EnemyProjectiles
            .Where(projectile => projectile.Kind == RoomEnemyProjectileKind.CacatacSpike)
            .ToArray();
        if (spikes.Length != 5 || spikes.Any(projectile =>
                projectile.PreInstruction != 0xd9db || projectile.XRadius != 2 ||
                projectile.YRadius != 2 || projectile.Damage != 5 ||
                projectile.GraphicsIndex != (source.PaletteIndex | source.VramTilesIndex) ||
                projectile.XPosition != source.XPosition || projectile.YPosition != source.YPosition))
        {
            throw new InvalidDataException("Cacatac spikes did not copy their ROM definition/source origin.");
        }

        ushort originX = source.XPosition;
        ushort originY = source.YPosition;
        ushort originXSubposition = source.XSubposition;
        ushort originYSubposition = source.YSubposition;
        enemies.StepEnemyProjectiles(level, samus: null, cameraX: 0, cameraY: 0);
        foreach (RoomEnemyProjectileSlot spike in spikes)
        {
            CacatacSpikeDirection direction = (CacatacSpikeDirection)spike.DirectionParameter;
            (ushort xVelocity, ushort yVelocity, ushort map) = ExpectedSpikeMotion(direction);
            (ushort expectedX, ushort expectedXSubposition) =
                AddEightBitVelocityReference(originX, originXSubposition, xVelocity);
            (ushort expectedY, ushort expectedYSubposition) =
                AddEightBitVelocityReference(originY, originYSubposition, yVelocity);
            if (spike.XPosition != expectedX || spike.YPosition != expectedY ||
                spike.XSubposition != expectedXSubposition ||
                spike.YSubposition != expectedYSubposition ||
                spike.SpritemapPointer != map)
            {
                throw new InvalidDataException(
                    $"Cacatac spike {direction} first step mismatch: " +
                    $"(${spike.XPosition:X4}.{spike.XSubposition:X4}," +
                    $"${spike.YPosition:X4}.{spike.YSubposition:X4}), map=" +
                    $"$8D:{spike.SpritemapPointer:X4}.");
            }
        }

        _ = upwardFacing; // Direction records above independently prove the selected half.

        // Place one projectile exactly on each inclusive edge: $DAC2 retains equality and
        // deletes only after the subsequent movement carries the actor strictly outside.
        RoomEnemyProjectileSlot edge = spikes[0];
        edge.Variable0 = (ushort)CacatacSpikeDirection.RightFacingUp;
        edge.XPosition = 0x00fe;
        edge.XSubposition = 0;
        edge.YPosition = 0x0080;
        enemies.StepEnemyProjectiles(level, samus: null, cameraX: 0, cameraY: 0);
        if (!edge.IsActive || edge.XPosition != 0x0100)
            throw new InvalidDataException("Cacatac spike did not retain camera+256 equality.");
        enemies.StepEnemyProjectiles(level, samus: null, cameraX: 0, cameraY: 0);
        if (edge.IsActive)
            throw new InvalidDataException("Cacatac spike was not deleted beyond camera+256.");
    }

    private static (ushort XVelocity, ushort YVelocity, ushort Map)
        ExpectedSpikeMotion(CacatacSpikeDirection direction) => direction switch
    {
        CacatacSpikeDirection.LeftFacingUp => (0xfe00, 0, 0xa908),
        CacatacSpikeDirection.Up => (0, 0xfe00, 0xa916),
        CacatacSpikeDirection.RightFacingUp => (0x0200, 0, 0xa924),
        CacatacSpikeDirection.LeftFacingDown => (0xfe00, 0, 0xa92b),
        CacatacSpikeDirection.Down => (0, 0x0200, 0xa939),
        CacatacSpikeDirection.RightFacingDown => (0x0200, 0, 0xa947),
        CacatacSpikeDirection.UpLeft => (0xfe80, 0xfe80, 0xa90f),
        CacatacSpikeDirection.UpRight => (0x0180, 0xfe80, 0xa91d),
        CacatacSpikeDirection.DownLeft => (0xfe80, 0x0180, 0xa932),
        CacatacSpikeDirection.DownRight => (0x0180, 0x0180, 0xa940),
        _ => throw new InvalidDataException($"Unexpected Cacatac spike direction {direction}."),
    };

    private static (ushort Position, ushort Subposition) AddEightBitVelocityReference(
        ushort position,
        ushort subposition,
        ushort velocity)
    {
        uint fixedPosition = ((uint)position << 16) | subposition;
        int signedVelocity = unchecked((short)velocity);
        uint result = unchecked(fixedPosition + (uint)(signedVelocity << 8));
        return (unchecked((ushort)(result >> 16)), unchecked((ushort)result));
    }

    private static void VerifySpikeContact(
        RoomEnemySystem enemies,
        SamusState samus,
        RoomLevelData level)
    {
        foreach (RoomEnemyProjectileSlot projectile in enemies.EnemyProjectiles)
        {
            if (projectile.Kind != RoomEnemyProjectileKind.CacatacSpike)
                projectile.Clear();
        }
        RoomEnemyProjectileSlot target = enemies.EnemyProjectiles.First(
            projectile => projectile.Kind == RoomEnemyProjectileKind.CacatacSpike);
        foreach (RoomEnemyProjectileSlot projectile in enemies.EnemyProjectiles)
        {
            if (!ReferenceEquals(projectile, target))
                projectile.Clear();
        }

        target.Variable0 = (ushort)CacatacSpikeDirection.RightFacingUp;
        target.XPosition = 0x0080;
        target.XSubposition = 0;
        target.YPosition = 0x0080;
        target.YSubposition = 0;
        samus.XPosition = 0x0082;
        samus.YPosition = 0x0080;
        samus.Health = 100;
        samus.InvincibilityTimer = 0;
        samus.KnockbackActive = false;
        enemies.StepEnemyProjectiles(level, samus, cameraX: 0, cameraY: 0);
        if (samus.Health != 95 || samus.InvincibilityTimer != 96 ||
            !samus.KnockbackActive || target.IsActive)
        {
            throw new InvalidDataException(
                $"Cacatac spike contact failed: health={samus.Health}, " +
                $"invincibility={samus.InvincibilityTimer}, " +
                $"knockback={samus.KnockbackActive}, active={target.IsActive}.");
        }
    }

    private static void VerifyActorCombat(
        ISnesAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        SamusState samus)
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var enemies = new RoomEnemySystem();
        enemies.Load(
            bus,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            () => 0x0080,
            level: assets.LevelData,
            samus: samus);
        RoomEnemySlot[] actors = enemies.Slots
            .Take(enemies.EnemyCount)
            .Where(slot => slot.EnemyDefinitionPointer == DefinitionPointer)
            .ToArray();

        enemies.StepFrame(0, 0, false, samus, level: assets.LevelData);
        samus.Health = 999;
        samus.InvincibilityTimer = 0;
        samus.KnockbackActive = false;
        samus.KnockbackDirection = 0;
        samus.KnockbackTimer = 0;
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.XPosition = actors[0].XPosition;
        samus.YPosition = actors[0].YPosition;
        if (!enemies.ResolveOrdinarySamusContact(samus, 0) ||
            samus.Health != 979 || !samus.KnockbackActive)
        {
            throw new InvalidDataException("Cacatac body contact did not deal header damage 20.");
        }

        var shots = new SamusProjectileSystem();
        var bombs = new SamusBombProjectileSystem();
        // The common enemy-shot routine deliberately halves a projectile's base
        // damage before applying the vulnerability multiplier, exactly as the ROM
        // does.  Cacatac's Power Beam multiplier is one, so a nominal 120-power
        // audit shot is the smallest single hit that can kill its 60 HP target.
        // Actor zero is the Cacatac admitted to the camera-zero interactive list.
        // Actor one is deliberately farther into Noob Bridge and native collision
        // processing must not let an off-screen slot consume this audit projectile.
        ArmProjectile(shots.Slots[0], actors[0], 120);
        if (enemies.ResolveOrdinaryProjectileHits(bus, shots, bombs, samus) != 1 ||
            actors[0].Health != 0 ||
            !actors[0].Properties.HasAny(EnemyProperties.Deleted) ||
            enemies.EnemiesKilled != 1)
        {
            throw new InvalidDataException("Cacatac ordinary beam/death path failed.");
        }

        foreach (RoomEnemySlot actor in enemies.Slots.Take(enemies.EnemyCount))
        {
            if (!ReferenceEquals(actor, actors[2]))
                actor.Properties = actor.Properties.With(EnemyProperties.Deleted);
        }
        enemies.StepFrame(0x0480, 0, false, samus, level: assets.LevelData);
        GrappleEnemyCollision grapple = enemies.ResolveGrappleEndpoint(
            actors[2].XPosition,
            actors[2].YPosition);
        if (!grapple.Collided || grapple.Reaction != GrappleEnemyReaction.Kill)
            throw new InvalidDataException("Cacatac did not select Grapple-kill AI $800A.");
        enemies.StepFrame(0x0480, 0, false, samus, level: assets.LevelData);
        if (!actors[2].Properties.HasAny(EnemyProperties.Deleted) ||
            enemies.EnemiesKilled != 2)
        {
            throw new InvalidDataException("Cacatac Grapple kill did not delete its target.");
        }
    }

    private static void VerifyDirections(
        CacatacSpikeDirection[] actual,
        params CacatacSpikeDirection[] expected)
    {
        CacatacSpikeDirection[] sortedExpected = expected.Order().ToArray();
        if (!actual.SequenceEqual(sortedExpected))
        {
            throw new InvalidDataException(
                $"Cacatac spike directions [{string.Join(',', actual)}] != " +
                $"[{string.Join(',', sortedExpected)}].");
        }
    }

    private static void VerifyHeader(ISnesAddressSpace bus)
    {
        RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(bus, DefinitionPointer);
        if (definition.TileDataSize != 0x0400 || definition.PalettePointer != 0x9e6a ||
            definition.Health != 60 || definition.Damage != 20 ||
            definition.XRadius != 8 || definition.YRadius != 8 || definition.Bank != 0xa2 ||
            definition.InitializationAiPointer != 0x9f48 || definition.PartCount != 1 ||
            definition.MainAiPointer != 0x9fb3 || definition.GrappleAiPointer != 0x800a ||
            definition.HurtAiPointer != 0x804c || definition.FrozenAiPointer != 0x8041 ||
            definition.DeathAnimation != 2 || definition.PowerBombReactionPointer != 0 ||
            definition.TouchAiPointer != 0x8023 || definition.ShotAiPointer != 0x802d ||
            definition.InitialSpritemapPointer != 0 ||
            definition.TileDataAddress != 0xace600 || definition.Layer != 5 ||
            definition.ItemDropChancesPointer != 0xf2ba ||
            definition.VulnerabilityPointer != 0xec1c)
        {
            throw new InvalidDataException("Retail Cacatac header words do not match $A0:CFFF.");
        }
    }

    private static SamusState CreateSamus(ISnesAddressSpace bus)
    {
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusPoseIds.FacingRightNormalPose,
            XPosition = 0,
            YPosition = 0,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        return samus;
    }

    private static CacatacEnemyState State(RoomEnemySystem enemies, RoomEnemySlot actor) =>
        enemies.CacatacStates[actor.SlotIndex] ?? throw new InvalidDataException(
            $"Cacatac slot {actor.SlotIndex} has no typed state.");

    private static void ArmProjectile(
        SamusProjectileSlot projectile,
        RoomEnemySlot target,
        ushort damage)
    {
        projectile.ClearFields();
        projectile.Type = 0;
        projectile.Damage = damage;
        projectile.Direction = (ushort)SamusProjectileDirection.Right;
        projectile.XPosition = target.XPosition;
        projectile.YPosition = target.YPosition;
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }
}
