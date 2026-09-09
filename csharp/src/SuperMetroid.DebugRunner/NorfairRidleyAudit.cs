using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>
/// ROM-backed audit for Lower Norfair Ridley. It starts from the retail room and population
/// so reveal bytecode, extended maps, palette tables, and the $E976 combat handoff all come
/// from the cartridge rather than a synthetic boss fixture.
/// </summary>
internal static partial class NorfairRidleyAudit
{
    private const ushort RoomPointer = 0xb32e;
    private const ushort PopulationPointer = 0xa626;
    private const ushort CameraX = 0;
    private const ushort CameraY = 256;

    public static int Run(string romPath, string? deathAudioTracePath = null)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, RoomPointer);
        if (room.AreaIndex != AreaId.Norfair || room.WidthInScreens != 1 || room.HeightInScreens != 2 ||
            room.State.EnemyPopulationPointer != PopulationPointer)
        {
            throw new InvalidDataException(
                $"Ridley room mismatch: area={room.AreaIndex}, size=" +
                $"{room.WidthInScreens}x{room.HeightInScreens}, state=${room.State.Pointer:X4}, " +
                $"population=${room.State.EnemyPopulationPointer:X4}.");
        }

        RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(
            bus,
            RoomEnemySystem.NorfairRidleyDefinition);
        if (definition.Bank != 0xa6 || definition.Health != 18000 || definition.Damage != 160 ||
            definition.InitializationAiPointer != 0xa0f5 || definition.MainAiPointer != 0xb227 ||
            definition.HurtAiPointer != 0xb297 || definition.TimeFrozenAiPointer != 0xb28a ||
            definition.PowerBombReactionPointer != 0xdfb2 || definition.ShotAiPointer != 0xdf8a)
        {
            throw new InvalidDataException(
                $"Ridley header mismatch: bank=${definition.Bank:X2}, hp/damage=" +
                $"{definition.Health}/{definition.Damage}, init/main/hurt/time=" +
                $"${definition.InitializationAiPointer:X4}/${definition.MainAiPointer:X4}/" +
                $"${definition.HurtAiPointer:X4}/${definition.TimeFrozenAiPointer:X4}, " +
                $"shot/pb=${definition.ShotAiPointer:X4}/${definition.PowerBombReactionPointer:X4}.");
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
            XPosition = 128,
            YPosition = 352,
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
            isAreaBossDefeated: () => false,
            cameraX: CameraX,
            cameraY: CameraY);

        RoomEnemySlot body = enemies.Slots[0];
        RidleyEnemyState state = enemies.Ridley ??
            throw new InvalidDataException("Live Ridley room did not allocate shared typed state.");
        if (enemies.EnemyCount != 1 || body.EnemyDefinitionPointer != 0xe17f ||
            body.XPosition != 96 || body.YPosition != 394 || body.Layer != 5 ||
            state.Function != RidleyAiFunction.WaitForDoorTransition ||
            state.FacingDirection != 2 || state.TailDamage != 120 ||
            state.TailSegments.Length != 7)
        {
            throw new InvalidDataException(
                $"Ridley initialization mismatch: count={enemies.EnemyCount}, " +
                $"definition=${body.EnemyDefinitionPointer:X4}, position=" +
                $"({body.XPosition},{body.YPosition}), layer={body.Layer}, " +
                $"function={state.Function}, facing={state.FacingDirection}, " +
                $"tail={state.TailDamage}/{state.TailSegments.Length}.");
        }

        bool sawEyeFade = false;
        bool sawBodyFade = false;
        bool sawArenaHandoff = false;
        int frame;
        for (frame = 0; frame < 1600; frame++)
        {
            enemies.StepFrame(
                CameraX,
                CameraY,
                timeIsFrozen: false,
                samus,
                level: assets.LevelData);
            sawEyeFade |= state.Function == RidleyAiFunction.FadeInEyes;
            sawBodyFade |= state.Function == RidleyAiFunction.FadeInBody;
            sawArenaHandoff |= state.Function == RidleyAiFunction.NorfairEnterArena;
            if (state.Function is not (
                    RidleyAiFunction.WaitForDoorTransition or
                    RidleyAiFunction.InitialDelay or
                    RidleyAiFunction.FadeInEyes or
                    RidleyAiFunction.FadeInBody or
                    RidleyAiFunction.WaitBeforeRoar or
                    RidleyAiFunction.WaitBeforeLiftoff or
                    RidleyAiFunction.ClearVelocity or
                    RidleyAiFunction.NorfairEnterArena))
            {
                break;
            }
        }

        if (!sawEyeFade || !sawBodyFade || !sawArenaHandoff || body.Layer != 2)
        {
            throw new InvalidDataException(
                $"Ridley reveal did not reach combat: frames={frame}, eyes/body/handoff=" +
                $"{sawEyeFade}/{sawBodyFade}/{sawArenaHandoff}, function={state.Function}, " +
                $"layer={body.Layer}, position=({body.XPosition},{body.YPosition}).");
        }

        var combatFunctions = new HashSet<RidleyAiFunction>();
        bool sawCombatMovement = false;
        ushort previousX = body.XPosition;
        ushort previousY = body.YPosition;
        for (int combatFrame = 0; combatFrame < 4096; combatFrame++)
        {
            enemies.StepFrame(
                CameraX,
                CameraY,
                timeIsFrozen: false,
                samus,
                level: assets.LevelData);
            combatFunctions.Add(state.Function);
            sawCombatMovement |= body.XPosition != previousX || body.YPosition != previousY;
            previousX = body.XPosition;
            previousY = body.YPosition;
        }
        if (!sawCombatMovement || combatFunctions.Count < 4)
        {
            throw new InvalidDataException(
                $"Ridley combat did not cycle: movement={sawCombatMovement}, " +
                $"functions={string.Join(',', combatFunctions)}.");
        }

        // The room/reveal pass above proves that cartridge population and main AI reach a
        // live fight. Damage and death are deliberately exercised on a fresh encounter in
        // the companion partial file: that prevents 4096 random combat frames (and their
        // accumulated fireballs) from making collision/death assertions order-dependent.
        using var deathAudioTrace = deathAudioTracePath == null ? null :
            new StreamWriter(new FileStream(deathAudioTracePath, FileMode.CreateNew, FileAccess.Write));
        deathAudioTrace?.WriteLine("frame,phase,kind,library,command,queueLimitOrDelayFrames");
        RidleyBattleAuditResult battle = VerifyCombatDamageAndDeath(bus, room, assets, deathAudioTrace);

        var defeated = new RoomEnemySystem();
        defeated.Load(
            bus,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            new SnesVram(),
            new SnesCgram(),
            random.NextRandom,
            isAreaBossDefeated: () => true);
        if (!defeated.Slots[0].Properties.HasAny(EnemyProperties.Deleted) ||
            defeated.Ridley is not null)
        {
            throw new InvalidDataException("Defeated Ridley did not take the native early-delete branch.");
        }

        Console.WriteLine(
            $"Lower Norfair Ridley audit passed through death in {frame} reveal frames: " +
            $"retail room/header/population, boss-bit deletion, reveal palettes, layer-five " +
            $"entrance, seven-part tail, wing/body bytecode, and {combatFunctions.Count} " +
            $"live combat states across 4096 frames; body/tail contact, tail armor, retail " +
            $"beam/normal-bomb immunity, missile/Power-Bomb damage, zero-health grab, " +
            $"{battle.BreakupActorCount} breakup actors, " +
            $"and persisted boss defeat across {battle.DeathFrames} death frames.");
        return 0;
    }
}
