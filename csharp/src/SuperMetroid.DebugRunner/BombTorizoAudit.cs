using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>
/// End-to-end ROM-backed audit for the area-zero Bomb Torizo. It consumes the retail room,
/// enemy header/population, extended spritemaps, instruction lists, vulnerability table,
/// projectile definitions, and collision layer while supplying deterministic Samus, RNG,
/// item-trigger removal, and weapon stimuli.
/// </summary>
internal static partial class BombTorizoAudit
{
    private const ushort RoomPointer = 0x9804;
    private const ushort Definition = 0xeeff;
    private const ushort PopulationPointer = 0x84ed;
    private const ushort TilesetPointer = 0x80b3;
    private const ushort CameraX = 0;
    private const ushort CameraY = 0;
    private const ushort StandUpSitDownShotCallback = 0xc9c2;
    private const ushort MainShotCallback = 0xc97c;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, RoomPointer);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        VerifyRetailStructures(bus, room);
        VerifyHitboxBoundaries(bus, room, assets);
        VerifyEncounter(bus, room, assets);
        VerifyNaturalAttackInteractions(bus, room, assets);
        VerifyLowHealthAndDeathProjectileLifecycles(bus, room, assets);
        VerifyAlreadyDefeatedLoad(bus, room, assets);

        Console.WriteLine(
            "Bomb Torizo audit passed: retail header/population, PLM-gated awakening, " +
            "extended animation maps, collision movement, Chozo-orb/sonic/swipe attacks, " +
            "exact natural attack contact, Chozo-orb shot response, Samus body contact " +
            "damage, projectile damage/flash, low-health gut/face breakup, drool and " +
            "death-explosion lifecycles, death bytecode, item drop, " +
            "delayed music, and area Torizo boss bit all used cartridge data.");
        return 0;
    }

    private static void VerifyRetailStructures(
        ISnesAddressSpace bus,
        CartridgeRoomHeader room)
    {
        if (room.Pointer != RoomPointer || room.AreaIndex != AreaId.Crateria ||
            room.WidthInScreens != 1 || room.HeightInScreens != 1 ||
            room.State.Pointer != 0x981b ||
            room.State.EnemyPopulationPointer != PopulationPointer ||
            room.State.EnemyTilesetPointer != TilesetPointer)
        {
            throw new InvalidDataException(
                $"Bomb Torizo room mismatch: room/state=${room.Pointer:X4}/${room.State.Pointer:X4}, " +
                $"area={room.AreaIndex}, size={room.WidthInScreens}x{room.HeightInScreens}, " +
                $"population/tiles=${room.State.EnemyPopulationPointer:X4}/" +
                $"${room.State.EnemyTilesetPointer:X4}.");
        }

        ushort[] expectedPopulation =
            [Definition, 0x00db, 0x00b3, 0, 0x2000, 0, 0, 0];
        for (int word = 0; word < expectedPopulation.Length; word++)
        {
            ushort actual = ReadWord(bus, 0xa10000 | (PopulationPointer + word * 2));
            if (actual != expectedPopulation[word])
            {
                throw new InvalidDataException(
                    $"Bomb Torizo population word {word} is ${actual:X4}, expected " +
                    $"${expectedPopulation[word]:X4}.");
            }
        }

        RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(bus, Definition);
        if (definition.Bank != 0xaa || definition.Health != 800 || definition.Damage != 8 ||
            definition.XRadius != 18 || definition.YRadius != 48 || definition.BossId != 2 ||
            definition.InitializationAiPointer != 0xc87f ||
            definition.MainAiPointer != 0xc6a4 || definition.HurtAiPointer != 0xc67e ||
            definition.TouchAiPointer != 0xc977 || definition.ShotAiPointer != 0xc97c ||
            definition.VulnerabilityPointer != 0xf0c0)
        {
            throw new InvalidDataException(
                $"Bomb Torizo header mismatch: bank=${definition.Bank:X2}, " +
                $"health/damage={definition.Health}/{definition.Damage}, " +
                $"radius={definition.XRadius}x{definition.YRadius}, boss={definition.BossId}, " +
                $"init/main/hurt=${definition.InitializationAiPointer:X4}/" +
                $"${definition.MainAiPointer:X4}/${definition.HurtAiPointer:X4}, " +
                $"touch/shot/vulnerability=${definition.TouchAiPointer:X4}/" +
                $"${definition.ShotAiPointer:X4}/${definition.VulnerabilityPointer:X4}.");
        }
    }

    private static void VerifyEncounter(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        bool handTriggerPresent = true;
        bool bossBitSet = false;
        LoadedBombTorizo loaded = Load(
            bus,
            room,
            assets,
            alreadyDefeated: false,
            header => header == 0xd6ea && handTriggerPresent,
            () => bossBitSet = true);

        VerifyStandUpSitDownNoOp(bus, room, assets);
        VerifyNormalBombReaction(bus, room, assets);

        if (loaded.Enemies.EnemyCount != 1 || loaded.Head.Health != 800 ||
            loaded.Head.XPosition != 0x00db || loaded.Head.YPosition != 0x00b3 ||
            loaded.State.Function != 0xc6bf || loaded.Head.XRadius != 18 ||
            loaded.Head.YRadius != 48)
        {
            throw new InvalidDataException(
                $"Bomb Torizo initialization mismatch: count={loaded.Enemies.EnemyCount}, " +
                $"health={loaded.Head.Health}, pos=(${loaded.Head.XPosition:X4}," +
                $"${loaded.Head.YPosition:X4}), function=${loaded.State.Function:X4}, " +
                $"radius={loaded.Head.XRadius}x{loaded.Head.YRadius}.");
        }

        // The initial list installs $AA:C6C6. Keeping $D6EA present must pin that exact
        // state; removing it advances the same ROM list and queues track six after 8 frames.
        for (int frame = 0; frame < 120; frame++)
            Step(loaded, assets.LevelData, stepProjectiles: true);
        if (loaded.State.Function != 0xc6c6 || loaded.State.AwakeningReleased)
            throw new InvalidDataException("Bomb Torizo did not remain gated by PLM $D6EA.");

        handTriggerPresent = false;
        var maps = new HashSet<ushort>();
        var attacks = new HashSet<RoomEnemyProjectileKind>();
        bool sawAwakeningMusic = false;
        bool movedSamusAcrossAttackSelectorPhase = false;
        ushort startX = loaded.Head.XPosition;
        for (int frame = 0; frame < 12000; frame++)
        {
            Step(loaded, assets.LevelData, stepProjectiles: true);
            maps.Add(loaded.Head.SpritemapPointer);
            sawAwakeningMusic |= loaded.Enemies.LastBombTorizoMusicRequest is
                { Command.RawValue: 6, Delay.Frames: 8 };
            foreach (RoomEnemyProjectileSlot projectile in loaded.Enemies.EnemyProjectiles)
            {
                if (projectile.IsActive)
                    attacks.Add(projectile.Kind);
            }

            // $AA:C5A4 deliberately combines the NMI phase with half of Samus's X
            // coordinate. Moving her by sixteen pixels flips selector bit 3 without
            // bypassing the ROM instruction, giving this deterministic audit coverage of
            // both authored branches instead of relying on incidental frame timing.
            if (!movedSamusAcrossAttackSelectorPhase &&
                attacks.Contains(RoomEnemyProjectileKind.BombTorizoChozoOrb))
            {
                loaded.Samus.XPosition = unchecked((ushort)(loaded.Samus.XPosition + 16));
                movedSamusAcrossAttackSelectorPhase = true;
            }
            if (attacks.Contains(RoomEnemyProjectileKind.BombTorizoChozoOrb) &&
                attacks.Contains(RoomEnemyProjectileKind.BombTorizoSonicBoom) &&
                attacks.Contains(RoomEnemyProjectileKind.BombTorizoExplosiveSwipe) &&
                maps.Count >= 8)
            {
                break;
            }
        }

        if (!loaded.State.AwakeningReleased || !sawAwakeningMusic || maps.Count < 8 ||
            loaded.Head.XPosition == startX ||
            !attacks.Contains(RoomEnemyProjectileKind.BombTorizoChozoOrb) ||
            !attacks.Contains(RoomEnemyProjectileKind.BombTorizoSonicBoom) ||
            !attacks.Contains(RoomEnemyProjectileKind.BombTorizoExplosiveSwipe))
        {
            throw new InvalidDataException(
                $"Bomb Torizo live cycle incomplete: released={loaded.State.AwakeningReleased}, " +
                $"music={sawAwakeningMusic}, maps={maps.Count}, moved=" +
                $"{loaded.Head.XPosition != startX}, attacks=[{string.Join(',', attacks)}].");
        }

        var oam = new OamBuffer();
        oam.BeginFrame();
        loaded.Enemies.DrawEnemyProjectiles(oam, CameraX, CameraY);
        loaded.Enemies.DrawLayers(oam, CameraX, CameraY, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException("Bomb Torizo encounter emitted no OBJ pieces.");

        // Place Samus over the current extended body map. The header's eight damage points
        // must travel through $AA:C977 and common touch damage, including knockback.
        loaded.Samus.XPosition = loaded.Head.XPosition;
        loaded.Samus.YPosition = loaded.Head.YPosition;
        loaded.Samus.InvincibilityTimer = 0;
        // A live attack may have legitimately touched the deterministic Samus during the
        // long AI walk above. Isolate this explicit body-contact stimulus from that prior
        // history so the assertion measures Torizo's touch callback, not a pending hurt arc.
        loaded.Samus.KnockbackTimer = 0;
        loaded.Samus.KnockbackDirection = 0;
        loaded.Samus.KnockbackActive = false;
        loaded.Samus.Pose = SamusPoseIds.FacingRightNormalPose;
        loaded.Samus.RefreshCollisionRadii(bus);
        loaded.Samus.InitializeAnimation(bus);
        EnemyContactAuditAssertions.BeforeHit beforeTouch = EnemyContactAuditAssertions.Capture(loaded.Samus);
        if (!loaded.Enemies.ResolveOrdinarySamusContact(loaded.Samus, controllerInput: 0))
            throw new InvalidDataException("Bomb Torizo body contact was not admitted.");
        // Native touch publishes the hit first; the later movement pass owns knockback.
        EnemyContactAuditAssertions.VerifyStandingAirHit(
            bus, loaded.Samus, beforeTouch, damage: 8, expectedSide: 1, "Bomb Torizo body contact");

        var shots = new SamusProjectileSystem();
        var bombs = new SamusBombProjectileSystem();
        ArmProjectile(shots.Slots[0], loaded.Head, type: 0x0200, damage: 4000);
        int hits = loaded.Enemies.ResolveOrdinaryProjectileHits(bus, shots, bombs, loaded.Samus);
        if (hits != 1 || loaded.Head.Health != 0 || !loaded.State.DeathStarted ||
            loaded.Head.Properties.HasAny(EnemyProperties.Deleted) ||
            !loaded.Head.Properties.HasAny(EnemyProperties.IgnoreSamusCollision) ||
            loaded.Head.Properties.HasAny(EnemyProperties.SolidToSamus))
        {
            throw new InvalidDataException(
                $"Bomb Torizo fatal shot mismatch: hits={hits}, health={loaded.Head.Health}, " +
                $"death={loaded.State.DeathStarted}, deleted=" +
                $"{loaded.Head.Properties.HasAny(EnemyProperties.Deleted)}, properties=" +
                $"${loaded.Head.Properties:X4}.");
        }

        bool sawDeathMusic = false;
        for (int frame = 0; frame < 6000 && !loaded.State.BossBitSet; frame++)
        {
            Step(loaded, assets.LevelData, stepProjectiles: true);
            sawDeathMusic |= loaded.Enemies.LastBombTorizoMusicRequest is
                { Command.RawValue: 3, Delay.Frames: 8 };
        }
        if (!loaded.State.BossBitSet || !bossBitSet || !loaded.State.ItemDropRequested ||
            !sawDeathMusic)
        {
            throw new InvalidDataException(
                $"Bomb Torizo death incomplete: state/callback boss=" +
                $"{loaded.State.BossBitSet}/{bossBitSet}, drop={loaded.State.ItemDropRequested}, " +
                $"music={sawDeathMusic}.");
        }
    }

    /// <summary>
    /// The long encounter route above proves producer cadence and boss choreography. These
    /// independent copies consume one naturally spawned actor apiece so the three damaging
    /// normal-health attacks also prove the shared Samus-contact tail without changing the
    /// authoritative fight timing. A fourth copy executes the orb's shootable response.
    /// </summary>
    private static void VerifyNaturalAttackInteractions(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        RoomEnemyProjectileKind[] damagingKinds =
        [
            RoomEnemyProjectileKind.BombTorizoChozoOrb,
            RoomEnemyProjectileKind.BombTorizoSonicBoom,
            RoomEnemyProjectileKind.BombTorizoExplosiveSwipe,
        ];
        foreach (RoomEnemyProjectileKind kind in damagingKinds)
        {
            (LoadedBombTorizo loaded, RoomEnemyProjectileSlot projectile, int frame) =
                AdvanceToNaturalAttack(bus, room, assets, kind);
            ushort definitionProperties = ReadWord(
                bus,
                0x860000 | unchecked((ushort)((ushort)kind + 8)));
            ushort definitionRadii = ReadWord(
                bus,
                0x860000 | unchecked((ushort)((ushort)kind + 6)));
            if (projectile.Damage != (definitionProperties & 0x0fff) ||
                projectile.XRadius != unchecked((byte)definitionRadii) ||
                projectile.YRadius != unchecked((byte)(definitionRadii >> 8)) ||
                !projectile.CanDamageSamus)
            {
                throw new InvalidDataException(
                    $"Natural Bomb Torizo {kind} at frame {frame} disagrees with its ROM " +
                    $"definition: damage/radii/collision={projectile.Damage}/" +
                    $"{projectile.XRadius}x{projectile.YRadius}/" +
                    $"{projectile.CanDamageSamus}, properties/radii=" +
                    $"${definitionProperties:X4}/${definitionRadii:X4}.");
            }

            EnemyProjectileAuditAssertions.VerifyNaturalSamusContact(
                bus,
                loaded.Enemies,
                loaded.Samus,
                new SamusBombProjectileSystem(),
                assets.LevelData,
                projectile,
                CameraX,
                CameraY,
                unchecked((byte)frame));
        }

        (LoadedBombTorizo shotLoaded, RoomEnemyProjectileSlot orb, int shotFrame) =
            AdvanceToNaturalAttack(
                bus,
                room,
                assets,
                RoomEnemyProjectileKind.BombTorizoChozoOrb);
        ushort orbX = orb.XPosition;
        ushort orbY = orb.YPosition;
        ushort shotResponse = EnemyProjectileAuditAssertions.VerifyNaturalDestructibleSamusShot(
            bus,
            shotLoaded.Enemies,
            new SamusProjectileSystem(),
            new SamusBombProjectileSystem(),
            orb);
        var responseMaps = new HashSet<ushort>();
        for (int frame = 0; frame < 128 && orb.IsActive; frame++)
        {
            shotLoaded.Enemies.StepEnemyProjectiles(
                assets.LevelData,
                samus: shotLoaded.Samus,
                cameraX: CameraX,
                cameraY: CameraY,
                nmiFrameCounter8: unchecked((byte)(shotFrame + frame + 1)));
            if (orb.IsActive && orb.SpritemapPointer is not 0 and not 0x8000)
                responseMaps.Add(orb.SpritemapPointer);
        }
        TorizoOrbDropRequest? drop = shotLoaded.Enemies.TorizoOrbDropRequests.Count == 1
            ? shotLoaded.Enemies.TorizoOrbDropRequests[0]
            : null;
        if (orb.IsActive || responseMaps.Count == 0 || drop is null ||
            drop.Value.X != orbX || drop.Value.Y != orbY ||
            drop.Value.EnemyDefinitionPointer != 0xef3f ||
            drop.Value.ItemDropChancesPointer != 0xf3fe)
        {
            throw new InvalidDataException(
                $"Bomb Torizo Chozo-orb shot response $86:{shotResponse:X4} ended with " +
                $"live={orb.IsActive} after {responseMaps.Count} visible maps; drop=" +
                (drop is null
                    ? "none."
                    : $"({drop.Value.X},{drop.Value.Y}) header/table=" +
                      $"${drop.Value.EnemyDefinitionPointer:X4}/" +
                      $"${drop.Value.ItemDropChancesPointer:X4}."));
        }
    }

    /// <summary>
    /// Runs the real PLM-unlocked boss until the requested bank-$86 definition is allocated.
    /// Sonic/swipe searches reproduce the retail selector-phase change already used by the
    /// main audit after its first orb, but never write the enemy state or projectile pool.
    /// </summary>
    private static (LoadedBombTorizo Loaded, RoomEnemyProjectileSlot Projectile, int Frame)
        AdvanceToNaturalAttack(
            SuperMetroidAddressSpace bus,
            CartridgeRoomHeader room,
            CartridgeRoomAssets assets,
            RoomEnemyProjectileKind requestedKind)
    {
        LoadedBombTorizo loaded = Load(
            bus,
            room,
            assets,
            alreadyDefeated: false,
            _ => false,
            () => { });
        bool selectorPhaseChanged = false;
        for (int frame = 0; frame < 12000; frame++)
        {
            loaded.Enemies.StepFrame(
                CameraX,
                CameraY,
                timeIsFrozen: false,
                loaded.Samus,
                level: assets.LevelData,
                nmiFrameCounter8: unchecked((byte)frame));

            RoomEnemyProjectileSlot? requested = loaded.Enemies.EnemyProjectiles.FirstOrDefault(
                projectile => projectile.Kind == requestedKind);
            if (requested is not null)
                return (loaded, requested, frame);

            if (!selectorPhaseChanged && loaded.Enemies.EnemyProjectiles.Any(
                    projectile => projectile.Kind == RoomEnemyProjectileKind.BombTorizoChozoOrb))
            {
                loaded.Samus.XPosition = unchecked((ushort)(loaded.Samus.XPosition + 16));
                selectorPhaseChanged = true;
            }

            // Keep the active Samus state installed because native death/drop selection
            // reads its inventory. The finder moves Samus away from the current attack
            // above, so retaining that state does not manufacture a projectile contact.
            loaded.Enemies.StepEnemyProjectiles(
                assets.LevelData,
                samus: loaded.Samus,
                cameraX: CameraX,
                cameraY: CameraY,
                nmiFrameCounter8: unchecked((byte)frame));
        }

        throw new InvalidDataException(
            $"Bomb Torizo never naturally produced {requestedKind} within 12000 frames.");
    }

    private static void VerifyAlreadyDefeatedLoad(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedBombTorizo loaded = Load(
            bus,
            room,
            assets,
            alreadyDefeated: true,
            _ => false,
            () => { });
        if (!loaded.Head.Properties.HasAny(EnemyProperties.Deleted) ||
            !loaded.State.BossBitSet)
        {
            throw new InvalidDataException("Defeated Bomb Torizo did not delete during init.");
        }
    }

    /// <summary>
    /// Area-zero `$AA:C9C2` is intentionally empty even when the awakening shot guard is
    /// clear. The bank-$A0 extended collision prelude still marks the projectile, but the
    /// callback must not create an impact, consult vulnerability, or damage Bomb Torizo.
    /// </summary>
    private static void VerifyStandUpSitDownNoOp(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedBombTorizo loaded = Load(
            bus,
            room,
            assets,
            alreadyDefeated: false,
            _ => false,
            () => { });
        ushort shotX = 0;
        ushort shotY = 0;
        bool found = false;
        for (int frame = 0; frame < 12000; frame++)
        {
            if (RetailExtendedHitboxProbe.TryFindShotPoint(
                    bus,
                    loaded.Head,
                    StandUpSitDownShotCallback,
                    out shotX,
                    out shotY))
            {
                found = true;
                break;
            }
            Step(loaded, assets.LevelData, stepProjectiles: true);
        }
        if (!found)
        {
            throw new InvalidDataException(
                "Bomb Torizo never displayed an authored $AA:C9C2 shot hitbox.");
        }

        loaded.Head.FlashTimer = 0;
        loaded.State.ShotGuard = 0;
        ushort healthBefore = loaded.Head.Health;
        ushort parameter2Before = loaded.Head.Parameter2;
        var shots = new SamusProjectileSystem();
        var bombs = new SamusBombProjectileSystem();
        ArmProjectile(shots.Slots[0], loaded.Head, type: 0x8200, damage: 300);
        shots.Slots[0].XPosition = shotX;
        shots.Slots[0].YPosition = shotY;
        shots.Slots[0].XRadius = 1;
        shots.Slots[0].YRadius = 1;
        int hits = loaded.Enemies.ResolveOrdinaryProjectileHits(
            bus,
            shots,
            bombs,
            loaded.Samus);
        if (hits != 1 || loaded.Head.Health != healthBefore ||
            loaded.Head.Parameter2 != parameter2Before || shots.Slots[0].Type != 0x8200 ||
            (shots.Slots[0].Direction & 0x0010) == 0)
        {
            throw new InvalidDataException(
                $"Bomb Torizo $C9C2 no-op mismatch: hits={hits}, " +
                $"health={healthBefore}->{loaded.Head.Health}, parameter2=" +
                $"${parameter2Before:X4}->${loaded.Head.Parameter2:X4}, projectile=" +
                $"${shots.Slots[0].Type:X4}/${shots.Slots[0].Direction:X4}.");
        }
    }

    /// <summary>
    /// Selects a real <c>$AA:C97C</c> body rectangle and sends an exploding normal bomb
    /// through the public bank-$A0 dispatcher. This covers Bomb Torizo's guard/flash gate,
    /// common no-death vulnerability tail, and preservation of the staged boss slot.
    /// </summary>
    private static void VerifyNormalBombReaction(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedBombTorizo loaded = Load(
            bus,
            room,
            assets,
            alreadyDefeated: false,
            _ => false,
            () => { });
        ushort bombX = 0;
        ushort bombY = 0;
        bool found = false;
        for (int frame = 0; frame < 12000; frame++)
        {
            if (RetailExtendedHitboxProbe.TryFindShotPoint(
                    bus,
                    loaded.Head,
                    MainShotCallback,
                    out bombX,
                    out bombY))
            {
                found = true;
                break;
            }
            Step(loaded, assets.LevelData, stepProjectiles: true);
        }
        if (!found)
            throw new InvalidDataException("Bomb Torizo never displayed a $AA:C97C hitbox.");

        loaded.Head.FlashTimer = 0;
        loaded.State.ShotGuard = 0;
        var bombs = new SamusBombProjectileSystem();
        var shots = new SamusProjectileSystem();
        SamusBombProjectileSlot bomb =
            EnemyProjectileAuditAssertions.ArmExplodingNormalBomb(
                bombs,
                bombX,
                bombY,
                damage: 100);
        bomb.XRadius = 1;
        bomb.YRadius = 1;
        ushort healthBefore = loaded.Head.Health;
        byte vulnerability = bus.ReadByte(
            0xb40000 | unchecked((ushort)(loaded.Head.Definition.VulnerabilityPointer + 14)));
        int damage = (bomb.Damage >> 1) * (vulnerability & 0x7f);
        ushort expectedHealth = damage >= healthBefore
            ? (ushort)0
            : unchecked((ushort)(healthBefore - damage));
        int hits = loaded.Enemies.ResolveOrdinaryBombHits(bombs, shots, loaded.Samus);
        if (hits != 1 || (bomb.Direction & 0x0010) == 0 ||
            loaded.Head.Health != expectedHealth ||
            loaded.Head.Properties.HasAny(EnemyProperties.Deleted) ||
            loaded.State.DeathStarted != (expectedHealth == 0))
        {
            throw new InvalidDataException(
                $"Bomb Torizo normal-bomb reaction mismatch: hits={hits}, " +
                $"direction=${bomb.Direction:X4}, health={loaded.Head.Health}/{expectedHealth}, " +
                $"vulnerability=${vulnerability:X2}, death={loaded.State.DeathStarted}, " +
                $"deleted={loaded.Head.Properties.HasAny(EnemyProperties.Deleted)}.");
        }
    }

    private static LoadedBombTorizo Load(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        bool alreadyDefeated,
        Func<ushort, bool> isRoomPlmPresent,
        Action setBossBit)
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState(0x1234);
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Missiles = 10,
            MaxMissiles = 10,
            XPosition = 0x0060,
            YPosition = 0x00b0,
            Pose = SamusPoseIds.FacingRightNormalPose,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);

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
            samus: samus,
            isAreaTorizoDefeated: () => alreadyDefeated,
            setAreaTorizoDefeated: setBossBit,
            isRoomPlmPresent: isRoomPlmPresent);

        RoomEnemySlot head = enemies.Slots[0];
        TorizoEnemyState state = enemies.BombTorizo ?? throw new InvalidDataException(
            "Bomb Torizo load produced no typed state.");
        return new LoadedBombTorizo(enemies, samus, head, state);
    }

    private static void Step(
        LoadedBombTorizo loaded,
        RoomLevelData level,
        bool stepProjectiles)
    {
        loaded.Enemies.StepFrame(
            CameraX,
            CameraY,
            timeIsFrozen: false,
            loaded.Samus,
            level: level);
        if (stepProjectiles)
        {
            loaded.Enemies.StepEnemyProjectiles(
                level,
                loaded.Samus,
                cameraX: CameraX,
                cameraY: CameraY);
        }
    }

    private static void ArmProjectile(
        SamusProjectileSlot projectile,
        RoomEnemySlot target,
        ushort type,
        ushort damage)
    {
        projectile.ClearFields();
        projectile.Type = type;
        projectile.Damage = damage;
        projectile.Direction = (ushort)SamusProjectileDirection.Right;
        projectile.XPosition = target.XPosition;
        projectile.YPosition = target.YPosition;
        projectile.XRadius = 8;
        projectile.YRadius = 16;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private readonly record struct LoadedBombTorizo(
        RoomEnemySystem Enemies,
        SamusState Samus,
        RoomEnemySlot Head,
        TorizoEnemyState State);
}
