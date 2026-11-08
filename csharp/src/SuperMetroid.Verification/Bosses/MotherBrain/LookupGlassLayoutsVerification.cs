using static SuperMetroid.Verification.Rooms.RoomObjects.RoomObjectRunHeaderAssertions;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.RoomObjects.SamusEaterObjectDefinitionsVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Rooms.RoomObjects;

using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction;
using static SuperMetroid.Verification.Harness.TerrainBlockFieldAssertions;
namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>Checks Mother Brain glass PLM draw geometry, collision words, and visual cells.</summary>
internal static class LookupGlassLayoutsVerification
{
    /// <summary>Checks eleven draw identities, run directions, counts, continuations, and lookup bounds.</summary>
    internal static void VerifyGlassLayoutGeometry(SuperMetroidAddressSpace rom) => VerifyGlassLayoutDrawField(rom, 0);
    /// <summary>Checks native collision bits in each glass draw cell.</summary>
    internal static void VerifyGlassLayoutCollision(SuperMetroidAddressSpace rom) => VerifyGlassLayoutDrawField(rom, 1);
    /// <summary>Checks native visual tile bits in each glass draw cell.</summary>
    internal static void VerifyGlassLayoutVisuals(SuperMetroidAddressSpace rom) => VerifyGlassLayoutDrawField(rom, 2);

    /// <summary>Compares one field over all glass layouts and their 92 physical cells.</summary>
    private static void VerifyGlassLayoutDrawField(SuperMetroidAddressSpace rom, int field)
    {
        ushort[] pointers = [0x9717,0x971d,0x9731,0x9745,0x974f,0x9769,0x9781,0x978f,0x97b7,0x97e7,0x9817];
        string[] ids = ["initial","pane-damage-1","pane-damage-2","pane-transition","shifted-pane-1","shifted-pane-2","shifted-pane-3","shatter-1","shatter-2","shatter-3","cleared"];
        var exported = RoomObjectMotherBrainGlassVisualFiles.All.ToArray();
        AssertEqual(11, exported.Length, "Glass layout export count");
        if (field == 0)
        {
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                ushort pointer = (ushort)raw;
                bool owned = pointers.Contains(pointer);
                AssertEqual(owned, MotherBrainGlassDraws.TryDescribe(NativeObjectDraws.At<MotherBrainGlassDraw>(pointer), out var shape), "Glass layout shape domain");
                AssertEqual(owned, RoomObjectMotherBrainGlassVisualFiles.TryGet(NativeObjectDraws.At<MotherBrainGlassDraw>(pointer), out var dto), "Glass layout DTO domain");
                if (!owned)
                {
                    AssertEqual(default(MotherBrainGlassDraws.Draw), shape, "Glass layout missing shape");
                    AssertEqual(default(RoomObjectShotBlockDrawDefinitions.DrawList), dto, "Glass layout missing DTO");
                    AssertThrows<InvalidDataException>(() => RoomObjectMotherBrainGlassVisualFiles.VisualId(NativeObjectDraws.At<MotherBrainGlassDraw>(pointer)), "Glass layout missing ID");
                }
            }
            foreach (string id in new[] { "INTACT", "broken", "", "unknown" })
            {
                AssertTrue(!RoomObjectMotherBrainGlassVisualFiles.TryGetByVisualId(id, out var missing), "Glass layout ordinal ID domain");
                AssertEqual(default(RoomObjectShotBlockDrawDefinitions.DrawList), missing, "Glass layout rejected ID output");
            }
        }
        int cells = 0;
        for (int index = 0; index < pointers.Length; index++)
        {
            ushort pointer = pointers[index];
            MotherBrainGlassDraws.TryDescribe(NativeObjectDraws.At<MotherBrainGlassDraw>(pointer), out var shape);
            RoomObjectMotherBrainGlassVisualFiles.TryGet(NativeObjectDraws.At<MotherBrainGlassDraw>(pointer), out var dto);
            RoomObjectMotherBrainGlassVisualFiles.TryGetByVisualId(ids[index], out var byId);
            if (field == 0)
            {
                AssertEqual(pointer, RoomObjectDrawAddressCodec.Of(exported[index].Draw), "Glass layout original export order");
                AssertEqual(ids[index], RoomObjectMotherBrainGlassVisualFiles.VisualId(NativeObjectDraws.At<MotherBrainGlassDraw>(pointer)), "Glass layout published ID");
                AssertEqual(pointer, RoomObjectDrawAddressCodec.Of(byId.Draw), "Glass layout reverse ID");
                foreach (int invalid in new[] {int.MinValue,-1,shape.RunCount,int.MaxValue})
                {
                    AssertThrows<ArgumentOutOfRangeException>(() => shape.BlockCount(invalid), "Glass layout run bounds");
                    AssertThrows<ArgumentOutOfRangeException>(() => shape.Start(invalid), "Glass layout start bounds");
                    AssertThrows<ArgumentOutOfRangeException>(() => shape.BlockAt(invalid, 0), "Glass layout word run bounds");
                }
            }
            int cursor = 0x840000 | pointer;
            int run = 0;
            while (true)
            {
                ushort header = ReadSamusEaterObjectWord(rom, cursor);
                int count = header & 0x7fff;
                ushort offset = ReadSamusEaterObjectWord(rom, cursor + 2 + count * 2);
                if (field == 0)
                {
                    AssertEqual((ushort)(count | (shape.Vertical(run) ? 0x8000 : 0)), header, "Glass layout native direction");
                    AssertEqual(count, shape.BlockCount(run), "Glass layout native calculated count");
                    // The cartridge ends each run with the next run's start, and the last with no offset.
                    Offset16<RoomBlocks> continuation = run + 1 < shape.RunCount ? shape.Start(run + 1) : Offset16<RoomBlocks>.Zero;
                    AssertEqual(LittleEndianBytes.Word(unchecked((byte)continuation.X), unchecked((byte)continuation.Y)), offset, "Glass layout native calculated continuation");
                    foreach (var frame in new[] {dto,exported[index],byId})
                    {
                        var part = frame.Runs.Span[run];
                        AssertRunHeader(header, part, "Glass layout native DTO geometry");
                        AssertEqual(count, part.Blocks.Length, "Glass layout native DTO count");
                        AssertEqual(offset, RoomObjectRunContinuationCodec.Encode(frame, run), "Glass layout native DTO offset");
                    }
                    int selectedRun = run;
                    foreach (int invalid in new[] {int.MinValue,-1,count,int.MaxValue})
                        AssertThrows<ArgumentOutOfRangeException>(() => shape.BlockAt(selectedRun, invalid), "Glass layout cell bounds");
                }
                else for (int cell = 0; cell < count; cell++)
                {
                    RoomTerrainBlock expected = RoomLevelWordCodec.Decode(ReadSamusEaterObjectWord(rom, cursor + 2 + cell * 2));
                    AssertTerrainField(field, expected, shape.BlockAt(run, cell), "Glass layout original calculated field");
                    foreach (var frame in new[] {dto,exported[index],byId})
                        AssertTerrainField(field, expected, frame.Runs.Span[run].Blocks.Span[cell], "Glass layout original DTO field");
                }
                cells += count;
                run++;
                cursor += 4 + count * 2;
                if (offset == 0) break;
                AssertTrue(run < 5, "Glass layout native run termination");
            }
            AssertEqual(run, shape.RunCount, "Glass layout native run count");
            AssertEqual(run, dto.Runs.Length, "Glass layout native DTO run count");
        }
        AssertEqual(92, cells, "Glass layout independent physical cell count");
    }
}
