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

        Console.WriteLine(
            "Mother Brain audit passed: retail room $DD58 loaded six physical records; " +
            "body/head initialization, BG2 clear, two palette slices, corpse-rot seed, " +
            "twelve turret requests, looping head bytecode, and the custom ordinary-spritemap " +
            "draw hook matched the untouched cartridge.");
        return 0;
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) => unchecked((ushort)(
        bus.ReadByte(address) |
        (bus.ReadByte((address & 0xff0000) | ((address + 1) & 0xffff)) << 8)));
}
