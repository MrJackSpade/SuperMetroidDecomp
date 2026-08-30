using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Untouched-ROM audit of Mother Brain's retail two-record load, first-phase animation, and
/// custom draw hook. The event callback remains clear, so every observed steady-state frame
/// follows the same pre-glass-destruction branch the cartridge executes on a fresh save.
/// </summary>
internal static class MotherBrainAudit
{
    private const ushort RoomPointer = 0xdd58;
    private const ushort PopulationPointer = 0xe321;
    private const ushort BodyDefinition = 0xec7f;
    private const ushort HeadDefinition = 0xec3f;
    private const ushort InitialHeadSpritemap = 0xa586;
    private const ushort TurretDefinition = 0xc17e;
    private const ushort TurretBulletDefinition = 0xc18c;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, RoomPointer);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        if (room.WidthInScreens != 4 || room.HeightInScreens != 1 || room.AreaIndex != 5 ||
            room.State.EnemyPopulationPointer != PopulationPointer)
        {
            throw new InvalidDataException(
                $"Mother Brain room mismatch: {room.WidthInScreens}x{room.HeightInScreens}, " +
                $"area={room.AreaIndex}, population=$A1:{room.State.EnemyPopulationPointer:X4}.");
        }

        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState(0x1234);
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            XPosition = 0x0080,
            YPosition = 0x00a0,
            Pose = SamusState.FacingRightNormalPose,
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
            isAreaBossDefeated: () => false,
            hasEvent: _ => false);

        MotherBrainEnemyState state = enemies.MotherBrain ??
            throw new InvalidDataException("Mother Brain room did not allocate typed encounter state.");
        RoomEnemySlot body = state.Body;
        RoomEnemySlot head = state.Head ??
            throw new InvalidDataException("Mother Brain's head record was not linked to its body.");
        if (enemies.EnemyCount != 6 || body.EnemyDefinitionPointer != BodyDefinition ||
            head.EnemyDefinitionPointer != HeadDefinition || body.SlotIndex != 0 || head.SlotIndex != 1)
        {
            throw new InvalidDataException(
                $"Mother Brain population mismatch: count={enemies.EnemyCount}, " +
                $"slots=${body.EnemyDefinitionPointer:X4}/${head.EnemyDefinitionPointer:X4}.");
        }

        MotherBrainCorpseRotEntry firstRotEntry = state.CorpseRotting.ReadEntry(bus, 0);
        MotherBrainCorpseRotEntry lastRotEntry =
            state.CorpseRotting.ReadEntry(bus, MotherBrainCorpseRottingState.EntryCount - 1);
        bool turretParametersMatch = state.InitialTurretParameters.Count == 12;
        for (int index = 0; index < state.InitialTurretParameters.Count; index++)
            turretParametersMatch &= state.InitialTurretParameters[index] == index;
        AuditInitialTurretPool(bus, enemies);

        if (body.XPosition != 0x0081 || body.YPosition != 0x006f ||
            head.XPosition != 0x0081 || head.YPosition != 0x006f || head.Health != 0x0bb8 ||
            body.CurrentInstruction != 0x9c13 || head.CurrentInstruction != 0x9c21 ||
            body.Properties != 0x3d00 || head.Properties != 0x3900 ||
            body.PaletteIndex != 0 || head.PaletteIndex != 0x0200 ||
            body.VramTilesIndex != 0 || head.VramTilesIndex != 0 ||
            state.Form != 0 || state.HitboxesEnabled != 2 || state.EnableUnpauseHook ||
            state.Function != MotherBrainBodyFunction.FirstPhase ||
            state.BrainFunction != MotherBrainBrainFunction.SetupBrainToBeDrawn ||
            state.FxEntry != 1 || !state.BackgroundTilemapPrepared ||
            state.NeckPaletteIndex != 0x0200 || state.BrainPaletteIndex != 0x0200 ||
            state.BrainPaletteTimer != 10 || !state.CorpseRotting.IsInitialized ||
            firstRotEntry.YOffset != 47 || firstRotEntry.Timer != 0 ||
            lastRotEntry.YOffset != 0 || lastRotEntry.Timer != 94 || !turretParametersMatch ||
            vram.ReadWord(0x4800) != 0x0338 || vram.ReadWord(0x4fff) != 0x0338)
        {
            throw new InvalidDataException(
                $"Mother Brain initialization mismatch: body=({body.XPosition:X4},{body.YPosition:X4})/" +
                $"${body.Properties:X4}, head=({head.XPosition:X4},{head.YPosition:X4})/" +
                $"hp={head.Health}/${head.Properties:X4}, lists={body.CurrentInstruction:X4}/" +
                $"{head.CurrentInstruction:X4}, function=$A9:{(ushort)state.Function:X4}, " +
                $"corpse={state.CorpseRotting.IsInitialized}, turrets={state.InitialTurretParameters.Count}.");
        }

        // Compare every copied palette word with its ROM source, including both endpoints;
        // this catches the easy-to-miss +2 source offset and byte-index/color-index mismatch.
        for (int color = 0; color < 15; color++)
        {
            ushort expectedGlass = ReadWord(bus, 0xa99514 + color * 2);
            ushort expectedTube = ReadWord(bus, 0xa994f4 + color * 2);
            if (cgram.Colors[177 + color] != expectedGlass || cgram.Colors[241 + color] != expectedTube)
            {
                throw new InvalidDataException(
                    $"Mother Brain palette copy diverged at color {color}: " +
                    $"glass=${cgram.Colors[177 + color]:X4}/${expectedGlass:X4}, " +
                    $"tube=${cgram.Colors[241 + color]:X4}/${expectedTube:X4}.");
            }
        }

        var observedHeadMaps = new HashSet<ushort>();
        for (int frame = 0; frame < 20; frame++)
        {
            enemies.StepFrame(
                cameraX: 0,
                cameraY: 0,
                timeIsFrozen: false,
                samus,
                level: assets.LevelData,
                nmiFrameCounter8: unchecked((byte)frame));
            observedHeadMaps.Add(head.SpritemapPointer);

            var oam = new OamBuffer();
            oam.BeginFrame();
            enemies.DrawLayers(oam, 0, 0, firstLayer: 5, lastLayer: 5);
            ushort authoredEntryCount = ReadWord(bus, 0xa90000 | head.SpritemapPointer);
            if (head.SpritemapPointer != InitialHeadSpritemap ||
                oam.NextByteOffset / 4 != authoredEntryCount)
            {
                throw new InvalidDataException(
                    $"Mother Brain draw hook mismatch on frame {frame}: map=${head.SpritemapPointer:X4}, " +
                    $"OAM={oam.NextByteOffset / 4}, authored={authoredEntryCount}.");
            }
        }

        if (observedHeadMaps.Count != 1 || !observedHeadMaps.Contains(InitialHeadSpritemap) ||
            !state.DrawBrain || state.Form != 0 ||
            state.Function != MotherBrainBodyFunction.FirstPhase || state.DeleteTurretsAndRinkas)
        {
            throw new InvalidDataException(
                $"Mother Brain phase-one loop mutated unexpectedly: maps={observedHeadMaps.Count}, " +
                $"draw={state.DrawBrain}, form={state.Form}, function=$A9:{(ushort)state.Function:X4}.");
        }

        AuditTurretRuntime(bus, assets.LevelData, enemies, samus);

        Console.WriteLine(
            "Mother Brain audit passed: retail room $DD58 loaded six physical records; " +
            "body/head initialization, BG2 clear, two palette slices, corpse-rot seed, " +
            "twelve shared-pool turrets, rotating/firing turret bytecode, bullet movement, " +
            "terrain/contact behavior, looping head bytecode, and the custom ordinary-" +
            "spritemap draw hook matched the untouched cartridge.");
        return 0;
    }

    private static void AuditInitialTurretPool(
        ISnesAddressSpace bus,
        RoomEnemySystem enemies)
    {
        if (enemies.ActiveEnemyProjectileCount != 12)
        {
            throw new InvalidDataException(
                $"Mother Brain allocated {enemies.ActiveEnemyProjectileCount} initial projectiles, not 12.");
        }

        // The encounter is loaded with seed $1234 above. Replaying only the two documented
        // RNG calls per turret proves both consumption order and the two native lower clamps.
        var expectedRandom = new Bank80SystemState(0x1234);
        for (ushort parameter = 0; parameter < 12; parameter++)
        {
            RoomEnemyProjectileSlot turret = enemies.EnemyProjectiles[17 - parameter];
            int offset = parameter * 2;
            ushort direction = ReadWord(bus, 0x86bee1 + offset);
            ushort expectedRotationTimer = Math.Max(
                unchecked((byte)expectedRandom.NextRandom()),
                (byte)0x20);
            ushort expectedCooldownTimer = Math.Max(
                unchecked((byte)expectedRandom.NextRandom()),
                (byte)0x80);
            ushort expectedList = ReadWord(bus, 0x86beb9 + direction * 2);

            if ((ushort)turret.Kind != TurretDefinition ||
                turret.SlotIndex != 17 - parameter ||
                turret.DirectionParameter != parameter ||
                turret.XPosition != ReadWord(bus, 0x86be89 + offset) ||
                turret.YPosition != ReadWord(bus, 0x86bea1 + offset) ||
                turret.XSubposition != ReadWord(bus, 0x86bec9 + offset) ||
                turret.YSubposition != (ushort)(0x0100 | direction) ||
                turret.XVelocity != expectedRotationTimer ||
                turret.YVelocity != expectedCooldownTimer ||
                turret.InstructionPointer != expectedList ||
                turret.InstructionTimer != 1 || turret.PreInstruction != 0xbfdf ||
                turret.GraphicsIndex != 0x0400 || turret.XRadius != 0 || turret.YRadius != 0 ||
                turret.Damage != 0 || turret.CanDamageSamus || !turret.PersistsOnSamusContact ||
                turret.BlocksSamusProjectiles)
            {
                throw new InvalidDataException(
                    $"Mother Brain turret parameter {parameter} diverged: slot={turret.SlotIndex}, " +
                    $"kind=$86:{(ushort)turret.Kind:X4}, pos=({turret.XPosition:X4}," +
                    $"{turret.YPosition:X4}), dir/delta=${turret.YSubposition:X4}, " +
                    $"timers={turret.XVelocity:X4}/{turret.YVelocity:X4}.");
            }
        }

        // Native indexes $00-$0A remain available for at most six simultaneous bullets.
        for (int slot = 0; slot < 6; slot++)
        {
            if (enemies.EnemyProjectiles[slot].IsActive)
                throw new InvalidDataException($"Mother Brain unexpectedly occupied spare projectile slot {slot}.");
        }
    }

    private static void AuditTurretRuntime(
        ISnesAddressSpace bus,
        RoomLevelData level,
        RoomEnemySystem enemies,
        SamusState samus)
    {
        const ushort cameraX = 0x00d0;
        RoomEnemyProjectileSlot? bullet = null;
        for (int frame = 0; frame < 0x0100 && bullet is null; frame++)
        {
            enemies.StepEnemyProjectiles(
                level,
                samus: null,
                cameraX: cameraX,
                cameraY: 0,
                nmiFrameCounter8: unchecked((byte)frame));
            bullet = enemies.EnemyProjectiles.FirstOrDefault(projectile =>
                (ushort)projectile.Kind == TurretBulletDefinition);
        }

        if (bullet is null)
            throw new InvalidDataException("No visible Mother Brain turret fired within one maximum cooldown.");

        ushort direction = bullet.DirectionParameter;
        ushort directionOffset = unchecked((ushort)(direction * 2));
        RoomEnemyProjectileSlot? source = enemies.EnemyProjectiles.FirstOrDefault(projectile =>
            (ushort)projectile.Kind == TurretDefinition &&
            bullet.XPosition == unchecked((ushort)(projectile.XPosition +
                ReadWord(bus, 0x86bf9f + directionOffset))) &&
            bullet.YPosition == unchecked((ushort)(projectile.YPosition +
                ReadWord(bus, 0x86bfaf + directionOffset))));
        if (source is null || bullet.PreInstruction != 0xc0e0 ||
            bullet.InstructionPointer != 0xc131 || bullet.InstructionTimer != 1 ||
            bullet.SpritemapPointer != 0x8000 || bullet.GraphicsIndex != 0x0400 ||
            bullet.XRadius != 3 || bullet.YRadius != 3 || bullet.Damage != 0x0014 ||
            bullet.XVelocity != ReadWord(bus, 0x86bfbf + directionOffset) ||
            bullet.YVelocity != ReadWord(bus, 0x86bfcf + directionOffset) ||
            bullet.Variable0 != directionOffset || bullet.Variable1 != 0 ||
            !bullet.CanDamageSamus || !bullet.PersistsOnSamusContact ||
            bullet.BlocksSamusProjectiles)
        {
            throw new InvalidDataException(
                $"Mother Brain turret bullet initialization diverged: slot={bullet.SlotIndex}, " +
                $"direction={direction}, pos=({bullet.XPosition:X4},{bullet.YPosition:X4}), " +
                $"velocity=({bullet.XVelocity:X4},{bullet.YVelocity:X4}), " +
                $"list=${bullet.InstructionPointer:X4}.");
        }

        int bulletSlot = bullet.SlotIndex;
        (ushort expectedX, ushort expectedXSubposition) = AddEightBitVelocityReference(
            bullet.XPosition,
            bullet.XSubposition,
            bullet.XVelocity);
        (ushort expectedY, ushort expectedYSubposition) = AddEightBitVelocityReference(
            bullet.YPosition,
            bullet.YSubposition,
            bullet.YVelocity);
        enemies.StepEnemyProjectiles(
            level,
            samus: null,
            cameraX: cameraX,
            cameraY: 0,
            nmiFrameCounter8: 0);
        bullet = enemies.EnemyProjectiles[bulletSlot];
        if (!bullet.IsActive || bullet.XPosition != expectedX ||
            bullet.XSubposition != expectedXSubposition || bullet.YPosition != expectedY ||
            bullet.YSubposition != expectedYSubposition || !bullet.BlocksSamusProjectiles ||
            bullet.SpritemapPointer == 0x8000)
        {
            throw new InvalidDataException(
                $"Mother Brain turret bullet movement/flicker diverged in slot {bulletSlot}: " +
                $"active={bullet.IsActive}, pos=({bullet.XPosition:X4}.{bullet.XSubposition:X4}," +
                $"{bullet.YPosition:X4}.{bullet.YSubposition:X4}), block={bullet.BlocksSamusProjectiles}.");
        }

        // Put Samus at the bullet's next 8.8 position. The common collision pass runs after
        // projectile movement, so this checks the exact $4014 persistent-contact behavior:
        // twenty damage, 96 invincibility frames, and a switch to ROM list $C19A.
        (ushort contactX, _) = AddEightBitVelocityReference(
            bullet.XPosition,
            bullet.XSubposition,
            bullet.XVelocity);
        (ushort contactY, _) = AddEightBitVelocityReference(
            bullet.YPosition,
            bullet.YSubposition,
            bullet.YVelocity);
        samus.XPosition = contactX;
        samus.YPosition = contactY;
        samus.Health = 999;
        samus.InvincibilityTimer = 0;
        enemies.StepEnemyProjectiles(
            level,
            samus,
            cameraX: cameraX,
            cameraY: 0,
            nmiFrameCounter8: 1);
        bullet = enemies.EnemyProjectiles[bulletSlot];
        if (!bullet.IsActive || samus.Health != 979 || samus.InvincibilityTimer != 96 ||
            bullet.InstructionPointer != 0xc19a || bullet.InstructionTimer != 1)
        {
            throw new InvalidDataException(
                $"Mother Brain bullet contact diverged: active={bullet.IsActive}, " +
                $"health={samus.Health}, invincibility={samus.InvincibilityTimer}, " +
                $"list=${bullet.InstructionPointer:X4}/{bullet.InstructionTimer}.");
        }

        enemies.StepEnemyProjectiles(
            level,
            samus,
            cameraX: cameraX,
            cameraY: 0,
            nmiFrameCounter8: 2);
        bullet = enemies.EnemyProjectiles[bulletSlot];
        if (!bullet.IsActive || bullet.GraphicsIndex != 0 || bullet.PreInstruction != 0x8170 ||
            bullet.InstructionTimer != 8 ||
            bullet.SpritemapPointer != ReadWord(bus, 0x86c1a0))
        {
            throw new InvalidDataException(
                $"Mother Brain bullet contact animation diverged: active={bullet.IsActive}, " +
                $"gfx=${bullet.GraphicsIndex:X4}, pre=${bullet.PreInstruction:X4}, " +
                $"timer={bullet.InstructionTimer}, map=${bullet.SpritemapPointer:X4}.");
        }

        var projectileOam = new OamBuffer();
        projectileOam.BeginFrame();
        enemies.DrawEnemyProjectiles(projectileOam, cameraX, 0);
        if (projectileOam.NextByteOffset == 0)
            throw new InvalidDataException("Mother Brain's live turret/bullet pool emitted no OAM.");
    }

    private static (ushort Position, ushort Subposition) AddEightBitVelocityReference(
        ushort position,
        ushort subposition,
        ushort velocity)
    {
        int fixedPosition = (position << 16) | subposition;
        fixedPosition = unchecked(fixedPosition + (unchecked((short)velocity) << 8));
        return (unchecked((ushort)(fixedPosition >> 16)), unchecked((ushort)fixedPosition));
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) => unchecked((ushort)(
        bus.ReadByte(address) |
        (bus.ReadByte((address & 0xff0000) | ((address + 1) & 0xffff)) << 8)));
}
