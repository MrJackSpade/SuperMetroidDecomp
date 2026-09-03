using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>
/// Retail-cartridge audit for the palette-only N00b Tube crack actor and both variants of
/// enemy $F0FF. Every population, state, callback, palette, and projectile definition below
/// is reread from the supplied ROM before the translated instruction streams are exercised.
/// </summary>
internal static class ChozoStatueAudit
{
    private const ushort N00bTubeRoomPointer = 0xcefb;
    private const ushort N00bTubePopulationPointer = 0xd529;
    private const ushort LowerNorfairRoomPointer = 0xb1e5;
    private const ushort LowerNorfairStatePointer = 0xb1f2;
    private const ushort LowerNorfairPopulationPointer = 0xa23c;
    private const ushort WreckedShipRoomPointer = 0xc98e;
    private const ushort WreckedShipLiveStatePointer = 0xc9a0;
    private const ushort WreckedShipLivePopulationPointer = 0xbe93;
    private const ushort WreckedShipDefeatedStatePointer = 0xc9ba;
    private const ushort WreckedShipDefeatedPopulationPointer = 0xc1ae;
    private const ushort N00bTubeDefinition = 0xf0bf;
    private const ushort ChozoStatueDefinition = 0xf0ff;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        VerifyRetailHeadersPopulationsAndStates(bus);
        VerifyN00bTubePaletteActor(bus);
        VerifyLowerNorfairSequence(bus);
        VerifyWreckedShipSequence(bus);
        Console.WriteLine(
            "Chozo statue audit passed: the N00b Tube palette actor and both retail $F0FF " +
            "variants loaded ROM palettes/lists, honored hand-trigger and boss gates, moved " +
            "through room collision while pinning Samus, emitted authored sounds/FX/PLM and " +
            "footstep-projectile requests, restored controls, updated scroll bytes, and rendered OBJ.");
        return 0;
    }

    private static void VerifyRetailHeadersPopulationsAndStates(ISnesAddressSpace bus)
    {
        RoomEnemyDefinition cracks = RoomEnemySystem.ReadDefinition(bus, N00bTubeDefinition);
        AssertDefinition(
            cracks,
            bank: 0xaa,
            health: 20,
            damage: 40,
            initialization: 0xe716,
            main: 0x804c,
            touch: 0x804c,
            shot: 0x804c,
            "N00b Tube cracks");

        RoomEnemyDefinition statue = RoomEnemySystem.ReadDefinition(bus, ChozoStatueDefinition);
        AssertDefinition(
            statue,
            bank: 0xaa,
            health: 20,
            damage: 40,
            initialization: 0xe725,
            main: 0xe7a7,
            touch: 0xe7db,
            shot: 0xe7dc,
            "Chozo statue");

        VerifyPopulationRecord(
            bus,
            N00bTubePopulationPointer,
            [N00bTubeDefinition, 0x0080, 0x0100, 0, 0x2200, 0, 0, 0]);
        VerifyPopulationRecord(
            bus,
            LowerNorfairPopulationPointer,
            [ChozoStatueDefinition, 0x002c, 0x009a, 0, 0x2000, 0, 0, 2]);
        VerifyPopulationRecord(
            bus,
            WreckedShipLivePopulationPointer,
            [ChozoStatueDefinition, 0x04c8, 0x018a, 0, 0x2000, 0, 0, 0]);
        VerifyPopulationRecord(
            bus,
            WreckedShipDefeatedPopulationPointer,
            [ChozoStatueDefinition, 0x04c8, 0x018a, 0, 0x2000, 0, 0, 0]);

        CartridgeRoomHeader lower = CartridgeRoomHeader.Load(bus, LowerNorfairRoomPointer);
        if (lower.State.Pointer != LowerNorfairStatePointer ||
            lower.State.EnemyPopulationPointer != LowerNorfairPopulationPointer)
        {
            throw new InvalidDataException(
                $"Lower Norfair Chozo state mismatch: ${lower.State.Pointer:X4}/" +
                $"${lower.State.EnemyPopulationPointer:X4}.");
        }

        CartridgeRoomHeader wreckedLive = CartridgeRoomHeader.Load(bus, WreckedShipRoomPointer);
        if (wreckedLive.State.Pointer != WreckedShipLiveStatePointer ||
            wreckedLive.State.EnemyPopulationPointer != WreckedShipLivePopulationPointer)
        {
            throw new InvalidDataException(
                $"Live Wrecked Ship state mismatch: ${wreckedLive.State.Pointer:X4}/" +
                $"${wreckedLive.State.EnemyPopulationPointer:X4}.");
        }

        var defeatedSelection = new RoomStateSelectionContext(
            ReadOnlyMemory<byte>.Empty,
            BossBits: 1,
            HasMorphBallAndMissiles: false,
            HasPowerBombs: false);
        CartridgeRoomHeader wreckedDefeated = CartridgeRoomHeader.Load(
            bus,
            WreckedShipRoomPointer,
            defeatedSelection);
        if (wreckedDefeated.State.Pointer != WreckedShipDefeatedStatePointer ||
            wreckedDefeated.State.EnemyPopulationPointer != WreckedShipDefeatedPopulationPointer)
        {
            throw new InvalidDataException(
                $"Defeated Wrecked Ship state mismatch: ${wreckedDefeated.State.Pointer:X4}/" +
                $"${wreckedDefeated.State.EnemyPopulationPointer:X4}.");
        }

        VerifyFootstepProjectileDefinition(
            bus,
            RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstep,
            expectedInstructionList: 0xaec4);
        VerifyFootstepProjectileDefinition(
            bus,
            RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstepAlternate,
            expectedInstructionList: 0xaedc);
    }

    private static void VerifyN00bTubePaletteActor(SuperMetroidAddressSpace bus)
    {
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, N00bTubeRoomPointer);
        if (room.State.EnemyPopulationPointer != N00bTubePopulationPointer)
        {
            throw new InvalidDataException(
                $"N00b Tube selected population ${room.State.EnemyPopulationPointer:X4}, " +
                $"expected ${N00bTubePopulationPointer:X4}.");
        }

        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        RoomEnemySystem enemies = CreateEncounter(
            bus,
            room,
            assets,
            bossDefeated: false,
            out _,
            out SnesCgram cgram,
            out _,
            out _,
            out _);
        RoomEnemySlot cracks = enemies.Slots[0];
        if (cracks.EnemyDefinitionPointer != N00bTubeDefinition ||
            !cracks.Properties.HasAny(EnemyProperties.Deleted))
        {
            throw new InvalidDataException(
                $"N00b Tube palette actor did not retain its one-shot deleted record: " +
                $"def=${cracks.EnemyDefinitionPointer:X4}, properties=${cracks.Properties:X4}.");
        }

        VerifyPalette(bus, cgram, destination: 144, source: 0xaae2dd, colorCount: 32);

        // $0200 makes the palette controller disappear at the next native activity scan.
        enemies.StepFrame(0, 0, false, samus: null, level: assets.LevelData);
        if (enemies.Slots[0].EnemyDefinitionPointer != 0)
            throw new InvalidDataException("N00b Tube palette actor survived its one-shot load frame.");
    }

    private static void VerifyLowerNorfairSequence(SuperMetroidAddressSpace bus)
    {
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, LowerNorfairRoomPointer);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        RoomEnemySystem enemies = CreateEncounter(
            bus,
            room,
            assets,
            bossDefeated: false,
            out SamusState samus,
            out SnesCgram cgram,
            out List<bool> controlWrites,
            out Dictionary<int, byte> scrollWrites,
            out List<int> events);
        RoomEnemySlot statue = RequireStatue(enemies, expectedParameter2: 2);
        ChozoStatueState state = enemies.ChozoStatueStates[0]
            ?? throw new InvalidDataException("Lower Norfair Chozo state wrapper is absent.");

        VerifyInitialStatue(
            bus,
            enemies,
            cgram,
            statue,
            state,
            expectedList: 0xe39d,
            expectedRowNineSource: 0xaae35d,
            expectedRowTenSource: 0xaae37d,
            expectedPlmHeader: 0xd6d6);

        // The initial one-frame list installs $E445 before sleeping. The hand-trigger seam
        // then reproduces $84:D18F's enemy/event/control writes and wakes that pre-instruction.
        enemies.StepFrame(0, 0, false, samus, level: assets.LevelData);
        if (state.PreInstruction != ChozoStatuePreInstruction.WaitForLowerNorfairHandTrigger)
            throw new InvalidDataException("Lower Norfair statue did not install pre-instruction $E445.");
        enemies.ActivateChozoStatueHandTrigger(assets.LevelData);
        if (statue.Parameter1 != 1 || controlWrites.LastOrDefault() || !events.Contains(0x0c))
            throw new InvalidDataException("Lower Norfair hand trigger lost parameter/event/control side effects.");

        var maps = new HashSet<ushort>();
        var positions = new HashSet<(ushort X, ushort Y)>();
        RunUntilSleep(
            enemies,
            assets.LevelData,
            samus,
            statue,
            sleepPointer: 0xe427,
            cameraX: 0,
            cameraY: 0,
            maps,
            positions,
            stepProjectiles: false);

        // This room's retail collision keeps the controller's origin pinned while its large
        // authored maps and joint tables move Samus. Do not require a host-visible origin
        // delta: the final table offset and exact joint assertion below prove $E5D8 ran.
        if (state.MovementTableOffset != 0x0020 || maps.Count < 8 ||
            enemies.LastChozoStatueSoundEffect != 0x001c ||
            enemies.ChozoStatueFxTimer != 32 ||
            enemies.ChozoStatueFxYVelocity != 64 ||
            enemies.ChozoStatueFxBaseYPosition != 722 ||
            !enemies.ChozoStatueSamusControlsEnabled || !controlWrites.LastOrDefault() ||
            scrollWrites.Count != 0)
        {
            throw new InvalidDataException(
                $"Lower Norfair sequence incomplete: offset=${state.MovementTableOffset:X4}, " +
                $"maps={maps.Count}, positions={positions.Count}, sound=" +
                $"${enemies.LastChozoStatueSoundEffect.GetValueOrDefault():X4}, FX=" +
                $"{enemies.ChozoStatueFxTimer}/{enemies.ChozoStatueFxYVelocity}/" +
                $"{enemies.ChozoStatueFxBaseYPosition}, controls=" +
                $"{enemies.ChozoStatueSamusControlsEnabled}, scrolls={scrollWrites.Count}.");
        }

        VerifyPinnedSamus(bus, statue, state, samus);
        VerifyStatueDraws(enemies, cameraX: 0, cameraY: 0);
    }

    private static void VerifyWreckedShipSequence(SuperMetroidAddressSpace bus)
    {
        var defeatedSelection = new RoomStateSelectionContext(
            ReadOnlyMemory<byte>.Empty,
            BossBits: 1,
            HasMorphBallAndMissiles: false,
            HasPowerBombs: false);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(
            bus,
            WreckedShipRoomPointer,
            defeatedSelection);
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
        RoomEnemySystem enemies = CreateEncounter(
            bus,
            room,
            assets,
            bossDefeated: true,
            out SamusState samus,
            out SnesCgram cgram,
            out List<bool> controlWrites,
            out Dictionary<int, byte> scrollWrites,
            out _);
        RoomEnemySlot statue = RequireStatue(enemies, expectedParameter2: 0);
        ChozoStatueState state = enemies.ChozoStatueStates[0]
            ?? throw new InvalidDataException("Wrecked Ship Chozo state wrapper is absent.");

        VerifyInitialStatue(
            bus,
            enemies,
            cgram,
            statue,
            state,
            expectedList: 0xe457,
            expectedRowNineSource: 0xaae31d,
            expectedRowTenSource: 0xaae33d,
            expectedPlmHeader: 0xd6ee);
        if (!enemies.ChozoStatuePlmRequests.Any(request =>
                request.HeaderPointer == 0xd6fc && request.IsHardcoded &&
                request.BlockX == 0x17 && request.BlockY == 0x1d))
        {
            throw new InvalidDataException("Wrecked Ship initializer omitted hardcoded PLM $D6FC.");
        }

        // $84:D3F4 is the callback in initial PLM $D6FC. Applying its exact two block writes
        // gives the enemy instruction audit the same spike terrain that retail sees.
        ApplyWreckedShipSpikeTerrain(assets.LevelData);

        enemies.StepFrame(0x0400, 0x0100, false, samus, level: assets.LevelData);
        if (state.PreInstruction != ChozoStatuePreInstruction.WaitForWreckedShipHandTrigger)
            throw new InvalidDataException("Wrecked Ship statue did not install pre-instruction $E7AE.");
        enemies.ActivateChozoStatueHandTrigger(assets.LevelData);
        if (statue.Parameter1 != 1 || controlWrites.LastOrDefault())
            throw new InvalidDataException("Wrecked Ship hand trigger failed to disable controls.");
        if (!scrollWrites.TryGetValue(7, out byte triggerScroll7) || triggerScroll7 != 2 ||
            !scrollWrites.TryGetValue(8, out byte triggerScroll8) || triggerScroll8 != 2 ||
            !scrollWrites.TryGetValue(13, out byte triggerScroll13) || triggerScroll13 != 1 ||
            !scrollWrites.TryGetValue(14, out byte triggerScroll14) || triggerScroll14 != 1 ||
            !enemies.ChozoStatuePlmRequests.Any(request =>
                request.HeaderPointer == 0xd6f8 && request.IsHardcoded))
        {
            throw new InvalidDataException(
                "Wrecked Ship hand trigger lost its green/blue scroll stores or PLM $D6F8 request.");
        }

        var maps = new HashSet<ushort>();
        var positions = new HashSet<(ushort X, ushort Y)>();
        bool sawFootstepProjectile = false;
        int maximumActiveProjectiles = 0;
        RunUntilSleep(
            enemies,
            assets.LevelData,
            samus,
            statue,
            sleepPointer: 0xe57d,
            cameraX: 0x0400,
            cameraY: 0x0100,
            maps,
            positions,
            stepProjectiles: true,
            afterFrame: () =>
            {
                sawFootstepProjectile |= enemies.EnemyProjectiles.Any(projectile =>
                    projectile.Kind == RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstep);
                maximumActiveProjectiles = Math.Max(
                    maximumActiveProjectiles,
                    enemies.ActiveEnemyProjectileCount);
            });

        bool hasFootstepPlm = enemies.ChozoStatuePlmRequests.Any(request =>
            request.HeaderPointer == 0xd113 && !request.IsHardcoded);
        int spikeTerrainRequestCount = enemies.ChozoStatuePlmRequests.Count(request =>
            request.HeaderPointer == 0xd6fc && request.IsHardcoded);
        if (state.MovementTableOffset != 0 || maps.Count < 12 || positions.Count < 12 ||
            state.VariableA != 0xff00 || state.VariableB != 0x0100 ||
            enemies.LastChozoStatueSoundEffect != 0x004b ||
            !sawFootstepProjectile || maximumActiveProjectiles == 0 || !hasFootstepPlm ||
            spikeTerrainRequestCount < 2 ||
            !enemies.ChozoStatueSamusControlsEnabled || !controlWrites.LastOrDefault())
        {
            throw new InvalidDataException(
                $"Wrecked Ship sequence incomplete: offset=${state.MovementTableOffset:X4}, " +
                $"maps={maps.Count}, positions={positions.Count}, AB=${state.VariableA:X4}/" +
                $"${state.VariableB:X4}, sound=${enemies.LastChozoStatueSoundEffect.GetValueOrDefault():X4}, " +
                $"footstep={sawFootstepProjectile}/{hasFootstepPlm}, projectiles=" +
                $"{maximumActiveProjectiles}, D6FC={spikeTerrainRequestCount}, controls=" +
                $"{enemies.ChozoStatueSamusControlsEnabled}.");
        }

        byte[] expectedScrolls = [0, 0, 0, 0, 0, 1, 0];
        int[] indexes = [6, 7, 8, 9, 10, 13, 14];
        for (int index = 0; index < indexes.Length; index++)
        {
            if (!scrollWrites.TryGetValue(indexes[index], out byte actual) ||
                actual != expectedScrolls[index])
            {
                throw new InvalidDataException(
                    $"Wrecked Ship scroll byte {indexes[index]} is " +
                    $"{(scrollWrites.TryGetValue(indexes[index], out actual) ? actual : -1)}, " +
                    $"expected {expectedScrolls[index]}.");
            }
        }

        VerifyPinnedSamus(bus, statue, state, samus);
        // The walking controller crosses from its $04C8 origin to the room's left side.
        // Re-run the sleeping actor's activity/queue scan around its final position exactly
        // as the gameplay camera following pinned Samus would before asserting OBJ output.
        ushort finalCameraX = unchecked((ushort)Math.Max(0, statue.XPosition - 128));
        ushort finalCameraY = unchecked((ushort)Math.Max(0, statue.YPosition - 128));
        enemies.StepFrame(
            finalCameraX,
            finalCameraY,
            false,
            samus,
            level: assets.LevelData);
        VerifyStatueDraws(enemies, finalCameraX, finalCameraY);
    }

    private static RoomEnemySystem CreateEncounter(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        bool bossDefeated,
        out SamusState samus,
        out SnesCgram cgram,
        out List<bool> controlWrites,
        out Dictionary<int, byte> scrollWrites,
        out List<int> events)
    {
        var vram = new SnesVram();
        cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState(0x4a1d);
        samus = new SamusState
        {
            Health = 999,
            MaxHealth = 999,
            XPosition = 0x0080,
            YPosition = 0x0080,
            Pose = SamusPoseIds.FacingRightNormalPose,
        };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        controlWrites = new List<bool>();
        scrollWrites = new Dictionary<int, byte>();
        events = new List<int>();
        List<bool> capturedControls = controlWrites;
        Dictionary<int, byte> capturedScrolls = scrollWrites;
        List<int> capturedEvents = events;

        var enemies = new RoomEnemySystem();
        // Three of the four populations continue into unrelated enemy families. Keep the
        // first untouched retail record and overlay only the following terminator, exactly as
        // InitializeEnemies would see a singleton diagnostic population. All actor code,
        // lists, palettes, spritemaps, terrain, and graphics still come from the cartridge.
        var actorBus = new PopulationPrefixAddressSpace(
            bus,
            room.State.EnemyPopulationPointer,
            retainedRecordCount: 1,
            deathQuota: 1);
        enemies.Load(
            actorBus,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            vram,
            cgram,
            random.NextRandom,
            random.SetRandomNumber,
            readRandomNumber: () => random.RandomNumber,
            level: assets.LevelData,
            samus: samus,
            isAreaBossDefeated: () => bossDefeated,
            setEvent: eventNumber => capturedEvents.Add((int)eventNumber),
            setSamusControlsEnabled: capturedControls.Add,
            setRoomScrollByte: (index, value) => capturedScrolls[index] = value);
        return enemies;
    }

    private static void RunUntilSleep(
        RoomEnemySystem enemies,
        RoomLevelData level,
        SamusState samus,
        RoomEnemySlot statue,
        ushort sleepPointer,
        ushort cameraX,
        ushort cameraY,
        HashSet<ushort> maps,
        HashSet<(ushort X, ushort Y)> positions,
        bool stepProjectiles,
        Action? afterFrame = null)
    {
        for (int frame = 0; frame < 5000; frame++)
        {
            enemies.StepFrame(cameraX, cameraY, false, samus, level: level);
            if (stepProjectiles)
            {
                enemies.StepEnemyProjectiles(
                    level,
                    samus,
                    cameraX: cameraX,
                    cameraY: cameraY,
                    nmiFrameCounter8: unchecked((byte)frame));
            }
            maps.Add(statue.SpritemapPointer);
            positions.Add((statue.XPosition, statue.YPosition));
            afterFrame?.Invoke();
            if (statue.CurrentInstruction == sleepPointer && statue.InstructionTimer == 0)
                return;
        }

        throw new InvalidDataException(
            $"Chozo statue did not reach sleep command $AA:{sleepPointer:X4} within 5000 frames; " +
            $"current=${statue.CurrentInstruction:X4}, timer=${statue.InstructionTimer:X4}.");
    }

    private static void VerifyInitialStatue(
        ISnesAddressSpace bus,
        RoomEnemySystem enemies,
        SnesCgram cgram,
        RoomEnemySlot statue,
        ChozoStatueState state,
        ushort expectedList,
        int expectedRowNineSource,
        int expectedRowTenSource,
        ushort expectedPlmHeader)
    {
        if (statue.Properties != 0xa800 || statue.Layer != 0 || statue.PaletteIndex != 0 ||
            statue.CurrentInstruction != expectedList || statue.InstructionTimer != 1 ||
            statue.Parameter1 != 0 || state.PreInstruction != ChozoStatuePreInstruction.Idle)
        {
            throw new InvalidDataException(
                $"Chozo initializer mismatch: props=${statue.Properties:X4}, layer={statue.Layer}, " +
                $"palette=${statue.PaletteIndex:X4}, list=${statue.CurrentInstruction:X4}, " +
                $"timer={statue.InstructionTimer}, parameter1={statue.Parameter1}, " +
                $"pre=${(ushort)state.PreInstruction:X4}.");
        }
        VerifyPalette(bus, cgram, 144, expectedRowNineSource, 16);
        VerifyPalette(bus, cgram, 160, expectedRowTenSource, 16);
        if (!enemies.ChozoStatuePlmRequests.Any(request =>
                request.HeaderPointer == expectedPlmHeader && request.IsHardcoded))
        {
            throw new InvalidDataException(
                $"Chozo initializer omitted hardcoded PLM ${expectedPlmHeader:X4}.");
        }
    }

    private static RoomEnemySlot RequireStatue(RoomEnemySystem enemies, ushort expectedParameter2)
    {
        RoomEnemySlot statue = enemies.Slots[0];
        if (enemies.EnemyCount != 1 || statue.EnemyDefinitionPointer != ChozoStatueDefinition ||
            statue.Parameter2 != expectedParameter2)
        {
            throw new InvalidDataException(
                $"Chozo population mismatch: count={enemies.EnemyCount}, " +
                $"definition=${statue.EnemyDefinitionPointer:X4}, parameter2=${statue.Parameter2:X4}.");
        }
        return statue;
    }

    private static void VerifyPinnedSamus(
        ISnesAddressSpace bus,
        RoomEnemySlot statue,
        ChozoStatueState state,
        SamusState samus)
    {
        ushort offset = state.MovementTableOffset;
        ushort expectedX = unchecked((ushort)(
            statue.XPosition + unchecked((short)ReadWord(bus, 0xaae670 + offset))));
        ushort expectedY = unchecked((ushort)(
            statue.YPosition + unchecked((short)ReadWord(bus, 0xaae6b0 + offset))));
        if (samus.XPosition != expectedX || samus.YPosition != expectedY)
        {
            throw new InvalidDataException(
                $"Chozo statue lost Samus joint ownership: ({samus.XPosition:X4}," +
                $"{samus.YPosition:X4}) != ({expectedX:X4},{expectedY:X4}).");
        }
    }

    private static void VerifyStatueDraws(RoomEnemySystem enemies, ushort cameraX, ushort cameraY)
    {
        var oam = new OamBuffer();
        oam.BeginFrame();
        enemies.DrawLayers(oam, cameraX, cameraY, 0, 7);
        oam.FinalizeFrame();
        if (oam.LastFinalizedSpriteCount == 0)
        {
            RoomEnemySlot statue = enemies.Slots[0];
            throw new InvalidDataException(
                $"Chozo statue emitted no OBJ pieces at its final authored map; position=" +
                $"({statue.XPosition:X4},{statue.YPosition:X4}), map=${statue.SpritemapPointer:X4}, " +
                $"layer={statue.Layer}, properties=${statue.Properties:X4}, camera=" +
                $"({cameraX:X4},{cameraY:X4}).");
        }
    }

    private static void ApplyWreckedShipSpikeTerrain(RoomLevelData level)
    {
        foreach (int byteOffset in new[] { 0x1608, 0x160a })
        {
            int blockIndex = byteOffset >> 1;
            RoomCollisionBlock block = level.GetCollisionBlockByIndex(blockIndex);
            level.SetForegroundEntry(blockIndex, unchecked((ushort)((block.LevelWord & 0x0fff) | 0xa000)));
            level.SetBehavior(blockIndex, 0);
        }
    }

    private static void VerifyFootstepProjectileDefinition(
        ISnesAddressSpace bus,
        RoomEnemyProjectileKind kind,
        ushort expectedInstructionList)
    {
        int address = 0x860000 | (ushort)kind;
        if (ReadWord(bus, address) != 0xaefc ||
            ReadWord(bus, address + 2) != 0x84fb ||
            ReadWord(bus, address + 4) != expectedInstructionList ||
            ReadWord(bus, address + 6) != 0 ||
            ReadWord(bus, address + 8) != 0x3000)
        {
            throw new InvalidDataException(
                $"Chozo footstep projectile definition ${(ushort)kind:X4} is not retail data.");
        }
    }

    private static void VerifyPalette(
        ISnesAddressSpace bus,
        SnesCgram cgram,
        int destination,
        int source,
        int colorCount)
    {
        for (int color = 0; color < colorCount; color++)
        {
            ushort expected = ReadWord(bus, source + color * 2);
            ushort actual = cgram.Colors[destination + color];
            if (actual != expected)
            {
                throw new InvalidDataException(
                    $"Palette color {destination + color} is ${actual:X4}, expected " +
                    $"${expected:X4} from ${source + color * 2:X6}.");
            }
        }
    }

    private static void VerifyPopulationRecord(
        ISnesAddressSpace bus,
        ushort populationPointer,
        ushort[] expected)
    {
        for (int word = 0; word < expected.Length; word++)
        {
            ushort actual = ReadWord(bus, 0xa10000 | (populationPointer + word * 2));
            if (actual != expected[word])
            {
                throw new InvalidDataException(
                    $"Population $A1:{populationPointer:X4} word {word} is ${actual:X4}, " +
                    $"expected ${expected[word]:X4}.");
            }
        }
    }

    private static void AssertDefinition(
        RoomEnemyDefinition definition,
        byte bank,
        ushort health,
        ushort damage,
        ushort initialization,
        ushort main,
        ushort touch,
        ushort shot,
        string name)
    {
        if (definition.Bank != bank || definition.Health != health || definition.Damage != damage ||
            definition.InitializationAiPointer != initialization || definition.MainAiPointer != main ||
            definition.TouchAiPointer != touch || definition.ShotAiPointer != shot)
        {
            throw new InvalidDataException(
                $"{name} header mismatch: bank=${definition.Bank:X2}, health/damage=" +
                $"{definition.Health}/{definition.Damage}, init/main=" +
                $"${definition.InitializationAiPointer:X4}/${definition.MainAiPointer:X4}, " +
                $"touch/shot=${definition.TouchAiPointer:X4}/${definition.ShotAiPointer:X4}.");
        }
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
}
