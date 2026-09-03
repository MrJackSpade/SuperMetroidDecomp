using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>
    /// Locks Bomb Torizo's resident `$D6EA` header, Bombs inventory gate, odd-byte DMA,
    /// eight debris requests, final music command, and cartridge-timed deletion together.
    /// </summary>
    static void VerifyBombTorizoHandPlm()
    {
        var bus = new TestAddressSpace();
        const ushort population = 0x9000;
        const int width = 16;
        const int height = 16;

        // One six-byte room-population record at the retail Bomb Torizo hand coordinate.
        bus.WriteBytes(0x8f0000 | population,
            [0xea, 0xd6, 0x0d, 0x0b, 0x00, 0x00, 0x00, 0x00]);

        // This is the complete `$84:D368` stream. Keeping the seven unaligned operands of
        // `$87E5` byte-for-byte is important: parsing this as words changes the following
        // 96-frame duration into `$7700` and never reaches the fragment sequence.
        bus.WriteBytes(0x84d368,
        [
            0x01, 0x00, 0x77, 0x98,
            0xc1, 0x86, 0x3b, 0xd3,
            0xb4, 0x86,
            0x78, 0x00, 0x77, 0x98,
            0xe5, 0x87, 0x00, 0x04, 0x00, 0xb2, 0xad, 0x00, 0x6e,
            0x60, 0x00, 0x77, 0x98,
            0x57, 0xd3, 0x00, 0x00,
            0x30, 0x00, 0x77, 0x98,
            0x57, 0xd3, 0x02, 0x00,
            0x0f, 0x00, 0x77, 0x98,
            0x57, 0xd3, 0x04, 0x00,
            0x0e, 0x00, 0x77, 0x98,
            0x57, 0xd3, 0x06, 0x00,
            0x0d, 0x00, 0x77, 0x98,
            0x57, 0xd3, 0x08, 0x00,
            0x0c, 0x00, 0x77, 0x98,
            0x57, 0xd3, 0x0a, 0x00,
            0x0b, 0x00, 0x77, 0x98,
            0x57, 0xd3, 0x0c, 0x00,
            0x0a, 0x00, 0x77, 0x98,
            0x57, 0xd3, 0x0e, 0x00,
            0x01, 0x00, 0x9d, 0x98,
            0xc7, 0xd3,
            0xbc, 0x86,
        ]);

        // Minimal valid one-block draw records let the common DrawPLM decoder remain the
        // authority without making this lifecycle test depend on retail block graphics.
        bus.WriteBytes(0x849877, [0x01, 0x00, 0x00, 0x00, 0x00, 0x00]);
        bus.WriteBytes(0x84989d, [0x01, 0x00, 0x00, 0x00, 0x00, 0x00]);

        var level = new RoomLevelData(
            width,
            height,
            new ushort[width * height],
            new byte[width * height],
            new ushort[width * height],
            new byte[8]);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var samus = new SamusState();
        var plms = new RoomPlmSystem();

        AssertEqual(1, plms.LoadRoomPopulation(
                bus,
                level,
                streamer,
                new SnesVram(),
                population,
                new Bank80SystemState(),
                areaIndex: AreaId.Crateria,
                getSamus: () => samus,
                isAreaTorizoDefeated: () => false,
                isTourianStatueFinished: null),
            "undefeated Bomb Torizo hand occupies PLM slot");
        AssertTrue(plms.HasActiveHeader(0xd6ea),
            "enemy header scan sees live Bomb Torizo hand");

        // First pass draws the closed hand; second installs D33B and reaches sleep. Many
        // additional passes prove that neither a countdown nor host wall clock bypasses it.
        plms.Step(bus, level, streamer, 0, 0, 0);
        plms.Step(bus, level, streamer, 0, 0, 0);
        for (int frame = 0; frame < 180; frame++)
            plms.Step(bus, level, streamer, 0, 0, 0);
        AssertTrue(plms.HasActiveHeader(0xd6ea),
            "hand remains resident while Samus lacks Bombs");
        AssertEqual(0, plms.VramWriteRequests.Count,
            "sleeping hand performs no premature DMA");

        samus.CollectedItems |= (ushort)SamusEquipmentFlags.Bombs;
        plms.Step(bus, level, streamer, 0, 0, 0); // D33B skips sleep, starts 120-frame wait.
        AssertTrue(plms.HasActiveHeader(0xd6ea),
            "Bombs wake starts authored delay without deleting hand");

        for (int frame = 0; frame < 119; frame++)
            plms.Step(bus, level, streamer, 0, 0, 0);
        AssertEqual(0, plms.VramWriteRequests.Count,
            "DMA waits for complete 120-frame post-pickup delay");
        plms.Step(bus, level, streamer, 0, 0, 0);
        AssertEqual(1, plms.VramWriteRequests.Count,
            "120th frame emits one PLM VRAM write");
        AssertEqual(0x0400, plms.VramWriteRequests[0].SizeInBytes,
            "hand DMA byte count");
        AssertEqual(0xadb200, plms.VramWriteRequests[0].SourceAddress,
            "hand DMA source address");
        AssertEqual(0x6e00, plms.VramWriteRequests[0].EncodedVramDestination,
            "hand DMA VRAM destination");

        var parameters = new List<ushort>();
        for (int frame = 0; frame < 400 && plms.HasActiveHeader(0xd6ea); frame++)
        {
            plms.Step(bus, level, streamer, 0, 0, 0);
            parameters.AddRange(plms.BombTorizoStatueProjectileRequests.Select(
                request => request.Parameter));
        }
        AssertSequenceEqual(
            new ushort[] { 0, 2, 4, 6, 8, 10, 12, 14 },
            parameters,
            "hand fragment parameters preserve cartridge order");
        AssertTrue(!plms.HasActiveHeader(0xd6ea),
            "final delete removes header seen by Torizo AI");
        AssertTrue(plms.BombTorizoHandWasDeleted,
            "hand deletion remains debugger-visible");
        AssertEqual(1, plms.MusicRequests.Count,
            "terminal hand pass queues one music command");
        AssertEqual(
            new PlmMusicRequest(MusicCommand.SelectTrack(6), MusicCommandDelay.EightFrames),
            plms.MusicRequests[0],
            "terminal hand music request");

        var defeated = new RoomPlmSystem();
        AssertEqual(1, defeated.LoadRoomPopulation(
                bus,
                level,
                streamer,
                new SnesVram(),
                population,
                new Bank80SystemState(),
                areaIndex: AreaId.Crateria,
                getSamus: () => samus,
                isAreaTorizoDefeated: () => true),
            "single loader still parses defeated hand record before setup deletes it");
        AssertTrue(!defeated.HasActiveHeader(0xd6ea),
            "defeated setup exposes no transient hand header");

        Console.WriteLine(
            "  Bomb Torizo hand: inventory gate, DMA, debris cadence, music, and deletion agree.");
    }
}
