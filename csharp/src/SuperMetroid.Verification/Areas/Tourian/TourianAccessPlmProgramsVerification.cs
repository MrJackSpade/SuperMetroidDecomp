using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Plms.SamusEaterPlmDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Areas.Tourian.TourianAccessPlmDefinitionsVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Areas.Tourian;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Tourian access crumble and clear PLM programs, draw lists, and physical floor effects.</summary>
internal static class TourianAccessPlmProgramsVerification
{
    /// <summary>Checks compiled controls and draws against the pinned ROM, then runs both floor paths without source reads.</summary>
    internal static void VerifyCompiledTourianAccessPlmPrograms()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Tourian draw native oracle revision");
        Suite(nameof(VerifyTourianAccessPlmDefinitions), () => VerifyTourianAccessPlmDefinitions(rom));
        Suite(nameof(RoomPlmProgramsVerification.VerifyTourianAccessPrograms), () => RoomPlmProgramsVerification.VerifyTourianAccessPrograms(rom));
        Suite(nameof(VerifyTourianAccessFrameSelector), () => VerifyTourianAccessFrameSelector(rom));

        Suite(nameof(VerifyTourianAccessPhysicalDrawMapping), () => VerifyTourianAccessPhysicalDrawMapping(rom));

        Suite(nameof(VerifyFloor), () => VerifyFloor(clear: true));
        Suite(nameof(VerifyFloor), () => VerifyFloor(clear: false));
        Console.WriteLine(
            "Tourian access PLMs: both native programs and five draws match ROM; complete clear and six-row crumble run without source reads.");
    }

    /// <summary>Checks the bounded crumble frame selector against the native program's draw operands.</summary>
    private static void VerifyTourianAccessFrameSelector(SuperMetroidAddressSpace rom)
    {
        ushort[] originalOperands = [0xaaea,0xaaee,0xaaf2,0xaaf6];
        for (int frame = 0; frame < originalOperands.Length; frame++)
            AssertEqual(ReadSamusEaterPlmWord(rom, 0x840000 | originalOperands[frame]),
                TourianAccessPlmDrawDefinitionsConstants.CrumbleFramePointer(frame), "Tourian access native frame selector");
        foreach (int invalid in new[] {int.MinValue,-1,4,5,255,256,int.MaxValue})
            AssertThrows<ArgumentOutOfRangeException>(() => TourianAccessPlmDrawDefinitionsConstants.CrumbleFramePointer(invalid),
                "Tourian access frame selector bounds");
    }

    /// <summary>Checks five native draw spans, row shapes, physical words, and continuation offsets.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Tourian access physical draw mapping")]
    private static void VerifyTourianAccessPhysicalDrawMapping(SuperMetroidAddressSpace rom)
    {
        RoomPlmShotBlockDrawDefinitions.DrawList[] draws =
            TourianAccessPlmDrawDefinitions.All.ToArray();
        AssertEqual(5, draws.Length, "Tourian access owns four frames and a six-row clear");
        ushort[] pointers = [0x9297,0x92a3,0x92af,0x92bb,0x92c7];
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort pointer = (ushort)raw;
            int index = Array.IndexOf(pointers, pointer);
            AssertEqual(index >= 0, TourianAccessPlmDrawDefinitionsConstants.TryDescribe(NativePlmDraws.At<TourianAccessDraw>(pointer), out int rows, out int word), "Tourian draw descriptor ownership");
            AssertEqual(index >= 0, TourianAccessPlmDrawDefinitionsConstants.TryGet(NativePlmDraws.At<TourianAccessDraw>(pointer), out var dto), "Tourian draw DTO ownership");
            if (index < 0)
            {
                AssertEqual(0, rows, "missing Tourian rows zero");
                AssertEqual((ushort)0, word, "missing Tourian word zero");
                AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList), dto, "missing Tourian DTO default");
                continue;
            }
            AssertEqual(pointer, PlmDrawAddressCodec.Of(draws[index].Draw), "Tourian draw export order");
            AssertEqual(pointer == 0x92c7 ? 6 : 1, rows, "Tourian native row count");
            AssertEqual(rows, dto.Runs.Length, "Tourian DTO row count");
            for (int row = 0; row < rows; row++)
            for (int column = 0; column < 4; column++)
            {
                ushort native = ReadSamusEaterPlmWord(rom, 0x840000 | (pointer + row * 12 + column * 2 + 2));
                AssertEqual(native, word, "Tourian scalar covers every native cell");
                AssertEqual(native, dto.Runs.Span[row].LevelWords.Span[column], "Tourian direct DTO word");
            }
        }
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList draw in draws)
        {
            int cursor = PlmDrawAddressCodec.Of(draw.Draw);
            foreach (RoomPlmShotBlockDrawDefinitions.Run run in draw.Runs.Span)
            {
                AssertTourianDrawWord(checked((ushort)run.DirectionAndCount));
                foreach (ushort word in run.LevelWords.Span)
                    AssertTourianDrawWord(word);
                AssertEqual(rom.ReadCartridgeByte(0x840000 | cursor++),
                    unchecked((byte)run.Next.X),
                    $"Tourian draw ${PlmDrawAddressCodec.Of(draw.Draw):X4} run X offset matches ROM");
                AssertEqual(rom.ReadCartridgeByte(0x840000 | cursor++),
                    unchecked((byte)run.Next.Y),
                    $"Tourian draw ${PlmDrawAddressCodec.Of(draw.Draw):X4} run Y offset matches ROM");
            }
            AssertEqual(checked((ushort)(cursor - PlmDrawAddressCodec.Of(draw.Draw))),
                checked((ushort)(PlmDrawAddressCodec.Of(draw.Draw) ==
                    PlmDrawAddressCodec.Of(PlmDraw.Of(TourianAccessDraw.Clear)) ? 72 : 12)),
                $"Tourian draw ${PlmDrawAddressCodec.Of(draw.Draw):X4} has the complete native span");

            void AssertTourianDrawWord(ushort expected)
            {
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0x840000 | cursor),
                    expected, $"Tourian draw ${PlmDrawAddressCodec.Of(draw.Draw):X4} word at +{cursor - PlmDrawAddressCodec.Of(draw.Draw)} matches ROM");
                cursor += 2;
            }
        }

    }

    /// <summary>Runs the clear or crumble PLM until deletion and checks frame timing and the six-row level mutation.</summary>
    private static void VerifyFloor(bool clear)
    {
        const int width = 16;
        const int height = 20;
        ushort[] words = new ushort[width * height];
        for (int y = 12; y < 18; y++)
        for (int x = 6; x < 10; x++)
            words[y * width + x] = 0x0123;
        RoomLevelData level = CreateRoom(new(width, height), words,
            new byte[words.Length], blockDefinitions: new byte[0x400 * 8]);
        var plms = PlmFixtures.Unbound();
        AssertTrue(plms.Tourian.TrySpawnTourianAccess(level, clear ? TourianAccessFloor.Clear : TourianAccessFloor.Crumble),
            $"Tourian {(clear ? "clear" : "crumble")} PLM allocates");
        var bus = NewTourianAccessSourceGuard(new TestAddressSpace());
        int deletionFrame = -1;
        for (int frame = 0; frame < 120 && plms.SlotTable.ActiveCount != 0; frame++)
        {
            plms.Step(level);
            if (frame == 0 && !clear)
                AssertEqual((ushort)0x0053,
                    level.GetCollisionBlockByIndex(12 * width + 6).PackedWord.Raw,
                    "Tourian crumble first frame draws the native first breakup word");
            if (plms.SlotTable.ActiveCount == 0)
                deletionFrame = frame;
        }
        AssertEqual(clear ? 1 : 96, deletionFrame,
            $"Tourian {(clear ? "clear" : "crumble")} PLM deletes on its native frame");
        for (int y = 12; y < 18; y++)
        for (int x = 6; x < 10; x++)
            AssertEqual((ushort)0x00ff,
                level.GetCollisionBlockByIndex(y * width + x).PackedWord.Raw,
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
                    ((pointer >= RoomPlmNativeListAddresses.TourianAccessEntries[0].Entry &&
                      pointer < RoomPlmNativeListAddresses.TourianAccessEntries[2].Entry + 6) ||
                     (pointer >= PlmDrawAddressCodec.Of(PlmDraw.Of(TourianAccessDraw.EmptyRow)) &&
                      pointer < 0x930f));
            },
            address => $"Tourian access PLM reread compiled source ${address:X6}.");

    /// <summary>Runs the Tourian access PLM suite through the verification command dispatcher.</summary>
    [VerificationCommand("--tourian-access-plms")]
    internal static int TourianAccessPlmsCommand()
    {
        Suite(nameof(VerifyCompiledTourianAccessPlmPrograms), () => VerifyCompiledTourianAccessPlmPrograms());
        return 0;
    }
}
