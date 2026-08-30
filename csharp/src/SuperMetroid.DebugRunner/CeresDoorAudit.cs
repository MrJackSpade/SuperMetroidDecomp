using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>
/// ROM-backed audit for enemy $E23F. The disassembly's "Ceres door" name hides two pieces
/// of gameplay-critical room machinery: ordinary door actors produce the escape earthquakes,
/// while variant three destroys Ridley's room overlay and hands control to the rotating-room
/// sequence. Both are loaded from their actual room populations here.
/// </summary>
internal static class CeresDoorAudit
{
    private const ushort CeresDoorDefinition = 0xe23f;
    private const ushort FallingTileRoomPointer = 0xdf8d;
    private const ushort CeresRidleyRoomPointer = 0xe0b5;
    private const ushort CeresElevatorRoomPointer = 0xdf45;
    private const ushort EarthquakeLeftFunction = 0xf76b;
    private const ushort EarthquakeRightFunction = 0xf770;
    private const ushort RotatingDefaultFunction = 0xf7bd;
    private const ushort RotatingRumbleFunction = 0xf7dc;
    private const ushort ElevatorAnimationFunction = 0xf850;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        VerifyDefinition(bus);
        VerifyBothEarthquakeVariants(bus);
        VerifyRidleyRoomDestruction(bus);

