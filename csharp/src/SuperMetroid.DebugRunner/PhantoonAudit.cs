using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>
/// Untouched-ROM encounter audit for Phantoon's retail Wrecked Ship room. This checkpoint
/// covers the complete load and fight introduction through the first figure-eight round;
/// later combat/death slices extend this same audit instead of replacing it with fixtures.
/// </summary>
internal static class PhantoonAudit
{
    private const ushort RoomPointer = 0xcd13;
    private const ushort PopulationPointer = 0xccd4;
    private static readonly ushort[] ExpectedDefinitions = [0xe4bf, 0xe4ff, 0xe53f, 0xe57f];

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, RoomPointer);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        if (room.WidthInScreens != 1 || room.HeightInScreens != 1 ||
            room.AreaIndex != 3 || room.State.EnemyPopulationPointer != PopulationPointer)
        {
            throw new InvalidDataException(
                $"Phantoon room mismatch: {room.WidthInScreens}x{room.HeightInScreens}, " +
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
            XPosition = 128,
            YPosition = 192,
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
            setAreaBossDefeated: () => throw new InvalidDataException(
                "Phantoon intro unexpectedly persisted boss defeat."));

        PhantoonEnemyState state = enemies.Phantoon ??
            throw new InvalidDataException("Phantoon room did not allocate typed encounter state.");
        if (enemies.EnemyCount != 4 || state.Eye is null ||
            state.Tentacles is null || state.Mouth is null)
        {
            throw new InvalidDataException("Phantoon's four physical records were not linked.");
        }
        for (int slot = 0; slot < ExpectedDefinitions.Length; slot++)
        {
            if (enemies.Slots[slot].EnemyDefinitionPointer != ExpectedDefinitions[slot])
            {
                throw new InvalidDataException(
                    $"Phantoon slot {slot} loaded ${enemies.Slots[slot].EnemyDefinitionPointer:X4}, " +
                    $"expected ${ExpectedDefinitions[slot]:X4}.");
            }
        }

        RoomEnemySlot body = state.Body;
        if (body.XPosition != 128 || body.YPosition != 96 || body.Health != 2500 ||
            body.CurrentInstruction != 0xcc41 || state.Eye.CurrentInstruction != 0xcc7b ||
            state.Tentacles.CurrentInstruction != 0xccd7 || state.Mouth.CurrentInstruction != 0xccf7 ||
            body.VariableE != 0x0060 ||
            body.VariableF != (ushort)PhantoonAiFunction.SpawnStartingFlames ||
            state.Mouth.VariableC != 0xffff || !state.BackgroundTilemapPrepared ||
            state.Bg2TilemapSize != 0x0360 || vram.ReadWord(0x4800) != 0x0338 ||
            vram.ReadWord(0x4fff) != 0x0338)
        {
            throw new InvalidDataException(
                $"Phantoon initialization mismatch: body=({body.XPosition},{body.YPosition}) " +
                $"hp={body.Health}, list=$A7:{body.CurrentInstruction:X4}, " +
                $"function=$A7:{body.VariableF:X4}, timer={body.VariableE}, " +
                $"mouth pattern=${state.Mouth.VariableC:X4}, BG={state.BackgroundTilemapPrepared}.");
        }

        var functions = new HashSet<PhantoonAiFunction>();
        var tentacleMaps = new HashSet<ushort>();
        var startingFlameSlots = new HashSet<int>();
        var movedStartingFlames = new HashSet<int>();
        var previousFlamePositions = new Dictionary<int, (ushort X, ushort Y)>();
        ushort initialBodyX = body.XPosition;
        ushort initialBodyY = body.YPosition;
        int firstRoundFrames = 0;
        int frame;
        for (frame = 0; frame < 1400; frame++)
        {
            byte nmi = unchecked((byte)frame);
            enemies.StepFrame(
                cameraX: 0,
                cameraY: 0,
                timeIsFrozen: false,
                samus,
                level: assets.LevelData,
                nmiFrameCounter8: nmi);
            tentacleMaps.Add(state.Tentacles.SpritemapPointer);
            functions.Add((PhantoonAiFunction)body.VariableF);

            foreach (RoomEnemyProjectileSlot projectile in enemies.EnemyProjectiles)
            {
                if (projectile.Kind != RoomEnemyProjectileKind.PhantoonStartingFlame)
                    continue;
                startingFlameSlots.Add(projectile.SlotIndex);
                if (previousFlamePositions.TryGetValue(
                        projectile.SlotIndex,
                        out (ushort X, ushort Y) previous) &&
                    (previous.X != projectile.XPosition || previous.Y != projectile.YPosition))
                {
                    movedStartingFlames.Add(projectile.SlotIndex);
                }
                previousFlamePositions[projectile.SlotIndex] =
                    (projectile.XPosition, projectile.YPosition);
            }

            enemies.StepEnemyProjectiles(
                assets.LevelData,
                samus,
                cameraX: 0,
                cameraY: 0,
                nmiFrameCounter8: nmi);

            // Extended body/eye/tentacle/mouth maps are the actual renderer producer. Run
            // the draw pass during the audit so a malformed BG2 stream cannot hide behind
            // state-only assertions.
            enemies.DrawLayers(new OamBuffer(), 0, 0, firstLayer: 0, lastLayer: 7);

            if (body.VariableF == (ushort)PhantoonAiFunction.MoveInFigureEightThenOpenEye)
            {
                firstRoundFrames++;
                if (firstRoundFrames >= 32)
                    break;
            }
        }

        int liveStartingFlames = enemies.EnemyProjectiles.Count(
            projectile => projectile.Kind == RoomEnemyProjectileKind.PhantoonStartingFlame);
        bool paletteMatchesFullHealth = true;
        for (int color = 0; color < 16; color++)
        {
            ushort expected = (ushort)(bus.ReadByte(0xa7cc21 + color * 2) |
                (bus.ReadByte(0xa7cc22 + color * 2) << 8));
            paletteMatchesFullHealth &= cgram.Colors[112 + color] == expected;
        }

        if (body.VariableF != (ushort)PhantoonAiFunction.MoveInFigureEightThenOpenEye ||
            firstRoundFrames < 32 || functions.Count < 5 ||
            state.StartingFlameRequests != 8 || state.StartingFlamesSpawned != 8 ||
            startingFlameSlots.Count != 8 || movedStartingFlames.Count != 8 ||
            liveStartingFlames != 0 || state.BossDoorPlmRequest != 0xb781 ||
            state.MusicRequest != 5 || state.Mouth.Parameter1 != 1 ||
            !paletteMatchesFullHealth || tentacleMaps.Count != 3 ||
            (body.XPosition == initialBodyX && body.YPosition == initialBodyY))
        {
            throw new InvalidDataException(
                $"Phantoon intro mismatch after {frame} frames: function=$A7:{body.VariableF:X4}, " +
                $"round frames={firstRoundFrames}, functions={functions.Count}, flames=" +
                $"{state.StartingFlamesSpawned}/{state.StartingFlameRequests} " +
                $"slots/moved/live={startingFlameSlots.Count}/{movedStartingFlames.Count}/{liveStartingFlames}, " +
                $"door={state.BossDoorPlmRequest}, music={state.MusicRequest}, " +
                $"mouth control=${state.Mouth.Parameter1:X4}, palette={paletteMatchesFullHealth}, " +
                $"tentacle maps={tentacleMaps.Count}, body=({body.XPosition},{body.YPosition}).");
        }

        Console.WriteLine(
            $"Phantoon audit passed through the first figure-eight round after {frame} frames: " +
            "retail 1x1 room/four-part population, cleared BG2 surface, independent body/eye/" +
            "tentacle/mouth lists, eight physical starting flames, activation/orbit contraction, " +
            "health palette materialization, delayed music, and ROM movement table.");
        return 0;
    }
}
