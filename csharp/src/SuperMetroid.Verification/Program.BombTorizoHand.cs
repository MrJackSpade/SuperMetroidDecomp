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
        VerifyBombTorizoHandProgramDefinitions();
        VerifyBombTorizoHandVisualInstallation();
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

        // Both physical draw payloads are compiled. They deliberately remain absent
        // from the synthetic bus so a fallback to the old decoder cannot pass.
        var guarded = new BombTorizoHandProgramReadGuard(bus);

        var blockDefinitions = new byte[0x400 * 8];
        blockDefinitions[0x53 * 8] = 0x53;
        var level = new RoomLevelData(
            width,
            height,
            new ushort[width * height],
            new byte[width * height],
            new ushort[width * height],
            blockDefinitions);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var samus = new SamusState();
        var plms = new RoomPlmSystem();
        var editedVisuals = new RoomPlmBombTorizoHandVisualCatalog(
        [
            new("intact", [0x0053, 0x0066, 0x0064, 0x0045,
                0x0046, 0x0047, 0x0048, 0x0049]),
            new("cleared", Enumerable.Repeat((ushort)0x00ff, 16).ToArray()),
        ]);
        plms.BombTorizoHandVisuals = editedVisuals;

        AssertEqual(1, plms.LoadRoomPopulation(
                guarded,
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
        plms.Step(guarded, level, streamer, 0, 0, 0);
        int handOrigin = level.GetBlockIndex(13, 11);
        AssertEqual((ushort)0x8065,
            level.GetCollisionBlockByIndex(handOrigin).LevelWord,
            "edited Bomb Torizo hand keeps the cartridge's physical level word");
        AssertEqual((ushort)0x0053,
            plms.TilemapUpdates[0].TopRow[0],
            "edited hand reaches the immediate tilemap update");
        AssertEqual((ushort)0x0053,
            level.CreateBackgroundStreamer().BuildPlmLevelBlockUpdate(
                handOrigin, 0).TopRow[0],
            "edited hand survives later background streaming");
        AssertEqual((ushort)0x8064, level.GetCollisionBlock(12, 11).LevelWord,
            "multi-run hand geometry retains the second native run");
        AssertEqual((ushort)0x8049, level.GetCollisionBlock(14, 12).LevelWord,
            "multi-run hand geometry retains the fourth native run");
        plms.Step(guarded, level, streamer, 0, 0, 0);
        for (int frame = 0; frame < 180; frame++)
            plms.Step(guarded, level, streamer, 0, 0, 0);
        AssertTrue(plms.HasActiveHeader(0xd6ea),
            "hand remains resident while Samus lacks Bombs");
        AssertEqual(0, plms.VramWriteRequests.Count,
            "sleeping hand performs no premature DMA");

        samus.CollectedItems |= (ushort)SamusEquipmentFlags.Bombs;
        plms.Step(guarded, level, streamer, 0, 0, 0); // D33B skips sleep, starts 120-frame wait.
        AssertTrue(plms.HasActiveHeader(0xd6ea),
            "Bombs wake starts authored delay without deleting hand");

        for (int frame = 0; frame < 119; frame++)
            plms.Step(guarded, level, streamer, 0, 0, 0);
        AssertEqual(0, plms.VramWriteRequests.Count,
            "DMA waits for complete 120-frame post-pickup delay");
        plms.Step(guarded, level, streamer, 0, 0, 0);
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
            plms.Step(guarded, level, streamer, 0, 0, 0);
            parameters.AddRange(plms.BombTorizoStatueProjectileRequests.Select(
                request => request.Parameter));
        }
        AssertSequenceEqual(
            new ushort[] { 0, 2, 4, 6, 8, 10, 12, 14 },
            parameters,
            "hand fragment parameters preserve cartridge order");
        AssertTrue(!plms.HasActiveHeader(0xd6ea),
            "final delete removes header seen by Torizo AI");
        AssertEqual((ushort)0x00ff,
            level.GetCollisionBlockByIndex(handOrigin).LevelWord,
            "five-run cleared hand removes its physical origin block");
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
                guarded,
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
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "complete hand lifecycle and defeated reload never reread program or draw bytes");

        Console.WriteLine(
            "  Bomb Torizo hand: inventory gate, DMA, debris cadence, music, and deletion agree.");
    }

    private static void VerifyBombTorizoHandProgramDefinitions()
    {
        var rom = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        for (int address = BombTorizoHandPlmProgramDefinitions.FirstAddress;
             address <= BombTorizoHandPlmProgramDefinitions.LastAddress; address++)
        {
            AssertTrue(BombTorizoHandPlmProgramDefinitions.TryReadMechanicsByte(
                checked((ushort)address), out byte compiled),
                $"Bomb Torizo hand claims authored byte $84:{address:X4}");
            AssertEqual(rom.ReadByte(0x840000 | address), compiled,
                $"Bomb Torizo hand program byte $84:{address:X4} matches ROM");
            if (address == BombTorizoHandPlmProgramDefinitions.LastAddress)
                continue;
            AssertTrue(BombTorizoHandPlmProgramDefinitions.TryReadMechanicsWord(
                checked((ushort)address), out ushort compiledWord),
                $"Bomb Torizo hand claims authored word $84:{address:X4}");
            ushort native = (ushort)(rom.ReadByte(0x840000 | address) |
                rom.ReadByte(0x840000 | (address + 1)) << 8);
            AssertEqual(native, compiledWord,
                $"Bomb Torizo hand program word $84:{address:X4} matches ROM");
        }
        AssertTrue(!BombTorizoHandPlmProgramDefinitions.TryReadMechanicsWord(0xd3c6, out _),
            "hand list refuses a word crossing into adjacent callback code");
        AssertTrue(!BombTorizoHandPlmProgramDefinitions.TryReadMechanicsByte(0xd3c7, out _),
            "hand list does not claim the adjacent music callback code");
    }

    private sealed class BombTorizoHandProgramReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (address is >= 0x84d368 and <= 0x84d3c6 or
                >= 0x849877 and <= 0x8498d0)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Bomb Torizo hand reread compiled program byte ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
