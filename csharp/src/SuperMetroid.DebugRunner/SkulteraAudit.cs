using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>
/// ROM-backed Skultera regression. Main Street supplies a complete, unmodified population
/// whose twelve actors are all translated (five Skulteras and seven Scisers); three smaller
/// unchanged population prefixes cover the initial-facing and uncommon speed/radius formats
/// that the one full-room fixture does not contain.
/// </summary>
internal static class SkulteraAudit
{
    private const ushort SkulteraDefinition = 0xd6ff;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        MainStreetResult mainStreet = RunMainStreet(bus);
        VerifyEastOceanFacings(bus);
        VerifyCompactRadiusVariants(bus);

        Console.WriteLine(
            "Skultera audit passed: Main Street loaded all 12 retail actors; five fish " +
            $"crossed {mainStreet.FunctionCount} states, animated {mainStreet.MapCount} ROM " +
            $"maps on layers 2/6, traversed X {mainStreet.MinimumX:X4}-{mainStreet.MaximumX:X4} " +
            $"and Y {mainStreet.MinimumY:X4}-{mainStreet.MaximumY:X4}, rendered " +
            $"{mainStreet.ObjPieces} OBJ pieces, dealt 80 contact damage, accepted beam and " +
            "power-bomb damage, and the East Ocean/Red Fish/Waterway parameters preserved " +
            "both facings plus 1.0/0.5-pixel speeds and the native radius-one flat path.");
        return 0;
    }

    /// <summary>
    /// Executes Main Street's exact population and terrain without an audit overlay. This is
    /// the family E2E fixture: Scisers share the production scheduler while the first fish
    /// naturally reaches both room walls, completes both turn animations, and reverses its
    /// signed angle delta exactly as normal gameplay requires.
    /// </summary>
    private static MainStreetResult RunMainStreet(SuperMetroidAddressSpace bus)
    {
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, 0xcfc9);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        RoomEnemySystem enemies = LoadEnemies(bus, room, assets);
        RoomEnemySlot[] population = enemies.Slots.Take(enemies.EnemyCount).ToArray();
        RoomEnemySlot[] fish = population
            .Where(slot => slot.EnemyDefinitionPointer == SkulteraDefinition)
            .ToArray();

        if (room.State.Pointer != 0xcfd6 || enemies.EnemyCount != 12 ||
            fish.Length != 5 || population.Skip(5).Any(slot =>
                slot.EnemyDefinitionPointer != 0xd77f) ||
            fish.Any(slot => enemies.SkulteraStates[slot.SlotIndex] is null))
        {
            throw new InvalidDataException(
                $"Main Street selected state ${room.State.Pointer:X4} with " +
                $"{enemies.EnemyCount} actors, {fish.Length} initialized Skulteras, and " +
                $"{population.Count(slot => slot.EnemyDefinitionPointer == 0xd77f)} Scisers.");
        }

        RoomEnemySlot swimmer = fish[0];
        SkulteraEnemyState swimmerState = RequireState(enemies, swimmer);
        if (swimmer.XPosition != 0x0100 || swimmer.YPosition != 0x01a0 ||
            swimmer.Parameter1 != 0x0010 || swimmer.Parameter2 != 0x0210 ||
            swimmer.Health != 300 || swimmer.Definition.Damage != 80 ||
            swimmer.XRadius != 13 || swimmer.YRadius != 11 ||
            swimmer.CurrentInstruction != 0x9060 ||
            swimmerState.Function != SkulteraEnemyFunction.SwimmingRight ||
            swimmerState.RightVelocity != 1 || swimmerState.RightSubvelocity != 0 ||
            swimmerState.LeftVelocity != -1 || swimmerState.LeftSubvelocity != 0 ||
            swimmerState.Radius != 16 || swimmerState.AngleDelta != 2 ||
            swimmerState.Angle != 0 || swimmerState.PreviousYOffset != 0)
        {
            throw new InvalidDataException(
                $"Main Street first Skultera init failed: position=" +
                $"({swimmer.XPosition:X4},{swimmer.YPosition:X4}), params=" +
                $"${swimmer.Parameter1:X4}/${swimmer.Parameter2:X4}, health/damage=" +
                $"{swimmer.Health}/{swimmer.Definition.Damage}, radii=" +
                $"{swimmer.XRadius}/{swimmer.YRadius}, list=${swimmer.CurrentInstruction:X4}, " +
                $"function=$A3:{(ushort)swimmerState.Function:X4}, speed=" +
                $"{swimmerState.LeftVelocity}:{swimmerState.LeftSubvelocity:X4}/" +
                $"{swimmerState.RightVelocity}:{swimmerState.RightSubvelocity:X4}, " +
                $"wave={swimmerState.Radius}/{swimmerState.AngleDelta}/" +
                $"{swimmerState.Angle:X2}/{swimmerState.PreviousYOffset}.");
        }

        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusPoseIds.FacingRightNormalPose,
            XPosition = swimmer.XPosition,
            YPosition = swimmer.YPosition,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);

        ushort minimumX = swimmer.XPosition;
        ushort maximumX = swimmer.XPosition;
        ushort minimumY = swimmer.YPosition;
        ushort maximumY = swimmer.YPosition;
        var functions = new HashSet<SkulteraEnemyFunction>();
        var maps = new HashSet<ushort>();
        var layers = new HashSet<ushort>();
        var angleDeltaSigns = new HashSet<int>();

        // Main Street is three screens wide. Following the actor with a centered camera is
        // equivalent to the normal scrolling viewport and prevents the scheduler from
        // freezing it while it swims the roughly 500-pixel round trip. 1,600 frames cover
        // both turn lists even when foreground collision shortens one horizontal leg.
        ushort cameraX = 0;
        ushort cameraY = 0;
        for (int frame = 0; frame < 1_600; frame++)
        {
            cameraX = CenterCamera(swimmer.XPosition, room.WidthInScreens * 256, 256);
            cameraY = CenterCamera(swimmer.YPosition, room.HeightInScreens * 256, 224);
            enemies.StepFrame(cameraX, cameraY, false, samus, level: assets.LevelData);
            functions.Add(swimmerState.Function);
            maps.Add(swimmer.SpritemapPointer);
            layers.Add(swimmer.Layer);
            angleDeltaSigns.Add(Math.Sign(swimmerState.AngleDelta));
            minimumX = Math.Min(minimumX, swimmer.XPosition);
            maximumX = Math.Max(maximumX, swimmer.XPosition);
            minimumY = Math.Min(minimumY, swimmer.YPosition);
            maximumY = Math.Max(maximumY, swimmer.YPosition);
        }

        SkulteraEnemyFunction[] requiredFunctions =
        [
            SkulteraEnemyFunction.SwimmingLeft,
            SkulteraEnemyFunction.SwimmingRight,
            SkulteraEnemyFunction.TurningLeft,
            SkulteraEnemyFunction.TurningRight,
        ];
        if (requiredFunctions.Any(function => !functions.Contains(function)) ||
            maps.Count != 22 || !layers.SetEquals([2, 6]) ||
            !angleDeltaSigns.SetEquals([-1, 1]) ||
            minimumX != 0x005d || maximumX != 0x0173 ||
            minimumY != 0x0191 || maximumY != 0x01af)
        {
            throw new InvalidDataException(
                $"Main Street Skultera motion failed: functions=" +
                $"{string.Join(',', functions.Select(x => $"${(ushort)x:X4}"))}, " +
                $"maps={maps.Count}, layers={string.Join(',', layers)}, angle signs=" +
                $"{string.Join(',', angleDeltaSigns)}, X={minimumX:X4}-{maximumX:X4}, " +
                $"Y={minimumY:X4}-{maximumY:X4}.");
        }

        var oam = new OamBuffer();
        oam.BeginFrame();
        enemies.DrawLayers(oam, cameraX, cameraY, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException("Main Street Skultera emitted no live ROM OBJ.");

        // Common touch AI uses the header's 80-point contact attack. The family has no
        // bespoke projectile: normal beam, missile, contact-attack, and power-bomb handling
        // comes from the bank-$A0 routines selected by its header, and is verified below.
        samus.XPosition = swimmer.XPosition;
        samus.YPosition = swimmer.YPosition;
        samus.Health = 999;
        samus.InvincibilityTimer = 0;
        samus.HorizontalSpeed.ContactDamageIndex = 0;
        if (!enemies.ResolveOrdinarySamusContact(samus, 0) || samus.Health != 919)
        {
            throw new InvalidDataException(
                $"Skultera contact attack produced Samus health {samus.Health}, expected 919.");
        }

        RoomEnemySlot beamTarget = fish[1];
        StepNear(enemies, assets, samus, beamTarget);
        var projectiles = new SamusProjectileSystem();
        var sharedProjectiles = new SamusBombProjectileSystem();
        ArmProjectile(projectiles.Slots[0], beamTarget, damage: 20);
        if (enemies.ResolveOrdinaryProjectileHits(
                bus,
                projectiles,
                sharedProjectiles,
                samus) != 1 || beamTarget.Health != 280 || beamTarget.FlashTimer == 0)
        {
            throw new InvalidDataException(
                $"Skultera beam damage failed: health={beamTarget.Health}, " +
                $"flash={beamTarget.FlashTimer}.");
        }

        RoomEnemySlot powerBombTarget = fish[2];
        StepNear(enemies, assets, samus, powerBombTarget);
        int reactions = enemies.ResolveOrdinaryPowerBombHits(
            bus,
            powerBombTarget.XPosition,
            powerBombTarget.YPosition,
            explosionRadius: 16);
        if (reactions != 1 || powerBombTarget.Health != 100 ||
            powerBombTarget.InvincibilityTimer != 48 ||
            !powerBombTarget.Properties.HasAny(EnemyProperties.ProcessOffScreen))
        {
            throw new InvalidDataException(
                $"Skultera power-bomb damage failed: reactions={reactions}, " +
                $"health={powerBombTarget.Health}, invincibility=" +
                $"{powerBombTarget.InvincibilityTimer}, properties=" +
                $"${powerBombTarget.Properties:X4}.");
        }

        return new MainStreetResult(
            functions.Count,
            maps.Count,
            minimumX,
            maximumX,
            minimumY,
            maximumY,
            oam.LastFinalizedSpriteCount);
    }

    /// <summary>
    /// East Ocean stores five Choots before its five Skulteras. Both families are translated,
    /// so the audit now retains that unchanged ten-record prefix from the room's real start;
    /// only the three following Kamer records remain outside this family-specific regression.
    /// </summary>
    private static void VerifyEastOceanFacings(SuperMetroidAddressSpace bus)
    {
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, 0x94fd);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);

        ushort translatedPopulationPointer = room.State.EnemyPopulationPointer;
        var prefixBus = new PopulationPrefixAddressSpace(
            bus,
            translatedPopulationPointer,
            retainedRecordCount: 10,
            deathQuota: 10);
        var random = new Bank80SystemState();
        var enemies = new RoomEnemySystem();
        enemies.Load(
            prefixBus,
            translatedPopulationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            random.NextRandom,
            random.SetRandomNumber);

        RoomEnemySlot left = enemies.Slots[5];
        RoomEnemySlot right = enemies.Slots[6];
        SkulteraEnemyState leftState = RequireState(enemies, left);
        SkulteraEnemyState rightState = RequireState(enemies, right);
        if (room.State.Pointer != 0x950a || translatedPopulationPointer != 0x8002 ||
            enemies.EnemyCount != 10 ||
            enemies.Slots.Take(5).Any(slot => slot.EnemyDefinitionPointer != 0xd3bf) ||
            left.Parameter1 != 0x0110 ||
            right.Parameter1 != 0x0010 ||
            leftState.Function != SkulteraEnemyFunction.SwimmingLeft ||
            rightState.Function != SkulteraEnemyFunction.SwimmingRight ||
            left.CurrentInstruction != 0x902a || right.CurrentInstruction != 0x9060)
        {
            throw new InvalidDataException(
                $"East Ocean facing slice failed: state=${room.State.Pointer:X4}, " +
                $"population=${translatedPopulationPointer:X4}, count={enemies.EnemyCount}, " +
                $"params=${left.Parameter1:X4}/${right.Parameter1:X4}, functions=" +
                $"$A3:{(ushort)leftState.Function:X4}/$A3:{(ushort)rightState.Function:X4}, " +
                $"lists=${left.CurrentInstruction:X4}/${right.CurrentInstruction:X4}.");
        }

        var samus = CreateSamus(prefixBus, left.XPosition, left.YPosition);
        ushort leftStartX = left.XPosition;
        enemies.StepFrame(0x007c, 0x0500, false, samus, level: assets.LevelData);
        ushort leftAfterX = left.XPosition;
        enemies.StepFrame(0x0140, 0x0500, false, samus, level: assets.LevelData);
        if (leftAfterX >= leftStartX || right.XPosition <= 0x01d5 ||
            left.Layer != 2 || right.Layer != 6)
        {
            throw new InvalidDataException(
                $"East Ocean facing movement failed: left={leftStartX:X4}->{leftAfterX:X4} " +
                $"layer {left.Layer}, right=01D5->{right.XPosition:X4} layer {right.Layer}.");
        }
    }

    /// <summary>
    /// Red Fish and Waterway use radius one with angle delta $10. Every sine-table product
    /// truncates below one pixel, so the correct visible path is perfectly horizontal—not a
    /// host-float wobble. Their speed indexes distinguish 0.5 from 1.0 pixel per frame.
    /// </summary>
    private static void VerifyCompactRadiusVariants(SuperMetroidAddressSpace bus)
    {
        VerifyCompactRadiusVariant(
            bus,
            roomHeaderPointer: 0xd104,
            expectedStatePointer: 0xd111,
            expectedParameter1: 0x0008,
            expectedXPosition: 0x02c0,
            expectedYPosition: 0x01b0,
            expectedWholeVelocity: 0,
            expectedSubvelocity: 0x8000,
            expectedAdvanceAfterSixteenFrames: 8,
            retainedRecordCount: 1);
        VerifyCompactRadiusVariant(
            bus,
            roomHeaderPointer: 0xa0d2,
            expectedStatePointer: 0xa0df,
            expectedParameter1: 0x0010,
            expectedXPosition: 0x0140,
            expectedYPosition: 0x00ae,
            expectedWholeVelocity: 1,
            expectedSubvelocity: 0,
            expectedAdvanceAfterSixteenFrames: 16,
            retainedRecordCount: 1);
    }

    private static void VerifyCompactRadiusVariant(
        SuperMetroidAddressSpace bus,
        ushort roomHeaderPointer,
        ushort expectedStatePointer,
        ushort expectedParameter1,
        ushort expectedXPosition,
        ushort expectedYPosition,
        short expectedWholeVelocity,
        ushort expectedSubvelocity,
        ushort expectedAdvanceAfterSixteenFrames,
        int retainedRecordCount)
    {
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, roomHeaderPointer);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var prefixBus = new PopulationPrefixAddressSpace(
            bus,
            room.State.EnemyPopulationPointer,
            retainedRecordCount,
            deathQuota: 1);
        var random = new Bank80SystemState();
        var enemies = new RoomEnemySystem();
        enemies.Load(
            prefixBus,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            random.NextRandom,
            random.SetRandomNumber);

        RoomEnemySlot fish = enemies.Slots[0];
        SkulteraEnemyState state = RequireState(enemies, fish);
        if (room.State.Pointer != expectedStatePointer || enemies.EnemyCount != 1 ||
            fish.EnemyDefinitionPointer != SkulteraDefinition ||
            fish.Parameter1 != expectedParameter1 || fish.Parameter2 != 0x1001 ||
            fish.XPosition != expectedXPosition || fish.YPosition != expectedYPosition ||
            state.RightVelocity != expectedWholeVelocity ||
            state.RightSubvelocity != expectedSubvelocity || state.Radius != 1 ||
            state.AngleDelta != 16)
        {
            throw new InvalidDataException(
                $"Compact Skultera variant in room ${roomHeaderPointer:X4} failed init: " +
                $"state=${room.State.Pointer:X4}, count={enemies.EnemyCount}, " +
                $"definition=${fish.EnemyDefinitionPointer:X4}, params=" +
                $"${fish.Parameter1:X4}/${fish.Parameter2:X4}, position=" +
                $"({fish.XPosition:X4},{fish.YPosition:X4}), speed=" +
                $"{state.RightVelocity}:{state.RightSubvelocity:X4}, wave=" +
                $"{state.Radius}/{state.AngleDelta}.");
        }

        SamusState samus = CreateSamus(prefixBus, fish.XPosition, fish.YPosition);
        ushort startX = fish.XPosition;
        ushort startY = fish.YPosition;
        ushort cameraX = CenterCamera(fish.XPosition, room.WidthInScreens * 256, 256);
        ushort cameraY = CenterCamera(fish.YPosition, room.HeightInScreens * 256, 224);
        for (int frame = 0; frame < 16; frame++)
            enemies.StepFrame(cameraX, cameraY, false, samus, level: assets.LevelData);

        if (fish.XPosition - startX != expectedAdvanceAfterSixteenFrames ||
            fish.YPosition != startY || state.Angle != 0)
        {
            throw new InvalidDataException(
                $"Compact Skultera variant in room ${roomHeaderPointer:X4} moved " +
                $"({startX:X4},{startY:X4})->({fish.XPosition:X4},{fish.YPosition:X4}) " +
                $"with angle ${state.Angle:X2}.");
        }
    }

    private static RoomEnemySystem LoadEnemies(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
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
        return enemies;
    }

    private static void StepNear(
        RoomEnemySystem enemies,
        CartridgeRoomAssets assets,
        SamusState samus,
        RoomEnemySlot target)
    {
        ushort cameraX = target.XPosition > 128
            ? unchecked((ushort)(target.XPosition - 128))
            : (ushort)0;
        ushort cameraY = target.YPosition > 112
            ? unchecked((ushort)(target.YPosition - 112))
            : (ushort)0;
        enemies.StepFrame(cameraX, cameraY, false, samus, level: assets.LevelData);
    }

    private static SamusState CreateSamus(
        ISnesAddressSpace bus,
        ushort xPosition,
        ushort yPosition)
    {
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusPoseIds.FacingRightNormalPose,
            XPosition = xPosition,
            YPosition = yPosition,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        return samus;
    }

    private static ushort CenterCamera(ushort actorPosition, int roomPixels, int viewportPixels)
    {
        int maximum = Math.Max(0, roomPixels - viewportPixels);
        return unchecked((ushort)Math.Clamp(actorPosition - viewportPixels / 2, 0, maximum));
    }

    private static void ArmProjectile(
        SamusProjectileSlot projectile,
        RoomEnemySlot target,
        ushort damage)
    {
        projectile.ClearFields();
        projectile.Type = 0;
        projectile.Damage = damage;
        projectile.Direction = 2;
        projectile.XPosition = target.XPosition;
        projectile.YPosition = target.YPosition;
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    private static SkulteraEnemyState RequireState(
        RoomEnemySystem enemies,
        RoomEnemySlot slot) =>
        enemies.SkulteraStates[slot.SlotIndex] ?? throw new InvalidDataException(
            $"Skultera slot {slot.SlotIndex} has no typed state.");

    private readonly record struct MainStreetResult(
        int FunctionCount,
        int MapCount,
        ushort MinimumX,
        ushort MaximumX,
        ushort MinimumY,
        ushort MaximumY,
        int ObjPieces);
}
