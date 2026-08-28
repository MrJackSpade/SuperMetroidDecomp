using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>
/// ROM-backed end-to-end audit for Puyo $CFBF. Waterway's untouched population proves the
/// retail loader/header/graphics path; a small deterministic collision room then isolates
/// all seven hop records without depending on player route timing or neighboring enemies.
/// </summary>
internal static class PuyoAudit
{
    private const ushort RoomPointer = 0xa0d2;
    private const ushort DefinitionPointer = 0xcfbf;
    private const ushort SolidBlock = 0x8000;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, RoomPointer);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);

        VerifyHeader(bus);

        // Keep the generator externally steerable. Native Puyo consumes the global seed at
        // two distinct points, so the audit can request each table branch without replacing
        // the production RNG contract or introducing a test-only AI entrypoint.
        ushort nextRandom = 0;
        var samus = CreateSamus(bus);
        RoomEnemySystem enemies = LoadWaterway(
            bus,
            room,
            assets,
            vram,
            cgram,
            samus,
            () => nextRandom);

        RoomEnemySlot[] puyos = enemies.Slots
            .Take(enemies.EnemyCount)
            .Where(slot => slot.EnemyDefinitionPointer == DefinitionPointer)
            .ToArray();
        if (room.State.Pointer != 0xa0df ||
            room.State.EnemyPopulationPointer != 0x911a ||
            enemies.EnemyCount != 7 || puyos.Length != 3 ||
            puyos.Any(slot => enemies.PuyoStates[slot.SlotIndex] is null))
        {
            throw new InvalidDataException(
                $"Waterway state=${room.State.Pointer:X4}, population=" +
                $"${room.State.EnemyPopulationPointer:X4}, count={enemies.EnemyCount}, " +
                $"Puyos={puyos.Length}.");
        }

        ushort[] expectedX = [0x0169, 0x01f7, 0x0224];
        ushort[] expectedY = [0x00b3, 0x00b3, 0x00b5];
        for (int index = 0; index < puyos.Length; index++)
        {
            RoomEnemySlot actor = puyos[index];
            PuyoEnemyState state = State(enemies, actor);
            if (actor.XPosition != expectedX[index] || actor.YPosition != expectedY[index] ||
                actor.Parameter1 != 2 || actor.Parameter2 != 1 ||
                actor.CurrentInstruction != 0x99ad || actor.SpritemapPointer != 0x804d ||
                actor.Health != 100 || actor.Properties != 0x2000 ||
                state.YSpeedTableIndex != 0 || state.HopCooldownTimer != 2 ||
                state.Function != PuyoEnemyFunction.Grounded ||
                (ushort)state.AirborneFunction != 0 ||
                state.HopType != PuyoHopType.NormalSmall || state.InvertDirection)
            {
                throw new InvalidDataException(
                    $"Puyo {index} initialization mismatch at " +
                    $"(${actor.XPosition:X4},${actor.YPosition:X4}), params=" +
                    $"${actor.Parameter1:X4}/${actor.Parameter2:X4}, list=" +
                    $"${actor.CurrentInstruction:X4}, function=" +
                    $"$A2:{(ushort)state.Function:X4}.");
            }
        }

        RoomEnemySlot audited = puyos[0];
        PuyoEnemyState auditedState = State(enemies, audited);
        RoomLevelData fixture = CreateCollisionFixture();

        // Stop the two neighboring retail actors from consuming RNG or adding draw maps.
        // The audited slot remains the untouched Waterway definition, palette, tiles, and
        // instruction bank; only its world coordinates are moved into deterministic walls.
        foreach (RoomEnemySlot actor in puyos.Skip(1))
            actor.Properties = actor.Properties.With(EnemyProperties.Deleted);

        var seenFunctions = new HashSet<PuyoAirborneFunction>();
        var seenMaps = new HashSet<ushort>();
        ushort globalMinimumY = ushort.MaxValue;
        int totalAirborneFrames = 0;

        // Normal-small is selected only from proximity result zero plus random bit zero.
        HopResult small = RunCompleteHop(
            enemies,
            audited,
            auditedState,
            samus,
            fixture,
            PuyoHopType.NormalSmall,
            PuyoAirborneFunction.NormalShortHop,
            closeToSamus: false,
            randomChoice: 0,
            nextRandomSetter: value => nextRandom = value,
            seenMaps);
        Accumulate(small);

        // Normal-big shares proximity result zero but consumes random bit one.
        HopResult big = RunCompleteHop(
            enemies,
            audited,
            auditedState,
            samus,
            fixture,
            PuyoHopType.NormalBig,
            PuyoAirborneFunction.NormalBigHop,
            closeToSamus: false,
            randomChoice: 1,
            nextRandomSetter: value => nextRandom = value,
            seenMaps);
        Accumulate(big);

        // Proximity result one leaves the full 0..7 random value alive; values two and up
        // clamp to the otherwise-labelled "unused" long-hop record. Retail data can reach
        // it, so the audit treats it as shipped behavior rather than dead code.
        HopResult longHop = RunCompleteHop(
            enemies,
            audited,
            auditedState,
            samus,
            fixture,
            PuyoHopType.NormalLong,
            PuyoAirborneFunction.NormalLongHop,
            closeToSamus: true,
            randomChoice: 2,
            nextRandomSetter: value => nextRandom = value,
            seenMaps);
        Accumulate(longHop);

        // Types three, five, and six bypass proximity/random type selection entirely. Their
        // arcs must still use their own height, X speed, gravity delta, and landing wrapper.
        HopResult giant = RunCompleteHop(
            enemies,
            audited,
            auditedState,
            samus,
            fixture,
            PuyoHopType.Giant,
            PuyoAirborneFunction.GiantHop,
            closeToSamus: false,
            randomChoice: 0,
            nextRandomSetter: value => nextRandom = value,
            seenMaps);
        Accumulate(giant);

        HopResult droppedSmall = RunCompleteHop(
            enemies,
            audited,
            auditedState,
            samus,
            fixture,
            PuyoHopType.DroppedSmall,
            PuyoAirborneFunction.Dropped,
            closeToSamus: false,
            randomChoice: 0,
            nextRandomSetter: value => nextRandom = value,
            seenMaps);
        Accumulate(droppedSmall);

        HopResult droppedBig = RunCompleteHop(
            enemies,
            audited,
            auditedState,
            samus,
            fixture,
            PuyoHopType.DroppedBig,
            PuyoAirborneFunction.Dropped,
            closeToSamus: false,
            randomChoice: 0,
            nextRandomSetter: value => nextRandom = value,
            seenMaps);
        Accumulate(droppedBig);

        // Type four has no arc: it falls at its table's constant 1.0 px/frame until floor
        // collision, consumes a fresh RNG value, and prepares dropped-small or dropped-big.
        ResetForHop(audited, auditedState, samus, PuyoHopType.Dropping, closeToSamus: false);
        // Record four is not routed through InitiateHop: its zero height and zero X speed
        // would make the cartridge's initial-speed loop non-terminating. A rising terrain
        // collision writes these three words and dispatches $9D98 directly on the next
        // enemy frame, so reproduce that actual entry edge here.
        audited.YPosition = 0x0060;
        auditedState.Function = PuyoEnemyFunction.Airborne;
        auditedState.AirborneFunction = PuyoAirborneFunction.Dropping;
        auditedState.HopTableIndex = 4 * 8;
        auditedState.HoppingAnimationActive = false;
        nextRandom = 1;
        if (auditedState.AirborneFunction != PuyoAirborneFunction.Dropping)
            throw new InvalidDataException("Puyo type four did not select dropping AI $9D98.");
        seenFunctions.Add(auditedState.AirborneFunction);
        int droppingFrames = 0;
        while (auditedState.Function != PuyoEnemyFunction.Grounded)
        {
            if (++droppingFrames > 96)
                throw new InvalidDataException("Puyo dropping AI did not reach the fixture floor.");
            enemies.StepFrame(0, 0, false, samus, level: fixture);
            seenMaps.Add(audited.SpritemapPointer);
        }
        if (auditedState.HopType != PuyoHopType.DroppedBig || audited.YPosition != 0x007c)
        {
            throw new InvalidDataException(
                $"Puyo drop landing mismatch: type={auditedState.HopType}, " +
                $"Y=${audited.YPosition:X4}, frames={droppingFrames}.");
        }

        // The five airborne pose lists reference maps three through seven. Grounded lists
        // reference maps zero through two; together a complete family pass must draw all
        // eight cartridge spritemaps rather than leaving a single placeholder tile.
        ushort[] expectedMaps = [0x9df6, 0x9e02, 0x9e0e, 0x9e1a, 0x9e26, 0x9e37, 0x9e4d, 0x9e5e];
        if (!expectedMaps.All(seenMaps.Contains) || seenFunctions.Count != 6)
        {
            throw new InvalidDataException(
                $"Puyo coverage missed ROM maps/functions: maps=" +
                $"{string.Join(',', seenMaps.Order())}, functions=" +
                $"{string.Join(',', seenFunctions.Order())}.");
        }

        var oam = new OamBuffer();
        oam.BeginFrame();
        enemies.DrawLayers(oam, 0, 0, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException("Live Puyo ROM maps emitted no OBJ pieces.");

        VerifyCombat(bus, room, assets, samus);

        Console.WriteLine(
            "Puyo audit passed: Waterway loaded three untouched actors; all seven hop " +
            $"records, six indirect functions, eight ROM maps, {totalAirborneFrames} " +
            $"airborne frames down to Y ${globalMinimumY:X4}, constant-speed dropping, " +
            "terrain landing, OBJ output, 60-damage contact, ordinary shot/death, and " +
            "Grapple-kill behavior were verified.");
        return 0;

        void Accumulate(HopResult result)
        {
            seenFunctions.Add(result.Function);
            globalMinimumY = Math.Min(globalMinimumY, result.MinimumY);
            totalAirborneFrames += result.AirborneFrames;
        }
    }

    private static HopResult RunCompleteHop(
        RoomEnemySystem enemies,
        RoomEnemySlot actor,
        PuyoEnemyState state,
        SamusState samus,
        RoomLevelData level,
        PuyoHopType expectedType,
        PuyoAirborneFunction expectedFunction,
        bool closeToSamus,
        ushort randomChoice,
        Action<ushort> nextRandomSetter,
        HashSet<ushort> seenMaps)
    {
        // Types zero through two begin from type zero because the ROM derives their final
        // record from proximity plus RNG. Types three and above are direct table indexes.
        PuyoHopType requested = (ushort)expectedType < 3
            ? PuyoHopType.NormalSmall
            : expectedType;
        ResetForHop(actor, state, samus, requested, closeToSamus);

        // Choose the callback value modulo eight after accounting for Enemy.frameCounter,
        // exactly as GetRandomNumber0_7 adds that word after advancing the global seed.
        nextRandomSetter(unchecked((ushort)((randomChoice - actor.FrameCounter) & 7)));
        enemies.StepFrame(0, 0, false, samus, level: level);
        if (state.Function != PuyoEnemyFunction.Airborne ||
            state.HopType != expectedType || state.AirborneFunction != expectedFunction ||
            state.HopTableIndex != (ushort)((ushort)expectedType * 8) ||
            state.YSpeedTableIndex == 0 || !state.HoppingAnimationActive)
        {
            throw new InvalidDataException(
                $"Puyo {expectedType} initiation mismatch: type={state.HopType}, " +
                $"function=$A2:{(ushort)state.AirborneFunction:X4}, table=" +
                $"${state.HopTableIndex:X4}, Y-index=${state.YSpeedTableIndex:X4}.");
        }

        ushort minimumY = actor.YPosition;
        int frames = 0;
        while (state.Function != PuyoEnemyFunction.Grounded)
        {
            if (++frames > 256)
                throw new InvalidDataException($"Puyo {expectedType} did not land within 256 frames.");
            enemies.StepFrame(0, 0, false, samus, level: level);
            seenMaps.Add(actor.SpritemapPointer);
            minimumY = Math.Min(minimumY, actor.YPosition);
        }

        if (actor.YPosition != 0x007c || minimumY >= actor.YPosition || frames < 2)
        {
            throw new InvalidDataException(
                $"Puyo {expectedType} arc mismatch: Y=${minimumY:X4}-" +
                $"${actor.YPosition:X4}, frames={frames}.");
        }
        return new HopResult(expectedFunction, minimumY, frames);
    }

    private static void ResetForHop(
        RoomEnemySlot actor,
        PuyoEnemyState state,
        SamusState samus,
        PuyoHopType requested,
        bool closeToSamus)
    {
        // The solid fixture floor begins at world Y $0080. Puyo's four-pixel radius places
        // its grounded center at $007C, precisely where MoveEnemyDown aligns a collision.
        actor.XPosition = 0x0080;
        actor.XSubposition = 0;
        actor.YPosition = 0x007c;
        actor.YSubposition = 0;
        actor.FrameCounter = 0;
        actor.Properties = actor.Properties.Without(EnemyProperties.Deleted);
        actor.CurrentInstruction = 0x99ad;
        actor.InstructionTimer = 1;
        actor.Timer = 0;

        state.YSpeedTableIndex = 0;
        state.HopCooldownTimer = 0;
        state.Function = PuyoEnemyFunction.Grounded;
        state.AirborneFunction = PuyoAirborneFunction.NormalShortHop;
        state.HopTableIndex = 0;
        state.HopType = requested;
        state.HoppingAnimationActive = false;
        state.Direction = PuyoDirection.Right;
        state.Falling = false;
        state.InvertDirection = false;
        state.InvertedDirection = PuyoDirection.Left;
        state.InitialYSpeedTableIndexHalf = 0;
        state.InitialYSpeedTableIndexThreeQuarters = 0;

        samus.XPosition = closeToSamus ? actor.XPosition : (ushort)0x0010;
        samus.YPosition = actor.YPosition;
        samus.InvincibilityTimer = 0;
        samus.KnockbackActive = false;
    }

    private static void VerifyCombat(
        ISnesAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        SamusState samus)
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        RoomEnemySystem enemies = LoadWaterway(
            bus,
            room,
            assets,
            vram,
            cgram,
            samus,
            () => 0x1234);
        RoomEnemySlot[] actors = enemies.Slots
            .Take(enemies.EnemyCount)
            .Where(slot => slot.EnemyDefinitionPointer == DefinitionPointer)
            .ToArray();

        // Step once to populate the same interactive list used by gameplay collision, then
        // overlap Samus after AI so body contact cannot be invalidated by Puyo movement.
        enemies.StepFrame(0x0100, 0, false, samus, level: assets.LevelData);
        RoomEnemySlot contact = actors[0];
        samus.Health = 999;
        samus.InvincibilityTimer = 0;
        samus.KnockbackActive = false;
        samus.XPosition = contact.XPosition;
        samus.YPosition = contact.YPosition;
        if (!enemies.ResolveOrdinarySamusContact(samus, 0) ||
            samus.Health != 939 || !samus.KnockbackActive)
        {
            throw new InvalidDataException(
                $"Puyo contact failed: health={samus.Health}, knockback={samus.KnockbackActive}.");
        }

        // Default vulnerability gives the basic Power Beam multiplier one. The shared shot
        // handler consumes one 20-damage projectile and leaves the actor alive at 80 HP.
        var projectiles = new SamusProjectileSystem();
        var bombs = new SamusBombProjectileSystem();
        ArmProjectile(projectiles.Slots[0], actors[1], damage: 20);
        if (enemies.ResolveOrdinaryProjectileHits(bus, projectiles, bombs, samus) != 1 ||
            actors[1].Health != 80 || actors[1].FlashTimer == 0)
        {
            throw new InvalidDataException(
                $"Puyo Power Beam hit failed: health={actors[1].Health}, " +
                $"flash={actors[1].FlashTimer}.");
        }

        // A second independently armed shot is lethal and must use the common death path,
        // including Deleted and the room kill counter, not merely clamp health to zero.
        var lethal = new SamusProjectileSystem();
        ArmProjectile(lethal.Slots[0], actors[1], damage: 100);
        if (enemies.ResolveOrdinaryProjectileHits(bus, lethal, bombs, samus) != 1 ||
            actors[1].Health != 0 ||
            !actors[1].Properties.HasAny(EnemyProperties.Deleted) ||
            enemies.EnemiesKilled != 1)
        {
            throw new InvalidDataException(
                $"Puyo death failed: health={actors[1].Health}, " +
                $"properties=${actors[1].Properties:X4}, kills={enemies.EnemiesKilled}.");
        }

        // Waterway also contains Skultera and Zero actors whose generous radii can win the
        // first-hit scan near the third Puyo. Isolate the target, then rebuild the native
        // active/interactive indexes before testing its header-selected reaction.
        foreach (RoomEnemySlot actor in enemies.Slots.Take(enemies.EnemyCount))
        {
            if (!ReferenceEquals(actor, actors[2]))
                actor.Properties = actor.Properties.With(EnemyProperties.Deleted);
        }
        enemies.StepFrame(0x0180, 0, false, samus, level: assets.LevelData);
        GrappleEnemyCollision grapple = enemies.ResolveGrappleEndpoint(
            actors[2].XPosition,
            actors[2].YPosition);
        if (!grapple.Collided || grapple.Reaction != GrappleEnemyReaction.Kill ||
            grapple.EnemyNativeIndex != actors[2].NativeIndex)
        {
            throw new InvalidDataException("Puyo did not select common Grapple-kill AI $800A.");
        }
        enemies.StepFrame(0x0180, 0, false, samus, level: assets.LevelData);
        if (!actors[2].Properties.HasAny(EnemyProperties.Deleted) ||
            enemies.EnemiesKilled != 2)
        {
            throw new InvalidDataException(
                $"Puyo Grapple kill did not delete the actor: properties=" +
                $"${actors[2].Properties:X4}, kills={enemies.EnemiesKilled}.");
        }
    }

    private static RoomEnemySystem LoadWaterway(
        ISnesAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        SnesVram vram,
        SnesCgram cgram,
        SamusState samus,
        Func<ushort> nextRandom)
    {
        var enemies = new RoomEnemySystem();
        enemies.Load(
            bus,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            nextRandom,
            level: assets.LevelData,
            samus: samus);
        return enemies;
    }

    private static SamusState CreateSamus(ISnesAddressSpace bus)
    {
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusState.FacingRightNormalPose,
            XPosition = 0x0010,
            YPosition = 0x007c,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        return samus;
    }

    private static RoomLevelData CreateCollisionFixture()
    {
        const int width = 16;
        const int height = 12;
        var foreground = new ushort[width * height];
        var behavior = new byte[foreground.Length];
        var background = new ushort[foreground.Length];

        // Solid border plus a floor at row eight. The ceiling remains far enough above the
        // 128-pixel giant hop that only intended wall tests can interrupt a rising arc.
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (x == 0 || x == width - 1 || y == 0 || y >= 8)
                    foreground[y * width + x] = SolidBlock;
            }
        }
        return new RoomLevelData(width, height, foreground, behavior, background, []);
    }

    private static void VerifyHeader(ISnesAddressSpace bus)
    {
        RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(bus, DefinitionPointer);
        if (definition.TileDataSize != 0x0200 || definition.PalettePointer != 0x998d ||
            definition.Health != 100 || definition.Damage != 60 ||
            definition.XRadius != 8 || definition.YRadius != 4 || definition.Bank != 0xa2 ||
            definition.InitializationAiPointer != 0x9a3f || definition.PartCount != 1 ||
            definition.MainAiPointer != 0x9a7d || definition.GrappleAiPointer != 0x800a ||
            definition.HurtAiPointer != 0x804c || definition.FrozenAiPointer != 0x8041 ||
            definition.DeathAnimation != 0 || definition.PowerBombReactionPointer != 0 ||
            definition.TouchAiPointer != 0x8023 || definition.ShotAiPointer != 0x802d ||
            definition.InitialSpritemapPointer != 0 ||
            definition.TileDataAddress != 0xace400 || definition.Layer != 5 ||
            definition.ItemDropChancesPointer != 0xf20c ||
            definition.VulnerabilityPointer != 0xec1c)
        {
            throw new InvalidDataException("Retail Puyo header words do not match $A0:CFBF.");
        }
    }

    private static PuyoEnemyState State(RoomEnemySystem enemies, RoomEnemySlot actor) =>
        enemies.PuyoStates[actor.SlotIndex] ?? throw new InvalidDataException(
            $"Puyo slot {actor.SlotIndex} has no typed state.");

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

    private readonly record struct HopResult(
        PuyoAirborneFunction Function,
        ushort MinimumY,
        int AirborneFrames);
}
