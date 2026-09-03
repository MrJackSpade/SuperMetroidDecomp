using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Untouched-ROM end-to-end audit for Volcano's six Funes and four Polyps. This room is the
/// useful acceptance boundary because it forces both actor families, both graphics-set
/// entries, the shared RNG seed, actor bytecode, and two unrelated bank-$86 projectile
/// engines to coexist exactly as shipped.
/// </summary>
internal static class VolcanoEnemyAudit
{
    private const ushort RoomPointer = 0xae32;
    private const ushort StatePointer = 0xae3f;
    private const ushort PopulationPointer = 0xbb34;
    private const ushort TilesetPointer = 0x8b29;
    private const ushort FuneDefinition = 0xe6ff;
    private const ushort PolypDefinition = 0xd1ff;
    private const ushort EmptySpritemap = 0x804d;

    private static readonly ushort[] FuneLeftMaps =
        [0x93f9, 0x9423, 0x944d, 0x9477, 0x94a1];

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, RoomPointer);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        VerifyHeaders(bus);
        VerifyProjectileDefinitions(bus);

        (RoomEnemySystem enemies, Bank80SystemState random) =
            LoadRoom(bus, room, assets, out SamusState samus);
        RoomEnemySlot[] population = enemies.Slots.Take(enemies.EnemyCount).ToArray();
        RoomEnemySlot[] funes = population.Where(slot =>
            slot.EnemyDefinitionPointer == FuneDefinition).ToArray();
        RoomEnemySlot[] polyps = population.Where(slot =>
            slot.EnemyDefinitionPointer == PolypDefinition).ToArray();

        if (room.State.Pointer != StatePointer ||
            room.State.EnemyPopulationPointer != PopulationPointer ||
            room.State.EnemyTilesetPointer != TilesetPointer ||
            enemies.EnemyCount != 10 || enemies.DeathQuota != 10 ||
            funes.Length != 6 || polyps.Length != 4 ||
            funes.Any(slot => enemies.FuneNamiheStates[slot.SlotIndex] is null) ||
            polyps.Any(slot => enemies.PolypStates[slot.SlotIndex] is null) ||
            random.RandomNumber != 0x0011)
        {
            throw new InvalidDataException(
                $"Volcano load mismatch: state=${room.State.Pointer:X4}, population=" +
                $"${room.State.EnemyPopulationPointer:X4}, set=" +
                $"${room.State.EnemyTilesetPointer:X4}, count/quota=" +
                $"{enemies.EnemyCount}/{enemies.DeathQuota}, Fune/Polyp=" +
                $"{funes.Length}/{polyps.Length}, random=${random.RandomNumber:X4}.");
        }

        VerifyPopulationInitialization(enemies, funes, polyps);
        VerifyNaturalFuneCycle(bus, room, assets);
        VerifyNaturalPolypCycle(bus, room, assets);
        VerifyAttackDamage(bus, room, assets);
        VerifyBodyAndShotReactions(bus, room, assets);

        Console.WriteLine(
            "Volcano enemy audit passed: untouched room data loaded six Funes and four " +
            "Polyps; Fune cooldown/mouth bytecode, all five actor maps, spit sound, " +
            "leftward 8.8 fireball flight, Polyp's three-sample RNG launch, quadratic " +
            "rise/apex/fall, exact cooldown underflow, projectile OBJ, 60/16 attack " +
            "damage, body contact, Ice freeze, and indestructible vent behavior were verified.");
        return 0;
    }

    private static void VerifyPopulationInitialization(
        RoomEnemySystem enemies,
        RoomEnemySlot[] funes,
        RoomEnemySlot[] polyps)
    {
        ushort[] expectedFuneX = [0x01d0, 0x02e0, 0x0220, 0x02e0, 0x0220, 0x02e0];
        ushort[] expectedFuneY = [0x0280, 0x0210, 0x01c8, 0x0178, 0x0128, 0x00d8];
        for (int index = 0; index < funes.Length; index++)
        {
            RoomEnemySlot actor = funes[index];
            FuneNamiheEnemyState state = FuneState(enemies, actor);
            bool facingRight = index is 2 or 4;
            ushort expectedParameter2 = index == 0 ? (ushort)0x8005 : (ushort)0x0007;
            if (actor.XPosition != expectedFuneX[index] ||
                actor.YPosition != expectedFuneY[index] ||
                actor.Parameter1 != (facingRight ? 0x8010 : 0x8000) ||
                actor.Parameter2 != expectedParameter2 ||
                actor.Properties != 0xa000 || actor.Health != 20 ||
                actor.CurrentInstruction != (facingRight ? 0x93c9 : 0x9399) ||
                actor.SpritemapPointer != EmptySpritemap ||
                state.InstructionListPointerTableCursor !=
                    (facingRight ? 0x96d9 : 0x96d7) ||
                state.Function != FuneNamiheEnemyFunction.FuneWaitForCooldown ||
                state.VariantIndex != 0 || state.CooldownTimer != 0 ||
                state.CooldownTime != 0x0080 ||
                state.YProximity != (index == 0 ? 0x0080 : 0))
            {
                throw new InvalidDataException(
                    $"Volcano Fune {index} initialization mismatch: position=" +
                    $"(${actor.XPosition:X4},${actor.YPosition:X4}), params=" +
                    $"${actor.Parameter1:X4}/${actor.Parameter2:X4}, list=" +
                    $"$A8:{actor.CurrentInstruction:X4}, table=$A8:" +
                    $"{state.InstructionListPointerTableCursor:X4}, function={state.Function}, " +
                    $"cooldown={state.CooldownTimer}/{state.CooldownTime}, " +
                    $"Y proximity={state.YProximity}.");
            }
        }

        ushort[] expectedPolypX = [0x00f8, 0x0080, 0x0088, 0x0108];
        for (int index = 0; index < polyps.Length; index++)
        {
            RoomEnemySlot actor = polyps[index];
            PolypEnemyState state = PolypState(enemies, actor);
            if (actor.XPosition != expectedPolypX[index] || actor.YPosition != 0x02c8 ||
                actor.Parameter1 != 0 || actor.Parameter2 != 0 ||
                actor.Properties != 0x2500 || actor.Health != 1 ||
                actor.CurrentInstruction != 0xb51a ||
                actor.SpritemapPointer != EmptySpritemap ||
                state.Function != PolypEnemyFunction.WaitingForSamus ||
                state.CooldownTimer != 0)
            {
                throw new InvalidDataException(
                    $"Volcano Polyp {index} initialization mismatch: position=" +
                    $"(${actor.XPosition:X4},${actor.YPosition:X4}), properties=" +
                    $"${actor.Properties:X4}, list=$A2:{actor.CurrentInstruction:X4}, " +
                    $"function={state.Function}, cooldown={state.CooldownTimer}.");
            }
        }
    }

    private static void VerifyNaturalFuneCycle(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        (RoomEnemySystem enemies, _) = LoadRoom(bus, room, assets, out SamusState samus);
        RoomEnemySlot actor = enemies.Slots.Take(enemies.EnemyCount).First(slot =>
            slot.EnemyDefinitionPointer == FuneDefinition);
        FuneNamiheEnemyState state = FuneState(enemies, actor);
        Isolate(enemies, actor);
        (ushort cameraX, ushort cameraY) = CenterCamera(room, actor);

        var actorMaps = new HashSet<ushort>();
        var projectileMaps = new HashSet<ushort>();
        bool sawActivity = false;
        bool returnedToWaiting = false;
        bool sawSpitSound = false;
        bool checkedInitialProjectile = false;
        int projectileObjCount = 0;

        for (int frame = 0; frame < 384 && !returnedToWaiting; frame++)
        {
            enemies.StepFrame(cameraX, cameraY, false, samus, level: assets.LevelData);
            actorMaps.Add(actor.SpritemapPointer);
            sawActivity |= state.Function == FuneNamiheEnemyFunction.FuneActivityNoOp;
            sawSpitSound |= enemies.LastFuneNamiheSoundEffect == 0x001f;

            RoomEnemyProjectileSlot? shot = enemies.EnemyProjectiles.FirstOrDefault(
                projectile => projectile.Kind == RoomEnemyProjectileKind.FuneFireball);
            if (shot is not null && !checkedInitialProjectile)
            {
                if (shot.XPosition != actor.XPosition || shot.YPosition != actor.YPosition ||
                    shot.XSubposition != actor.XSubposition ||
                    shot.YSubposition != actor.YSubposition ||
                    shot.InstructionPointer != 0xde96 || shot.PreInstruction != 0xdf39 ||
                    shot.Variable0 != 0xdf40 || shot.YVelocity != 0xfe80 ||
                    shot.XVelocity != 0x0180 || shot.Damage != 60 ||
                    shot.XRadius != 4 || shot.YRadius != 8)
                {
                    throw new InvalidDataException(
                        $"Fune fireball initialization mismatch: position=" +
                        $"(${shot.XPosition:X4}.${shot.XSubposition:X4}," +
                        $"${shot.YPosition:X4}.${shot.YSubposition:X4}), list/pre/function=" +
                        $"${shot.InstructionPointer:X4}/${shot.PreInstruction:X4}/" +
                        $"${shot.Variable0:X4}, velocity=${shot.YVelocity:X4}/" +
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
            if (shot is not null && shot.IsActive)
                projectileMaps.Add(shot.SpritemapPointer);

            if (checkedInitialProjectile && projectileObjCount == 0 && shot?.IsActive == true)
            {
                var oam = new OamBuffer();
                oam.BeginFrame();
                enemies.DrawLayers(oam, cameraX, cameraY, 0, 7);
                enemies.DrawEnemyProjectiles(oam, cameraX, cameraY);
                oam.FinalizeFrame();
                projectileObjCount = oam.LastFinalizedSpriteCount;
            }

            returnedToWaiting = sawActivity &&
                state.Function == FuneNamiheEnemyFunction.FuneWaitForCooldown &&
                state.InstructionListPointerTableCursor == 0x96d7;
        }

        ushort[] expectedProjectileMaps = [0xaab9, 0xaac0, 0xaac7];
        if (!returnedToWaiting || !sawSpitSound || !checkedInitialProjectile ||
            !FuneLeftMaps.All(actorMaps.Contains) ||
            !expectedProjectileMaps.All(projectileMaps.Contains) || projectileObjCount < 9)
        {
            throw new InvalidDataException(
                $"Fune natural cycle mismatch: returned={returnedToWaiting}, " +
                $"sound={sawSpitSound}, projectile={checkedInitialProjectile}, " +
                $"actor maps={string.Join(',', actorMaps.Select(value => $"${value:X4}"))}, " +
                $"projectile maps=" +
                $"{string.Join(',', projectileMaps.Select(value => $"${value:X4}"))}, " +
                $"OBJ={projectileObjCount}.");
        }
    }

    private static void VerifyNaturalPolypCycle(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        (RoomEnemySystem enemies, Bank80SystemState random) =
            LoadRoom(bus, room, assets, out SamusState samus);
        RoomEnemySlot actor = enemies.Slots.Take(enemies.EnemyCount)
            .Where(slot => slot.EnemyDefinitionPointer == PolypDefinition)
            .Skip(1)
            .First();
        PolypEnemyState state = PolypState(enemies, actor);
        Isolate(enemies, actor);
        samus.XPosition = actor.XPosition;
        samus.YPosition = actor.YPosition;
        (ushort cameraX, ushort cameraY) = CenterCamera(room, actor);

        // Waiting only installs the shooter. The following actor frame owns all three RNG
        // advances and allocation, which is an externally visible one-frame tell.
        enemies.StepFrame(cameraX, cameraY, false, samus, level: assets.LevelData);
        if (state.Function != PolypEnemyFunction.ShootingRock ||
            enemies.ActiveEnemyProjectileCount != 0 || actor.SpritemapPointer != 0xb5fb)
        {
            throw new InvalidDataException("Polyp did not enter its one-frame shooting tell.");
        }
        enemies.StepFrame(cameraX, cameraY, false, samus, level: assets.LevelData);

        RoomEnemyProjectileSlot rock = enemies.EnemyProjectiles.Single(projectile =>
            projectile.Kind == RoomEnemyProjectileKind.PolypRock);
        if (random.RandomNumber != 0x295c ||
            state.Function != PolypEnemyFunction.Cooldown || state.CooldownTimer != 0x0040 ||
            rock.XPosition != actor.XPosition || rock.YPosition != actor.YPosition ||
            rock.XVelocity != 0x0090 || rock.YVelocity != 0x0023 ||
            rock.InstructionPointer != 0xbbd5 || rock.PreInstruction != 0xbc0f ||
            rock.Variable0 != 0xbc16 || rock.Damage != 16 ||
            rock.XRadius != 2 || rock.YRadius != 2)
        {
            throw new InvalidDataException(
                $"Polyp launch mismatch: random=${random.RandomNumber:X4}, " +
                $"function/cooldown={state.Function}/{state.CooldownTimer}, position=" +
                $"(${rock.XPosition:X4},${rock.YPosition:X4}), velocity=" +
                $"${rock.XVelocity:X4}/${rock.YVelocity:X4}, list/pre/function=" +
                $"${rock.InstructionPointer:X4}/${rock.PreInstruction:X4}/" +
                $"${rock.Variable0:X4}, damage/radii={rock.Damage}/" +
                $"{rock.XRadius}/{rock.YRadius}.");
        }

        ushort minimumY = rock.YPosition;
        bool sawApexPause = false;
        bool sawFalling = false;
        bool sawMap = false;
        int rockObjCount = 0;
        for (int frame = 0; frame < 384 && rock.IsActive; frame++)
        {
            ushort beforeX = rock.XPosition;
            ushort beforeXSub = rock.XSubposition;
            ushort beforeY = rock.YPosition;
            ushort beforeYSub = rock.YSubposition;
            bool willPauseAtApex = rock.Variable0 == 0xbc16 && rock.YVelocity == 1;

            enemies.StepEnemyProjectiles(
                assets.LevelData,
                samus: null,
                cameraX: cameraX,
                cameraY: cameraY);
            minimumY = Math.Min(minimumY, rock.YPosition);
            sawFalling |= rock.Variable0 == 0xbc8f;
            sawMap |= rock.SpritemapPointer == 0x9340;
            if (willPauseAtApex)
            {
                sawApexPause = rock.IsActive && rock.Variable0 == 0xbc8f &&
                    rock.YVelocity == 0 && rock.XPosition == beforeX &&
                    rock.XSubposition == beforeXSub && rock.YPosition == beforeY &&
                    rock.YSubposition == beforeYSub;
            }

            if (rock.IsActive && rockObjCount == 0 && rock.SpritemapPointer == 0x9340)
            {
                var oam = new OamBuffer();
                oam.BeginFrame();
                enemies.DrawEnemyProjectiles(oam, cameraX, cameraY);
                oam.FinalizeFrame();
                rockObjCount = oam.LastFinalizedSpriteCount;
            }
        }
        if (rock.IsActive || minimumY >= actor.YPosition || !sawApexPause ||
            !sawFalling || !sawMap || rockObjCount != 1)
        {
            throw new InvalidDataException(
                $"Polyp rock arc mismatch: live={rock.IsActive}, Ymin=${minimumY:X4}, " +
                $"apex={sawApexPause}, falling={sawFalling}, map={sawMap}, OBJ={rockObjCount}.");
        }

        // Cooldown decrements through zero and only rearms on the following underflow.
        for (int frame = 0; frame < 64; frame++)
            enemies.StepFrame(cameraX, cameraY, false, samus, level: assets.LevelData);
        if (state.Function != PolypEnemyFunction.Cooldown || state.CooldownTimer != 0)
            throw new InvalidDataException("Polyp cooldown did not retain its zero frame.");
        enemies.StepFrame(cameraX, cameraY, false, samus, level: assets.LevelData);
        if (state.Function != PolypEnemyFunction.WaitingForSamus ||
            state.CooldownTimer != 0xffff)
        {
            throw new InvalidDataException("Polyp cooldown did not rearm on signed underflow.");
        }
    }

    private static void VerifyAttackDamage(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        // First prove Fune's natural producer all the way into common enemy-projectile
        // contact. A fresh room avoids reusing the presentation-cycle projectile above.
        (RoomEnemySystem enemies, _) = LoadRoom(bus, room, assets, out SamusState producer);
        RoomEnemySlot fune = enemies.Slots.Take(enemies.EnemyCount).First(slot =>
            slot.EnemyDefinitionPointer == FuneDefinition);
        Isolate(enemies, fune);
        (ushort cameraX, ushort cameraY) = CenterCamera(room, fune);
        RoomEnemyProjectileSlot? fireball = null;
        for (int frame = 0; frame < 256 && fireball is null; frame++)
        {
            enemies.StepFrame(cameraX, cameraY, false, producer, level: assets.LevelData);
            fireball = enemies.EnemyProjectiles.FirstOrDefault(projectile =>
                projectile.Kind == RoomEnemyProjectileKind.FuneFireball);
        }
        if (fireball is null)
            throw new InvalidDataException("Natural Fune cycle produced no damage-test fireball.");
        SamusState target = CreateSamus(bus, fireball.XPosition, fireball.YPosition);
        enemies.StepEnemyProjectiles(
            assets.LevelData,
            target,
            cameraX: cameraX,
            cameraY: cameraY);
        if (target.Health != 939 || !target.KnockbackActive || fireball.IsActive)
        {
            throw new InvalidDataException(
                $"Fune fireball contact mismatch: health={target.Health}, " +
                $"knockback={target.KnockbackActive}, live={fireball.IsActive}.");
        }

        // Repeat through Polyp's natural proximity/tell/RNG sequence.
        (enemies, _) = LoadRoom(bus, room, assets, out producer);
        RoomEnemySlot polyp = enemies.Slots.Take(enemies.EnemyCount)
            .Where(slot => slot.EnemyDefinitionPointer == PolypDefinition)
            .Skip(1)
            .First();
        Isolate(enemies, polyp);
        producer.XPosition = polyp.XPosition;
        producer.YPosition = polyp.YPosition;
        (cameraX, cameraY) = CenterCamera(room, polyp);
        enemies.StepFrame(cameraX, cameraY, false, producer, level: assets.LevelData);
        enemies.StepFrame(cameraX, cameraY, false, producer, level: assets.LevelData);
        RoomEnemyProjectileSlot rock = enemies.EnemyProjectiles.Single(projectile =>
            projectile.Kind == RoomEnemyProjectileKind.PolypRock);
        target = CreateSamus(bus, rock.XPosition, rock.YPosition);
        enemies.StepEnemyProjectiles(
            assets.LevelData,
            target,
            cameraX: cameraX,
            cameraY: cameraY);
        if (target.Health != 983 || !target.KnockbackActive || rock.IsActive)
        {
            throw new InvalidDataException(
                $"Polyp rock contact mismatch: health={target.Health}, " +
                $"knockback={target.KnockbackActive}, live={rock.IsActive}.");
        }
    }

    private static void VerifyBodyAndShotReactions(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        (RoomEnemySystem enemies, _) = LoadRoom(bus, room, assets, out SamusState samus);
        RoomEnemySlot fune = enemies.Slots.Take(enemies.EnemyCount).First(slot =>
            slot.EnemyDefinitionPointer == FuneDefinition);
        Isolate(enemies, fune);
        (ushort cameraX, ushort cameraY) = CenterCamera(room, fune);
        enemies.StepFrame(cameraX, cameraY, false, samus, level: assets.LevelData);
        samus.XPosition = fune.XPosition;
        samus.YPosition = fune.YPosition;
        samus.Health = 999;
        samus.InvincibilityTimer = 0;
        if (!enemies.ResolveOrdinarySamusContact(samus, 0) ||
            samus.Health != 989 || !samus.KnockbackActive)
        {
            throw new InvalidDataException("Fune body contact did not deal header damage ten.");
        }

        var shots = new SamusProjectileSystem();
        var bombs = new SamusBombProjectileSystem();
        ArmShot(shots.Slots[0], fune, type: 0x0002, damage: 2);
        if (enemies.ResolveOrdinaryProjectileHits(bus, shots, bombs, samus) != 1 ||
            fune.FrozenTimer == 0 || fune.Health != 20 ||
            fune.Properties.HasAny(EnemyProperties.Deleted))
        {
            throw new InvalidDataException(
                $"Fune Ice reaction mismatch: health={fune.Health}, " +
                $"frozen={fune.FrozenTimer}, properties=${fune.Properties:X4}.");
        }

        (enemies, _) = LoadRoom(bus, room, assets, out samus);
        RoomEnemySlot polyp = enemies.Slots.Take(enemies.EnemyCount)
            .First(slot => slot.EnemyDefinitionPointer == PolypDefinition);
        Isolate(enemies, polyp);
        (cameraX, cameraY) = CenterCamera(room, polyp);
        enemies.StepFrame(cameraX, cameraY, false, samus, level: assets.LevelData);
        shots = new SamusProjectileSystem();
        bombs = new SamusBombProjectileSystem();
        ArmShot(shots.Slots[0], polyp, type: 0x0200, damage: 1000);
        int hits = enemies.ResolveOrdinaryProjectileHits(bus, shots, bombs, samus);
        // Population property $0400 excludes the tiny vent from the interactive-enemy list
        // before vulnerability lookup. The all-zero table is still faithful header data,
        // but ordinary beams never reach it in this shipped room configuration.
        if (hits != 0 || polyp.Health != 1 ||
            polyp.Properties.HasAny(EnemyProperties.Deleted))
        {
            throw new InvalidDataException(
                $"Indestructible Polyp reaction mismatch: hits={hits}, health=" +
                $"{polyp.Health}, properties=${polyp.Properties:X4}.");
        }
    }

    private static void VerifyHeaders(ISnesAddressSpace bus)
    {
        RoomEnemyDefinition fune = RoomEnemySystem.ReadDefinition(bus, FuneDefinition);
        if (fune.TileDataSize != 0x0800 || fune.PalettePointer != 0x9379 ||
            fune.Health != 20 || fune.Damage != 10 ||
            fune.XRadius != 16 || fune.YRadius != 16 || fune.Bank != 0xa8 ||
            fune.InitializationAiPointer != 0x96e3 || fune.PartCount != 1 ||
            fune.MainAiPointer != 0x9730 || fune.GrappleAiPointer != 0x800f ||
            fune.HurtAiPointer != 0x804c || fune.FrozenAiPointer != 0x8041 ||
            fune.DeathAnimation != 2 || fune.TouchAiPointer != 0x8023 ||
            fune.ShotAiPointer != 0x802d || fune.TileDataAddress != 0xb19e00 ||
            fune.Layer != 5 || fune.ItemDropChancesPointer != 0xf3d4 ||
            fune.VulnerabilityPointer != 0xeeb0 || fune.NamePointer != 0xe2bb)
        {
            throw new InvalidDataException("Retail Fune header does not match $A0:E6FF.");
        }

        RoomEnemyDefinition polyp = RoomEnemySystem.ReadDefinition(bus, PolypDefinition);
        if (polyp.TileDataSize != 0x0600 || polyp.PalettePointer != 0xba5b ||
            polyp.Health != 1 || polyp.Damage != 4 ||
            polyp.XRadius != 4 || polyp.YRadius != 4 || polyp.Bank != 0xa2 ||
            polyp.InitializationAiPointer != 0xb570 || polyp.PartCount != 1 ||
            polyp.MainAiPointer != 0xb58f || polyp.GrappleAiPointer != 0x800f ||
            polyp.HurtAiPointer != 0x804c || polyp.FrozenAiPointer != 0x8041 ||
            polyp.DeathAnimation != 0 || polyp.TouchAiPointer != 0x8023 ||
            polyp.ShotAiPointer != 0x802d || polyp.TileDataAddress != 0xaeb800 ||
            polyp.Layer != 5 || polyp.ItemDropChancesPointer != 0xf48e ||
            polyp.VulnerabilityPointer != 0xeec6 || polyp.NamePointer != 0xe029)
        {
            throw new InvalidDataException("Retail Polyp header does not match $A0:D1FF.");
        }
    }

    private static void VerifyProjectileDefinitions(ISnesAddressSpace bus)
    {
        VerifyProjectileDefinition(
            bus, 0xbd5a, init: 0xbbdb, pre: 0xbc0f, list: 0xbbd5,
            radii: 0x0202, properties: 0x0010);
        VerifyProjectileDefinition(
            bus, 0xdfca, init: 0xded6, pre: 0xdf39, list: 0xde96,
            radii: 0x0804, properties: 0x003c);
    }

    private static void VerifyProjectileDefinition(
        ISnesAddressSpace bus,
        ushort pointer,
        ushort init,
        ushort pre,
        ushort list,
        ushort radii,
        ushort properties)
    {
        if (ReadWord(bus, 0x860000 | pointer) != init ||
            ReadWord(bus, 0x860000 | unchecked((ushort)(pointer + 2))) != pre ||
            ReadWord(bus, 0x860000 | unchecked((ushort)(pointer + 4))) != list ||
            ReadWord(bus, 0x860000 | unchecked((ushort)(pointer + 6))) != radii ||
            ReadWord(bus, 0x860000 | unchecked((ushort)(pointer + 8))) != properties)
        {
            throw new InvalidDataException(
                $"Enemy projectile definition $86:{pointer:X4} differs from retail data.");
        }
    }

    private static (RoomEnemySystem Enemies, Bank80SystemState Random) LoadRoom(
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
        return (enemies, random);
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
            Pose = SamusPoseIds.FacingRightNormalPose,
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

    private static FuneNamiheEnemyState FuneState(
        RoomEnemySystem enemies,
        RoomEnemySlot actor) =>
        enemies.FuneNamiheStates[actor.SlotIndex] ?? throw new InvalidDataException(
            $"Fune slot {actor.SlotIndex} has no typed state.");

    private static PolypEnemyState PolypState(
        RoomEnemySystem enemies,
        RoomEnemySlot actor) =>
        enemies.PolypStates[actor.SlotIndex] ?? throw new InvalidDataException(
            $"Polyp slot {actor.SlotIndex} has no typed state.");

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
}
