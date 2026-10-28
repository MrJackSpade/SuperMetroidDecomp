using static SuperMetroid.Verification.Rooms.Plms.PlmRunHeaderAssertions;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Enemies.Chozo.ChozoStatueVisualsVerification;
using static SuperMetroid.Verification.Rooms.Plms.SamusEaterPlmDefinitionsVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Rooms.Plms;
namespace SuperMetroid.Verification.Enemies.Chozo;

/// <summary>Checks three Chozo statue PLM draw layouts against native geometry and block words.</summary>
internal static class LookupChozoLayoutsVerification
{
    /// <summary>Checks draw-domain ownership, continuation offsets, and physical block placement.</summary>
    internal static void VerifyChozoLayoutGeometry(SuperMetroidAddressSpace rom) => VerifyChozoLayoutDrawField(rom, 0);
    /// <summary>Compares collision-type bits of all Chozo layout cells with native draw words.</summary>
    internal static void VerifyChozoLayoutCollision(SuperMetroidAddressSpace rom) => VerifyChozoLayoutDrawField(rom, 1);
    /// <summary>Compares visual bits of all Chozo layout cells with native draw words.</summary>
    internal static void VerifyChozoLayoutVisuals(SuperMetroidAddressSpace rom) => VerifyChozoLayoutDrawField(rom, 2);

