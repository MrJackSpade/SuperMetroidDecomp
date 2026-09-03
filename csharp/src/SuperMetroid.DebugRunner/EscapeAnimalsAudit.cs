using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Retail-ROM regression for the escape Dachora/Etecoon population. The audit selects the
/// timebomb-specific room state through cartridge selector bytecode, loads all four authored
/// records, and drives the real level collision and instruction streams.
/// </summary>
internal static class EscapeAnimalsAudit
{
    private const ushort EscapeRoomHeader = 0x9804;
    private const ushort EscapeRoomState = 0x984f;
    private const ushort EscapePopulation = 0x8ed3;
    private const ushort EscapeEtecoonDefinition = 0xf2d3;
    private const ushort EscapeDachoraDefinition = 0xf313;
    private const ushort LowLavaSurface = 0x00cd;

    private static readonly EscapeAnimalRecord[] RetailRecords =
    [
        new(0xf313, 0x00e0, 0x00b8, 0x0000, 0x2400, 0x0000, 0x0000, 0x0000),
        new(0xf2d3, 0x00e0, 0x00b8, 0x0000, 0x2000, 0x0000, 0x0000, 0x0000),
        new(0xf2d3, 0x00e0, 0x00b8, 0x0000, 0x2000, 0x0000, 0x0002, 0x0000),
        new(0xf2d3, 0x00e0, 0x00b8, 0x0000, 0x2000, 0x0000, 0x0004, 0x0000),
    ];

    private static readonly HashSet<ushort> DachoraSpritemaps =
    [
        0xeb1b, 0xeb4a, 0xeb79, 0xebb2, 0xebe1, 0xec10,
        0xec49, 0xec78, 0xeca7, 0xece0, 0xed0f, 0xed3e,
    ];

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        VerifyDefinitionHeaders(bus);
        VerifyRetailPopulation(bus);

        CartridgeRoomHeader room = LoadTimebombRoom(bus);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        VerifyInitializationPalettesAndDrawing(bus, room, assets);
        VerifyLavaBranches(bus, room, assets);
        VerifyNativeMovementAndEscapeEvent(bus, room, assets);
        VerifyAlreadyEscapedLoadGate(bus, room, assets);
        VerifyAuthoredNonCombatCallbacks(bus, room, assets);

