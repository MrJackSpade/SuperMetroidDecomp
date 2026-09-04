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
    private static class RoomDefinitions
    {
        /// <summary>Room <c>$01/$02</c> header at <c>$8F:9B9D</c>.</summary>
        public const ushort CrateriaCeilingSidehopper = 0x9b9d;

        /// <summary>Ceiling Sidehopper's first landed spritemap at <c>$A3:AF34</c>.</summary>
        public const ushort CeilingSidehopperLandedSpritemap = 0xaf34;
    }

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

        // Blue Hopper contains one parameter-$8000 ceiling actor followed by one floor
        // actor. Exercise the latter here so the long-standing upward-hop assertions retain
        // their original meaning; room $01/$02 below covers the reported ceiling variant.
        RoomEnemySlot auditedSlot = enemies.Slots[1];
        HopperEnemyState auditedState = enemies.HopperStates[1]
            ?? throw new InvalidDataException("Blue Hopper slot one has no typed hopper state.");
        if (auditedSlot.XPosition != 0x0086 || auditedSlot.YPosition != 0x00a9 ||
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
            Pose = SamusPoseIds.FacingRightNormalPose,
            XPosition = 0x0080,
            YPosition = 0x0080,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);

        VerifyBlueHopperFloorOrientation(bus, enemies, assets, auditedSlot, auditedState);

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

        VerifyRoom0102CeilingOrientation(bus);

        Console.WriteLine(
            "Blue Hopper audit passed: two retail Tourian Sidehoppers loaded, both random " +
            $"hop sizes traversed the ROM quadratic arc and landing loop, {maps.Count} maps " +
            $"animated, 120 contact damage resolved, and {oam.LastFinalizedSpriteCount} OBJ pieces rendered.");
        return 0;
    }

    /// <summary>
    /// Reproduces issue #248 against the actual floor actor in Blue Hopper. The ceiling
    /// and floor actors share one draw queue, so use the floor actor's live cartridge
    /// spritemap count and native population order to isolate its trailing OAM records.
    /// This asserts the pixels' vertical-flip attributes rather than merely trusting the
    /// already-correct typed <see cref="HopperEnemyState.UpsideDown"/> selector.
    /// </summary>
    private static void VerifyBlueHopperFloorOrientation(
        SuperMetroidAddressSpace bus,
        RoomEnemySystem enemies,
        CartridgeRoomAssets assets,
        RoomEnemySlot floorHopper,
        HopperEnemyState floorState)
    {
        enemies.StepFrame(0, 0, false, samus: null, level: assets.LevelData);
        int pieceCount = bus.ReadByte(0xa30000 | floorHopper.SpritemapPointer) |
            (bus.ReadByte(0xa30000 | unchecked((ushort)(floorHopper.SpritemapPointer + 1))) << 8);
        if (pieceCount <= 0)
        {
            throw new InvalidDataException(
                $"Blue Hopper floor map $A3:{floorHopper.SpritemapPointer:X4} has no OBJ pieces.");
        }

        var oam = new OamBuffer();
        oam.BeginFrame();
        enemies.DrawLayers(oam, 0, 0, 0, 7);
        oam.FinalizeFrame();
        OamEntry[] floorPieces = Enumerable.Range(0, oam.LastFinalizedSpriteCount)
            .Select(oam.GetEntry)
            .TakeLast(pieceCount)
            .ToArray();
        if (floorState.UpsideDown || floorPieces.Length != pieceCount ||
            floorPieces.Any(piece => piece.FlipY))
        {
            throw new InvalidDataException(
                $"Blue Hopper floor actor emitted {floorPieces.Length}/{pieceCount} OBJ pieces " +
                $"with vertical flips [{string.Join(',', floorPieces.Select(piece => piece.FlipY))}].");
        }
    }

    private static void VerifyRoom0102CeilingOrientation(SuperMetroidAddressSpace bus)
    {
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(
            bus,
            RoomDefinitions.CrateriaCeilingSidehopper);
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

        RoomEnemySlot ceilingHopper = enemies.Slots.Single(
            slot => slot.EnemyDefinitionPointer == RoomEnemySystem.SidehopperDefinition);
        HopperEnemyState state = enemies.HopperStates[ceilingHopper.SlotIndex]
            ?? throw new InvalidDataException("Room $01/$02 Sidehopper has no typed hopper state.");
        if (ceilingHopper.Parameter1 != 1 || !state.UpsideDown)
        {
            throw new InvalidDataException(
                $"Room $01/$02 ceiling selector mismatch: parameter1=${ceilingHopper.Parameter1:X4}, " +
                $"upsideDown={state.UpsideDown}.");
        }

        const ushort cameraX = 0x0180;
        enemies.StepFrame(cameraX, 0, false, samus: null, level: assets.LevelData);
        if (ceilingHopper.SpritemapPointer != RoomDefinitions.CeilingSidehopperLandedSpritemap)
        {
            throw new InvalidDataException(
                $"Room $01/$02 ceiling Sidehopper selected map $A3:{ceilingHopper.SpritemapPointer:X4}, " +
                $"not cartridge ceiling map $A3:{RoomDefinitions.CeilingSidehopperLandedSpritemap:X4}.");
        }

        var oam = new OamBuffer();
        oam.BeginFrame();
        enemies.DrawLayers(oam, cameraX, 0, 0, 7);
        oam.FinalizeFrame();
        OamEntry[] pieces = Enumerable.Range(0, oam.LastFinalizedSpriteCount)
            .Select(oam.GetEntry)
            .ToArray();
        OamEntry[] hopperPieces = pieces.Take(5).ToArray();
        if (hopperPieces.Length != 5 || hopperPieces.Any(piece => !piece.FlipY))
        {
            throw new InvalidDataException(
                $"Room $01/$02 ceiling Sidehopper emitted {hopperPieces.Length} leading OBJ " +
                $"pieces with vertical flips [{string.Join(',', hopperPieces.Select(piece => piece.FlipY))}].");
        }
    }
}
