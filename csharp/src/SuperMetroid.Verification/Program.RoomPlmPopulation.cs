using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>
    /// Locks the single bank-$8F parser, descending physical IDs, synchronous slot reuse,
    /// elevator animation, and all requested station setup/contact families to bounded
    /// synthetic rooms. No assertion crosses a door or depends on frontend choreography.
    /// </summary>
    private static void VerifySequentialRoomPlmPopulationLoader()
    {
        var bus = new TestAddressSpace();
        SeedRoomPlmPopulationRom(bus);
        const ushort population = 0x9000;
        bus.WriteBytes(0x8f0000 | population, [
            // First resident takes native slot 39.
            0x03, 0xb7, 0x08, 0x04, 0x00, 0x91,
            // Setup deletes this temporary extension synchronously.
            0x3b, 0xb6, 0x09, 0x04, 0x00, 0x80,
            // It must reuse slot 38, not receive a slot based on family load order.
            0x0b, 0xb7, 0x0c, 0x08, 0x00, 0x00,
            // The following resident then receives slot 37.
            0xdf, 0xb6, 0x14, 0x08, 0x00, 0x00,
            0x00, 0x00,
        ]);

        const int width = 32;
        const int height = 16;
        RoomLevelData level = CreateRoom(
            width,
            height,
            new ushort[width * height],
            new byte[width * height],
            blockDefinitions: new byte[0x400 * 8]);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var system = new Bank80SystemState();
        var samus = new SamusState
        {
            Health = 25,
            MaxHealth = 199,
            Missiles = 1,
            MaxMissiles = 15,
        };
        var plms = new RoomPlmSystem();
        int parsed = plms.LoadRoomPopulation(
            bus,
            level,
            streamer,
            new SnesVram(),
            population,
            system,
            areaIndex: AreaId.Crateria,
            getSamus: () => samus,
            isAreaTorizoDefeated: () => false);

        AssertEqual(4, parsed, "single parser consumes every six-byte record once");
        RoomPlmSlotSnapshot[] slots = plms.PopulationSlots.ToArray();
        AssertEqual(3, slots.Length, "extension setup deletes its preallocated actor");
        AssertEqual(39, slots[0].NativeSlotIndex, "first ROM record receives highest native slot");
        AssertEqual(0xb703, slots[0].HeaderPointer, "first resident preserves record identity");
        AssertEqual(38, slots[1].NativeSlotIndex, "post-delete record reuses next native slot");
        AssertEqual(0xb70b, slots[1].HeaderPointer, "elevator follows ROM order, not family order");
        AssertEqual(37, slots[2].NativeSlotIndex, "later station receives descending slot");
        AssertEqual(0xb6df, slots[2].HeaderPointer, "station header remains observable");
        AssertEqual(1, plms.ElevatorPlatforms.Count, "elevator platform family is resident");
        AssertEqual(0x0000, level.GetCollisionBlock(12, 8).LevelWord,
            "elevator setup clears collision bits through shared setup dispatcher");

        // Energy right access is parent+1 and retains solid type B while publishing the
        // parent trigger. The next PLM pass refills through live Samus state and emits the
        // cartridge message ID; it does not require a room-name callback.
        AssertEqual(0xb000, level.GetCollisionBlock(21, 8).LevelWord,
            "energy setup installs type-B right access");
        AssertEqual(0x49, level.GetCollisionBlock(21, 8).Behavior,
            "energy setup installs BTS $49");
        AssertTrue(plms.TryNotifyStationTouch(level.GetBlockIndex(21, 8), 0x49),
            "generic BTS dispatcher locates energy-station parent");
        plms.Step(bus, level, streamer, 0, 0, 0);
        AssertTrue(samus.InputLocked,
            "station access command locks Samus during six-plus-$60 insertion");
        AssertTrue(plms.SoundRequests.Contains(new PlmSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x37), 6)),
            "station access begins with cartridge extension sound $37");
        for (int frame = 1; frame < 102; frame++)
            plms.Step(bus, level, streamer, 0, 0, 0);
        AssertEqual(199, samus.Health, "energy station restores health to cartridge maximum");
        AssertEqual(1, plms.StationActivationEvents.Count,
            "energy station publishes one shared message request");
        AssertEqual(GameplayMessageIds.EnergyRechargeCompleted,
            plms.StationActivationEvents[0].MessageBoxIndex,
            "energy station uses cartridge message $15");

        VerifyOtherStationFamilies(bus);
        VerifySaveStationConfirmation(bus);
        VerifyMetroidsClearedStatePlm(bus);
        VerifyMotherBrainEscapeRoomGate(bus);
        VerifyUnsupportedPopulationContext(bus);
        Console.WriteLine(
            "  Room PLM population: one-pass native slots, synchronous reuse, elevator, and station families agree.");
    }

    /// <summary>
    /// Exercises both native uses of header $C8CA: an ordinary room load leaves its
    /// six-frame closed gate list intact, while Mother Brain's direction-nine entrance
    /// redirects that same physical slot to the header's second, three-frame closing list.
    /// </summary>
    private static void VerifyMotherBrainEscapeRoomGate(TestAddressSpace bus)
    {
        const ushort population = 0x9680;
        const int width = 16;
        const int height = 16;
        const int gateX = 7;
        const int gateY = 2;
        const ushort originalGateWord = 0xf123;
        const ushort deactivatedGateWord = 0x8123;
        const ushort openGateWord = 0x80ff;
        const ushort halfClosedGateTopWord = 0x830f;
        const ushort closedGateMiddleWord = 0x8ae8;

        WriteWord(bus, 0x84c8ca, 0xb3c1);
        WriteWord(bus, 0x84c8cc, RoomPlmInstructionLists.MotherBrainEscapeRoomGateClosed);
        WriteWord(bus, 0x84c8ce, RoomPlmInstructionLists.MotherBrainEscapeRoomGateClosing);
        WriteWord(bus, 0x84c8d0, 0xb3c1);
        WriteWord(bus, 0x84c8d2, RoomPlmInstructionLists.MotherBrainEscapeRoomGateClosing);
        bus.WriteBytes(0x8f0000 | population,
        [
            0xca, 0xc8, gateX, gateY, 0x00, 0x80,
            0x00, 0x00,
        ]);

        WriteWord(bus, 0x84bb34, 6);
        WriteWord(bus, 0x84bb36, 0x948b);
        WriteWord(bus, 0x84bb38, RoomPlmInstructionCodes.Delete);
        WriteWord(bus, 0x84bb44, 2);
        WriteWord(bus, 0x84bb46, 0x9473);
        WriteWord(bus, 0x84bb48, 2);
        WriteWord(bus, 0x84bb4a, 0x947f);
        WriteWord(bus, 0x84bb4c, 2);
        WriteWord(bus, 0x84bb4e, 0x948b);
        WriteWord(bus, 0x84bb50, RoomPlmInstructionCodes.Delete);
        WriteVerticalPlmDraw(bus, 0x9473, [0x80ff, 0x80ff, 0x80ff, 0x80ff]);
        WriteVerticalPlmDraw(bus, 0x947f, [0x830f, 0x80ff, 0x80ff, 0x830f]);
        WriteVerticalPlmDraw(bus, 0x948b, [0x830f, 0x8ae8, 0x82e8, 0x830f]);

        RoomLevelData ordinaryLevel = CreateRoom(
            width,
            height,
            new ushort[width * height],
            new byte[width * height],
            blockDefinitions: new byte[0x400 * 8]);
        int gateBlock = ordinaryLevel.GetBlockIndex(gateX, gateY);
        ordinaryLevel.SetForegroundEntry(gateBlock, originalGateWord);
        BackgroundTilemapStreamer ordinaryStreamer = ordinaryLevel.CreateBackgroundStreamer();
        var ordinary = new RoomPlmSystem();
        var system = new Bank80SystemState();
        AssertEqual(1, ordinary.LoadRoomPopulation(
                bus,
                ordinaryLevel,
                ordinaryStreamer,
                new SnesVram(),
                population,
                system,
                AreaId.Tourian,
                () => new SamusState(),
                () => false),
            "escape gate population record is accepted by the sequential loader");
        AssertEqual(deactivatedGateWord, ordinaryLevel.GetCollisionBlockByIndex(gateBlock).LevelWord,
            "C8CA setup clears collision class bits without deleting its resident slot");
        AssertEqual(39, ordinary.PopulationSlots.Single().NativeSlotIndex,
            "escape gate retains the highest native population slot");
        AssertEqual(RoomPlmInstructionLists.MotherBrainEscapeRoomGateClosed,
            ordinary.PopulationSlots.Single().InstructionPointer,
            "ordinary entry retains C8CA's first closed-gate list");
        ordinary.Step(bus, ordinaryLevel, ordinaryStreamer, 0, 0, 0);
        AssertEqual(closedGateMiddleWord,
            ordinaryLevel.GetCollisionBlock(gateX, gateY + 1).LevelWord,
            "non-escape entry draws the cartridge's already-closed gate");
        for (int frame = 0; frame < 6; frame++)
            ordinary.Step(bus, ordinaryLevel, ordinaryStreamer, 0, 0, 0);
        AssertEqual(0, ordinary.ActiveCount,
            "non-escape closed-gate actor releases its slot after six authored frames");

        RoomLevelData escapeLevel = CreateRoom(
            width,
            height,
            new ushort[width * height],
            new byte[width * height],
            blockDefinitions: new byte[0x400 * 8]);
        escapeLevel.SetForegroundEntry(gateBlock, originalGateWord);
        BackgroundTilemapStreamer escapeStreamer = escapeLevel.CreateBackgroundStreamer();
        var escape = new RoomPlmSystem();
        escape.LoadRoomPopulation(
            bus,
            escapeLevel,
            escapeStreamer,
            new SnesVram(),
            population,
            system,
            AreaId.Tourian,
            () => new SamusState(),
            () => false);
        var motherBrainExit = new CartridgeDoorHeader(
            Pointer: 0xaa8c,
            DestinationRoomPointer: 0xde4d,
            BitFlags: 0,
            Orientation: 9,
            PlmX: gateX,
            PlmY: gateY,
            DestinationScreenX: 0,
            DestinationScreenY: 0,
            SamusDistance: 0x8000,
            SetupCodePointer: 0);
        AssertTrue(escape.TrySpawnDoorClosingPlm(bus, escapeLevel, motherBrainExit, system),
            "direction-nine Mother Brain exit redirects the resident gate");
        AssertEqual(1, escape.ActiveCount,
            "resident escape gate is redirected in place rather than duplicated");
        AssertEqual(39, escape.PopulationSlots.Single().NativeSlotIndex,
            "redirect preserves native room-population slot ordering");
        AssertEqual(RoomPlmInstructionLists.MotherBrainEscapeRoomGateClosing,
            escape.PopulationSlots.Single().InstructionPointer,
            "door transition selects C8CA's second instruction list");

        escape.Step(bus, escapeLevel, escapeStreamer, 0, 0, 0);
        AssertEqual(openGateWord, escapeLevel.GetCollisionBlock(gateX, gateY).LevelWord,
            "closing frame one draws the fully open gate");
        escape.Step(bus, escapeLevel, escapeStreamer, 0, 0, 0);
        AssertEqual(openGateWord, escapeLevel.GetCollisionBlock(gateX, gateY).LevelWord,
            "two-frame gate timer retains the open image on its countdown frame");
        escape.Step(bus, escapeLevel, escapeStreamer, 0, 0, 0);
        AssertEqual(halfClosedGateTopWord, escapeLevel.GetCollisionBlock(gateX, gateY).LevelWord,
            "closing frame two draws the half-closed gate");
        escape.Step(bus, escapeLevel, escapeStreamer, 0, 0, 0);
        escape.Step(bus, escapeLevel, escapeStreamer, 0, 0, 0);
        AssertEqual(closedGateMiddleWord, escapeLevel.GetCollisionBlock(gateX, gateY + 1).LevelWord,
            "closing frame three installs the final solid gate collision");
        escape.Step(bus, escapeLevel, escapeStreamer, 0, 0, 0);
        escape.Step(bus, escapeLevel, escapeStreamer, 0, 0, 0);
        AssertEqual(0, escape.ActiveCount,
            "escape gate releases its slot after the final two-frame hold");

        RoomLevelData fallbackLevel = CreateRoom(
            width,
            height,
            new ushort[width * height],
            new byte[width * height],
            blockDefinitions: new byte[0x400 * 8]);
        fallbackLevel.SetForegroundEntry(gateBlock, originalGateWord);
        var fallback = new RoomPlmSystem();
        AssertTrue(fallback.TrySpawnDoorClosingPlm(bus, fallbackLevel, motherBrainExit, system),
            "special door without a resident cap spawns C8D0 fallback");
        AssertEqual(RoomPlmHeaders.MotherBrainEscapeRoomGateClosing,
            fallback.PopulationSlots.Single().HeaderPointer,
            "fallback actor uses the cartridge's dedicated C8D0 header");
        AssertEqual(deactivatedGateWord, fallbackLevel.GetCollisionBlockByIndex(gateBlock).LevelWord,
            "fallback C8D0 executes the shared deactivate setup");

        var nonClosingDoor = motherBrainExit with { Orientation = 0 };
        var nonClosing = new RoomPlmSystem();
        AssertTrue(!nonClosing.TrySpawnDoorClosingPlm(bus, fallbackLevel, nonClosingDoor, system),
            "directions zero through three retain the native no-closing-PLM branch");
        AssertEqual(0, nonClosing.ActiveCount,
            "non-closing door does not consume a PLM slot");
    }

    private static void WriteVerticalPlmDraw(
        TestAddressSpace bus,
        ushort pointer,
        ReadOnlySpan<ushort> levelWords)
    {
        WriteWord(bus, 0x840000 | pointer, unchecked((ushort)(0x8000 | levelWords.Length)));
        for (int index = 0; index < levelWords.Length; index++)
            WriteWord(bus, 0x840000 | unchecked((ushort)(pointer + 2 + index * 2)), levelWords[index]);
        WriteWord(bus, 0x840000 | unchecked((ushort)(pointer + 2 + levelWords.Length * 2)), 0);
    }

    /// <summary>
    /// Exercises all semantic branches in `$DB44`'s thirteen-entry room-argument table:
    /// no-op, below-quota, four event writes, persistent sleep, and malformed input.
    /// </summary>
    private static void VerifyMetroidsClearedStatePlm(TestAddressSpace bus)
    {
        const ushort population = 0x9500;
        WriteWord(bus, 0x84db46, RoomPlmInstructionLists.SetMetroidsClearedStatesWhenRequired);
        WriteWord(bus, 0x84db42, RoomPlmInstructionCodes.Sleep);
        bus.WriteBytes(0x8f0000 | population,
        [
            0x44, 0xdb, 0x01, 0x01, 0x10, 0x00,
            0x44, 0xdb, 0x02, 0x01, 0x12, 0x00,
            0x44, 0xdb, 0x03, 0x01, 0x14, 0x00,
            0x44, 0xdb, 0x04, 0x01, 0x16, 0x00,
            0x44, 0xdb, 0x05, 0x01, 0x18, 0x00,
            0x00, 0x00,
        ]);

        RoomLevelData level = CreateRoom(
            8,
            8,
            new ushort[64],
            new byte[64],
            blockDefinitions: new byte[0x400 * 8]);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var system = new Bank80SystemState();
        var plms = new RoomPlmSystem();
        AssertEqual(5, plms.LoadRoomPopulation(
                bus,
                level,
                streamer,
                new SnesVram(),
                population,
                system,
                AreaId.Tourian,
                () => new SamusState(),
                () => false,
                hasEvent: system.HasEvent,
                setEvent: system.SetEvent),
            "Metroids-cleared population loads all argument families");

        plms.Step(bus, level, streamer, 0, 0, 0, enemyDeaths: 2, enemyDeathQuota: 3);
        foreach (EventNumber eventNumber in new[]
                 {
                     EventNumber.FirstMetroidHallCleared,
                     EventNumber.FirstMetroidShaftCleared,
                     EventNumber.SecondMetroidHallCleared,
                     EventNumber.SecondMetroidShaftCleared,
                 })
        {
            AssertTrue(!system.HasEvent(eventNumber),
                $"Metroids-cleared {eventNumber} remains clear below quota");
        }

        plms.Step(bus, level, streamer, 0, 0, 0, enemyDeaths: 3, enemyDeathQuota: 3);
        foreach (EventNumber eventNumber in new[]
                 {
                     EventNumber.FirstMetroidHallCleared,
                     EventNumber.FirstMetroidShaftCleared,
                     EventNumber.SecondMetroidHallCleared,
                     EventNumber.SecondMetroidShaftCleared,
                 })
        {
            AssertTrue(system.HasEvent(eventNumber),
                $"Metroids-cleared {eventNumber} is marked at quota");
        }
        AssertEqual(5, plms.ActiveCount,
            "Metroids-cleared observers remain resident on their sleep instruction");
        AssertTrue(plms.PopulationSlots.All(slot =>
                slot.InstructionPointer ==
                    RoomPlmInstructionLists.SetMetroidsClearedStatesWhenRequired),
            "Metroids-cleared observers retain the cartridge sleep list");

        const ushort invalidPopulation = 0x9580;
        bus.WriteBytes(0x8f0000 | invalidPopulation,
        [
            0x44, 0xdb, 0x01, 0x01, 0x01, 0x00,
            0x00, 0x00,
        ]);
        AssertThrows<InvalidDataException>(
            () => new RoomPlmSystem().LoadRoomPopulation(
                bus,
                level,
                streamer,
                new SnesVram(),
                invalidPopulation,
                new Bank80SystemState(),
                AreaId.Tourian,
                () => new SamusState(),
                () => false),
            "odd Metroids-cleared room argument fails loudly");
    }

    private static void VerifyOtherStationFamilies(TestAddressSpace bus)
    {
        const ushort population = 0x9200;
        bus.WriteBytes(0x8f0000 | population, [
            0xd3, 0xb6, 0x06, 0x06, 0x00, 0x00,
            0xeb, 0xb6, 0x0c, 0x06, 0x00, 0x00,
            0x6f, 0xb7, 0x12, 0x06, 0x03, 0x00,
            0x00, 0x00,
        ]);
        const int width = 32;
        RoomLevelData level = CreateRoom(
            width,
            16,
            new ushort[width * 16],
            new byte[width * 16],
            blockDefinitions: new byte[0x400 * 8]);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var system = new Bank80SystemState();
        var samus = new SamusState { Health = 99, MaxHealth = 99, Missiles = 0, MaxMissiles = 10 };
        var plms = new RoomPlmSystem();
        plms.LoadRoomPopulation(
            bus,
            level,
            streamer,
            new SnesVram(),
            population,
            system,
            areaIndex: AreaId.Norfair,
            getSamus: () => samus,
            isAreaTorizoDefeated: () => false);

        AssertEqual(3, plms.Stations.Count, "map, missile, and save parents share one pool");
        AssertEqual(0x47, level.GetCollisionBlock(7, 6).Behavior,
            "map right access setup uses BTS $47");
        AssertEqual(0x48, level.GetCollisionBlock(4, 6).Behavior,
            "map left access setup uses native two-block offset");
        AssertEqual(0x4b, level.GetCollisionBlock(13, 6).Behavior,
            "missile right access setup uses BTS $4B");
        AssertEqual(0x4c, level.GetCollisionBlock(11, 6).Behavior,
            "missile left access setup uses BTS $4C");
        AssertEqual(0x4d, level.GetCollisionBlock(18, 6).Behavior,
            "save station setup installs trigger BTS $4D");

        AssertTrue(plms.TryNotifyStationTouch(level.GetBlockIndex(7, 6), 0x47),
            "map access resolves parent");
        AssertTrue(plms.TryNotifyStationTouch(level.GetBlockIndex(13, 6), 0x4b),
            "missile access resolves parent");
        for (int frame = 0; frame < 102; frame++)
            plms.Step(bus, level, streamer, 0, 0, 0);
        AssertTrue(system.HasAreaMap(2), "map station marks current area acquired");
        AssertEqual(10, samus.Missiles, "missile station restores missiles");
        AssertEqual(2, plms.StationActivationEvents.Count,
            "two station parents publish two explicit activations");
        AssertTrue(samus.InputLocked,
            "station access remains locked while bank-$85 owns the completion message");

        // The runtime freezes this PLM while the message is open. Once bank $85 returns,
        // the original instruction lists execute three six-frame phases: post-message
        // hold, retract, and final hold. Both simultaneously active fixtures must release
        // their shared Samus owner at the native endpoint instead of looping access sound.
        for (int frame = 0; frame < 18; frame++)
            plms.Step(bus, level, streamer, 0, 0, 0);
        AssertTrue(!samus.InputLocked,
            "map/resource stations unlock Samus after post-message retraction");

        AssertTrue(plms.TryNotifyStationTouch(level.GetBlockIndex(18, 6), 0x4d),
            "save trigger resolves its same-block parent");
        plms.Step(bus, level, streamer, 0, 0, 0);
        AssertEqual(StationKind.Save, plms.StationActivationEvents.Single().Kind,
            "save trigger publishes typed confirmation request");
        AssertEqual(3, plms.StationActivationEvents.Single().StationIndex,
            "save request preserves native room argument/station index");

        StationActivationEvent saveRequest = plms.StationActivationEvents.Single();
        samus.XPosition = 0x0127;
        AssertTrue(plms.ResolveSaveStationConfirmation(bus, saveRequest, accepted: true),
            "accepted save resumes the sleeping cartridge PLM");
        AssertEqual(0x0120, samus.XPosition,
            "save animation centers Samus on the station's 16-pixel boundary");
        AssertTrue(SamusState.IsForwardFacingPose(samus.Pose),
            "save animation applies shared MakeSamusFaceForward setup");
        AssertTrue(samus.InputLocked, "save animation owns Samus input");
        AssertEqual(level.GetBlockIndex(18, 6), saveRequest.BlockIndex,
            "save activation retains the executing PLM block for bank-$86 electricity");
        VerifySaveStationElectricity(bus, level, saveRequest.BlockIndex);

        StationActivationEvent completion = default;
        bool completionPublished = false;
        for (int frame = 0; frame < 32 && !completionPublished; frame++)
        {
            plms.Step(bus, level, streamer, 0, 0, 0);
            if (frame == 0)
            {
                AssertTrue(plms.SoundRequests.Contains(new PlmSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, 0x2e), 6)),
                    "save animation queues cartridge library-one sound $2E");
            }
            if (plms.StationActivationEvents.Count != 0)
            {
                completion = plms.StationActivationEvents.Single();
                completionPublished = true;
            }
        }
        AssertTrue(completionPublished, "save animation reaches completion message");
        AssertEqual(GameplayMessageIds.SaveCompleted, completion.MessageBoxIndex,
            "save animation publishes game-saved message only after its draw loop");
        plms.CompleteSaveStation(completion);
        AssertTrue(!samus.InputLocked, "game-saved message return unlocks Samus");
        AssertTrue(plms.Stations.Single(station => station.Kind == StationKind.Save)
            .SaveStationLockedOut,
            "save completion installs once-per-room-entry lockout");

        AssertTrue(plms.TryNotifyStationTouch(level.GetBlockIndex(18, 6), 0x4d),
            "locked save trigger remains a solid station block");
        plms.Step(bus, level, streamer, 0, 0, 0);
        AssertEqual(0, plms.StationActivationEvents.Count,
            "locked save station cannot reopen until room re-entry");
    }

    private static void VerifySaveStationElectricity(
        TestAddressSpace bus,
        RoomLevelData level,
        int saveBlockIndex)
    {
        // Empty room population plus the retail-shaped seven-word $86:E6D2 definition.
        // The frame list is shortened to one visible map followed by the shared delete
        // opcode; spawn coordinates and shared interpreter ownership remain unchanged.
        bus.WriteBytes(0xa19600, [0xff, 0xff]);
        bus.WriteBytes(0x86e6d2,
        [
            0xad, 0xe6,
            0xd1, 0xe6,
            0x83, 0xe6,
            0x00, 0x00,
            0x00, 0x30,
            0x00, 0x00,
            0xfc, 0x84,
        ]);
        bus.WriteBytes(0x86e683,
        [
            0xd5, 0x81, 0x14, 0x00,
            0x01, 0x00, 0x62, 0xb5,
            0x54, 0x81,
        ]);
        // Enemy-projectile animation lists live in bank $86, but their frame maps are
        // consumed by the shared bank-$8D spritemap writer.
        bus.WriteBytes(0x8db562,
        [
            0x01, 0x00,
            0x00, 0x00, 0x00, 0x01, 0x20,
        ]);

        var enemies = new RoomEnemySystem();
        enemies.Load(
            bus,
            populationPointer: 0x9600,
            tilesetPointer: 0,
            new SnesVram(),
            new SnesCgram(),
            nextRandom: () => 0x4040,
            level: level,
            samus: new SamusState());
        enemies.SpawnSaveStationElectricity(saveBlockIndex, level.WidthInBlocks);

        RoomEnemyProjectileSlot electricity = enemies.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.SaveStationElectricity);
        AssertEqual(0x0130, electricity.XPosition,
            "save electricity starts one block right of the station PLM");
        AssertEqual(0x0040, electricity.YPosition,
            "save electricity starts two blocks above the station PLM");

        enemies.StepEnemyProjectiles(level, samus: null);
        AssertEqual(0xb562, electricity.SpritemapPointer,
            "save electricity runs through the shared bank-$86 frame-list interpreter");
        var oam = new OamBuffer();
        oam.BeginFrame();
        enemies.DrawEnemyProjectiles(oam, cameraX: 0x0100, cameraY: 0);
        oam.FinalizeFrame();
        AssertTrue(Enumerable.Range(0, oam.LastFinalizedSpriteCount)
            .Select(oam.GetEntry)
            .Any(entry => entry.X == 0x0030 && entry.Y == 0x40),
            "save electricity reaches the ordinary room enemy-projectile draw path");
    }

    private static void VerifyUnsupportedPopulationContext(TestAddressSpace bus)
    {
        const ushort population = 0x9400;
        const ushort unsupportedHeader = 0xb123;
        WriteWord(bus, 0x840000 | unsupportedHeader, 0x9876);
        WriteWord(bus, 0x840000 | unsupportedHeader + 2, 0x9abc);
        bus.WriteBytes(0x8f0000 | population, [
            0x23, 0xb1, 0x03, 0x04, 0x55, 0xaa, 0x00, 0x00,
        ]);
        RoomLevelData level = CreateRoom(
            8,
            8,
            new ushort[64],
            new byte[64],
            blockDefinitions: new byte[0x400 * 8]);
        var plms = new RoomPlmSystem();
        NotSupportedException? error = null;
        try
        {
            plms.LoadRoomPopulation(
                bus,
                level,
                level.CreateBackgroundStreamer(),
                new SnesVram(),
                population,
                new Bank80SystemState(),
                0,
                () => new SamusState(),
                () => false);
        }
        catch (NotSupportedException caught)
        {
            error = caught;
        }
        AssertTrue(error is not null,
            "unsupported room-population header fails at load time");
        AssertTrue(error!.Message.Contains("$8F:9400", StringComparison.Ordinal) &&
            error.Message.Contains("record 0", StringComparison.Ordinal) &&
            error.Message.Contains("$84:B123", StringComparison.Ordinal) &&
            error.Message.Contains("(3,4)", StringComparison.Ordinal) &&
            error.Message.Contains("$AA55", StringComparison.Ordinal),
            "unsupported PLM failure includes population/header/record/coordinate/argument context");
    }

    private static void VerifySaveStationConfirmation(TestAddressSpace bus)
    {
        const int definitions = 0x85869b;
        int message23 = definitions + (0x17 - 1) * 6;
        WriteWord(bus, message23, 0x8436);
        WriteWord(bus, message23 + 2, 0x8289);
        WriteWord(bus, message23 + 4, 0x9800);
        WriteWord(bus, message23 + 6, 0x8436);
        WriteWord(bus, message23 + 8, 0x8289);
        WriteWord(bus, message23 + 10, 0x9900);
        WriteWord(bus, message23 + 12, 0x8436);
        WriteWord(bus, message23 + 14, 0x8289);
        WriteWord(bus, message23 + 16, 0x9940);
        for (int word = 0; word < 32; word++)
        {
            WriteWord(bus, 0x858040 + word * 2, 0x3801);
            WriteWord(bus, 0x859581 + (32 + word) * 2,
                unchecked((ushort)(0x5100 + word)));
            WriteWord(bus, 0x859581 + (64 + word) * 2,
                unchecked((ushort)(0x5200 + word)));
        }
        for (int word = 0; word < 128; word++)
            WriteWord(bus, 0x859800 + word * 2, unchecked((ushort)(0x2800 + word)));
        for (int word = 0; word < 32; word++)
            WriteWord(bus, 0x859900 + word * 2, unchecked((ushort)(0x2a00 + word)));

        var message = new GameplayMessageBoxState();
        message.Begin(bus, GameplayMessageIds.SaveConfirmation);
        for (int guard = 0;
             message.Phase != GameplayMessageBoxPhase.AwaitingInput && guard < 32;
             guard++)
        {
            message.Step(0);
        }
        AssertEqual(GameplayMessageBoxPhase.AwaitingInput, message.Phase,
            "save message enters shared yes/no selection phase");
        AssertEqual(0x5100, message.Tilemap[128],
            "save message begins with native selected-YES row");
        message.Step((ushort)SuperMetroid.Core.Input.SnesButton.Left);
        AssertTrue(!message.ConfirmationSelectionYes,
            "save cursor changes the shared confirmation selection");
        AssertEqual(0x5200, message.Tilemap[128],
            "save cursor redraws the native selected-NO row");
        message.Step((ushort)SuperMetroid.Core.Input.SnesButton.Left);
        AssertTrue(!message.ConfirmationSelectionYes,
            "holding a direction does not toggle the newly-pressed save cursor repeatedly");
        message.Step((ushort)SuperMetroid.Core.Input.SnesButton.A);
        for (int guard = 0; message.IsActive && guard < 32; guard++)
            message.Step(0);
        AssertEqual(false, message.ConsumeConfirmationResult(),
            "save confirmation publishes selected no result after close");

        message.Begin(bus, GameplayMessageIds.SaveConfirmation);
        for (int guard = 0;
             message.Phase != GameplayMessageBoxPhase.AwaitingInput && guard < 32;
             guard++)
        {
            message.Step(0);
        }
        message.Step((ushort)SuperMetroid.Core.Input.SnesButton.A);
        for (int guard = 0; message.IsActive && guard < 32; guard++)
            message.Step(0);
        AssertEqual(true, message.ConsumeConfirmationResult(),
            "save confirmation defaults to yes and publishes acceptance");
    }

    private static void SeedRoomPlmPopulationRom(TestAddressSpace bus)
    {
        SeedPoseOneSamusData(bus);
        // Header instruction-list fields consumed by the generic allocator.
        WriteWord(bus, 0x84b705, 0xaf86);
        WriteWord(bus, 0x84b63d, 0xafa4);
        WriteWord(bus, 0x84b70d, 0xafb6);
        WriteWord(bus, 0x84b6e1, 0xadc2);
        WriteWord(bus, 0x84b6d5, 0xad62);
        WriteWord(bus, 0x84b6ed, 0xae4c);
        WriteWord(bus, 0x84b771, 0xafe8);
        WriteWord(bus, 0x84b6d9, 0xad86);
        WriteWord(bus, 0x84b6e5, 0xadf1);
        WriteWord(bus, 0x84b6f1, 0xae7b);

        WriteWord(bus, 0x84ad8b, 0xa200);
        WriteWord(bus, 0x84ad8f, 0xa206);
        WriteWord(bus, 0x84adfa, 0xa20c);
        WriteWord(bus, 0x84adfe, 0xa212);
        WriteWord(bus, 0x84ae84, 0xa218);
        WriteWord(bus, 0x84ae88, 0xa21e);
        for (ushort draw = 0xa200; draw <= 0xa21e; draw += 6)
            WriteOneBlockDraw(bus, draw, unchecked((ushort)(0xb100 + draw - 0xa200)));

        // Elevator's four cartridge frames and station idle draw triples.
        SeedTimedDrawLoop(bus, 0xafb6, 4, 4, 0xa100);
        WriteWord(bus, 0x84afc6, 0x8724);
        WriteWord(bus, 0x84afc8, 0xafb6);
        SeedTimedDrawLoop(bus, 0xad66, 6, 3, 0xa140);
        SeedTimedDrawLoop(bus, 0xad76, 2, 3, 0xa160);
        SeedTimedDrawLoop(bus, 0xadc6, 6, 3, 0xa180);
        SeedTimedDrawLoop(bus, 0xae50, 6, 3, 0xa1a0);
        WriteWord(bus, 0x84afe8, 1);
        WriteWord(bus, 0x84afea, 0xa1c0);
        WriteOneBlockDraw(bus, 0xa1c0, 0xb180);

        // Save list `$AFF7-$B006`: the fixture uses two alternating loops rather than the
        // retail region's $10/$15 so the complete coroutine remains a small unit test.
        bus.WriteBytes(0x84aff9, [0x02]);
        WriteWord(bus, 0x84affa, 4);
        WriteWord(bus, 0x84affc, 0xa1c6);
        WriteWord(bus, 0x84affe, 4);
        WriteWord(bus, 0x84b000, 0xa1cc);
        WriteOneBlockDraw(bus, 0xa1c6, 0xb181);
        WriteOneBlockDraw(bus, 0xa1cc, 0xb182);
    }

    private static void SeedTimedDrawLoop(
        TestAddressSpace bus,
        ushort list,
        ushort timer,
        int frameCount,
        ushort firstDraw)
    {
        for (int frame = 0; frame < frameCount; frame++)
        {
            ushort draw = unchecked((ushort)(firstDraw + frame * 6));
            WriteWord(bus, 0x840000 | unchecked((ushort)(list + frame * 4)), timer);
            WriteWord(bus, 0x840000 | unchecked((ushort)(list + frame * 4 + 2)), draw);
            WriteOneBlockDraw(bus, draw, unchecked((ushort)(0x8000 | frame)));
        }
    }
}
