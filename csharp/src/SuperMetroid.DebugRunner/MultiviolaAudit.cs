using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Untouched-ROM audit for Multiviola $D1BF. Single Chamber contains three cartridge
/// population records beside four already-translated Alcoons. The audit first proves that
/// complete room load, then isolates the fixed-angle actor against transparent and bounded
/// collision fixtures so every reflection can be asserted without changing ROM data.
/// </summary>
internal static class MultiviolaAudit
{
    private const ushort RoomPointer = 0xad5e;
    private const ushort DefinitionPointer = 0xd1bf;
    private const ushort PopulationPointer = 0xb81c;
    private const ushort TilesetPointer = 0x8aa9;
    private const ushort StatePointer = 0xad6b;
    private const ushort InstructionList = 0xb2dc;
    private const ushort EmptySpritemap = 0x804d;
    private const ushort TestOrigin = 0x0080;
    private const ushort BoundaryCamera = 0x0300;

    private static readonly ushort[] AnimationMaps =
    [
        0xb4aa,
        0xb4b1,
        0xb4b8,
        0xb4bf,
        0xb4c6,
        0xb4cd,
        0xb4d4,
        0xb4db,
    ];

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, RoomPointer);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        VerifyHeader(bus);

        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var samus = CreateSamus(bus);
        var enemies = new RoomEnemySystem();
        enemies.Load(
            bus,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            () => 0x1234,
            level: assets.LevelData,
            samus: samus);

        RoomEnemySlot[] actors = enemies.Slots
            .Take(enemies.EnemyCount)
            .Where(slot => slot.EnemyDefinitionPointer == DefinitionPointer)
            .ToArray();
        if (room.State.Pointer != StatePointer ||
            room.State.EnemyPopulationPointer != PopulationPointer ||
            room.State.EnemyTilesetPointer != TilesetPointer ||
            enemies.EnemyCount != 7 || actors.Length != 3 ||
            actors.Any(actor => enemies.MultiviolaStates[actor.SlotIndex] is null))
        {
            throw new InvalidDataException(
                $"Single Chamber state=${room.State.Pointer:X4}, population=" +
                $"${room.State.EnemyPopulationPointer:X4}, set=" +
                $"${room.State.EnemyTilesetPointer:X4}, count={enemies.EnemyCount}, " +
                $"Multiviolas={actors.Length}.");
        }

        // The middle record has no regional angle variant and starts in the visible part of
        // the shaft, making it a stable NTSC/PAL audit subject. Initial AI has already run,
        // while InitializeEnemies has replaced its first map with the native empty map.
        RoomEnemySlot actor = actors[1];
        MultiviolaEnemyState state = State(enemies, actor);
        if (actor.XPosition != 0x0099 || actor.YPosition != 0x01e3 ||
            actor.Parameter1 != 0x0058 || actor.Parameter2 != 0x0002 ||
            actor.Properties != 0x2000 || actor.Health != 90 ||
            actor.CurrentInstruction != InstructionList ||
            actor.SpritemapPointer != EmptySpritemap ||
            state.XVelocity != 0x0001 || state.XSubvelocity != 0x1c72 ||
            state.YVelocity != 0x0001 || state.YSubvelocity != 0xa9b4)
        {
            throw new InvalidDataException(
                $"Multiviola initialization mismatch: position=(${actor.XPosition:X4}," +
                $"${actor.YPosition:X4}), params=${actor.Parameter1:X4}/" +
                $"${actor.Parameter2:X4}, velocity=${state.XVelocity:X4}." +
                $"{state.XSubvelocity:X4}/${state.YVelocity:X4}." +
                $"{state.YSubvelocity:X4}, list=$A2:{actor.CurrentInstruction:X4}, " +
                $"map=$A2:{actor.SpritemapPointer:X4}.");
        }

        // Keep the untouched seven-record proof above, then delete only the neighboring
        // actors so their movement and projectile timing cannot obscure this family audit.
        foreach (RoomEnemySlot other in enemies.Slots.Take(enemies.EnemyCount))
        {
            if (!ReferenceEquals(other, actor))
                other.Properties = other.Properties.With(EnemyProperties.Deleted);
        }

        RoomLevelData openLevel = CreateOpenLevel(64, 64);
        VerifyMovementAndNativeNegation(enemies, actor, state, samus, openLevel);
        VerifyBoundaryReflections(enemies, actor, state, samus, openLevel);
        VerifyAnimationAndDrawing(enemies, actor, samus, openLevel);
        VerifyCombat(bus, room, assets);
        VerifyGrappleCancel(bus, room, assets);

        Console.WriteLine(
            "Multiviola audit passed: untouched Single Chamber loaded three actors beside " +
            "four Alcoons; unsigned ROM sine/cosine motion, both native zero-fraction " +
            "negation bugs, horizontal/vertical terrain reflections, all eight animation " +
            "maps, one-piece OBJ output, 50-damage contact, beam death, Ice multiplier, and Grapple " +
            "cancel were verified.");
        return 0;
    }

    private static void VerifyMovementAndNativeNegation(
        RoomEnemySystem enemies,
        RoomEnemySlot actor,
        MultiviolaEnemyState state,
        SamusState samus,
        RoomLevelData openLevel)
    {
        actor.XPosition = TestOrigin;
        actor.XSubposition = 0;
        actor.YPosition = TestOrigin;
        actor.YSubposition = 0;
        actor.Parameter1 = 0x0058;
        actor.Parameter2 = 0xff02; // The native routine deliberately ignores this high byte.
        enemies.StepFrame(0, 0, false, samus, level: openLevel);
        if (actor.XPosition != 0x0081 || actor.XSubposition != 0x1c72 ||
            actor.YPosition != 0x0081 || actor.YSubposition != 0xa9b4 ||
            state.XVelocity != 0x0001 || state.XSubvelocity != 0x1c72 ||
            state.YVelocity != 0x0001 || state.YSubvelocity != 0xa9b4)
        {
            throw new InvalidDataException(
                $"Multiviola angle $58/speed 2 motion mismatch: position=" +
                $"(${actor.XPosition:X4}.{actor.XSubposition:X4}," +
                $"${actor.YPosition:X4}.{actor.YSubposition:X4}), velocity=" +
                $"${state.XVelocity:X4}.{state.XSubvelocity:X4}/" +
                $"${state.YVelocity:X4}.{state.YSubvelocity:X4}.");
        }

        // At angle $C0, |cos| is exactly zero but X is in the negative half-plane. Retail
        // complements the zero whole word and separately increments the complemented zero
        // fraction, producing $FFFF.0000 (-1.0) instead of mathematical zero. The Y product
        // is $0001.FFFE and its ordinary nonzero-fraction negation becomes $FFFE.0002.
        actor.XPosition = 0x0200;
        actor.XSubposition = 0;
        actor.YPosition = 0x0200;
        actor.YSubposition = 0;
        actor.Parameter1 = 0x00c0;
        actor.Parameter2 = 0x0002;
        enemies.StepFrame(0x0180, 0x0180, false, samus, level: openLevel);
        if (actor.XPosition != 0x01ff || actor.XSubposition != 0 ||
            actor.YPosition != 0x01fe || actor.YSubposition != 0x0002 ||
            state.XVelocity != 0xffff || state.XSubvelocity != 0 ||
            state.YVelocity != 0xfffe || state.YSubvelocity != 0x0002)
        {
            throw new InvalidDataException("Multiviola horizontal zero-fraction negation bug diverged.");
        }

        // Angle $80 is the vertical counterpart: the zero sine magnitude is transformed
        // into $FFFF.0000. Its horizontal component is in the positive half-plane and
        // therefore retains the unsigned maximum product $0001.FFFE.
        actor.XPosition = 0x0200;
        actor.XSubposition = 0;
        actor.YPosition = 0x0200;
        actor.YSubposition = 0;
        actor.Parameter1 = 0x0080;
        enemies.StepFrame(0x0180, 0x0180, false, samus, level: openLevel);
        if (actor.XPosition != 0x0201 || actor.XSubposition != 0xfffe ||
            actor.YPosition != 0x01ff || actor.YSubposition != 0 ||
            state.XVelocity != 0x0001 || state.XSubvelocity != 0xfffe ||
            state.YVelocity != 0xffff || state.YSubvelocity != 0)
        {
            throw new InvalidDataException("Multiviola vertical zero-fraction negation bug diverged.");
        }
    }

    private static void VerifyBoundaryReflections(
        RoomEnemySystem enemies,
        RoomEnemySlot actor,
        MultiviolaEnemyState state,
        SamusState samus,
        RoomLevelData openLevel)
    {
        ushort boundaryCenter = unchecked((ushort)(openLevel.WidthInBlocks * 16 - actor.XRadius));
        actor.XPosition = boundaryCenter;
        actor.XSubposition = 0;
        actor.YPosition = TestOrigin;
        actor.YSubposition = 0;
        // Angle $80 points right in the native screen-coordinate sign convention. The
        // horizontal collision reflects it by XOR $40 to angle $C0.
        actor.Parameter1 = 0x0080;
        actor.Parameter2 = 0x0002;
        enemies.StepFrame(BoundaryCamera, 0, false, samus, level: openLevel);
        if (actor.XPosition != boundaryCenter || actor.XSubposition != 0xffff ||
            actor.Parameter1 != 0x00c0 ||
            state.XVelocity != 0x0001 || state.XSubvelocity != 0xfffe)
        {
            throw new InvalidDataException(
                $"Multiviola horizontal reflection mismatch: X=${actor.XPosition:X4}." +
                $"{actor.XSubposition:X4}, angle=${actor.Parameter1:X4}.");
        }

        boundaryCenter = unchecked((ushort)(openLevel.HeightInBlocks * 16 - actor.YRadius));
        actor.XPosition = TestOrigin;
        actor.XSubposition = 0;
        actor.YPosition = boundaryCenter;
        actor.YSubposition = 0;
        actor.Parameter1 = 0x0040;
        enemies.StepFrame(0, BoundaryCamera, false, samus, level: openLevel);
        if (actor.YPosition != boundaryCenter || actor.YSubposition != 0xffff ||
            actor.Parameter1 != 0x0080 ||
            state.YVelocity != 0x0001 || state.YSubvelocity != 0xfffe)
        {
            throw new InvalidDataException(
                $"Multiviola vertical reflection mismatch: Y=${actor.YPosition:X4}." +
                $"{actor.YSubposition:X4}, angle=${actor.Parameter1:X4}.");
        }
    }

    private static void VerifyAnimationAndDrawing(
        RoomEnemySystem enemies,
        RoomEnemySlot actor,
        SamusState samus,
        RoomLevelData openLevel)
    {
        actor.XPosition = TestOrigin;
        actor.XSubposition = 0;
        actor.YPosition = TestOrigin;
        actor.YSubposition = 0;
        actor.Parameter1 = 0x0058;
        actor.Parameter2 = 1;
        actor.CurrentInstruction = InstructionList;
        actor.InstructionTimer = 1;
        actor.Timer = 0;
        actor.SpritemapPointer = EmptySpritemap;

        var seenMaps = new HashSet<ushort>();
        for (int frame = 0; frame < 141; frame++)
        {
            enemies.StepFrame(0, 0, false, samus, level: openLevel);
            seenMaps.Add(actor.SpritemapPointer);
        }
        if (!AnimationMaps.All(seenMaps.Contains))
        {
            throw new InvalidDataException(
                "Multiviola's fourteen-step ROM animation omitted maps: " +
                string.Join(',', AnimationMaps.Where(map => !seenMaps.Contains(map))
                    .Select(map => $"${map:X4}")));
        }

        var oam = new OamBuffer();
        oam.BeginFrame();
        enemies.DrawLayers(oam, 0, 0, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount != 1)
            throw new InvalidDataException("Multiviola's one-piece ROM map did not emit one OBJ.");
    }

    private static void VerifyCombat(
        ISnesAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        RoomEnemySystem enemies = LoadIsolated(bus, room, assets, out RoomEnemySlot actor);
        var samus = CreateSamus(bus);
        RoomLevelData openLevel = CreateOpenLevel(64, 64);
        actor.XPosition = TestOrigin;
        actor.YPosition = TestOrigin;
        enemies.StepFrame(0, 0, false, samus, level: openLevel);

        samus.XPosition = actor.XPosition;
        samus.YPosition = actor.YPosition;
        samus.Health = 999;
        samus.InvincibilityTimer = 0;
        samus.KnockbackActive = false;
        if (!enemies.ResolveOrdinarySamusContact(samus, 0) ||
            samus.Health != 949 || !samus.KnockbackActive)
        {
            throw new InvalidDataException("Multiviola body contact did not deal header damage 50.");
        }

        var shots = new SamusProjectileSystem();
        var bombs = new SamusBombProjectileSystem();
        ArmPowerBeam(shots.Slots[0], actor);
        if (enemies.ResolveOrdinaryProjectileHits(bus, shots, bombs, samus) != 1 ||
            actor.Health != 0 || !actor.Properties.HasAny(EnemyProperties.Deleted) ||
            enemies.EnemiesKilled != 1)
        {
            throw new InvalidDataException("Multiviola common Power Beam/death path failed.");
        }

        enemies = LoadIsolated(bus, room, assets, out actor);
        samus = CreateSamus(bus);
        openLevel = CreateOpenLevel(64, 64);
        actor.XPosition = TestOrigin;
        actor.YPosition = TestOrigin;
        enemies.StepFrame(0, 0, false, samus, level: openLevel);
        ArmIceBeam(shots.Slots[0], actor);
        // Unlike enemies whose Ice vulnerability byte is $FF, Multiviola's byte is four:
        // the projectile deals ordinary multiplied damage and does not freeze the actor.
        if (enemies.ResolveOrdinaryProjectileHits(bus, shots, bombs, samus) != 1 ||
            actor.Health != 86 || actor.FrozenTimer != 0 ||
            actor.Properties.HasAny(EnemyProperties.Deleted))
        {
            throw new InvalidDataException("Multiviola common Ice Beam multiplier path failed.");
        }
    }

    private static void VerifyGrappleCancel(
        ISnesAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        RoomEnemySystem enemies = LoadIsolated(bus, room, assets, out RoomEnemySlot actor);
        var samus = CreateSamus(bus);
        RoomLevelData openLevel = CreateOpenLevel(64, 64);
        actor.XPosition = TestOrigin;
        actor.YPosition = TestOrigin;
        enemies.StepFrame(0, 0, false, samus, level: openLevel);
        GrappleEnemyCollision collision = enemies.ResolveGrappleEndpoint(
            actor.XPosition,
            actor.YPosition);
        if (!collision.Collided || collision.Reaction != GrappleEnemyReaction.Cancel)
            throw new InvalidDataException("Multiviola did not select Grapple-cancel AI $800F.");
        enemies.StepFrame(0, 0, false, samus, level: openLevel);
        if ((actor.AiHandlerBits & 4) == 0 || actor.Properties.HasAny(EnemyProperties.Deleted))
            throw new InvalidDataException("Multiviola Grapple cancel did not enter frozen AI.");
    }

    private static RoomEnemySystem LoadIsolated(
        ISnesAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        out RoomEnemySlot actor)
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
            () => 0x1234,
            level: assets.LevelData);
        actor = enemies.Slots
            .Take(enemies.EnemyCount)
            .Where(slot => slot.EnemyDefinitionPointer == DefinitionPointer)
            .Skip(1)
            .First();
        foreach (RoomEnemySlot other in enemies.Slots.Take(enemies.EnemyCount))
        {
            if (!ReferenceEquals(other, actor))
                other.Properties = other.Properties.With(EnemyProperties.Deleted);
        }
        return enemies;
    }

    private static void VerifyHeader(ISnesAddressSpace bus)
    {
        RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(bus, DefinitionPointer);
        if (definition.TileDataSize != 0x0400 || definition.PalettePointer != 0xb2bc ||
            definition.Health != 90 || definition.Damage != 50 ||
            definition.XRadius != 8 || definition.YRadius != 8 || definition.Bank != 0xa2 ||
            definition.HurtAiTime != 0 || definition.HurtSoundEffect != 0x003e ||
            definition.BossId != 0 || definition.InitializationAiPointer != 0xb3e0 ||
            definition.PartCount != 1 || definition.Unused16 != 0 ||
            definition.MainAiPointer != 0xb40f || definition.GrappleAiPointer != 0x800f ||
            definition.HurtAiPointer != 0x804c || definition.FrozenAiPointer != 0x8041 ||
            definition.TimeFrozenAiPointer != 0 || definition.DeathAnimation != 0 ||
            definition.PowerBombReactionPointer != 0 ||
            definition.VariantIndex != InstructionList ||
            definition.TouchAiPointer != 0x8023 || definition.ShotAiPointer != 0x802d ||
            definition.InitialSpritemapPointer != 0 ||
            definition.TileDataAddress != 0xaeb400 || definition.Layer != 5 ||
            definition.ItemDropChancesPointer != 0xf362 ||
            definition.VulnerabilityPointer != 0xee6e || definition.NamePointer != 0xdfb9)
        {
            throw new InvalidDataException(
                "Retail Multiviola header words do not match $A0:D1BF.");
        }
    }

    private static RoomLevelData CreateOpenLevel(int width, int height)
    {
        int blockCount = checked(width * height);
        return new RoomLevelData(
            width,
            height,
            new ushort[blockCount],
            new byte[blockCount],
            new ushort[blockCount],
            Array.Empty<byte>());
    }

    private static MultiviolaEnemyState State(
        RoomEnemySystem enemies,
        RoomEnemySlot actor) =>
        enemies.MultiviolaStates[actor.SlotIndex] ?? throw new InvalidDataException(
            $"Multiviola slot {actor.SlotIndex} has no typed state.");

    private static SamusState CreateSamus(ISnesAddressSpace bus)
    {
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusState.FacingRightNormalPose,
            XPosition = 0,
            YPosition = 0,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        return samus;
    }

    private static void ArmPowerBeam(SamusProjectileSlot projectile, RoomEnemySlot target)
    {
        projectile.ClearFields();
        // Multiviola's first vulnerability byte is multiplier two. The common shot routine
        // halves nominal power first, so a 90-power diagnostic beam deals exactly 90 HP.
        projectile.Type = 0;
        projectile.Damage = 90;
        ArmProjectileGeometry(projectile, target);
    }

    private static void ArmIceBeam(SamusProjectileSlot projectile, RoomEnemySlot target)
    {
        projectile.ClearFields();
        projectile.Type = 0x0002;
        projectile.Damage = 2;
        ArmProjectileGeometry(projectile, target);
    }

    private static void ArmProjectileGeometry(
        SamusProjectileSlot projectile,
        RoomEnemySlot target)
    {
        projectile.Direction = (ushort)SamusProjectileDirection.Right;
        projectile.XPosition = target.XPosition;
        projectile.YPosition = target.YPosition;
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }
}
