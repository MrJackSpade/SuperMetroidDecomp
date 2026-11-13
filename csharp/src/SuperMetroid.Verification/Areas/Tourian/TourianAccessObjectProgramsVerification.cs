using SuperMetroid.AssetExtraction;
using static SuperMetroid.Verification.Rooms.RoomObjects.RoomObjectRunHeaderAssertions;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Verification.Rooms.RoomObjects;
using static SuperMetroid.Verification.Rooms.RoomObjects.SamusEaterObjectDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Areas.Tourian.TourianAccessObjectDefinitionsVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Areas.Tourian.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Areas.Tourian;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Tourian access crumble and clear PLM programs, draw lists, and physical floor effects.</summary>
internal static class TourianAccessObjectProgramsVerification
{
    /// <summary>Checks compiled controls and draws against the pinned ROM, then runs both floor paths without source reads.</summary>
    internal static void VerifyCompiledTourianAccessObjectPrograms()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Tourian draw native oracle revision");
        Suite(nameof(VerifyTourianAccessObjectDefinitions), () => VerifyTourianAccessObjectDefinitions(rom));
        Suite(nameof(RoomObjectProgramsVerification.VerifyTourianAccessPrograms), () => RoomObjectProgramsVerification.VerifyTourianAccessPrograms(rom));
        Suite(nameof(VerifyTourianAccessFrameSelector), () => VerifyTourianAccessFrameSelector(rom));

        Suite(nameof(VerifyTourianAccessPhysicalDrawMapping), () => VerifyTourianAccessPhysicalDrawMapping(rom));

