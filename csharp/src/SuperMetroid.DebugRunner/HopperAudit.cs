using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Cartridge-backed regression for the shared Sidehopper/Dessgeega state machine. Blue
/// Hopper is deliberately used because its retail population contains only two Tourian
/// hoppers; unrelated untranslated actors therefore cannot make this family appear healthy.
/// </summary>
internal static class HopperAudit
{
    private const ushort BlueHopperRoomHeader = 0xdc19;
    private const ushort BlueHopperDefaultState = 0xdc2b;
    private const ushort TourianSidehopperDefinition = 0xd9ff;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, BlueHopperRoomHeader);
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

        if (room.State.Pointer != BlueHopperDefaultState || enemies.EnemyCount != 2 ||
            enemies.Slots.Take(2).Any(
                slot => slot.EnemyDefinitionPointer != TourianSidehopperDefinition) ||
            enemies.HopperStates.Take(2).Any(state => state is null))
        {
            throw new InvalidDataException(
                $"Blue Hopper selected state ${room.State.Pointer:X4} with " +
                $"{enemies.EnemyCount} non-uniform or uninitialized actors.");
        }

        RoomEnemySlot auditedSlot = enemies.Slots[0];
        HopperEnemyState auditedState = enemies.HopperStates[0]
            ?? throw new InvalidDataException("Blue Hopper slot zero has no typed hopper state.");
        if (auditedSlot.XPosition != 0x00f8 || auditedSlot.YPosition != 0x0061 ||
            auditedSlot.Health != 1500 || auditedSlot.Definition.Damage != 120 ||
            auditedState.UpsideDown || auditedState.HopTableIndex != 2 ||
            auditedState.VariantTableOffset != 2 ||
            auditedState.Function != HopperEnemyFunction.ChooseHopSize ||
            auditedState.InstalledInstructionList != 0xb0d1)
        {
            throw new InvalidDataException(
                "Tourian hopper disagrees with its retail population/header/list data: " +
                $"position=(${auditedSlot.XPosition:X4},${auditedSlot.YPosition:X4}), " +
                $"health/damage={auditedSlot.Health}/{auditedSlot.Definition.Damage}, " +
                $"upsideDown={auditedState.UpsideDown}, physics={auditedState.HopTableIndex}, " +
                $"variant={auditedState.VariantTableOffset}, " +
                $"function=$A3:{(ushort)auditedState.Function:X4}, " +
                $"list=$A3:{auditedState.InstalledInstructionList:X4}.");
        }

        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusState.FacingRightNormalPose,
            XPosition = 0x0080,
            YPosition = 0x0080,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);

        ushort startX = auditedSlot.XPosition;
        ushort startY = auditedSlot.YPosition;
        ushort minimumY = startY;
        var functions = new HashSet<HopperEnemyFunction>();
        var maps = new HashSet<ushort>();
        var sounds = new HashSet<ushort>();
        bool sawFalling = false;
        bool sawReturnToLandedWait = false;
        for (int frame = 0; frame < 600; frame++)
        {
            enemies.StepFrame(0, 0, false, samus, level: assets.LevelData);
            functions.Add(auditedState.Function);
            maps.Add(auditedSlot.SpritemapPointer);
            minimumY = Math.Min(minimumY, auditedSlot.YPosition);
            sawFalling |= auditedState.Falling;
            sawReturnToLandedWait |= frame > 20 &&
                auditedState.Function == HopperEnemyFunction.WaitToHop;
            if (enemies.LastHopperSoundEffect is ushort sound)
                sounds.Add(sound);
        }

        if (!functions.Contains(HopperEnemyFunction.PrepareSmallHop) ||
            !functions.Contains(HopperEnemyFunction.PrepareBigHop) ||
            !functions.Contains(HopperEnemyFunction.JumpingUpsideUpBackward) ||
            !sawFalling || !sawReturnToLandedWait || minimumY >= startY ||
            auditedSlot.XPosition == startX || maps.Count < 3 ||
            !sounds.SetEquals([0x005d, 0x005e]))
        {
            throw new InvalidDataException(
                $"Tourian hopper cycle failed: functions={string.Join(',', functions.Select(x => $"${(ushort)x:X4}"))}, " +
                $"falling={sawFalling}, landed={sawReturnToLandedWait}, " +
                $"position=({startX},{startY})->({auditedSlot.XPosition},{auditedSlot.YPosition}), " +
                $"minY={minimumY}, maps={maps.Count}, sounds={string.Join(',', sounds.Select(x => $"${x:X2}"))}.");
        }

        var oam = new OamBuffer();
        oam.BeginFrame();
        enemies.DrawLayers(oam, 0, 0, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException("Blue Hopper's live ROM spritemap emitted no enemy OBJ.");

        // The Tourian definition has its own restrictive projectile vulnerability table,
        // but touch remains the common $A0:8023 path and must apply the header's 120 damage.
        enemies.StepFrame(0, 0, false, samus, level: assets.LevelData);
        samus.XPosition = auditedSlot.XPosition;
        samus.YPosition = auditedSlot.YPosition;
        samus.Health = 999;
        samus.InvincibilityTimer = 0;
        if (!enemies.ResolveOrdinarySamusContact(samus, 0) || samus.Health != 879 ||
            !samus.KnockbackActive)
        {
            throw new InvalidDataException(
                $"Tourian hopper common contact failed: health={samus.Health}, " +
                $"knockback={samus.KnockbackActive}.");
        }

        Console.WriteLine(
            "Blue Hopper audit passed: two retail Tourian Sidehoppers loaded, both random " +
            $"hop sizes traversed the ROM quadratic arc and landing loop, {maps.Count} maps " +
            $"animated, 120 contact damage resolved, and {oam.LastFinalizedSpriteCount} OBJ pieces rendered.");
        return 0;
    }
}
