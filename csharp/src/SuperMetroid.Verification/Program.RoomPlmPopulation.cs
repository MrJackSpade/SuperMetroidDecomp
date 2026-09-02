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
            areaIndex: 0,
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
        AssertTrue(plms.SoundRequests.Contains(new PlmSoundRequest(2, 0x37, 6)),
            "station access begins with cartridge extension sound $37");
        for (int frame = 1; frame < 102; frame++)
            plms.Step(bus, level, streamer, 0, 0, 0);
        AssertEqual(199, samus.Health, "energy station restores health to cartridge maximum");
        AssertEqual(1, plms.StationActivationEvents.Count,
            "energy station publishes one shared message request");
        AssertEqual(0x15, plms.StationActivationEvents[0].MessageBoxIndex,
            "energy station uses cartridge message $15");

        VerifyOtherStationFamilies(bus);
        VerifySaveStationConfirmation(bus);
        VerifyUnsupportedPopulationContext(bus);
        Console.WriteLine(
            "  Room PLM population: one-pass native slots, synchronous reuse, elevator, and station families agree.");
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
            areaIndex: 2,
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
        WriteWord(bus, message23 + 4, 0x9600);
        WriteWord(bus, message23 + 6, 0x8436);
        WriteWord(bus, message23 + 8, 0x8289);
        WriteWord(bus, message23 + 10, 0x9640);
        WriteWord(bus, message23 + 12, 0x8436);
        WriteWord(bus, message23 + 14, 0x8289);
        WriteWord(bus, message23 + 16, 0x9680);
        for (int word = 0; word < 32; word++)
        {
            WriteWord(bus, 0x858040 + word * 2, 0x3801);
            WriteWord(bus, 0x859600 + word * 2, unchecked((ushort)(0x2800 + word)));
            WriteWord(bus, 0x859640 + word * 2, unchecked((ushort)(0x2840 + word)));
        }

        var message = new GameplayMessageBoxState();
        message.Begin(bus, 0x17);
        for (int guard = 0;
             message.Phase != GameplayMessageBoxPhase.AwaitingInput && guard < 32;
             guard++)
        {
            message.Step(0);
        }
        AssertEqual(GameplayMessageBoxPhase.AwaitingInput, message.Phase,
            "save message enters shared yes/no selection phase");
        message.Step((ushort)SuperMetroid.Core.Input.SnesButton.Left);
        AssertTrue(!message.ConfirmationSelectionYes,
            "save cursor changes the shared confirmation selection");
        message.Step((ushort)SuperMetroid.Core.Input.SnesButton.A);
        for (int guard = 0; message.IsActive && guard < 32; guard++)
            message.Step(0);
        AssertEqual(false, message.ConsumeConfirmationResult(),
            "save confirmation publishes selected no result after close");

        message.Begin(bus, 0x17);
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