        Suite(nameof(VerifyFloor), () => VerifyFloor(clear: true));
        Suite(nameof(VerifyFloor), () => VerifyFloor(clear: false));
        Console.WriteLine(
            "Tourian access PLMs: both native programs and five draws match ROM; complete clear and six-row crumble run without source reads.");
    }

    /// <summary>Checks the bounded crumble frame selector against the native program's draw operands.</summary>
    private static void VerifyTourianAccessFrameSelector(CartridgeImportAddressSpace rom)
    {
        ushort[] originalOperands = [0xaaea,0xaaee,0xaaf2,0xaaf6];
        for (int frame = 0; frame < originalOperands.Length; frame++)
            AssertEqual(ReadSamusEaterObjectWord(rom, 0x840000 | originalOperands[frame]),
                TourianAccessObjectDrawDefinitionsConstants.CrumbleFramePointer(frame), "Tourian access native frame selector");
        foreach (int invalid in new[] {int.MinValue,-1,4,5,255,256,int.MaxValue})
            AssertThrows<ArgumentOutOfRangeException>(() => TourianAccessObjectDrawDefinitionsConstants.CrumbleFramePointer(invalid),
                "Tourian access frame selector bounds");
    }

    /// <summary>Checks five native draw spans, row shapes, physical words, and continuation offsets.</summary>
    private static void VerifyTourianAccessPhysicalDrawMapping(CartridgeImportAddressSpace rom)
    {
        RoomObjectShotBlockDrawDefinitions.DrawList[] draws =
            RoomObjectTourianAccessVisualFiles.All.ToArray();
        AssertEqual(5, draws.Length, "Tourian access owns four frames and a six-row clear");
        ushort[] pointers = [0x9297,0x92a3,0x92af,0x92bb,0x92c7];
        RoomObjectTourianAccessVisuals pictures = RoomObjectTourianAccessVisuals.Stock();
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort pointer = (ushort)raw;
            int index = Array.IndexOf(pointers, pointer);
            AssertEqual(index >= 0, TourianAccessObjectDrawDefinitionsConstants.TryDescribe(NativeObjectDraws.At<TourianAccessDraw>(pointer), out int rows), "Tourian draw descriptor ownership");
            AssertEqual(index >= 0, TourianAccessObjectDrawDefinitionsConstants.TryGet(NativeObjectDraws.At<TourianAccessDraw>(pointer), out var dto), "Tourian draw DTO ownership");
            if (index < 0)
            {
                AssertEqual(0, rows, "missing Tourian rows zero");
                AssertEqual(default(RoomObjectShotBlockDrawDefinitions.DrawList), dto, "missing Tourian DTO default");
                continue;
            }
            AssertEqual(pointer, RoomObjectDrawAddressCodec.Of(draws[index].Draw), "Tourian draw export order");
            AssertEqual(pointer == 0x92c7 ? 6 : 1, rows, "Tourian native row count");
            AssertEqual(rows, dto.Runs.Length, "Tourian DTO row count");
            for (int row = 0; row < rows; row++)
            for (int column = 0; column < 4; column++)
            {
                RoomTerrainBlock native = RoomLevelWordCodec.Decode(ReadSamusEaterObjectWord(rom, 0x840000 | (pointer + row * 12 + column * 2 + 2)));
                AssertEqual(native.Collision, TourianAccessObjectDrawDefinitions.Collision, "Tourian collision covers every cartridge cell");
                AssertEqual(native.Collision, dto.Runs.Span[row].Collisions.Span[column], "Tourian direct DTO collision");
                AssertEqual(native.Appearance, pictures.Image(dto.Draw).At(new Offset16<RoomBlocks>(column, row)), "Tourian installed picture");
            }
        }
        foreach (RoomObjectShotBlockDrawDefinitions.DrawList draw in draws)
        {
            int cursor = RoomObjectDrawAddressCodec.Of(draw.Draw);
            for (int runIndex = 0; runIndex < draw.Runs.Length; runIndex++)
            {
                RoomObjectShotBlockDrawDefinitions.Run run = draw.Runs.Span[runIndex];
                AssertRunHeader(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0x840000 | cursor), run,
                    $"Tourian draw ${RoomObjectDrawAddressCodec.Of(draw.Draw):X4} run header at +{cursor - RoomObjectDrawAddressCodec.Of(draw.Draw)}");
                cursor += 2;
                foreach (RoomCollisionType collision in run.Collisions.Span)
                    AssertTourianDrawCollision(collision);
                AssertEqual(rom.ReadCartridgeByte(0x840000 | cursor++),
                    unchecked((byte)RoomObjectRunContinuationCodecConstants.X(draw, runIndex)),
                    $"Tourian draw ${RoomObjectDrawAddressCodec.Of(draw.Draw):X4} run X offset matches ROM");
                AssertEqual(rom.ReadCartridgeByte(0x840000 | cursor++),
                    unchecked((byte)RoomObjectRunContinuationCodecConstants.Y(draw, runIndex)),
                    $"Tourian draw ${RoomObjectDrawAddressCodec.Of(draw.Draw):X4} run Y offset matches ROM");
            }
            AssertEqual(checked((ushort)(cursor - RoomObjectDrawAddressCodec.Of(draw.Draw))),
                checked((ushort)(RoomObjectDrawAddressCodec.Of(draw.Draw) ==
                    RoomObjectDrawAddressCodec.Of(RoomObjectDraw.Of(TourianAccessDraw.Clear)) ? 72 : 12)),
                $"Tourian draw ${RoomObjectDrawAddressCodec.Of(draw.Draw):X4} has the complete native span");

            void AssertTourianDrawCollision(RoomCollisionType expected)
            {
                AssertEqual(RoomLevelWordCodec.Decode(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0x840000 | cursor)).Collision,
                    expected, $"Tourian draw ${RoomObjectDrawAddressCodec.Of(draw.Draw):X4} collision at +{cursor - RoomObjectDrawAddressCodec.Of(draw.Draw)} decodes from the ROM word");

                cursor += 2;
            }
        }

    }

    /// <summary>Runs the clear or crumble PLM until deletion and checks frame timing and the six-row level mutation.</summary>
    private static void VerifyFloor(bool clear)
    {
        const int width = 16;
        const int height = 20;
        var words = new RoomTerrainBlock[width * height];
        for (int y = 12; y < 18; y++)
        for (int x = 6; x < 10; x++)
            words[y * width + x] = RoomTerrainBlock.Of(0x123, BlockMirroring.None, RoomCollisionType.Air);
        RoomLevelData level = CreateRoom(new(width, height), words,
            new RoomBlockBehavior[words.Length]);
        var roomObjects = RoomObjectFixtures.Unbound();
        AssertTrue(roomObjects.Tourian.TrySpawnTourianAccess(level, clear ? TourianAccessFloor.Clear : TourianAccessFloor.Crumble),
            $"Tourian {(clear ? "clear" : "crumble")} PLM allocates");
        var bus = NewTourianAccessSourceGuard(new TestAddressSpace());
        int deletionFrame = -1;
        for (int frame = 0; frame < 120 && roomObjects.SlotTable.ActiveCount != 0; frame++)
        {
            roomObjects.Step(level);
            if (frame == 0 && !clear)
                AssertEqual(RoomTerrainBlock.Of(0x053, BlockMirroring.None, RoomCollisionType.Air),
                    level.GetCollisionBlockByIndex(12 * width + 6).Terrain,
                    "Tourian crumble first frame draws the native first breakup word");
            if (roomObjects.SlotTable.ActiveCount == 0)
                deletionFrame = frame;
        }
        AssertEqual(clear ? 1 : 96, deletionFrame,
            $"Tourian {(clear ? "clear" : "crumble")} PLM deletes on its native frame");
        for (int y = 12; y < 18; y++)
        for (int x = 6; x < 10; x++)
            AssertEqual(BlankAirBlock.Value,
                level.GetCollisionBlockByIndex(y * width + x).Terrain,
                $"Tourian {(clear ? "clear" : "crumble")} clears ({x},{y}) physically");
        AssertEqual(0, bus.ForbiddenReadAttempts,
            $"Tourian {(clear ? "clear" : "crumble")} reads no migrated ROM bytes");
    }

    /// <summary>Rejects runtime reads from the compiled Tourian access programs and draw lists.</summary>
    internal static CartridgeReadGuard NewTourianAccessSourceGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address =>
            {
                int pointer = SnesAddressMath.OffsetInBank(address);
                return (SnesAddressMath.BankNumber(address)) == 0x84 &&
                    ((pointer >= RoomObjectNativeListAddresses.TourianAccessEntries[0].Entry &&
                      pointer < RoomObjectNativeListAddresses.TourianAccessEntries[1].Entry + 6) ||
                     (pointer >= RoomObjectDrawAddressCodec.Of(RoomObjectDraw.Of(TourianAccessDraw.EmptyRow)) &&
                      pointer < 0x930f));
            },
            address => $"Tourian access PLM reread compiled source ${address:X6}.");

    /// <summary>Runs the Tourian access PLM suite through the verification command dispatcher.</summary>
    [VerificationCommand("--tourian-access-plms")]
    internal static int TourianAccessObjectsCommand()
    {
        Suite(nameof(VerifyCompiledTourianAccessObjectPrograms), () => VerifyCompiledTourianAccessObjectPrograms());
        return 0;
    }
}