    /// <summary>Checks the selected geometry or cell field across pointer and visual-ID views.</summary>
    private static void VerifyChozoLayoutDrawField(SuperMetroidAddressSpace rom, int field)
    {
        ushort[] pointers = [0xa2b5,0x9cc5,0x9d0f];
        string[] ids = ["lower-norfair-cleared-hand","wrecked-ship-clear-slope-access","wrecked-ship-block-slope-access"];
        var exported = ChozoStatueDraws.All.ToArray();
        AssertEqual(3, exported.Length, "Chozo layout export count");
        if (field == 0)
        {
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                ushort pointer = (ushort)raw;
                bool owned = pointers.Contains(pointer);
                AssertEqual(owned, ChozoStatueDraws.TryDescribe(NativePlmDraws.At<ChozoStatueDraw>(pointer), out var shape), "Chozo layout shape domain");
                AssertEqual(owned, ChozoStatueDraws.TryGet(NativePlmDraws.At<ChozoStatueDraw>(pointer), out var dto), "Chozo layout DTO domain");
                if (!owned)
                {
                    AssertEqual(default(ChozoStatueDraws.Draw), shape, "Chozo layout missing shape");
                    AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList), dto, "Chozo layout missing DTO");
                    AssertThrows<InvalidDataException>(() => ChozoStatueDraws.VisualId(NativePlmDraws.At<ChozoStatueDraw>(pointer)), "Chozo layout missing ID");
                }
            }
            foreach (string id in new[] { "LOWER-NORFAIR-CLEARED-HAND", "broken", "", "unknown" })
            {
                AssertTrue(!ChozoStatueDraws.TryGetByVisualId(id, out var missing), "Chozo layout ordinal ID domain");
                AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList), missing, "Chozo layout rejected ID output");
            }
        }
        int cells = 0;
        for (int index = 0; index < pointers.Length; index++)
        {
            ushort pointer = pointers[index];
            ChozoStatueDraws.TryDescribe(NativePlmDraws.At<ChozoStatueDraw>(pointer), out var shape);
            ChozoStatueDraws.TryGet(NativePlmDraws.At<ChozoStatueDraw>(pointer), out var dto);
            ChozoStatueDraws.TryGetByVisualId(ids[index], out var byId);
            if (field == 0)
            {
                AssertEqual(pointer, PlmDrawAddressCodec.Of(exported[index].Draw), "Chozo layout original export order");
                AssertEqual(ids[index], ChozoStatueDraws.VisualId(NativePlmDraws.At<ChozoStatueDraw>(pointer)), "Chozo layout published ID");
                AssertEqual(pointer, PlmDrawAddressCodec.Of(byId.Draw), "Chozo layout reverse ID");
                foreach (int invalid in new[] {int.MinValue,-1,shape.RunCount,int.MaxValue})
                {
                    AssertThrows<ArgumentOutOfRangeException>(() => shape.WordCount(invalid), "Chozo layout run bounds");
                    AssertThrows<ArgumentOutOfRangeException>(() => shape.NextX(invalid), "Chozo layout X bounds");
                    AssertThrows<ArgumentOutOfRangeException>(() => shape.NextY(invalid), "Chozo layout offset bounds");
                    AssertThrows<ArgumentOutOfRangeException>(() => shape.WordAt(invalid, 0), "Chozo layout word run bounds");
                }
            }
            var level = CreateRoom(new(32, 16), new ushort[512], new byte[512]);
            var plms = PlmFixtures.Unbound();
            var guarded = NewChozoProgramAndDrawReadGuard(rom);
            if (field == 0)
            {
                AssertTrue(plms.ChozoStatue.TrySpawnChozoStatuePlm(level,
                    new ChozoStatuePlmRequest(PlmHeaderId.ClearSlopeAccessForWreckedShipChozo, new Position16<RoomBlocks>(3, 4), IsHardcoded: false)), "Chozo layout fixture spawn");
                plms.DrawSolePlmFrameForVerification(level, NativePlmDraws.At<ChozoStatueDraw>(pointer));
                AssertEqual(0, guarded.ForbiddenReadAttempts, "Chozo calculated draw avoids ROM reads");
            }
            int drawX = 3, drawY = 4;
            int cursor = 0x840000 | pointer;
            int run = 0;
            while (true)
            {
                ushort header = ReadSamusEaterPlmWord(rom, cursor);
                int count = header & 0x7fff;
                ushort offset = ReadSamusEaterPlmWord(rom, cursor + 2 + count * 2);
                if (field == 0)
                {
                    AssertEqual((ushort)count, header, "Chozo layout native direction");
                    AssertEqual(count, shape.WordCount(run), "Chozo layout native calculated count");
                    AssertEqual(LittleEndianBytes.Word((byte)shape.NextX(run), (byte)shape.NextY(run)), offset, "Chozo layout native calculated continuation");
                    foreach (var frame in new[] {dto,exported[index],byId})
                    {
                        var part = frame.Runs.Span[run];
                        AssertRunHeader(header, part, "Chozo layout native DTO geometry");
                        AssertEqual(count, part.LevelWords.Length, "Chozo layout native DTO count");
                        AssertEqual(offset, LittleEndianBytes.Word((byte)part.Next.X, (byte)part.Next.Y), "Chozo layout native DTO offset");
                    }
                    for (int cell = 0; cell < count; cell++)
                        AssertEqual(ReadSamusEaterPlmWord(rom, cursor + 2 + cell * 2),
                            level.GetCollisionBlock(new(checked((ushort)(drawX + cell)), checked((ushort)drawY))).PackedWord.Raw,
                            "Chozo native physical word at original absolute run position");
                    drawX = 3 + unchecked((sbyte)offset);
                    drawY = 4 + unchecked((sbyte)(LittleEndianBytes.HighByte(offset)));
                    int selectedRun = run;
                    foreach (int invalid in new[] {int.MinValue,-1,count,int.MaxValue})
                        AssertThrows<ArgumentOutOfRangeException>(() => shape.WordAt(selectedRun, invalid), "Chozo layout cell bounds");
                }
                else for (int cell = 0; cell < count; cell++)
                {
                    int mask = field == 1 ? 0xf000 : 0xfff;
                    int expected = ReadSamusEaterPlmWord(rom, cursor + 2 + cell * 2) & mask;
                    AssertEqual(expected, shape.WordAt(run, cell) & mask, "Chozo layout original calculated field");
                    foreach (var frame in new[] {dto,exported[index],byId})
                        AssertEqual(expected, frame.Runs.Span[run].LevelWords.Span[cell] & mask, "Chozo layout original DTO field");
                }
                cells += count;
                run++;
                cursor += 4 + count * 2;
                if (offset == 0) break;
                AssertTrue(run < 6, "Chozo layout native run termination");
            }
            AssertEqual(run, shape.RunCount, "Chozo layout native run count");
            AssertEqual(run, dto.Runs.Length, "Chozo layout native DTO run count");
        }
        AssertEqual(55, cells, "Chozo layout independent physical cell count");
    }
}
