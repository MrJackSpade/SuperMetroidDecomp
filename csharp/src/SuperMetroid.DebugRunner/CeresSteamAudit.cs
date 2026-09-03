using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>
/// ROM-backed audit for all six Ceres steam population variants. This intentionally uses
/// the real post-Ridley elevator population so its two Mode-7 variants execute beside the
/// translated Ceres doors exactly as they do in the retail enemy scheduler.
/// </summary>
internal static class CeresSteamAudit
{
    private const ushort ElevatorRoomPointer = 0xdf45;
    private const ushort EscapeElevatorStatePointer = 0xdf71;
    private const ushort EscapeElevatorPopulationPointer = 0xe95c;

    // Retail enemy coverage proves Steam appears in these five population lists. Keeping
    // the complete set here prevents a future audit from validating only the convenient
    // elevator actors while silently losing an ordinary directional variant elsewhere.
    private static readonly ushort[] RetailPopulationPointers =
        [0x8c0d, 0x8da0, 0xe95c, 0xea2f, 0xeb02];

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        VerifyDefinitionAndRetailPopulations(bus);
        VerifyEscapeElevatorAnimationMode7AndCollision(bus);

        Console.WriteLine(
            "Ceres steam audit passed: 68 retail records across five populations cover " +
            "all six variants; ROM bytecode animated the post-Ridley elevator plumes, " +
            "Mode-7 graphical offsets matched the shared signed transform, OBJ rendered, " +
            "authored touch rectangles remained live, and canonical-header suppression " +
            "rejected projectile/bomb multibox scans before collision side effects.");
        return 0;
    }

    private static void VerifyDefinitionAndRetailPopulations(ISnesAddressSpace bus)
    {
        RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(
            bus,
            RoomEnemySystem.CeresSteamDefinition);
        if (definition.Bank != 0xa6 || definition.Health != 0x7fff ||
            definition.Damage != 0 || definition.InitializationAiPointer != 0xefb1 ||
            definition.MainAiPointer != 0xf00d || definition.TouchAiPointer != 0xf03f ||
            definition.ShotAiPointer != 0x804c)
        {
            throw new InvalidDataException(
                $"Ceres steam header mismatch: bank=${definition.Bank:X2}, " +
                $"hp/damage={definition.Health}/{definition.Damage}, " +
                $"init/main=${definition.InitializationAiPointer:X4}/" +
                $"${definition.MainAiPointer:X4}, touch/shot=" +
                $"${definition.TouchAiPointer:X4}/${definition.ShotAiPointer:X4}.");
        }

        int steamRecordCount = 0;
        var variants = new HashSet<ushort>();
        foreach (ushort populationPointer in RetailPopulationPointers)
        {
            int address = 0xa10000 | populationPointer;
            for (int guard = 0; guard < RoomEnemySystem.MaximumEnemyCount; guard++)
            {
                ushort enemyDefinition = ReadWord(bus, address);
                if (enemyDefinition == 0xffff)
                    break;
                if (enemyDefinition == RoomEnemySystem.CeresSteamDefinition)
                {
                    steamRecordCount++;
                    variants.Add(ReadWord(bus, address + 12));
                }
                address += 16;
            }
        }

        if (steamRecordCount != 68 ||
            !variants.SetEquals(new ushort[] { 0, 1, 2, 3, 4, 5 }))
        {
            throw new InvalidDataException(
                $"Ceres steam coverage found {steamRecordCount} records and variants " +
                $"[{string.Join(',', variants.Order())}], expected 68 and 0..5.");
        }
    }

    private static void VerifyEscapeElevatorAnimationMode7AndCollision(
        SuperMetroidAddressSpace bus)
    {
        var selection = new RoomStateSelectionContext(
            ReadOnlyMemory<byte>.Empty,
            BossBits: 1,
            HasMorphBallAndMissiles: false,
            HasPowerBombs: false);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(
            bus,
            ElevatorRoomPointer,
            selection);
        if (room.State.Pointer != EscapeElevatorStatePointer ||
            room.State.EnemyPopulationPointer != EscapeElevatorPopulationPointer)
        {
            throw new InvalidDataException(
                $"Ceres elevator selected state/population " +
                $"${room.State.Pointer:X4}/${room.State.EnemyPopulationPointer:X4}.");
        }

        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState(0x1234);
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusPoseIds.FacingRightNormalPose,
            XPosition = 0x005e,
            YPosition = 0x006c,
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
            samus: samus);

        RoomEnemySlot[] steam = enemies.Slots
            .Take(enemies.EnemyCount)
            .Where(slot => slot.EnemyDefinitionPointer == RoomEnemySystem.CeresSteamDefinition)
            .ToArray();
        if (enemies.EnemyCount != 13 || steam.Length != 11 ||
            steam.Any(slot => slot.Health != 0x7fff || slot.PaletteIndex != 0x0a00 ||
                slot.VramTilesIndex != 0 || slot.InstructionTimer != 1 ||
                !slot.Properties.HasAny(EnemyProperties.ProcessInstructions) ||
                !slot.ExtraProperties.HasAny(EnemyExtraProperties.UsesExtendedSpritemap)) ||
            steam.Count(slot => slot.Parameter1 == 4) != 5 ||
            steam.Count(slot => slot.Parameter1 == 5) != 6)
        {
            throw new InvalidDataException(
                $"Ceres elevator load produced {enemies.EnemyCount} enemies and " +
                $"{steam.Length} steam actors with invalid initialization state.");
        }

        var identity = new SamusMode7Transform(0x0100, 0, 0, 0x0080, 0x03f0);
        RoomEnemySlot target = steam.First(slot => slot.Parameter1 == 4);
        var animationMaps = new HashSet<ushort>();
        bool sawVisibleFrame = false;
        bool sawContact = false;
        bool verifiedShotSuppression = false;
        bool verifiedNormalBombSuppression = false;

        // The random activation delay is at most 32 one-frame instruction passes. The full
        // visible burst is seven maps at three frames each followed by a 64-frame hidden
        // wait, so 192 frames necessarily cover a complete burst for this deterministic
        // retail actor without mutating its private timer.
        for (int frame = 0; frame < 192; frame++)
        {
            enemies.StepFrame(
                cameraX: 0,
                cameraY: 0,
                timeIsFrozen: false,
                samus,
                level: assets.LevelData,
                mode7Transform: identity);

            if (target.SpawnXOffset != 0 || target.SpawnYOffset != 0)
            {
                throw new InvalidDataException(
                    $"Identity Mode-7 steam offset became " +
                    $"({target.SpawnXOffset:X4},{target.SpawnYOffset:X4}).");
            }

            if (!target.Properties.HasAny(
                    EnemyProperties.Invisible | EnemyProperties.IgnoreSamusCollision) &&
                target.SpritemapPointer != 0)
            {
                sawVisibleFrame = true;
                animationMaps.Add(target.SpritemapPointer);

                // Centering both subjects on the vent overlaps the base portion of the
                // authored left-facing plume but would not justify replacing its growing
                // per-frame rectangle with the header's generic radius.
                if (!sawContact)
                {
                    samus.XPosition = target.XPosition;
                    samus.YPosition = target.YPosition;
                    samus.InvincibilityTimer = 0;
                    sawContact = enemies.ResolveOrdinarySamusContact(samus, 0);
                }

                if (!verifiedShotSuppression)
                {
                    var shots = new SamusProjectileSystem();
                    var bombs = new SamusBombProjectileSystem();
                    ArmPowerBeam(shots.Slots[0], target);
                    ushort healthBeforeShot = target.Health;
                    ushort instructionBeforeShot = target.CurrentInstruction;
                    int hitCount = enemies.ResolveOrdinaryProjectileHits(
                        bus,
                        shots,
                        bombs,
                        samus);
                    verifiedShotSuppression = hitCount == 0 &&
                        (shots.Slots[0].Direction & 0x0010) == 0 &&
                        shots.Slots[0].Type == 0 &&
                        shots.Slots[0].InstructionPointer == 0x9000 &&
                        target.Health == healthBeforeShot &&
                        target.CurrentInstruction == instructionBeforeShot;
                }

                if (!verifiedNormalBombSuppression)
                {
                    // Both native multibox handlers compare the definition-header shot AI
                    // against canonical `$804B/$804C` before walking any components. Steam's
                    // `$804C` header therefore suppresses the entire scan: even a physical
                    // family-$0500 explosion centered inside the visible plume is unmarked.
                    var bombProjectiles = new SamusBombProjectileSystem();
                    var ordinaryProjectiles = new SamusProjectileSystem();
                    SamusBombProjectileSlot normalBomb =
                        EnemyProjectileAuditAssertions.ArmExplodingNormalBomb(
                            bombProjectiles,
                            target.XPosition,
                            target.YPosition,
                            damage: 1000);
                    ushort healthBeforeBomb = target.Health;
                    ushort instructionBeforeBomb = target.CurrentInstruction;
                    int bombHits = enemies.ResolveOrdinaryBombHits(
                        bombProjectiles,
                        ordinaryProjectiles,
                        samus);
                    verifiedNormalBombSuppression = bombHits == 0 &&
                        (normalBomb.Direction & 0x0010) == 0 &&
                        target.Health == healthBeforeBomb &&
                        target.CurrentInstruction == instructionBeforeBomb &&
                        !target.Properties.HasAny(EnemyProperties.Deleted);
                }
            }
        }

        if (!sawVisibleFrame || animationMaps.Count != 7 || !sawContact ||
            !verifiedShotSuppression || !verifiedNormalBombSuppression ||
            samus.Health != 999 || target.Health != 0x7fff)
        {
            throw new InvalidDataException(
                $"Ceres steam cycle/combat failed: visible={sawVisibleFrame}, " +
                $"maps={animationMaps.Count}, touch/shot-suppressed/bomb-suppressed=" +
                $"{sawContact}/{verifiedShotSuppression}/{verifiedNormalBombSuppression}, " +
                $"Samus/steam HP={samus.Health}/{target.Health}.");
        }

        var rotated = new SamusMode7Transform(
            MatrixA: 0x00e0,
            MatrixB: 0x0080,
            MatrixC: unchecked((ushort)-0x0080),
            CenterX: 0x0080,
            CenterY: 0x03f0);
        enemies.StepFrame(
            cameraX: 0,
            cameraY: 0,
            timeIsFrozen: false,
            samus,
            level: assets.LevelData,
            mode7Transform: rotated);
        SamusMode7Point expected = rotated.Transform(target.XPosition, target.YPosition);
        if (target.SpawnXOffset != unchecked((ushort)(expected.X - target.XPosition)) ||
            target.SpawnYOffset != unchecked((ushort)(expected.Y - target.YPosition)))
        {
            throw new InvalidDataException(
                $"Ceres steam rotation offset " +
                $"({target.SpawnXOffset:X4},{target.SpawnYOffset:X4}) did not match " +
                $"transformed point ({expected.X:X4},{expected.Y:X4}).");
        }

        // Return to identity before drawing so the retail elevator actor remains inside the
        // audit viewport. DrawLayers must consume its extended map into ordinary SNES OBJ.
        enemies.StepFrame(0, 0, false, samus, level: assets.LevelData,
            mode7Transform: identity);
        var oam = new OamBuffer();
        oam.BeginFrame();
        enemies.DrawLayers(oam, 0, 0, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException("Ceres steam emitted no ROM-backed OBJ pieces.");
    }

    private static void ArmPowerBeam(
        SamusProjectileSlot projectile,
        RoomEnemySlot target)
    {
        projectile.ClearFields();
        projectile.Type = 0;
        projectile.Damage = 20;
        projectile.Direction = (ushort)SamusProjectileDirection.Right;
        projectile.XPosition = target.XPosition;
        projectile.YPosition = target.YPosition;
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
}
