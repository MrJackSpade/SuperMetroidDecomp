using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>
/// ROM-backed end-to-end regression for Mt. Everest's three two-slot Powamps. The audit
/// keeps the retail population intact, so loading also proves that Powamp coexists with the
/// room's six shared-crawler Scisers rather than relying on a synthetic one-enemy fixture.
/// </summary>
internal static partial class PowampAudit
{
    private const ushort MountEverestRoom = 0xd0b9;
    private const ushort PowampDefinition = 0xe8bf;
    private const ushort SciserDefinition = 0xd77f;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, MountEverestRoom);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        LoadedPowamps loaded = LoadEnemies(bus, room, assets);
        VerifyPopulation(room, loaded.Enemies);

        RoomEnemySlot balloon = loaded.Enemies.Slots[0];
        RoomEnemySlot body = loaded.Enemies.Slots[1];
        PowampEnemyState bodyState = RequireState(loaded.Enemies, body);
        VerifyNaturalCycle(loaded.Enemies, room, assets, loaded.Samus, balloon, body, bodyState);
        int objPieces = VerifyDrawing(loaded.Enemies, room, balloon);
        VerifyGrapple(bus, room, assets);
        VerifyLiveFiringPipeline(bus, room, assets);
        VerifyContact(bus, room, assets);
        VerifyShotDeathAndSpikes(bus, room, assets);
        VerifyNormalBombDeathAndSpikes(bus, room, assets);
        VerifySpikeInteractions(bus, room, assets);
        VerifyPowerBombPairing(bus, room, assets);

        Console.WriteLine(
            "Powamp audit passed: unchanged Mt. Everest loaded three paired Powamps and " +
            $"six Scisers; inflation/rise/wiggle/deflation/sink, {objPieces} OBJ pieces, " +
            "moving grapple attachment, touch damage, paired shot/normal-bomb/power-bomb " +
            "damage, " +
            "32-frame death, and all eight accelerating spike trajectories, animation, " +
            "terrain disposal, shot pass-through, and exact contact damage were verified.");
        return 0;
    }

    private static void VerifyPopulation(CartridgeRoomHeader room, RoomEnemySystem enemies)
    {
        RoomEnemySlot[] population = enemies.Slots.Take(enemies.EnemyCount).ToArray();
        ushort[] expectedX = [0x0200, 0x0200, 0x0308, 0x0308, 0x0400, 0x0400];
        ushort[] expectedY = [0x0180, 0x0180, 0x0200, 0x0200, 0x0230, 0x0230];
        ushort[] expectedTravel = [0x0070, 0x0120, 0x0030];
        if (room.State.Pointer != 0xd0c6 || enemies.EnemyCount != 12 ||
            population.Take(6).Any(slot => slot.EnemyDefinitionPointer != PowampDefinition) ||
            population.Skip(6).Any(slot => slot.EnemyDefinitionPointer != SciserDefinition))
        {
            throw new InvalidDataException(
                $"Mt. Everest population failed: state=${room.State.Pointer:X4}, " +
                $"population=${room.State.EnemyPopulationPointer:X4}, set=" +
                $"${room.State.EnemyTilesetPointer:X4}, count={enemies.EnemyCount}, " +
                $"Powamp={population.Count(x => x.EnemyDefinitionPointer == PowampDefinition)}, " +
                $"Sciser={population.Count(x => x.EnemyDefinitionPointer == SciserDefinition)}.");
        }

        for (int index = 0; index < 6; index++)
        {
            RoomEnemySlot slot = population[index];
            PowampEnemyState state = RequireState(enemies, slot);
            bool balloon = (index & 1) == 0;
            if (slot.XPosition != expectedX[index] || slot.YPosition != expectedY[index] ||
                slot.Health != 10 || slot.Definition.Damage != 100 ||
                slot.XRadius != 8 || slot.YRadius != 16 || slot.Layer != 5 ||
                slot.Definition.Bank != 0xa8 || slot.Definition.PartCount != 2 ||
                slot.Definition.InitializationAiPointer != 0xc1c9 ||
                slot.Definition.MainAiPointer != 0xc21c ||
                slot.Definition.GrappleAiPointer != 0x8014 ||
                slot.Definition.PowerBombReactionPointer != 0xc63f ||
                slot.Definition.TouchAiPointer != 0xc5be ||
                slot.Definition.ShotAiPointer != 0xc5ef || state.IsBalloon != balloon ||
                state.Function != (balloon
                    ? PowampEnemyFunction.BalloonNoOp
                    : PowampEnemyFunction.DeflatedResting))
            {
                throw new InvalidDataException(
                    $"Powamp slot {index} initialization failed: position=" +
                    $"({slot.XPosition:X4},{slot.YPosition:X4}), params=" +
                    $"${slot.Parameter1:X4}/${slot.Parameter2:X4}, properties=" +
                    $"${slot.Properties:X4}, health/damage={slot.Health}/" +
                    $"{slot.Definition.Damage}, radii={slot.XRadius}/{slot.YRadius}, " +
                    $"layer={slot.Layer}, function={state.Function}.");
            }

            if (balloon && (state.BalloonSpawnX != expectedX[index] ||
                state.BalloonSpawnY != expectedY[index] ||
                state.BalloonGrappleTravelDistance != expectedTravel[index / 2]))
            {
                throw new InvalidDataException(
                    $"Powamp balloon {index / 2} retained incorrect spawn/travel words: " +
                    $"({state.BalloonSpawnX:X4},{state.BalloonSpawnY:X4})/" +
                    $"${state.BalloonGrappleTravelDistance:X4}.");
            }
        }
    }

    private static void VerifyNaturalCycle(
        RoomEnemySystem enemies,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        SamusState samus,
        RoomEnemySlot balloon,
        RoomEnemySlot body,
        PowampEnemyState state)
    {
        var functions = new HashSet<PowampEnemyFunction>();
        var bodyMaps = new HashSet<ushort>();
        var balloonMaps = new HashSet<ushort>();
        ushort spawnY = body.YPosition;
        ushort minimumY = spawnY;
        bool returnedToSpawn = false;
        for (int frame = 0; frame < 512; frame++)
        {
            StepCentered(enemies, room, assets, samus, body);
            functions.Add(state.Function);
            bodyMaps.Add(body.SpritemapPointer);
            balloonMaps.Add(balloon.SpritemapPointer);
            minimumY = Math.Min(minimumY, body.YPosition);
            returnedToSpawn |= frame > 100 && body.YPosition == spawnY &&
                state.Function == PowampEnemyFunction.DeflatedResting;
        }

        PowampEnemyFunction[] expectedFunctions =
        [
            PowampEnemyFunction.DeflatedResting,
            PowampEnemyFunction.Inflating,
            PowampEnemyFunction.InflatedRiseToTargetHeight,
            PowampEnemyFunction.InflatedFinishWiggle,
            PowampEnemyFunction.Deflating,
            PowampEnemyFunction.DeflatedSinking,
        ];
        if (expectedFunctions.Any(function => !functions.Contains(function)) ||
            minimumY > spawnY - 0x40 || !returnedToSpawn ||
            bodyMaps.Count != 3 || balloonMaps.Count != 3)
        {
            throw new InvalidDataException(
                $"Powamp natural cycle failed: Y=${spawnY:X4}->${minimumY:X4}, " +
                $"returned={returnedToSpawn}, body/balloon maps=" +
                $"{bodyMaps.Count}/{balloonMaps.Count}, functions=" +
                $"{string.Join(',', functions)}.");
        }
    }

    private static int VerifyDrawing(
        RoomEnemySystem enemies,
        CartridgeRoomHeader room,
        RoomEnemySlot actor)
    {
        var oam = new OamBuffer();
        oam.BeginFrame();
        (ushort cameraX, ushort cameraY) = CenterCamera(room, actor);
        enemies.DrawLayers(oam, cameraX, cameraY, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException("Mt. Everest Powamps emitted no ROM-authored OBJ.");
        return oam.LastFinalizedSpriteCount;
    }

    private static void VerifyGrapple(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedPowamps loaded = LoadEnemies(bus, room, assets);
        RoomEnemySlot body = loaded.Enemies.Slots[1];
        PowampEnemyState state = RequireState(loaded.Enemies, body);

        // Let the retail timers inflate the body. The audit never installs an AI function;
        // the only stimulus is repeatedly sampling its live center as a held beam would.
        for (int frame = 0; frame < 72; frame++)
            StepCentered(loaded.Enemies, room, assets, loaded.Samus, body);
        ushort beforeY = body.YPosition;
        GrappleEnemyCollision collision = loaded.Enemies.ResolveGrappleEndpoint(
            body.XPosition,
            body.YPosition);
        if (!collision.Collided ||
            collision.Reaction != GrappleEnemyReaction.AttachWithoutInvincibility ||
            collision.EnemyNativeIndex != body.NativeIndex || (body.AiHandlerBits & 1) == 0)
        {
            throw new InvalidDataException(
                $"Powamp grapple resolver failed: collided={collision.Collided}, " +
                $"reaction={collision.Reaction}, enemy=${collision.EnemyNativeIndex:X4}, " +
                $"AI=${body.AiHandlerBits:X4}.");
        }

        StepCentered(loaded.Enemies, room, assets, loaded.Samus, body);
        if (state.Function != PowampEnemyFunction.GrappledRiseToTargetHeight ||
            body.YPosition >= beforeY || body.AiHandlerBits != 0)
        {
            throw new InvalidDataException(
                $"Powamp common grapple AI failed: function={state.Function}, " +
                $"Y=${beforeY:X4}->${body.YPosition:X4}, AI=${body.AiHandlerBits:X4}.");
        }
    }

    private static void VerifyContact(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedPowamps loaded = LoadEnemies(bus, room, assets);
        RoomEnemySlot body = loaded.Enemies.Slots[1];
        StepCentered(loaded.Enemies, room, assets, loaded.Samus, body);
        loaded.Samus.XPosition = body.XPosition;
        loaded.Samus.YPosition = body.YPosition;
        ushort health = loaded.Samus.Health;
        if (!loaded.Enemies.ResolveOrdinarySamusContact(loaded.Samus, 0) ||
            loaded.Samus.Health != health - 100 || loaded.Samus.InvincibilityTimer != 0x60 ||
            !loaded.Samus.KnockbackActive)
        {
            throw new InvalidDataException(
                $"Powamp touch failed: health={health}->{loaded.Samus.Health}, " +
                $"invincibility={loaded.Samus.InvincibilityTimer}, " +
                $"knockback={loaded.Samus.KnockbackActive}.");
        }
    }

    private static void VerifyLiveFiringPipeline(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedPowamps loaded = LoadEnemies(bus, room, assets);
        RoomEnemySlot body = loaded.Enemies.Slots[1];
        for (int frame = 0; frame < 72; frame++)
            StepCentered(loaded.Enemies, room, assets, loaded.Samus, body);

        // Ask BeginFiring for the retail pose's authored hand offset, then place a fresh
        // identical Samus so her initial endpoint is exactly 80 pixels left of the live
        // body. The beam must reach the actor through ordinary extension frames—neither the
        // endpoint nor the resulting connected phase is injected by the audit.
        SamusState offsetProbe = CreateSamus(bus, 0, 0);
        SamusGrappleMovement.BeginFiring(bus, offsetProbe);
        loaded.Samus.XPosition = unchecked((ushort)(
            body.XPosition - 80 - offsetProbe.Grapple.OriginXOffset));
        loaded.Samus.YPosition = unchecked((ushort)(
            body.YPosition - offsetProbe.Grapple.OriginYOffset));
        SamusGrappleMovement.BeginFiring(bus, loaded.Samus);

        GrappleMovementResult result = default;
        bool connected = false;
        var plms = new RoomPlmSystem();
        for (int frame = 0; frame < 10 && !connected; frame++)
        {
            result = SamusGrappleMovement.StepFiring(
                bus,
                assets.LevelData,
                loaded.Samus,
                (ushort)SnesButton.X,
                plms,
                loaded.Enemies.ResolveGrappleEndpoint);
            connected = result.Connected;
            if (result.CancelQueued)
                break;
        }
        if (!connected || !loaded.Samus.Grapple.ValidateAnchorEnemy ||
            loaded.Samus.Grapple.ValidateAnchorBlock)
        {
            throw new InvalidDataException(
                $"Powamp live grapple firing failed: phase={loaded.Samus.Grapple.Phase}, " +
                $"connected={connected}, cancel={result.CancelQueued}, validators=" +
                $"{loaded.Samus.Grapple.ValidateAnchorBlock}/" +
                $"{loaded.Samus.Grapple.ValidateAnchorEnemy}, endpoint=" +
                $"({loaded.Samus.Grapple.AnchorX:X4},{loaded.Samus.Grapple.AnchorY:X4}), " +
                $"body=({body.XPosition:X4},{body.YPosition:X4}).");
        }

        // EnemyMain consumes the handler bit first on the next gameplay frame and moves the
        // actor. The following connected-beam call must reacquire the new center rather than
        // validate the old coordinate as a block or retain a detached static rope.
        StepCentered(loaded.Enemies, room, assets, loaded.Samus, body);
        result = SamusGrappleMovement.Step(
            bus,
            assets.LevelData,
            loaded.Samus,
            (ushort)SnesButton.X,
            newlyPressedInput: 0,
            nmiFrameCounter: 0,
            loaded.Enemies.ResolveGrappleEndpoint);
        if (result.AnchorDisconnected || loaded.Samus.Grapple.AnchorX != body.XPosition ||
            loaded.Samus.Grapple.AnchorY != body.YPosition)
        {
            throw new InvalidDataException(
                $"Powamp moving grapple anchor failed: phase={result.Phase}, " +
                $"disconnected={result.AnchorDisconnected}, anchor=" +
                $"({loaded.Samus.Grapple.AnchorX:X4},{loaded.Samus.Grapple.AnchorY:X4}), " +
                $"body=({body.XPosition:X4},{body.YPosition:X4}).");
        }
    }

    private static void VerifyShotDeathAndSpikes(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedPowamps loaded = LoadEnemies(bus, room, assets);
        RoomEnemySlot balloon = loaded.Enemies.Slots[0];
        RoomEnemySlot body = loaded.Enemies.Slots[1];
        PowampEnemyState state = RequireState(loaded.Enemies, body);
        StepCentered(loaded.Enemies, room, assets, loaded.Samus, body);
        var projectiles = new SamusProjectileSystem();
        var sharedProjectiles = new SamusBombProjectileSystem();
        ArmProjectile(projectiles.Slots[0], body, projectileType: 0x0200, damage: 1000);
        if (loaded.Enemies.ResolveOrdinaryProjectileHits(
                bus,
                projectiles,
                sharedProjectiles,
                loaded.Samus) != 1 || body.Health != 0 ||
            body.Properties.HasAny(EnemyProperties.Deleted) ||
            state.Function != PowampEnemyFunction.FatalDamage || body.Parameter2 != 1)
        {
            throw new InvalidDataException(
                $"Powamp lethal shot handoff failed: health={body.Health}, " +
                $"deleted={body.Properties.HasAny(EnemyProperties.Deleted)}, " +
                $"function={state.Function}, guard={body.Parameter2}.");
        }

        for (int frame = 0; frame < 33; frame++)
            StepCentered(loaded.Enemies, room, assets, loaded.Samus, body);
        RoomEnemyProjectileSlot[] spikes = loaded.Enemies.EnemyProjectiles
            .Where(projectile => projectile.Kind == RoomEnemyProjectileKind.PowampSpike)
            .ToArray();
        if (!body.Properties.HasAny(EnemyProperties.Deleted) ||
            !balloon.Properties.HasAny(EnemyProperties.Deleted) || spikes.Length != 8 ||
            spikes.Select(spike => spike.DirectionParameter).Order().Where((x, i) => x != i).Any() ||
            spikes.Any(spike => spike.Damage != 20 || spike.XRadius != 4 ||
                spike.YRadius != 4 || !spike.CanDamageSamus ||
                spike.PersistsOnSamusContact || spike.BlocksSamusProjectiles ||
                spike.CollisionOption != 0 || spike.InvincibilityFrames != 96 ||
                spike.PreInstruction != 0xd263 || spike.InstructionPointer != 0xd208 ||
                spike.InstructionTimer != 1 || spike.SpritemapPointer != 0x8000))
        {
            throw new InvalidDataException(
                $"Powamp delayed death/spawn failed: body/balloon deleted=" +
                $"{body.Properties.HasAny(EnemyProperties.Deleted)}/" +
                $"{balloon.Properties.HasAny(EnemyProperties.Deleted)}, spikes={spikes.Length}.");
        }

        // Keep Samus away so all eight projectiles survive their first pre-instruction and
        // expose the exact signed 8.8 acceleration vectors selected by directions zero..7.
        loaded.Samus.XPosition = 0;
        loaded.Samus.YPosition = 0;
        loaded.Enemies.StepEnemyProjectiles(assets.LevelData, loaded.Samus);
        short[] expectedX = [0, 0x20, 0x20, 0x20, 0, -0x20, -0x20, -0x20];
        short[] expectedY = [-0x20, -0x20, 0, 0x20, 0x20, 0x20, 0, -0x20];
        foreach (RoomEnemyProjectileSlot spike in spikes)
        {
            int direction = spike.DirectionParameter;
            if (!spike.IsActive || unchecked((short)spike.XVelocity) != expectedX[direction] ||
                unchecked((short)spike.YVelocity) != expectedY[direction] ||
                spike.SpritemapPointer == 0)
            {
                throw new InvalidDataException(
                    $"Powamp spike {direction} first frame failed: active={spike.IsActive}, " +
                    $"velocity={unchecked((short)spike.XVelocity)}/" +
                    $"{unchecked((short)spike.YVelocity)}, map=${spike.SpritemapPointer:X4}.");
            }
        }

        // Follow every physical slot until the accelerating burst reaches room terrain.
        // Velocity is checked on every surviving frame, so an implementation that merely
        // applies the first table entry—or moves at a constant speed—cannot pass. Retaining
        // the direction separately is necessary because Clear() correctly zeros the slot.
        RoomEnemyProjectileSlot[] byDirection = spikes
            .OrderBy(spike => spike.DirectionParameter)
            .ToArray();
        Dictionary<int, HashSet<(ushort X, ushort Y)>> positions = Enumerable.Range(0, 8)
            .ToDictionary(direction => direction, direction =>
                new HashSet<(ushort X, ushort Y)>
                {
                    (byDirection[direction].XPosition, byDirection[direction].YPosition),
                });
        Dictionary<int, HashSet<ushort>> maps = Enumerable.Range(0, 8)
            .ToDictionary(direction => direction, direction =>
                new HashSet<ushort> { byDirection[direction].SpritemapPointer });
        for (int frame = 1; frame < 1024 && byDirection.Any(spike => spike.IsActive); frame++)
        {
            (bool Active, short X, short Y)[] before = byDirection
                .Select(spike => (spike.IsActive,
                    unchecked((short)spike.XVelocity),
                    unchecked((short)spike.YVelocity)))
                .ToArray();
            loaded.Enemies.StepEnemyProjectiles(
                assets.LevelData,
                samus: null,
                nmiFrameCounter8: unchecked((byte)frame));

            for (int direction = 0; direction < byDirection.Length; direction++)
            {
                RoomEnemyProjectileSlot spike = byDirection[direction];
                if (!spike.IsActive)
                    continue;
                short nextX = unchecked((short)(before[direction].X + expectedX[direction]));
                short nextY = unchecked((short)(before[direction].Y + expectedY[direction]));
                if (!before[direction].Active ||
                    unchecked((short)spike.XVelocity) != nextX ||
                    unchecked((short)spike.YVelocity) != nextY)
                {
                    throw new InvalidDataException(
                        $"Powamp spike {direction} acceleration diverged on frame {frame}: " +
                        $"velocity={unchecked((short)spike.XVelocity)}/" +
                        $"{unchecked((short)spike.YVelocity)}, expected {nextX}/{nextY}.");
                }
                positions[direction].Add((spike.XPosition, spike.YPosition));
                if (spike.SpritemapPointer is not 0 and not 0x8000)
                    maps[direction].Add(spike.SpritemapPointer);
            }
        }

        for (int direction = 0; direction < byDirection.Length; direction++)
        {
            if (byDirection[direction].IsActive || positions[direction].Count < 2 ||
                maps[direction].Count != 3)
            {
                throw new InvalidDataException(
                    $"Powamp spike {direction} terminal lifecycle failed: live=" +
                    $"{byDirection[direction].IsActive}, positions=" +
                    $"{positions[direction].Count}, maps={maps[direction].Count}.");
            }
        }
    }

    private static void VerifyPowerBombPairing(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedPowamps loaded = LoadEnemies(bus, room, assets);
        RoomEnemySlot balloon = loaded.Enemies.Slots[0];
        RoomEnemySlot body = loaded.Enemies.Slots[1];
        int reactions = loaded.Enemies.ResolveOrdinaryPowerBombHits(
            bus,
            body.XPosition,
            body.YPosition,
            explosionRadius: 16);
        if (reactions != 1 || !body.Properties.HasAny(EnemyProperties.Deleted) ||
            !balloon.Properties.HasAny(EnemyProperties.Deleted))
        {
            throw new InvalidDataException(
                $"Powamp paired power-bomb death failed: reactions={reactions}, " +
                $"body/balloon deleted={body.Properties.HasAny(EnemyProperties.Deleted)}/" +
                $"{balloon.Properties.HasAny(EnemyProperties.Deleted)}.");
        }
    }

    private static void VerifyNormalBombDeathAndSpikes(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedPowamps loaded = LoadEnemies(bus, room, assets);
        RoomEnemySlot balloon = loaded.Enemies.Slots[0];
        RoomEnemySlot body = loaded.Enemies.Slots[1];
        PowampEnemyState state = RequireState(loaded.Enemies, body);
        StepCentered(loaded.Enemies, room, assets, loaded.Samus, body);

        var bombs = new SamusBombProjectileSystem();
        ArmNormalBomb(bombs.Slots[0], body);
        int hits = loaded.Enemies.ResolveOrdinaryBombHits(
            bombs,
            new SamusProjectileSystem(),
            loaded.Samus);
        if (hits != 1 || (bombs.Slots[0].Direction & 0x0010) == 0 ||
            body.Health != 0 || body.Properties.HasAny(EnemyProperties.Deleted) ||
            state.Function != PowampEnemyFunction.FatalDamage || body.Parameter2 != 1 ||
            balloon.FlashTimer != body.FlashTimer ||
            (balloon.AiHandlerBits & 0x0002) == 0)
        {
            throw new InvalidDataException(
                $"Powamp normal-bomb handoff failed: hits={hits}, direction=" +
                $"${bombs.Slots[0].Direction:X4}, health={body.Health}, deleted=" +
                $"{body.Properties.HasAny(EnemyProperties.Deleted)}, function={state.Function}, " +
                $"guard={body.Parameter2}, flash={body.FlashTimer}/{balloon.FlashTimer}, " +
                $"balloon AI=${balloon.AiHandlerBits:X4}.");
        }

        for (int frame = 0; frame < 33; frame++)
            StepCentered(loaded.Enemies, room, assets, loaded.Samus, body);
        int spikes = loaded.Enemies.EnemyProjectiles.Count(projectile =>
            projectile.Kind == RoomEnemyProjectileKind.PowampSpike);
        if (!body.Properties.HasAny(EnemyProperties.Deleted) ||
            !balloon.Properties.HasAny(EnemyProperties.Deleted) || spikes != 8)
        {
            throw new InvalidDataException(
                $"Powamp normal-bomb staged death failed: body/balloon deleted=" +
                $"{body.Properties.HasAny(EnemyProperties.Deleted)}/" +
                $"{balloon.Properties.HasAny(EnemyProperties.Deleted)}, spikes={spikes}.");
        }
    }

    private static LoadedPowamps LoadEnemies(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        SamusState samus = CreateSamus(bus, 0x0100, 0x0100);
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
        return new LoadedPowamps(enemies, samus);
    }

    private static void ArmNormalBomb(
        SamusBombProjectileSlot bomb,
        RoomEnemySlot target)
    {
        bomb.ClearFields();
        // Powamp's `$82` bomb entry has multiplier two. Twenty raw points therefore deal
        // twenty health after common half-damage—more than enough for its ten-HP body—and
        // must still hand death to `$A8:C5EF` rather than deleting the slot immediately.
        bomb.Type = SamusBombProjectileSystem.NormalBombType;
        bomb.Damage = 20;
        bomb.Direction = (ushort)SamusProjectileDirection.Right;
        bomb.XPosition = target.XPosition;
        bomb.YPosition = target.YPosition;
        bomb.XRadius = 16;
        bomb.YRadius = 16;
        bomb.BombTimer = 0;
        bomb.InstructionPointer = 0xa06b;
        bomb.InstructionTimer = 1;
    }

    private static void StepCentered(
        RoomEnemySystem enemies,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        SamusState samus,
        RoomEnemySlot target)
    {
        (ushort cameraX, ushort cameraY) = CenterCamera(room, target);
        enemies.StepFrame(cameraX, cameraY, false, samus, level: assets.LevelData);
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

    private static SamusState CreateSamus(ISnesAddressSpace bus, ushort x, ushort y)
    {
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusPoseIds.FacingRightNormalPose,
            XPosition = x,
            YPosition = y,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        return samus;
    }

    private static void ArmProjectile(
        SamusProjectileSlot projectile,
        RoomEnemySlot target,
        ushort projectileType,
        ushort damage)
    {
        projectile.ClearFields();
        projectile.Type = projectileType;
        projectile.Damage = damage;
        projectile.Direction = 2;
        projectile.XPosition = target.XPosition;
        projectile.YPosition = target.YPosition;
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    private static PowampEnemyState RequireState(RoomEnemySystem enemies, RoomEnemySlot slot) =>
        enemies.PowampStates[slot.SlotIndex] ?? throw new InvalidDataException(
            $"Powamp slot {slot.SlotIndex} has no typed state.");

    private readonly record struct LoadedPowamps(RoomEnemySystem Enemies, SamusState Samus);
}
