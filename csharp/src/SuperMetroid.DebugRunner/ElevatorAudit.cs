using System.Globalization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>
/// ROM-backed end-to-end audit for normal elevator enemy $D73F. It inventories every named
/// retail placement, verifies literal initialization and animation data, then exercises both
/// departure directions, the door-transition movement gate, projectile clearing, Samus
/// ownership, and both destination-room return paths.
/// </summary>
internal static class ElevatorAudit
{
    private const ushort ElevatorDefinition = 0xd73f;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace retailBus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        List<RoomEnemyPopulationRecord> records = ReadEveryNamedRetailRecord(retailBus);
        if (records.Count != 15)
        {
            throw new InvalidDataException(
                $"Elevator $D73F appeared in {records.Count}, not 15, named retail records.");
        }

        VerifyDefinition(retailBus);
        VerifySamusDisplayCadence(retailBus);
        VerifyEveryRetailInitialization(retailBus, records);

        RoomEnemyPopulationRecord down = records.First(record => record.Parameter1 == 0);
        RoomEnemyPopulationRecord up = records.First(record => record.Parameter1 == 1);
        AnimationResult animation = VerifyAnimationAndInertCombat(retailBus, down);
        DepartureResult downDeparture = VerifyDeparture(retailBus, down, movesUp: false);
        DepartureResult upDeparture = VerifyDeparture(retailBus, up, movesUp: true);
        int downReturnFrames = VerifyArrivalReturn(retailBus, down, approachesFromBelow: true);
        int upReturnFrames = VerifyArrivalReturn(retailBus, up, approachesFromBelow: false);

