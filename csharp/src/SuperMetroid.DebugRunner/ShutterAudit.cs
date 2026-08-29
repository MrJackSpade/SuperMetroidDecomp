using System.Globalization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;

/// <summary>
/// ROM-backed regression for all five bank-$A2 shutter definitions. It inventories every
/// named retail population record, validates literal parameter decoding, exercises the three
/// movement engines and their custom collision callbacks, and separately instantiates the
/// complete horizontal definition which Nintendo shipped without a named room placement.
/// </summary>
internal static class ShutterAudit
{
    private const ushort GrowingDefinition = 0xd4ff;
    private const ushort VerticalDefinition = 0xd53f;
    private const ushort HorizontalDefinition = 0xd57f;
    private const ushort DestroyableVerticalDefinition = 0xd5bf;
    private const ushort KamerVerticalDefinition = 0xd5ff;

    private static readonly ushort[] FamilyDefinitions =
    {
        GrowingDefinition,
        VerticalDefinition,
        HorizontalDefinition,
        DestroyableVerticalDefinition,
        KamerVerticalDefinition,
    };

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace retailBus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        Dictionary<ushort, List<RoomEnemyPopulationRecord>> records =
            ReadEveryNamedRetailRecord(retailBus);

        VerifyDefinitionHeaders(retailBus);
        VerifyRetailOccurrenceCounts(records);
        VerifyEveryRetailInitialization(retailBus, records);
        VerifyEveryGrowingDispatcherSelector(retailBus, records[GrowingDefinition][0]);
        GrowingResult growing = VerifyGrowingShutter(retailBus, records[GrowingDefinition][0]);
        VerticalResult vertical = VerifyVerticalPlatformCycle(
            retailBus,
            records[KamerVerticalDefinition][0]);
        VerifyVerticalReactions(
            retailBus,
            records[VerticalDefinition].First(record => (record.Parameter1 & 0xff) == 4));
        VerifyDestroyableVerticalShot(retailBus, records[DestroyableVerticalDefinition][0]);
        HorizontalResult horizontal = VerifyHorizontalShutter(retailBus);