        Console.WriteLine(
            "Escape animal audit passed: the timebomb state loaded one Dachora and three " +
            "Etecoons; ROM palettes/maps, lava-speed branches, Etecoon wall/floor movement, " +
            "Dachora callback sprint, event-$0F escape transition/load deletion, live OBJ, " +
            "and authored no-contact/no-damage behavior all agreed.");
        return 0;
    }

    private static void VerifyDefinitionHeaders(SuperMetroidAddressSpace bus)
    {
        RoomEnemyDefinition etecoon = RoomEnemySystem.ReadDefinition(
            bus,
            EscapeEtecoonDefinition);
        if (etecoon.TileDataSize != 0x0600 || etecoon.PalettePointer != 0xe525 ||
            etecoon.Health != 3000 || etecoon.Damage != 3000 ||
            etecoon.XRadius != 6 || etecoon.YRadius != 8 || etecoon.Bank != 0xb3 ||
            etecoon.InitializationAiPointer != 0xe6cb ||
            etecoon.MainAiPointer != 0xe655 || etecoon.GrappleAiPointer != 0x8000 ||
            etecoon.HurtAiPointer != 0x804c || etecoon.FrozenAiPointer != 0x8041 ||
            etecoon.PowerBombReactionPointer != 0x804c ||
            etecoon.TouchAiPointer != 0x804c || etecoon.ShotAiPointer != 0x804c ||
            etecoon.TileDataAddress != 0xac8200 || etecoon.Layer != 5 ||
            etecoon.VulnerabilityPointer != 0)
        {
            throw new InvalidDataException(
                "Escape Etecoon $F2D3 header disagrees with $B3:E525-$E730.");
        }

        RoomEnemyDefinition dachora = RoomEnemySystem.ReadDefinition(
            bus,
            EscapeDachoraDefinition);
        if (dachora.TileDataSize != 0x0c00 || dachora.PalettePointer != 0xe944 ||
            dachora.Health != 3000 || dachora.Damage != 3000 ||
            dachora.XRadius != 8 || dachora.YRadius != 24 || dachora.Bank != 0xb3 ||
            dachora.InitializationAiPointer != 0xeae5 ||
            dachora.MainAiPointer != 0xeb1a || dachora.GrappleAiPointer != 0x8000 ||
            dachora.HurtAiPointer != 0x804c || dachora.FrozenAiPointer != 0x8041 ||
            dachora.PowerBombReactionPointer != 0x804c ||
            dachora.TouchAiPointer != 0x804c || dachora.ShotAiPointer != 0x804c ||
            dachora.TileDataAddress != 0xac8800 || dachora.Layer != 5 ||
            dachora.VulnerabilityPointer != 0)
        {
            throw new InvalidDataException(
                "Escape Dachora $F313 header disagrees with $B3:E944-$EB1A.");
        }
    }

    private static void VerifyRetailPopulation(SuperMetroidAddressSpace bus)
    {
        var actual = new List<EscapeAnimalRecord>();
        int cursor = 0xa10000 | EscapePopulation;
        for (int index = 0; index < RoomEnemySystem.MaximumEnemyCount; index++, cursor += 16)
        {
            ushort definition = ReadWord(bus, cursor);
            if (definition == 0xffff)
                break;
            actual.Add(new EscapeAnimalRecord(
                definition,
                ReadWord(bus, cursor + 2),
                ReadWord(bus, cursor + 4),
                ReadWord(bus, cursor + 6),
                ReadWord(bus, cursor + 8),
                ReadWord(bus, cursor + 10),
                ReadWord(bus, cursor + 12),
                ReadWord(bus, cursor + 14)));
        }

        if (!actual.SequenceEqual(RetailRecords))
        {
            throw new InvalidDataException(
                $"Escape population $A1:{EscapePopulation:X4} contained {actual.Count} " +
                "records instead of the exact Dachora-plus-three-Etecoon sequence.");
        }
    }

    private static CartridgeRoomHeader LoadTimebombRoom(SuperMetroidAddressSpace bus)
    {
        // Room $9804 first tests event $0E and selects state $984F; only then does it test
        // the area boss bit for $9835 and fall back to $981B. Supplying the real event byte
        // proves the animals belong to the timebomb state rather than bypassing selection by
        // loading $984F as if it were a standalone room header.
        var events = new byte[Bank80SystemState.EventByteCount];
        events[(int)EventNumber.ZebesTimebombSet >> 3] = unchecked((byte)(
            1 << ((int)EventNumber.ZebesTimebombSet & 7)));
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(
            bus,
            EscapeRoomHeader,
            new RoomStateSelectionContext(events, 0, false, false));
        if (room.State.Pointer != EscapeRoomState ||
            room.State.EnemyPopulationPointer != EscapePopulation)
        {
            throw new InvalidDataException(
                $"Escape room selected state ${room.State.Pointer:X4} and population " +
                $"${room.State.EnemyPopulationPointer:X4}.");
        }
        return room;
    }

    private static void VerifyInitializationPalettesAndDrawing(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedEscapeAnimals loaded = Load(bus, room, assets);
        if (loaded.Enemies.EnemyCount != 4)
            throw new InvalidDataException($"Escape room loaded {loaded.Enemies.EnemyCount} actors.");

        RoomEnemySlot dachora = loaded.Enemies.Slots[0];
        if (dachora.EnemyDefinitionPointer != EscapeDachoraDefinition ||
            dachora.XPosition != 0x00e0 || dachora.YPosition != 0x00b8 ||
            dachora.Properties != 0x2400 || dachora.CurrentInstruction != 0xe964 ||
            dachora.SpritemapPointer != 0x804d || dachora.InstructionTimer != 1 ||
            loaded.Enemies.EscapeDachoraStates[0] is null)
        {
            throw new InvalidDataException(
                $"Escape Dachora initialization failed: position=({dachora.XPosition:X4}," +
                $"{dachora.YPosition:X4}), properties=${dachora.Properties:X4}, list/map=" +
                $"${dachora.CurrentInstruction:X4}/${dachora.SpritemapPointer:X4}.");
        }

        ushort[] expectedX = [0x0080, 0x00a0, 0x00e8];
        ushort[] expectedLists = [0xe556, 0xe582, 0xe5c6];
        ushort[] expectedSpeeds = [0xfe00, 0x0280, 0x0000];
        EscapeEtecoonPreInstruction[] expectedPreInstructions =
        [
            EscapeEtecoonPreInstruction.WalkAndFall,
            EscapeEtecoonPreInstruction.WalkAndFall,
            EscapeEtecoonPreInstruction.WaitForEscapeEvent,
        ];
        for (int role = 0; role < 3; role++)
        {
            RoomEnemySlot actor = loaded.Enemies.Slots[role + 1];
            EscapeEtecoonEnemyState state = RequireEtecoonState(loaded.Enemies, actor);
            if (actor.EnemyDefinitionPointer != EscapeEtecoonDefinition ||
                actor.XPosition != expectedX[role] || actor.YPosition != 0x00c8 ||
                actor.Properties != 0xa400 || actor.CurrentInstruction != expectedLists[role] ||
                actor.InstructionTimer != 1 || actor.Parameter1 != role * 2 ||
                state.HorizontalSpeed != expectedSpeeds[role] ||
                state.PreInstruction != expectedPreInstructions[role])
            {
                throw new InvalidDataException(
                    $"Escape Etecoon role {role} initialization failed: position=" +
                    $"({actor.XPosition:X4},{actor.YPosition:X4}), properties=" +
                    $"${actor.Properties:X4}, list=${actor.CurrentInstruction:X4}, " +
                    $"speed=${state.HorizontalSpeed:X4}, pre={state.PreInstruction}.");
            }
        }

        // Dachora retains the graphics-set palette index ($0E00 / OBJ row seven).
        // Etecoon deliberately overwrites its actor palette index with zero in $B3:E6E8,
        // even though ProcessEnemyTilesets still copies header palette $E525 into the row
        // selected by graphics-set destination one. Verify both independent native writes.
        if (!PaletteMatches(loaded.Cgram, dachora.PaletteIndex, bus, 0xe944) ||
            !PaletteMatches(loaded.Cgram, 0x0200, bus, 0xe525))
        {
            throw new InvalidDataException(
                "Escape-animal CGRAM rows do not match ROM palettes: " +
                $"Dachora palette=${dachora.PaletteIndex:X4}, Etecoon palette=" +
                $"${loaded.Enemies.Slots[1].PaletteIndex:X4}, graphics=[" +
                string.Join(
                    ", ",
                    loaded.Enemies.GraphicsSet.Select(entry =>
                        $"${entry.DefinitionPointer:X4}@${entry.VramDestination:X4}")) + "].");
        }

        Step(loaded);
        ushort[] expectedFirstMaps = [0xeb1b, 0xe736, 0xe75f, 0xe8b0];
        for (int index = 0; index < expectedFirstMaps.Length; index++)
        {
            if (loaded.Enemies.Slots[index].SpritemapPointer != expectedFirstMaps[index])
            {
                throw new InvalidDataException(
                    $"Escape actor {index} emitted map " +
                    $"${loaded.Enemies.Slots[index].SpritemapPointer:X4}, expected " +
                    $"${expectedFirstMaps[index]:X4}.");
            }
        }

        var oam = new OamBuffer();
        oam.BeginFrame();
        loaded.Enemies.DrawLayers(oam, 0, 0, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
            throw new InvalidDataException("Escape animals emitted no live ROM OBJ pieces.");
    }

    private static void VerifyLavaBranches(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedEscapeAnimals normal = Load(bus, room, assets);
        Step(normal);
        RoomEnemySlot normalEtecoon = normal.Enemies.Slots[1];
        if (normalEtecoon.SpritemapPointer != 0xe736 ||
            normalEtecoon.InstructionTimer != 5)
        {
            throw new InvalidDataException(
                "Escape Etecoon's high-lava/default branch did not select its five-frame list.");
        }

        LoadedEscapeAnimals low = Load(bus, room, assets, lavaSurface: LowLavaSurface);
        Step(low);
        RoomEnemySlot lowEtecoon = low.Enemies.Slots[1];
        if (lowEtecoon.SpritemapPointer != 0xe736 || lowEtecoon.InstructionTimer != 3 ||
            lowEtecoon.CurrentInstruction != 0xe572)
        {
            throw new InvalidDataException(
                $"Escape Etecoon's low-lava branch produced list/map/timer " +
                $"${lowEtecoon.CurrentInstruction:X4}/${lowEtecoon.SpritemapPointer:X4}/" +
                $"{lowEtecoon.InstructionTimer}.");
        }

        // Dachora tests the same global surface after completing the six-map left half.
        // The low branch jumps to $E9FC and thereby enters two-frame animation. Observe the
        // actual timer instead of merely asserting that the operand word was decoded.
        bool sawFastDachoraFrame = false;
        for (int frame = 0; frame < 80; frame++)
        {
            Step(low);
            RoomEnemySlot dachora = low.Enemies.Slots[0];
            if (DachoraSpritemaps.Contains(dachora.SpritemapPointer) &&
                dachora.InstructionTimer == 2)
            {
                sawFastDachoraFrame = true;
                break;
            }
        }
        if (!sawFastDachoraFrame)
            throw new InvalidDataException("Escape Dachora never entered its low-lava two-frame list.");
    }

    private static void VerifyNativeMovementAndEscapeEvent(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedEscapeAnimals loaded = Load(bus, room, assets);
        RoomEnemySlot dachora = loaded.Enemies.Slots[0];
        RoomEnemySlot leftEtecoon = loaded.Enemies.Slots[1];
        RoomEnemySlot rightEtecoon = loaded.Enemies.Slots[2];
        ushort initialDachoraX = dachora.XPosition;
        bool dachoraMovedLeft = false;
        bool dachoraReturnedRight = false;
        bool leftEtecoonReversed = false;
        bool rightEtecoonReversed = false;
        var seenDachoraMaps = new HashSet<ushort>();

        for (int frame = 0; frame < 220; frame++)
        {
            Step(loaded);
            if (DachoraSpritemaps.Contains(dachora.SpritemapPointer))
                seenDachoraMaps.Add(dachora.SpritemapPointer);
            dachoraMovedLeft |= unchecked((short)(dachora.XPosition - initialDachoraX)) < 0;
            dachoraReturnedRight |= dachoraMovedLeft && dachora.XPosition >= initialDachoraX;
            leftEtecoonReversed |= unchecked((short)RequireEtecoonState(
                loaded.Enemies,
                leftEtecoon).HorizontalSpeed) > 0;
            rightEtecoonReversed |= unchecked((short)RequireEtecoonState(
                loaded.Enemies,
                rightEtecoon).HorizontalSpeed) < 0;
            if (dachoraReturnedRight && leftEtecoonReversed && rightEtecoonReversed &&
                seenDachoraMaps.SetEquals(DachoraSpritemaps))
            {
                break;
            }
        }

        if (!dachoraMovedLeft || !dachoraReturnedRight ||
            !leftEtecoonReversed || !rightEtecoonReversed ||
            !seenDachoraMaps.SetEquals(DachoraSpritemaps))
        {
            throw new InvalidDataException(
                $"Escape-animal normal movement incomplete: dachora=" +
                $"{dachoraMovedLeft}/{dachoraReturnedRight}, reversals=" +
                $"{leftEtecoonReversed}/{rightEtecoonReversed}, maps=" +
                $"[{FormatWords(seenDachoraMaps)}].");
        }

        // Event $0F is set after room load by the animal-rescue PLM. It must not be confused
        // with event $0E, which selected this room state. Dachora branches to $EA34; the
        // first two Etecoons switch at their next wall collision, while role two immediately
        // enters $E5DA and later installs the shared $E65C rightward escape pre-instruction.
        loaded.System.SetEvent((int)EventNumber.CrittersEscaped);
        bool sawDachoraEscapeList = false;
        ushort dachoraXAtEvent = dachora.XPosition;
        for (int frame = 0; frame < 900; frame++)
        {
            Step(loaded);
            sawDachoraEscapeList |= dachora.CurrentInstruction is >= 0xea34 and < 0xeaa8;
            bool allEtecoonsEscaping = loaded.Enemies.Slots
                .Skip(1)
                .Take(3)
                .Select(actor => RequireEtecoonState(loaded.Enemies, actor))
                .All(state => state.PreInstruction == EscapeEtecoonPreInstruction.EscapeRight);
            if (sawDachoraEscapeList && allEtecoonsEscaping &&
                unchecked((short)(dachora.XPosition - dachoraXAtEvent)) > 0)
            {
                return;
            }
        }

        throw new InvalidDataException(
            "Event $0F did not drive Dachora and all three Etecoons into their authored escape paths.");
    }

    private static void VerifyAlreadyEscapedLoadGate(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedEscapeAnimals loaded = Load(bus, room, assets, crittersEscapedAtLoad: true);
        if (loaded.Enemies.Slots.Take(4).Any(actor =>
            !actor.Properties.HasAny(EnemyProperties.Deleted)))
        {
            throw new InvalidDataException(
                "An escape animal survived initialization after event $0F was already set.");
        }

        Step(loaded);
        if (loaded.Enemies.Slots.Take(4).Any(actor => actor.EnemyDefinitionPointer != 0))
        {
            throw new InvalidDataException(
                "Deleted escape-animal slots were not retired by the ordinary scheduler.");
        }
    }

    private static void VerifyAuthoredNonCombatCallbacks(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        LoadedEscapeAnimals loaded = Load(bus, room, assets);
        Step(loaded);
        ushort[] health = loaded.Enemies.Slots.Take(4).Select(actor => actor.Health).ToArray();
        ushort samusHealth = loaded.Samus.Health;

        foreach (RoomEnemySlot actor in loaded.Enemies.Slots.Take(4))
        {
            if (loaded.Enemies.ResolveOrdinarySamusContact(loaded.Samus, actor.NativeIndex))
                throw new InvalidDataException("An escape animal synthesized contact damage.");
        }

        var projectiles = new SamusProjectileSystem();
        var sharedProjectiles = new SamusBombProjectileSystem();
        ArmProjectile(projectiles.Slots[0], loaded.Enemies.Slots[1]);
        int shots = loaded.Enemies.ResolveOrdinaryProjectileHits(
            bus,
            projectiles,
            sharedProjectiles,
            loaded.Samus);
        int powerBombCallbacks = loaded.Enemies.ResolveOrdinaryPowerBombHits(
            bus,
            loaded.Enemies.Slots[1].XPosition,
            loaded.Enemies.Slots[1].YPosition,
            explosionRadius: 1);
        GrappleEnemyCollision grapple = loaded.Enemies.ResolveGrappleEndpoint(
            loaded.Enemies.Slots[1].XPosition,
            loaded.Enemies.Slots[1].YPosition);

        if (shots != 0 || powerBombCallbacks < 1 || grapple.Collided ||
            loaded.Samus.Health != samusHealth ||
            !loaded.Enemies.Slots.Take(4).Select(actor => actor.Health).SequenceEqual(health))
        {
            throw new InvalidDataException(
                $"Escape-animal no-combat callbacks failed: shots={shots}, powerBombs=" +
                $"{powerBombCallbacks}, grapple={grapple}, Samus={samusHealth}->" +
                $"{loaded.Samus.Health}, enemyHealth=[{string.Join(',', health)}]->" +
                $"[{string.Join(',', loaded.Enemies.Slots.Take(4).Select(actor => actor.Health))}].");
        }
    }

    private static LoadedEscapeAnimals Load(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        ushort? lavaSurface = null,
        bool crittersEscapedAtLoad = false)
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var system = new Bank80SystemState();
        system.SetEvent((int)EventNumber.ZebesTimebombSet);
        if (crittersEscapedAtLoad)
            system.SetEvent((int)EventNumber.CrittersEscaped);

        var samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            Pose = SamusPoseIds.FacingRightNormalPose,
            XPosition = 0x0040,
            YPosition = 0x00b8,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        if (lavaSurface.HasValue)
            samus.LiquidPhysics.ConfigureLavaAcid(lavaSurface.Value);

        var enemies = new RoomEnemySystem();
        enemies.Load(
            bus,
            EscapePopulation,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            system.NextRandom,
            system.SetRandomNumber,
            readRandomNumber: () => system.RandomNumber,
            level: assets.LevelData,
            samus: samus,
            hasEvent: system.HasEvent,
            setEvent: system.SetEvent,
            clearEvent: system.ClearEvent);
        return new LoadedEscapeAnimals(enemies, samus, system, cgram, assets.LevelData);
    }

    private static void Step(LoadedEscapeAnimals loaded) =>
        loaded.Enemies.StepFrame(
            0,
            0,
            false,
            loaded.Samus,
            level: loaded.Level);

    private static bool PaletteMatches(
        SnesCgram cgram,
        ushort paletteIndex,
        ISnesAddressSpace bus,
        ushort sourcePointer)
    {
        int destination = 128 + ((paletteIndex >> 9) & 7) * 16;
        for (int color = 0; color < 16; color++)
        {
            if (cgram.Colors[destination + color] !=
                ReadWord(bus, 0xb30000 | unchecked((ushort)(sourcePointer + color * 2))))
            {
                return false;
            }
        }
        return true;
    }

    private static EscapeEtecoonEnemyState RequireEtecoonState(
        RoomEnemySystem enemies,
        RoomEnemySlot actor) =>
        enemies.EscapeEtecoonStates[actor.SlotIndex] ?? throw new InvalidDataException(
            $"Escape Etecoon slot {actor.SlotIndex} has no typed state.");

    private static void ArmProjectile(SamusProjectileSlot projectile, RoomEnemySlot target)
    {
        projectile.ClearFields();
        projectile.Type = 0x0100;
        projectile.Damage = 100;
        projectile.Direction = 2;
        projectile.XPosition = target.XPosition;
        projectile.YPosition = target.YPosition;
        projectile.XRadius = 4;
        projectile.YRadius = 4;
        projectile.InstructionPointer = 0x9000;
        projectile.InstructionTimer = 1;
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));

    private static string FormatWords(IEnumerable<ushort> words) =>
        string.Join(',', words.Order().Select(word => $"${word:X4}"));

    private readonly record struct EscapeAnimalRecord(
        ushort Definition,
        ushort X,
        ushort Y,
        ushort InitializationParameter,
        ushort Properties,
        ushort ExtraProperties,
        ushort Parameter1,
        ushort Parameter2);

    private sealed record LoadedEscapeAnimals(
        RoomEnemySystem Enemies,
        SamusState Samus,
        Bank80SystemState System,
        SnesCgram Cgram,
        RoomLevelData Level);
}
