using static SuperMetroid.Verification.Rooms.RoomObjects.RoomObjectRunHeaderAssertions;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Verification.Rooms.RoomObjects;
using static SuperMetroid.Verification.Rooms.RoomObjects.SamusEaterObjectDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.AssetExtraction;
namespace SuperMetroid.Verification.Bosses.SporeSpawn;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Spore Spawn ceiling PLM instructions, draws, and crumble/clear timing against bank-$84 data.</summary>
internal static class SporeSpawnCeilingObjectsVerification
{
    /// <summary>Runs native program and draw comparisons and both production ceiling mutations with source reads forbidden.</summary>
    internal static void VerifyCompiledSporeSpawnCeilingObjects()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Spore ceiling oracle revision");
        Suite(nameof(BossRoomObjectProgramsVerification.VerifySporeSpawnCeilingPrograms), () => BossRoomObjectProgramsVerification.VerifySporeSpawnCeilingPrograms(rom));
        Suite(nameof(VerifySporeSpawnCeilingFrameSelector), () => VerifySporeSpawnCeilingFrameSelector(rom));

        Suite(nameof(VerifySporeSpawnCeilingDrawMapping), () => VerifySporeSpawnCeilingDrawMapping(rom));

        Suite(nameof(VerifySporeSpawnCeiling), () => VerifySporeSpawnCeiling(clear: false));
        Suite(nameof(VerifySporeSpawnCeiling), () => VerifySporeSpawnCeiling(clear: true));
        Console.WriteLine(
            "Spore Spawn ceiling: two native lists and four 2x2 physical draws match ROM; crumble/clear, sound and deletion run with source bytes forbidden.");
    }

    /// <summary>Bounds the three-frame crumble selector against the native program's draw operands.</summary>
    private static void VerifySporeSpawnCeilingFrameSelector(CartridgeImportAddressSpace rom)
    {
        ushort[] originalOperands = [0xab17,0xab1b,0xab1f];
        for (int frame = 0; frame < originalOperands.Length; frame++)
            AssertEqual(ReadSamusEaterObjectWord(rom, 0x840000 | originalOperands[frame]),
                SporeSpawnCeilingObjectDrawDefinitionsConstants.CrumbleFramePointer(frame), "Spore ceiling native frame selector");
        foreach (int invalid in new[] {int.MinValue,-1,3,4,255,256,int.MaxValue})
            AssertThrows<ArgumentOutOfRangeException>(() => SporeSpawnCeilingObjectDrawDefinitionsConstants.CrumbleFramePointer(invalid),
                "Spore ceiling frame selector bounds");
    }

    /// <summary>
    /// Compares the four frames' two-by-two block shapes and the import's run form with the cartridge, checking exact
    /// pointer ownership, that each shape covers exactly the square's four cells, and that it rejects a cell outside.
    /// </summary>
    private static void VerifySporeSpawnCeilingDrawMapping(CartridgeImportAddressSpace rom)
    {
        RoomObjectShotBlockDrawDefinitions.DrawList[] draws =
            RoomObjectSporeSpawnCeilingVisualFiles.All.ToArray();
        AssertEqual(4, draws.Length,
            "Spore Spawn ceiling has clear and three crumble appearances");
        ushort[] pointers = [0x9413,0x9423,0x9433,0x9443];
        InstalledRoomObjectSporeSpawnCeilingVisuals pictures = InstalledRoomObjectSporeSpawnCeilingVisuals.Stock();
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort pointer = (ushort)raw;
            int index = Array.IndexOf(pointers, pointer);
            AssertEqual(index >= 0, SporeSpawnCeilingDraws.TryDescribe(NativeObjectDraws.At<SporeSpawnCeilingDraw>(pointer), out RoomBlockShape? described), "Spore ceiling shape ownership");
            AssertEqual(index >= 0, RoomObjectSporeSpawnCeilingVisualFiles.TryGet(NativeObjectDraws.At<SporeSpawnCeilingDraw>(pointer), out var dto), "Spore ceiling DTO ownership");
            if (described is null)
            {
                AssertTrue(index < 0, "Spore ceiling frame has a shape");
                AssertEqual(default(RoomObjectShotBlockDrawDefinitions.DrawList), dto, "missing Spore ceiling DTO default");
                continue;
            }
            RoomBlockShape shape = described;
            AssertEqual(pointer, RoomObjectDrawAddressCodec.Of(draws[index].Draw), "Spore ceiling native export order");
            AssertEqual(4, shape.Cells.Count, "Spore ceiling shape covers exactly its two-by-two square");
            foreach (Offset16<RoomBlocks> outside in new Offset16<RoomBlocks>[] { new(-1, 0), new(0, -1), new(2, 0), new(0, 2) })
                AssertThrows<ArgumentOutOfRangeException>(() => shape.CollisionAt(outside), "Spore ceiling shape covers only its square");
            for (int row = 0; row < 2; row++)
            for (int column = 0; column < 2; column++)
            {
                RoomTerrainBlock native = RoomLevelWordCodec.Decode(ReadSamusEaterObjectWord(rom, 0x840000 | (pointer + row * 8 + column * 2 + 2)));
                Offset16<RoomBlocks> place = new(column, row);
                AssertEqual(native.Collision, shape.CollisionAt(place), "Spore ceiling shape collision covers each decoded native cell");
                AssertEqual(native.Collision, dto.Runs.Span[row].Collisions.Span[column], "Spore ceiling direct DTO collision");
                AssertEqual(place, dto.Runs.Span[row].CellAt(column), "Spore ceiling DTO places the native cell");
                AssertEqual(native.Appearance, pictures.Image(dto.Draw).At(place), "Spore ceiling installed picture");
            }
        }
        foreach (RoomObjectShotBlockDrawDefinitions.DrawList draw in draws)
        {
            int cursor = RoomObjectDrawAddressCodec.Of(draw.Draw);
            for (int runIndex = 0; runIndex < draw.Runs.Length; runIndex++)
            {
                RoomObjectShotBlockDrawDefinitions.Run run = draw.Runs.Span[runIndex];
                AssertRunHeader(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0x840000 | cursor), run,
                    $"Spore Spawn draw ${RoomObjectDrawAddressCodec.Of(draw.Draw):X4} run header at +{cursor - RoomObjectDrawAddressCodec.Of(draw.Draw)}");
                cursor += 2;
                foreach (RoomCollisionType cell in run.Collisions.Span)
                    AssertDrawCollision(cell);
                AssertEqual(rom.ReadCartridgeByte(0x840000 | cursor++),
                    unchecked((byte)RoomObjectRunContinuationCodecConstants.X(draw, runIndex)),
                    $"Spore Spawn draw ${RoomObjectDrawAddressCodec.Of(draw.Draw):X4} signed X offset");
                AssertEqual(rom.ReadCartridgeByte(0x840000 | cursor++),
                    unchecked((byte)RoomObjectRunContinuationCodecConstants.Y(draw, runIndex)),
                    $"Spore Spawn draw ${RoomObjectDrawAddressCodec.Of(draw.Draw):X4} signed Y offset");
            }
            AssertEqual(16, cursor - RoomObjectDrawAddressCodec.Of(draw.Draw),
                $"Spore Spawn draw ${RoomObjectDrawAddressCodec.Of(draw.Draw):X4} covers its native 2x2 span");

            // Checks one draw cell's collision against the decoded native block at the cursor and advances past it.

            void AssertDrawCollision(RoomCollisionType expected)
            {
                AssertEqual(RoomLevelWordCodec.Decode(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0x840000 | cursor)).Collision,
                    expected,
                    $"Spore Spawn draw ${RoomObjectDrawAddressCodec.Of(draw.Draw):X4} block +{cursor - RoomObjectDrawAddressCodec.Of(draw.Draw)}");
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
        var roomObjects = RoomObjectFixtures.Unbound();
        AssertTrue(roomObjects.BossRoomMutations.SporeSpawnCeiling.TrySpawn(level,
                clear ? SporeSpawnCeilingMutation.Clear :
                    SporeSpawnCeilingMutation.Crumble),
            $"Spore Spawn {(clear ? "clear" : "crumble")} PLM allocates");
        var guarded = NewSporeSpawnCeilingSourceGuard(new TestAddressSpace());
        int deletionFrame = -1;
        int soundFrame = -1;
        for (int frame = 0; frame < 32 && roomObjects.SlotTable.ActiveCount != 0; frame++)
        {
            roomObjects.Step(level);
            if (roomObjects.Sounds.SoundRequests.Any(request =>
                    request.SoundEffect == SporeSpawnCeilingMutationDefinitions.CrumbleSoundId))
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
                        level.GetCollisionBlock(new Position16<RoomBlocks>(x, y)).Terrain,
                        $"Spore Spawn {(clear ? "clear" : "crumble")} frame {frame} ({x},{y})");
            }
            if (roomObjects.SlotTable.ActiveCount == 0)
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
                    ((pointer >= BossRoomObjectNativeListAddresses.SporeSpawnCeilingEntries[0].Entry &&
                      pointer < BossRoomObjectNativeListAddresses.SporeSpawnCeilingEntries[1].Entry + 6) ||
                     (pointer >= RoomObjectDrawSourceDefinitions.SporeSpawnCeiling[SporeSpawnCeilingDraw.Clear].Offset &&
                      pointer < RoomObjectDrawSourceDefinitions.SporeSpawnCeiling[SporeSpawnCeilingDraw.CrumbleThird].Offset + 16));
            },
            address => $"Spore Spawn ceiling reread migrated source ${address:X6}.");

    /// <summary>Runs the retail-cartridge Spore Spawn ceiling PLM verification.</summary>
    [VerificationCommand("--spore-spawn-ceiling-plms")]
    internal static int SporeSpawnCeilingObjectsCommand()
    {
        Suite(nameof(VerifyCompiledSporeSpawnCeilingObjects), () => VerifyCompiledSporeSpawnCeilingObjects());
        return 0;
    }
}