        Console.WriteLine(
            "Ceres door audit passed: ordinary and Ridley-room earthquakes sampled the existing RNG seed without " +
            "advancing it, and elevator variant 2 ran its exact 48-tick rumble, ten " +
            "alternating ROM-backed explosions, 49th-call deletion, and $8000 handoff.");
        return 0;
    }

    private static void VerifyDefinition(ISnesAddressSpace bus)
    {
        RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(bus, CeresDoorDefinition);
        if (definition.Bank != 0xa6 || definition.Health != 40 || definition.Damage != 15 ||
            definition.XRadius != 8 || definition.YRadius != 0x20 ||
            definition.InitializationAiPointer != 0xf6c5 ||
            definition.MainAiPointer != 0xf765 ||
            definition.TouchAiPointer != 0xf920 || definition.ShotAiPointer != 0xf920)
        {
            throw new InvalidDataException(
                $"Ceres door header mismatch: bank=${definition.Bank:X2}, " +
                $"hp/damage={definition.Health}/{definition.Damage}, " +
                $"radius={definition.XRadius}/{definition.YRadius}, " +
                $"init/main=${definition.InitializationAiPointer:X4}/" +
                $"${definition.MainAiPointer:X4}, touch/shot=" +
                $"${definition.TouchAiPointer:X4}/${definition.ShotAiPointer:X4}.");
        }
    }

    private static void VerifyBothEarthquakeVariants(SuperMetroidAddressSpace bus)
    {
        // Room $8F:DF8D contains ordinary init0 variants zero and one. They intentionally
        // share $F76B but select different right/left instruction lists. Keeping init0
        // separate from the population's earlier instruction-list seed also matters for
        // dynamically spawned getaway walls, whose init0 words are five and six.
        LoadedCeresRoom loaded = LoadRoom(bus, FallingTileRoomPointer, generatedRandom: () => 0xbeef,
            readRandom: () => 0x007f);
        RoomEnemySlot[] doors = loaded.Enemies.Slots
            .Take(loaded.Enemies.EnemyCount)
            .Where(slot => slot.EnemyDefinitionPointer == CeresDoorDefinition)
            .ToArray();
        ushort expectedRightFacingList = ReadWord(bus, 0xa6f52c);
        ushort expectedLeftFacingList = ReadWord(bus, 0xa6f52e);
        if (doors.Length != 2 ||
            doors[0].Parameter1 != 0 || doors[1].Parameter1 != 1 ||
            doors.Any(door => door.VariableA != EarthquakeLeftFunction) ||
            doors[0].CurrentInstruction != expectedRightFacingList ||
            doors[1].CurrentInstruction != expectedLeftFacingList)
        {
            throw new InvalidDataException(
                $"Falling-tile room did not preserve init0 variants/lists: " +
                $"[{string.Join(',', doors.Select(slot =>
                    $"init0={slot.Parameter1}/" +
                    $"AI=$A6:{slot.VariableA:X4}/list=${slot.CurrentInstruction:X4}"))}].");
        }

        loaded.Enemies.CeresStatus = 2;
        loaded.Enemies.StepFrame(0, 0, timeIsFrozen: false, level: loaded.Assets.LevelData);
        if (loaded.Enemies.EarthquakeTimer != 4 || loaded.Enemies.EarthquakeType != 0x001a)
        {
            throw new InvalidDataException(
                $"Left Ceres door rare quake became type ${loaded.Enemies.EarthquakeType:X4}/" +
                $"timer {loaded.Enemies.EarthquakeTimer}, expected $001A/4.");
        }

        int rareQuakeTable = 0xa0872d + 0x001a * 8;
        var expectedPositiveShake = new RoomShakeFrameResult(
            Applied: true,
            Bg1X: unchecked((short)ReadWord(bus, rareQuakeTable)),
            Bg1Y: unchecked((short)ReadWord(bus, rareQuakeTable + 2)),
            Bg2X: unchecked((short)ReadWord(bus, rareQuakeTable + 4)),
            Bg2Y: unchecked((short)ReadWord(bus, rareQuakeTable + 6)),
            ShakesEnemies: true);
        RoomShakeFrameResult positiveShake = loaded.Enemies.HandleRoomShaking(timeIsFrozen: false);
        if (positiveShake != expectedPositiveShake || loaded.Enemies.EarthquakeTimer != 3 ||
            doors.Any(door => door.ShakeTimer != 2))
        {
            throw new InvalidDataException(
                $"Ceres room-shake first phase became {positiveShake}/" +
                $"timer {loaded.Enemies.EarthquakeTimer}, expected {expectedPositiveShake}/3.");
        }

        // Frozen time returns before displacement, timer decrement, and enemy-shake writes.
        if (loaded.Enemies.HandleRoomShaking(timeIsFrozen: true) != default ||
            loaded.Enemies.EarthquakeTimer != 3)
        {
            throw new InvalidDataException("Frozen Ceres room shaking changed its timer or scroll delta.");
        }

        RoomShakeFrameResult negativeShake = loaded.Enemies.HandleRoomShaking(timeIsFrozen: false);
        if (!negativeShake.Applied ||
            negativeShake.Bg1X != unchecked((short)-expectedPositiveShake.Bg1X) ||
            negativeShake.Bg1Y != unchecked((short)-expectedPositiveShake.Bg1Y) ||
            negativeShake.Bg2X != unchecked((short)-expectedPositiveShake.Bg2X) ||
            negativeShake.Bg2Y != unchecked((short)-expectedPositiveShake.Bg2Y) ||
            loaded.Enemies.EarthquakeTimer != 2)
        {
            throw new InvalidDataException(
                $"Ceres room-shake negative phase became {negativeShake}/" +
                $"timer {loaded.Enemies.EarthquakeTimer}.");
        }

        loaded.Enemies.HandleRoomShaking(timeIsFrozen: false);
        loaded.Enemies.HandleRoomShaking(timeIsFrozen: false);
        if (loaded.Enemies.EarthquakeTimer != 0)
            throw new InvalidDataException("Ceres room-shake consumer did not expire its four-frame timer.");

        // Deleted is the native scheduler exclusion bit. This audit-only isolation lets the
        // second ordinary door acquire the shared timer on the next frame.
        doors[0].Properties = doors[0].Properties.With(EnemyProperties.Deleted);
        loaded.Enemies.EarthquakeTimer = 0;

        // A seed at the exact unsigned boundary takes the common two-frame/base-type path.
        loaded.RandomSeed.Value = 0x0080;
        loaded.Enemies.StepFrame(0, 0, timeIsFrozen: false, level: loaded.Assets.LevelData);
        if (loaded.Enemies.EarthquakeTimer != 2 || loaded.Enemies.EarthquakeType != 0x0014)
        {
            throw new InvalidDataException(
                $"Second ordinary Ceres door quake became type " +
                $"${loaded.Enemies.EarthquakeType:X4}/timer {loaded.Enemies.EarthquakeTimer}, " +
                $"expected $0014/2.");
        }

        // Ridley's actual room supplies init0 variant three, the sole $F770 user. Delete
        // Ridley itself so no cinematic RNG or state transition can contaminate this one
        // enemy-function probe.
        LoadedCeresRoom ridleyRoom = LoadRoom(
            bus,
            CeresRidleyRoomPointer,
            generatedRandom: () => 0xbeef,
            readRandom: () => 0x0080);
        RoomEnemySlot ridleyDoor = ridleyRoom.Enemies.Slots
            .Take(ridleyRoom.Enemies.EnemyCount)
            .Single(slot => slot.EnemyDefinitionPointer == CeresDoorDefinition);
        foreach (RoomEnemySlot slot in ridleyRoom.Enemies.Slots.Take(ridleyRoom.Enemies.EnemyCount))
        {
            if (!ReferenceEquals(slot, ridleyDoor))
                slot.Properties = slot.Properties.With(EnemyProperties.Deleted);
        }
        if (ridleyDoor.Parameter1 != 3 ||
            ridleyDoor.VariableA != EarthquakeRightFunction)
        {
            throw new InvalidDataException(
                $"Ridley-room door selected init/AI " +
                $"{ridleyDoor.Parameter1}/" +
                $"$A6:{ridleyDoor.VariableA:X4} instead of 3/$A6:F770.");
        }

        ridleyRoom.Enemies.CeresStatus = 2;
        ridleyRoom.Enemies.StepFrame(0, 0, timeIsFrozen: false, level: ridleyRoom.Assets.LevelData);
        if (ridleyRoom.Enemies.EarthquakeTimer != 2 ||
            ridleyRoom.Enemies.EarthquakeType != 0x001d)
        {
            throw new InvalidDataException(
                $"Ridley-room Ceres door quake became type " +
                $"${ridleyRoom.Enemies.EarthquakeType:X4}/" +
                $"timer {ridleyRoom.Enemies.EarthquakeTimer}, expected $001D/2.");
        }
    }

    private static void VerifyRidleyRoomDestruction(SuperMetroidAddressSpace bus)
    {
        int generatedRandomCalls = 0;
        ushort GenerateAlternatingExplosionRandom()
        {
            // $3FFF selects explosion table entry $0C; $4000 is the exact boundary that
            // selects smoke entry three. Alternating them proves the native unsigned branch.
            ushort result = (generatedRandomCalls & 1) == 0 ? (ushort)0x3fff : (ushort)0x4000;
            generatedRandomCalls++;
            return result;
        }

        LoadedCeresRoom loaded = LoadRoom(
            bus,
            CeresElevatorRoomPointer,
            GenerateAlternatingExplosionRandom,
            readRandom: () => 0x1234,
            selection: new RoomStateSelectionContext(
                ReadOnlyMemory<byte>.Empty,
                BossBits: 1,
                HasMorphBallAndMissiles: false,
                HasPowerBombs: false));
        generatedRandomCalls = 0; // Ignore any initialization consumers; only $F7DC counts.

        RoomEnemySlot door = loaded.Enemies.Slots
            .Take(loaded.Enemies.EnemyCount)
            .Single(slot => slot.EnemyDefinitionPointer == CeresDoorDefinition &&
                slot.Parameter1 == 2);
        foreach (RoomEnemySlot slot in loaded.Enemies.Slots.Take(loaded.Enemies.EnemyCount))
        {
            if (!ReferenceEquals(slot, door))
                slot.Properties = slot.Properties.With(EnemyProperties.Deleted);
        }
        if (door.Parameter1 != 2 ||
            door.XPosition != 0x00e8 || door.YPosition != 0x0277 ||
            door.VariableA != RotatingDefaultFunction)
        {
            throw new InvalidDataException(
                $"Ceres elevator overlay loaded parameter/init0/position/function " +
                $"{door.Parameter1}/" +
                $"({door.XPosition:X4},{door.YPosition:X4})/" +
                $"$A6:{door.VariableA:X4}.");
        }

        loaded.Enemies.CeresStatus = 2;
        loaded.Enemies.StepFrame(0, 0, timeIsFrozen: false, level: loaded.Assets.LevelData);
        if (door.VariableA != RotatingRumbleFunction || door.VariableD != 0x0030 ||
            door.VariableE != 0 || door.VariableF != 0 ||
            loaded.Enemies.LastCeresDoorSoundEffectLibrary2 is not null ||
            generatedRandomCalls != 0)
        {
            throw new InvalidDataException(
                $"Ridley-room destruction setup became function $A6:{door.VariableA:X4}, " +
                $"timers={door.VariableD:X4}/{door.VariableE:X4}/{door.VariableF:X4}, " +
                $"sound={loaded.Enemies.LastCeresDoorSoundEffectLibrary2}, " +
                $"RNG calls={generatedRandomCalls}.");
        }

        ushort explosionInstruction = ReadWord(bus, 0x86e42c + 0x000c * 2);
        ushort smokeInstruction = ReadWord(bus, 0x86e42c + 3 * 2);
        (ushort X, ushort Y)[] expectedPositions =
        [
            (0x00ea, 0x0283), // Table index 3: (+2,+12).
            (0x00e6, 0x028d), // Table index 2: (-2,+22).
            (0x00e8, 0x027b), // Table index 1: ( 0, +4).
            (0x00e4, 0x026f), // Table index 0: (-4, -8).
        ];
        int explosionCount = 0;

        // Calls 1..48 leave the signed destruction timer nonnegative. The first call emits
        // immediately because rumble interval zero underflows; reloading four yields exactly
        // one event every fifth call thereafter (1, 6, ..., 46), for ten total effects.
        for (int call = 1; call <= 48; call++)
        {
            loaded.Enemies.StepFrame(0, 0, timeIsFrozen: false, level: loaded.Assets.LevelData);
            bool shouldExplode = (call - 1) % 5 == 0;
            if ((loaded.Enemies.LastCeresDoorSoundEffectLibrary2 is not null) != shouldExplode)
            {
                throw new InvalidDataException(
                    $"Ceres door sound cadence disagreed on rumble call {call}: " +
                    $"{loaded.Enemies.LastCeresDoorSoundEffectLibrary2?.ToString("X4") ?? "none"}.");
            }

            if (!shouldExplode)
                continue;

            if (loaded.Enemies.LastCeresDoorSoundEffectLibrary2 != 0x0025)
                throw new InvalidDataException("Ceres door queued the wrong library-two sound.");

            int eventIndex = explosionCount;
            (ushort expectedX, ushort expectedY) = expectedPositions[eventIndex & 3];
            ushort expectedInstruction = (eventIndex & 1) == 0
                ? explosionInstruction
                : smokeInstruction;
            explosionCount++;

            RoomEnemyProjectileSlot[] effects = loaded.Enemies.EnemyProjectiles
                .Where(projectile => projectile.Kind == RoomEnemyProjectileKind.MiscDustExplosion)
                .ToArray();
            if (effects.Length != explosionCount ||
                !effects.Any(projectile => projectile.XPosition == expectedX &&
                    projectile.YPosition == expectedY &&
                    projectile.InstructionPointer == expectedInstruction))
            {
                throw new InvalidDataException(
                    $"Ceres door effect {eventIndex} did not allocate expected " +
                    $"({expectedX:X4},{expectedY:X4}) list ${expectedInstruction:X4}; " +
                    $"live={string.Join(';', effects.Select(effect =>
                        $"({effect.XPosition:X4},{effect.YPosition:X4})/${effect.InstructionPointer:X4}"))}.");
            }
        }

        if (door.VariableA != RotatingRumbleFunction || door.VariableD != 0 ||
            explosionCount != 10 || generatedRandomCalls != 10)
        {
            throw new InvalidDataException(
                $"Ceres door's 48 active calls ended at function $A6:{door.VariableA:X4}, " +
                $"timer {door.VariableD}, effects/RNG={explosionCount}/{generatedRandomCalls}.");
        }

        // Call 49 decrements zero to $FFFF and returns before touching the interval timer,
        // RNG, projectile pool, or sound queue.
        loaded.Enemies.StepFrame(0, 0, timeIsFrozen: false, level: loaded.Assets.LevelData);
        if (door.VariableA != ElevatorAnimationFunction ||
            !door.Properties.HasAny(EnemyProperties.Invisible) ||
            loaded.Enemies.CeresStatus != 0x8000 ||
            loaded.Enemies.LastCeresDoorSoundEffectLibrary2 is not null ||
            generatedRandomCalls != 10)
        {
            throw new InvalidDataException(
                $"Ceres door completion became function $A6:{door.VariableA:X4}, " +
                $"properties=${door.Properties:X4}, status=${loaded.Enemies.CeresStatus:X4}, " +
                $"sound={loaded.Enemies.LastCeresDoorSoundEffectLibrary2}, " +
                $"RNG calls={generatedRandomCalls}.");
        }

        // The following frame must be stable and non-destructive: $F850 resumes the shared
        // elevator tile/palette animation forever while the room controller consumes $8000.
        loaded.Enemies.StepFrame(0, 0, timeIsFrozen: false, level: loaded.Assets.LevelData);
        if (door.VariableA != ElevatorAnimationFunction ||
            loaded.Enemies.CeresStatus != 0x8000 || generatedRandomCalls != 10)
        {
            throw new InvalidDataException("Ceres door did not remain in its post-destruction state.");
        }
    }

    private static LoadedCeresRoom LoadRoom(
        SuperMetroidAddressSpace bus,
        ushort roomPointer,
        Func<ushort> generatedRandom,
        Func<ushort> readRandom,
        RoomStateSelectionContext? selection = null)
    {
        CartridgeRoomHeader room = selection is RoomStateSelectionContext selected
            ? CartridgeRoomHeader.Load(bus, roomPointer, selected)
            : CartridgeRoomHeader.Load(bus, roomPointer);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);

        // The mutable holder preserves the important distinction between GenerateRandomNumber
        // and simply reading RandomNumberSeed. Callers may change the sampled seed without
        // replacing or advancing the generator delegate captured by RoomEnemySystem.Load.
        var randomSeed = new MutableWord(readRandom());
        var enemies = new RoomEnemySystem();
        enemies.Load(
            bus,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            generatedRandom,
            setRandomNumber: value => randomSeed.Value = value,
            readRandomNumber: () => randomSeed.Value,
            level: assets.LevelData);
        return new LoadedCeresRoom(enemies, assets, randomSeed);
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private sealed class MutableWord(ushort value)
    {
        public ushort Value { get; set; } = value;
    }

    private sealed record LoadedCeresRoom(
        RoomEnemySystem Enemies,
        CartridgeRoomAssets Assets,
        MutableWord RandomSeed);
}
