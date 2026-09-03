using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;

/// <summary>Retail Pre-Bowling regression for the Wrecked Ship orange Zoomer.</summary>
internal static class HZoomerAudit
{
    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, 0x968f);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState();
        var enemies = new RoomEnemySystem();
        enemies.Load(
            bus,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            random.NextRandom,
            random.SetRandomNumber);

        RoomEnemySlot slot = enemies.Slots[0];
        CrawlerEnemyState state = enemies.CrawlerStates[0]
            ?? throw new InvalidDataException("Pre-Bowling HZoomer has no crawler state.");
        if (room.State.Pointer != 0x969c || enemies.EnemyCount != 1 ||
            slot.EnemyDefinitionPointer != 0xdc3f || slot.XPosition != 0x0080 ||
            slot.YPosition != 0x0048 || state.XVelocity != 0x0080 ||
            state.YVelocity != 0x0080 ||
            state.Function != CrawlerEnemyFunction.HZoomerInstructionPending)
        {
            throw new InvalidDataException(
                $"Pre-Bowling HZoomer init failed: state=${room.State.Pointer:X4}, " +
                $"count={enemies.EnemyCount}, definition=${slot.EnemyDefinitionPointer:X4}, " +
                $"position=({slot.XPosition},{slot.YPosition}), velocity=" +
                $"${state.XVelocity:X4}/${state.YVelocity:X4}, " +
                $"function=$A3:{(ushort)state.Function:X4}.");
        }

        var samus = new SamusState
        {
            Health = 99,
            MaxHealth = 99,
            Pose = SamusPoseIds.FacingRightNormalPose,
            XPosition = 0x0040,
            YPosition = 0x0080,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);

        var maps = new HashSet<ushort>();
        ushort startX = slot.XPosition;
        enemies.StepFrame(0, 0, false, samus, level: assets.LevelData);
        if (state.Function != CrawlerEnemyFunction.HZoomerCrawlingHorizontally)
        {
            throw new InvalidDataException(
                $"HZoomer instruction dispatch selected $A3:{(ushort)state.Function:X4}.");
        }
        for (int frame = 0; frame < 90; frame++)
        {
            enemies.StepFrame(0, 0, false, samus, level: assets.LevelData);
            maps.Add(slot.SpritemapPointer);
        }
        if (maps.Count != 5 || slot.XPosition >= startX || unchecked((short)state.XVelocity) >= 0)
        {
            throw new InvalidDataException(
                $"HZoomer homing crawl failed: maps={maps.Count}, X={startX}->{slot.XPosition}, " +
                $"velocity=${state.XVelocity:X4}.");
        }

        enemies.EarthquakeTimer = 0x001e;
        enemies.EarthquakeType = 0x0014;
        enemies.StepFrame(0, 0, false, samus, level: assets.LevelData);
        if (state.Function != CrawlerEnemyFunction.Falling ||
            state.NonFallingFunction != CrawlerEnemyFunction.HZoomerCrawlingHorizontally)
        {
            throw new InvalidDataException(
                $"HZoomer earthquake did not enter shared fall: function=" +
                $"$A3:{(ushort)state.Function:X4}, return=$A3:{(ushort)state.NonFallingFunction:X4}.");
        }
        enemies.EarthquakeTimer = 0;
        enemies.EarthquakeType = 0;

        var oam = new OamBuffer();
        oam.BeginFrame();
        enemies.DrawLayers(oam, 0, 0, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException("Pre-Bowling HZoomer emitted no live ROM OBJ.");

        enemies.StepFrame(0, 0, false, samus, level: assets.LevelData);
        samus.XPosition = slot.XPosition;
        samus.YPosition = slot.YPosition;
        samus.Health = 99;
        samus.InvincibilityTimer = 0;
        if (!enemies.ResolveOrdinarySamusContact(samus, 0) || samus.Health != 94 ||
            !samus.KnockbackActive)
        {
            throw new InvalidDataException(
                $"HZoomer contact failed: health={samus.Health}, knockback={samus.KnockbackActive}.");
        }

        Console.WriteLine(
            "Pre-Bowling HZoomer audit passed: the retail actor homed along its surface, " +
            $"animated five maps, entered shared falling on earthquake $14/$1E, dealt five " +
            $"contact damage, and rendered {oam.LastFinalizedSpriteCount} OBJ pieces.");
        return 0;
    }
}
