using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>
/// ROM-backed Choot regression. Bowling Alley Path is a complete unmodified population of
/// three Choots and three already-translated Wavers. A Pseudo Plasma Spark prefix supplies
/// the other retail falling patterns and nonzero delay formats without modifying any actor.
/// </summary>
internal static class ChootAudit
{
    private const ushort ChootDefinition = 0xd3bf;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        VerifyAllPatternTables(bus);
        BowlingResult bowling = RunBowlingAlleyPath(bus);
        VerifyPseudoPlasmaVariants(bus);

        Console.WriteLine(
            "Choot audit passed: Bowling Alley Path loaded three retail Choots beside three " +
            $"Wavers; proximity/delay/ascent/{bowling.FallingFrames}-frame authored descent " +
            $"crossed {bowling.FunctionCount} states and {bowling.MapCount} ROM maps, ranged " +
            $"X {bowling.MinimumX:X4}-{bowling.MaximumX:X4} / Y " +
            $"{bowling.MinimumY:X4}-{bowling.MaximumY:X4}, rendered {bowling.ObjPieces} OBJ " +
            "pieces, dealt 80 contact damage, accepted beam/power-bomb damage, and Pseudo " +
            "Plasma verified retail patterns 0/1/2 plus the exact 17-frame delay-$10 gate.");
        return 0;
    }

    private static BowlingResult RunBowlingAlleyPath(SuperMetroidAddressSpace bus)
    {
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, 0x9461);
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

        RoomEnemySlot[] population = enemies.Slots.Take(enemies.EnemyCount).ToArray();
        RoomEnemySlot[] choots = population.Take(3).ToArray();
        if (room.State.Pointer != 0x946e || enemies.EnemyCount != 6 ||
            choots.Any(slot => slot.EnemyDefinitionPointer != ChootDefinition) ||
            population.Skip(3).Any(slot => slot.EnemyDefinitionPointer != 0xd63f) ||
            choots.Any(slot => enemies.ChootStates[slot.SlotIndex] is null))
        {
            throw new InvalidDataException(
                $"Bowling Alley Path selected state ${room.State.Pointer:X4} with " +
                $"{enemies.EnemyCount} actors, " +
                $"{population.Count(slot => slot.EnemyDefinitionPointer == ChootDefinition)} " +
                "initialized Choots, and " +
                $"{population.Count(slot => slot.EnemyDefinitionPointer == 0xd63f)} Wavers.");
        }

        RoomEnemySlot actor = choots[0];
        ChootEnemyState state = RequireState(enemies, actor);
        if (actor.XPosition != 0x0070 || actor.YPosition != 0x00cc ||
            actor.Parameter1 != 0x0204 || actor.Parameter2 != 0 ||
            actor.Health != 100 || actor.Definition.Damage != 80 ||
            actor.XRadius != 16 || actor.YRadius != 5 ||
            actor.CurrentInstruction != 0xd82c ||
            state.Function != ChootEnemyFunction.WaitingForSamus ||
            state.FallingPatternPointer != 0xdaa0 ||
            state.FallingPatternYDistance != 0x0020 ||
            state.SpawnXPosition != 0x0070 || state.SpawnYPosition != 0x00cc ||
            state.InitialFallingXPosition != 0x0070 ||
            state.InitialFallingYPosition != 0x004c ||
            state.InitialYSpeedTableIndex != 0x4a00 ||
            state.YSpeedTableIndex != state.InitialYSpeedTableIndex)
        {
            throw new InvalidDataException(
                $"Bowling Choot init failed: position=({actor.XPosition:X4}," +
                $"{actor.YPosition:X4}), params=${actor.Parameter1:X4}/" +
                $"${actor.Parameter2:X4}, health/damage={actor.Health}/" +
                $"{actor.Definition.Damage}, radii={actor.XRadius}/{actor.YRadius}, " +
                $"list=${actor.CurrentInstruction:X4}, function=" +
                $"$A2:{(ushort)state.Function:X4}, pattern=" +
                $"${state.FallingPatternPointer:X4}/{state.FallingPatternYDistance}, " +
                $"spawn=({state.SpawnXPosition:X4},{state.SpawnYPosition:X4}), apex=" +
                $"({state.InitialFallingXPosition:X4},{state.InitialFallingYPosition:X4}), " +
                $"speed=${state.InitialYSpeedTableIndex:X4}/" +
                $"${state.YSpeedTableIndex:X4}.");
        }

        SamusState samus = CreateSamus(bus, 0x0200, actor.YPosition);
        var maps = new HashSet<ushort>();

        // A far-away frame runs the idle list's common disable-off-screen command and
        // installs its sole map without activating the 80-pixel horizontal proximity gate.
        enemies.StepFrame(0, 0, false, samus, level: assets.LevelData);
        maps.Add(actor.SpritemapPointer);
        if (state.Function != ChootEnemyFunction.WaitingForSamus ||
            actor.SpritemapPointer != 0xe146 ||
            actor.Properties.HasAny(EnemyProperties.ProcessOffScreen))
        {
            throw new InvalidDataException(
                $"Choot idle failed: function=$A2:{(ushort)state.Function:X4}, " +
                $"map=${actor.SpritemapPointer:X4}, properties=${actor.Properties:X4}.");
        }

        ushort initialSpeed = state.InitialYSpeedTableIndex;
        ushort minimumX = actor.XPosition;
        ushort maximumX = actor.XPosition;
        ushort minimumY = actor.YPosition;
        ushort maximumY = actor.YPosition;
        int fallingFrames = 0;
        var functions = new HashSet<ChootEnemyFunction>();
        var fallingOrigins = new HashSet<ushort>();

        samus.XPosition = actor.XPosition;
        enemies.StepFrame(0, 0, false, samus, level: assets.LevelData);
        functions.Add(state.Function);
        if (state.Function != ChootEnemyFunction.PreparingJump || state.JumpDelayTimer != 0)
        {
            throw new InvalidDataException(
                $"Choot proximity gate selected $A2:{(ushort)state.Function:X4} with " +
                $"delay ${state.JumpDelayTimer:X4}.");
        }

        bool sawFalling = false;
        bool completedCycle = false;
        for (int frame = 0; frame < 700; frame++)
        {
            enemies.StepFrame(0, 0, false, samus, level: assets.LevelData);
            functions.Add(state.Function);
            maps.Add(actor.SpritemapPointer);
            minimumX = Math.Min(minimumX, actor.XPosition);
            maximumX = Math.Max(maximumX, actor.XPosition);
            minimumY = Math.Min(minimumY, actor.YPosition);
            maximumY = Math.Max(maximumY, actor.YPosition);
            if (state.Function == ChootEnemyFunction.Falling)
            {
                sawFalling = true;
                fallingFrames++;
                fallingOrigins.Add(state.FallingYOrigin);
            }
            else if (sawFalling && state.Function == ChootEnemyFunction.WaitingForSamus)
            {
                completedCycle = true;
                break;
            }
        }

        ChootEnemyFunction[] requiredFunctions =
        [
            ChootEnemyFunction.PreparingJump,
            ChootEnemyFunction.Jumping,
            ChootEnemyFunction.Falling,
            ChootEnemyFunction.WaitingForSamus,
        ];
        if (!completedCycle || requiredFunctions.Any(function => !functions.Contains(function)) ||
            maps.Count != 4 || actor.XPosition != state.SpawnXPosition ||
            actor.YPosition != state.SpawnYPosition || actor.XSubposition != 0 ||
            actor.YSubposition != 0 || state.YSpeedTableIndex != initialSpeed ||
            state.FallingYOrigin != 0x00cc ||
            !fallingOrigins.SetEquals([0x004c, 0x006c, 0x008c, 0x00ac]))
        {
            throw new InvalidDataException(
                $"Choot jump cycle failed: completed={completedCycle}, functions=" +
                $"{string.Join(',', functions.Select(x => $"${(ushort)x:X4}"))}, " +
                $"maps={maps.Count}, position=({actor.XPosition:X4},{actor.YPosition:X4})." +
                $"({actor.XSubposition:X4},{actor.YSubposition:X4}), speed=" +
                $"${state.YSpeedTableIndex:X4}/${initialSpeed:X4}, origins=" +
                $"{string.Join(',', fallingOrigins.Select(x => $"${x:X4}"))}.");
        }

        var oam = new OamBuffer();
        oam.BeginFrame();
        enemies.DrawLayers(oam, 0, 0, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException("Bowling Alley Choot emitted no live ROM OBJ.");

        // A second retail actor supplies normal contact and beam fixtures while the first
        // remains intact for the complete-cycle assertions above.
        RoomEnemySlot contactTarget = choots[1];
        samus.XPosition = contactTarget.XPosition;
        samus.YPosition = contactTarget.YPosition;
        samus.Health = 999;
        samus.InvincibilityTimer = 0;
        samus.HorizontalSpeed.ContactDamageIndex = 0;
        enemies.StepFrame(0, 0, false, samus, level: assets.LevelData);
        if (!enemies.ResolveOrdinarySamusContact(samus, 0) || samus.Health != 919)
        {
            throw new InvalidDataException(
                $"Choot contact attack produced Samus health {samus.Health}, expected 919.");
        }

        var projectiles = new SamusProjectileSystem();
        var sharedProjectiles = new SamusBombProjectileSystem();
        ArmProjectile(projectiles.Slots[0], contactTarget, damage: 20);
        if (enemies.ResolveOrdinaryProjectileHits(
                bus,
                projectiles,
                sharedProjectiles,
                samus) != 1 || contactTarget.Health != 80 ||
            contactTarget.FlashTimer == 0)
        {
            throw new InvalidDataException(
                $"Choot beam damage failed: health={contactTarget.Health}, " +
                $"flash={contactTarget.FlashTimer}.");
        }

        RoomEnemySlot powerBombTarget = choots[2];
        samus.XPosition = powerBombTarget.XPosition;
        samus.YPosition = powerBombTarget.YPosition;
        enemies.StepFrame(0x0100, 0, false, samus, level: assets.LevelData);
        int reactions = enemies.ResolveOrdinaryPowerBombHits(
            bus,
            powerBombTarget.XPosition,
            powerBombTarget.YPosition,
            explosionRadius: 16);
        if (reactions != 1 || powerBombTarget.Health != 0 ||
            !powerBombTarget.Properties.HasAny(
                EnemyProperties.Deleted | EnemyProperties.ProcessOffScreen))
        {
            throw new InvalidDataException(
                $"Choot power-bomb damage failed: reactions={reactions}, health=" +
                $"{powerBombTarget.Health}, properties=${powerBombTarget.Properties:X4}.");
        }

        return new BowlingResult(
            functions.Count,
            maps.Count,
            fallingFrames,
            minimumX,
            maximumX,
            minimumY,
            maximumY,
            oam.LastFinalizedSpriteCount);
    }

    private static void VerifyPseudoPlasmaVariants(SuperMetroidAddressSpace bus)
    {
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, 0xd1dd);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);

        // The unchanged records immediately after Pseudo Plasma's leading Owtch are five
        // Choots. Their parameters cover pattern 2, pattern 0 twice, pattern 1, and delays
        // $02/$04/$10/$08/$04; the next record is a different translated family.
        ushort firstChootPointer = unchecked((ushort)(
            room.State.EnemyPopulationPointer + 16));
        var prefixBus = new PopulationPrefixAddressSpace(
            bus,
            firstChootPointer,
            retainedRecordCount: 5,
            deathQuota: 5);
        var random = new Bank80SystemState();
        var enemies = new RoomEnemySystem();
        enemies.Load(
            prefixBus,
            firstChootPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            random.NextRandom,
            random.SetRandomNumber);

        ushort[] expectedParameter1 = [0x0203, 0x0004, 0x0102, 0x0002, 0x0002];
        ushort[] expectedParameter2 = [0x0002, 0x0004, 0x0010, 0x0008, 0x0004];
        ushort[] expectedPointers = [0xdaa0, 0xd84c, 0xd976, 0xd84c, 0xd84c];
        ushort[] expectedDistances = [0x20, 0x1e, 0x1c, 0x1e, 0x1e];
        for (int index = 0; index < 5; index++)
        {
            RoomEnemySlot slot = enemies.Slots[index];
            ChootEnemyState state = RequireState(enemies, slot);
            if (slot.EnemyDefinitionPointer != ChootDefinition ||
                slot.Parameter1 != expectedParameter1[index] ||
                slot.Parameter2 != expectedParameter2[index] ||
                state.FallingPatternPointer != expectedPointers[index] ||
                state.FallingPatternYDistance != expectedDistances[index])
            {
                throw new InvalidDataException(
                    $"Pseudo Plasma Choot {index} failed: definition=" +
                    $"${slot.EnemyDefinitionPointer:X4}, params=${slot.Parameter1:X4}/" +
                    $"${slot.Parameter2:X4}, pattern=${state.FallingPatternPointer:X4}/" +
                    $"{state.FallingPatternYDistance}.");
            }
        }

        // Delay $10 requires sixteen non-launching decrements and launches only on the
        // seventeenth preparation frame when the wrapped word becomes $FFFF.
        RoomEnemySlot delayed = enemies.Slots[2];
        ChootEnemyState delayedState = RequireState(enemies, delayed);
        SamusState samus = CreateSamus(prefixBus, delayed.XPosition, delayed.YPosition);
        ushort cameraX = CenterCamera(delayed.XPosition, room.WidthInScreens * 256, 256);
        ushort cameraY = CenterCamera(delayed.YPosition, room.HeightInScreens * 256, 224);
        enemies.StepFrame(cameraX, cameraY, false, samus, level: assets.LevelData);
        if (delayedState.Function != ChootEnemyFunction.PreparingJump ||
            delayedState.JumpDelayTimer != 0x0010)
        {
            throw new InvalidDataException("Pseudo Plasma delay-$10 Choot did not arm.");
        }
        for (int frame = 0; frame < 16; frame++)
            enemies.StepFrame(cameraX, cameraY, false, samus, level: assets.LevelData);
        if (delayedState.Function != ChootEnemyFunction.PreparingJump ||
            delayedState.JumpDelayTimer != 0)
        {
            throw new InvalidDataException(
                $"Choot delay-$10 launched early: function=" +
                $"$A2:{(ushort)delayedState.Function:X4}, timer=" +
                $"${delayedState.JumpDelayTimer:X4}.");
        }
        enemies.StepFrame(cameraX, cameraY, false, samus, level: assets.LevelData);
        if (delayedState.Function != ChootEnemyFunction.Jumping ||
            delayed.CurrentInstruction != 0xd83a)
        {
            // ProcessInstructions consumes the enable-off-screen opcode and first frame in
            // this same StepFrame, leaving CurrentInstruction at the word after map $E15C.
            throw new InvalidDataException(
                $"Choot delay-$10 failed to launch: function=" +
                $"$A2:{(ushort)delayedState.Function:X4}, cursor=" +
                $"${delayed.CurrentInstruction:X4}.");
        }

        // Run the pattern-one actor through its full two-loop return. This proves its ROM
        // stream beyond merely checking that initialization selected the expected pointer.
        bool sawFalling = false;
        bool completed = false;
        ushort spawnX = delayedState.SpawnXPosition;
        ushort spawnY = delayedState.SpawnYPosition;
        for (int frame = 0; frame < 500; frame++)
        {
            enemies.StepFrame(cameraX, cameraY, false, samus, level: assets.LevelData);
            sawFalling |= delayedState.Function == ChootEnemyFunction.Falling;
            if (sawFalling && delayedState.Function == ChootEnemyFunction.WaitingForSamus)
            {
                completed = true;
                break;
            }
        }
        if (!completed || delayed.XPosition != spawnX || delayed.YPosition != spawnY ||
            delayed.XSubposition != 0 || delayed.YSubposition != 0)
        {
            throw new InvalidDataException(
                $"Pattern-one Choot did not reset: completed={completed}, position=" +
                $"({delayed.XPosition:X4},{delayed.YPosition:X4})." +
                $"({delayed.XSubposition:X4},{delayed.YSubposition:X4}).");
        }
    }

    /// <summary>
    /// Verifies the complete five-entry source tables, including slow patterns three/four
    /// that no retail population selects. The production AI still supports them because
    /// they are genuine, terminated ROM streams rather than host-authored extrapolations.
    /// </summary>
    private static void VerifyAllPatternTables(ISnesAddressSpace bus)
    {
        ushort[] expectedPatterns = [0xd84c, 0xd976, 0xdaa0, 0xdbca, 0xdd44];
        ushort[] expectedDistancePointers = [0xd974, 0xda9e, 0xdbc8, 0xdd42, 0xdf5c];
        ushort[] expectedDistances = [0x001e, 0x001c, 0x0020, 0x001e, 0x001e];
        for (int index = 0; index < expectedPatterns.Length; index++)
        {
            ushort pattern = ReadWord(bus, 0xa2df5e + index * 2);
            ushort distancePointer = ReadWord(bus, 0xa2df6a + index * 2);
            ushort distance = ReadWord(bus, 0xa20000 | distancePointer);
            if (pattern != expectedPatterns[index] ||
                distancePointer != expectedDistancePointers[index] ||
                distance != expectedDistances[index])
            {
                throw new InvalidDataException(
                    $"Choot pattern table {index} is ${pattern:X4}/" +
                    $"${distancePointer:X4}/{distance}, expected " +
                    $"${expectedPatterns[index]:X4}/" +
                    $"${expectedDistancePointers[index]:X4}/{expectedDistances[index]}.");
            }
        }
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
            Pose = SamusState.FacingRightNormalPose,
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

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8));

    private static ChootEnemyState RequireState(
        RoomEnemySystem enemies,
        RoomEnemySlot slot) =>
        enemies.ChootStates[slot.SlotIndex] ?? throw new InvalidDataException(
            $"Choot slot {slot.SlotIndex} has no typed state.");

    private readonly record struct BowlingResult(
        int FunctionCount,
        int MapCount,
        int FallingFrames,
        ushort MinimumX,
        ushort MaximumX,
        ushort MinimumY,
        ushort MaximumY,
        int ObjPieces);
}