        Console.WriteLine(
            "Elevator audit passed: all 15 named retail records initialized; two ROM " +
            $"animation maps emitted {animation.SpriteCount} OBJ pieces; down/up departure " +
            $"moved {downDeparture.DeltaFixed:X8}/{upDeparture.DeltaFixed:X8}, pinned and " +
            "locked Samus, cleared projectile state, honored the transition gate, and " +
            $"completed destination returns in {downReturnFrames}/{upReturnFrames} frames " +
            "with inert touch/shot behavior and exact sound/event publication.");
        return 0;
    }

    private static void VerifyDefinition(ISnesAddressSpace bus)
    {
        RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(bus, ElevatorDefinition);
        if (definition.Bank != 0xa3 || definition.Health != 40 || definition.Damage != 15 ||
            definition.InitializationAiPointer != 0x94e6 || definition.MainAiPointer != 0x952a ||
            definition.TouchAiPointer != 0x804c || definition.ShotAiPointer != 0x804c ||
            definition.PowerBombReactionPointer != 0x804c || definition.Layer != 5)
        {
            throw new InvalidDataException(
                $"Elevator definition mismatch: bank=${definition.Bank:X2}, health/damage=" +
                $"{definition.Health}/{definition.Damage}, radii={definition.XRadius}/" +
                $"{definition.YRadius}, init/main=${definition.InitializationAiPointer:X4}/" +
                $"${definition.MainAiPointer:X4}, touch/shot/PB=" +
                $"${definition.TouchAiPointer:X4}/${definition.ShotAiPointer:X4}/" +
                $"${definition.PowerBombReactionPointer:X4}, layer={definition.Layer}.");
        }

        ushort downInput = ReadWord(bus, 0xa394e2);
        ushort upInput = ReadWord(bus, 0xa394e4);
        if (downInput != (ushort)SnesButton.Down || upInput != (ushort)SnesButton.Up)
        {
            throw new InvalidDataException(
                $"Elevator input table is ${downInput:X4}/${upInput:X4}, expected Down/Up.");
        }
    }

    /// <summary>
    /// Pins the translated predicate to the two machine-code decisions at $90:EC14:
    /// load the NMI counter, test bit zero, and return when it is set. This is the visible
    /// half-rate flicker reported over RDP; it is authored behavior, not dropped host frames.
    /// </summary>
    private static void VerifySamusDisplayCadence(SuperMetroidAddressSpace bus)
    {
        // LDA $05B6; BIT #$0001; BEQ +1; RTS. The following byte begins the fatal/no-
        // animation renderer that the even branch deliberately falls through into.
        byte[] expected = [0xad, 0xb6, 0x05, 0x89, 0x01, 0x00, 0xf0, 0x01, 0x60];
        for (int index = 0; index < expected.Length; index++)
        {
            byte actual = bus.ReadByte(0x90ec14 + index);
            if (actual != expected[index])
            {
                throw new InvalidDataException(
                    $"Elevator Samus display opcode {index} is ${actual:X2}; " +
                    $"retail $90:EC14 requires ${expected[index]:X2}.");
            }
        }

        for (ushort frame = 0; frame < 16; frame++)
        {
            bool expectedDraw = (frame & 1) == 0;
            if (SuperMetroid.Core.Runtime.SuperMetroidRuntime.ShouldDrawSamusOnElevator(frame) !=
                expectedDraw)
            {
                throw new InvalidDataException(
                    $"Elevator Samus draw cadence disagreed with $90:EC14 at NMI {frame}.");
            }
        }
    }

    private static void VerifyEveryRetailInitialization(
        ISnesAddressSpace retailBus,
        List<RoomEnemyPopulationRecord> records)
    {
        LoadedElevator loaded = LoadSelected(retailBus, records);
        if (loaded.Enemies.EnemyCount != records.Count)
            throw new InvalidDataException("Selected elevator population did not preserve all records.");

        for (int index = 0; index < records.Count; index++)
        {
            RoomEnemyPopulationRecord population = records[index];
            RoomEnemySlot slot = loaded.Enemies.Slots[index];
            ElevatorEnemyState state = loaded.Enemies.ElevatorStates[index]
                ?? throw new InvalidDataException($"Elevator record {index} omitted typed state.");
            if (population.Parameter1 > 1 ||
                slot.Spawn.Population != population ||
                slot.EnemyDefinitionPointer != ElevatorDefinition ||
                slot.XPosition != population.XPosition || slot.YPosition != population.YPosition ||
                slot.Parameter1 != unchecked((ushort)(population.Parameter1 * 2)) ||
                slot.Parameter2 != population.Parameter2 ||
                state.RestingYPosition != population.YPosition ||
                state.DirectionTableByteOffset != slot.Parameter1 ||
                slot.CurrentInstruction != 0x94d6 || slot.InstructionTimer != 1 ||
                slot.Timer != 0 || slot.SpritemapPointer != 0x804d)
            {
                throw new InvalidDataException(
                    $"Elevator retail record {index} init mismatch at " +
                    $"({population.XPosition:X4},{population.YPosition:X4}): init=" +
                    $"${population.Parameter1:X4}->${slot.Parameter1:X4}, " +
                    $"destination=${slot.Parameter2:X4}, rest=${state.RestingYPosition:X4}, " +
                    $"list/map=${slot.CurrentInstruction:X4}/${slot.SpritemapPointer:X4}.");
            }
        }
    }

    private static AnimationResult VerifyAnimationAndInertCombat(
        ISnesAddressSpace retailBus,
        RoomEnemyPopulationRecord population)
    {
        LoadedElevator loaded = LoadSelected(retailBus, new[] { population });
        RoomEnemySlot slot = loaded.Enemies.Slots[0];
        var maps = new HashSet<ushort>();
        for (int frame = 0; frame < 8; frame++)
        {
            Step(loaded, newlyPressedInput: 0);
            maps.Add(slot.SpritemapPointer);
        }
        if (!maps.SetEquals(new ushort[] { 0x962f, 0x9645 }))
        {
            throw new InvalidDataException(
                $"Elevator animation produced [{string.Join(",", maps.Select(map => map.ToString("X4")))}].");
        }

        var oam = new OamBuffer();
        oam.BeginFrame();
        loaded.Enemies.DrawLayers(
            oam,
            CameraX(slot),
            CameraY(slot),
            0,
            7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException("Elevator animation emitted no live ROM OBJ.");

        // Retail property $0400 excludes this controller actor from ordinary body, beam,
        // and power-bomb collision. Its header callbacks are literal RTL routines; health
        // and Samus energy must remain unchanged even when test origins overlap exactly.
        loaded.Samus.XPosition = slot.XPosition;
        loaded.Samus.YPosition = slot.YPosition;
        ushort health = loaded.Samus.Health;
        if (loaded.Enemies.ResolveOrdinarySamusContact(loaded.Samus, 0) ||
            loaded.Samus.Health != health)
        {
            throw new InvalidDataException("Elevator unexpectedly entered ordinary touch damage.");
        }

        ArmProjectile(loaded.Projectiles.Slots[0], slot);
        var bombs = new SamusBombProjectileSystem();
        if (loaded.Enemies.ResolveOrdinaryProjectileHits(
                loaded.Bus,
                loaded.Projectiles,
                bombs,
                loaded.Samus) != 0 ||
            slot.Health != slot.Definition.Health)
        {
            throw new InvalidDataException("Elevator unexpectedly admitted ordinary shot damage.");
        }
        return new AnimationResult(maps.Count, oam.LastFinalizedSpriteCount);
    }

    private static DepartureResult VerifyDeparture(
        ISnesAddressSpace retailBus,
        RoomEnemyPopulationRecord population,
        bool movesUp)
    {
        LoadedElevator loaded = LoadSelected(retailBus, new[] { population });
        RoomEnemySlot slot = loaded.Enemies.Slots[0];
        ushort requiredInput = ReadWord(retailBus, 0xa394e2 + slot.Parameter1);
        ushort wrongInput = movesUp ? (ushort)SnesButton.Down : (ushort)SnesButton.Up;

        loaded.Enemies.PublishElevatorDoorContact();
        Step(loaded, wrongInput);
        if (loaded.Enemies.ElevatorFlags != 0 ||
            loaded.Enemies.ElevatorStatus != ElevatorActorStatus.Inactive)
        {
            throw new InvalidDataException("Elevator failed to clear an unaccepted door contact.");
        }

        // A live projectile proves ResetProjectileData was executed at the exact accepted
        // input boundary rather than being approximated later by room loading.
        ArmProjectile(loaded.Projectiles.Slots[0], slot);
        loaded.Enemies.PublishElevatorDoorContact();
        Step(loaded, requiredInput);
        if (loaded.Enemies.ElevatorStatus != ElevatorActorStatus.Departing ||
            loaded.Enemies.LastElevatorEvent != ElevatorFrameEvent.DepartureStarted ||
            loaded.Enemies.LastElevatorSoundEffectLibrary1 != 0x0032 ||
            loaded.Enemies.LastElevatorSoundEffectLibrary3 != 0x000b ||
            !loaded.Enemies.ElevatorClearedProjectileData ||
            loaded.Projectiles.Slots[0].IsActive || !loaded.Samus.InputLocked ||
            !SamusState.IsForwardFacingPose(loaded.Samus.Pose) ||
            loaded.Samus.XPosition != slot.XPosition ||
            loaded.Samus.YPosition != unchecked((ushort)(slot.YPosition - 26)))
        {
            throw new InvalidDataException(
                $"Elevator {(movesUp ? "up" : "down")} activation mismatch: status=" +
                $"{loaded.Enemies.ElevatorStatus}, event={loaded.Enemies.LastElevatorEvent}, " +
                $"sounds={loaded.Enemies.LastElevatorSoundEffectLibrary1:X4}/" +
                $"{loaded.Enemies.LastElevatorSoundEffectLibrary3:X4}, locked=" +
                $"{loaded.Samus.InputLocked}, pose=${loaded.Samus.Pose:X2}.");
        }

        uint before = FixedY(slot);
        Step(loaded, newlyPressedInput: 0);
        uint after = FixedY(slot);
        uint expected = unchecked(before + (uint)(movesUp ? -0x00018000 : 0x00018000));
        ushort expectedDirection = movesUp ? (ushort)0x8000 : (ushort)0;
        if (after != expected || loaded.Enemies.ElevatorDirection != expectedDirection ||
            loaded.Samus.XPosition != slot.XPosition ||
            loaded.Samus.YPosition != unchecked((ushort)(slot.YPosition - 26)) ||
            loaded.Samus.Kinematics.YSpeed != 0 || loaded.Samus.Kinematics.YSubspeed != 0)
        {
            throw new InvalidDataException(
                $"Elevator {(movesUp ? "up" : "down")} movement mismatch: " +
                $"${before:X8}->${after:X8}/${expected:X8}, direction=" +
                $"${loaded.Enemies.ElevatorDirection:X4}.");
        }

        loaded.Enemies.ElevatorDoorTransitionActive = true;
        Step(loaded, newlyPressedInput: 0);
        if (FixedY(slot) != after)
            throw new InvalidDataException("Elevator moved while the door-transition gate was active.");

        // Continue the actual departing elevator with held Fire. Its command-seven
        // forward-facing pose must bypass weapon production on every travel frame.
        loaded.Enemies.ElevatorDoorTransitionActive = false;
        loaded.Samus.EquippedBeams = (ushort)SamusBeamFlags.Charge;
        var air = new RoomLevelData(16, 16, new ushort[256], new byte[256], new ushort[256], []);
        var bombs = new SamusBombProjectileSystem();
        for (int tick = 0; tick < 90; tick++)
        {
            Step(loaded, 0);
            loaded.Projectiles.StepFrame(loaded.Bus, air, loaded.Samus,
                (ushort)SnesButton.X, tick == 0 ? (ushort)SnesButton.X : (ushort)0,
                CameraX(slot), CameraY(slot), bombs);
            if (loaded.Projectiles.FlareCounter != 0 || loaded.Projectiles.Slots.Any(projectile => projectile.IsActive))
                throw new InvalidDataException("Held Fire charged/fired while the real elevator actor was travelling.");
        }
        if (FixedY(slot) == after || !loaded.Samus.InputLocked)
            throw new InvalidDataException("Elevator charge fixture did not keep moving with locked Samus.");
        return new DepartureResult(unchecked(after - before));
    }

    private static int VerifyArrivalReturn(
        ISnesAddressSpace retailBus,
        RoomEnemyPopulationRecord template,
        bool approachesFromBelow)
    {
        ushort restingY = 0x0180;
        ushort arrivalY = unchecked((ushort)(
            restingY + (approachesFromBelow ? 48 : -48)));
        var population = new RoomEnemyPopulationRecord(
            ElevatorDefinition,
            XPosition: 0x0100,
            YPosition: restingY,
            InitializationParameter: template.InitializationParameter,
            Properties: template.Properties,
            ExtraProperties: template.ExtraProperties,
            Parameter1: approachesFromBelow ? (ushort)0 : (ushort)1,
            Parameter2: arrivalY);
        LoadedElevator loaded = LoadSelected(retailBus, new[] { population }, prepareArrival: true);
        RoomEnemySlot slot = loaded.Enemies.Slots[0];
        if (loaded.Enemies.ElevatorStatus != ElevatorActorStatus.BeginArrivalReturn ||
            slot.YPosition != arrivalY || loaded.Samus.XPosition != slot.XPosition ||
            loaded.Samus.YPosition != unchecked((ushort)(arrivalY - 26)))
        {
            throw new InvalidDataException("Elevator arrival initializer did not apply parameter-2 placement.");
        }

        loaded.Samus.InputLocked = true;
        int frames = 0;
        for (; frames < 128; frames++)
        {
            Step(loaded, newlyPressedInput: 0);
            if (loaded.Enemies.ElevatorStatus == ElevatorActorStatus.Inactive)
                break;
        }
        frames++;
        // The upward return uses an unsigned >= comparison and therefore advances one
        // additional 1.5-pixel step at equality. Native then snaps only the whole Y word,
        // deliberately retaining the resulting $8000 subposition. The downward return
        // completes at equality and retains zero.
        ushort expectedSubposition = approachesFromBelow ? (ushort)0x8000 : (ushort)0;
        if (frames >= 128 || slot.YPosition != restingY ||
            slot.YSubposition != expectedSubposition ||
            loaded.Enemies.ElevatorFlags != 0 || loaded.Samus.InputLocked ||
            loaded.Enemies.LastElevatorEvent != ElevatorFrameEvent.ArrivalCompleted ||
            loaded.Enemies.LastElevatorSoundEffectLibrary3 != 0x0025 ||
            loaded.Samus.XPosition != slot.XPosition ||
            loaded.Samus.YPosition != unchecked((ushort)(restingY - 26)))
        {
            throw new InvalidDataException(
                $"Elevator arrival return failed after {frames} frames: status=" +
                $"{loaded.Enemies.ElevatorStatus}, flags=${loaded.Enemies.ElevatorFlags:X4}, " +
                $"Y=${slot.YPosition:X4}:{slot.YSubposition:X4}, event=" +
                $"{loaded.Enemies.LastElevatorEvent}, sound=" +
                $"{loaded.Enemies.LastElevatorSoundEffectLibrary3?.ToString("X4") ?? "none"}.");
        }
        return frames;
    }

    private static List<RoomEnemyPopulationRecord> ReadEveryNamedRetailRecord(
        ISnesAddressSpace bus)
    {
        string symbolPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "upstream-sm",
            "assets",
            "names.txt");
        if (!File.Exists(symbolPath))
            throw new FileNotFoundException("Elevator audit requires upstream-sm/assets/names.txt.", symbolPath);

        var records = new List<RoomEnemyPopulationRecord>();
        foreach (ushort populationPointer in File.ReadLines(symbolPath)
            .Where(line => line.StartsWith("0xa1", StringComparison.OrdinalIgnoreCase) &&
                line.Contains(" kEnemyPopulation_", StringComparison.Ordinal))
            .Select(ParseBankA1Pointer)
            .Distinct())
        {
            int address = 0xa10000 | populationPointer;
            for (int index = 0; index < RoomEnemySystem.MaximumEnemyCount; index++, address += 16)
            {
                ushort definition = ReadWord(bus, address);
                if (definition == 0xffff)
                    break;
                if (definition != ElevatorDefinition)
                    continue;
                records.Add(new RoomEnemyPopulationRecord(
                    definition,
                    ReadWord(bus, address + 2),
                    ReadWord(bus, address + 4),
                    ReadWord(bus, address + 6),
                    ReadWord(bus, address + 8),
                    ReadWord(bus, address + 10),
                    ReadWord(bus, address + 12),
                    ReadWord(bus, address + 14)));
            }
        }
        return records;
    }

    private static ushort ParseBankA1Pointer(string line)
    {
        int separator = line.IndexOf(' ');
        if (separator < 0 ||
            !int.TryParse(
                line.AsSpan(2, separator - 2),
                NumberStyles.HexNumber,
                CultureInfo.InvariantCulture,
                out int address) ||
            (address & 0xff0000) != 0xa10000)
        {
            throw new InvalidDataException($"Malformed bank-$A1 symbol line: {line}");
        }
        return unchecked((ushort)address);
    }

    private static LoadedElevator LoadSelected(
        ISnesAddressSpace retailBus,
        IReadOnlyList<RoomEnemyPopulationRecord> records,
        bool prepareArrival = false)
    {
        var bus = new PopulationSelectionAddressSpace(retailBus, records);
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusPoseIds.FacingRightNormalPose,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        var projectiles = new SamusProjectileSystem();
        var enemies = new RoomEnemySystem();
        if (prepareArrival)
            enemies.PrepareElevatorArrival();
        enemies.Load(
            bus,
            PopulationSelectionAddressSpace.PopulationPointer,
            PopulationSelectionAddressSpace.TilesetPointer,
            new SnesVram(),
            new SnesCgram(),
            () => 0x1234,
            samus: samus);
        return new LoadedElevator(bus, enemies, samus, projectiles);
    }

    private static void Step(LoadedElevator loaded, ushort newlyPressedInput)
    {
        RoomEnemySlot slot = loaded.Enemies.Slots[0];
        loaded.Enemies.StepFrame(
            CameraX(slot),
            CameraY(slot),
            timeIsFrozen: false,
            loaded.Samus,
            newlyPressedControllerInput: newlyPressedInput,
            samusProjectiles: loaded.Projectiles);
    }

    private static void ArmProjectile(SamusProjectileSlot projectile, RoomEnemySlot target)
    {
        projectile.ClearFields();
        projectile.Type = 0;
        projectile.Damage = 999;
        projectile.Direction = (ushort)SamusProjectileDirection.Right;
        projectile.XPosition = target.XPosition;
        projectile.YPosition = target.YPosition;
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    private static ushort CameraX(RoomEnemySlot slot) =>
        slot.XPosition > 128 ? unchecked((ushort)(slot.XPosition - 128)) : (ushort)0;

    private static ushort CameraY(RoomEnemySlot slot) =>
        slot.YPosition > 112 ? unchecked((ushort)(slot.YPosition - 112)) : (ushort)0;

    private static uint FixedY(RoomEnemySlot slot) =>
        ((uint)slot.YPosition << 16) | slot.YSubposition;

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private readonly record struct LoadedElevator(
        PopulationSelectionAddressSpace Bus,
        RoomEnemySystem Enemies,
        SamusState Samus,
        SamusProjectileSystem Projectiles);

    private readonly record struct AnimationResult(int MapCount, int SpriteCount);
    private readonly record struct DepartureResult(uint DeltaFixed);
}
