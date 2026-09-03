using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>
/// End-to-end ROM audit for the two Nuclear Waffle/Puromi actors in room $8F:B457. The
/// population is used untouched so the test covers the actual parameter packing, both
/// shared child pools, cartridge animations, the complete articulated sweep, and damage.
/// </summary>
internal static class NuclearWaffleAudit
{
    private const ushort RoomPointer = 0xb457;
    private const ushort Definition = 0xe0bf;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, RoomPointer);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);

        LoadedWaffles initialized = Load(bus, room, assets);
        VerifyPopulationAndInitialization(bus, room, initialized);
        SweepResult sweep = VerifyAnimationMovementAndDrawing(bus, room, assets);
        VerifyHeadAndBodyCombat(bus, room, assets);

        Console.WriteLine(
            "Nuclear Waffle audit passed: untouched two-Puromi Norfair population loaded; " +
            $"all eight $BBC7 links loaded exact definition/slot state; head/body/" +
            $"sprite-object animations each covered eight ROM maps, {sweep.WaitFrames} " +
            $"waiting frames and {sweep.SweepFrames} articulated sweep frames moved all " +
            $"seven alternating links, turn overlays and SFX executed, {sweep.ObjPieces} OBJ " +
            "pieces rendered, head contact dealt 50 damage, four persistent projectile links " +
            "dealt 256 in one native collision pass and blocked beams, head shots/power bombs " +
            "passed through, and grapple selected " +
            "the cartridge's no-interaction reaction.");
        return 0;
    }

    private static void VerifyPopulationAndInitialization(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        LoadedWaffles loaded)
    {
        if (room.State.Pointer != 0xb464 ||
            room.State.EnemyPopulationPointer != 0xad6c ||
            loaded.Enemies.EnemyCount != 2 ||
            loaded.Enemies.ActiveEnemyProjectileCount != 8 ||
            loaded.Enemies.RoomSpriteObjects.Count(slot => slot.IsActive) != 6)
        {
            throw new InvalidDataException(
                $"Nuclear Waffle room selected state ${room.State.Pointer:X4}, population " +
                $"${room.State.EnemyPopulationPointer:X4}, actors={loaded.Enemies.EnemyCount}, " +
                $"projectile links={loaded.Enemies.ActiveEnemyProjectileCount}, sprite links=" +
                $"{loaded.Enemies.RoomSpriteObjects.Count(slot => slot.IsActive)}.");
        }

        ushort[] expectedX = [0x0190, 0x02d0];
        for (int actorIndex = 0; actorIndex < 2; actorIndex++)
        {
            RoomEnemySlot actor = loaded.Enemies.Slots[actorIndex];
            NuclearWaffleEnemyState state = RequireState(loaded.Enemies, actor);
            ushort originX = expectedX[actorIndex];
            ushort expectedHeadX = unchecked((ushort)(originX +
                ReadEightBitCosineProduct(bus, 0x00f0, 0x0040)));
            ushort expectedHeadY = unchecked((ushort)(0x00d0 +
                ReadEightBitSineProduct(bus, 0x00f0, 0x0040)));

            if (actor.EnemyDefinitionPointer != Definition ||
                actor.XPosition != expectedHeadX || actor.YPosition != expectedHeadY ||
                actor.Parameter1 != 0x4010 || actor.Parameter2 != 0x2001 ||
                actor.Properties != 0x2000 || actor.Health != 40 ||
                actor.Definition.Damage != 50 || actor.XRadius != 8 || actor.YRadius != 8 ||
                actor.Definition.Bank != 0xa6 ||
                actor.Definition.InitializationAiPointer != 0x94c4 ||
                actor.Definition.MainAiPointer != 0x960e ||
                actor.Definition.GrappleAiPointer != 0x8000 ||
                actor.Definition.TouchAiPointer != 0x8023 ||
                actor.Definition.ShotAiPointer != 0x804c ||
                actor.Definition.VulnerabilityPointer != 0xeec6 ||
                actor.CurrentInstruction != 0x9490 ||
                state.Function != NuclearWaffleEnemyFunction.Waiting ||
                state.WaitingTimer != 0x20 || state.WaitingTimerReset != 0x20 ||
                state.AngularSpeedIndex != 0x10 || state.OrbitRadius != 0x40 ||
                state.Direction != 1 || state.CurrentAngle != 0x00f0 ||
                state.SweepStartAngle != 0x00f0 || state.SweepEndAngle != 0x0190 ||
                state.SegmentSpacing != 0x18 || state.InterleavedSegmentOffset != 0x0c ||
                state.AngularSpeedWhole != 1 || state.AngularSpeedFraction != 0 ||
                state.OriginX != originX || state.OriginY != 0x00d0)
            {
                throw new InvalidDataException(
                    $"Nuclear Waffle {actorIndex} initialization failed: position=" +
                    $"(${actor.XPosition:X4},${actor.YPosition:X4}), params=" +
                    $"${actor.Parameter1:X4}/${actor.Parameter2:X4}, function=" +
                    $"$A6:{(ushort)state.Function:X4}, timer={state.WaitingTimer}, angles=" +
                    $"${state.SweepStartAngle:X4}->${state.SweepEndAngle:X4}, spacing=" +
                    $"{state.SegmentSpacing}/{state.InterleavedSegmentOffset}, speed=" +
                    $"{state.AngularSpeedWhole}.{state.AngularSpeedFraction:X4}.");
            }

            if (state.ProjectileSegments.Any(segment =>
                    segment is null ||
                    segment.Kind != RoomEnemyProjectileKind.NuclearWaffleBody ||
                    segment.XPosition != expectedHeadX || segment.YPosition != expectedHeadY ||
                    segment.InstructionPointer != 0xbb5e ||
                    segment.PreInstruction != 0xbbc6 || segment.XRadius != 8 ||
                    segment.YRadius != 8 || segment.Damage != 0x40 ||
                    !segment.CanDamageSamus || !segment.PersistsOnSamusContact ||
                    !segment.BlocksSamusProjectiles) ||
                state.SpriteSegments.Any(segment =>
                    segment is null ||
                    segment.Kind != RoomSpriteObjectKind.NuclearWaffleBody ||
                    segment.XPosition != expectedHeadX || segment.YPosition != expectedHeadY ||
                    segment.InstructionPointer != 0xc30a))
            {
                throw new InvalidDataException(
                    $"Nuclear Waffle {actorIndex} did not allocate its four damaging " +
                    "projectile and three sprite-object links from the cartridge definitions.");
            }

            for (int segmentIndex = 0;
                 segmentIndex < state.ProjectileSegments.Length;
                 segmentIndex++)
            {
                VerifyBodySegmentDefinition(
                    bus,
                    actor,
                    state,
                    actorIndex,
                    segmentIndex,
                    state.ProjectileSegments[segmentIndex]!);
            }
        }
    }

    private static void VerifyBodySegmentDefinition(
        ISnesAddressSpace bus,
        RoomEnemySlot owner,
        NuclearWaffleEnemyState state,
        int actorIndex,
        int segmentIndex,
        RoomEnemyProjectileSlot segment)
    {
        int definition = 0x860000 | (ushort)RoomEnemyProjectileKind.NuclearWaffleBody;
        ushort packedRadii = ReadNuclearWaffleAuditWord(bus, definition + 6);
        ushort properties = ReadNuclearWaffleAuditWord(bus, definition + 8);
        int expectedSlot = 17 - actorIndex * 4 - segmentIndex;
        if (ReadNuclearWaffleAuditWord(bus, definition) != 0xbb92 ||
            segment.SlotIndex != expectedSlot ||
            segment.PreInstruction != ReadNuclearWaffleAuditWord(bus, definition + 2) ||
            segment.PreInstruction != 0xbbc6 ||
            segment.InstructionPointer != ReadNuclearWaffleAuditWord(bus, definition + 4) ||
            segment.InstructionPointer != 0xbb5e || segment.InstructionTimer != 1 ||
            segment.SpritemapPointer != 0x8000 ||
            segment.XRadius != unchecked((byte)packedRadii) ||
            segment.YRadius != unchecked((byte)(packedRadii >> 8)) ||
            segment.Damage != (properties & 0x0fff) || segment.Damage != 64 ||
            segment.InvincibilityFrames != 96 || !segment.CanDamageSamus ||
            !segment.PersistsOnSamusContact || !segment.BlocksSamusProjectiles ||
            segment.CollisionOption != 1 || segment.GraphicsIndex != state.GraphicsIndex ||
            segment.XPosition != state.InitialHeadX ||
            segment.XSubposition != owner.XSubposition ||
            segment.YPosition != state.InitialHeadY ||
            segment.YSubposition != owner.YSubposition ||
            segment.Variable0 != 0 || segment.Variable1 != 0)
        {
            throw new InvalidDataException(
                $"Nuclear Waffle {actorIndex} body link {segmentIndex} definition " +
                $"$86:BBC7 mismatch: slot=${segment.SlotIndex * 2:X2}, position=" +
                $"(${segment.XPosition:X4}.${segment.XSubposition:X4}," +
                $"${segment.YPosition:X4}.${segment.YSubposition:X4}), list/pre/map=" +
                $"${segment.InstructionPointer:X4}/${segment.PreInstruction:X4}/" +
                $"${segment.SpritemapPointer:X4}, radii={segment.XRadius}/" +
                $"{segment.YRadius}, damage={segment.Damage}, option=" +
                $"{segment.CollisionOption}, flags={segment.CanDamageSamus}/" +
                $"{segment.PersistsOnSamusContact}/{segment.BlocksSamusProjectiles}.");
        }
    }

    private static SweepResult VerifyAnimationMovementAndDrawing(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedWaffles loaded = Load(bus, room, assets);
        RoomEnemySlot actor = loaded.Enemies.Slots[0];
        NuclearWaffleEnemyState state = RequireState(loaded.Enemies, actor);
        var headMaps = new HashSet<ushort>();
        var projectileMaps = new HashSet<ushort>();
        var spriteMaps = new HashSet<ushort>();
        var headPositions = new HashSet<(ushort X, ushort Y)>();
        bool sawTurnSound = false;
        bool sawTurnClockwise = false;
        bool sawTurnCounterClockwise = false;
        bool sawTurnOverlay = false;

        int waitFrames = 0;
        while (state.Function == NuclearWaffleEnemyFunction.Waiting && waitFrames < 64)
        {
            StepGameplayFrame(loaded, room, assets, actor);
            CaptureAnimationMaps(loaded, state, headMaps, projectileMaps, spriteMaps);
            waitFrames++;
        }
        if (waitFrames != 33 || state.Function != NuclearWaffleEnemyFunction.Sweeping ||
            state.CurrentAngle != 0x00f0)
        {
            throw new InvalidDataException(
                $"Nuclear Waffle waiting transition took {waitFrames} frames and reached " +
                $"$A6:{(ushort)state.Function:X4} at angle ${state.CurrentAngle:X4}.");
        }

        int sweepFrames = 0;
        while (state.Function == NuclearWaffleEnemyFunction.Sweeping && sweepFrames < 512)
        {
            StepGameplayFrame(loaded, room, assets, actor);
            CaptureAnimationMaps(loaded, state, headMaps, projectileMaps, spriteMaps);
            headPositions.Add((actor.XPosition, actor.YPosition));
            sawTurnSound |= loaded.Enemies.LastNuclearWaffleSoundEffect == 0x005e;
            sawTurnClockwise |= loaded.Enemies.RoomSpriteObjects.Any(slot =>
                slot.IsActive && slot.Kind == RoomSpriteObjectKind.NuclearWaffleTurnClockwise);
            sawTurnCounterClockwise |= loaded.Enemies.RoomSpriteObjects.Any(slot =>
                slot.IsActive && slot.Kind == RoomSpriteObjectKind.NuclearWaffleTurnCounterClockwise);
            sawTurnOverlay |= loaded.Enemies.RoomSpriteObjects.Any(slot =>
                slot.IsActive && slot.Kind == RoomSpriteObjectKind.NuclearWaffleTurnOverlay);
            sweepFrames++;
        }

        if (sweepFrames != 245 || state.Function != NuclearWaffleEnemyFunction.Waiting ||
            state.CurrentAngle != 0x01e5 ||
            actor.Properties.HasAny(EnemyProperties.ProcessOffScreen) ||
            headPositions.Count < 100 || headMaps.Count != 8 || projectileMaps.Count != 8 ||
            spriteMaps.Count != 8 || !sawTurnSound || !sawTurnClockwise ||
            !sawTurnCounterClockwise || !sawTurnOverlay)
        {
            throw new InvalidDataException(
                $"Nuclear Waffle sweep failed: frames={sweepFrames}, function=" +
                $"$A6:{(ushort)state.Function:X4}, angle=${state.CurrentAngle:X4}, " +
                $"positions={headPositions.Count}, maps={headMaps.Count}/" +
                $"{projectileMaps.Count}/{spriteMaps.Count}, turn objects=" +
                $"{sawTurnClockwise}/{sawTurnCounterClockwise}/{sawTurnOverlay}, " +
                $"sound={sawTurnSound}.");
        }

        // Completion is owned by the final projectile link, so every articulated point has
        // reached the same cartridge endpoint when the function returns to Waiting.
        (ushort endpointX, ushort endpointY) = (
            unchecked((ushort)(state.OriginX +
                ReadEightBitCosineProduct(bus, state.SweepEndAngle, state.OrbitRadius))),
            unchecked((ushort)(state.OriginY +
                ReadEightBitSineProduct(bus, state.SweepEndAngle, state.OrbitRadius))));
        if (actor.XPosition != endpointX || actor.YPosition != endpointY ||
            state.ProjectileSegments.Any(segment =>
                segment is null || segment.XPosition != endpointX || segment.YPosition != endpointY) ||
            state.SpriteSegments.Any(segment =>
                segment is null || segment.XPosition != endpointX || segment.YPosition != endpointY))
        {
            throw new InvalidDataException(
                $"Nuclear Waffle links did not coalesce at endpoint " +
                $"(${endpointX:X4},${endpointY:X4}).");
        }

        var oam = new OamBuffer();
        oam.BeginFrame();
        (ushort cameraX, ushort cameraY) = CenterCamera(room, actor);
        loaded.Enemies.DrawEnemyProjectiles(oam, cameraX, cameraY);
        loaded.Enemies.DrawLayers(oam, cameraX, cameraY, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount < 8)
        {
            throw new InvalidDataException(
                $"Nuclear Waffle composite emitted only {oam.LastFinalizedSpriteCount} OBJ pieces.");
        }

        return new SweepResult(waitFrames, sweepFrames, oam.LastFinalizedSpriteCount);
    }

    private static void VerifyHeadAndBodyCombat(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedWaffles headLoad = Load(bus, room, assets);
        RoomEnemySlot head = headLoad.Enemies.Slots[0];
        StepGameplayFrame(headLoad, room, assets, head);
        headLoad.Samus.XPosition = head.XPosition;
        headLoad.Samus.YPosition = head.YPosition;
        headLoad.Samus.InvincibilityTimer = 0;
        ushort health = headLoad.Samus.Health;
        if (!headLoad.Enemies.ResolveOrdinarySamusContact(headLoad.Samus, 0) ||
            headLoad.Samus.Health != health - 50 || !headLoad.Samus.KnockbackActive)
        {
            throw new InvalidDataException(
                $"Nuclear Waffle head contact failed: health={health}->" +
                $"{headLoad.Samus.Health}, knockback={headLoad.Samus.KnockbackActive}.");
        }

        LoadedWaffles bodyLoad = Load(bus, room, assets);
        NuclearWaffleEnemyState bodyState = RequireState(
            bodyLoad.Enemies,
            bodyLoad.Enemies.Slots[0]);
        RoomEnemyProjectileSlot body = bodyState.ProjectileSegments[0]
            ?? throw new InvalidDataException("Nuclear Waffle body link zero is absent.");
        bodyLoad.Samus.XPosition = body.XPosition;
        bodyLoad.Samus.YPosition = body.YPosition;
        bodyLoad.Samus.InvincibilityTimer = 0;
        health = bodyLoad.Samus.Health;
        bodyLoad.Enemies.StepEnemyProjectiles(assets.LevelData, bodyLoad.Samus);
        if (bodyLoad.Samus.Health != health - (4 * 64) || !bodyLoad.Samus.KnockbackActive ||
            !body.IsActive)
        {
            throw new InvalidDataException(
                $"Nuclear Waffle body contact failed: health={health}->" +
                $"{bodyLoad.Samus.Health}, knockback={bodyLoad.Samus.KnockbackActive}, " +
                $"body active={body.IsActive}.");
        }

        LoadedWaffles shotLoad = Load(bus, room, assets);
        RoomEnemySlot shotHead = shotLoad.Enemies.Slots[0];
        NuclearWaffleEnemyState shotState = RequireState(shotLoad.Enemies, shotHead);
        var projectiles = new SamusProjectileSystem();
        var sharedProjectiles = new SamusBombProjectileSystem();
        ArmBeam(projectiles.Slots[0], shotHead.XPosition, shotHead.YPosition);
        StepGameplayFrame(shotLoad, room, assets, shotHead);
        int headHits = shotLoad.Enemies.ResolveOrdinaryProjectileHits(
            bus,
            projectiles,
            sharedProjectiles,
            shotLoad.Samus);
        // The bank-$A0 collision walker counts this overlap and performs its universal
        // projectile prelude before dispatching the head's literal RTL shot callback.
        // Consequently the beam survives with direction bit $10 set, its animation list
        // remains untouched, and the indestructible head takes no damage. Treating the
        // returned hit count as "damage dealt" hid that important native distinction.
        if (headHits != 1 || !projectiles.Slots[0].IsActive ||
            projectiles.Slots[0].Direction != 0x0012 ||
            projectiles.Slots[0].InstructionPointer != 0x9000 ||
            shotHead.Health != 40)
        {
            throw new InvalidDataException(
                $"Nuclear Waffle no-op head shot AI diverged: hits={headHits}, " +
                $"beam active={projectiles.Slots[0].IsActive}, type=" +
                $"${projectiles.Slots[0].Type:X4}, direction=" +
                $"${projectiles.Slots[0].Direction:X4}, list=" +
                $"${projectiles.Slots[0].InstructionPointer:X4}, health={shotHead.Health}.");
        }

        RoomEnemyProjectileSlot shotBody = shotState.ProjectileSegments[0]
            ?? throw new InvalidDataException("Nuclear Waffle shot body link is absent.");
        ArmBeam(projectiles.Slots[0], shotBody.XPosition, shotBody.YPosition);
        int bodyHits = shotLoad.Enemies.ResolveEnemyProjectileSamusProjectileHits(
            bus,
            projectiles,
            sharedProjectiles);
        if (bodyHits != 1 || !shotBody.IsActive ||
            shotLoad.Enemies.LastEnemyProjectileDudSoundEffect != 0x003d)
        {
            throw new InvalidDataException(
                $"Nuclear Waffle body beam block failed: hits={bodyHits}, " +
                $"body active={shotBody.IsActive}, sound=" +
                $"{shotLoad.Enemies.LastEnemyProjectileDudSoundEffect?.ToString("X4") ?? "none"}.");
        }

        int powerBombHits = shotLoad.Enemies.ResolveOrdinaryPowerBombHits(
            bus,
            shotHead.XPosition,
            shotHead.YPosition,
            explosionRadius: 64);
        if (powerBombHits != 0 || shotHead.Health != 40)
            throw new InvalidDataException("Nuclear Waffle accepted indestructible power-bomb damage.");

        LoadedWaffles grappleLoad = Load(bus, room, assets);
        RoomEnemySlot grappleHead = grappleLoad.Enemies.Slots[0];
        StepGameplayFrame(grappleLoad, room, assets, grappleHead);
        GrappleEnemyCollision grapple = grappleLoad.Enemies.ResolveGrappleEndpoint(
            grappleHead.XPosition,
            grappleHead.YPosition);
        if (!grapple.Collided || grapple.Reaction != GrappleEnemyReaction.None ||
            grapple.EnemyNativeIndex != grappleHead.NativeIndex)
        {
            throw new InvalidDataException(
                $"Nuclear Waffle grapple failed: collided={grapple.Collided}, " +
                $"reaction={grapple.Reaction}, index=${grapple.EnemyNativeIndex:X4}.");
        }
    }

    private static void CaptureAnimationMaps(
        LoadedWaffles loaded,
        NuclearWaffleEnemyState state,
        HashSet<ushort> headMaps,
        HashSet<ushort> projectileMaps,
        HashSet<ushort> spriteMaps)
    {
        headMaps.Add(loaded.Enemies.Slots[0].SpritemapPointer);
        foreach (RoomEnemyProjectileSlot? segment in state.ProjectileSegments)
        {
            if (segment is not null)
                projectileMaps.Add(segment.SpritemapPointer);
        }
        foreach (RoomSpriteObjectSlot? segment in state.SpriteSegments)
        {
            if (segment is not null)
                spriteMaps.Add(segment.SpritemapPointer);
        }
    }

    private static LoadedWaffles Load(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusPoseIds.FacingRightNormalPose,
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
        return new LoadedWaffles(enemies, samus);
    }

    private static void StepGameplayFrame(
        LoadedWaffles loaded,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        RoomEnemySlot actor)
    {
        (ushort cameraX, ushort cameraY) = CenterCamera(room, actor);
        loaded.Enemies.StepFrame(
            cameraX,
            cameraY,
            timeIsFrozen: false,
            loaded.Samus,
            level: assets.LevelData);
        loaded.Enemies.StepEnemyProjectiles(
            assets.LevelData,
            loaded.Samus,
            cameraX: cameraX,
            cameraY: cameraY);
    }

    private static (ushort X, ushort Y) CenterCamera(
        CartridgeRoomHeader room,
        RoomEnemySlot actor)
    {
        int maximumX = Math.Max(0, room.WidthInScreens * 256 - 256);
        int maximumY = Math.Max(0, room.HeightInScreens * 256 - 224);
        return (
            unchecked((ushort)Math.Clamp(actor.XPosition - 128, 0, maximumX)),
            unchecked((ushort)Math.Clamp(actor.YPosition - 112, 0, maximumY)));
    }

    private static void ArmBeam(
        SamusProjectileSlot projectile,
        ushort x,
        ushort y)
    {
        projectile.ClearFields();
        projectile.Type = 0;
        projectile.Damage = 20;
        projectile.Direction = 2;
        projectile.XPosition = x;
        projectile.YPosition = y;
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    private static NuclearWaffleEnemyState RequireState(
        RoomEnemySystem enemies,
        RoomEnemySlot actor) =>
        enemies.NuclearWaffleStates[actor.SlotIndex] ?? throw new InvalidDataException(
            $"Nuclear Waffle slot {actor.SlotIndex} has no typed state.");

    private static int ReadEightBitSineProduct(
        ISnesAddressSpace bus,
        ushort angle,
        ushort radius)
    {
        int byteAngle = angle & 0xff;
        int sample = bus.ReadByte(0xa0b143 + (byteAngle & 0x7f));
        int magnitude = sample * (radius & 0xff) >> 8;
        return byteAngle < 0x80 ? magnitude : -magnitude;
    }

    private static ushort ReadNuclearWaffleAuditWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private static int ReadEightBitCosineProduct(
        ISnesAddressSpace bus,
        ushort angle,
        ushort radius) =>
        ReadEightBitSineProduct(bus, unchecked((ushort)(angle + 0x40)), radius);

    private readonly record struct LoadedWaffles(
        RoomEnemySystem Enemies,
        SamusState Samus);

    private readonly record struct SweepResult(
        int WaitFrames,
        int SweepFrames,
        int ObjPieces);
}