        Console.WriteLine(
            "Shutter audit passed: all 33 named retail records decoded (5 growing, " +
            "16 shootable vertical, 8 destroyable vertical, 4 Kamer); the retail-unused " +
            $"horizontal definition also loaded. Growing travel={growing.TravelPixels}px/" +
            $"{growing.MapCount} maps, Kamer travel={vertical.TravelPixels}px/" +
            $"{vertical.MapCount} maps with rider carry, horizontal travel=" +
            $"{horizontal.TravelPixels}px with shot triggers and power-bomb gates; touch, beam, " +
            "freeze/damage admission, indestructible callbacks, sound gating, and live OBJ " +
            "all matched the ROM-defined paths.");
        return 0;
    }

    private static void VerifyDefinitionHeaders(ISnesAddressSpace bus)
    {
        AssertHeader(
            bus,
            GrowingDefinition,
            xRadius: 8,
            yRadius: 8,
            initialization: 0xe9da,
            main: 0xeab6,
            touch: 0x804c,
            shot: 0x802d,
            powerBomb: 0x804c);
        AssertHeader(
            bus,
            VerticalDefinition,
            xRadius: 8,
            yRadius: 32,
            initialization: 0xee12,
            main: 0xeed1,
            touch: 0xf09d,
            shot: 0xf0a2,
            powerBomb: 0xf0b6);
        AssertHeader(
            bus,
            HorizontalDefinition,
            xRadius: 32,
            yRadius: 8,
            initialization: 0xf111,
            main: 0xf1de,
            touch: 0xf3d8,
            shot: 0xf40e,
            powerBomb: 0xf41a);
        AssertHeader(
            bus,
            DestroyableVerticalDefinition,
            xRadius: 8,
            yRadius: 32,
            initialization: 0xee12,
            main: 0xeed1,
            touch: 0xf09d,
            shot: 0xf0aa,
            powerBomb: 0xf0b6);
        AssertHeader(
            bus,
            KamerVerticalDefinition,
            xRadius: 16,
            yRadius: 8,
            initialization: 0xee05,
            main: 0xeed1,
            touch: 0xf09d,
            shot: 0xf0a2,
            powerBomb: 0xf0b6);
    }

    private static void AssertHeader(
        ISnesAddressSpace bus,
        ushort definitionPointer,
        ushort xRadius,
        ushort yRadius,
        ushort initialization,
        ushort main,
        ushort touch,
        ushort shot,
        ushort powerBomb)
    {
        RoomEnemyDefinition definition = RoomEnemySystem.ReadDefinition(bus, definitionPointer);
        if (definition.Bank != 0xa2 || definition.Health != 20 || definition.Damage != 40 ||
            definition.XRadius != xRadius || definition.YRadius != yRadius ||
            definition.InitializationAiPointer != initialization ||
            definition.MainAiPointer != main || definition.TouchAiPointer != touch ||
            definition.ShotAiPointer != shot ||
            definition.PowerBombReactionPointer != powerBomb || definition.Layer != 5)
        {
            throw new InvalidDataException(
                $"Shutter ${definitionPointer:X4} header mismatch: bank=${definition.Bank:X2}, " +
                $"health/damage={definition.Health}/{definition.Damage}, radii=" +
                $"{definition.XRadius}/{definition.YRadius}, init/main=" +
                $"${definition.InitializationAiPointer:X4}/${definition.MainAiPointer:X4}, " +
                $"touch/shot/PB=${definition.TouchAiPointer:X4}/" +
                $"${definition.ShotAiPointer:X4}/${definition.PowerBombReactionPointer:X4}, " +
                $"layer={definition.Layer}.");
        }
    }

    private static void VerifyRetailOccurrenceCounts(
        IReadOnlyDictionary<ushort, List<RoomEnemyPopulationRecord>> records)
    {
        var expected = new Dictionary<ushort, int>
        {
            [GrowingDefinition] = 5,
            [VerticalDefinition] = 16,
            [HorizontalDefinition] = 0,
            [DestroyableVerticalDefinition] = 8,
            [KamerVerticalDefinition] = 4,
        };
        foreach ((ushort definition, int count) in expected)
        {
            int actual = records[definition].Count;
            if (actual != count)
            {
                throw new InvalidDataException(
                    $"Shutter ${definition:X4} appeared in {actual}, not {count}, named retail records.");
            }
        }
    }

    private static void VerifyEveryRetailInitialization(
        ISnesAddressSpace retailBus,
        IReadOnlyDictionary<ushort, List<RoomEnemyPopulationRecord>> records)
    {
        foreach (ushort definition in FamilyDefinitions.Where(value => value != HorizontalDefinition))
        {
            IReadOnlyList<RoomEnemyPopulationRecord> familyRecords = records[definition];
            LoadedPopulation loaded = LoadSelected(retailBus, familyRecords);
            if (loaded.Enemies.EnemyCount != familyRecords.Count)
            {
                throw new InvalidDataException(
                    $"Shutter ${definition:X4} selected load produced " +
                    $"{loaded.Enemies.EnemyCount}/{familyRecords.Count} actors.");
            }

            for (int index = 0; index < familyRecords.Count; index++)
            {
                RoomEnemyPopulationRecord population = familyRecords[index];
                RoomEnemySlot slot = loaded.Enemies.Slots[index];
                if (slot.Spawn.Population != population || slot.EnemyDefinitionPointer != definition ||
                    slot.XPosition != population.XPosition || slot.YPosition != population.YPosition ||
                    slot.Parameter1 != population.Parameter1 || slot.Parameter2 != population.Parameter2 ||
                    slot.ExtraProperties != 0)
                {
                    throw new InvalidDataException(
                        $"Shutter ${definition:X4} retail record {index} was not preserved during load.");
                }

                if (definition == GrowingDefinition)
                    AssertGrowingInitialization(
                        retailBus,
                        slot,
                        loaded.Enemies.GrowingShutterStates[index]!);
                else
                    AssertVerticalInitialization(
                        retailBus,
                        slot,
                        loaded.Enemies.VerticalShutterStates[index]!);
            }
        }
    }

    private static void AssertGrowingInitialization(
        ISnesAddressSpace retailBus,
        RoomEnemySlot slot,
        GrowingShutterEnemyState state)
    {
        RoomEnemyPopulationRecord population = slot.Spawn.Population;
        int selector = population.ExtraProperties * 2 + population.InitializationParameter;
        if ((uint)selector >= 4)
            throw new InvalidDataException($"Retail growing shutter used selector {selector}.");
        var expectedFunction = (GrowingShutterFunction)ReadWord(
            retailBus,
            0xa2ea4e + selector * 2);
        bool growsUp = population.ExtraProperties != 0;
        int speedAddress = 0xa2ea56 + (byte)population.Parameter2 * 4;
        short expectedVelocity = unchecked((short)ReadWord(retailBus, speedAddress));
        ushort expectedSubvelocity = ReadWord(retailBus, speedAddress + 2);
        if (state.Function != expectedFunction || state.GrowthLevel != 0 ||
            state.GrowthLevel0OriginY != population.YPosition ||
            state.GrowthLevel1OriginY != unchecked((ushort)(population.YPosition + (growsUp ? -8 : 8))) ||
            state.GrowthLevel2OriginY != unchecked((ushort)(population.YPosition + (growsUp ? -16 : 16))) ||
            state.GrowthLevel3OriginY != unchecked((ushort)(population.YPosition + (growsUp ? -24 : 24))) ||
            state.GrowthVelocity != expectedVelocity ||
            state.GrowthSubvelocity != expectedSubvelocity ||
            slot.CurrentInstruction != 0xe998 || slot.YRadius != 8)
        {
            throw new InvalidDataException(
                $"Growing shutter init mismatch at ({population.XPosition:X4}," +
                $"{population.YPosition:X4}): function={state.Function}, level={state.GrowthLevel}, " +
                $"origins={state.GrowthLevel0OriginY:X4}/{state.GrowthLevel1OriginY:X4}/" +
                $"{state.GrowthLevel2OriginY:X4}/{state.GrowthLevel3OriginY:X4}, speed=" +
                $"{state.GrowthVelocity}:{state.GrowthSubvelocity:X4}, list/radius=" +
                $"${slot.CurrentInstruction:X4}/{slot.YRadius}.");
        }
    }

    private static void AssertVerticalInitialization(
        ISnesAddressSpace retailBus,
        RoomEnemySlot slot,
        VerticalShutterEnemyState state)
    {
        RoomEnemyPopulationRecord population = slot.Spawn.Population;
        ushort speedIndex = unchecked((byte)population.InitializationParameter);
        ushort direction = unchecked((byte)(population.InitializationParameter >> 8));
        (short downWhole, ushort downFraction) = ReadLinearSpeed(
            retailBus,
            speedIndex,
            negative: false,
            slot);
        (short upWhole, ushort upFraction) = ReadLinearSpeed(
            retailBus,
            speedIndex,
            negative: true,
            slot);
        ushort distance = unchecked((byte)(population.Parameter1 >> 8));
        ushort expectedMin = direction == 0
            ? unchecked((ushort)(population.YPosition - distance))
            : population.YPosition;
        ushort expectedMax = direction == 0
            ? population.YPosition
            : unchecked((ushort)(population.YPosition + distance));
        ushort expectedList = population.DefinitionPointer == KamerVerticalDefinition
            ? (ushort)0xede7
            : (ushort)0xe9aa;

        if (state.Function != VerticalShutterFunction.Initial ||
            state.SpeedTableIndex != speedIndex || state.PrimaryDirection != direction ||
            state.ReactionDirection != direction ||
            state.DownVelocity != downWhole || state.DownSubvelocity != downFraction ||
            state.UpVelocity != upWhole || state.UpSubvelocity != upFraction ||
            state.MovedUpRestParameter != (byte)population.ExtraProperties ||
            state.MovedDownRestParameter != (byte)(population.ExtraProperties >> 8) ||
            state.MovedUpRestTime != unchecked((ushort)((byte)population.ExtraProperties << 4)) ||
            state.MovedDownRestTime != unchecked((ushort)((byte)(population.ExtraProperties >> 8) << 4)) ||
            state.TriggerMode != (byte)population.Parameter1 ||
            state.InitialFunctionTableOffset != unchecked((ushort)((byte)population.Parameter1 * 2)) ||
            state.TravelDistance != distance ||
            state.HorizontalProximityOrWaitTime != population.Parameter2 ||
            state.FunctionTimer != population.Parameter2 ||
            state.MinimumYPosition != expectedMin || state.MaximumYPosition != expectedMax ||
            slot.CurrentInstruction != expectedList)
        {
            throw new InvalidDataException(
                $"Vertical shutter ${population.DefinitionPointer:X4} init mismatch at " +
                $"({population.XPosition:X4},{population.YPosition:X4}): function={state.Function}, " +
                $"speed={state.DownVelocity}:{state.DownSubvelocity:X4}/" +
                $"{state.UpVelocity}:{state.UpSubvelocity:X4}, direction={state.PrimaryDirection}, " +
                $"rests={state.MovedUpRestTime:X4}/{state.MovedDownRestTime:X4}, " +
                $"mode/distance={state.TriggerMode}/{state.TravelDistance}, bounds=" +
                $"{state.MinimumYPosition:X4}-{state.MaximumYPosition:X4}, list=" +
                $"${slot.CurrentInstruction:X4}.");
        }
    }

    private static (short Whole, ushort Fraction) ReadLinearSpeed(
        ISnesAddressSpace bus,
        ushort speedIndex,
        bool negative,
        RoomEnemySlot slot)
    {
        // The common speed table is cartridge-global, not actor state. This helper receives
        // the slot solely so a corrupt definition bank produces an actionable assertion.
        if (slot.Definition.Bank != 0xa2)
            throw new InvalidDataException("Shutter unexpectedly left bank $A2.");
        int address = 0xa08187 + speedIndex * 8 + (negative ? 4 : 0);
        return (
            unchecked((short)ReadWord(bus, address)),
            ReadWord(bus, address + 2));
    }

    private static GrowingResult VerifyGrowingShutter(
        ISnesAddressSpace retailBus,
        RoomEnemyPopulationRecord population)
    {
        LoadedPopulation loaded = LoadSelected(retailBus, new[] { population });
        RoomEnemySlot slot = loaded.Enemies.Slots[0];
        GrowingShutterEnemyState state = loaded.Enemies.GrowingShutterStates[0]!;
        SamusState samus = loaded.Samus;
        ushort startY = slot.YPosition;

        samus.XPosition = unchecked((ushort)(slot.XPosition + population.Parameter1));
        samus.YPosition = slot.YPosition;
        StepCentered(loaded.Enemies, samus, slot);
        if (state.Function != GrowingShutterFunction.WaitToGrowDownForProximity)
            throw new InvalidDataException("Growing shutter accepted equality in its strict proximity test.");

        samus.XPosition = unchecked((ushort)(slot.XPosition + population.Parameter1 - 1));
        StepCentered(loaded.Enemies, samus, slot);
        if (state.Function != GrowingShutterFunction.GrowDown ||
            loaded.Enemies.LastShutterSoundEffect != 0x000e)
        {
            throw new InvalidDataException(
                $"Growing shutter activation failed: function={state.Function}, " +
                $"sound={loaded.Enemies.LastShutterSoundEffect?.ToString("X4") ?? "none"}.");
        }

        var maps = new HashSet<ushort>();
        for (int frame = 0; frame < 512 && state.GrowthLevel < 4; frame++)
        {
            StepCentered(loaded.Enemies, samus, slot);
            maps.Add(slot.SpritemapPointer);
        }
        short travel = unchecked((short)(slot.YPosition - startY));
        if (state.GrowthLevel != 4 || travel != 40 || slot.YRadius != 32 || maps.Count != 4)
        {
            throw new InvalidDataException(
                $"Growing shutter stages failed: level={state.GrowthLevel}, travel={travel}, " +
                $"radius={slot.YRadius}, maps={maps.Count}.");
        }
        AssertDrawsObj(loaded.Enemies, slot);
        return new GrowingResult(travel, maps.Count);
    }

    /// <summary>
    /// The retail rooms happen to use only the downward proximity selector. Construct one
    /// actor for each of the four legal selector values so the two timer paths and both
    /// growth directions cannot silently regress behind otherwise-complete retail coverage.
    /// Definition data, function pointers, speeds, instructions, and graphics still come
    /// directly from the supplied cartridge.
    /// </summary>
    private static void VerifyEveryGrowingDispatcherSelector(
        ISnesAddressSpace retailBus,
        RoomEnemyPopulationRecord retailTemplate)
    {
        GrowingShutterFunction[] expectedFunctions =
        {
            GrowingShutterFunction.WaitToGrowDownForTimer,
            GrowingShutterFunction.WaitToGrowDownForProximity,
            GrowingShutterFunction.WaitToGrowUpForTimer,
            GrowingShutterFunction.WaitToGrowUpForProximity,
        };

        for (int selector = 0; selector < expectedFunctions.Length; selector++)
        {
            ushort waitValue = (selector & 1) == 0 ? (ushort)2 : (ushort)32;
            var population = new RoomEnemyPopulationRecord(
                GrowingDefinition,
                XPosition: 0x0100,
                YPosition: 0x0180,
                InitializationParameter: unchecked((ushort)(selector & 1)),
                Properties: retailTemplate.Properties,
                ExtraProperties: unchecked((ushort)(selector >> 1)),
                Parameter1: waitValue,
                Parameter2: retailTemplate.Parameter2);
            LoadedPopulation loaded = LoadSelected(retailBus, new[] { population });
            RoomEnemySlot slot = loaded.Enemies.Slots[0];
            GrowingShutterEnemyState state = loaded.Enemies.GrowingShutterStates[0]!;
            GrowingShutterFunction expectedWait = expectedFunctions[selector];
            ushort tableFunction = ReadWord(retailBus, 0xa2ea4e + selector * 2);
            if (state.Function != expectedWait || tableFunction != (ushort)expectedWait)
            {
                throw new InvalidDataException(
                    $"Growing shutter selector {selector} resolved to {state.Function}/" +
                    $"${tableFunction:X4}, expected {expectedWait}.");
            }

            if ((selector & 1) == 0)
            {
                // $A2:EABD/$EAFD test before decrementing: 2 -> 1 -> 0 consumes two
                // complete frames, and only the third call changes the dispatcher.
                StepCentered(loaded.Enemies, loaded.Samus, slot);
                if (state.Function != expectedWait || slot.Parameter1 != 1)
                    throw new InvalidDataException($"Growing shutter timer selector {selector} lost frame one.");
                StepCentered(loaded.Enemies, loaded.Samus, slot);
                if (state.Function != expectedWait || slot.Parameter1 != 0)
                    throw new InvalidDataException($"Growing shutter timer selector {selector} lost frame two.");
                StepCentered(loaded.Enemies, loaded.Samus, slot);
            }
            else
            {
                loaded.Samus.XPosition = unchecked((ushort)(slot.XPosition + waitValue));
                loaded.Samus.YPosition = slot.YPosition;
                StepCentered(loaded.Enemies, loaded.Samus, slot);
                if (state.Function != expectedWait)
                {
                    throw new InvalidDataException(
                        $"Growing shutter proximity selector {selector} accepted equality.");
                }
                loaded.Samus.XPosition--;
                StepCentered(loaded.Enemies, loaded.Samus, slot);
            }

            bool growsUp = expectedWait is GrowingShutterFunction.WaitToGrowUpForTimer or
                GrowingShutterFunction.WaitToGrowUpForProximity;
            GrowingShutterFunction expectedGrowth = growsUp
                ? GrowingShutterFunction.GrowUp
                : GrowingShutterFunction.GrowDown;
            if (state.Function != expectedGrowth || loaded.Enemies.LastShutterSoundEffect != 0x000e)
            {
                throw new InvalidDataException(
                    $"Growing shutter selector {selector} failed activation: function=" +
                    $"{state.Function}, sound={loaded.Enemies.LastShutterSoundEffect?.ToString("X4") ?? "none"}.");
            }

            ushort expectedEnd = unchecked((ushort)(
                state.GrowthLevel3OriginY + (growsUp ? -16 : 16)));
            for (int frame = 0; frame < 1_024 && state.GrowthLevel < 4; frame++)
                StepCentered(loaded.Enemies, loaded.Samus, slot);
            if (state.GrowthLevel != 4 || slot.YPosition != expectedEnd || slot.YRadius != 32)
            {
                throw new InvalidDataException(
                    $"Growing shutter selector {selector} failed its terminal stage: level=" +
                    $"{state.GrowthLevel}, Y=${slot.YPosition:X4}/${expectedEnd:X4}, radius={slot.YRadius}.");
            }
        }
    }

    private static VerticalResult VerifyVerticalPlatformCycle(
        ISnesAddressSpace retailBus,
        RoomEnemyPopulationRecord population)
    {
        LoadedPopulation loaded = LoadSelected(retailBus, new[] { population });
        RoomEnemySlot slot = loaded.Enemies.Slots[0];
        VerticalShutterEnemyState state = loaded.Enemies.VerticalShutterStates[0]!;
        SamusState samus = loaded.Samus;
        ushort startY = slot.YPosition;

        StepCentered(loaded.Enemies, samus, slot);
        samus.XPosition = unchecked((ushort)(slot.XPosition + state.HorizontalProximityOrWaitTime));
        samus.YPosition = slot.YPosition;
        StepCentered(loaded.Enemies, samus, slot);
        if (state.Function != VerticalShutterFunction.WaitForHorizontalProximity)
            throw new InvalidDataException("Kamer vertical platform accepted equality in proximity mode.");

        samus.XPosition--;
        StepCentered(loaded.Enemies, samus, slot);
        if (state.Function != VerticalShutterFunction.MovingDown ||
            loaded.Enemies.LastShutterSoundEffect != 0x000e)
        {
            throw new InvalidDataException(
                $"Kamer proximity activation failed: function={state.Function}, " +
                $"sound={loaded.Enemies.LastShutterSoundEffect?.ToString("X4") ?? "none"}.");
        }

        PlaceSamusOnTop(samus, slot);
        var maps = new HashSet<ushort>();
        bool sawCarry = false;
        ushort maximumY = slot.YPosition;
        bool returnedToProximity = false;
        for (int frame = 0; frame < 1_024; frame++)
        {
            ClearExternalDisplacement(samus);
            StepCentered(loaded.Enemies, samus, slot);
            short carry = unchecked((short)samus.Kinematics.ExtraYDisplacement);
            samus.YPosition = unchecked((ushort)(samus.YPosition + carry));
            sawCarry |= carry != 0;
            maximumY = Math.Max(maximumY, slot.YPosition);
            maps.Add(slot.SpritemapPointer);
            returnedToProximity = state.Function == VerticalShutterFunction.WaitForHorizontalProximity &&
                maximumY > startY;
            if (returnedToProximity)
                break;
        }
        int travel = maximumY - startY;
        if (!sawCarry || !returnedToProximity || travel < state.TravelDistance || maps.Count != 4)
        {
            throw new InvalidDataException(
                $"Kamer vertical cycle failed: carry={sawCarry}, returned={returnedToProximity}, " +
                $"travel={travel}/{state.TravelDistance}, maps={maps.Count}, function={state.Function}.");
        }
        AssertDrawsObj(loaded.Enemies, slot);
        return new VerticalResult(travel, maps.Count);
    }

    private static void VerifyVerticalReactions(
        ISnesAddressSpace retailBus,
        RoomEnemyPopulationRecord population)
    {
        LoadedPopulation touch = LoadSelected(retailBus, new[] { population });
        RoomEnemySlot touchSlot = touch.Enemies.Slots[0];
        StepCentered(touch.Enemies, touch.Samus, touchSlot);
        touch.Samus.XPosition = touchSlot.XPosition;
        touch.Samus.YPosition = touchSlot.YPosition;
        touch.Samus.Health = 999;
        if (!touch.Enemies.ResolveOrdinarySamusContact(touch.Samus, 0) ||
            touch.Samus.Health != 999 ||
            touch.Enemies.VerticalShutterStates[0]!.Function != VerticalShutterFunction.MovingUp)
        {
            throw new InvalidDataException("Vertical shutter touch trigger entered common damage or failed to move.");
        }

        LoadedPopulation shot = LoadSelected(retailBus, new[] { population });
        RoomEnemySlot shotSlot = shot.Enemies.Slots[0];
        StepCentered(shot.Enemies, shot.Samus, shotSlot);
        var projectiles = new SamusProjectileSystem();
        var shared = new SamusBombProjectileSystem();
        ArmProjectile(projectiles.Slots[0], shotSlot, projectileType: 0, damage: 999);
        if (shot.Enemies.ResolveOrdinaryProjectileHits(
                shot.Bus,
                projectiles,
                shared,
                shot.Samus) != 1 ||
            shotSlot.Health != 20 || projectiles.Slots[0].Type != 0 ||
            (projectiles.Slots[0].Direction & 0x0010) == 0 ||
            shot.Enemies.VerticalShutterStates[0]!.Function != VerticalShutterFunction.MovingUp)
        {
            throw new InvalidDataException(
                $"Reaction-only vertical shot failed: health={shotSlot.Health}, type=" +
                $"${projectiles.Slots[0].Type:X4}, direction=${projectiles.Slots[0].Direction:X4}, " +
                $"function={shot.Enemies.VerticalShutterStates[0]!.Function}.");
        }

        LoadedPopulation powerBomb = LoadSelected(retailBus, new[] { population });
        RoomEnemySlot powerBombSlot = powerBomb.Enemies.Slots[0];
        StepCentered(powerBomb.Enemies, powerBomb.Samus, powerBombSlot);
        int powerBombHits = powerBomb.Enemies.ResolveOrdinaryPowerBombHits(
                powerBomb.Bus,
                powerBombSlot.XPosition,
                powerBombSlot.YPosition,
                explosionRadius: 64);
        byte powerBombVulnerability = powerBomb.Bus.ReadByte(
            0xb40000 | unchecked((ushort)(
                powerBombSlot.Definition.VulnerabilityPointer + 14)));
        if (powerBombHits != 0 || powerBombVulnerability != 0 ||
            powerBombSlot.Health != 20 ||
            powerBomb.Enemies.VerticalShutterStates[0]!.Function != VerticalShutterFunction.InitialNoOp)
        {
            throw new InvalidDataException(
                $"Vertical shutter power-bomb vulnerability gate failed: hits={powerBombHits}, " +
                $"health={powerBombSlot.Health}, function=" +
                $"{powerBomb.Enemies.VerticalShutterStates[0]!.Function}, reaction=" +
                $"${powerBombSlot.Definition.PowerBombReactionPointer:X4}, vulnerability=" +
                $"${powerBombSlot.Definition.VulnerabilityPointer:X4}:${powerBombVulnerability:X2}.");
        }
    }

    private static void VerifyDestroyableVerticalShot(
        ISnesAddressSpace retailBus,
        RoomEnemyPopulationRecord population)
    {
        LoadedPopulation loaded = LoadSelected(retailBus, new[] { population });
        RoomEnemySlot slot = loaded.Enemies.Slots[0];
        StepCentered(loaded.Enemies, loaded.Samus, slot);
        var projectiles = new SamusProjectileSystem();
        var shared = new SamusBombProjectileSystem();
        ArmProjectile(projectiles.Slots[0], slot, projectileType: 0, damage: 20);
        if (loaded.Enemies.ResolveOrdinaryProjectileHits(
                loaded.Bus,
                projectiles,
                shared,
                loaded.Samus) != 1 ||
            slot.Health != 0 || !slot.Properties.HasAny(EnemyProperties.Deleted))
        {
            throw new InvalidDataException(
                $"Destroyable vertical shutter did not run common shot damage: health=" +
                $"{slot.Health}, properties=${slot.Properties:X4}.");
        }
    }

    private static HorizontalResult VerifyHorizontalShutter(ISnesAddressSpace retailBus)
    {
        // No named retail population contains $D57F. These parameters are therefore an
        // explicit executable fixture, while the definition, speed table, instruction list,
        // graphics, vulnerability, and all callbacks remain untouched cartridge data.
        var population = new RoomEnemyPopulationRecord(
            HorizontalDefinition,
            XPosition: 0x0100,
            YPosition: 0x0100,
            InitializationParameter: 0x0108,
            Properties: 0xa800,
            ExtraProperties: 0x0101,
            Parameter1: 0x2004,
            Parameter2: 0x0010);
        LoadedPopulation loaded = LoadSelected(retailBus, new[] { population });
        RoomEnemySlot slot = loaded.Enemies.Slots[0];
        HorizontalShutterEnemyState state = loaded.Enemies.HorizontalShutterStates[0]
            ?? throw new InvalidDataException("Horizontal shutter omitted typed state.");
        if (state.Function != HorizontalShutterFunction.Initial || state.PrimaryDirection != 1 ||
            state.ReactionDirection != 0 || state.TriggerMode != 4 ||
            state.TravelDistance != 0x20 || slot.CurrentInstruction != 0xe9d4)
        {
            throw new InvalidDataException(
                $"Horizontal shutter init failed: function={state.Function}, direction=" +
                $"{state.PrimaryDirection}/{state.ReactionDirection}, mode={state.TriggerMode}, " +
                $"distance={state.TravelDistance}, list=${slot.CurrentInstruction:X4}.");
        }

        StepCentered(loaded.Enemies, loaded.Samus, slot);
        var projectiles = new SamusProjectileSystem();
        var shared = new SamusBombProjectileSystem();
        ArmProjectile(projectiles.Slots[0], slot, projectileType: 0, damage: 999);
        if (loaded.Enemies.ResolveOrdinaryProjectileHits(
                loaded.Bus,
                projectiles,
                shared,
                loaded.Samus) != 1 ||
            slot.Health != 20 || state.Function != HorizontalShutterFunction.MovingRight)
        {
            throw new InvalidDataException(
                $"Horizontal shutter shot trigger failed: health={slot.Health}, function={state.Function}.");
        }

        loaded.Samus.XPosition = unchecked((ushort)(slot.XPosition + 1));
        loaded.Samus.YPosition = slot.YPosition;
        ClearExternalDisplacement(loaded.Samus);
        StepCentered(loaded.Enemies, loaded.Samus, slot, controllerInput: (ushort)SnesButton.Left);
        ushort expectedPush = unchecked((ushort)(state.RightVelocity + 4));
        if (!state.MovingSamus ||
            loaded.Samus.Kinematics.ExtraXSubdisplacement != state.RightSubvelocity ||
            loaded.Samus.Kinematics.ExtraXDisplacement != expectedPush)
        {
            throw new InvalidDataException(
                $"Horizontal shutter rider push failed: movingSamus={state.MovingSamus}, " +
                $"extra={loaded.Samus.Kinematics.ExtraXDisplacement}:" +
                $"{loaded.Samus.Kinematics.ExtraXSubdisplacement:X4}, expected=" +
                $"{expectedPush}:{state.RightSubvelocity:X4}.");
        }

        ushort startX = population.XPosition;
        loaded.Samus.XPosition = 0;
        loaded.Samus.YPosition = 0;
        for (int frame = 0; frame < 512 &&
            state.Function == HorizontalShutterFunction.MovingRight; frame++)
        {
            ClearExternalDisplacement(loaded.Samus);
            StepCentered(loaded.Enemies, loaded.Samus, slot);
        }
        int travel = unchecked((ushort)(slot.XPosition - startX));
        if (travel < state.TravelDistance ||
            state.Function != HorizontalShutterFunction.StoppedAfterMovingRight)
        {
            throw new InvalidDataException(
                $"Horizontal shutter travel failed: travel={travel}/{state.TravelDistance}, " +
                $"function={state.Function}.");
        }
        AssertDrawsObj(loaded.Enemies, slot);

        LoadedPopulation powerBomb = LoadSelected(retailBus, new[] { population });
        RoomEnemySlot pbSlot = powerBomb.Enemies.Slots[0];
        StepCentered(powerBomb.Enemies, powerBomb.Samus, pbSlot);
        int powerBombHits = powerBomb.Enemies.ResolveOrdinaryPowerBombHits(
                powerBomb.Bus,
                pbSlot.XPosition,
                pbSlot.YPosition,
                explosionRadius: 64);
        byte powerBombVulnerability = powerBomb.Bus.ReadByte(
            0xb40000 | unchecked((ushort)(pbSlot.Definition.VulnerabilityPointer + 14)));
        if (powerBombHits != 0 || powerBombVulnerability != 0 ||
            pbSlot.Health != 20 ||
            powerBomb.Enemies.HorizontalShutterStates[0]!.Function !=
                HorizontalShutterFunction.InitialNoOp)
        {
            throw new InvalidDataException(
                $"Horizontal shutter power-bomb vulnerability gate failed: hits=" +
                $"{powerBombHits}, vulnerability=${powerBombVulnerability:X2}, health=" +
                $"{pbSlot.Health}, function=" +
                $"{powerBomb.Enemies.HorizontalShutterStates[0]!.Function}.");
        }
        return new HorizontalResult(travel);
    }

    private static Dictionary<ushort, List<RoomEnemyPopulationRecord>>
        ReadEveryNamedRetailRecord(ISnesAddressSpace bus)
    {
        string symbolPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "upstream-sm",
            "assets",
            "names.txt");
        if (!File.Exists(symbolPath))
            throw new FileNotFoundException("Shutter audit requires upstream-sm/assets/names.txt.", symbolPath);

        var records = FamilyDefinitions.ToDictionary(
            definition => definition,
            _ => new List<RoomEnemyPopulationRecord>());
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
                if (!records.TryGetValue(definition, out List<RoomEnemyPopulationRecord>? family))
                    continue;
                family.Add(new RoomEnemyPopulationRecord(
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

    private static LoadedPopulation LoadSelected(
        ISnesAddressSpace retailBus,
        IReadOnlyList<RoomEnemyPopulationRecord> records)
    {
        var bus = new PopulationSelectionAddressSpace(retailBus, records);
        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusState.FacingRightNormalPose,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        var enemies = new RoomEnemySystem();
        enemies.Load(
            bus,
            PopulationSelectionAddressSpace.PopulationPointer,
            PopulationSelectionAddressSpace.TilesetPointer,
            new SnesVram(),
            new SnesCgram(),
            () => 0x1234,
            samus: samus);
        return new LoadedPopulation(bus, enemies, samus);
    }

    private static void StepCentered(
        RoomEnemySystem enemies,
        SamusState samus,
        RoomEnemySlot slot,
        ushort controllerInput = 0)
    {
        ushort cameraX = slot.XPosition > 128
            ? unchecked((ushort)(slot.XPosition - 128))
            : (ushort)0;
        ushort cameraY = slot.YPosition > 112
            ? unchecked((ushort)(slot.YPosition - 112))
            : (ushort)0;
        enemies.StepFrame(
            cameraX,
            cameraY,
            timeIsFrozen: false,
            samus,
            controllerInput: controllerInput);
    }

    private static void PlaceSamusOnTop(SamusState samus, RoomEnemySlot slot)
    {
        samus.XPosition = slot.XPosition;
        samus.YPosition = unchecked((ushort)(
            slot.YPosition - slot.YRadius - samus.Kinematics.YRadius));
        samus.Kinematics.XSubposition = 0;
        samus.Kinematics.YSubposition = 0;
    }

    private static void ClearExternalDisplacement(SamusState samus)
    {
        samus.Kinematics.ExtraXSubdisplacement = 0;
        samus.Kinematics.ExtraXDisplacement = 0;
        samus.Kinematics.ExtraYSubdisplacement = 0;
        samus.Kinematics.ExtraYDisplacement = 0;
    }

    private static void ArmProjectile(
        SamusProjectileSlot projectile,
        RoomEnemySlot target,
        ushort projectileType,
        ushort damage)
    {
        projectile.ClearFields();
        projectile.Type = projectileType;
        projectile.Damage = damage;
        projectile.Direction = (ushort)SamusProjectileDirection.Right;
        projectile.XPosition = target.XPosition;
        projectile.YPosition = target.YPosition;
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    private static void AssertDrawsObj(RoomEnemySystem enemies, RoomEnemySlot slot)
    {
        var oam = new OamBuffer();
        oam.BeginFrame();
        ushort cameraX = slot.XPosition > 128 ? unchecked((ushort)(slot.XPosition - 128)) : (ushort)0;
        ushort cameraY = slot.YPosition > 112 ? unchecked((ushort)(slot.YPosition - 112)) : (ushort)0;
        enemies.DrawLayers(oam, cameraX, cameraY, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException($"Shutter ${slot.EnemyDefinitionPointer:X4} emitted no live ROM OBJ.");
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private readonly record struct LoadedPopulation(
        PopulationSelectionAddressSpace Bus,
        RoomEnemySystem Enemies,
        SamusState Samus);

    private readonly record struct GrowingResult(int TravelPixels, int MapCount);
    private readonly record struct VerticalResult(int TravelPixels, int MapCount);
    private readonly record struct HorizontalResult(int TravelPixels);
}
