using static SuperMetroid.Verification.Rooms.Plms.PlmRunHeaderAssertions;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Verification.Rooms.Plms;
using static SuperMetroid.Verification.Rooms.Plms.SamusEaterPlmDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.AssetExtraction;
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
                SporeSpawnCeilingPlmDrawDefinitionsConstants.CrumbleFramePointer(frame), "Spore ceiling native frame selector");
        foreach (int invalid in new[] {int.MinValue,-1,3,4,255,256,int.MaxValue})
            AssertThrows<ArgumentOutOfRangeException>(() => SporeSpawnCeilingPlmDrawDefinitionsConstants.CrumbleFramePointer(invalid),
                "Spore ceiling frame selector bounds");
    }

    /// <summary>Compares four two-by-two physical draw layouts and exact pointer ownership with the cartridge.</summary>
    private static void VerifySporeSpawnCeilingDrawMapping(SuperMetroidAddressSpace rom)
    {
        RoomPlmShotBlockDrawDefinitions.DrawList[] draws =
            SporeSpawnCeilingDraws.All.ToArray();
        AssertEqual(4, draws.Length,
            "Spore Spawn ceiling has clear and three crumble appearances");
        ushort[] pointers = [0x9413,0x9423,0x9433,0x9443];
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort pointer = (ushort)raw;
            int index = Array.IndexOf(pointers, pointer);
            AssertEqual(index >= 0, SporeSpawnCeilingDraws.TryGetBlock(NativePlmDraws.At<SporeSpawnCeilingDraw>(pointer), out RoomTerrainBlock block), "Spore ceiling scalar ownership");
            AssertEqual(index >= 0, SporeSpawnCeilingDraws.TryGet(NativePlmDraws.At<SporeSpawnCeilingDraw>(pointer), out var dto), "Spore ceiling DTO ownership");
            if (index < 0)
            {
                AssertEqual(default(RoomTerrainBlock), block, "missing Spore ceiling scalar default");
                AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList), dto, "missing Spore ceiling DTO default");
                continue;
            }
            AssertEqual(pointer, PlmDrawAddressCodec.Of(draws[index].Draw), "Spore ceiling native export order");
            for (int row = 0; row < 2; row++)
            for (int column = 0; column < 2; column++)
            {
                RoomTerrainBlock native = RoomLevelWordCodec.Decode(ReadSamusEaterPlmWord(rom, 0x840000 | (pointer + row * 8 + column * 2 + 2)));
                AssertEqual(native, block, "Spore ceiling calculated scalar covers each decoded native cell");
                AssertEqual(native, dto.Runs.Span[row].Blocks.Span[column], "Spore ceiling direct DTO block");
            }
        }
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList draw in draws)
        {
            int cursor = PlmDrawAddressCodec.Of(draw.Draw);
            foreach (RoomPlmShotBlockDrawDefinitions.Run run in draw.Runs.Span)
            {
                AssertRunHeader(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0x840000 | cursor), run,
                    $"Spore Spawn draw ${PlmDrawAddressCodec.Of(draw.Draw):X4} run header at +{cursor - PlmDrawAddressCodec.Of(draw.Draw)}");
                cursor += 2;
                foreach (RoomTerrainBlock cell in run.Blocks.Span)
                    AssertDrawBlock(cell);
                AssertEqual(rom.ReadCartridgeByte(0x840000 | cursor++),
                    unchecked((byte)run.Next.X),
                    $"Spore Spawn draw ${PlmDrawAddressCodec.Of(draw.Draw):X4} signed X offset");
                AssertEqual(rom.ReadCartridgeByte(0x840000 | cursor++),
                    unchecked((byte)run.Next.Y),
                    $"Spore Spawn draw ${PlmDrawAddressCodec.Of(draw.Draw):X4} signed Y offset");
            }
            AssertEqual(16, cursor - PlmDrawAddressCodec.Of(draw.Draw),
                $"Spore Spawn draw ${PlmDrawAddressCodec.Of(draw.Draw):X4} covers its native 2x2 span");

            // Checks one compiled draw block against the decoded native block at the cursor and advances past it.
            void AssertDrawBlock(RoomTerrainBlock expected)
            {
                AssertEqual(RoomLevelWordCodec.Decode(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0x840000 | cursor)),
                    expected,
                    $"Spore Spawn draw ${PlmDrawAddressCodec.Of(draw.Draw):X4} block +{cursor - PlmDrawAddressCodec.Of(draw.Draw)}");
                cursor += 2;
            }
        }

    }

    /// <summary>Checks live ceiling block states, sound frame, and deletion frame for crumble or immediate clear.</summary>
    private static void VerifySporeSpawnCeiling(bool clear)
    {
        const int width = 16;
        const int height = 32;
        var terrain = new RoomTerrainBlock[width * height];
        terrain.AsSpan().Fill(RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.Air));
        for (int y = 30; y <= 31; y++)
        for (int x = 7; x <= 8; x++)
            terrain[y * width + x] = RoomTerrainBlock.Of(0x123, BlockMirroring.None, RoomCollisionType.Air);
        RoomLevelData level = CreateRoom(new(width, height), terrain, new RoomBlockBehavior[terrain.Length]);
        var plms = PlmFixtures.Unbound();
        AssertTrue(plms.BossRoomMutations.TrySpawnSporeSpawnCeiling(level,
                clear ? PlmHeaderId.ClearSporeSpawnCeiling :
                    PlmHeaderId.CrumbleSporeSpawnCeiling),
            $"Spore Spawn {(clear ? "clear" : "crumble")} PLM allocates");
        var guarded = NewSporeSpawnCeilingSourceGuard(new TestAddressSpace());
        int deletionFrame = -1;
        int soundFrame = -1;
        for (int frame = 0; frame < 32 && plms.SlotTable.ActiveCount != 0; frame++)
        {
            plms.Step(level);
            if (plms.Sounds.SoundRequests.Any(request =>
                    request.SoundEffect == SporeSpawnCeilingPlmProgramDefinitions.CrumbleSoundId))
                soundFrame = frame;
            if (frame % 4 == 0 && frame <= (clear ? 0 : 12))
            {
                int expectedBlock = clear ? 0xff : frame switch
                {
                    0 => 0x53,
                    4 => 0x54,
                    8 => 0x55,
                    _ => 0xff,
                };
                RoomTerrainBlock expected = RoomTerrainBlock.Of(expectedBlock, BlockMirroring.None, RoomCollisionType.Air);
                for (int y = 30; y <= 31; y++)
                for (int x = 7; x <= 8; x++)
                    AssertEqual(expected,
                        level.GetCollisionBlockByIndex(y * width + x).Terrain,
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
                     (pointer >= PlmDrawSourceDefinitions.SporeSpawnCeiling[SporeSpawnCeilingDraw.Clear].Offset &&
                      pointer < PlmDrawSourceDefinitions.SporeSpawnCeiling[SporeSpawnCeilingDraw.CrumbleThird].Offset + 16));
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
