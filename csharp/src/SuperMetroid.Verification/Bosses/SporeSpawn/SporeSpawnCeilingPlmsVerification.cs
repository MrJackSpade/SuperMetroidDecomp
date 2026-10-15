using SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Plms.SamusEaterPlmDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Bosses.SporeSpawn;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Spore Spawn ceiling PLM instructions, draws, and crumble/clear timing against bank-$84 data.</summary>
internal static class SporeSpawnCeilingPlmsVerification
{
    /// <summary>Runs native program and draw comparisons and both production ceiling mutations with source reads forbidden.</summary>
    internal static void VerifyCompiledSporeSpawnCeilingPlms()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Spore ceiling oracle revision");
        Suite(nameof(BossRoomPlmProgramsVerification.VerifySporeSpawnCeilingPrograms), () => BossRoomPlmProgramsVerification.VerifySporeSpawnCeilingPrograms(rom));
        Suite(nameof(VerifySporeSpawnCeilingFrameSelector), () => VerifySporeSpawnCeilingFrameSelector(rom));

        Suite(nameof(VerifySporeSpawnCeilingDrawMapping), () => VerifySporeSpawnCeilingDrawMapping(rom));

        Suite(nameof(VerifySporeSpawnCeiling), () => VerifySporeSpawnCeiling(clear: false));
        Suite(nameof(VerifySporeSpawnCeiling), () => VerifySporeSpawnCeiling(clear: true));
        Console.WriteLine(
            "Spore Spawn ceiling: two native lists and four 2x2 physical draws match ROM; crumble/clear, sound and deletion run with source bytes forbidden.");
    }

    /// <summary>Bounds the three-frame crumble selector against the native program's draw operands.</summary>
    private static void VerifySporeSpawnCeilingFrameSelector(SuperMetroidAddressSpace rom)
    {
        ushort[] originalOperands = [0xab17,0xab1b,0xab1f];
        for (int frame = 0; frame < originalOperands.Length; frame++)
            AssertEqual(ReadSamusEaterPlmWord(rom, 0x840000 | originalOperands[frame]),
                SporeSpawnCeilingPlmDrawDefinitions.CrumbleFramePointer(frame), "Spore ceiling native frame selector");
        foreach (int invalid in new[] {int.MinValue,-1,3,4,255,256,int.MaxValue})
            AssertThrows<ArgumentOutOfRangeException>(() => SporeSpawnCeilingPlmDrawDefinitions.CrumbleFramePointer(invalid),
                "Spore ceiling frame selector bounds");
    }

    /// <summary>Compares four two-by-two physical draw layouts and exact pointer ownership with the cartridge.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Spore Spawn ceiling draw mapping")]
    private static void VerifySporeSpawnCeilingDrawMapping(SuperMetroidAddressSpace rom)
    {
        RoomPlmShotBlockDrawDefinitions.DrawList[] draws =
            SporeSpawnCeilingPlmDrawDefinitions.All.ToArray();
        AssertEqual(4, draws.Length,
            "Spore Spawn ceiling has clear and three crumble appearances");
        ushort[] pointers = [0x9413,0x9423,0x9433,0x9443];
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort pointer = (ushort)raw;
            int index = Array.IndexOf(pointers, pointer);
            AssertEqual(index >= 0, SporeSpawnCeilingPlmDrawDefinitions.TryGetWord(pointer, out ushort word), "Spore ceiling scalar ownership");
            AssertEqual(index >= 0, SporeSpawnCeilingPlmDrawDefinitions.TryGet(pointer, out var dto), "Spore ceiling DTO ownership");
            if (index < 0)
            {
                AssertEqual((ushort)0, word, "missing Spore ceiling scalar zero");
                AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList), dto, "missing Spore ceiling DTO default");
                continue;
            }
            AssertEqual(pointer, draws[index].Pointer, "Spore ceiling native export order");
            for (int row = 0; row < 2; row++)
            for (int column = 0; column < 2; column++)
            {
                ushort native = ReadSamusEaterPlmWord(rom, 0x840000 | (pointer + row * 8 + column * 2 + 2));
                AssertEqual(native, word, "Spore ceiling calculated scalar covers each native cell");
                AssertEqual(native, dto.Runs.Span[row].LevelWords.Span[column], "Spore ceiling direct DTO word");
            }
        }
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList draw in draws)
        {
            int cursor = draw.Pointer;
            foreach (RoomPlmShotBlockDrawDefinitions.Run run in draw.Runs.Span)
            {
                AssertDrawWord(run.DirectionAndCount);
                foreach (ushort word in run.LevelWords.Span)
                    AssertDrawWord(word);
                AssertEqual(rom.ReadCartridgeByte(0x840000 | cursor++),
                    unchecked((byte)run.Next.X),
                    $"Spore Spawn draw ${draw.Pointer:X4} signed X offset");
                AssertEqual(rom.ReadCartridgeByte(0x840000 | cursor++),
                    unchecked((byte)run.Next.Y),
                    $"Spore Spawn draw ${draw.Pointer:X4} signed Y offset");
            }
            AssertEqual(16, cursor - draw.Pointer,
                $"Spore Spawn draw ${draw.Pointer:X4} covers its native 2x2 span");

            // Checks one compiled draw word at the current native cursor and advances past it.
            void AssertDrawWord(ushort expected)
            {
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0x840000 | cursor),
                    expected,
                    $"Spore Spawn draw ${draw.Pointer:X4} word +{cursor - draw.Pointer}");
                cursor += 2;
            }
        }

    }

    /// <summary>Checks live ceiling block states, sound frame, and deletion frame for crumble or immediate clear.</summary>
    private static void VerifySporeSpawnCeiling(bool clear)
    {
        const int width = 16;
        const int height = 32;
        ushort[] words = new ushort[width * height];
        for (int y = 30; y <= 31; y++)
        for (int x = 7; x <= 8; x++)
            words[y * width + x] = 0x0123;
        RoomLevelData level = CreateRoom(new(width, height), words,
            new byte[words.Length], blockDefinitions: new byte[0x400 * 8]);
        var plms = new RoomPlmSystem();
        AssertTrue(plms.BossRoomMutations.TrySpawnSporeSpawnCeiling(level,
                clear ? PlmHeaderId.ClearSporeSpawnCeiling :
                    PlmHeaderId.CrumbleSporeSpawnCeiling),
            $"Spore Spawn {(clear ? "clear" : "crumble")} PLM allocates");
        var guarded = NewSporeSpawnCeilingSourceGuard(new TestAddressSpace());
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        int deletionFrame = -1;
        int soundFrame = -1;
        for (int frame = 0; frame < 32 && plms.SlotTable.ActiveCount != 0; frame++)
        {
            plms.Step(guarded, level, streamer, new PlmView { Layer1Position = new(0, 16 * 16), Bg1XOffset = 0 });
            if (plms.Sounds.SoundRequests.Any(request =>
                    request.SoundEffect == SoundEffectId.FromCartridge(
                        SoundEffectLibrary.Library2,
                        SporeSpawnCeilingPlmProgramDefinitions.CrumbleSoundId)))
                soundFrame = frame;
            if (frame % 4 == 0 && frame <= (clear ? 0 : 12))
            {
                ushort expected = clear ? (ushort)0x00ff : (ushort)(frame switch
                {
                    0 => 0x0053,
                    4 => 0x0054,
                    8 => 0x0055,
                    _ => 0x00ff,
                });
                for (int y = 30; y <= 31; y++)
                for (int x = 7; x <= 8; x++)
                    AssertEqual(expected,
                        level.GetCollisionBlockByIndex(y * width + x).LevelWord,
                        $"Spore Spawn {(clear ? "clear" : "crumble")} frame {frame} ({x},{y})");
            }
            if (plms.SlotTable.ActiveCount == 0)
                deletionFrame = frame;
        }
        AssertEqual(clear ? 4 : 16, deletionFrame,
            $"Spore Spawn {(clear ? "clear" : "crumble")} native deletion frame");
        AssertEqual(clear ? -1 : 0, soundFrame,
            $"Spore Spawn {(clear ? "clear" : "crumble")} sound frame");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "Spore Spawn ceiling reads no migrated program or draw bytes");
    }

    /// <summary>Forbids production reads from migrated Spore Spawn ceiling program and draw ranges.</summary>
    internal static CartridgeReadGuard NewSporeSpawnCeilingSourceGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address =>
            {
                int pointer = SnesAddressMath.OffsetInBank(address);
                return (SnesAddressMath.BankNumber(address)) == 0x84 &&
                    ((pointer >= BossRoomPlmNativeListAddresses.SporeSpawnCeilingEntries[0].Entry &&
                      pointer < BossRoomPlmNativeListAddresses.SporeSpawnCeilingEntries[1].Entry + 6) ||
                     (pointer >= (ushort)SporeSpawnCeilingDraw.Clear &&
                      pointer < SporeSpawnCeilingPlmDrawDefinitions.EndExclusive));
            },
            address => $"Spore Spawn ceiling reread migrated source ${address:X6}.");

    /// <summary>Runs the retail-cartridge Spore Spawn ceiling PLM verification.</summary>
    [VerificationCommand("--spore-spawn-ceiling-plms")]
    internal static int SporeSpawnCeilingPlmsCommand()
    {
        Suite(nameof(VerifyCompiledSporeSpawnCeilingPlms), () => VerifyCompiledSporeSpawnCeilingPlms());
        return 0;
    }
}
